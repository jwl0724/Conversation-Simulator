using Godot;

public class SubmitBox : Control
{
    private const float PITCH_VARIANCE = 0.1f;
    private const float ANIMATION_TIME = 0.2f;
    private const float H_SEP_FACTOR = 0.8f;
    private const float V_SEP_FACTOR = 4f;
    private static readonly Vector2 DEFAULT_SIZE = new Vector2(200, 75);

    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    public Thought Submitted { get; private set; } = null;
    private Thought heldThought = null;
    private AudioStreamPlayer2D audio;
    private bool readyToAccept = false;
    private bool canBeSwapped = false;
    private int startingChildCount; // Used to detect if submit box has thought parented to it
    private int boxMargins;

    /*
        GODOT PROCESSES
    */
    public override void _Ready()
    {
        boxMargins = GetParent<HBoxContainer>().GetConstant("separation") / 2;
        audio = GetNode<AudioStreamPlayer2D>("Audio");
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

        canBeSwapped = false;
        if (!MouseInRange())
        {
            if (GetOtherHoveredBox() == null) heldThought.LerpTarget = null;
            if (Submitted != null) Submitted.LerpTarget = RectGlobalPosition;
        }
        else if (Submitted == null)
        {
            heldThought.LerpTarget = RectGlobalPosition;
        }
        else // Has something submitted and mouse in range
        {
            canBeSwapped = true;
            heldThought.LerpTarget = RectGlobalPosition;
            if (heldThought.IsSubmitted)
            {
                SubmitBox target = heldThought.GetParent<SubmitBox>();
                Submitted.LerpTarget = target.RectGlobalPosition;
            }
            else Submitted.LerpTarget = MathHelper.GetPositionFromCenter(Submitted, GetViewport().GetMousePosition());
        }
    }

    /*
        PUBLIC INTERFACE FUNCTIONS
    */
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

        if (Submitted != null)
        {
            var thoughtDespawn = CreateTween();
            thoughtDespawn.TweenProperty(Submitted, PropertyNames.RectScale, Vector2.Zero, ANIMATION_TIME)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Back);
            thoughtDespawn.TweenCallback(Submitted, PropertyNames.QueueFree);
            thoughtDespawn.TweenCallback(this, PropertyNames.QueueFree);
            thoughtDespawn.Play();
        }

        despawn.Play();
    }

    public void EjectThought()
    {
        if (Submitted == null) return; // Do nothing if box empty

        var thought = Submitted;
        Submitted = null;

        NodeHelper.ReparentNode(thought);
        thought.SetAsToplevel(false);
        thought.LerpTarget = null;
        thought.SetVelocityToCenter();
        thought.RemoveRim(false);

        EmitSignal(nameof(Unsubmit));
    }

    /*
        HELPER FUNCTIONS
    */
    private bool MouseInRange()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - boxMargins * H_SEP_FACTOR,
        right = RectGlobalPosition.x + RectSize.x + boxMargins * H_SEP_FACTOR,
        up = RectGlobalPosition.y - boxMargins * V_SEP_FACTOR,
        down = RectGlobalPosition.y + RectSize.y + boxMargins * V_SEP_FACTOR;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }

    private SubmitBox GetOtherHoveredBox() // Will return null if hovered box is self
    {
        foreach(SubmitBox box in GetTree().GetNodesInGroup(GroupNames.SubmitBoxes))
        {
            if (box == this) continue;
            if (box.MouseInRange()) return box;
        }
        return null;
    }

    private void SubmitThought(Thought thought)
    {
        Submitted = thought;
        NodeHelper.ReparentNode(thought, this);
        thought.LerpTarget = RectGlobalPosition;
        NodeHelper.PlayRandomPitchAudio(audio, 1 - PITCH_VARIANCE, 1 + PITCH_VARIANCE);
        EmitSignal(nameof(Submit));
    }

    /*
        SIGNAL EVENTS
    */
    private void OnThoughtPickup(Thought thought)
    {
        heldThought = thought;
        thought.SetAsToplevel(true);
        if (thought != Submitted) return; // If held doesn't match submitted, do nothing

        Submitted = null;
        EmitSignal(nameof(Unsubmit));
    }

    private void OnThoughtReleased(Thought thought)
    {
        heldThought = null;
        if (MouseInRange())
        {
            if (canBeSwapped && thought.IsSubmitted) // Works because it only reparents AFTER releasing mouse
            {
                SubmitBox otherBox = thought.GetParent<SubmitBox>();
                Thought currentSubmitted = Submitted;

                NodeHelper.ReparentNode(Submitted, otherBox);
                otherBox.Submitted = currentSubmitted;
                Submitted.LerpTarget = otherBox.RectGlobalPosition;

                NodeHelper.ReparentNode(thought, this);
                Submitted = thought;
                thought.LerpTarget = RectGlobalPosition;
                EmitSignal(nameof(Submit));
                return;
            }
            else if (canBeSwapped) EjectThought(); // Swap with un-submitted thought
            SubmitThought(thought);
        }
        else if (GetChildCount() > startingChildCount && GetOtherHoveredBox() == null)
        {
            NodeHelper.ReparentNode(thought);
            thought.SetAsToplevel(GetOtherHoveredBox() != null);
        }
    }

    private void OnFinishedPositioning()
    {
        readyToAccept = true;
    }
}
