using Godot;
using System;

public class Mouth : Control
{
    private const float SPAWN_TIME = 0.2f;
    private const float EXTRA_MARGINS = 20;

    public override void _Ready()
    {
        Visible = false;
        SetProcess(false);
    }

    public void PlaySpawn()
    {
        RectScale = Vector2.Zero;
        Visible = true;
        SetProcess(true);

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME);
        spawn.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        // TODO: Find a way to make the food lerp to the center of this thing
    }

    private bool MouseInRange()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - EXTRA_MARGINS,
        right = RectGlobalPosition.x + RectSize.x + EXTRA_MARGINS,
        up = RectGlobalPosition.y - EXTRA_MARGINS,
        down = RectGlobalPosition.y + RectSize.y + EXTRA_MARGINS;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }
}
