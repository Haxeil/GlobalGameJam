using Godot;
using System;

public class Leg : Position2D
{
 
	private int MIN_DIST = 100;
 
	private Position2D joint1;
	private Position2D joint2;
	private Position2D hand;
 
	private float lenUpper = 0.0f;
	private float lenMiddle = 0.0f;
	private float lenLower = 0.0f;
 
	[Export] bool flipped = true;
 
	public Vector2 goalPos = Vector2.Zero;

	private Vector2 intPos = Vector2.Zero;
	private Vector2 startPos = Vector2.Zero;
	private float stepHeight = 40.0f;
	private float stepRate = 0.5f;
	private float stepTime = 0.0f;
	public Vector2 targetPos = Vector2.Zero;
	public override void _Ready() {
		joint1 = GetNode<Position2D>("Joint1");
		joint2 = GetNode<Position2D>("Joint1/Joint2");
		hand = GetNode<Position2D>("Joint1/Joint2/Hand");

		
		lenUpper = joint1.Position.x;
		lenMiddle = joint2.Position.x;
		lenLower = hand.Position.x;
	
		if (!flipped) {
			GetNode<Sprite>("Sprite").FlipH = true;
			
			joint1.GetNode<Sprite>("Sprite").FlipH = true;
			joint2.GetNode<Sprite>("Sprite").FlipH = true;
		}

	}

 
	public void Step(Vector2 gPos) {
		if (this.goalPos == gPos) {
			return;
		}
			
	
		this.goalPos = gPos;
		var handPos = this.hand.GlobalPosition;
	
		var highest = this.goalPos.y;
		if (handPos.y < highest) {
			highest = handPos.y;
		}
		   
	
		var mid = (this.goalPos.x + handPos.x) / 2.0;
		startPos = handPos;
		intPos = new Vector2(((float)mid), highest - this.stepHeight);
		stepTime = 0.0f;
	
	}

	public override void _Process(float delta) {
		
		if (true) {
			this.stepTime += delta;
			
			var time = this.stepTime / this.stepRate;

			if (time < 0.5) {
				targetPos = this.startPos.LinearInterpolate(this.intPos, time / 0.5f);

			}
			else if (time < 1.0) {
				targetPos = this.intPos.LinearInterpolate(this.GlobalPosition, (time - 0.5f) / 0.5f);
			}
			else {
				targetPos = this.goalPos;

			}
			UpdateIk(targetPos);

		}

 
	}
	public void UpdateIk(Vector2 targetPos) {
		var offset = targetPos - this.GlobalPosition;
		var disToTarget = offset.Length();

		if (disToTarget < MIN_DIST) {
			offset = (offset / disToTarget) * MIN_DIST;
			disToTarget = MIN_DIST;
		}
	
		var baseR = offset.Angle();
		var lenTotal = this.lenUpper + this.lenMiddle + this.lenLower;
		var lenDummySide = (lenUpper + lenMiddle) * Mathf.Clamp(disToTarget / lenTotal, 0.0f, 1.0f);
		
		var baseAngles = SSSCalc(lenDummySide, this.lenLower, disToTarget);
		var nextAngles = SSSCalc(this.lenUpper, this.lenMiddle, lenDummySide);

		GlobalRotation = baseAngles["B"] + nextAngles["B"] + baseR;
		joint1.Rotation = nextAngles["C"];
		joint2.Rotation = baseAngles["C"] + nextAngles["A"];
	
	}

	private Godot.Collections.Dictionary<String, float> SSSCalc(float sideA, float sideB, float sideC) {
		if (sideC >= sideA + sideB) {
			var anglesCol = new Godot.Collections.Dictionary<String, float>();
			anglesCol.Add("A", 0);
			anglesCol.Add("B", 0);
			anglesCol.Add("C", 0);
			return anglesCol;

		}
		var angleA = LawOfCos(sideB, sideC, sideA);
		var angleB = LawOfCos(sideC, sideA, sideB) + Mathf.Pi;
		var angleC = Mathf.Pi - angleA - angleB;
	
		if (this.flipped) {
			angleA = -angleA;
			angleB = -angleB;
			angleC = -angleC;
		}

		var angles = new Godot.Collections.Dictionary<String, float>();
		angles.Add("A", angleA);
		angles.Add("B", angleB);
		angles.Add("C", angleC);

		return angles;
	

	}

	private float LawOfCos(float a, float b, float c) {
		if (2 * a * b == 0) {
			return 0;
		}
		return Mathf.Acos((a * a + b * b - c * c) / ( 2 * a * b) );
	}

}
