[ExecuteInEditMode]
public class PointLight : LightBase
{
    public float Radius = 10;
    public static Matrix4x4 LightSpaceViewProjectionMatrix { get; private set; } = Matrix4x4.Identity;

    
    // public void RenderPointLightShadowDepth()
    // {
    //     for (int face = 0; face < 6; face++)
    //     {
    //         ConfigureCameraForShadowMapping(face);
    //         Tofu.SceneManager.CurrentScene.RenderOpaques();
    //         Tofu.SceneManager.CurrentScene.RenderTransparency();
    //
    //     }
    //    
    //     ConfigureCameraForSceneRender();
    // }

    private void ConfigureCameraForShadowMapping(int cubemapFaceIndex)
    {
        _cameraBeforeTransformationWorldPosition = Camera.GameViewCamera.Transform.WorldPosition;
        _cameraBeforeTransformationRotation = Camera.GameViewCamera.Transform.Rotation;
        _cameraBeforeTransformationIsOrthographic = Camera.GameViewCamera.IsOrthographic;
        _cameraBeforeTransformationOrthographicSize = Camera.GameViewCamera.OrthographicSize;
        _cameraBeforeTransformationSize = Camera.GameViewCamera.Size;
        _cameraBeforeTransformationNearPlaneDistance = Camera.GameViewCamera.NearPlaneDistance;
        _cameraBeforeTransformationFarPlaneDistance = Camera.GameViewCamera.FarPlaneDistance;

        Camera.GameViewCamera.IsOrthographic = false;
        Camera.GameViewCamera.FieldOfView = 90;
        Camera.GameViewCamera.Transform.WorldPosition =
            Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
        Camera.GameViewCamera.Transform.Rotation = Transform.Rotation;
        Camera.GameViewCamera.Size = new Vector2(2048, 2048);
        Camera.GameViewCamera.NearPlaneDistance = 0.1f;
        Camera.GameViewCamera.FarPlaneDistance = Radius;
        Camera.GameViewCamera.UpdateMatrices();

        LightSpaceViewProjectionMatrix = Camera.GameViewCamera.GetLightViewMatrix() *
                                         Camera.GameViewCamera.GetPerspectiveProjectionMatrix();
    }
}