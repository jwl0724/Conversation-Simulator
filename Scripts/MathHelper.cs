using Godot;

public static class MathHelper
{
    public static float FactorToDB(float factor)
    {
        if (factor == 0) return -60; // Non-audible range dB
        if (factor > 1)
        {
            GD.PushWarning($"Volume factor should not exceed 1 ({factor} was used)");
            return 0;
        }
        return 20 * Mathf.Log(factor);
    }

    public static Vector2 GetPositionFromCenter(Control element, Vector2 centerPoint, bool centerX = true, bool centerY = true)
    {
        Vector2 position = centerPoint - element.RectSize / 2;
        if (!centerX) position.x = centerPoint.x;
        if (!centerY) position.y = centerPoint.y;
        return position;
    }
}
