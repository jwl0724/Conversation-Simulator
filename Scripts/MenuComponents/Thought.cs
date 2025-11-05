using Godot;
using System;

public class Thought : Control
{
    private const float POP_PITCH_VARIANCE = 0.2f;
    private const float SPAWN_SFX_DELAY = 0.4f;
    private const float MIN_SPAWN_VELOCITY = 100;
    private const float MAX_SPAWN_VELOCITY = 250;
    private const float MOUSE_STICK_AMOUNT = 0.25f;
    private const float SUBMIT_LERP_STRENGTH = 0.5f;
    private const float RETURN_TIME = 0.75f;
    private const float SPAWN_ANIMATION_TIME = 0.5f;
    private const float RESIZE_TIME = 0.2f;

    [Export] private string startingText = "";
    [Export] private bool spawnSFX = true;

    private AudioStreamPlayer2D audio;
    private ColorRect boxVisual;
    private ControlScaler scaler;
    private Tween tweener;
    private Label text;

    public string Word { get => text.Text; }
    public bool IsInBounds { get; set; } = true;
    public bool IsSubmitted { get; private set; } = false;
    public bool IsHovered { get; private set; } = false;
    public bool IsHeld { get; private set; } = false;

    public SubmissionBox SubmitTarget { get; set; } = null;
    public Vector2 Velocity { get; private set; } = Vector2.Zero;
    private Vector2 lastFramePosition = Vector2.Zero;
    private Vector2 originalPosition = Vector2.Zero;
    private Vector2 originalVisualSize;

    // TODO: Move away from tweens and use LERP instead? noticing some massive bugginess in dragging the things

    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Tweener");
        text = GetNode<Label>("Text");
        audio = GetNode<AudioStreamPlayer2D>("Audio");
        boxVisual = GetNode<ColorRect>("Background/Body");

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        RectScale = Vector2.Zero;
        text.Text = startingText;
        originalVisualSize = boxVisual.RectSize;
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_SPAWN_VELOCITY, MAX_SPAWN_VELOCITY);

        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Back);
        if (spawnSFX)
        {
            audio.PitchScale = (float)GD.RandRange(1 - POP_PITCH_VARIANCE, 1 + POP_PITCH_VARIANCE);
            var soundDelay = CreateTween();
            soundDelay.TweenCallback(audio, nameof(audio.Play).ToLower()).SetDelay(SPAWN_SFX_DELAY);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (tweener.IsActive()) return;

        if (IsHeld)
        {
            if (SubmitTarget == null) RectPosition = RectPosition.LinearInterpolate(GetViewport().GetMousePosition() - RectSize / 2, MOUSE_STICK_AMOUNT);
            else RectPosition = RectPosition.LinearInterpolate(SubmitTarget.RectPosition, SUBMIT_LERP_STRENGTH);
        }
        else if (IsSubmitted)
        {
            RectPosition = RectPosition.LinearInterpolate(SubmitTarget.RectPosition, SUBMIT_LERP_STRENGTH);
        }
        else
        {
            lastFramePosition = RectPosition;
            RectPosition += Velocity * delta;
        }
    }

    public void Rebound(bool flipX, bool flipY)
    {
        RectPosition = lastFramePosition;

        Vector2 newVelocity = Velocity;
        newVelocity.x *= flipX ? -1 : 1;
        newVelocity.y *= flipY ? -1 : 1;
        Velocity = newVelocity;
    }

    public void SetText(string newText)
    {
        text.Text = newText;
    }

    public void SetSpeed(float newSpeed)
    {
        Velocity = Velocity.Normalized() * newSpeed;
    }

    private void OnButtonUp()
    {
        IsHeld = false;
        scaler.ScaleToDefault();

        if (SubmitTarget == null)
        {
            tweener.InterpolateProperty(this, PropertyNames.RectPosition, RectPosition, originalPosition, RETURN_TIME, Tween.TransitionType.Back, Tween.EaseType.InOut);
            tweener.Start();
        }
        else
        {
            IsSubmitted = true;
            SubmitTarget.NotifySubmit();
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, RectSize, originalVisualSize + Vector2.One * 5, RESIZE_TIME);
            tweener.Start();
        }
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        scaler.Scale(0.95f);

        if (IsSubmitted)
        {
            IsSubmitted = false;
            SubmitTarget.NotifyUnsubmit();
        }
        else
        {
            if (!tweener.IsActive()) originalPosition = RectPosition;
            tweener.StopAll();
        }
    }

    private void OnMouseEnter()
    {
        if (IsHeld) return;

        if (IsSubmitted)
        {
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, RectSize, originalVisualSize, RESIZE_TIME);
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
            // TODO: Fix this thing from snapping for some reason?
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, RectSize, originalVisualSize + Vector2.One * 5, RESIZE_TIME);
            tweener.Start();
        }
        else
        {
            IsHovered = false;
            scaler.ScaleToDefault();
        }
    }
}
