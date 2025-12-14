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
    private GoodEndHandler goodEndHandler;
    private BadEndHandler badEndHandler;
    private TimerBar timerBar;
    private DialogueHandler dialogue;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;

    private bool gameOver = false;

    // TODO: increase move speed of thoughts as time gets lower
    // TODO: add some slight flashing effect when running low on time (similar to that of vertigo)
    // TODO: maybe add some particle emitter depending on what stage the dialogue is at that emits the desired thing
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
        goodEndHandler = GetNode<GoodEndHandler>("FilterOverlay/GoodEnd");
        badEndHandler = GetNode<BadEndHandler>("FilterOverlay/BadEnd");

        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        bgm.Play();

        timerBar.ConnectToTimeout(this, nameof(OnTimeout));
        dialogue.Connect(nameof(DialogueHandler.BadEndDialogueFinished), this, nameof(PlayBadEnd));
        timerBar.ConnectTimerToSignal(dialogue, nameof(DialogueHandler.OutOfDialogue), nameof(Timer.Stop).ToLower());
        dialogue.Connect(nameof(DialogueHandler.PromptFinished), this, nameof(SpawnWordsAndSubmitBoxes));
        dialogue.Connect(nameof(DialogueHandler.LastDialogueFinished), this, nameof(PlayGoodEnd));
        submitArea.Connect(nameof(SubmitHandler.CorrectSubmission), this, nameof(ToNextPhase));
        submitArea.Connect(nameof(SubmitHandler.WrongSubmission), this, nameof(PlayError));

        thoughtBox.SetBounds();
        thoughtBox.RectScale = Vector2.Zero;
        timerBar.RectScale = Vector2.Zero;
        filter.Color = Colors.Black;

        PlaySpawning();
    }

    private void StartGame()
    {
        dialogue.NextDialogue();
        timerBar.Start();
    }

    private void SpawnWordsAndSubmitBoxes()
    {
        if (gameOver) return;

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
        if (gameOver) return;

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

    private void OnAllThoughtSpawned()
    {
        if (gameOver) return;

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

    private void OnTimeout()
    {
        gameOver = true;
        dialogue.BadEndDialogue();
        submitArea.DespawnSubmitBoxes();

        if (GetTree().GetNodesInGroup(GroupNames.Thoughts).Count == 0) return;

        var despawn = CreateTween();
        foreach(Thought thought in GetTree().GetNodesInGroup(GroupNames.Thoughts))
        {
            thought.Disabled = true;
            if (thought.IsSubmitted) continue; // SubmissionBox will handle despawning submitted
            despawn.TweenCallback(thought, nameof(thought.Despawn)).SetDelay(THOUGHT_DESPAWN_INTERVAL);
        }
        despawn.Play();
    }
}
