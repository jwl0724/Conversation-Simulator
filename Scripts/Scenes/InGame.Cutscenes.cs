using Godot;

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
        filter.Color = Colors.White;
        Globals.FadeInTransitionColor = Colors.White;
        Globals.RetryPrompt = Globals.GOOD_END_RETRY;

        NodeHelper.FadeMusic(bgm, spawnInTime / 3, false);
        NodeHelper.FadeMusic(goodEndHandler.BGM, spawnInTime / 3, true, spawnInTime * 2/3);

        var ending = CreateTween();
        ending.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, spawnInTime);

        goodEndHandler.Connect
        (
            nameof(GoodEndHandler.FinishSequence),
            SceneManager.Instance,
            nameof(SceneManager.Instance.ChangeScene),
            new Godot.Collections.Array(){SceneManager.GameScene.TRY_AGAIN},
            flags: (uint)ConnectFlags.Oneshot
        );
        ending.TweenCallback(goodEndHandler, nameof(goodEndHandler.PlaySequence));
        ending.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.Transparent, 0);
        ending.Play();
    }

    private void PlayBadEnd()
    {
        filter.Color = Colors.Black;
        Globals.FadeInTransitionColor = Colors.Black;
        Globals.RetryPrompt = Globals.BAD_END_RETRY;

        NodeHelper.FadeMusic(bgm, spawnInTime / 3, false);
        NodeHelper.FadeMusic(badEndHandler.BGM, spawnInTime / 3, true, spawnInTime * 2/3);

        var ending = CreateTween();
        ending.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, spawnInTime);

        badEndHandler.Connect
        (
            nameof(BadEndHandler.FinishSequence),
            SceneManager.Instance,
            nameof(SceneManager.Instance.ChangeScene),
            new Godot.Collections.Array(){SceneManager.GameScene.TRY_AGAIN},
            flags: (uint)ConnectFlags.Oneshot
        );
        ending.TweenCallback(badEndHandler, nameof(badEndHandler.PlaySequence));
        ending.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.Transparent, 0);
        ending.Play();
    }

    private const float errorShakeOffset = 20;
    private void PlayError()
    {
        sfx.Stream = wrongSFX;
        NodeHelper.PlayRandomPitchAudio(sfx, 1, 1);
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
        redFlash.TweenCallback(dialogue, nameof(dialogue.ErrorDialogue), new Godot.Collections.Array(){submitArea.LastSubmitSentence});

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
