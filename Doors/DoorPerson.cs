using Godot;
using System;

public class DoorPerson : Area2D
{
	private PackedScene ghostPackedScene = ResourceLoader.Load<PackedScene>("res://Ghost/Ghost.tscn");
	private Ghost ghost;
	public override void _Ready() {
		
	}
	private void DoorPersonPersonEntered(Player person)
	{
		person.canMove = false;
		person.GetNode<Sprite>("Sprite").Visible = false;
		person.velocity = Vector2.Zero;
		ghost = ghostPackedScene.Instance<Ghost>();
		ghost.GlobalPosition = person.GlobalPosition;
		GetParent().CallDeferred("add_child", ghost);
	}
}



