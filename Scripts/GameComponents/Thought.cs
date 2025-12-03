using Godot;

public class Thought : Button
{
    // SFX CONSTANTS
    private const float POP_PITCH_VARIANCE = 0.2f;
    private const float SPAWN_SFX_DELAY = 0.4f;

    // ANIMATION CONSTANTS
    public const float HELD_SIZE = 0.9f;
    public const float HOVERED_SIZE = 1.25f;
    private const float SPAWN_ANIMATION_TIME = 0.5f;
    private const float RESIZE_TIME = 0.2f;

    // LERP CONSTANTS
    private const float TARGET_LERP_STRENGTH = 0.4f;
    private const float VELOCITY_SLOW_STRENGTH = 0.01f;

    // TIME CONSTANTS
    private const float RETURN_TIME = 0.75f;

    // STATIC VALUES
    private static float MIN_VELOCITY = 100;
    private static float MAX_VELOCITY = 200;

    // SIGNALS
    [Signal] public delegate void ThoughtPickedUp(Thought thought);
    [Signal] public delegate void ThoughtReleased(Thought thought);

    // EXPORTS
    [Export] private string startingText = "";
    [Export] private bool spawnSFX = true;
    [Export] private bool startDisabled = true;

    private AudioStreamPlayer2D audio;
    private ColorRect boxVisual;
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
    private Vector2 originalVisualSize;

    /*
        GODOT PROCESSES
    */
    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Tweener");
        label = GetNode<Label>("Text");
        audio = GetNode<AudioStreamPlayer2D>("Audio");
        boxVisual = GetNode<ColorRect>("Background/Body");

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        RectScale = Vector2.Zero;
        Modulate = Colors.Transparent;
        Disabled = startDisabled;
        label.Text = startingText;
        originalVisualSize = boxVisual.RectSize;
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_VELOCITY, MAX_VELOCITY);

        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.Out, Tween.TransitionType.Bounce);
        if (spawnSFX) NodeHelper.PlayRandomPitchAudio(audio, 1 - POP_PITCH_VARIANCE, 1 + POP_PITCH_VARIANCE, SPAWN_SFX_DELAY);
        GetNode<ModulateHelper>("ModulateHelper").ModulateToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Circ);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (lerpTarget != Vector2.Inf) // Has target
        {
            // TODO: Cubic interpolate maybe?
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
        }
    }

    /*
        PUBLIC INTERFACE FUNCTIONS
    */
    public void Despawn()
    {
        scaler.Scale(0, SPAWN_ANIMATION_TIME, Tween.EaseType.In, Tween.TransitionType.Back);
        var delay = CreateTween();
        delay.TweenCallback(this, PropertyNames.QueueFree).SetDelay(SPAWN_ANIMATION_TIME);
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
        int factor = remove ? 5 : 1;
        tweener.StopAll();
        tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, boxVisual.RectSize, originalVisualSize + Vector2.One * factor, RESIZE_TIME);
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
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        scaler.Scale(HELD_SIZE);
        EmitSignal(nameof(ThoughtPickedUp), this);
    }

    private void OnMouseEnter()
    {
        if (IsHeld) return;

        if (IsSubmitted) RemoveRim(false);
        else scaler.Scale(HOVERED_SIZE);
    }

    private void OnMouseExit()
    {
        if (IsHeld) return;

        if (IsSubmitted) RemoveRim(true);
        else scaler.ScaleToDefault();
    }
}
