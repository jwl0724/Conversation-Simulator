using Godot;

public class ThoughtBox : Control
{
    public static Vector2 Center { get; private set; }

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

            if (thought.IsReturning)
            {
                // TODO: Determine if the below is necessary
                
                // bool inBoundsX = left <= thought.RectGlobalPosition.x && thought.RectGlobalPosition.x <= right;
                // bool inBoundsY = up <= thought.RectGlobalPosition.y && thought.RectGlobalPosition.y <= down;

                // thought.IsReturning = !inBoundsX || !inBoundsY;
            }
            else
            {
                bool flipX = thought.RectGlobalPosition.x < left || thought.RectGlobalPosition.x + thought.RectSize.x > right;
                bool flipY = thought.RectGlobalPosition.y < up || thought.RectGlobalPosition.y + thought.RectSize.y > down;
                thought.IsInBounds = !(flipX || flipY);
                if (!thought.IsHeld && !thought.IsInBounds) thought.Rebound(flipX, flipY);
            }

        }
    }

    private void SetBounds()
    {
        left = RectGlobalPosition.x;
        right = RectGlobalPosition.x + RectSize.x;
        up = RectGlobalPosition.y;
        down = RectGlobalPosition.y + RectSize.y;

        Center = new Vector2(left + RectSize.x / 2, up + RectSize.y / 2);
    }

    private void OnSceneReady()
    {
        CallDeferred("set_physics_process", true);
    }
}
