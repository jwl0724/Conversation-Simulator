using Godot;
using System;

public class TryAgain : Control
{
    private const float TRANSITION_TIME = 1;
    private const float SPAWN_TIME = 0.5f;

    private Godot.Collections.Array buttons;
    private SpeechBubble parentBubble;
    private ThoughtBox thoughtBox;
    private SubmitBox submitBox;
    private ColorRect filter;

    public override void _Ready()
    {
        GD.Randomize();

        filter = GetNode<ColorRect>("FilterOverlay/FadeColor");
        filter.Color = Globals.FadeInTransitionColor;
        filter.Modulate = Colors.White;

        parentBubble = GetNode<SpeechBubble>("ParentBubble");
        parentBubble.Visible = false;

        thoughtBox = GetNode<ThoughtBox>("ThoughtBox");
        thoughtBox.SetBounds();
        thoughtBox.Visible = false;

        submitBox = GetNode<SubmitBox>("SubmitArea/SubmissionBox");
        submitBox.Connect(nameof(SubmitBox.Submit), this, nameof(OnSubmit));
        submitBox.Visible = false;
        submitBox.StartListening();

        buttons = GetTree().GetNodesInGroup(GroupNames.Thoughts);
        foreach(Thought button in buttons)
        {
            button.Disabled = true;
            button.Visible = false; // TODO: WHY THE FUCK ARE YOU VISIBLE????
        }
        StartupSequence();
    }

    private void StartupSequence()
    {
        var seq = CreateTween();
        seq.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.Transparent, TRANSITION_TIME);
        seq.TweenCallback(parentBubble, nameof(parentBubble.PlayShow), new Godot.Collections.Array(){Globals.RetryPrompt, SPAWN_TIME, SPAWN_TIME, 0});
        seq.TweenInterval(TRANSITION_TIME + SPAWN_TIME);

        thoughtBox.RectScale = Vector2.Zero;
        thoughtBox.Visible = true;
        seq.TweenProperty(thoughtBox, PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

        submitBox.GetParent<Control>().RectScale = Vector2.Zero;
        submitBox.Visible = true;
        seq.TweenProperty(submitBox.GetParent(), PropertyNames.RectScale, Vector2.One, SPAWN_TIME).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        seq.TweenInterval(SPAWN_TIME);

        seq.TweenCallback(this, nameof(ShowButtons));
        seq.Play();
    }

    private void ShowButtons()
    {
        foreach(Thought button in buttons)
        {
            var show = CreateTween();
            show.TweenCallback(button, "set_visible", new Godot.Collections.Array(){true});
            show.TweenProperty(button, nameof(Modulate).ToLower(), Colors.Transparent, 0); // Undo the initial modulate spawn in on button Ready (very scuffed)
            show.TweenProperty(button, nameof(Modulate).ToLower(), Colors.White, SPAWN_TIME);
            show.TweenCallback(this, nameof(EnableButton), new Godot.Collections.Array(){button});
            show.Play();
        }
    }

    private void EnableButton(Button button)
    {
        button.Disabled = false;
    }

    private void OnSubmit()
    {
        string choice = submitBox.Submitted.Word;
        if (choice == "Yes")
        {
            filter.Color = Colors.Black;
            var transition = CreateTween();

            transition.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, TRANSITION_TIME * 0.75f);
            transition.TweenInterval(TRANSITION_TIME * 0.25f);
            transition.TweenCallback(SceneManager.Instance, nameof(SceneManager.Instance.ChangeScene), new Godot.Collections.Array(){SceneManager.GameScene.IN_GAME});

            transition.Play();
        }
        else // i.e. Don't try again
        {
            filter.Color = Globals.FadeInTransitionColor;
            var transition = CreateTween();

            transition.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, TRANSITION_TIME * 0.75f);
            transition.TweenInterval(TRANSITION_TIME * 0.25f);
            transition.TweenCallback(SceneManager.Instance, nameof(SceneManager.Instance.ChangeScene), new Godot.Collections.Array(){SceneManager.GameScene.MAIN_MENU});

            transition.Play();
        }
    }
}
