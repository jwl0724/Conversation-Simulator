using Godot;
using System;

public class DialogueHandler : Node
{
    public const float SPAWN_TIME = 0.2f;
    public const float CRAWL_TIME = 0.3f;
    private const float TEXT_LINGER_TIME = 0.4f;

    [Export] private NodePath clerkBubblePath;
    [Export] private NodePath playerBubblePath;

    [Signal] public delegate void OutOfDialogue();
    [Signal] public delegate void PromptFinished();
    [Signal] private delegate void ExchangeFinished();

    public string[] WordList { get => Globals.WORD_BANK[dialogueIndex]; }
    public string Answer { get => Globals.DIALOGUE_KEY[dialogueIndex].Item2; }
    public bool IsErrorText { get => clerkBubble.CurrentText == Globals.ERROR_TEXT || clerkBubble.CurrentText == Globals.ERROR_CHANGE; }
    private int dialogueIndex = -1; // Needs to call NextDialogue to populate the first line

    private SpeechBubble clerkBubble;
    private SpeechBubble playerBubble;
    private SceneTreeTween currentRunning;

    public override void _Ready()
    {
        clerkBubble = GetNode<SpeechBubble>(clerkBubblePath);
        playerBubble = GetNode<SpeechBubble>(playerBubblePath);
    }

    public void NextDialogue()
    {
        if (dialogueIndex >= Globals.DIALOGUE_KEY.Length - 1) // Ending reached
        {
            // clerkBubble.Connect(nameof(SpeechBubble.FinishAnimation), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(OutOfDialogue)}, flags: (uint)ConnectFlags.Oneshot);
            // clerkBubble.PlaySwap(Globals.LAST_DIALOGUE, SPAWN_TIME, CRAWL_TIME);
            Connect(nameof(ExchangeFinished), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(OutOfDialogue)}, flags: (uint)ConnectFlags.Oneshot);
            PlayExchange(new Tuple<string, string>(Globals.DIALOGUE_KEY[dialogueIndex - 1].Item2, Globals.LAST_DIALOGUE));
            return;
        }
        dialogueIndex++;
        if (dialogueIndex == 0)
        {
            clerkBubble.PlayShow(Globals.DIALOGUE_KEY[dialogueIndex].Item1, SPAWN_TIME, CRAWL_TIME);
            clerkBubble.Connect(nameof(SpeechBubble.FinishAnimation), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(PromptFinished)}, flags: (uint)ConnectFlags.Oneshot);
        }
        else
        {
            Connect(nameof(ExchangeFinished), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(PromptFinished)}, flags: (uint)ConnectFlags.Oneshot);
            PlayExchange(new Tuple<string, string>(Globals.DIALOGUE_KEY[dialogueIndex - 1].Item2, Globals.DIALOGUE_KEY[dialogueIndex].Item1));
        }

    }

    public void ErrorDialogue(string submittedAnswer)
    {
        // clerkBubble.Connect(nameof(SpeechBubble.FinishAnimation), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(PromptFinished)}, flags: (uint)ConnectFlags.Oneshot);
        // clerkBubble.PlaySwap(errorText, SPAWN_TIME, CRAWL_TIME, ERROR_LINGER_TIME);
        // Connect(nameof(ExchangeFinished), this, nameof(PlayExchange), new Godot.Collections.Array(){Globals.PLAYER_ERROR, Globals.DIALOGUE_KEY[dialogueIndex].Item1}, flags:(uint)ConnectFlags.Oneshot);

        if (currentRunning != null && currentRunning.IsRunning()) currentRunning.Kill();
        string errorText = dialogueIndex != 4 ? Globals.ERROR_TEXT : Globals.ERROR_CHANGE;
        PlayExchange(new Tuple<string, string>(submittedAnswer, errorText), new Tuple<string, string>(Globals.PLAYER_ERROR, Globals.DIALOGUE_KEY[dialogueIndex].Item1));
    }

    // public void CurrentDialogue()
    // {
    //     bubble.PlaySwap(Globals.DIALOGUE_KEY[dialogueIndex].Item1, SPAWN_TIME, CRAWL_TIME);
    // }

    // public void Reset()
    // {
    //     dialogueIndex = -1;
    //     clerkBubble.Hide();
    // }

    private void PlayExchange(params Tuple<string, string>[] playerClerkTexts)
    {
        var exchange = CreateTween();
        currentRunning = exchange;
        foreach(Tuple<string, string> texts in playerClerkTexts)
        {
            // Hide clerk text
            exchange.TweenCallback(clerkBubble, nameof(clerkBubble.PlayHide), new Godot.Collections.Array(){SPAWN_TIME});
            exchange.TweenInterval(SPAWN_TIME);

            // Show player bubble with given text
            exchange.TweenCallback(playerBubble, nameof(playerBubble.PlayShow), new Godot.Collections.Array(){texts.Item1, SPAWN_TIME, CRAWL_TIME, TEXT_LINGER_TIME});
            exchange.TweenInterval(SPAWN_TIME + CRAWL_TIME + TEXT_LINGER_TIME);

            // Hide player bubble
            exchange.TweenCallback(playerBubble, nameof(playerBubble.PlayHide), new Godot.Collections.Array(){SPAWN_TIME});
            exchange.TweenInterval(SPAWN_TIME);

            // Show clerk response to player text
            exchange.TweenCallback(clerkBubble, nameof(clerkBubble.PlayShow), new Godot.Collections.Array(){texts.Item2, SPAWN_TIME, CRAWL_TIME, TEXT_LINGER_TIME});
            exchange.TweenInterval(SPAWN_TIME + CRAWL_TIME + TEXT_LINGER_TIME);
        }
        exchange.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(ExchangeFinished)});
        exchange.Play();
    }
}
