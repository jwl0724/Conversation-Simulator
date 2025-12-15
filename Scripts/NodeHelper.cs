using Godot;
using System;

public static class NodeHelper
{
    public static void ReparentNode(Node node, Node newParent = null) // Will parent to scene root if null
    {
        Node oldParent = node.GetParent();
        if (newParent == null) newParent = node.GetTree().CurrentScene;
        oldParent.RemoveChild(node);
        newParent.AddChild(node);
    }

    public static void PlayRandomPitchAudio(AudioStreamPlayer audio, float lower, float upper, float delay = 0)
    {
        audio.VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        if (audio.Bus.BaseName() == PropertyNames.PitchShiftBus)
        {
            AudioEffectPitchShift audioEffect = (AudioEffectPitchShift)AudioServer.GetBusEffect(AudioServer.GetBusIndex(PropertyNames.PitchShiftBus), 0);
            audioEffect.PitchScale = (float)GD.RandRange(lower, upper);
        }
        else audio.PitchScale = (float)GD.RandRange(lower, upper);
        if (delay > 0)
        {
            var soundDelay = audio.CreateTween();
            soundDelay.TweenCallback(audio, nameof(audio.Play).ToLower()).SetDelay(delay);
        }
        else audio.Play();
    }

    public static void PlayRandomPitchAudio(AudioStreamPlayer2D audio, float lower, float upper, float delay = 0)
    {
        audio.VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        if (audio.Bus.BaseName() == PropertyNames.PitchShiftBus)
        {
            AudioEffectPitchShift audioEffect = (AudioEffectPitchShift)AudioServer.GetBusEffect(AudioServer.GetBusIndex(PropertyNames.PitchShiftBus), 0);
            audioEffect.PitchScale = (float)GD.RandRange(lower, upper);
        }
        else audio.PitchScale = (float)GD.RandRange(lower, upper);
        if (delay > 0)
        {
            var soundDelay = audio.CreateTween();
            soundDelay.TweenCallback(audio, nameof(audio.Play).ToLower()).SetDelay(delay);
        }
        else audio.Play();
    }

    public static void FadeMusic(AudioStreamPlayer music, float fadeTime, bool playMusic, float delay = 0)
    {
        if (playMusic)
        {
            music.VolumeDb = Globals.MUTE_DB;
            music.Play();

            var fade = music.CreateTween();
            fade.TweenInterval(delay);
            fade.TweenProperty(music, PropertyNames.VolumeDb, MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume), fadeTime);
            fade.Play();
        }
        else
        {
            var fade = music.CreateTween();
            fade.TweenInterval(delay);
            fade.TweenProperty(music, PropertyNames.VolumeDb, Globals.MUTE_DB, fadeTime);
            fade.TweenCallback(music, nameof(music.Stop).ToLower());
            fade.Play();
        }
    }
}
