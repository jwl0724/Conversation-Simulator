using Godot;
using System;

public partial class InGame // File to handle cutscenes
{
    private void PlaySpawning()
    {
        var opening = CreateTween();
        // TODO: Way later, probably some bubbly intro animation where all the UI elements pop u

        opening.TweenCallback(dialogue, nameof(dialogue.NextDialogue));
        opening.TweenCallback(timerBar.Timer, nameof(timerBar.Timer.Start).ToLower());
    }

    private void PlayGoodEnd()
    {
        // TODO: way later, probably fade to white, then food appears, then fade the food away and have a message saying "You ate the food, it was delicious, thanks for playing" then go back to menu
        timerBar.Timer.Stop();

        var ending = CreateTween();

        ending.TweenCallback(SceneManager.Instance, nameof(SceneManager.ChangeScene), new Godot.Collections.Array(){SceneManager.GameScene.MAIN_MENU});
        GD.Print("Good end.");
    }

    private void PlayBadEnd()
    {
        // TODO: way later, probably change prompt text to "I'm sorry you're holding up the line, could you move along please?" Then another dialogue from the side saying "It's okay honey, I know you're trying to your best, go get em next time" and probably a retry screen?

        GD.Print("Bad end.");
    }

    private const float errorShakeOffset = 20;
    private void PlayError()
    {
        Vector2 shakeOffset = new Vector2(GD.Randf() * errorShakeOffset, GD.Randf() * errorShakeOffset);

        var shake = CreateTween();
        shake.TweenProperty(this, PropertyNames.RectPosition, shakeOffset, DialogueHandler.SPAWN_TIME / 6);
        ApplyTweenToSubmitted(shake, PropertyNames.RectPosition, shakeOffset, DialogueHandler.SPAWN_TIME / 6);
        shake.TweenProperty(this, PropertyNames.RectPosition, shakeOffset * -1, DialogueHandler.SPAWN_TIME / 6);
        ApplyTweenToSubmitted(shake, PropertyNames.RectPosition, shakeOffset * -1, DialogueHandler.SPAWN_TIME / 6);
        shake.TweenProperty(this, PropertyNames.RectPosition, Vector2.Zero, DialogueHandler.SPAWN_TIME / 6);
        ApplyTweenToSubmitted(shake, PropertyNames.RectPosition, Vector2.Zero, DialogueHandler.SPAWN_TIME / 6);

        var redFlash = CreateTween();
        redFlash.TweenProperty(this, nameof(Modulate).ToLower(), Colors.IndianRed, DialogueHandler.SPAWN_TIME / 4);
        ApplyTweenToSubmitted(redFlash, nameof(Modulate).ToLower(), Colors.IndianRed, DialogueHandler.SPAWN_TIME / 4);
        redFlash.TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, DialogueHandler.SPAWN_TIME / 4);
        ApplyTweenToSubmitted(redFlash, nameof(Modulate).ToLower(), Colors.White, DialogueHandler.SPAWN_TIME / 4);
        redFlash.TweenCallback(dialogue, nameof(dialogue.ErrorDialogue));

        redFlash.Play();
        shake.Play();
    }

    // Needed because submitted are top level (effects to root don't apply to them)
    private void ApplyTweenToSubmitted(SceneTreeTween tween, string property, object finalValue, float duration)
    {
        var submitBoxes = GetTree().GetNodesInGroup(GroupNames.SubmitBoxes);
        foreach(SubmitBox box in submitBoxes)
        {
            Thought submitted = box.Submitted;
            if (submitted == null) continue;

            if (property == PropertyNames.RectPosition && finalValue is Vector2 offset) // Top level uses different positional coords
            {
                tween.Parallel().TweenProperty(submitted, property, submitted.RectPosition + offset, duration);
            }
            else
            {
                tween.Parallel().TweenProperty(submitted, property, finalValue, duration);
            }
        }
    }
}
