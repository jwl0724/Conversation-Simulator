using Godot;

public class SubmissionBox : Control
{
    private const float SPAWN_ANIMATION_TIME = 0.2f;
    private static readonly Vector2 DEFAULT_SIZE = new Vector2(200, 75);

    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    public Thought Submitted { get; private set; } = null;
    private Thought hovered = null;

    public override void _Ready()
    {
        Area2D submitArea = GetNode<Area2D>("Area");
        submitArea.Connect(SignalNames.AreaEntered, this, nameof(OnAreaEntered));
        submitArea.Connect(SignalNames.AreaExited, this, nameof(OnAreaExited));

        RectMinSize = Vector2.Zero;
        Modulate = Colors.Transparent;

        var tween = CreateTween();
        tween.TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, SPAWN_ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(this, PropertyNames.RectMinSize, DEFAULT_SIZE, SPAWN_ANIMATION_TIME)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        tween.Play();
    }

    public override void _PhysicsProcess(float delta) // Needed cause button blocks MouseEnter/Exit
    {
        if (hovered == null || Submitted != null) return;

        Vector2 mousePos = GetViewport().GetMousePosition();

        // TODO: Maybe can move this later if necessary?
        float left = RectGlobalPosition.x, right = RectGlobalPosition.x + RectSize.x, up = RectGlobalPosition.y, down = RectGlobalPosition.y + RectSize.y;
        if (mousePos.x < left || mousePos.x > right || mousePos.y < up || mousePos.y > down)
        {
            hovered.SubmitTarget = null;
        }
        else
        {
            hovered.SubmitTarget = this;
        }
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
