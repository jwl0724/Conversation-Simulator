using Godot;

public class MainMenu : Control
{
    private Godot.Collections.Array menuButtons;
    private SubmitBox submitBox;
    private OptionsMenu optionsMenu;
    private AudioStreamPlayer bgm;

    public override void _Ready()
    {
        GD.Randomize();
        submitBox = GetNode<SubmitBox>("SubmitArea/SubmissionBox");
        submitBox.Connect(nameof(SubmitBox.Submit), this, nameof(OnSubmit));
        submitBox.StartListening();

        bgm = GetNode<AudioStreamPlayer>("BGM");
        bgm.VolumeDb = MathHelper.FactorToDB(Globals.MusicVolume) + MathHelper.FactorToDB(Globals.MasterVolume);

        optionsMenu = GetNode<OptionsMenu>("OptionsMenu");
        optionsMenu.Connect(nameof(OptionsMenu.OptionsClosed), this, nameof(OnOptionsClosed));
        optionsMenu.Visible = false;

        menuButtons = GetTree().GetNodesInGroup(GroupNames.Thoughts);
    }

    private void EnableAllButtons(bool enable)
    {
        foreach(Thought button in menuButtons)
        {
            button.Disabled = !enable;
        }
    }

    private void OnOptionsClosed()
    {
        EnableAllButtons(true);
    }

    private void OnSubmit()
    {
        string option = submitBox.Submitted.Word;

        if (option == "Ready")
        {
            SceneManager.Instance.ChangeScene(SceneManager.GameScene.IN_GAME);
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
