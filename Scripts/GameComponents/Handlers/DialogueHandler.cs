using Godot;
using System;

public class DialogueHandler : Node
{
    public const float SPAWN_TIME = 0.35f;
    public const float CRAWL_TIME = 0.5f;
    private const float ERROR_LINGER_TIME = 0.75f;

    [Export] private NodePath speechBubblePath;

    [Signal] public delegate void OutOfDialogue();
    [Signal] public delegate void FinishDisplay();

    public string[] WordList { get => Globals.WORD_BANK[dialogueIndex]; }
    public string Answer { get => Globals.DIALOGUE_KEY[dialogueIndex].Item2; }
    public bool IsErrorText { get => bubble.CurrentText == Globals.ERROR_TEXT || bubble.CurrentText == Globals.ERROR_CHANGE; }
    private int dialogueIndex = -1; // Needs to call NextDialogue to populate the first line
    private SpeechBubble bubble;

    public override void _Ready()
    {
        bubble = GetNode<SpeechBubble>(speechBubblePath);
        bubble.Connect(nameof(SpeechBubble.FinishAnimation), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishDisplay)});
    }

    public void NextDialogue()
    {
        if (dialogueIndex >= Globals.DIALOGUE_KEY.Length - 1)
        {
            EmitSignal(nameof(OutOfDialogue));
            return;
        }
        dialogueIndex++;
        bubble.PlayShow(Globals.DIALOGUE_KEY[dialogueIndex].Item1, SPAWN_TIME, CRAWL_TIME);
    }

    public void CurrentDialogue()
    {
        bubble.PlaySwap(Globals.DIALOGUE_KEY[dialogueIndex].Item1, SPAWN_TIME, CRAWL_TIME);
    }

    public void ErrorDialogue()
    {
        string errorText = dialogueIndex != 4 ? Globals.ERROR_TEXT : Globals.ERROR_CHANGE;
        bubble.PlaySwap(errorText, SPAWN_TIME, CRAWL_TIME, ERROR_LINGER_TIME);
    }

    public void Reset()
    {
        dialogueIndex = -1;
        bubble.Hide();
    }
}
