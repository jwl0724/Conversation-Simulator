using Godot;
using System;

public class TimerBar : Control
{
    public Timer Timer { get; private set; }
    private ProgressBar bar;

    public override void _Ready()
    {
        Timer = GetNode<Timer>("Timer");
        bar = GetNode<ProgressBar>("ProgressBar");

        Timer.WaitTime = Globals.TIME_LIMIT;
        bar.MaxValue = Globals.TIME_LIMIT;
        bar.Value = Globals.TIME_LIMIT;
    }

    public override void _Process(float delta)
    {
        if (Timer.IsStopped() || Timer.Paused) return;
        bar.Value = Timer.TimeLeft;
    }

    // TODO: Add flashing red effect when running low on time and pulse effect?
}
