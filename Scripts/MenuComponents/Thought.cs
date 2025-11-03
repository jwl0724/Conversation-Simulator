using Godot;
using System;

public class Thought : Control
{
    public const float DRAG_DELAY = 0.5f;

    public bool IsHovered { get; private set; } = false;
    public bool IsHeld { get; private set; } = false;
    private ControlScaler scaler;

    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));
    }

    public override void _Process(float delta)
    {
        if (IsHeld) GD.Print("pretend its moving now");
    }

    private void OnButtonUp()
    {
        IsHeld = false;
        scaler.ScaleToDefault();
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        scaler.Scale(0.95f);
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
