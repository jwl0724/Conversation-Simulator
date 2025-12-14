using Godot;

public class PanicHandler : ColorRect
{
    private const string wobble = "panic_strength";
    private const string blur = "blur_amount";
    private const string speed = "time_scale";

    private const float maxWobbleValue = 0.4f;
    private const float maxBlurStrength = 1f;
    private const float maxEffectSpeed = 2;
    private const float minHeartbeatInterval = 0.5f;
    private const float heartbeatPitchVariance = 0.1f;
    private const float tinnitusVolumeReduction = -10;

    private static readonly float wobbleStartTime = Globals.TIME_LIMIT / 2;
    private static readonly float blurStartTime = Globals.TIME_LIMIT / 4;

    private ShaderMaterial panicShader;
    private AudioStreamPlayer heartbeat;
    private AudioStreamPlayer tinnitus;
    private TimerBar timerBar;

    private bool isPanicking = false;
    private float tinnitusMaxVolume;
    private float heartbeatElapsedTime = 0;

    public override void _Ready()
    {
        panicShader = Material as ShaderMaterial;
        heartbeat = GetNode<AudioStreamPlayer>("Heartbeat");
        tinnitus = GetNode<AudioStreamPlayer>("Tinnitus");

        panicShader.SetShaderParam(wobble, 0);
        panicShader.SetShaderParam(blur, 0);
        panicShader.SetShaderParam(speed, 0);

        tinnitusMaxVolume = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume) + tinnitusVolumeReduction;
        tinnitus.VolumeDb = Globals.MUTE_DB;
    }

    public override void _Process(float delta)
    {
        if (!isPanicking || timerBar.IsStopped) return;

        // Heartbeat SFX
        float timeUsedRatio = 1 - timerBar.TimeLeft / wobbleStartTime;
        heartbeatElapsedTime += delta * timeUsedRatio * 1.5f;
        if (heartbeatElapsedTime > minHeartbeatInterval)
        {
            heartbeatElapsedTime = 0;
            NodeHelper.PlayRandomPitchAudio(heartbeat, 1 - heartbeatPitchVariance, 1 + heartbeatPitchVariance);
        }

        // Wobble effect
        panicShader.SetShaderParam(wobble, maxWobbleValue * timeUsedRatio);
        panicShader.SetShaderParam(speed, maxEffectSpeed * timeUsedRatio);

        // Blur effect
        if (timerBar.TimeLeft > blurStartTime) return;
        float blurTimeUsedRatio = 1 - timerBar.TimeLeft / blurStartTime;
        panicShader.SetShaderParam(blur, maxBlurStrength * blurTimeUsedRatio);

        // Tinnitus sound
        float newDb = Mathf.Min(tinnitus.VolumeDb + delta * Mathf.Abs(Globals.MUTE_DB - tinnitusMaxVolume) / blurStartTime * 1.25f, tinnitusMaxVolume);
        tinnitus.VolumeDb = newDb;
    }

    public void StartPanic(TimerBar timer)
    {
        timerBar = timer;
        isPanicking = true;
        tinnitus.Play();
    }

    public void StopPanic()
    {
        isPanicking = false;
        panicShader.SetShaderParam(wobble, 0);
        panicShader.SetShaderParam(speed, 0);
        panicShader.SetShaderParam(blur, 0);
        tinnitus.Stop();
    }
}
