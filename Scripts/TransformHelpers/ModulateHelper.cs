using Godot;

public class ModulateHelper : Node
{
    [Signal] public delegate void ModulateComplete();

    public const float DEFAULT_TRANSITION_TIME = 0.2f;
    public static readonly Color DEFAULT_COLOR = Colors.White;
    private Control scaledNode;

    public override void _Ready()
    {
        scaledNode = Owner as Control;
    }

    public void ModulateToColor
    (
        Color color,
        float transitionTime = DEFAULT_TRANSITION_TIME,
        Tween.EaseType ease = Tween.EaseType.InOut,
        Tween.TransitionType transition = Tween.TransitionType.Linear
    )
    {
        var modulateTween = CreateTween();
        modulateTween.SetTrans(transition).SetEase(ease);
        modulateTween.TweenProperty(scaledNode, nameof(scaledNode.Modulate).ToLower(), color, transitionTime);
        modulateTween.TweenCallback(this, nameof(OnComplete));
        modulateTween.Play();
    }

    public void ModulateToDefault
    (
        float transitionTime = DEFAULT_TRANSITION_TIME,
        Tween.EaseType ease = Tween.EaseType.InOut,
        Tween.TransitionType transition = Tween.TransitionType.Linear
    )
    {
        var modulateTween = CreateTween();
        modulateTween.SetTrans(transition).SetEase(ease);
        modulateTween.TweenProperty(scaledNode, nameof(scaledNode.Modulate).ToLower(), Colors.White, transitionTime);
        modulateTween.TweenCallback(this, nameof(OnComplete));
        modulateTween.Play();
    }

    private void OnComplete()
    {
        EmitSignal(nameof(ModulateComplete));
    }
}
