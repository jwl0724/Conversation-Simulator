using Godot;

public class ThoughtBox : Control
{
    private float left;
    private float right;
    private float up;
    private float down;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        CallDeferred(nameof(SetBounds));
        GetTree().CurrentScene.Connect(SignalNames.Ready, this, nameof(OnSceneReady), flags: (uint)ConnectFlags.Oneshot);
    }

    public override void _PhysicsProcess(float delta)
    {
        var thoughts = GetTree().GetNodesInGroup(GroupNames.Thoughts);
        foreach(Thought thought in thoughts)
        {
            if (thought.IsSubmitted) continue;

            bool flipX = thought.RectGlobalPosition.x < left || thought.RectGlobalPosition.x + thought.RectSize.x > right;
            bool flipY = thought.RectGlobalPosition.y < up || thought.RectGlobalPosition.y + thought.RectSize.y > down;
            thought.IsInBounds = !(flipX || flipY);
            if (!thought.IsHeld && !thought.IsInBounds) thought.Rebound(flipX, flipY);
        }
    }

    private void SetBounds()
    {
        left = RectGlobalPosition.x;
        right = RectGlobalPosition.x + RectSize.x;
        up = RectGlobalPosition.y;
        down = RectGlobalPosition.y + RectSize.y;
    }

    private void OnSceneReady()
    {
        CallDeferred("set_physics_process", true);
    }
}
