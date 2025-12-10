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
    private Vector2 portraitDefaultPos;

    public override void _Ready()
    {
        speakerPortrait = GetNode<TextureRect>("Speaker");
        crawler = GetNode<TextCrawler>("Background/Label");
        bubble = GetNode<Control>("Background");

        Visible = false;
        speakerPortrait.Texture = speakerPicture;
        portraitDefaultPos = speakerPortrait.RectPosition;

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
        speakerPortrait.RectPosition = new Vector2(-speakerPortrait.RectSize.x, portraitDefaultPos.y);
        speakerPortrait.Modulate = Colors.Transparent;
        bubble.RectScale = Vector2.Zero;
        bubble.Modulate = Colors.Transparent;
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
        hide.TweenCallback(crawler, nameof(crawler.Reset));

        hide.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishAnimation)});
        hide.Play();
    }
}
