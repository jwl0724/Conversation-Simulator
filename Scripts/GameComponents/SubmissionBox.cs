using Godot;

public class SubmissionBox : Control
{
    private const float ANIMATION_TIME = 0.2f;
    private const float SEPARATION_BONUS_FACTOR = 0.8f;
    private static readonly Vector2 DEFAULT_SIZE = new Vector2(200, 75);

    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    public Thought Submitted { get; private set; } = null; // TODO: Add ability to swap boxes when dragging one box to another that is filled
    private Thought hovered = null;
    private bool readyToSubmit = false;
    private int boxSeparation;

    public override void _Ready()
    {
        Area2D submitArea = GetNode<Area2D>("Area");
        submitArea.Connect(SignalNames.AreaEntered, this, nameof(OnAreaEntered));
        submitArea.Connect(SignalNames.AreaExited, this, nameof(OnAreaExited));

        boxSeparation = GetParent<HBoxContainer>().GetConstant("separation");
        RectMinSize = Vector2.Zero;
        Modulate = Colors.Transparent;

        var spawning = CreateTween();
        spawning.TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Expo);
        spawning.TweenProperty(this, PropertyNames.RectMinSize, DEFAULT_SIZE, ANIMATION_TIME)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        spawning.TweenCallback(this, nameof(OnFinishedPositioning));
        spawning.Play();
    }

    public override void _PhysicsProcess(float delta) // Needed cause button blocks MouseEnter/Exit
    {
        if (hovered == null || Submitted != null || !readyToSubmit) return;

        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - boxSeparation * SEPARATION_BONUS_FACTOR,
        right = RectGlobalPosition.x + RectSize.x + boxSeparation * SEPARATION_BONUS_FACTOR,
        up = RectGlobalPosition.y - boxSeparation / SEPARATION_BONUS_FACTOR,
        down = RectGlobalPosition.y + RectSize.y + boxSeparation / SEPARATION_BONUS_FACTOR;

        if (mousePos.x < left || mousePos.x > right || mousePos.y < up || mousePos.y > down)
        {
            hovered.SubmitTarget = null;
        }
        else
        {
            hovered.SubmitTarget = this;
        }
    }

    public void PlayDespawn()
    {
        readyToSubmit = false;

        var previousLocation = RectGlobalPosition;
        Node root = GetTree().CurrentScene, submitArea = GetParent();

        submitArea.RemoveChild(this);
        root.AddChild(this);
        root.MoveChild(this, 1);

        RectPosition = previousLocation;
        SetAnchorsPreset(LayoutPreset.Center);
        RectPivotOffset = RectSize / 2;

        var despawn = CreateTween();
        despawn.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        despawn.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.Transparent, ANIMATION_TIME);

        var thoughtDespawn = CreateTween();
        thoughtDespawn.TweenProperty(Submitted, PropertyNames.RectScale, Vector2.Zero, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        thoughtDespawn.TweenCallback(Submitted, PropertyNames.QueueFree);
        thoughtDespawn.TweenCallback(this, PropertyNames.QueueFree);

        despawn.Play();
        thoughtDespawn.Play();
    }

    public void NotifySubmit()
    {
        Submitted = hovered;
        EmitSignal(nameof(Submit));
    }

    public void NotifyUnsubmit()
    {
        Submitted = null;
        EmitSignal(nameof(Unsubmit));
    }

    private void OnFinishedPositioning()
    {
        readyToSubmit = true;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (Submitted != null) return;
        hovered = GetThought(area);
    }

    private void OnAreaExited(Area2D area)
    {
        if (Submitted != null) return;
        hovered = null;
    }

    private Thought GetThought(Area2D area)
    {
        Node node = area;
        while (node != null)
        {
            if (node is Thought thought) return thought;
            node = node.GetParent();
        }
        GD.PushError($"Could not find Thought Class in parent hiearchy for {Name}");
        return null;
    }
}
