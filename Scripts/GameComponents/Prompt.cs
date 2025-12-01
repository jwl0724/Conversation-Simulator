using Godot;
using System;
using System.Linq;

public class Prompt : Label
{
    public const float CRAWL_TIME = 0.25f;
    private const float ERROR_LINGER_TIME = 0.75f;

    [Signal] public delegate void FinishCrawl();
    [Signal] public delegate void OutOfDialogue();

    public string[] WordList { get => Globals.WORD_BANK[dialogueIndex]; }
    public string Answer { get => Globals.DIALOGUE_KEY[dialogueIndex].Item2; }
    public bool IsErrorText { get => Text == Globals.ERROR_TEXT; }
    private int dialogueIndex = -1; // Needs to call NextDialogue to populate the first line
    private SceneTreeTween currentCrawlTween;

    public override void _Ready()
    {
        PercentVisible = 0;
    }

    public void NextDialogue()
    {
        if (dialogueIndex >= Globals.DIALOGUE_KEY.Length - 1)
        {
            EmitSignal(nameof(OutOfDialogue));
            return;
        }
        dialogueIndex++;
        PercentVisible = 0;
        Text = Globals.DIALOGUE_KEY[dialogueIndex].Item1;

        PlayCrawl();
    }

    public void CurrentDialogue()
    {
        PercentVisible = 0;
        Text = Globals.DIALOGUE_KEY[dialogueIndex].Item1;
        PlayCrawl();
    }

    public void ErrorDialogue()
    {
        PercentVisible = 0;
        Text = Globals.ERROR_TEXT;
        PlayCrawl(ERROR_LINGER_TIME);
    }

    public void Reset()
    {
        dialogueIndex = -1;
        PercentVisible = 0;
    }

    private void PlayCrawl(float signalDelay = 0)
    {
        // TODO: Maybe add some sfx when text is being added? do much later
        var crawl = CreateTween();
        crawl.TweenProperty(this, PropertyNames.PercentVisible, 1, CRAWL_TIME);
        crawl.TweenInterval(signalDelay);
        crawl.TweenCallback(this, "emit_signal", new Godot.Collections.Array(){nameof(FinishCrawl)});
        crawl.Play();

        if (currentCrawlTween != null && currentCrawlTween.IsRunning()) currentCrawlTween.Kill();
        currentCrawlTween = crawl;
    }
}
