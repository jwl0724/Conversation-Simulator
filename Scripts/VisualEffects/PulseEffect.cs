using Godot;
public class PulseEffect : Node
{
    [Export] private bool startOnLaunch = true;
    [Export] private float minScale = 1;
    [Export] private float maxScale = 1.1f;
    [Export] private float cycleTime = 1;

    private Node2D appliedNode;
    private SceneTreeTween pulseTween;

    public override void _Ready()
    {
        appliedNode = GetParent() as Node2D;

        pulseTween = CreateTween();
        pulseTween.TweenProperty(appliedNode, "scale", Vector2.One * maxScale, cycleTime / 2).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        pulseTween.TweenProperty(appliedNode, "scale", Vector2.One * minScale, cycleTime / 2).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        pulseTween.SetLoops();

        if (!startOnLaunch) pulseTween.Pause();
    }

    public void PlayEffect(bool play)
    {
        if (play) pulseTween.Play();
        else pulseTween.Pause();
    }
}
