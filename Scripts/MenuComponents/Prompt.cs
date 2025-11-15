using Godot;
using System;
using System.Linq;

public class Prompt : Label
{
    private const float CRAWL_TIME = 1f;

    [Signal] public delegate void FinishCrawl();
    [Signal] public delegate void OutOfDialogue();

    private int dialogueIndex = 0;

    public override void _Ready()
    {
        PercentVisible = 0;
    }

    public void NextDialogue()
    {
        if (dialogueIndex >= Globals.DIALOGUE_KEY.Length)
        {
            EmitSignal(nameof(OutOfDialogue));
            return;
        }
        PercentVisible = 0;
        Text = Globals.DIALOGUE_KEY[dialogueIndex].Item1;
        dialogueIndex++;

        PlayCrawl();
    }

    public void ErrorDialogue()
    {
        PercentVisible = 0;
        Text = Globals.ERROR_TEXT;
        PlayCrawl();
    }

    public void Reset()
    {
        dialogueIndex = 0;
        PercentVisible = 0;
    }

    private void PlayCrawl()
    {
        // TODO: Maybe add some sfx when text is being added? do much later
        var crawl = CreateTween();
        crawl.TweenProperty(this, PropertyNames.PercentVisible, 1, CRAWL_TIME);
        crawl.Play();
    }
}
