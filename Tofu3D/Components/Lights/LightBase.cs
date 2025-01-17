public class LightBase : Component
{
    public Color Color = Color.White;

    public float Intensity = 1;

    protected float _cameraBeforeTransformationFarPlaneDistance;
    protected bool _cameraBeforeTransformationIsOrthographic;
    protected float _cameraBeforeTransformationNearPlaneDistance;
    protected float _cameraBeforeTransformationOrthographicSize;
    protected Vector3 _cameraBeforeTransformationRotation;
    protected Vector2 _cameraBeforeTransformationSize;
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