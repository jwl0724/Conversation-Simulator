using Godot;

public class ControlScaler : Node
{
    public const float DEFAULT_TRANSITION_TIME = 0.1f;
    public const float DEFAULT_TARGET_AMOUNT = 1.05f;
    private Control ScaledNode;

    public override void _Ready()
    {
        ScaledNode = Owner as Control;
    }


    public void Scale(float scale = DEFAULT_TARGET_AMOUNT, float transitionTime = DEFAULT_TRANSITION_TIME)
    {
        var sizeTween = CreateTween();
        sizeTween.TweenProperty(ScaledNode, "rect_scale", Vector2.One * scale, transitionTime);
        sizeTween.Play();
    }

    public void ScaleToDefault(float transitionTime = DEFAULT_TRANSITION_TIME)
    {
        var sizeTween = CreateTween();
        sizeTween.TweenProperty(ScaledNode, "rect_scale", Vector2.One, transitionTime);
        sizeTween.Play();
    }
}
