public class LightBase : Component
{
    [Color3]
    public Color Color = Color.White;

    public float Intensity = 1;

    [Hide]
    protected float _cameraBeforeTransformationFarPlaneDistance;

    [Hide]
    protected bool _cameraBeforeTransformationIsOrthographic;

    [Hide]
    protected float _cameraBeforeTransformationNearPlaneDistance;

    [Hide]
    protected float _cameraBeforeTransformationOrthographicSize;

    [Hide]
    protected Vector3 _cameraBeforeTransformationRotation;

    [Hide]
    protected Vector2 _cameraBeforeTransformationSize;

    [Hide]
    protected Vector3 _cameraBeforeTransformationWorldPosition;

    protected void ConfigureCameraForSceneRender()
    {
        Camera.GameViewCamera.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
        Camera.GameViewCamera.OrthographicSize = _cameraBeforeTransformationOrthographicSize;
        Camera.GameViewCamera.Size = _cameraBeforeTransformationSize;
        Camera.GameViewCamera.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
        Camera.GameViewCamera.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
        Camera.GameViewCamera.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
        Camera.GameViewCamera.Transform.Rotation = _cameraBeforeTransformationRotation;
        Camera.GameViewCamera.UpdateMatrices();
    }
}