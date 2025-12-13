using Godot;
using System;

public class Credits : Control
{
    private const float ANIMATION_TIME = 0.5f;

    [Signal] public delegate void CreditsClosed();
    [Export] private NodePath closeButtonPath;

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>(closeButtonPath).Connect(SignalNames.Pressed, this, nameof(HideCredits));
    }

    public void ShowCredits()
    {
        RectScale = Vector2.Zero;
        Visible = true;

        var animation = CreateTween();
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.One * 1.15f, ANIMATION_TIME / 2)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        animation.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, ANIMATION_TIME / 2);
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.One, ANIMATION_TIME / 2)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        animation.Play();
    }

    public void HideCredits()
    {
        var animation = CreateTween();
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        animation.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.Transparent, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Expo);
        animation.TweenCallback(this, nameof(OnFinishHide));
        animation.Play();
    }

    private void OnFinishHide()
    {
        RectScale = Vector2.Zero;
        Visible = false;
        EmitSignal(nameof(CreditsClosed));
    }
}
