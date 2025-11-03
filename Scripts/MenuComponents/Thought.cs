using Godot;
using System;

public class Thought : Control
{
    public const float STICK_AMOUNT = 0.25f;
    public const float RETURN_TIME = 0.65f;

    private ControlScaler scaler;
    private Tween tweener;

    public bool IsHovered { get; private set; } = false;
    public bool IsHeld { get; private set; } = false;
    private Vector2 OriginalPosition = Vector2.Zero;

    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Translation");

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));
    }

    public override void _Process(float delta)
    {
        if (!IsHeld) return;

        RectPosition = RectPosition.LinearInterpolate(GetPositionFromMouse(), STICK_AMOUNT);
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

    private Vector2 GetPositionFromMouse()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        return mousePos - RectSize / 2;
    }
}
