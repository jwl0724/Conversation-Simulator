using Godot;
using System;

public class Thought : Control
{
    private const float MIN_SPAWN_VELOCITY = 100;
    private const float MAX_SPAWN_VELOCITY = 250;
    private const float STICK_AMOUNT = 0.25f;
    private const float RETURN_TIME = 0.75f;
    private const float SPAWN_ANIMATION_TIME = 0.5f;

    [Export] private string StartingText = "";

    private ControlScaler scaler;
    private Tween tweener;
    private Label text;

    public string Word { get => text.Text; }
    public bool IsInBounds { get; set; } = true;
    public bool IsSubmitted { get; private set; } = false;
    public bool IsHovered { get; private set; } = false;
    public bool IsHeld { get; private set; } = false;

    public Vector2 Velocity { get; private set; } = Vector2.Zero;
    private Vector2 LastFramePosition = Vector2.Zero;
    private Vector2 OriginalPosition = Vector2.Zero;

    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Tweener");
        text = GetNode<Label>("Text");

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        RectScale = Vector2.Zero;
        text.Text = StartingText;
        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Back);

        // TODO: Play a pop sound effect

        // TODO URGENT: MOVE THIS TO SOMEWHERE ELSE SO IT DOESN'T KEEP ON RESEEDING, FOR NOW THIS IS FINE FOR TESTING MAIN MENU
        GD.Randomize();
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_SPAWN_VELOCITY, MAX_SPAWN_VELOCITY);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (tweener.IsActive()) return;

        if (IsHeld)
        {
            RectPosition = RectPosition.LinearInterpolate(GetViewport().GetMousePosition() - RectSize / 2, STICK_AMOUNT);
        }
        else
        {
            LastFramePosition = RectPosition;
            RectPosition += Velocity * delta;
        }
    }

    public void Rebound(bool flipX, bool flipY)
    {
        RectPosition = LastFramePosition;

        Vector2 newVelocity = Velocity;
        newVelocity.x *= flipX ? -1 : 1;
        newVelocity.y *= flipY ? -1 : 1;
        Velocity = newVelocity;
    }

    public void Submit(Vector2 submitPosition)
    {

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

        tweener.InterpolateProperty(this, PropertyNames.RectPosition, RectPosition, OriginalPosition, RETURN_TIME, Tween.TransitionType.Back, Tween.EaseType.InOut);
        tweener.Start();
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        scaler.Scale(0.95f);

        if (!tweener.IsActive()) OriginalPosition = RectPosition;
        tweener.StopAll();
    }

    private void OnMouseEnter()
    {
        if (IsHeld) return;

        IsHovered = true;
        scaler.Scale();
    }

    private void OnMouseExit()
    {
        if (IsHeld) return;

        IsHovered = false;
        scaler.ScaleToDefault();
    }
}
