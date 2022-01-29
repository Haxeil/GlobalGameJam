using Godot;
using System;

public class DoorGhost : Area2D
{
	private void DoorGhostGhostEntered(Ghost body)
	{
		body.QueueFree();
		var player = GetParent().GetNode<Player>("Player");
		player.GlobalPosition = body.GlobalPosition;
		player.GetNode<Sprite>("Sprite").Visible = true;
		player.canMove = true;
	}
	private void DoorPersonPersonEntered(Player person) {
		return;
	}

}



