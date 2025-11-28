using Godot;
using System;

public class Thought : Button
{
    // SFX CONSTANTS
    private const float POP_PITCH_VARIANCE = 0.2f;
    private const float SPAWN_SFX_DELAY = 0.4f;

    // ANIMATION CONSTANTS
    public const float HELD_SIZE = 0.9f;
    public const float HOVERED_SIZE = 1.25f;
    private const float SPAWN_ANIMATION_TIME = 0.5f;
    private const float RESIZE_TIME = 0.2f;

    // LERP CONSTANTS
    private const float MOUSE_STICK_AMOUNT = 0.4f;
    private const float VELOCITY_SLOW_STRENGTH = 0.01f;

    // TIME CONSTANTS
    private const float RETURN_TIME = 0.75f;

    // STATIC VALUES
    private static float MIN_VELOCITY = 100;
    private static float MAX_VELOCITY = 200;

    // SIGNALS
    [Signal] public delegate void ThoughtPickedUp(Thought thought);
    [Signal] public delegate void ThoughtReleased(Thought thought);

    // EXPORTS
    [Export] private string startingText = "";
    [Export] private bool spawnSFX = true;

    private AudioStreamPlayer2D audio;
    private ColorRect boxVisual;
    private ControlScaler scaler;
    private Tween tweener;
    private Label label;

    public string Word { get => label.Text; }
    public bool IsSubmitted { get => GetParent() is SubmitBox; }
    public bool IsSelected { get; private set; } = false; // Hovered
    public bool IsHeld { get; private set; } = false;

    public Vector2 Velocity { get; private set; } = Vector2.Zero;
    private Vector2 originalVisualSize;
    private bool canMove = true;

