using System;
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	[Header("Jump parametrs")]
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	
	[Header("Movement parametrs")]
	public float MoveSpeed = 6;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .2f;

	[Header("Wall climbing parametrs")]
	public Vector2 wallJumpClimb;
	public Vector2 wallLeap;
	public float wallSlideSpeedMax = 3;
	
	[Header("Dash parametrs")]
	public float DashForce;
	public float DashDuration;
	public int MaxDashCount = 1;
	
	private float _currenDashDuration;
	private bool _isDashing = false;
	private Vector2 _direction;
	private int _dashCount = 0;
	
	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private Vector3 velocity;
	private float velocityXSmoothing;

	private Controller2D controller;

	private Vector2 directionalInput;
	private bool wallSliding;
	private int wallDirX;
	
	private float jumpDuration = 0;
	
	void Start() 
	{
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		wallSlideSpeedMax = gravity + wallSlideSpeedMax;
	}

	void Update() 
	{
//		if (!_isDashing)
		{
			CalculateVelocity();
		}
		if(_isDashing)
		{
			_currenDashDuration -= Time.deltaTime;

			if (_currenDashDuration <= 0)
			{
				_isDashing = false;
//				velocity = Vector2.zero;
			}
			else
			{
				velocity = _direction * DashForce;
			}
		}

		HandleWallSliding ();

		controller.Move (velocity * Time.deltaTime, directionalInput);
		
		if (controller.collisions.above || controller.collisions.below) 
		{
			if (controller.collisions.slidingDownMaxSlope) 
			{
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} 
			else 
			{
				velocity.y = 0;
			}
		}

		if (controller.collisions.below)
		{
			_dashCount = 0;
		}
	}

	public void SetDirectionalInput (Vector2 input) 
	{
		directionalInput = input;
	}

	private bool CompareSognOfNumber(float num1, float num2)
	{
		if (num1 > 0 && num2 > 0)
		{
			return true;
		}
		else
		{
			if (num1 < 0 && num2 < 0)
			{
				return true;
			}
		}

		return false;
	}

	public void OnJumpInputDown() 
	{
		if (wallSliding)
		{
			if (CompareSognOfNumber(wallDirX, directionalInput.x)) 
			{				
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			else
			{				
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
			}
		}
		if (controller.collisions.below)
		{
			if (controller.collisions.slidingDownMaxSlope)
			{
				if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) 
				{ // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else
			{
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnDashInputDown(float horizontalInput, float verticalInput)
	{
		if (_dashCount < MaxDashCount)
		{
			_direction = new Vector2(horizontalInput, verticalInput);
			_direction.Normalize();
			_dashCount++;
                
			_currenDashDuration = DashDuration; 
			_isDashing = true;	
		}
	}

	public void OnJumpInputUp()
	{
		if (velocity.y > minJumpVelocity) 
		{
			velocity.y = minJumpVelocity;
		}
	}
		

	void HandleWallSliding() 
	{
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right)) 
		{
			wallSliding = true;

			if (velocity.y < wallSlideSpeedMax) 
			{
				velocity.y = wallSlideSpeedMax;
			}
		}
	}

	void CalculateVelocity() 
	{
		float targetVelocityX = directionalInput.x * MoveSpeed;
		
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, 
				(controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
//		if (!isJump)
		{
			velocity.y += gravity * Time.deltaTime;
		}
	}
}
