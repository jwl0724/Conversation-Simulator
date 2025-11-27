using Godot;
using System;

public class OptionsMenu : Control
{
    private enum VolumeType { MASTER, SFX, MUSIC }
    private const int SLIDER_MAX = 100;

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
        GD.Print("Opened pressed");
    }

    public void HideOptions()
    {
        GD.Print("Closed Pressed");
    }

    private void OnVolumeChange(float value, VolumeType type)
    {
        switch(type)
        {
            case VolumeType.MUSIC:
                Globals.MusicVolume = value / SLIDER_MAX;
                break;
            case VolumeType.SFX:
                Globals.SFXVolume = value / SLIDER_MAX;
                break;
            case VolumeType.MASTER:
                Globals.MasterVolume = value / SLIDER_MAX;
                break;
        }
    }

    private void OnToggle(bool pressed)
    {
        OS.WindowFullscreen = pressed;
    }
}
