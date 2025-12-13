using Godot;
using System;

public class OptionsMenu : Control
{
    private enum VolumeType { MASTER, SFX, MUSIC }
    private const int SLIDER_MAX = 100;
    private const float ANIMATION_TIME = 0.5f;

    [Signal] public delegate void OptionsClosed();
    [Signal] public delegate void MusicVolumeChanged(); // Don't need to signal SFX since volume is set on call

    [Export] private NodePath musicSliderPath;
    [Export] private NodePath sfxSliderPath;
    [Export] private NodePath masterSliderPath;
    [Export] private NodePath fullscreenPath;
    [Export] private NodePath closeButtonPath;

    private HSlider musicVolumeSlider;
    private HSlider sfxVolumeSlider;
    private HSlider masterVolumeSlider;
    private CheckBox fullscreenToggle;
    private Button closeButton;

    public override void _Ready()
    {
        Visible = false;
        
        musicVolumeSlider = GetNode<HSlider>(musicSliderPath);
        sfxVolumeSlider = GetNode<HSlider>(sfxSliderPath);
        masterVolumeSlider = GetNode<HSlider>(masterSliderPath);
        fullscreenToggle = GetNode<CheckBox>(fullscreenPath);
        closeButton = GetNode<Button>(closeButtonPath);

        musicVolumeSlider.Value = Mathf.RoundToInt(Globals.MusicVolume * SLIDER_MAX);
        sfxVolumeSlider.Value = Mathf.RoundToInt(Globals.SFXVolume * SLIDER_MAX);
        masterVolumeSlider.Value = Mathf.RoundToInt(Globals.MasterVolume * SLIDER_MAX);
        fullscreenToggle.Pressed = OS.WindowFullscreen;

        musicVolumeSlider.Connect(SignalNames.ValueChanged, this, nameof(OnVolumeChange), new Godot.Collections.Array(){VolumeType.MUSIC});
        sfxVolumeSlider.Connect(SignalNames.ValueChanged, this, nameof(OnVolumeChange), new Godot.Collections.Array(){VolumeType.SFX});
        masterVolumeSlider.Connect(SignalNames.ValueChanged, this, nameof(OnVolumeChange), new Godot.Collections.Array(){VolumeType.MASTER});
        closeButton.Connect(SignalNames.Pressed, this, nameof(HideOptions));
        fullscreenToggle.Connect(SignalNames.Toggled, this, nameof(OnToggle));
    }

    public void ShowOptions()
    {
        RectScale = Vector2.Zero;
        Visible = true;

        var animation = CreateTween();
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.One * 1.15f, ANIMATION_TIME / 2)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        animation.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, ANIMATION_TIME / 2);
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.One, ANIMATION_TIME / 2)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        animation.Play();
    }

    public void HideOptions()
    {
        var animation = CreateTween();
        animation.TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        animation.Parallel().TweenProperty(this, nameof(Modulate).ToLower(), Colors.Transparent, ANIMATION_TIME)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Expo);
        animation.TweenCallback(this, nameof(ResetVisuals));
        animation.Play();
    }

    private void ResetVisuals()
    {
        RectScale = Vector2.Zero;
        Visible = false;
        EmitSignal(nameof(OptionsClosed));
    }

    private void OnVolumeChange(float value, VolumeType type)
    {
        switch(type)
        {
            case VolumeType.MUSIC:
                Globals.MusicVolume = value / SLIDER_MAX;
                EmitSignal(nameof(MusicVolumeChanged));
                break;
            case VolumeType.SFX:
                Globals.SFXVolume = value / SLIDER_MAX;
                break;
            case VolumeType.MASTER:
                Globals.MasterVolume = value / SLIDER_MAX;
                EmitSignal(nameof(MusicVolumeChanged));
                break;
        }
    }

    private void OnToggle(bool pressed)
    {
        OS.WindowFullscreen = pressed;
    }
}
