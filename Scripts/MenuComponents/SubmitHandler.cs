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

        for(int i = 0; i < expectedAnswer.Length; i++)
        {
            var box = SpawnBox();

            // TODO: Fix this pop-up animation just now working
            // var spawnAnimation = CreateTween();

            // spawnAnimation.TweenProperty(box, PropertyNames.RectScale, Vector2.Zero, 0);
            // spawnAnimation.TweenProperty(box, nameof(Visible).ToLower(), true, 0f);
            // spawnAnimation.TweenProperty(box, PropertyNames.RectScale, Vector2.One, 0.75f)
            //     .SetEase(Tween.EaseType.Out)
            //     .SetTrans(Tween.TransitionType.Back)
            //     .SetDelay(SPAWN_INTERVAL * i);

            // spawnAnimation.Play();
        }
    }

    private SubmissionBox SpawnBox()
    {
        SubmissionBox box = submitBoxTemplate.Instance<SubmissionBox>();
        submitBoxes.Add(box);
        AddChild(box);
        return box;
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
