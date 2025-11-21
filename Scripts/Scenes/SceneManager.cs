using Godot;
using System;

public class SceneManager : Node
{
    public enum GameScene
    {
        MAIN_MENU,
        IN_GAME
    }

    public static SceneManager Instance { get; private set; }
    public override void _Ready()
    {
        Instance = this;
    }

    private readonly PackedScene mainMenu = GD.Load<PackedScene>("res://Scenes/Screens/MainMenu.tscn");
    private readonly PackedScene inGameScene = GD.Load<PackedScene>("res://Scenes/Screens/InGame.tscn");

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
        }
    }
}
