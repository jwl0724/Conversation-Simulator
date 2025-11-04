using Godot;

public class SubmissionBox : Control
{
    [Signal] public delegate void Submit();
    [Signal] public delegate void Unsubmit();

    private float left;
    private float right;
    private float up;
    private float down;

    public Thought Submitted { get; private set; } = null;
    private Thought hovered = null;

    public override void _Ready()
    {
        CallDeferred(nameof(SetBounds));

        Area2D submitArea = GetNode<Area2D>("Area");
        submitArea.Connect(SignalNames.AreaEntered, this, nameof(OnAreaEntered));
        submitArea.Connect(SignalNames.AreaExited, this, nameof(OnAreaExited));
    }

    public override void _PhysicsProcess(float delta) // Needed cause button blocks MouseEnter/Exit
    {
        if (hovered == null || Submitted != null) return;

        Vector2 mousePos = GetViewport().GetMousePosition();
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

    private void SetBounds()
    {
        left = RectGlobalPosition.x;
        right = RectGlobalPosition.x + RectSize.x;
        up = RectGlobalPosition.y;
        down = RectGlobalPosition.y + RectSize.y;
    }
}
