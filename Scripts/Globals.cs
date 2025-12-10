using System;
using Godot;

public class Globals : Node
{
    public const float TIME_LIMIT = 20;
    public const float MUTE_DB = -60;
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

    // DIALOGUE SEQUENCES
    public static readonly Tuple<string, string>[] DIALOGUE_KEY =
    {
        new Tuple<string, string>("Welcome to RucRonalds, What can I get for you?", "One CheeseBurger Please"),
        new Tuple<string, string>("Would you like anything else?", "Two Box Of RucNuggets"),
        new Tuple<string, string>("That would be $6.90 please.", "5+2")
    };
    public static readonly string[] GOOD_END_NARRATION_SEQUENCE =
    {
        "Your food arrived.",
        "You ate the food.",
        "It was delicious.",
        "The End.",
        "Thanks for Playing!"
    };
    public static readonly string[] BAD_END_DIALOGUE_SEQUENCE =
    {
        "I'm sorry but you're holding up the line",
        "Could you please move aside?"
    };
    public static readonly string PLAYER_ERROR = "Sorry let me try that again.";
    public static readonly string LAST_DIALOGUE = "Your order will be ready soon";
    public static readonly string ERROR_TEXT = "Sorry, could you repeat that?";
    public static readonly string ERROR_CHANGE = "Sorry, but that's not enough money to pay.";
}
