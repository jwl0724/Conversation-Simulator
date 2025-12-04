using Godot;

public class MainMenu : Control
{
    private const float START_TRANSITION_TIME = 1.5f;

    private Godot.Collections.Array menuButtons;
    private SubmitBox submitBox;
    private OptionsMenu optionsMenu;
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
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

        optionsMenu = GetNode<OptionsMenu>("Modals/OptionsMenu");
        optionsMenu.Connect(nameof(OptionsMenu.OptionsClosed), this, nameof(OnOptionsClosed));
        optionsMenu.Connect(nameof(OptionsMenu.MusicVolumeChanged), this, nameof(OnMusicChanged));
        optionsMenu.Visible = false;

        thoughtBox = GetNode<ThoughtBox>("ThoughtBox");
        thoughtBox.SetBounds();

        filter = GetNode<ColorRect>("FilterOverlay/FadeColor");
        filter.Modulate = Colors.Transparent;
        filter.Color = Colors.Black;

        menuButtons = GetTree().GetNodesInGroup(GroupNames.Thoughts);
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

    private void OnOptionsClosed()
    {
        EnableAllButtons(true);
        submitBox.EjectThought();
    }

    private void OnSubmit()
    {
        string option = submitBox.Submitted.Word;

        if (option == "Ready")
        {
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
            GD.Print("Credits Opened"); // TODO: Open a modal for credits
        }
        else if (option == "Quit")
        {
            GetTree().Quit();
        }
    }
}
