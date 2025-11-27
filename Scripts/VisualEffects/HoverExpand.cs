using Godot;
using System;

public class HoverExpand : Node
{
    [Export] private NodePath nodeToApplyPath; // Will default to parent if left empty
    [Export] private Vector2 expandSize;
    [Export] private float scaleStrength = 0.25f;

    private Control appliedNode;
    private bool doExpand = false;

    public override void _Ready()
    {
        if (nodeToApplyPath == null) appliedNode = GetParent<Control>();
        else appliedNode = GetNode<Control>(nodeToApplyPath);

        appliedNode.Connect(SignalNames.MouseEntered, this, nameof(OnMouseEvent), new Godot.Collections.Array(){true});
        appliedNode.Connect(SignalNames.MouseExited, this, nameof(OnMouseEvent), new Godot.Collections.Array(){false});
    }

    public override void _Process(float delta)
    {
        if (doExpand) appliedNode.RectScale = appliedNode.RectScale.LinearInterpolate(expandSize, scaleStrength);
        else appliedNode.RectScale = appliedNode.RectScale.LinearInterpolate(Vector2.One, scaleStrength);
    }

    private void OnMouseEvent(bool entered)
    {
        doExpand = entered;
    }
}
