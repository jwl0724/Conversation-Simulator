using System;
using Godot;

public class Globals : Node
{
    // GLOBAL CONSTANTS
    public const string BAD_END_RETRY = "Are you ready to try again honey?";
    public const string GOOD_END_RETRY = "Do you want more food honey?";
    public const float TIME_LIMIT = 20;
    public const float MUTE_DB = -60;

    // CROSS SCENE VARIABLES
    public static Color FadeInTransitionColor = Colors.Black;
    public static string RetryPrompt = BAD_END_RETRY;

    // OPTIONS MENU VARIABLES
    public static float MasterVolume = 1;
    public static float SFXVolume = 1;
    public static float MusicVolume = 1;

    // WORD BANK
    public static readonly string[][] WORD_BANK =
    {
        new[]{"Cow", "Dairy", "Eat", "Pig", "Polite", "Amount", "Quantity", "Number", "Courteous", "Respect", "Food", "Talk", "One", "CheeseBurger", "Please"},
        new[]{"Quantity", "Number", "Chicken", "Meat", "Container", "Preposition", "Connect", "Word", "Canister", "Create", "Amount", "Two", "Box", "Of", "RucNuggets"},
        new[]{"2+2", "1+1", "2+2", "1+0.5", "5+0.5", "5+0.1", "2+0.5", "5+0.1", "0.5+0.5", "0.1+0.1", "2+0.5", "2+2+2", "2+1", "5+1", "5+2"}
    };

    // INTRO SEQUENCE
    public static readonly string PARENT_INTRO = "I'm going to RucRonalds now. Want anything?";
    public static readonly string PLAYER_INTRO = "No, I'm coming with you";

    // DIALOGUE SEQUENCES
    public static readonly Tuple<string, string>[] DIALOGUE_KEY =
    {
        new Tuple<string, string>("Welcome to RucRonalds, What can I get for you?", "One CheeseBurger Please"),
        new Tuple<string, string>("Would you like anything else?", "Two Box Of RucNuggets"),
        new Tuple<string, string>("That would be $6.90 please.", "5+2")
    };
    public static readonly string ERROR_TEXT = "Sorry, could you repeat that?";
    public static readonly string PLAYER_ERROR = "Sorry let me try that again.";
    public static readonly string ERROR_CHANGE = "Sorry, but that's not enough money to pay.";
    public static readonly string LAST_DIALOGUE = "Your order will be ready soon";

    // DIALOGUE ENDINGS
    public static readonly string[] GOOD_END_NARRATION_SEQUENCE =
    {
        "You ate the food.",
        "It was delicious.",
        "The End.",
        "Thanks for Playing!"
    };
    public static readonly string[] BAD_END_CLERK_DIALOGUE =
    {
        "I'm sorry but you're holding up the line",
        "Could you please move aside?"
    };
    public static readonly string[] BAD_END_PARENT_DIALOGUE =
    {
        "It's okay honey everyone makes mistakes!",
        "All that matters is that you try your best!",
        "No matter what, I will always love you~"
    };
}
