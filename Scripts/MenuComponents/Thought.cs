using Godot;
using System;

public class Thought : Control
{
    // SFX CONSTANTS
    private const float POP_PITCH_VARIANCE = 0.2f;
    private const float SPAWN_SFX_DELAY = 0.4f;

    // ANIMATION CONSTANTS
    private const float SPAWN_ANIMATION_TIME = 0.5f;
    private const float RESIZE_TIME = 0.2f;

    // LERP CONSTANTS
    private const float RETURN_LERP_STRENGTH = 0.05f;
    private const float MOUSE_STICK_AMOUNT = 0.4f;
    private const float VELOCITY_SLOW_STRENGTH = 0.005f;
    private const float SUBMIT_LERP_STRENGTH = 0.5f;

    // STATIC VALUES
    private static float MIN_VELOCITY = 100;
    private static float MAX_VELOCITY = 200;

    [Export] private string startingText = "";
    [Export] private bool spawnSFX = true;

    private AudioStreamPlayer2D audio;
    private ColorRect boxVisual;
    private ControlScaler scaler;
    private Tween tweener;
    private Label label;

    public string Word { get => label.Text; }
    public bool IsInBounds { get; set; } = true;
    public bool IsSubmitted { get; private set; } = false;
    public bool IsHovered { get; private set; } = false;
    public bool IsHeld { get; private set; } = false;
    public bool IsReturning { get; private set; } = false;

    public SubmissionBox SubmitTarget { get; set; } = null; // TODO: Fix where box sometimes overlaps with submit box but it doesn't submit? probably mouse slightly outside area (maybe add box overlap as another standard?)
    public Vector2 Velocity { get; private set; } = Vector2.Zero;
    private Vector2 lastFramePosition = Vector2.Zero;
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
        label.Text = startingText;
        originalVisualSize = boxVisual.RectSize;
        audio.VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_VELOCITY, MAX_VELOCITY);

        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.Out, Tween.TransitionType.Bounce);
        if (spawnSFX)
        {
            audio.PitchScale = (float)GD.RandRange(1 - POP_PITCH_VARIANCE, 1 + POP_PITCH_VARIANCE);
            var soundDelay = CreateTween();
            soundDelay.TweenCallback(audio, nameof(audio.Play).ToLower()).SetDelay(SPAWN_SFX_DELAY);
        }
        GetNode<ModulateHelper>("ModulateHelper").ModulateToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Circ);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsHeld) // TODO: Fix wonky interaction where the box snaps to the submit position after it's been submitted once and then returned and grabbed again
        {
            if (SubmitTarget == null || !IsInstanceValid(SubmitTarget))
            {
                Vector2 newPos = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), MOUSE_STICK_AMOUNT);
                Velocity = (newPos - RectPosition) / delta;
                RectPosition = newPos;
            }
            else RectPosition = RectPosition.LinearInterpolate(SubmitTarget.RectGlobalPosition, SUBMIT_LERP_STRENGTH);
        }
        else if (IsSubmitted)
        {
            if (!IsInstanceValid(SubmitTarget)) return; // Check for final frames when submit box is deleted
            RectPosition = RectPosition.LinearInterpolate(SubmitTarget.RectGlobalPosition, SUBMIT_LERP_STRENGTH);
        }
        else if (IsReturning)
        {
            Vector2 newPosition = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, ThoughtBox.Center), RETURN_LERP_STRENGTH);
            if (ThoughtBox.IsInBounds(this))
            {
                IsReturning = false;
                Velocity = (newPosition - RectPosition) / delta;
            }
            RectPosition = newPosition;
        }
        else
        {
            lastFramePosition = RectPosition;
            RectPosition += Velocity * delta;

            if (Velocity.Length() > MAX_VELOCITY) Velocity = Velocity.LinearInterpolate(Velocity.Normalized() * MAX_VELOCITY, VELOCITY_SLOW_STRENGTH);
        }
    }

    /*
        PUBLIC INTERFACE FUNCTIONS
    */
    public static void SetSpawnVelocity(float min = -1, float max = -1) // Expected to be positive values
    {
        if (max < min)
        {
            GD.PushError($"Invalid usage of function, max should be greater than min ({max} < {min})");
            return;
        }
        MIN_VELOCITY = min >= 0 ? min : MIN_VELOCITY;
        MAX_VELOCITY = max >= 0 ? max : MAX_VELOCITY;
    }

    public void Rebound(bool flipX, bool flipY)
    {
        RectPosition = lastFramePosition;

        Vector2 newVelocity = Velocity;
        newVelocity.x *= flipX ? -1 : 1;
        newVelocity.y *= flipY ? -1 : 1;
        Velocity = newVelocity;
    }

    public void Despawn()
    {
        scaler.Scale(0, SPAWN_ANIMATION_TIME, Tween.EaseType.In, Tween.TransitionType.Back);
        var delay = CreateTween();
        delay.TweenCallback(this, PropertyNames.QueueFree).SetDelay(SPAWN_ANIMATION_TIME);
        delay.Play();
    }

    public void SetText(string newText)
    {
        label.Text = newText;
    }

    /*
        MOUSE EVENTS
    */
    private void OnButtonUp()
    {
        IsHeld = false;
        scaler.ScaleToDefault();

        if (SubmitTarget == null)
        {
            IsReturning = !IsInBounds;
        }
        else
        {
            IsSubmitted = true;
            SubmitTarget.NotifySubmit();
        }
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        IsReturning = false;
        scaler.Scale(0.95f);

        if (IsSubmitted)
        {
            IsSubmitted = false;
            SubmitTarget.NotifyUnsubmit();
        }
    }

    private void OnMouseEnter()
    {
        if (IsHeld) return;

        if (IsSubmitted)
        {
            tweener.StopAll();
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, boxVisual.RectSize, originalVisualSize, RESIZE_TIME);
            tweener.Start();
        }
        else
        {
            IsHovered = true;
            scaler.Scale();
        }
    }

    private void OnMouseExit()
    {
        if (IsHeld) return;

        if (IsSubmitted)
        {
            tweener.StopAll();
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, boxVisual.RectSize, originalVisualSize + Vector2.One * 5, RESIZE_TIME);
            tweener.Start();
        }
        else
        {
            IsHovered = false;
            scaler.ScaleToDefault();
        }
    }
}
