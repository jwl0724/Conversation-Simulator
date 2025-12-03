using Godot;
using System;

public class DialogueHandler : Node
{
    public const float CRAWL_TIME = 0.25f;
    private const float ERROR_LINGER_TIME = 0.75f;

    [Export] private NodePath textPath;

    [Signal] public delegate void OutOfDialogue();
    [Signal] public delegate void FinishCrawl();

    public string[] WordList { get => Globals.WORD_BANK[dialogueIndex]; }
    public string Answer { get => Globals.DIALOGUE_KEY[dialogueIndex].Item2; }
    public bool IsErrorText { get => crawler.Text == Globals.ERROR_TEXT || crawler.Text == Globals.ERROR_CHANGE; }
    private int dialogueIndex = -1; // Needs to call NextDialogue to populate the first line
    private TextCrawler crawler;

    public override void _Ready()
    {
        crawler = GetNode<TextCrawler>(textPath);
        crawler.Connect(nameof(TextCrawler.FinishCrawl), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishCrawl)}); // Propagate signal up
    }

    public void NextDialogue()
    {
        if (dialogueIndex >= Globals.DIALOGUE_KEY.Length - 1)
        {
            EmitSignal(nameof(OutOfDialogue));
            return;
        }
        dialogueIndex++;
        crawler.PlayCrawl(Globals.DIALOGUE_KEY[dialogueIndex].Item1, CRAWL_TIME);
    }

    public void CurrentDialogue()
    {
        crawler.PlayCrawl(Globals.DIALOGUE_KEY[dialogueIndex].Item1, CRAWL_TIME);
    }

    public void ErrorDialogue()
    {
        string errorText = dialogueIndex != 4 ? Globals.ERROR_TEXT : Globals.ERROR_CHANGE;
        crawler.PlayCrawl(errorText, CRAWL_TIME, ERROR_LINGER_TIME);
    }

    public void Reset()
    {
        dialogueIndex = -1;
        crawler.Reset();
    }
}
