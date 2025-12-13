using Godot;
using System.Collections.Generic;
using System.Linq;

public class SubmitHandler : HBoxContainer
{
    private const float SPAWN_INTERVAL = 0.125f;

    [Signal] public delegate void CorrectSubmission();
    [Signal] public delegate void WrongSubmission();
    [Export] private PackedScene submitBoxTemplate;

    public string LastSubmitSentence { get; private set; } = "";
    private readonly List<SubmitBox> submitBoxes = new List<SubmitBox>();
    private string[] expectedAnswer;
    private int totalSubmitted = 0;

    public void SpawnSubmitBoxes(string response)
    {
        totalSubmitted = 0;
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
        submitBoxes.Clear();
    }

    private void SpawnBox()
    {
        SubmitBox box = submitBoxTemplate.Instance<SubmitBox>();
        submitBoxes.Add(box);
        AddChild(box);

        box.Connect(nameof(SubmitBox.Submit), this, nameof(OnSubmitReceived));
        box.Connect(nameof(SubmitBox.Unsubmit), this, nameof(OnUnsubmitReceived));
    }

    private void OnSubmitReceived()
    {
        totalSubmitted++;
        if (totalSubmitted >= expectedAnswer.Length)
        {
            SetLastSentence();
            ValidateSubmission();
        }
    }

    private void OnUnsubmitReceived()
    {
        totalSubmitted--;
    }

    private void ValidateSubmission()
    {
        bool allCorrect = true;
        for(int i = 0; i < expectedAnswer.Length; i++)
        {
            if(expectedAnswer[i].Contains("/")) // Handle submission where multiple words work
            {
                var validAnswers = expectedAnswer[i].Split("/");
                if (!validAnswers.Contains(submitBoxes[i].Submitted.Word))
                {
                    allCorrect = false;
                    submitBoxes[i].Submitted.SetBorderColor(GetBorderColor(submitBoxes[i].Submitted, i));
                    continue;
                }
            }
            else
            {
                if (submitBoxes[i].Submitted.Word != expectedAnswer[i])
                {
                    allCorrect = false;
                    submitBoxes[i].Submitted.SetBorderColor(GetBorderColor(submitBoxes[i].Submitted, i));
                    continue;
                }
            }
            // Early exits if wrong
            submitBoxes[i].Submitted.SetBorderColor(Thought.BorderColors.GREEN);
        }
        if (allCorrect) EmitSignal(nameof(CorrectSubmission));
        else EmitSignal(nameof(WrongSubmission));
    }

    private Thought.BorderColors GetBorderColor(Thought thought, int index)
    {
        if (expectedAnswer[index] == thought.Word) return Thought.BorderColors.GREEN; // Check word directly
        else if (expectedAnswer[index].Contains("/")) // Check answer if it allows multiple answers
        {
            string[] answers = expectedAnswer[index].Split("/");
            if (answers.Contains(thought.Word)) return Thought.BorderColors.GREEN;
        }


        // Check if expected answers has the word
        foreach(string word in expectedAnswer)
        {
            if (word.Contains("/"))
            {
                string[] answers = word.Split("/");
                if (answers.Contains(thought.Word)) return Thought.BorderColors.YELLOW;
            }
            else
            {
                if (thought.Word == word) return Thought.BorderColors.YELLOW;
            }
        }
        return Thought.BorderColors.RED;
    }

    private void SetLastSentence()
    {
        string sentence = "";
        foreach(SubmitBox box in submitBoxes)
        {
            sentence += $"{box.Submitted.Word} ";
        }
        LastSubmitSentence = sentence.Trim();
    }
}
