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
        submitArea.Connect(nameof(SubmitHandler.WrongSubmission), this, nameof(PlayError));

        PlayOpening();
    }

    private void SpawnWordsAndSubmitBoxes()
    {
        // TODO: Spawn these one by one instead of all at once
        foreach(string word in prompt.WordList)
        {
            Thought thought = thoughtTemplate.Instance<Thought>();
            AddChild(thought);

            thought.RectPosition = MathHelper.GetPositionFromCenter(thought, ThoughtBox.Center);
            thought.SetText(word);
        }

        // TODO: Do some visual effect where it pops up one after another (probably one by one tween to adjust scale to 1 with bounce ease)
        submitArea.SpawnSubmitBoxes(prompt.Answer);
    }

    private void ToNextPhase()
    {
        // TODO: Implement this once submission is completely done (probably need to delete submit boxes and words here, call the group?)
        GD.Print("Going to next phase");
    }

    private void ResetGame()
    {
        prompt.Reset();
    }
}
