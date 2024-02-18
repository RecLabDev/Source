#if TOOLS
using Godot;
using System;

[Tool]
public partial class Aby : EditorPlugin
{
	public override void _EnterTree()
	{
		GD.Print("Aby plugin loaded.");
	}

	public override void _ExitTree()
	{
		GD.Print("Aby plugin unloaded.");
	}
}
#endif
