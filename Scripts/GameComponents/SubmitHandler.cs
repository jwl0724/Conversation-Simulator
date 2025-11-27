using Godot;
using System.Collections.Generic;
using System.Linq;

public class SubmitHandler : HBoxContainer
{
    private const float SPAWN_INTERVAL = 0.15f;
    private const float DESPAWN_INTERVAL = 0.075f;

    [Signal] public delegate void CorrectSubmission();
    [Signal] public delegate void WrongSubmission();
    [Export] private PackedScene submitBoxTemplate;

    private readonly List<SubmissionBox> submitBoxes = new List<SubmissionBox>();
    private string[] expectedAnswer;
    private int totalSubmitted = 0;

    public void SpawnSubmitBoxes(string response)
    {
        totalSubmitted = 0;
        submitBoxes.Clear();
        expectedAnswer = response.Split(" ");

        var spawning = CreateTween();
        for(int i = 0; i < expectedAnswer.Length; i++)
        {
            spawning.TweenCallback(this, nameof(SpawnBox)).SetDelay(SPAWN_INTERVAL);
        }
        spawning.Play();
    }

    public void DespawnSubmitBoxes()
    {
        foreach(var box in submitBoxes)
        {
            box.PlayDespawn();
        }
    }

    private void SpawnBox()
    {
        SubmissionBox box = submitBoxTemplate.Instance<SubmissionBox>();
        submitBoxes.Add(box);
        AddChild(box);

        box.Connect(nameof(SubmissionBox.Submit), this, nameof(OnSubmitReceived));
        box.Connect(nameof(SubmissionBox.Unsubmit), this, nameof(OnUnsubmitReceived));
    }

    private void OnSubmitReceived()
    {
        totalSubmitted++;
        if (totalSubmitted >= expectedAnswer.Length) ValidateSubmission();
    }

    private void OnUnsubmitReceived()
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
