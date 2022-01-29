using Godot;
using System;

public class Player : KinematicBody2D
{
	
	[Export] public Vector2 velocity = Vector2.Zero;
	[Export] private float speed = 50.0f;
	[Export] private float friction = 0.1f;
	[Export] private float jumpPower = 200;
	[Export] private float acceleration = 0.1f;
	[Export] private float gravity = 20;
	private int direction = 1;
	private Timer cayoteWallJump;
	private Node2D Floor;
	private Node2D Celling;
	private Timer cayoteTimer;
	[Export] public float maxHealth = 100;
	[Export] public float health;
 	[Signal] public delegate void changeState(StateMachine state);
	[Signal] public delegate void healthUpdated(float health);
	[Signal] public delegate void maxHealthUpdated(float maxHealth);
	StateMachine state = new StateMachine();

	private bool isOnGround = false;
	public bool canMove = true;

	public override void _Ready() {
		Init();
		this.health = this.maxHealth;
		EmitSignal("maxHealthUpdated", maxHealth);
		
		//EmitSignal("healthUpdated", this.health);
		EmitSignal("changeState", Enums.PlayerState.IDLE);
	}

	private void Init() {
		Floor = GetNode<Node2D>("Floor");
		Celling = GetNode<Node2D>("Celling");
		cayoteTimer = GetNode<Timer>("CayoteTime");
		RayException();
	}
	public override void _PhysicsProcess(float delta)
	{
		ApplyGravity();
		Jump();
		Move(delta);	

	}

	public void ApplyGravity() {
		if (RayCheckOnCelling()) {
			velocity.y = 0;
			velocity.y = gravity * 5;
		}


		if (this.isOnGround) {
			velocity.y = 0;
		} else {
			velocity.y += this.gravity;

			if (this.velocity.y != 0 && this.velocity.y > this.gravity && this.velocity.y > -this.jumpPower/this.gravity)
			{
				EmitSignal("changeState", Enums.PlayerState.FALL);
			}
		}
		
	}

	private void Move(float delta) {
		
		if (canMove && this.state.currentState != Enums.PlayerState.HURT) {
			if (Input.IsActionPressed("Right") && !Input.IsActionPressed("Left")) {
				
				EmitSignal("changeState", Enums.PlayerState.RUN);
				
				direction = 1;
				velocity.x = Mathf.Lerp(velocity.x, speed * direction, this.acceleration);
			} else if (Input.IsActionPressed("Left") && !Input.IsActionPressed("Right")) {

				
				EmitSignal("changeState", Enums.PlayerState.RUN);

				
				direction = -1;
				velocity.x = Mathf.Lerp(velocity.x, speed * direction, this.acceleration);

			} else {
				
				EmitSignal("changeState", Enums.PlayerState.IDLE);
				
				velocity.x = Mathf.Lerp(velocity.x, 0, this.friction);
			}
		}

		bool wasOnFloor = this.isOnGround;

		MoveAndSlide(velocity, Vector2.Up, false, 4, -Mathf.Pi/4, false);

		this.isOnGround = RayCheckOnFloor();

		

		if (!this.isOnGround && wasOnFloor)
		{
			cayoteTimer.Start();
		}	
	}

	private void Jump() {


		if (Input.IsActionPressed("Jump")) {
			if ((this.canMove && this.isOnGround) || !this.cayoteTimer.IsStopped()) {
				//ApplyDamage(25);
				cayoteTimer.Stop();
				velocity.y = -jumpPower;

				EmitSignal("changeState", Enums.PlayerState.JUMP);
				

			}

		}

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

	public void SetHealth(float value) {
		var prevHealth = this.health;
		this.health = Mathf.Clamp(value, 0, this.maxHealth);
		if (prevHealth != this.health)
		{	
			EmitSignal("healthUpdated", this.health);
			if (this.health == 0)
			{
				//TODO Die();
				EmitSignal("killed", this);
			}
		}
	}
	//checking if player grounded by RayCasting rather than the Default IsOnfloor()
	private bool RayCheckOnFloor() {
		var isColliding = false;
		foreach (RayCast2D ray in this.Floor.GetChildren()) {
			if (ray.IsColliding()) {
				isColliding = true;
			}
		}
		return isColliding;

	}

	private bool RayCheckOnCelling() {
		var isColliding = false;
		foreach (RayCast2D ray in this.Celling.GetChildren()) {
			if (ray.IsColliding()) {
				isColliding = true;
			}
		}

		return isColliding;

	}
	//To exclude Player From Collision check 
	private void RayException() {
		foreach (RayCast2D ray in this.Celling.GetChildren()) {
			ray.AddException(this);
		}
		foreach (RayCast2D ray in this.Floor.GetChildren()) {
			ray.AddException(this);
		}
	}
}
