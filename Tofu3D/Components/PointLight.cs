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
        _cameraBeforeTransformationWorldPosition = Camera.MainCamera.Transform.WorldPosition;
        _cameraBeforeTransformationRotation = Camera.MainCamera.Transform.Rotation;
        _cameraBeforeTransformationIsOrthographic = Camera.MainCamera.IsOrthographic;
        _cameraBeforeTransformationOrthographicSize = Camera.MainCamera.OrthographicSize;
        _cameraBeforeTransformationSize = Camera.MainCamera.Size;
        _cameraBeforeTransformationNearPlaneDistance = Camera.MainCamera.NearPlaneDistance;
        _cameraBeforeTransformationFarPlaneDistance = Camera.MainCamera.FarPlaneDistance;

        Camera.MainCamera.IsOrthographic = false;
        Camera.MainCamera.FieldOfView = 90;
        Camera.MainCamera.Transform.WorldPosition =
            Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
        Camera.MainCamera.Transform.Rotation = Transform.Rotation;
        Camera.MainCamera.Size = new Vector2(2048, 2048);
        Camera.MainCamera.NearPlaneDistance = 0.1f;
        Camera.MainCamera.FarPlaneDistance = Radius;
        Camera.MainCamera.UpdateMatrices();

        LightSpaceViewProjectionMatrix = Camera.MainCamera.GetLightViewMatrix() *
                                         Camera.MainCamera.GetPerspectiveProjectionMatrix();
    }
}