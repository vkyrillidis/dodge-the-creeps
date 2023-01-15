using Godot;

public class Player : Area2D
{
	[Signal]
	public delegate void Hit();

	[Export]
	public int speed = 400; // How fast the player will move (pixels/sec).

	public Vector2 screenSize; // Size of the game window.

	[Export]
	public int shieldUpTime = 2;
	[Export]
	public int shieldCooldown = 10;
	public bool shieldUsable = true;

	public override void _Ready()
	{
		screenSize = GetViewportRect().Size;
		Hide();
	}

	public override void _Process(float delta)
	{
		var velocity = Vector2.Zero; // The player's movement vector.

		if (Input.IsActionPressed("move_right"))
		{
			velocity.x += 1;
		}

		if (Input.IsActionPressed("move_left"))
		{
			velocity.x -= 1;
		}

		if (Input.IsActionPressed("move_down"))
		{
			velocity.y += 1;
		}

		if (Input.IsActionPressed("move_up"))
		{
			velocity.y -= 1;
		}

		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * speed;
			animatedSprite.Play();
		}
		else
		{
			animatedSprite.Stop();
		}

		Position += velocity * delta;
		Position = new Vector2(
			x: Mathf.Clamp(Position.x, 0, screenSize.x),
			y: Mathf.Clamp(Position.y, 0, screenSize.y)
		);

		if (velocity.x != 0)
		{
			animatedSprite.Animation = "right";
			// See the note below about boolean assignment.
			animatedSprite.FlipH = velocity.x < 0;
			animatedSprite.FlipV = false;
		}
		else if (velocity.y != 0)
		{
			animatedSprite.Animation = "up";
			animatedSprite.FlipV = velocity.y > 0;
		}
		
		if (Input.IsActionJustPressed("space"))
		{
			if (shieldUsable)
			{
				GD.Print("Using shield");
				useShield();
			}
		}
	}

	public void Start(Vector2 pos)
	{
		Position = pos;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}

	public void OnPlayerBodyEntered(PhysicsBody2D body)
	{
		Hide(); // Player disappears after being hit.
		EmitSignal(nameof(Hit));
		// Must be deferred as we can't change physics properties on a physics callback.
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
	}
	
	public void useShield() 
	{
		shieldUsable = false;
		// Show the shield
		var shieldSprite = GetNode<AnimatedSprite>("ShieldAnimatedSprite");
		shieldSprite.Visible = true;

		// Disable the collision shape of the player
		var playerCollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		playerCollisionShape.Disabled = true;

		// Fire timer shield uptime
		var shieldUptimeTimer = GetNode<Timer>("ShieldUptimeTimer");
		shieldUptimeTimer.OneShot = true;
		shieldUptimeTimer.WaitTime = shieldUpTime;
		shieldUptimeTimer.Start();

		// Fire timer shieldCooldownTimer
		var shieldCooldownTimer = GetNode<Timer>("ShieldCooldownTimer");
		shieldCooldownTimer.OneShot = true;
		shieldCooldownTimer.WaitTime = shieldCooldown;
		shieldCooldownTimer.Start();
	}

	private void _on_ShieldCooldownTimer_timeout()
	{
		GD.Print("ShieldCooldownTimer called");
		shieldUsable = true;
	}

	private void _on_ShieldUptimeTimer_timeout()
	{
		GD.Print("ShieldUptimeTimer called");
		var playerCollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		playerCollisionShape.Disabled = false;
		var shieldSprite = GetNode<AnimatedSprite>("ShieldAnimatedSprite");
		shieldSprite.Visible = false;
	}
}
