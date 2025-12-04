using Godot;

public class TextCrawler : Label
{
    [Signal] public delegate void FinishCrawl();
    private SceneTreeTween currentCrawlTween;

    public override void _Ready()
    {
        PercentVisible = 0;
    }

    public void PlayCrawl(string text, float crawlTime, float signalDelay = 0)
    {
        PercentVisible = 0;
        Text = text;

        // TODO: Maybe add some sfx when text is being added? do much later
        var crawl = CreateTween();
        crawl.TweenProperty(this, PropertyNames.PercentVisible, 1, crawlTime);
        crawl.TweenInterval(signalDelay);
        crawl.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishCrawl)});
        crawl.Play();

        if (currentCrawlTween != null && currentCrawlTween.IsRunning()) currentCrawlTween.Kill();
        currentCrawlTween = crawl;
    }

    public void Reset()
    {
        if (currentCrawlTween != null && currentCrawlTween.IsRunning()) currentCrawlTween.Kill();
        PercentVisible = 0;
    }
}
