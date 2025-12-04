using Godot;

public class GoodEndHandler : Control
{
    [Signal] public delegate void FinishSequence();

    private TextCrawler clerkText;
    private TextCrawler narrationText;
    private TextureRect burger;
    private TextureRect nuggets;

    public override void _Ready()
    {
        // Visible = false;

        clerkText = GetNode<TextCrawler>("ClerkText");
        narrationText = GetNode<TextCrawler>("NarrationText");
        burger = GetNode<TextureRect>("Burger");
        nuggets = GetNode<TextureRect>("Nuggets");

        PlaySequence(); // Temporarily here just to play it and see the sequence
    }

    public void PlaySequence()
    {

    }
}
