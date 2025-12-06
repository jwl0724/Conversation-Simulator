using Godot;

public class TextCrawler : Label
{
    private const float SFX_VARIANCE = 0.3f;

    [Signal] public delegate void FinishCrawl();
    [Export] private NodePath overlapAudioPlayerPath = null;

    private SceneTreeTween currentCrawlTween;
    private SceneTreeTween currentAudioTween;
    private OverlapAudioPlayer audioPlayer = null;

    public override void _Ready()
    {
        PercentVisible = 0;
        if (overlapAudioPlayerPath != null) audioPlayer = GetNode<OverlapAudioPlayer>(overlapAudioPlayerPath);
    }

    public void PlayCrawl(string text, float crawlTime, float signalDelay = 0)
    {
        PercentVisible = 0;
        Text = text;

        var crawl = CreateTween();
        crawl.TweenProperty(this, PropertyNames.PercentVisible, 1, crawlTime);
        crawl.TweenInterval(signalDelay);
        crawl.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishCrawl)});
        crawl.Play();

        if (audioPlayer != null) // TODO: Probably sync the audio of clicks better with text display?
        {
            var sfx = CreateTween();
            float sfxInterval = 0.1f;
            for(int i = 0; i < crawlTime / sfxInterval; i++)
            {
                sfx.TweenInterval(sfxInterval);
                sfx.TweenCallback(audioPlayer, nameof(audioPlayer.Play), new Godot.Collections.Array(){1 - SFX_VARIANCE, 1 + SFX_VARIANCE});
            }
            sfx.Play();

            if (currentAudioTween != null && currentAudioTween.IsRunning()) currentAudioTween.Kill();
            currentAudioTween = sfx;
        }

        if (currentCrawlTween != null && currentCrawlTween.IsRunning()) currentCrawlTween.Kill();
        currentCrawlTween = crawl;
    }

    public void Reset()
    {
        if (currentCrawlTween != null && currentCrawlTween.IsRunning()) currentCrawlTween.Kill();
        if (currentAudioTween != null && currentAudioTween.IsRunning()) currentAudioTween.Kill();
        PercentVisible = 0;
    }
}
