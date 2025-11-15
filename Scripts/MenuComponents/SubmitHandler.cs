using Godot;
using System;
using System.Collections.Generic;

public class SubmitHandler : Control
{
    private const float SPAWN_INTERVAL = 0.15f;

    [Signal] public delegate void FinishedSpawning();
    [Export] private PackedScene submitBoxTemplate;

    private readonly List<SubmissionBox> submitBoxes = new List<SubmissionBox>();
    private string answerSentence;

    public override void _Ready()
    {

    }

    public void SpawnSubmitBoxes(string response)
    {
        answerSentence = response;

        var spawning = CreateTween();
        var answers = response.Split(" ");
        foreach(string word in answers)
        {
            spawning.TweenCallback(this, nameof(SpawnBox), new Godot.Collections.Array(){word}).SetDelay(SPAWN_INTERVAL);
        }
    }

    private void SpawnBox(string word)
    {

    }
}
