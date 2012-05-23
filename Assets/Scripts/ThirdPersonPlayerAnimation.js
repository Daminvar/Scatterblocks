var runSpeedScale = 1.0;
var walkSpeedScale = 1.0;

function Start ()
{
	// By default loop all animations
	animation.wrapMode = WrapMode.Loop;

	animation["run"].layer = -1;
	animation["walk"].layer = -1;
	animation["strafe"].layer = -1;
	animation["idle"].layer = -2;
	animation.SyncLayer(-1);

	animation["ledgefall"].layer = 9;	
	animation["ledgefall"].wrapMode = WrapMode.Loop;
	
	animation["strafe"].speed = 1.8;

	// The jump animation is clamped and overrides all others
	animation["jump"].layer = 10;
	animation["jump"].wrapMode = WrapMode.ClampForever;
	animation["spinJump"].layer = 10;
	animation["spinJump"].wrapMode = WrapMode.ClampForever;

	animation["jumpfall"].layer = 10;	
	animation["jumpfall"].wrapMode = WrapMode.ClampForever;

	// This is the jet-pack controlled descent animation.
	animation["jetpackjump"].layer = 10;	
	animation["jetpackjump"].wrapMode = WrapMode.ClampForever;

	animation["jumpland"].layer = 10;	
	animation["jumpland"].wrapMode = WrapMode.Once;

	animation["walljump"].layer = 11;	
	animation["walljump"].wrapMode = WrapMode.Once;

	// we actually use this as a "got hit" animation
	animation["buttstomp"].speed = 0.15;
	animation["buttstomp"].layer = 20;
	animation["buttstomp"].wrapMode = WrapMode.Once;	
	var punch = animation["punch"];
	punch.wrapMode = WrapMode.Once;

	// We are in full control here - don't let any other animations play when we start
	animation.Stop();
	animation.Play("idle");
}

function Update ()
{
	var playerController : ThirdPersonController = GetComponent(ThirdPersonController);
	var currentSpeed = playerController.GetSpeed();

	// Fade in run
	if (currentSpeed > playerController.walkSpeed)
	{
		if ( Mathf.Abs(playerController.GetInputDirection().x) > 0.1)
		{
			
			animation.CrossFade("strafe");	
		
			if ( Mathf.Abs(playerController.GetInputDirection().z) > 0.3)
			{
				animation.Blend("run", 0.4, 0.0);
			}
		}
		else
		{
			if ( Mathf.Abs(playerController.GetInputDirection().z) > 0.3)
			{
				animation.CrossFade("run");
			}
		}
		
		// We fade out jumpland quick otherwise we get sliding feet
		animation.Blend("jumpland", 0.2);

	}
	// Fade in walk
	else if (currentSpeed > 0.1)
	{
		animation.CrossFade("walk");
		// We fade out jumpland realy quick otherwise we get sliding feet
		animation.Blend("jumpland", 0);
	}
	// Fade out walk and run
	else
	{
		animation.Blend("walk", 0.0, 0.3);
		animation.Blend("run", 0.0, 0.3);
		animation.Blend("run", 0.0, 0.3);
	}
	
	animation["run"].normalizedSpeed = runSpeedScale;
	animation["walk"].normalizedSpeed = walkSpeedScale;
	
	if (playerController.IsJumping ())
	{
		//if (playerController.HasJumpReachedApex ())
		//{
			//animation.CrossFade ("jumpfall", 0.2);
		//}
		//else
		//{
			animation.CrossFade ("spinJump", 0.0);
		//}
	}
	
	/*
	// We fell down somewhere
	else if (!playerController.IsGrounded())
	{
		animation.CrossFade ("ledgefall", 0.2);
	}
	// We are not falling down anymore
	else
	{
		animation.Blend ("ledgefall", 0.0, 0.2);
	}
	*/
}

function DidLand () {
	animation.Play("jumpland");
}

function DidButtStomp () {
	animation.CrossFade("buttstomp", 0.1);
	animation.CrossFadeQueued("jumpland", 0.2);
}

function Slam () {
	animation.CrossFade("buttstomp", 0.2);
	var playerController : ThirdPersonController = GetComponent(ThirdPersonController);
	while(!playerController.IsGrounded())
	{
		yield;	
	}
	animation.Blend("buttstomp", 0, 0);
}


function DidWallJump ()
{
	// Wall jump animation is played without fade.
	// We are turning the character controller 180 degrees around when doing a wall jump so the animation accounts for that.
	// But we really have to make sure that the animation is in full control so 
	// that we don't do weird blends between 180 degree apart rotations
	animation.Play ("walljump");
}

function MakeIdle() {
	animation.Stop();
	animation.Play("idle");
}

@script AddComponentMenu ("Third Person Player/Third Person Player Animation")