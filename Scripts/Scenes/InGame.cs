using Godot;
using System;

public partial class InGame : Control
{
    private const float TIME_LIMIT = 20;

    [Export] private PackedScene thoughtTemplate;

    private Timer timer;
    private Prompt prompt;
    private AudioStreamPlayer bgm;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        prompt = GetNode<Prompt>("Prompt");

        timer.WaitTime = TIME_LIMIT;
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

        timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));
        prompt.Connect(nameof(Prompt.FinishCrawl), this, nameof(SpawnGameObjects));

        PlayOpening();
    }

    private void SpawnGameObjects()
    {
        // TODO: Spawn the thought templates at the center of the thought box, and the submitBoxTemplate somewhere down
    }

    private void ResetGame()
    {
        prompt.Reset();
    }
}
