using Godot;
using System;
using System.Collections.Generic;

public class OverlapAudioPlayer : Node
{
    [Export] private AudioStream audioClip;
    private Queue<AudioStreamPlayer> playerPool = new Queue<AudioStreamPlayer>();

    public override void _Ready()
    {
        playerPool.Enqueue(CreateNewAudioPlayer());
    }

    public void Play(float lower = 1, float upper = 1)
    {
        if (playerPool.Count == 0) // No available audio players to be used
        {
            var player = CreateNewAudioPlayer();
            NodeHelper.PlayRandomPitchAudio(player, lower, upper);
        }
        else // Has an audio player that's available
        {
            var player = playerPool.Dequeue();
            NodeHelper.PlayRandomPitchAudio(player, lower, upper);
        }
    }

    private AudioStreamPlayer CreateNewAudioPlayer()
    {
        var player = new AudioStreamPlayer()
        {
            Stream = audioClip,
            VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume)
        };
        AddChild(player);
        player.Connect("finished", this, nameof(OnFinished), new Godot.Collections.Array(){player});
        return player;
    }

    private void OnFinished(AudioStreamPlayer finishedPlayer)
    {
        playerPool.Enqueue(finishedPlayer);
    }
}
