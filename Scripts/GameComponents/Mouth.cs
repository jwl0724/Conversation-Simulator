using Godot;
using System;

public class Mouth : Control
{
    public const float SPAWN_TIME = 0.75f;
    private const float EXTRA_MARGINS = 60;
    private AudioStreamPlayer2D audio;
    private float baseVolume;

    public override void _Ready()
    {
        Visible = false;

        audio = GetNode<AudioStreamPlayer2D>("Audio");
        baseVolume = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        audio.VolumeDb = baseVolume;
        audio.Stop();
    }

    public void PlaySpawn()
    {
        RectScale = Vector2.Zero;
        Visible = true;
        audio.VolumeDb = Globals.MUTE_DB;
        audio.Play();

        var spawn = CreateTween();
        spawn.TweenProperty(this, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        spawn.Parallel().TweenProperty(audio, PropertyNames.VolumeDb, baseVolume, SPAWN_TIME);
        spawn.Play();
    }

    public void PlayDespawn()
    {
        var despawn = CreateTween();
        despawn.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, SPAWN_TIME).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
        despawn.Parallel().TweenProperty(audio, PropertyNames.VolumeDb, Globals.MUTE_DB, SPAWN_TIME);
        despawn.TweenCallback(audio, nameof(audio.Stop).ToLower());
        despawn.Play();
    }

    public bool MouseInRange()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        float left = RectGlobalPosition.x - EXTRA_MARGINS,
        right = RectGlobalPosition.x + RectSize.x + EXTRA_MARGINS,
        up = RectGlobalPosition.y - EXTRA_MARGINS,
        down = RectGlobalPosition.y + RectSize.y + EXTRA_MARGINS;
        return mousePos.x > left && mousePos.x < right && mousePos.y > up && mousePos.y < down;
    }
}
