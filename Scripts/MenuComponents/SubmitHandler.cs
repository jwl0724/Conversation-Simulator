using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class SubmitHandler : HBoxContainer
{
    private const float SPAWN_INTERVAL = 0.15f;

    [Signal] public delegate void CorrectSubmission();
    [Signal] public delegate void WrongSubmission();
    [Export] private PackedScene submitBoxTemplate;

    private readonly List<SubmissionBox> submitBoxes = new List<SubmissionBox>();
    private string[] expectedAnswer;
    private int totalSubmitted = 0;

    public override void _Ready()
    {

    }

    public void SpawnSubmitBoxes(string response)
    {
        totalSubmitted = 0;
        expectedAnswer = response.Split(" ");

        var spawning = CreateTween();
        for(int i = 0; i < expectedAnswer.Length; i++)
        {
            spawning.TweenCallback(this, nameof(SpawnBox)).SetDelay(SPAWN_INTERVAL * i);
        }
        spawning.Play();
    }

    private void SpawnBox()
    {
        SubmissionBox box = submitBoxTemplate.Instance<SubmissionBox>();
        submitBoxes.Add(box);
        AddChild(box);
    }

    private void OnSubmit()
    {
        totalSubmitted++;
        if (totalSubmitted >= expectedAnswer.Length) ValidateSubmission();
    }

    private void OnUnsubmit()
    {
        totalSubmitted--;
    }

    private void ValidateSubmission()
    {
        for(int i = 0; i < expectedAnswer.Length; i++)
        {
            if(expectedAnswer[i].Contains("/")) // Handle submission where multiple words work
            {
                var validAnswers = expectedAnswer[i].Split("/");
                if (!validAnswers.Contains(submitBoxes[i].Submitted.Word))
                {
                    EmitSignal(nameof(WrongSubmission));
                    return;
                }
            }
            else
            {
                if (submitBoxes[i].Submitted.Word != expectedAnswer[i])
                {
                    EmitSignal(nameof(WrongSubmission));
                    return;
                }
            }
        }
        EmitSignal(nameof(CorrectSubmission));
    }
}
