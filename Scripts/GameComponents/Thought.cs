using Godot;

public class Thought : Button
{
    // BORDER COLORS
    public enum BorderColors { DEFAULT, RED, YELLOW, GREEN }
    [Export] private Color borderRed;
    [Export] private Color borderYellow;
    [Export] private Color borderGreen;
    private Color borderDefault;

    // SFX CONSTANTS
    private const float POP_PITCH_VARIANCE = 0.2f;
    private const float SPAWN_SFX_DELAY = 0.4f;

    // ANIMATION CONSTANTS
    private const int BG_NORMAL_BORDER_WIDTH = 7;
    private const int BG_HOVERED_BORDER_WIDTH = 2;
    public const float HELD_SIZE = 0.9f;
    public const float HOVERED_SIZE = 1.25f;
    private const float SPAWN_ANIMATION_TIME = 0.5f;
    private const float DESPAWN_ANIMATION_TIME = 0.2f;
    private const float RESIZE_TIME = 0.2f;

    // LERP CONSTANTS
    private const float TARGET_LERP_STRENGTH = 0.4f;
    private const float VELOCITY_SLOW_STRENGTH = 0.01f;
    private const float VELOCITY_ACCEL_STRENGTH = 0.075f;

    // VELOCITY CONSTANTS
    private const float BASE_MIN_VELOCITY = 100;
    private const float BASE_MAX_VELOCITY = 200;
    private const float RETURN_TIME = 0.75f;

    // STATIC VALUES
    private static float MIN_VELOCITY = BASE_MIN_VELOCITY;
    private static float MAX_VELOCITY = BASE_MAX_VELOCITY;

    // SIGNALS
    [Signal] public delegate void ThoughtPickedUp(Thought thought);
    [Signal] public delegate void ThoughtReleased(Thought thought);

    // EXPORTS
    [Export] private Godot.Collections.Array<AudioStreamMP3> spawnSounds;
    [Export] private string startingText = "";
    [Export] private bool spawnSFX = true;
    [Export] private bool startDisabled = true;

    private AudioStreamPlayer2D audio;
    private StyleBoxFlat bgStyle;
    private ControlScaler scaler;
    private Tween tweener;
    private Label label;

    public string Word { get => label.Text; }
    public bool IsSubmitted { get => GetParent() is SubmitBox; }
    public bool IsHeld { get; private set; } = false;

    public Vector2 Velocity { get; private set; } = Vector2.Zero;
    public Vector2? LerpTarget
    {
        get => lerpTarget == Vector2.Inf ? (Vector2?)null : lerpTarget;
        set => lerpTarget = value ?? Vector2.Inf;
    }
    private Vector2 lerpTarget = Vector2.Inf;

