using Godot;
using System;

public class Ghost : KinematicBody2D
{


	[Export] private Vector2 velocity = Vector2.Zero;
	[Export] private float speed = 50.0f;
	[Export] private float friction = 0.1f;
	[Export] private float jumpPower = 200;
	[Export] private float acceleration = 0.1f;
 	[Signal] public delegate void changeState(StateMachine state);

	StateMachine state = new StateMachine();

	public override void _Ready() {
		Init();

		
		//EmitSignal("healthUpdated", this.health);
		EmitSignal("changeState", Enums.PlayerState.IDLE);
	}

	private void Init() {

	}
	public override void _PhysicsProcess(float delta)
	{

		Move(delta);	

	}


	private void Move(float delta) {
		var direction = Vector2.Zero;
		if (this.state.currentState != Enums.PlayerState.HURT) {
			if (Input.IsActionPressed("Right") && !Input.IsActionPressed("Left")) {
				
				EmitSignal("changeState", Enums.PlayerState.RUN);
				direction.x = 1;
			} else if (Input.IsActionPressed("Left") && !Input.IsActionPressed("Right")) {

				
				EmitSignal("changeState", Enums.PlayerState.RUN);
				direction.x = -1;

			} else {
				
				EmitSignal("changeState", Enums.PlayerState.IDLE);
				direction.x = 0;
			}

			if (Input.IsActionPressed("Jump") && !Input.IsActionPressed("Down")) {
				EmitSignal("changeState", Enums.PlayerState.RUN);
				direction.y = -1;

			} else if (Input.IsActionPressed("Down") && !Input.IsActionPressed("Jump")) {
				EmitSignal("changeState", Enums.PlayerState.RUN);
				direction.y = 1;

			} else {
				EmitSignal("changeState", Enums.PlayerState.IDLE);
				direction.y = 0;
			}
		}

		if (direction.Length() > 0 ) {
			velocity.x = Mathf.Lerp(velocity.x, direction.Normalized().x * speed, this.acceleration);
			velocity.y = Mathf.Lerp(velocity.y, direction.Normalized().y * speed, this.acceleration);
		} else {
			velocity.x = Mathf.Lerp(velocity.x, 0, this.friction);
			velocity.y = Mathf.Lerp(velocity.y, 0, this.friction);
		}
		
		MoveAndSlide(velocity);
	}

	private void OnChangeState(Enums.PlayerState state)
	{

		if (this.state.currentState == Enums.PlayerState.DEAD) {
			return;
		}

		this.state.nexState = state; //Jump // falling // moving // moving 
		
		// dont change the prevState if the nextState = the currentState
		if (this.state.nexState != this.state.currentState)
		{
			this.state.pervState = this.state.currentState; // idle // jump //fall //moving
		}
		this.state.currentState = this.state.nexState; //jump  // fall // moving // moving
		//PlayAnimation();
		//PlaySounds();
	}
}
