using Godot;

public class GoodEndHandler : Control
{
    private const float DIALOGUE_LINGER_TIME = 1;
    private const float DIALOGUE_CRAWL_TIME = 0.5f;
    private const float VISUALS_SPAWN_TIME = 1f; // Food spawning and blackhole spawning

    [Signal] public delegate void FinishSequence();

    private TextCrawler clerkText;
    private TextCrawler narrationText;
    private ColorRect blackhole;
    private TextureRect burger;
    private TextureRect nuggets;

    private Vector2 defaultBurgerPosition;
    private Vector2 defaultNuggetsPosition;

    public override void _Ready()
    {
        Visible = false;

        clerkText = GetNode<TextCrawler>("ClerkText");
        narrationText = GetNode<TextCrawler>("NarrationText");
        burger = GetNode<TextureRect>("Burger");
        nuggets = GetNode<TextureRect>("Nuggets");
        blackhole = GetNode<ColorRect>("Blackhole");

        burger.Visible = false;
        nuggets.Visible = false;
        blackhole.Visible = false;

        defaultBurgerPosition = burger.RectPosition;
        defaultNuggetsPosition = nuggets.RectPosition;

        // PlaySequence(); // Temporarily here just to play it and see the sequence
        CallDeferred(nameof(PlaySequence));
    }

    public void PlaySequence()
    {
        Visible = true;
        var seq = CreateTween();

        // Clerk food is ready dialogue
        seq.TweenCallback(clerkText, nameof(clerkText.PlayCrawl), new Godot.Collections.Array(){Globals.GOOD_END_CLERK_TEXT, DIALOGUE_CRAWL_TIME, 0});
        seq.TweenInterval(DIALOGUE_CRAWL_TIME + DIALOGUE_LINGER_TIME);
        seq.TweenProperty(clerkText, nameof(Modulate).ToLower(), Colors.Transparent, DIALOGUE_CRAWL_TIME);

        // Set up food visuals
        burger.RectPosition = new Vector2(defaultBurgerPosition.x, 0);
        burger.RectScale = Vector2.Zero;
        burger.RectRotation = 0;
        burger.Visible = true;
        nuggets.RectPosition = new Vector2(defaultNuggetsPosition.x, 0);
        nuggets.RectScale = Vector2.Zero;
        nuggets.RectRotation = 0;
        nuggets.Visible = true;

        // Burger spawn in
        seq.TweenProperty(burger, PropertyNames.RectScale, Vector2.One, VISUALS_SPAWN_TIME * 0.25f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        seq.TweenInterval(VISUALS_SPAWN_TIME * 0.1f);
        seq.TweenProperty(burger, PropertyNames.RectPosition, defaultBurgerPosition, VISUALS_SPAWN_TIME * 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenCallback(this, nameof(PlayLandingWiggle), new Godot.Collections.Array(){burger, 10, 0.85f, VISUALS_SPAWN_TIME * 0.075f});

        // Nuggets spawn in
        seq.TweenProperty(nuggets, PropertyNames.RectScale, Vector2.One, VISUALS_SPAWN_TIME * 0.25f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        seq.TweenInterval(VISUALS_SPAWN_TIME * 0.1f);
        seq.TweenProperty(nuggets, PropertyNames.RectPosition, defaultNuggetsPosition, VISUALS_SPAWN_TIME * 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenCallback(this, nameof(PlayLandingWiggle), new Godot.Collections.Array(){nuggets, 16, 0.85f, VISUALS_SPAWN_TIME * 0.075f});

        seq.TweenInterval(DIALOGUE_LINGER_TIME);

        // Spawn in blackhole
        blackhole.RectScale = Vector2.Zero;
        blackhole.Visible = true;
        seq.TweenProperty(blackhole, PropertyNames.RectScale, Vector2.One, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        seq.TweenInterval(0.01f);

        // Despawn burger
        seq.Parallel().TweenProperty(burger, PropertyNames.RectPosition, blackhole.RectPosition, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenProperty(burger, PropertyNames.RectScale, Vector2.Zero, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenProperty(burger, PropertyNames.RectRotation, 360, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart);
        seq.Parallel().TweenProperty(burger, nameof(Modulate).ToLower(), Colors.Transparent, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);

        // Despawn nuggets
        seq.Parallel().TweenProperty(nuggets, PropertyNames.RectPosition, blackhole.RectPosition, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenProperty(nuggets, PropertyNames.RectScale, Vector2.Zero, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
        seq.Parallel().TweenProperty(nuggets, PropertyNames.RectRotation, 360, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart);
        seq.Parallel().TweenProperty(nuggets, nameof(Modulate).ToLower(), Colors.Transparent, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);

        seq.TweenProperty(blackhole, PropertyNames.RectScale, Vector2.Zero, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
        seq.TweenInterval(DIALOGUE_LINGER_TIME);

        // Play ending narration
        foreach(string line in Globals.GOOD_END_NARRATION_SEQUENCE)
        {
            seq.TweenCallback(narrationText, nameof(clerkText.PlayCrawl), new Godot.Collections.Array(){line, DIALOGUE_CRAWL_TIME, 0});
            seq.TweenInterval(DIALOGUE_CRAWL_TIME + DIALOGUE_LINGER_TIME);
        }
        seq.TweenProperty(narrationText, nameof(Modulate).ToLower(), Colors.Transparent, DIALOGUE_CRAWL_TIME);

        seq.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishSequence)});
        seq.Play();
    }

    private void PlayLandingWiggle(Object @object, float offset, float ratioUsed, float delay)
    {
        float factor = (1 - ratioUsed) / 3;
        var wiggle = CreateTween();
        wiggle.TweenInterval(delay);
        wiggle.TweenProperty(@object, PropertyNames.RectRotation, -offset, VISUALS_SPAWN_TIME * factor);
        wiggle.TweenProperty(@object, PropertyNames.RectRotation, offset, VISUALS_SPAWN_TIME * factor);
        wiggle.TweenProperty(@object, PropertyNames.RectRotation, 0, VISUALS_SPAWN_TIME * factor);
        wiggle.Play();
    }
}
