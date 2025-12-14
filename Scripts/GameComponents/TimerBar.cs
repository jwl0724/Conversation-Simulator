using Godot;

public class TimerBar : Control
{
    private const float maxPulseFactor = 0.015f;
    private const float maxPulseSpeed = 20;
    private const float pulseCurveAmount = 1.5f;
    private const float colorTransTime = 0.1f;
    private readonly float vfxStartTime = Globals.TIME_LIMIT / 2;
    [Export] private Color flashColor;

    private Tween tween;
    private Timer timer;
    private ProgressBar bar;
    private Node2D barWrapper; // Needed cause animation stuttery without it

    private float theta = 0;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Timer");
        tween = GetNode<Tween>("Tween");
        barWrapper = GetNode<Node2D>("Node2D");
        bar = barWrapper.GetNode<ProgressBar>("ProgressBar");

        timer.WaitTime = Globals.TIME_LIMIT;
        bar.MaxValue = Globals.TIME_LIMIT;
        bar.Value = Globals.TIME_LIMIT;
    }

    public void ConnectToTimeout(Node node, string methodName)
    {
        timer.Connect(SignalNames.Timeout, node, methodName);
    }

    public void ConnectTimerToSignal(Node connectingTo, string signalName, string timerMethod)
    {
        connectingTo.Connect(signalName, timer, timerMethod);
    }

    public void Start()
    {
        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();

        // TODO: Change the color to the default color it isn't due to flashing effect
        theta = 0;
        barWrapper.Scale = Vector2.One;
    }

    public override void _Process(float delta)
    {
        if (timer.IsStopped() || timer.Paused) return;
        bar.Value = timer.TimeLeft;

        // TODO: Add flashing red effect when running low on time and pulse effect?
        if (timer.TimeLeft > vfxStartTime) return;

        float timeLeftRatio = (vfxStartTime - timer.TimeLeft) / vfxStartTime;

        // Pulse effect
        barWrapper.Scale = Vector2.One * maxPulseFactor * Mathf.Sin(theta) + Vector2.One;
        theta += delta * maxPulseSpeed * Mathf.Ease(timeLeftRatio, pulseCurveAmount);
        if (theta > Mathf.Pi * 2) theta = 0;
    }
}
