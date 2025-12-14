using Godot;
using System;

public class SceneManager : Node
{
    public enum GameScene
    {
        MAIN_MENU,
        IN_GAME,
        TRY_AGAIN
    }

    public static SceneManager Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;
        Input.MouseMode = Input.MouseModeEnum.Confined; // All scenes uses this setting
    }

    private readonly PackedScene mainMenu = GD.Load<PackedScene>("res://Scenes/Screens/MainMenu.tscn");
    private readonly PackedScene inGameScene = GD.Load<PackedScene>("res://Scenes/Screens/InGame.tscn");
    private readonly PackedScene tryAgainScene = GD.Load<PackedScene>("res://Scenes/Screens/TryAgain.tscn");

    public void ChangeScene(GameScene screen)
    {
        switch(screen)
        {
            case GameScene.MAIN_MENU:
                GetTree().ChangeSceneTo(mainMenu);
                break;
            case GameScene.IN_GAME:
                GetTree().ChangeSceneTo(inGameScene);
                break;
            case GameScene.TRY_AGAIN:
                GetTree().ChangeSceneTo(tryAgainScene);
                break;
        }
    }
}
