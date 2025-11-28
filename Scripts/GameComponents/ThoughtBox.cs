using Godot;

public class ThoughtBox : Control
{
    public static Vector2 Center { get; private set; }

    private static float left;
    private static float right;
    private static float up;
    private static float down;
    private static bool boundsSet = false;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        CallDeferred(nameof(SetBounds));
        GetTree().CurrentScene.Connect(SignalNames.Ready, this, nameof(OnSceneReady), flags: (uint)ConnectFlags.Oneshot);
    }

    public static bool IsInBounds(Thought thought)
    {
        if (!boundsSet) return true; // Assumes all boxes will be spawning near center anyways
        bool inBoundsX = left <= thought.RectGlobalPosition.x && thought.RectGlobalPosition.x + thought.RectSize.x <= right;
        bool inBoundsY = up <= thought.RectGlobalPosition.y && thought.RectGlobalPosition.y  + thought.RectSize.y <= down;
        return inBoundsX && inBoundsY;
    }

    public static bool IsMovingAway(Thought thought, bool checkingX)
    {
        if (checkingX)
        {
            return (thought.RectGlobalPosition.x < left && thought.Velocity.x < 0) ||
                (thought.RectGlobalPosition.x + thought.RectSize.x > right && thought.Velocity.x > 0);
        }
        else
        {
            return (thought.RectGlobalPosition.y < up && thought.Velocity.y < 0) ||
                (thought.RectGlobalPosition.y + thought.RectSize.y > down && thought.Velocity.y > 0);
        }
    }

    private void SetBounds()
    {
        left = RectGlobalPosition.x;
        right = RectGlobalPosition.x + RectSize.x;
        up = RectGlobalPosition.y;
        down = RectGlobalPosition.y + RectSize.y;

        Center = new Vector2(left + RectSize.x / 2, up + RectSize.y / 2);
        boundsSet = true;
    }

    private void OnSceneReady()
    {
        CallDeferred(PropertyNames.SetPhysicsProcess, true);
    }
}
