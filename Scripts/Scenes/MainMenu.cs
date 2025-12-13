using Godot;

public class MainMenu : Control
{
    private const float START_TRANSITION_TIME = 1.5f;

    private Godot.Collections.Array menuButtons;
    private SubmitBox submitBox;
    private OptionsMenu optionsMenu;
    private Credits credits;
    private AudioStreamPlayer bgm;
    private ThoughtBox thoughtBox;
    private ColorRect filter;

    // TODO: Mom speech bubble shows up and says "Honey I'm going to RucRonalds now, you want anything?" -> respond with "I'm coming with you" -> fade to black and start game
    // TODO: Change up the main menu to make it look better

    public override void _Ready()
    {
        GD.Randomize();
        submitBox = GetNode<SubmitBox>("SubmitArea/SubmissionBox");
        submitBox.Connect(nameof(SubmitBox.Submit), this, nameof(OnSubmit));
        submitBox.StartListening();

        bgm = GetNode<AudioStreamPlayer>("BGM");
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

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
            var transition = CreateTween();

            transition.TweenProperty(filter, nameof(Modulate).ToLower(), Colors.White, START_TRANSITION_TIME * 0.75f);
            transition.Parallel().TweenProperty(bgm, PropertyNames.VolumeDb, Globals.MUTE_DB, START_TRANSITION_TIME);
            transition.TweenInterval(START_TRANSITION_TIME * 0.25f);
            transition.TweenCallback(SceneManager.Instance, nameof(SceneManager.Instance.ChangeScene), new Godot.Collections.Array(){SceneManager.GameScene.IN_GAME});

            transition.Play();
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
}
