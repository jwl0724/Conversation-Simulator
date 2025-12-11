using Godot;
using System;

public class Food : Button
{
    public const float SPAWN_TIME = 0.2f;
    private const float LERP_STRENGTH = 0.3f;

    [Export] private StreamTexture foodTexture;
    [Signal] public delegate void WasEaten();

    public bool IsEaten { get; private set; } = false;
    private bool isHeld = false;
    private Mouth mouth;

    public override void _Ready()
    {
        GetNode<TextureRect>("Image").Texture = foodTexture;

        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        Disabled = true;
        Visible = false;
        SetProcess(false);
    }

    public void PlaySpawn(Mouth mouth)
    {
        this.mouth = mouth;

        SetProcess(true);
        RectScale = Vector2.Zero;
        Visible = true;
        Disabled = false;

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        spawn.Play();
    }

    // TODO: Add some tilt effect when moving the food around
    public override void _PhysicsProcess(float delta)
    {
        if (!isHeld) return;
        if (mouth.MouseInRange())
        {
            RectPosition = RectPosition.LinearInterpolate(mouth.RectPosition, LERP_STRENGTH);
        }
        else
        {
            RectPosition = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), LERP_STRENGTH);
        }
    }

    private void PlayDespawn()
    {
        Disabled = true;

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
