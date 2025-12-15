using Godot;

public class GoodEndHandler : Control
{
    private const float DIALOGUE_LINGER_TIME = 1;
    private const float DIALOGUE_CRAWL_TIME = 0.5f;
    private const float VISUALS_SPAWN_TIME = 0.75f; // Food spawning and blackhole spawning

    [Signal] public delegate void FinishSequence();

    private TextCrawler narration;
    private Control plate;
    private Mouth mouth;

    public override void _Ready()
    {
        plate = GetNode<Control>("Plate");
        mouth = GetNode<Mouth>("Mouth");
        narration = GetNode<TextCrawler>("NarrationText");

        Visible = false;
        plate.Visible = false;
    }

    public void PlaySequence()
    {
        Visible = true;

        var sfx = plate.GetNode<AudioStreamPlayer2D>("Audio");
        NodeHelper.PlayRandomPitchAudio(sfx, 1, 1);

        var seq = CreateTween();

        // Spawn plate
        plate.RectScale = Vector2.Zero;
        plate.Modulate = Colors.Transparent;
        plate.Visible = true;
        seq.TweenProperty(plate, nameof(Modulate).ToLower(), Colors.White, VISUALS_SPAWN_TIME);
        seq.Parallel().TweenProperty(plate, PropertyNames.RectScale, Vector2.One, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

        // Spawn mouth
        seq.TweenCallback(mouth, nameof(mouth.PlaySpawn));
        seq.TweenInterval(Mouth.SPAWN_TIME);

        // Spawn foods
        foreach(Food food in GetTree().GetNodesInGroup(GroupNames.Foods))
        {
            food.Connect(nameof(Food.WasEaten), this, nameof(OnFoodEaten));
            seq.TweenCallback(food, nameof(food.PlaySpawn), new Godot.Collections.Array(){mouth});
            seq.TweenInterval(Food.SPAWN_TIME);
        }
        seq.Play();
    }

    private void OnFoodEaten()
    {
        foreach(Food food in GetTree().GetNodesInGroup(GroupNames.Foods))
        {
            if (!food.IsEaten) return;
        }
        PlayNarration();
    }

    private void PlayNarration()
    {
        narration.Visible = true;
        var seq = CreateTween();

        // Despawn plate and mouth
        seq.TweenProperty(plate, nameof(Modulate).ToLower(), Colors.Transparent, VISUALS_SPAWN_TIME);
        seq.Parallel().TweenProperty(plate, PropertyNames.RectScale, Vector2.Zero, VISUALS_SPAWN_TIME).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
        seq.Parallel().TweenCallback(mouth, nameof(mouth.PlayDespawn));

        // Play narration text
        foreach(string line in Globals.GOOD_END_NARRATION_SEQUENCE)
        {
            seq.TweenCallback(narration, nameof(narration.PlayCrawl), new Godot.Collections.Array(){line, DIALOGUE_CRAWL_TIME, 0});
            seq.TweenInterval(DIALOGUE_CRAWL_TIME + DIALOGUE_LINGER_TIME);
        }
        seq.TweenProperty(narration, nameof(Modulate).ToLower(), Colors.Transparent, DIALOGUE_LINGER_TIME);
        seq.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishSequence)});
        seq.Play();
    }
}
