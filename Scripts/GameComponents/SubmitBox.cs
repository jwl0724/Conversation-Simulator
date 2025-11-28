using Godot;

public class SubmitBox : Control
{
    private const float ANIMATION_TIME = 0.2f;
    private const float SUBMIT_LERP_STRENGTH = 0.6f;
    private const float SEPARATION_BONUS_FACTOR = 0.8f;
    private static readonly Vector2 DEFAULT_SIZE = new Vector2(200, 75);

    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    public Thought Submitted { get; private set; } = null; // TODO: Add ability to swap boxes when dragging one box to another that is filled
    private Thought heldThought = null;
    private bool readyToAccept = false;
    private int boxMargins;
    private int startingChildCount; // Used to detect if submit box has thought parented to it

    public override void _Ready()
    {
        boxMargins = GetParent<HBoxContainer>().GetConstant("separation") / 2;
        startingChildCount = GetChildCount();
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
            // Lerp towards center and not the left corner
            heldThought.RectGlobalPosition = MathHelper.GetPositionFromCenter(heldThought,
                (heldThought.RectGlobalPosition + heldThought.RectSize / 2 * Thought.HELD_SIZE)
                    .LinearInterpolate(RectGlobalPosition + RectSize / 2, SUBMIT_LERP_STRENGTH)
            );
        }
        else
        {
            heldThought.ToggleMovement(true);
            foreach(SubmitBox box in GetTree().GetNodesInGroup(GroupNames.SubmitBoxes))
            {
                if (box == this) continue; // Skip the caller instance
                if (box.MouseInRange() && box.Submitted == null) heldThought.ToggleMovement(false);
            }
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
        float left = RectGlobalPosition.x - boxMargins * SEPARATION_BONUS_FACTOR,
        right = RectGlobalPosition.x + RectSize.x + boxMargins * SEPARATION_BONUS_FACTOR,
        up = RectGlobalPosition.y - boxMargins / SEPARATION_BONUS_FACTOR,
        down = RectGlobalPosition.y + RectSize.y + boxMargins / SEPARATION_BONUS_FACTOR;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }

    private void OnThoughtPickup(Thought thought)
    {
        heldThought = thought;
        if (thought != Submitted) return; // If held doesn't match submitted, do nothing

        Submitted = null;
        var oldGlobalPos = thought.RectGlobalPosition;
        heldThought.SetAsToplevel(true);
        thought.RectGlobalPosition = oldGlobalPos;
        EmitSignal(nameof(Unsubmit));
    }

    private void OnThoughtReleased(Thought thought)
    {
        heldThought = null;
        if (Submitted != null) return; // If submit box has something, do nothing

        if (MouseInRange())
        {
            Submitted = thought;
            NodeHelper.ReparentNode(thought, this);
            thought.RectPosition = Vector2.Zero;
            thought.SetAsToplevel(false);
            EmitSignal(nameof(Submit));
        }
        else if (GetChildCount() > startingChildCount)
        {
            NodeHelper.ReparentNode(thought);
            thought.SetAsToplevel(false);
        }
    }

    private void OnFinishedPositioning()
    {
        readyToAccept = true;
    }
}
