using System;
using Godot;

public class Globals : Node
{
    public static float MasterVolume = 1;
    public static float SFXVolume = 1;
    public static float MusicVolume = 1;

    // WORD BANK
    public static readonly string[][] WORD_BANK =
    {
        new[]{"One", "CheeseBurger", "Please", "Cow", "Dairy", "Eat", "Pig", "Polite", "Amount", "Quantity", "Number", "Courteous", "Respect", "Food", "Talk"},
        new[]{"Two", "Box", "Of", "RucNuggets", "Quantity", "Number", "Chicken", "Meat", "Container", "Preposition", "Connect", "Word", "Canister", "Create", "Amount"},
        new[]{"Yes", "Sure", "Response", "Answer", "Reaction", "Reply", "Return", "Comment", "Retort", "Acknowledgement", "Respond", "Input", "Retort", "Dialogue", "Feedback"},
        new[]{"Yes", "Sure", "Response", "Answer", "Reaction", "Reply", "Return", "Comment", "Retort", "Acknowledgement", "Respond", "Input", "Retort", "Dialogue", "Feedback"},
        new[]{"5+2", "2+2", "1+1", "2+2", "1+0.5", "5+0.5", "5+0.1", "2+0.5", "5+0.1", "0.5+0.5", "0.1+0.1", "2+0.5", "2+2+2", "2+1", "5+1"},
        new[]{"Yes", "No", "Response", "Answer", "Reaction", "Reply", "Return", "Comment", "Retort", "Acknowledgement", "Respond", "Input", "Retort", "Dialogue", "Feedback"},
    };

    // NOTE: Uncomment for development for less options
    // public static readonly string[][] WORD_BANK =
    // {
    //     new[]{"One", "CheeseBurger", "Please"},
    //     new[]{"Two", "Box", "Of", "RucNuggets"},
    //     new[]{"Yes", "Sure"},
    //     new[]{"Yes", "Sure"},
    //     new[]{"5+2"},
    //     new[]{"Yes", "No"},
    // };

    // DIALOGUE SEQUENCES
    public static readonly Tuple<string, string>[] DIALOGUE_KEY =
    {
        new Tuple<string, string>("Welcome to RucRonalds, What can I get for you today?", "One CheeseBurger Please"),
        new Tuple<string, string>("Would you like anything else?", "Two Box Of RucNuggets"),
        new Tuple<string, string>("Would you like fries with that?", "Yes/Sure"),
        new Tuple<string, string>("How about soda?", "Yes/Sure"),
        new Tuple<string, string>("That would be $6.90 please.", "5+2"), // Note: All text options can't add over $6.90
        new Tuple<string, string>("Would you like change?", "Yes/No")
    };
    public static readonly string[] GOOD_END_SEQUENCE =
    {
        "One Big Ruc, RucNuggets, and fries with soda coming up.",
        "Please wait one moment, your order will be ready soon."
    };
    public static readonly string[] BAD_END_SEQUENCE =
    {
        "I'm sorry but you're holding up the line",
        "Could you please move aside for the other customers?"
    };
    public static readonly string ERROR_TEXT = "Sorry I couldn't quite understand, could you repeat that?";
}
