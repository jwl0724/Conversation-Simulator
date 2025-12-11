using Godot;
using System;

public class Food : Button
{
    [Export] private StreamTexture foodTexture;
    private const float LERP_STRENGTH = 0.2f;
    private const float SPAWN_TIME = 0.2f;
    private bool isHeld = false;

    public override void _Ready()
    {
        GetNode<TextureRect>("Image").Texture = foodTexture;

        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        Disabled = true;
        Visible = false;
        SetProcess(false);
    }

    public void PlaySpawn()
    {
        SetProcess(true);
        RectScale = Vector2.Zero;
        Visible = true;
        Disabled = false;

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        spawn.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!isHeld) return;
        RectPosition = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), LERP_STRENGTH);
    }

    private void OnButtonUp()
    {
        isHeld = false;
    }

    private void OnButtonDown()
    {
        isHeld = true;
    }
}
