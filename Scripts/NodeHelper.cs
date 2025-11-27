using Godot;
using System;

public static class NodeHelper
{
    public static void ReparentNode(Node node, Node newParent = null) // Will parent to scene root if null
    {
        Node oldParent = node.GetParent();
        if (newParent == null) newParent = node.GetTree().CurrentScene;
        oldParent.RemoveChild(node);
        newParent.AddChild(node);
    }
}
