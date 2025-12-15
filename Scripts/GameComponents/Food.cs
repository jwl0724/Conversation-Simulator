using Godot;
using System;

public class Food : Button
{
    public const float SPAWN_TIME = 0.2f;
    private const float LERP_STRENGTH = 0.3f;
    private const float MAX_TILT = 30;
    private const float BASE_PITCH = 0.9f;
    private const float PITCH_VARIANCE = 0.2f;

    [Export] private StreamTexture foodTexture;
    [Export] private AudioStreamMP3 spawnSFX;
    [Export] private AudioStreamMP3 despawnSFX;

    [Signal] public delegate void WasEaten();

    public bool IsEaten { get; private set; } = false;
    private bool isHeld = false;
    private AudioStreamPlayer2D audio;
    private Mouth mouth;

    public override void _Ready()
    {
        GetNode<TextureRect>("Image").Texture = foodTexture;
        audio = GetNode<AudioStreamPlayer2D>("Audio");

        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        Disabled = true;
        Visible = false;
        audio.Stop();
        SetProcess(false);
    }

    public void PlaySpawn(Mouth mouth)
    {
        this.mouth = mouth;

        SetProcess(true);
        RectScale = Vector2.Zero;
        Visible = true;
        Disabled = false;

        audio.Stream = spawnSFX;
        NodeHelper.PlayRandomPitchAudio(audio, BASE_PITCH - PITCH_VARIANCE, BASE_PITCH + PITCH_VARIANCE);

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        spawn.Play();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (RectRotation != 0 && !isHeld) RectRotation = Mathf.MoveToward(RectRotation, 0, MAX_TILT * LERP_STRENGTH);
        if (!isHeld) return;
        if (mouth.MouseInRange())
        {
            RectPosition = RectPosition.LinearInterpolate(mouth.RectPosition, LERP_STRENGTH);
            RectRotation += MAX_TILT * delta;
        }
        else
        {
            Vector2 convertedMousePos = MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition());
            RectPosition = RectPosition.LinearInterpolate(convertedMousePos, LERP_STRENGTH);

            if (RectRotation > MAX_TILT)
            {
                RectRotation = Mathf.MoveToward(RectRotation, 0, MAX_TILT * LERP_STRENGTH);
            }
            else
            {
                float diff = Mathf.Abs(RectPosition.x - convertedMousePos.x) / 8;
                RectRotation = RectPosition.x > convertedMousePos.x ? -diff : diff;
            }
        }
    }

    private void PlayDespawn()
    {
        Disabled = true;
        audio.Stream = despawnSFX;
        NodeHelper.PlayRandomPitchAudio(audio, 1 - PITCH_VARIANCE, 1 + PITCH_VARIANCE);

        var despawn = CreateTween();
        despawn.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, SPAWN_TIME * 2);
        despawn.Parallel().TweenProperty(this, PropertyNames.RectPosition, mouth.RectPosition, SPAWN_TIME);
        despawn.Parallel().TweenProperty(this, PropertyNames.RectRotation, 360, SPAWN_TIME * 2);
        despawn.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.Transparent, SPAWN_TIME * 2).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo);
        despawn.TweenCallback(this, nameof(OnDespawnFinished));
        despawn.Play();
    }

    private void OnButtonUp()
    {
        isHeld = false;

        if (!mouth.MouseInRange()) return;
        PlayDespawn();
    }

    private void OnButtonDown()
    {
        isHeld = true;
    }

    private void OnDespawnFinished()
    {
        IsEaten = true;
        EmitSignal(nameof(WasEaten));
    }
}
