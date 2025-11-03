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
    }

    public override void _Process(float delta)
    {

    }

    public void OnMouseEnter()
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
