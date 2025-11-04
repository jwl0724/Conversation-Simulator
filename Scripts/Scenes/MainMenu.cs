using Godot;
using System;

public class MainMenu : Control
{
    private SubmissionBox submitBox;

    public override void _Ready()
    {
        submitBox = GetNode<SubmissionBox>("SubmissionBox");
        submitBox.Connect(nameof(SubmissionBox.Submit), this, nameof(OnSubmit));
    }

    private void OnSubmit()
    {
        string option = submitBox.Submitted.Word;

        if (option == "Ready")
        {
            GD.Print("Game Started"); // TODO: Switch to the game scene here
        }
        else if (option == "Options")
        {
            GD.Print("Options Opened"); // TODO: Open a modal that has some toggles to change some global variables
        }
        else if (option == "Credits")
        {
            GD.Print("Credits Opened"); // TODO: Open a modal for credits
        }
        else if (option == "Quit")
        {
            GetTree().Quit();
        }
    }
}
