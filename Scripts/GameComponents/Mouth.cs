using Godot;
using System;

public class Mouth : Control
{
    public const float SPAWN_TIME = 0.75f;
    private const float EXTRA_MARGINS = 60;

    public override void _Ready()
    {
        Visible = false;
    }

    public void PlaySpawn()
    {
        RectScale = Vector2.Zero;
        Visible = true;

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        spawn.Play();
    }

    public void PlayDespawn()
    {
        var despawn = CreateTween();
        despawn.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, SPAWN_TIME).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
        despawn.Play();
    }

    public bool MouseInRange()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - EXTRA_MARGINS,
        right = RectGlobalPosition.x + RectSize.x + EXTRA_MARGINS,
        up = RectGlobalPosition.y - EXTRA_MARGINS,
        down = RectGlobalPosition.y + RectSize.y + EXTRA_MARGINS;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }
}
