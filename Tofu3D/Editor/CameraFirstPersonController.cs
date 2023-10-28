public class PositionConstraint : Component, IComponentUpdateable
{
    public float PosY = 0;
    private float _jumpY = 0;
    private bool _jumping = false;
    private float _jumpProgress = 0;
    private Vector3 _lastFramePosition = Vector3.Zero;
    private float _positionDelta = 0;

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
        if (_jumping == false) _positionDelta += Vector3.Distance(Transform.WorldPosition, _lastFramePosition);

        if (KeyboardInput.IsKeyDown(Keys.Space) && _jumping == false) _jumping = true;

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

        float wobble = (float)Math.Sin(_positionDelta / 4) + (float)Math.Cos(_positionDelta / 4);
        float rotationWobble = (float)Math.Sin(_positionDelta / 8) + (float)Math.Cos(_positionDelta / 8);
        Transform.WorldPosition = Transform.WorldPosition.Set(y: PosY + wobble + _jumpY);


        Transform.Rotation = Transform.Rotation.Set(z: rotationWobble * 2);

        _lastFramePosition = Transform.WorldPosition;
        base.Update();
    }
}