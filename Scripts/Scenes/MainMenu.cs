using Godot;

public class MainMenu : Control
{
    private const float START_TRANSITION_TIME = 1.5f;
    private const float LINGER_TIME = 1.5f;
    private const float SPAWN_IN_TIME = 0.25f;
    private const float CRAWL_TIME = 0.75f;

    private Godot.Collections.Array menuButtons;
    private SubmitBox submitBox;
    private OptionsMenu optionsMenu;
    private Credits credits;
    private AudioStreamPlayer bgm;
    private ThoughtBox thoughtBox;
    private ColorRect filter;

    public override void _Ready()
    {
        GD.Randomize();
        submitBox = GetNode<SubmitBox>("SubmitArea/SubmissionBox");
        submitBox.Connect(nameof(SubmitBox.Submit), this, nameof(OnSubmit));
        submitBox.StartListening();

        bgm = GetNode<AudioStreamPlayer>("BGM");
        NodeHelper.FadeMusic(bgm, SPAWN_IN_TIME, true);

        optionsMenu = GetNode<OptionsMenu>("Modals/OptionsMenu");
        optionsMenu.Connect(nameof(OptionsMenu.OptionsClosed), this, nameof(OnModalClosed));
        optionsMenu.Connect(nameof(OptionsMenu.MusicVolumeChanged), this, nameof(OnMusicChanged));

        credits = GetNode<Credits>("Modals/Credits");
        credits.Connect(nameof(Credits.CreditsClosed), this, nameof(OnModalClosed));

        thoughtBox = GetNode<ThoughtBox>("ThoughtBox");
        thoughtBox.SetBounds();

        filter = GetNode<ColorRect>("FilterOverlay/FadeColor");
        filter.Color = Globals.FadeInTransitionColor;
        filter.Modulate = Colors.White;

        menuButtons = GetTree().GetNodesInGroup(GroupNames.Thoughts);
        EnableAllButtons(false);

        var fadeIn = CreateTween();
        fadeIn.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.Transparent, START_TRANSITION_TIME / 2);
        fadeIn.TweenCallback(this, nameof(EnableAllButtons), new Godot.Collections.Array(){true});
        fadeIn.Play();

        if (OS.HasFeature("web"))
        {
            var quitButton = GetNode<Thought>("Quit");
            quitButton.Disabled = true;
            quitButton.Visible = false;
        }
    }

    private void EnableAllButtons(bool enable)
    {
        foreach(Thought button in menuButtons)
        {
            button.Disabled = !enable;
        }
    }

    private void OnMusicChanged()
    {
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
    }

    private void OnModalClosed()
    {
        EnableAllButtons(true);
        submitBox.EjectThought();
    }

    private void OnSubmit()
    {
        string option = submitBox.Submitted.Word;

        if (option == "Start")
        {
            filter.Color = Colors.Black;
            EnableAllButtons(false);
            StartSequence();
        }
        else if (option == "Options")
        {
            optionsMenu.ShowOptions();
            EnableAllButtons(false);
        }
        else if (option == "Credits")
        {
            credits.ShowCredits();
            EnableAllButtons(false);
        }
        else if (option == "Quit")
        {
            GetTree().Quit();
        }
    }

    private void StartSequence()
    {
        SpeechBubble parent = GetNode<SpeechBubble>("SpeechBubbles/Parent");
        SpeechBubble player = GetNode<SpeechBubble>("SpeechBubbles/Player");
        parent.Visible = false;
        player.Visible = false;

        var transition = CreateTween();

        // Show parent speech bubble then hide
        transition.TweenCallback(parent, "set_visible", new Godot.Collections.Array(){true});
        transition.TweenCallback(parent, nameof(parent.PlayShow), new Godot.Collections.Array(){Globals.PARENT_INTRO, CRAWL_TIME, CRAWL_TIME, 0});
        transition.TweenInterval(SPAWN_IN_TIME + CRAWL_TIME + LINGER_TIME);
        transition.TweenCallback(parent, nameof(parent.PlayHide), new Godot.Collections.Array(){SPAWN_IN_TIME});
        transition.TweenInterval(SPAWN_IN_TIME);

        // Show show player speech bubble then hide
        transition.TweenCallback(player, "set_visible", new Godot.Collections.Array(){true});
        transition.TweenCallback(player, nameof(player.PlayShow), new Godot.Collections.Array(){Globals.PLAYER_INTRO, CRAWL_TIME, CRAWL_TIME, 0});
        transition.TweenInterval(SPAWN_IN_TIME + CRAWL_TIME + LINGER_TIME);
        transition.TweenCallback(player, nameof(player.PlayHide), new Godot.Collections.Array(){SPAWN_IN_TIME});
        transition.TweenInterval(SPAWN_IN_TIME);

        // Fade filter and effects
        transition.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, START_TRANSITION_TIME * 0.75f);
        // transition.Parallel().TweenProperty(bgm, PropertyNames.VolumeDb, Globals.MUTE_DB, START_TRANSITION_TIME);
        NodeHelper.FadeMusic(bgm, START_TRANSITION_TIME, false, SPAWN_IN_TIME * 4 + CRAWL_TIME * 2 + LINGER_TIME * 2 + START_TRANSITION_TIME * 0.75f);
        transition.TweenInterval(START_TRANSITION_TIME * 0.25f);
        transition.TweenCallback(SceneManager.Instance, nameof(SceneManager.Instance.ChangeScene), new Godot.Collections.Array(){SceneManager.GameScene.IN_GAME});

        transition.Play();
    }
}
