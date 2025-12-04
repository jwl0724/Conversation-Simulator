using Godot;
using System;

public partial class InGame // File to handle cutscenes
{
    private const float spawnInTime = 3;
    private void PlaySpawning()
    {
        countdown.Connect(nameof(CountdownHandler.CountdownFinished), this, nameof(StartGame), flags: (uint)ConnectFlags.Oneshot);

        var opening = CreateTween();

        opening.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.Transparent, spawnInTime * 0.5f);
        opening.TweenProperty(thoughtBox, PropertyNames.RectScale, Vector2.One, spawnInTime * 0.1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        opening.TweenInterval(0.1f);
        opening.TweenProperty(timerBar, PropertyNames.RectScale, Vector2.One, spawnInTime * 0.1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

        opening.TweenInterval(spawnInTime * 0.2f);
        opening.TweenCallback(countdown, nameof(countdown.StartCountdown));

        opening.Play();
    }

    private void PlayGoodEnd()
    {
        // TODO: way later, probably fade to white, then food appears, then fade the food away and have a message saying "You ate the food, it was delicious, thanks for playing" then go back to menu
        timerBar.Timer.Stop();

        var ending = CreateTween();

        // TODO: Get the speech bubble and set the dialogue to good end dialogue (probably put this in the dialoguehandler maybe?)

        // TODO: Set filter color to white and fate the modulate to white

        // TODO: Have another dialogue similar style to customer font style that says "Your food is ready" and crawl that in the middle of the screen

        // TODO: Place the cheeseburger in the scene, and make it pop up like everything else

        // TODO: Place the nuggets in the scene, and make it pop up somewhere else like everything else

        // TODO: Find a blackhole asset and put it on the scene and spawn it in the middle

        // TODO: Spin, Scale, Move the foods to the blackhole asset position

        // TODO: Have another dialogue in the same style as the countdown style say "You ate the food" with crawl

        // TODO: Then crawl again "It was delicious" with crawl

        // TODO: Then crawl "The End." with crawl

        // TODO: Then crawl "Thanks for Playing" with crawl

        // TODO: Then load the try again scene

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
