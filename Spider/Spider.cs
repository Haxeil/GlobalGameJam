using Godot;
using System;

public class Spider : KinematicBody2D
{

	private RayCast2D lowMidCheck;
	private RayCast2D HighMidCheck;

	private RayCast2D FrontCheck;
	private RayCast2D BackCheck;

	private Node2D frontLegs;
	private Node2D backLegs;
	[Export] private float xSpeed = 20.0f;
	[Export] private float ySpeed = 40.0f;

	[Export] private float step_rate = 0.6f;
	private float timeSinceLastStep = 0.0f;
	private int curFLeg = 0;
	private int curBLeg = 0;
	private bool useFront = false;
	public bool canMove = true;
	private Timer nextPosTimer;
	private Position2D nextPos;
	private Timer legAttackTimer;
	public override void _Ready()
	{
		lowMidCheck = GetNode<RayCast2D>("LowMidCheck");
		HighMidCheck = GetNode<RayCast2D>("HighMidCheck");
		FrontCheck = GetNode<RayCast2D>("FrontCheck");
		BackCheck = GetNode<RayCast2D>("BackCheck");
		frontLegs = GetNode<Node2D>("FrontLegs");
		backLegs = GetNode<Node2D>("BackLegs");
		nextPosTimer = GetNode<Timer>("NextPosTimer");
		nextPos = GetNode<Position2D>("NextPos");
		legAttackTimer = GetNode<Timer>("LegAttack");
		this.FrontCheck.ForceRaycastUpdate();
		this.BackCheck.ForceRaycastUpdate();

		for (int i = 0; i < 8; i++) {
			Step();
		}
	}

	public override void _PhysicsProcess(float delta) {
		var moveVec = new Vector2(this.xSpeed, 0);

		if (this.HighMidCheck.IsColliding()) {
			moveVec.y = -ySpeed;

		} else if (!this.lowMidCheck.IsColliding()) {
			moveVec.y = ySpeed;
		}
		if (canMove) {
			MoveAndSlide(moveVec);
		}
		
	}

 
	public override void _Process(float delta) {
		this.timeSinceLastStep += delta;
		if (this.timeSinceLastStep >= step_rate) {
			this.timeSinceLastStep = 0.0f;
			Step();
		}

	}

 
	private void Step() {

		Leg leg = null;
		RayCast2D sensor = null;

		if (this.useFront) {

			leg = (Leg)this.frontLegs.GetChildren()[curFLeg];
			this.curFLeg += 1;
			curFLeg %= frontLegs.GetChildren().Count;
			sensor = this.FrontCheck;
		} else {
			leg = (Leg)this.backLegs.GetChildren()[curBLeg];
			curBLeg += 1;
			curBLeg %= this.backLegs.GetChildren().Count;
			sensor = this.BackCheck;

		}
		useFront = !useFront;
	
		var target = sensor.GetCollisionPoint();
		leg.Step(target);
	}

	//signal
	private async void CheckPersonBodyEntered(Player person) 
	{
		//person.SetDeferred("set_physics_process", false);
		person.SetPhysicsProcess(false);
		person.velocity = Vector2.Zero;
		canMove = false;
		person.spider = this;
		nextPosTimer.Start();
		for (int i = 1; i <= 3; i++) {
			Leg leg = (Leg)GetNode<Node2D>("FrontLegs").GetChildren()[i];
			leg.goalPos = person.GlobalPosition;

		}

		await ToSignal(nextPosTimer, "timeout");
		
		for (int i = 1; i <= 3; i++) {
			Leg leg = (Leg)GetNode<Node2D>("FrontLegs").GetChildren()[i];
			leg.goalPos = this.nextPos.GlobalPosition;
		}
		

	}



}

