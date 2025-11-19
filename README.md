## Bugs Note

### Issue: First submit box is positioning incorrectly on the SetBounds call after CallDeferred
The first CallDeferred(SetBounds) executes one idle frame earlier than the later ones, because
AddChild() runs layout updates immediately for the first child, but batched for subsequent children

This is a known Godot Control behavior:

When the first Control is added, the parent does a layout pass immediately.
When more Controls are added in the same frame, Godot batches the layout updates until next idle frame.
So their position is stabilized before your deferred call runs.

This causes:

For Box #1 - SetBounds() runs right after the first layout, so it still has its default (0,306.5)

For Box #2+ - Their layout finishes before your deferred SetBounds call, so they get correct positions.

### Issue: Parameter PTR is null
```md
E 0:03:51.488   godot_icall_0_6: Parameter "ptr" is null.
  <C++ Source>  modules/mono/glue/mono_glue.gen.cpp:59 @ godot_icall_0_6()
  <Stack Trace> :0 @ System.String Godot.NativeCalls.godot_icall_0_6(IntPtr , IntPtr )()
                Label.cs:313 @ System.String Godot.Label.GetText()()
                Label.cs:64 @ System.String Godot.Label.get_Text()()
                Thought.cs:33 @ System.String Thought.get_Word()()
                SubmitHandler.cs:84 @ void SubmitHandler.ValidateSubmission()()
                SubmitHandler.cs:61 @ void SubmitHandler.OnSubmitReceived()()
                :0 @ void Godot.NativeCalls.godot_icall_2_713(IntPtr , IntPtr , System.String , System.Object[] , System.String )()
                Object.cs:365 @ void Godot.Object.EmitSignal(System.String , System.Object[] )()
                SubmissionBox.cs:69 @ void SubmissionBox.NotifySubmit()()
                Thought.cs:166 @ void Thought.OnButtonUp()()
```

## Credits List

- Chewy Font - Designed by Sideshow from Google Fonts
- Fuzzy Bubbles - Designed by Robert Leuschke from Google Fonts
- Pop SFX - DRAGON-STUDIO from Pixabay