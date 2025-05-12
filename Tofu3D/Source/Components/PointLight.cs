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
        _cameraBeforeTransformationWorldPosition = Camera.CurrentlyRenderingCamera.Transform.WorldPosition;
        _cameraBeforeTransformationRotation = Camera.CurrentlyRenderingCamera.Transform.Rotation;
        _cameraBeforeTransformationIsOrthographic = Camera.CurrentlyRenderingCamera.IsOrthographic;
        _cameraBeforeTransformationOrthographicSize = Camera.CurrentlyRenderingCamera.OrthographicSize;
        _cameraBeforeTransformationSize = Camera.CurrentlyRenderingCamera.Size;
        _cameraBeforeTransformationNearPlaneDistance = Camera.CurrentlyRenderingCamera.NearPlaneDistance;
        _cameraBeforeTransformationFarPlaneDistance = Camera.CurrentlyRenderingCamera.FarPlaneDistance;

        Camera.CurrentlyRenderingCamera.IsOrthographic = false;
        Camera.CurrentlyRenderingCamera.FieldOfView = 90;
        Camera.CurrentlyRenderingCamera.Transform.WorldPosition =
            Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
        Camera.CurrentlyRenderingCamera.Transform.Rotation = Transform.Rotation;
        Camera.CurrentlyRenderingCamera.Size = new Vector2(2048, 2048);
        Camera.CurrentlyRenderingCamera.NearPlaneDistance = 0.1f;
        Camera.CurrentlyRenderingCamera.FarPlaneDistance = Radius;
        Camera.CurrentlyRenderingCamera.UpdateMatrices();

        LightSpaceViewProjectionMatrix = Camera.CurrentlyRenderingCamera.GetLightViewMatrix() *
        Camera.CurrentlyRenderingCamera.GetPerspectiveProjectionMatrix();
    }
}