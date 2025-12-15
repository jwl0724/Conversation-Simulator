using Godot;
using System;

public partial class InGame : Control
{
    private const float THOUGHT_SPAWN_INTERVAL = 0.05f;
    private const float THOUGHT_DESPAWN_INTERVAL = 0.025f;
    private const float OFFSET_RANGE = 50;
    private const float BONUS_SPEED_INCREASE = 1;

    [Export] private PackedScene thoughtTemplate;
    [Export] private AudioStreamMP3 correctSFX;
    [Export] private AudioStreamMP3 wrongSFX;

    private PanicHandler panicFilter;
    private ColorRect filter;
    private ThoughtBox thoughtBox;
    private CountdownHandler countdown;
    private ParticleHandler particles;
    private GoodEndHandler goodEndHandler;
    private BadEndHandler badEndHandler;
    private TimerBar timerBar;
    private DialogueHandler dialogue;
    private SubmitHandler submitArea;
    private AudioStreamPlayer bgm;
    private AudioStreamPlayer sfx;

    private bool gameOver = false;
    private bool isIncreasingVelocity = false;

    public override void _Ready()
    {
        GD.Randomize();

        timerBar = GetNode<TimerBar>("TimerBar");
        bgm = GetNode<AudioStreamPlayer>("BGM");
        sfx = GetNode<AudioStreamPlayer>("SFX");
        particles = GetNode<ParticleHandler>("Particles");
        dialogue = GetNode<DialogueHandler>("DialogueHandler");
        submitArea = GetNode<SubmitHandler>("SubmitArea");
        countdown = GetNode<CountdownHandler>("CountdownText");
        thoughtBox = GetNode<ThoughtBox>("ThoughtBox");
        filter = GetNode<ColorRect>("FilterOverlay/FadeColor");
        panicFilter = GetNode<PanicHandler>("FilterOverlay/PanicFilter");
        goodEndHandler = GetNode<GoodEndHandler>("FilterOverlay/GoodEnd");
        badEndHandler = GetNode<BadEndHandler>("FilterOverlay/BadEnd");

        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        bgm.Play();

        timerBar.ConnectToTimeout(this, nameof(OnTimeout));
        timerBar.Connect(nameof(TimerBar.HalfTimeUsed), this, nameof(OnHalfTimeUsed));
        dialogue.Connect(nameof(DialogueHandler.OutOfDialogue), this, nameof(OnOutOfDialogue));
        dialogue.Connect(nameof(DialogueHandler.BadEndDialogueFinished), this, nameof(PlayBadEnd));
        dialogue.Connect(nameof(DialogueHandler.PromptFinished), this, nameof(SpawnWordsAndSubmitBoxes));
        dialogue.Connect(nameof(DialogueHandler.LastDialogueFinished), this, nameof(PlayGoodEnd));
        submitArea.Connect(nameof(SubmitHandler.CorrectSubmission), this, nameof(ToNextPhase));
        submitArea.Connect(nameof(SubmitHandler.WrongSubmission), this, nameof(PlayError));

        thoughtBox.SetBounds();
        thoughtBox.RectScale = Vector2.Zero;
        timerBar.RectScale = Vector2.Zero;
        filter.Color = Colors.Black;
        particles.Emitting = false;

        PlaySpawning();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!isIncreasingVelocity) return;

        float timeRatio = 1 - timerBar.TimeLeft / (Globals.TIME_LIMIT / 2);
        Thought.SetNewVelocityRange(1 + BONUS_SPEED_INCREASE * timeRatio);
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
        particles.EmitHint(dialogue.CurrentOrder);
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

        sfx.Stream = correctSFX;
        NodeHelper.PlayRandomPitchAudio(sfx, 1, 1);
        particles.EmitHint(DialogueHandler.Order.NONE);

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
        isIncreasingVelocity = false;
        particles.Emitting = false;
        panicFilter.StopPanic();
        dialogue.BadEndDialogue();
        submitArea.DespawnSubmitBoxes();
        Thought.SetNewVelocityRange(1);

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

    private void OnOutOfDialogue()
    {
        timerBar.Stop();
        panicFilter.StopPanic();
        gameOver = true;
        isIncreasingVelocity = false;
        Thought.SetNewVelocityRange(1);
    }

    private void OnHalfTimeUsed()
    {
        panicFilter.StartPanic(timerBar);
        isIncreasingVelocity = true;
    }
}
