using Godot;
using System;

public class ParticleHandler : CPUParticles2D
{
    [Export] private StreamTexture burgerTexture;
    [Export] private StreamTexture nuggetTexture;
    public void EmitHint(DialogueHandler.Order texture)
    {
        switch(texture)
        {
            case DialogueHandler.Order.BURGER:
                Emitting = true;
                Texture = burgerTexture;
                break;
            case DialogueHandler.Order.NUGGETS:
                Emitting = true;
                Texture = nuggetTexture;
                break;
            case DialogueHandler.Order.NONE:
                Emitting = false;
                break;
        }
    }
}
