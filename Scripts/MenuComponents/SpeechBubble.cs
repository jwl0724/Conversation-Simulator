using Godot;
using System;

public class SpeechBubble : Control
{
    [Export] private Texture speakerPicture;
    [Export] private bool flipH = false;

    [Signal] public delegate void FinishAnimation();

    public string CurrentText { get => crawler.Text; }
    private Control bubble;
    private TextureRect speakerPortrait;
    private TextCrawler crawler;

    public override void _Ready()
    {
        speakerPortrait = GetNode<TextureRect>("Speaker");
        crawler = GetNode<TextCrawler>("Label");
        bubble = GetNode<Control>("Background");

        Visible = false;
        speakerPortrait.Texture = speakerPicture;

        // Crawl will always be last step of Show/Swap, Hide won't touch the text
        crawler.Connect(nameof(TextCrawler.FinishCrawl), this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishAnimation)});

        if (flipH) // Flip entire node then flip text again to restore text
        {
            RectScale = new Vector2(-1, 1);
            crawler.RectScale = new Vector2(-1, 1);
        }
    }

    public void PlayShow(string text, float animationTime, float crawlTime, float signalDelay = 0)
    {
        Vector2 portraitDefaultPos = speakerPortrait.RectPosition;

        speakerPortrait.RectPosition = new Vector2(-speakerPortrait.RectSize.x, portraitDefaultPos.y);
        speakerPortrait.Modulate = Colors.Transparent;
        bubble.RectScale = Vector2.Zero;
        bubble.Modulate = Colors.Transparent;

        crawler.Reset();
        Visible = true;

        float steps = 2;
        var show = CreateTween();

        show.TweenProperty(speakerPortrait, PropertyNames.RectPosition, portraitDefaultPos, animationTime / steps);
        show.Parallel().TweenProperty(speakerPortrait, nameof(Modulate).ToLower(), Colors.White, animationTime / steps);

        show.TweenProperty(bubble, PropertyNames.RectScale, Vector2.One, animationTime / steps);
        show.Parallel().TweenProperty(bubble, nameof(Modulate).ToLower(), Colors.White, animationTime / steps);

        show.TweenCallback(crawler, nameof(crawler.PlayCrawl), new Godot.Collections.Array(){text, crawlTime, signalDelay});
        show.Play();
    }

    public void PlayHide(float animationTime)
    {
        float steps = 2;
        var hide = CreateTween();

        hide.TweenProperty(bubble, PropertyNames.RectScale, Vector2.Zero, animationTime / steps);
        hide.Parallel().TweenProperty(bubble, nameof(Modulate).ToLower(), Colors.Transparent, animationTime / steps);

        hide.TweenProperty(speakerPortrait, PropertyNames.RectPosition, new Vector2(-speakerPortrait.RectSize.x, speakerPortrait.RectPosition.y), animationTime / steps);
        hide.Parallel().TweenProperty(speakerPortrait, nameof(Modulate).ToLower(), Colors.Transparent, animationTime / steps);

        hide.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishAnimation)});
        hide.Play();
    }

    public void PlaySwap(string newText, float animationTime, float crawlTime, float signalDelay = 0)
    {
        Vector2 portraitDefaultPos = speakerPortrait.RectPosition; 
        float steps = 5;
        var swap = CreateTween();

        swap.TweenProperty(bubble, PropertyNames.RectScale, Vector2.Zero, animationTime / steps);
        swap.Parallel().TweenProperty(bubble, nameof(Modulate).ToLower(), Colors.Transparent, animationTime / steps);

        swap.TweenProperty(speakerPortrait, PropertyNames.RectPosition, new Vector2(-speakerPortrait.RectSize.x, portraitDefaultPos.y), animationTime / steps);
        swap.Parallel().TweenProperty(speakerPortrait, nameof(Modulate).ToLower(), Colors.Transparent, animationTime / steps);

        swap.TweenInterval(animationTime / steps);
        crawler.Reset();

        swap.TweenProperty(speakerPortrait, PropertyNames.RectPosition, portraitDefaultPos, animationTime / steps);
        swap.Parallel().TweenProperty(speakerPortrait, nameof(Modulate).ToLower(), Colors.White, animationTime / steps);

        swap.TweenProperty(bubble, PropertyNames.RectScale, Vector2.One, animationTime / steps);
        swap.Parallel().TweenProperty(bubble, nameof(Modulate).ToLower(), Colors.White, animationTime / steps);

        swap.TweenCallback(crawler, nameof(crawler.PlayCrawl), new Godot.Collections.Array(){newText, crawlTime, signalDelay});
        swap.Play();
    }
}
