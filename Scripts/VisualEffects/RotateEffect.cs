using Godot;
using System;

public class RotateEffect : Node
{
    [Export] private float shiftDegreeAmount = 4;
    [Export] private float cycleTime = 1;
    [Export] private bool startOnLaunch = true;

    SceneTreeTween rotateTween;

    public override void _Ready()
    {
        Node2D parent = GetParent() as Node2D;

        rotateTween = CreateTween();
        rotateTween.TweenProperty(parent, "rotation_degrees", shiftDegreeAmount, cycleTime / 2);
        rotateTween.TweenProperty(parent, "rotation_degrees", -shiftDegreeAmount, cycleTime / 2);
        rotateTween.SetLoops();

        if (!startOnLaunch) rotateTween.Pause();
    }

    public void PlayEffect(bool play)
    {
        if (play) rotateTween.Play();
        else rotateTween.Pause();
    }
}
