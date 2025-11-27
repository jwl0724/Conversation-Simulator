## Bugs Note

### Issue: Submit Then Picking up Again Causes Thought to Stick to Mouse
Consider the sequence:

Player is holding the button

You drag into the submit box

Submit box reparents the Thought (your button) into itself

User releases the mouse

Godot no longer considers the mouse “over” the button because it CHANGED SCENE POSITION

The InputEventMouseButton release event is no longer routed to your button
→ OnButtonUp never fires

## Credits List

- Chewy Font - Designed by Sideshow from Google Fonts
- Fuzzy Bubbles - Designed by Robert Leuschke from Google Fonts
- Pop SFX - DRAGON-STUDIO from Pixabay