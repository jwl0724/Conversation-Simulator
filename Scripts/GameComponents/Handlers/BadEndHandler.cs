using Godot;
using System;

public class BadEndHandler : Control
{
    private const float SPAWN_TIME = 0.4f;
    private const float CRAWL_TIME = 0.6f;
    private const float TEXT_LINGER_TIME = 0.5f;

    [Signal] public delegate void FinishSequence();
    private SpeechBubble parentBubble;
    public override void _Ready()
    {
        Visible = false;
        parentBubble = GetNode<SpeechBubble>("ParentBubble");
    }

    public void PlaySequence()
    {
        Visible = true;

        var seq = CreateTween();
        foreach(string sentence in Globals.BAD_END_PARENT_DIALOGUE)
        {
            // Show text
            seq.TweenCallback
            (
                parentBubble,
                nameof(parentBubble.PlayShow),
                new Godot.Collections.Array(){sentence, SPAWN_TIME, CRAWL_TIME, TEXT_LINGER_TIME}
            );
            seq.TweenInterval(SPAWN_TIME + CRAWL_TIME + TEXT_LINGER_TIME);

            // Hide previous text
            seq.TweenCallback(parentBubble, nameof(parentBubble.PlayHide), new Godot.Collections.Array(){SPAWN_TIME});
            seq.TweenInterval(SPAWN_TIME);
        }
        seq.TweenInterval(TEXT_LINGER_TIME + SPAWN_TIME + CRAWL_TIME);
        seq.TweenCallback(this, PropertyNames.EmitSignal, new Godot.Collections.Array(){nameof(FinishSequence)});
        seq.Play();
    }
}
