using Godot;
using System;

public partial class InGame : Control
{
    private const float THOUGHT_SPAWN_INTERVAL = 0.05f;
    private const float THOUGHT_DESPAWN_INTERVAL = 0.025f;
    private const float OFFSET_RANGE = 50;

    [Export] private PackedScene thoughtTemplate;

    private ColorRect filter;
    private ThoughtBox thoughtBox;
    private CountdownHandler countdown;
    private TimerBar timerBar;
    private DialogueHandler dialogue;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;

    private bool isTransitioning = false; // Tracks if stage is mid transitioning

    public override void _Ready()
    {
        GD.Randomize();

        timerBar = GetNode<TimerBar>("TimerBar");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        dialogue = GetNode<DialogueHandler>("DialogueHandler");
        submitArea = GetNode<SubmitHandler>("SubmitArea");
        countdown = GetNode<CountdownHandler>("CountdownText");
        thoughtBox = GetNode<ThoughtBox>("ThoughtBox");
        filter = GetNode<ColorRect>("FilterOverlay/FadeColor");

        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        bgm.Play();

        timerBar.Timer.Connect(SignalNames.Timeout, this, nameof(PlayBadEnd));
        dialogue.Connect(nameof(DialogueHandler.FinishDisplay), this, nameof(OnFinishDisplay));
        dialogue.Connect(nameof(DialogueHandler.OutOfDialogue), this, nameof(PlayGoodEnd));
        submitArea.Connect(nameof(SubmitHandler.CorrectSubmission), this, nameof(ToNextPhase));
        submitArea.Connect(nameof(SubmitHandler.WrongSubmission), this, nameof(PlayError));

        thoughtBox.SetBounds();
        thoughtBox.RectScale = Vector2.Zero;
        timerBar.RectScale = Vector2.Zero;
        filter.Color = Colors.Black;
        isTransitioning = true;

        PlaySpawning();
    }

    private void StartGame()
    {
        dialogue.NextDialogue();
        timerBar.Timer.Start();
    }

    private void SpawnWordsAndSubmitBoxes()
    {
        var spawning = CreateTween();
        for(int i = 0; i < dialogue.WordList.Length; i++)
        {
            spawning.TweenCallback(this, nameof(SpawnThought), new Godot.Collections.Array(){dialogue.WordList[i]}).SetDelay(THOUGHT_SPAWN_INTERVAL);
        }
        spawning.TweenCallback(this, nameof(OnAllThoughtSpawned));
        spawning.Play();
        submitArea.SpawnSubmitBoxes(dialogue.Answer);
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
        dialogue.NextDialogue();
        isTransitioning = true;
    }

    private void ResetGame() // TODO: Debate if this is even necessary -> only for pausing but is pausing even needed?
    {
        dialogue.Reset();
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

    private void OnFinishDisplay()
    {
        if (isTransitioning)
        {
            SpawnWordsAndSubmitBoxes();
            isTransitioning = false;
        }
        else if (dialogue.IsErrorText)
        {
            dialogue.CurrentDialogue();
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