    /*
        GODOT PROCESSES
    */
    public override void _Ready()
    {
        scaler = GetNode<ControlScaler>("ScaleHelper");
        tweener = GetNode<Tween>("Tweener");
        label = GetNode<Label>("Text");
        audio = GetNode<AudioStreamPlayer2D>("Audio");
        boxVisual = GetNode<ColorRect>("Background/Body");

        Connect(SignalNames.MouseEntered, this, nameof(OnMouseEnter));
        Connect(SignalNames.MouseExited, this, nameof(OnMouseExit));
        Connect(SignalNames.ButtonDown, this, nameof(OnButtonDown));
        Connect(SignalNames.ButtonUp, this, nameof(OnButtonUp));

        RectScale = Vector2.Zero;
        Modulate = Colors.Transparent;
        label.Text = startingText;
        originalVisualSize = boxVisual.RectSize;
        audio.VolumeDb = MathHelper.FactorToDB(Globals.SFXVolume) + MathHelper.FactorToDB(Globals.MasterVolume);
        Velocity = new Vector2((float)GD.RandRange(-1, 1), (float)GD.RandRange(-1, 1)).Normalized() * (float)GD.RandRange(MIN_VELOCITY, MAX_VELOCITY);

        scaler.ScaleToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.Out, Tween.TransitionType.Bounce);
        if (spawnSFX) NodeHelper.PlayRandomPitchAudio(audio, 1 - POP_PITCH_VARIANCE, 1 + POP_PITCH_VARIANCE, SPAWN_SFX_DELAY);
        GetNode<ModulateHelper>("ModulateHelper").ModulateToDefault(SPAWN_ANIMATION_TIME, Tween.EaseType.InOut, Tween.TransitionType.Circ);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!canMove) return;

        if (IsHeld) // TODO: Fix wonky interaction where the box snaps to the submit position after it's been submitted once and then returned and grabbed again
        {
            // if (HoveredSubmitBox == null || !IsInstanceValid(HoveredSubmitBox))
            // {
            //     Vector2 newPos = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), MOUSE_STICK_AMOUNT);
            //     Velocity = (newPos - RectPosition) / delta;
            //     RectPosition = newPos;
            // }
            // else RectPosition = RectPosition.LinearInterpolate(HoveredSubmitBox.RectGlobalPosition, SUBMIT_LERP_STRENGTH);

            Vector2 newPos = RectGlobalPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, GetViewport().GetMousePosition()), MOUSE_STICK_AMOUNT);
            Velocity = (newPos - RectGlobalPosition) / delta;
            RectGlobalPosition = newPos;
        }
        // else if (IsSubmitted) // TODO: Refactor to make it reparent to the submit box and reparent back when removed // Reparent
        // {
        //     if (!IsInstanceValid(HoveredSubmitBox)) return; // Check for final frames when submit box is deleted
        //     RectPosition = RectPosition.LinearInterpolate(HoveredSubmitBox.RectGlobalPosition, SUBMIT_LERP_STRENGTH);
        // }
        // else if (IsReturning) // Make it auto return when outside of boundaries
        // {
        //     Vector2 newPosition = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, ThoughtBox.Center), RETURN_LERP_STRENGTH);
        //     if (ThoughtBox.IsInBounds(this))
        //     {
        //         IsReturning = false;
        //         Velocity = (newPosition - RectPosition) / delta;
        //     }
        //     RectPosition = newPosition;
        // }
        else
        {
            // if (!ThoughtBox.IsInBounds(this))
            // {
            //     Vector2 newPosition = RectPosition.LinearInterpolate(MathHelper.GetPositionFromCenter(this, ThoughtBox.Center), RETURN_LERP_STRENGTH);
            //     Velocity = (newPosition - RectPosition) / delta;
            //     RectPosition = newPosition;
            //     return;
            // }

            if (!ThoughtBox.IsInBounds(this)) Rebound(ThoughtBox.IsMovingAway(this, true), ThoughtBox.IsMovingAway(this, false));
            RectPosition += Velocity * delta;
            if (Velocity.Length() > MAX_VELOCITY) Velocity = Velocity.LinearInterpolate(Velocity.Normalized() * MAX_VELOCITY, VELOCITY_SLOW_STRENGTH);
        }
    }

    /*
        PUBLIC INTERFACE FUNCTIONS
    */
    public void Rebound(bool flipX, bool flipY)
    {
        Vector2 newVelocity = Velocity;
        newVelocity.x *= flipX ? -1 : 1;
        newVelocity.y *= flipY ? -1 : 1;
        Velocity = newVelocity;
    }

    // TODO: Fix box not despawning properly via following the submission box upon going to next dialogue
    public void Despawn()
    {
        scaler.Scale(0, SPAWN_ANIMATION_TIME, Tween.EaseType.In, Tween.TransitionType.Back);
        var delay = CreateTween();
        delay.TweenCallback(this, PropertyNames.QueueFree).SetDelay(SPAWN_ANIMATION_TIME);
        delay.Play();
    }

    public void SetWord(string newText)
    {
        label.Text = newText;
    }

    public void ToggleMovement(bool move)
    {
        canMove = move;
    }

    /*
        MOUSE EVENTS
    */
    private void OnButtonUp()
    {
        IsHeld = false;
        scaler.ScaleToDefault();
        if (!ThoughtBox.IsInBounds(this)) Velocity = (ThoughtBox.Center - RectGlobalPosition - RectSize / 2) / RETURN_TIME;
        EmitSignal(nameof(ThoughtReleased), this);


        // if (HoveredSubmitBox == null)
        // {
        //     IsReturning = !ThoughtBox.IsInBounds(this);
        // }
        // else
        // {
        //     IsSubmitted = true;
        //     HoveredSubmitBox.NotifySubmit();
        // }
    }

    private void OnButtonDown()
    {
        IsHeld = true;
        // IsReturning = false;
        scaler.Scale(HELD_SIZE);
        EmitSignal(nameof(ThoughtPickedUp), this);

        // if (IsSubmitted)
        // {
        //     IsSubmitted = false;
        //     HoveredSubmitBox.NotifyUnsubmit();
        // }
    }

    private void OnMouseEnter()
    {
        if (IsHeld) return;

        if (IsSubmitted)
        {
            tweener.StopAll();
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, boxVisual.RectSize, originalVisualSize, RESIZE_TIME);
            tweener.Start();
        }
        else
        {
            IsSelected = true;
            scaler.Scale(HOVERED_SIZE);
        }
    }

    private void OnMouseExit()
    {
        if (IsHeld) return;

        if (IsSubmitted)
        {
            tweener.StopAll();
            tweener.InterpolateProperty(boxVisual, PropertyNames.RectSize, boxVisual.RectSize, originalVisualSize + Vector2.One * 5, RESIZE_TIME);
            tweener.Start();
        }
        else
        {
            IsSelected = false;
            scaler.ScaleToDefault();
        }
    }
}
