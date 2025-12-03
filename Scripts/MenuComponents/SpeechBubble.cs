using Godot;
using System;

public class SpeechBubble : Control
{
    [Export] private Texture speakerPicture;
    [Export] private bool flipH = false;

    [Signal] public delegate void FinishAnimation();

    public Label BubbleText { get; private set; }
    private TextureRect speakerPortrait;
    private Control bubbleBackground;

    public override void _Ready()
    {
        speakerPortrait = GetNode<TextureRect>("Speaker");
        BubbleText = GetNode<Label>("Label");
        bubbleBackground = GetNode<Control>("Background");

        Visible = false;
        speakerPortrait.Texture = speakerPicture;

        if (flipH) // Flip entire node then flip text again to restore text
        {
            RectScale = new Vector2(-1, 1);
            BubbleText.RectScale = new Vector2(-1, 1);
        }
    }

    public void PlayShow(float animationTime)
    {

    }

    public void PlayHide(float animationTime)
    {

    }
}