    /*
        GODOT PROCESSES
    */
    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Tweener");
        label = GetNode<Label>("Text");
        audio = GetNode<AudioStreamPlayer2D>("Audio");
        bgStyle = GetNode<Panel>("Background").GetStylebox("panel") as StyleBoxFlat;

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        RectScale = Vector2.Zero;
        Modulate = Colors.Transparent;
        Disabled = startDisabled;
        label.Text = startingText;
        borderDefault = bgStyle.BorderColor;
        audio.Stream = spawnSounds[(int)(GD.Randi() % spawnSounds.Count)];
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_VELOCITY, MAX_VELOCITY);

        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.Out, Tween.TransitionType.Bounce);
        if (spawnSFX) NodeHelper.PlayRandomPitchAudio(audio, 1 - POP_PITCH_VARIANCE, 1 + POP_PITCH_VARIANCE, SPAWN_SFX_DELAY);
        GetNode<ModulateHelper>("ModulateHelper").ModulateToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Circ);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (lerpTarget != Vector2.Inf) // Has target
        {
            Vector2 newPos = RectPosition.LinearInterpolate(lerpTarget, TARGET_LERP_STRENGTH);
            Velocity = (newPos - RectPosition) / delta;
            RectPosition = newPos;
        }
        else if (IsHeld)
        {
            Vector2 newPos = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), TARGET_LERP_STRENGTH);
            Velocity = (newPos - RectPosition) / delta;
            RectPosition = newPos;
        }
        else // No target and not held (default movement)
        {
            if (!ThoughtBox.IsInBounds(this)) Rebound(ThoughtBox.IsMovingAway(this, true), ThoughtBox.IsMovingAway(this, false));
            RectPosition += Velocity * delta;
            if (Velocity.Length() > MAX_VELOCITY) Velocity = Velocity.LinearInterpolate(Velocity.Normalized() * MAX_VELOCITY, VELOCITY_SLOW_STRENGTH);
            else if (Velocity.Length() < MIN_VELOCITY)
            {
                if (Velocity.IsEqualApprox(Vector2.Zero)) Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized();
                Velocity = Velocity.LinearInterpolate(Velocity.Normalized() * MIN_VELOCITY, VELOCITY_ACCEL_STRENGTH);
            }
        }
    }

    /*
        PUBLIC INTERFACE FUNCTIONS
    */
    public static void SetNewVelocityRange(float factorToMultiplyRange)
    {
        MIN_VELOCITY = BASE_MIN_VELOCITY * factorToMultiplyRange;
        MAX_VELOCITY = BASE_MAX_VELOCITY * factorToMultiplyRange;
    }

    public void SetBorderColor(BorderColors color)
    {
        switch(color)
        {
            case BorderColors.RED:
                bgStyle.BorderColor = borderRed;
                break;
            case BorderColors.YELLOW:
                bgStyle.BorderColor = borderYellow;
                break;
            case BorderColors.GREEN:
                bgStyle.BorderColor = borderGreen;
                break;
            case BorderColors.DEFAULT:
                bgStyle.BorderColor = borderDefault;
                break;
        }
    }

    public void Despawn()
    {
        Disabled = true;
        NodeHelper.PlayRandomPitchAudio(audio, 1 - POP_PITCH_VARIANCE / 2 - 0.3f, 1 + POP_PITCH_VARIANCE / 2 - 0.3f);
        scaler.Scale(0, DESPAWN_ANIMATION_TIME, Tween.EaseType.In, Tween.TransitionType.Back);
        var delay = CreateTween();
        delay.TweenCallback(this, PropertyNames.QueueFree).SetDelay(Mathf.Max(DESPAWN_ANIMATION_TIME, audio.Stream.GetLength()));
        delay.Play();
    }

    public void SetWord(string newText)
    {
        label.Text = newText;
    }

    public void SetVelocityToCenter()
    {
        Velocity = (ThoughtBox.Center - RectGlobalPosition - RectSize / 2) / RETURN_TIME;
    }

    public void RemoveRim(bool remove)
    {
        int width = remove ? BG_HOVERED_BORDER_WIDTH : BG_NORMAL_BORDER_WIDTH;
        tweener.StopAll();
        tweener.InterpolateProperty(bgStyle, "border_width_right", bgStyle.BorderWidthRight, width, RESIZE_TIME);
        tweener.InterpolateProperty(bgStyle, "border_width_bottom", bgStyle.BorderWidthBottom, width, RESIZE_TIME);
        tweener.Start();
    }

    /*
        HELPER FUNCTIONS
    */
    private void Rebound(bool flipX, bool flipY)
    {
        Vector2 newVelocity = Velocity;
        newVelocity.x *= flipX ? -1 : 1;
        newVelocity.y *= flipY ? -1 : 1;
        Velocity = newVelocity;
    }

    /*
        MOUSE EVENTS
    */
    private void OnButtonUp()
    {
        IsHeld = false;
        scaler.ScaleToDefault();
        if (!ThoughtBox.IsInBounds(this)) SetVelocityToCenter();
        EmitSignal(nameof(ThoughtReleased), this);

        // Fixes problem where rim not removed when mouse exited BEFORE submission
        if (IsSubmitted && bgStyle.BorderWidthRight != BG_HOVERED_BORDER_WIDTH) RemoveRim(true);
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        scaler.Scale(HELD_SIZE);
        EmitSignal(nameof(ThoughtPickedUp), this);
    }

    private void OnMouseEnter()
    {
        if (IsHeld || Disabled) return;

        if (IsSubmitted) RemoveRim(false);
        else scaler.Scale(HOVERED_SIZE);
    }

    private void OnMouseExit()
    {
        if (IsHeld || Disabled) return;

        if (IsSubmitted) RemoveRim(true);
        else scaler.ScaleToDefault();
    }
}
