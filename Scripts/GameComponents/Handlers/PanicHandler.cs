using Godot;

public class PanicHandler : ColorRect
{
    private const string wobble = "panic_strength";
    private const string blur = "blur_amount";
    private const string speed = "time_scale";

    private const float maxWobbleValue = 0.4f;
    private const float maxBlurStrength = 1f;
    private const float maxEffectSpeed = 2;

    private static readonly float wobbleStartTime = Globals.TIME_LIMIT / 2;
    private static readonly float blurStartTime = Globals.TIME_LIMIT / 4;

    private ShaderMaterial panicShader;
    private AudioStreamPlayer sfx;
    private TimerBar timerBar;
    private bool isPanicking = false;

    public override void _Ready()
    {
        panicShader = Material as ShaderMaterial;
        sfx = GetNode<AudioStreamPlayer>("SFX");

        panicShader.SetShaderParam(wobble, 0);
        panicShader.SetShaderParam(blur, 0);
        panicShader.SetShaderParam(speed, 0);
    }

    public override void _Process(float delta)
    {
        if (!isPanicking || timerBar.IsStopped) return;

        // TODO: Heartbeat SFX

        // Wobble effect
        float wobbleRatio = 1 - timerBar.TimeLeft / wobbleStartTime;
        panicShader.SetShaderParam(wobble, maxWobbleValue * wobbleRatio);
        panicShader.SetShaderParam(speed, maxEffectSpeed * wobbleRatio);

        // Blur effect
        if (timerBar.TimeLeft > blurStartTime) return;
        float blurRatio = 1 - timerBar.TimeLeft / blurStartTime;
        panicShader.SetShaderParam(blur, maxBlurStrength * blurRatio);

        // TODO: tinnitus sound?
    }

    public void StartPanic(TimerBar timer)
    {
        timerBar = timer;
        isPanicking = true;
    }

    public void StopPanic()
    {
        isPanicking = false;
        panicShader.SetShaderParam(wobble, 0);
        panicShader.SetShaderParam(speed, 0);
        panicShader.SetShaderParam(blur, 0);
    }
}
