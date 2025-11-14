using Godot;
using System;
using System.Linq;

public class Prompt : Label
{
    // DIALOGUE SEQUENCES
    private readonly string[] DIALOGUE_SEQUENCE =
    {
        "Welcome to RucRonalds, What can I get for you today?", // One Big Ruc Please
        "Would you like anything else?", // A Box of RucNuggets
        "Would you like fries with that?", // Yes
        "How about soda?", // Yes
        "That would be $6.90 please.", // Value > 6.90
        "Would you like change?" // Yes/No both work
    };
    private readonly string[] GOOD_END_SEQUENCE =
    {
        "One Big Ruc, RucNuggets, and fries with soda coming up.",
        "Please wait one moment, your order will be ready soon."
    };
    private readonly string[] BAD_END_SEQUENCE =
    {
        "I'm sorry but you're holding up the line",
        "Could you please move aside for the other customers?"
    };
    private readonly string ERROR_TEXT = "Sorry I couldn't quite understand, could you repeat that?";

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
        if (dialogueIndex >= DIALOGUE_SEQUENCE.Length)
        {
            EmitSignal(nameof(OutOfDialogue));
            return;
        }
        PercentVisible = 0;
        Text = DIALOGUE_SEQUENCE[dialogueIndex];
        dialogueIndex++;

        // TODO: Maybe add some sfx when text is being added? do much later
        PlayCrawl();
    }

    public void ErrorDialogue()
    {
        PercentVisible = 0;
        Text = ERROR_TEXT;
        PlayCrawl();
    }

    public void Reset()
    {
        dialogueIndex = 0;
        PercentVisible = 0;
    }

    private void PlayCrawl()
    {
        var crawl = CreateTween();
        crawl.TweenProperty(this, PropertyNames.PercentVisible, 1, CRAWL_TIME);
        crawl.Play();
    }
}
