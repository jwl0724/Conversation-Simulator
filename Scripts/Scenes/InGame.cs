using Godot;
using System;

public partial class InGame : Control
{
    private const float TIME_LIMIT = 20;
    private const float THOUGHT_SPAWN_INTERVAL = 0.05f;
    private const float THOUGHT_DESPAWN_INTERVAL = 0.025f;
    private const float OFFSET_RANGE = 50;

    [Export] private PackedScene thoughtTemplate;

    private Timer timer;
    private Prompt prompt;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;

    public override void _Ready()
    {
        GD.Randomize();

        timer = GetNode<Timer>("Timer");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        prompt = GetNode<Prompt>("Prompt");
        submitArea = GetNode<SubmitHandler>("SubmitArea");

        timer.WaitTime = TIME_LIMIT;
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

        timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));
        prompt.Connect(nameof(Prompt.FinishCrawl), this, nameof(SpawnWordsAndSubmitBoxes));
        prompt.Connect(nameof(Prompt.OutOfDialogue), this, nameof(PlayGoodEnd));
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
        // TODO: Add a call to start listening on the spawn submit boxes
    }

    private void ToNextPhase()
    {
        var thoughts = GetTree().GetNodesInGroup(GroupNames.Thoughts);
        var despawn = CreateTween();
        foreach(Thought thought in thoughts)
        {
            if (thought.IsSubmitted) continue; // SubmissionBox will handle despawning submitted
            despawn.TweenCallback(thought, nameof(thought.Despawn)).SetDelay(THOUGHT_DESPAWN_INTERVAL);
        }
        despawn.Play();
        submitArea.DespawnSubmitBoxes();
        prompt.NextDialogue();
    }

    private void ResetGame()
    {
        prompt.Reset();
    }

    private void SpawnThought(string word)
    {
        Thought thought = thoughtTemplate.Instance<Thought>();
        AddChild(thought);
        thought.SetWord(word);

        Vector2 center = MathHelper.GetPositionFromCenter(thought, ThoughtBox.Center);
        center += Vector2.Right * (float)GD.RandRange(-OFFSET_RANGE, OFFSET_RANGE);
        center += Vector2.Down * (float)GD.RandRange(-OFFSET_RANGE, OFFSET_RANGE);
        thought.RectPosition = center;
    }
}
