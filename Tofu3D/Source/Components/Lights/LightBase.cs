public class LightBase : Component
{
    [Color3Attrib]
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
        Camera.MainCamera.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
        Camera.MainCamera.OrthographicSize = _cameraBeforeTransformationOrthographicSize;
        Camera.MainCamera.Size = _cameraBeforeTransformationSize;
        Camera.MainCamera.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
        Camera.MainCamera.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
        Camera.MainCamera.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
        Camera.MainCamera.Transform.Rotation = _cameraBeforeTransformationRotation;
        Camera.MainCamera.UpdateMatrices();
    }
}