public class PositionConstraint : Component
{
	public float PosY = 0;
	float _jumpY = 0;
	bool _jumping = false;
	float _jumpProgress = 0;
	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		if (KeyboardInput.WasKeyJustPressed(Keys.Space))
		{
			_jumping = true;
		}

		if (_jumping)
		{
			_jumpProgress += Time.DeltaTime;
			_jumpY = (float)Math.Sin(Mathf.Pi * _jumpProgress) * 35;
		}

		if (_jumpProgress >= 1)
		{
			_jumping = false;
			_jumpProgress = 0;
		}
		
		float wobble = (float) Math.Sin(Transform.WorldPosition.X / 4) + (float) Math.Cos(Transform.WorldPosition.Z / 4);
		float rotationWobble = (float) Math.Sin(Transform.WorldPosition.X / 8) + (float) Math.Cos(Transform.WorldPosition.Z / 8);
		Transform.WorldPosition = Transform.WorldPosition.Set(y: PosY + wobble - _jumpY);
		Transform.Rotation = Transform.Rotation.Set(z: rotationWobble * 2);
		base.Update();
	}
}