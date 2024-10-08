[ExecuteInEditMode]
public class CoordinateSystemCubeTest : Component, IComponentUpdateable
{
    private Vector3 _startingPosition;

    [Show] public GameObject CubeForward;

    [Show] public GameObject CubeLoopTarget;

    [Show] public GameObject CubeUp;

    [Show] public Vector3 MoveLoopTarget = new(0, 0, 5);

    [Show] public float MoveSpeed = 1;

    public void Update()
    {
        var endPosition = _startingPosition + Transform.TransformVectorToWorldSpaceVector(MoveLoopTarget) * 5;
        Transform.WorldPosition = Vector3.Lerp(_startingPosition, endPosition,
            (float)Math.Abs(Math.Cos(Time.EditorElapsedTime * MoveSpeed)));

        if (CubeForward != null)
        {
            var cubeForwardPosition = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 2));
            CubeForward.Transform.WorldPosition = Transform.WorldPosition + cubeForwardPosition;
        }

        if (CubeUp != null)
        {
            var cubeUpPosition = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 2, 0));
            CubeUp.Transform.WorldPosition = Transform.WorldPosition + cubeUpPosition;
        }

        if (CubeLoopTarget != null)
        {
            CubeLoopTarget.Transform.WorldPosition = endPosition;
        }
    }

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
}