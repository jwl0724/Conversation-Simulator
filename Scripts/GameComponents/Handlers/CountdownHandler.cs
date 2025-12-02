using Godot;
using System;

public class CountdownHandler : Label
{
    private const float Y_SPAWN_OFFSET = 100;
    private const float LOWER_PITCH = 0.6f;
    private const float UPPER_PITCH = 0.8f;
    private readonly Vector2 SPAWN_SCALE = Vector2.One * 2;

    [Signal] public delegate void CountdownFinished();
    private string[] countdownTextOrder = {"Get Ready to Order", "3", "2", "1", "Go Order"};
    private AudioStreamPlayer2D audio;

    public override void _Ready()
    {
        audio = GetNode<AudioStreamPlayer2D>("CountdownSFX");
        Visible = false;
    }

    public void StartCountdown()
    {
        Modulate = Colors.Transparent;
        Visible = true;

        float steps = 6;
        Vector2 defaultPos = RectPosition;
        var countdown = CreateTween();

        foreach(string text in countdownTextOrder)
        {
            // Undo default properties from previous text
            countdown.TweenProperty(this, PropertyNames.RectPosition, defaultPos + Vector2.Up * Y_SPAWN_OFFSET, 0);
            countdown.TweenProperty(this, PropertyNames.RectScale, SPAWN_SCALE, 0);
            countdown.TweenProperty(this, nameof(Text).ToLower(), text, 0);

            // Tween spawn in
            countdown.TweenCallback(this, nameof(PlaySFX));
            countdown.TweenProperty(this, nameof(Modulate).ToLower(), Colors.White, 1 / steps);
            countdown.Parallel().TweenProperty(this, PropertyNames.RectPosition, defaultPos, 1 / steps);
            countdown.Parallel().TweenProperty(this, PropertyNames.RectScale, Vector2.One, 1 / steps);

            // Linger text
            countdown.TweenInterval(3 / steps);

            // Tween spawn out
            countdown.TweenProperty(this, nameof(Modulate).ToLower(), Colors.Transparent, 1 / steps);
            countdown.Parallel().TweenProperty(this, PropertyNames.RectPosition, defaultPos + Vector2.Down * Y_SPAWN_OFFSET, 1 / steps);
            countdown.Parallel().TweenProperty(this, PropertyNames.RectScale, Vector2.Zero, 1 / steps);
        }
        countdown.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(CountdownFinished)});
        countdown.Play();
    }

    private void PlaySFX()
    {
        NodeHelper.PlayRandomPitchAudio(audio, LOWER_PITCH, UPPER_PITCH);
    }
}
