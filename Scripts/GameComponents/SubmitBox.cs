using Godot;

public class SubmitBox : Control
{
    private const float ANIMATION_TIME = 0.2f;
    private const float SUBMIT_LERP_STRENGTH = 0.5f;
    private const float SEPARATION_BONUS_FACTOR = 0.8f;
    private static readonly Vector2 DEFAULT_SIZE = new Vector2(200, 75);

    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    public Thought Submitted { get; private set; } = null; // TODO: Add ability to swap boxes when dragging one box to another that is filled
    private Thought heldThought = null;
    private bool readyToAccept = false;
    private int boxSeparation;

    // TODO: Bug where selecting a box just randomly went back to something that ALREADY had something selected?????

    public override void _Ready()
    {
        Area2D submitArea = GetNode<Area2D>("Area");
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

    public override void _PhysicsProcess(float delta)
    {
        if (heldThought == null || !readyToAccept) return;

        if (MouseInRange() && Submitted == null)
        {
            heldThought.ToggleMovement(false);
            heldThought.RectGlobalPosition = heldThought.RectGlobalPosition.LinearInterpolate(RectGlobalPosition, SUBMIT_LERP_STRENGTH);
        }
        else
        {
            heldThought.ToggleMovement(true);
        }

    }

    public void StartListening()
    {
        var thoughts = GetTree().GetNodesInGroup(GroupNames.Thoughts);
        foreach(Thought thought in thoughts)
        {
            thought.Connect(nameof(Thought.ThoughtPickedUp), this, nameof(OnThoughtPickup));
            thought.Connect(nameof(Thought.ThoughtReleased), this, nameof(OnThoughtReleased));
        }
    }

    public void PlayDespawn()
    {
        readyToAccept = false;

        var previousLocation = RectGlobalPosition;
        NodeHelper.ReparentNode(this);

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

    // TODO: Eject function that kicks the submitted thought out of the submit box
    public void EjectThought()
    {

    }

    private bool MouseInRange()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - boxSeparation * SEPARATION_BONUS_FACTOR,
        right = RectGlobalPosition.x + RectSize.x + boxSeparation * SEPARATION_BONUS_FACTOR,
        up = RectGlobalPosition.y - boxSeparation / SEPARATION_BONUS_FACTOR,
        down = RectGlobalPosition.y + RectSize.y + boxSeparation / SEPARATION_BONUS_FACTOR;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }

    private void OnThoughtPickup(Thought thought)
    {
        heldThought = thought;
        if (thought != Submitted) return;

        Submitted = null;
        NodeHelper.ReparentNode(heldThought);
        EmitSignal(nameof(Unsubmit));
    }

    private void OnThoughtReleased(Thought thought)
    {
        heldThought = null;
        if (!MouseInRange() || Submitted != null) return;

        Submitted = thought;
        NodeHelper.ReparentNode(thought, this);
        thought.RectPosition = Vector2.Zero;
        EmitSignal(nameof(Submit));
    }

    private void OnFinishedPositioning()
    {
        readyToAccept = true;
    }
}
