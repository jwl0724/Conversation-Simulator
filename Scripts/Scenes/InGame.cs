using Godot;
using System;

public partial class InGame : Control
{
    private const float TIME_LIMIT = 20;
    private const float THOUGHT_SPAWN_INTERVAL = 0.1f;
    private const float OFFSET_RANGE = 100;

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
        var spawning = CreateTween();
        for(int i = 0; i < prompt.WordList.Length; i++)
        {
            spawning.TweenCallback(this, nameof(SpawnThought), new Godot.Collections.Array(){prompt.WordList[i]}).SetDelay(THOUGHT_SPAWN_INTERVAL);
        }
        spawning.Play();
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

    private void SpawnThought(string word)
    {
        Thought thought = thoughtTemplate.Instance<Thought>();
        AddChild(thought);
        thought.SetText(word);

        Vector2 center = MathHelper.GetPositionFromCenter(thought, ThoughtBox.Center);
        center += Vector2.Right * (float)GD.RandRange(-OFFSET_RANGE, OFFSET_RANGE);
        center += Vector2.Down * (float)GD.RandRange(-OFFSET_RANGE, OFFSET_RANGE);
        thought.RectPosition = center;
    }
}
