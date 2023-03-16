namespace Scripts;

public class CharacterController : Component
{
	public float JumpForce = 10000;
	bool _jumpKeyDown;
	public float MoveSpeed = 10;

	// LINKABLECOMPONENT PURGE //[LinkableComponent]
	Rigidbody _rb;

	public override void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		base.Awake();
	}

	public override void FixedUpdate()
	{
		if (_rb == null)
		{
			return;
		}

		Vector2 input = Vector2.Zero;
		if (KeyboardInput.IsKeyDown(Keys.A))
		{
			input.X = -MoveSpeed;
		}

		if (KeyboardInput.IsKeyDown(Keys.D))
		{
			input.X = MoveSpeed;
		}

		if (_jumpKeyDown == false && KeyboardInput.IsKeyDown(Keys.W))
		{
			//rb.body.ApplyForce(new Vector2(0, -JumpForce));
		}

		_jumpKeyDown = KeyboardInput.IsKeyDown(Keys.W);
		//rb.body.ApplyForce(new Vector2(input.X, 0));
		base.Update();
	}
}