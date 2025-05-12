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
        Camera.CurrentlyRenderingCamera.IsOrthographic = _cameraBeforeTransformationIsOrthographic;
        Camera.CurrentlyRenderingCamera.OrthographicSize = _cameraBeforeTransformationOrthographicSize;
        Camera.CurrentlyRenderingCamera.Size = _cameraBeforeTransformationSize;
        Camera.CurrentlyRenderingCamera.NearPlaneDistance = _cameraBeforeTransformationNearPlaneDistance;
        Camera.CurrentlyRenderingCamera.FarPlaneDistance = _cameraBeforeTransformationFarPlaneDistance;
        Camera.CurrentlyRenderingCamera.Transform.WorldPosition = _cameraBeforeTransformationWorldPosition;
        Camera.CurrentlyRenderingCamera.Transform.Rotation = _cameraBeforeTransformationRotation;
        Camera.CurrentlyRenderingCamera.UpdateMatrices();
    }
}