using Godot;
using System;

public partial class InGame : Control
{
    private const float THOUGHT_SPAWN_INTERVAL = 0.05f;
    private const float THOUGHT_DESPAWN_INTERVAL = 0.025f;
    private const float OFFSET_RANGE = 50;

    [Export] private PackedScene thoughtTemplate;

    private CountdownHandler countdown;
    private TimerBar timerBar;
    private Prompt prompt;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;

    private bool isTransitioning = false; // Tracks if stage is mid transitioning

    // TODO: Probably have some portrait avatar with speech bubble and lip flaps when prompt dialogue playing?
    public override void _Ready()
    {
        GD.Randomize();

        timerBar = GetNode<TimerBar>("TimerBar");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        prompt = GetNode<Prompt>("Prompt");
        submitArea = GetNode<SubmitHandler>("SubmitArea");
        countdown = GetNode<CountdownHandler>("CountdownText");

        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        bgm.Play();

        timerBar.Timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));
        prompt.Connect(nameof(Prompt.FinishCrawl), this, nameof(OnFinishTextCrawl));
        prompt.Connect(nameof(Prompt.OutOfDialogue), this, nameof(PlayGoodEnd));
        submitArea.Connect(nameof(SubmitHandler.CorrectSubmission), this, nameof(ToNextPhase));
        submitArea.Connect(nameof(SubmitHandler.WrongSubmission), this, nameof(PlayError));

        isTransitioning = true;
        countdown.Connect(nameof(CountdownHandler.CountdownFinished), this, nameof(PlaySpawning), flags: (uint)ConnectFlags.Oneshot);
        countdown.StartCountdown();
    }

    private void SpawnWordsAndSubmitBoxes()
    {
        var spawning = CreateTween();
        for(int i = 0; i < prompt.WordList.Length; i++)
        {
            spawning.TweenCallback(this, nameof(SpawnThought), new Godot.Collections.Array(){prompt.WordList[i]}).SetDelay(THOUGHT_SPAWN_INTERVAL);
        }
        spawning.TweenCallback(this, nameof(OnAllThoughtSpawned));
        spawning.Play();
        submitArea.SpawnSubmitBoxes(prompt.Answer);
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
        isTransitioning = true;
    }

    private void ResetGame() // TODO: Debate if this is even necessary -> only for pausing but is pausing even needed?
    {
        prompt.Reset();
        countdown.Connect(nameof(CountdownHandler.CountdownFinished), this, nameof(PlaySpawning), flags: (uint)ConnectFlags.Oneshot);
        countdown.StartCountdown();
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

    private void OnFinishTextCrawl()
    {
        if (isTransitioning)
        {
            SpawnWordsAndSubmitBoxes();
            isTransitioning = false;
        }
        else if (prompt.IsErrorText)
        {
            prompt.CurrentDialogue();
        }
    }

    private void OnAllThoughtSpawned()
    {
        // Assumes all submit boxes finish BEFORE thoughts finish
        foreach(SubmitBox box in GetTree().GetNodesInGroup(GroupNames.SubmitBoxes))
        {
            box.StartListening();
        }
        foreach(Thought thought in GetTree().GetNodesInGroup(GroupNames.Thoughts))
        {
            thought.Disabled = false;
        }
    }
}
