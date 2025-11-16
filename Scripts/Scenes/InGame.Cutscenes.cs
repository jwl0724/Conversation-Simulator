using Godot;
using System;

public partial class InGame // File to handle cutscenes
{
    private void PlayOpening()
    {
        var opening = CreateTween();
        // TODO: Way later, probably some bubbly intro animation where all the UI elements pop u

        opening.TweenCallback(prompt, nameof(prompt.NextDialogue));
        opening.TweenCallback(timer, nameof(timer.Start).ToLower());
    }

    private void PlayGoodEnd()
    {
        // TODO: way later, probably fade to white, then food appears, then fade the food away and have a message saying "You ate the food, it was delicious, thanks for playing" then go back to menu

        GD.Print("Good end.");
    }

    private void PlayBadEnd()
    {
        // TODO: way later, probably change prompt text to "I'm sorry you're holding up the line, could you move along please?" Then another dialogue from the side saying "It's okay honey, I know you're trying to your best, go get em next time" and probably a retry screen?

        GD.Print("Bad end.");
    }

    private void PlayError()
    {
        // TODO: way later, probably do a screen shake or something

        GD.Print("Error effect.");
    }
}
