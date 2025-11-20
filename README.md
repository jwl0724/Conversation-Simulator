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

## Credits List

- Chewy Font - Designed by Sideshow from Google Fonts
- Fuzzy Bubbles - Designed by Robert Leuschke from Google Fonts
- Pop SFX - DRAGON-STUDIO from Pixabay