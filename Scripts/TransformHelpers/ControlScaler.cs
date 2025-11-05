using Godot;

public class ControlScaler : Node
{
    [Signal] public delegate void ScaleComplete();

    public const float DEFAULT_TRANSITION_TIME = 0.1f;
    public const float DEFAULT_TARGET_AMOUNT = 1.05f;
    private Control ScaledNode;

    public override void _Ready()
    {
        ScaledNode = Owner as Control;
    }

    public void Scale(
        float scale = DEFAULT_TARGET_AMOUNT,
        float transitionTime = DEFAULT_TRANSITION_TIME,
        Tween.EaseType ease = Tween.EaseType.InOut,
        Tween.TransitionType transition = Tween.TransitionType.Linear
    )
    {
        var sizeTween = CreateTween();
        sizeTween.SetTrans(transition).SetEase(ease);
        sizeTween.TweenProperty(ScaledNode, PropertyNames.RectScale, Vector2.One * scale, transitionTime);
        sizeTween.TweenCallback(this, nameof(OnComplete));
        sizeTween.Play();
    }

    public void ScaleToDefault(
        float transitionTime = DEFAULT_TRANSITION_TIME,
        Tween.EaseType ease = Tween.EaseType.InOut,
        Tween.TransitionType transition = Tween.TransitionType.Linear
    )
    {
        var sizeTween = CreateTween();
        sizeTween.SetTrans(transition).SetEase(ease);
        sizeTween.TweenProperty(ScaledNode, PropertyNames.RectScale, Vector2.One, transitionTime);
        sizeTween.TweenCallback(this, nameof(OnComplete));
        sizeTween.Play();
    }

    private void OnComplete()
    {
        EmitSignal(nameof(ScaleComplete));
    }
}
