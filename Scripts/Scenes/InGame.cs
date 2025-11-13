using Godot;
using System;

public partial class InGame : Control
{
    private const float TIME_LIMIT = 2;

    [Export] private PackedScene thoughtTemplate;
    [Export] private PackedScene submitBoxTemplate;

    private Timer timer;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        timer.WaitTime = TIME_LIMIT;
        timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));

        PlayOpening();
    }
}
