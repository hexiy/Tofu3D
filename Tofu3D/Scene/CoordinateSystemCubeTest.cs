[ExecuteInEditMode]
public class CoordinateSystemCubeTest : Component, IComponentUpdateable
{
	[Show]
	public GameObject CubeForward;
	[Show]
	public GameObject CubeUp;
	[Show]
	public GameObject CubeLoopTarget;
	[Show]
	public Vector3 MoveLoopTarget = new Vector3(0, 0, 5);
	Vector3 _startingPosition;
	[Show]
	public float MoveSpeed = 1;

	public override void Awake()
	{
		_startingPosition = Transform.WorldPosition;
		base.Awake();
	}

	public override void OnDestroyed()
	{
		Transform.WorldPosition = _startingPosition;
		base.OnDestroyed();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		Vector3 endPosition = _startingPosition + Transform.TransformVectorToWorldSpaceVector(MoveLoopTarget) * 5;
		Transform.WorldPosition = Vector3.Lerp(_startingPosition, endPosition, (float) Math.Abs(Math.Cos(Time.EditorElapsedTime * MoveSpeed)));

		if (CubeForward != null)
		{
			Vector3 cubeForwardPosition = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 2));
			CubeForward.Transform.WorldPosition = Transform.WorldPosition + cubeForwardPosition;
		}

		if (CubeUp != null)
		{
			Vector3 cubeUpPosition = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 2, 0));
			CubeUp.Transform.WorldPosition = Transform.WorldPosition + cubeUpPosition;
		}

		if (CubeLoopTarget != null)
		{
			CubeLoopTarget.Transform.WorldPosition = endPosition;
		}

		base.Update();
	}
}