using Godot;
using System;

public partial class InGame : Control
{
    private const float TIME_LIMIT = 20;

    [Export] private PackedScene thoughtTemplate;

    private Timer timer;
    private Prompt prompt;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        prompt = GetNode<Prompt>("Prompt");
        submitArea = GetNode<SubmitHandler>("SubmitArea");

        timer.WaitTime = TIME_LIMIT;
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

        timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));
        prompt.Connect(nameof(Prompt.FinishCrawl), this, nameof(SpawnWordsAndSubmitBoxes));
        submitArea.Connect(nameof(SubmitHandler.CorrectSubmission), this, nameof(ToNextPhase));

        PlayOpening();
    }

    private void SpawnWordsAndSubmitBoxes()
    {
        // TODO: Spawn the thought templates at the center of the thought box, and the submitBoxTemplate somewhere down

        submitArea.SpawnSubmitBoxes(prompt.Answer);
    }

    private void ToNextPhase()
    {
        // TODO: Implement this once submission is completely done
        GD.Print("Going to next phase");
    }

    private void ResetGame()
    {
        prompt.Reset();
    }
}
