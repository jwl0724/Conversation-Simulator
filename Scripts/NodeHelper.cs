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

    public static void PlayRandomPitchAudio(AudioStreamPlayer2D audio, float lower, float upper, float delay = 0)
    {
        audio.VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        audio.PitchScale = (float)GD.RandRange(lower, upper);
        if (delay > 0)
        {
            var soundDelay = audio.CreateTween();
            soundDelay.TweenCallback(audio, nameof(audio.Play).ToLower()).SetDelay(delay);
        }
        else audio.Play();
    }
}
