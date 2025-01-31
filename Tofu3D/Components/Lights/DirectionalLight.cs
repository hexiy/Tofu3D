namespace Tofu3D;

[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
    public static Matrix4x4 LightSpaceViewProjectionMatrix { get; private set; } = Matrix4x4.Identity;


    // [XmlIgnore] public static RenderTexture DepthRenderTexture { get; private set; }
    // [XmlIgnore] public static RenderTexture DisplayDepthRenderTexture { get; private set; }

    public float FarPlaneDistance = 1000;

    public float NearPlaneDistance = 0.0001f;
    public float OrthographicSize = 40;

    public int RefreshRate = 60;

    public bool Rotate = false;

    public float RotateOffset = 0;
    public Vector2 Size = new Vector2(4096, 4096);

    [Show]
    public float Speed = 100;

    public static DirectionalLight I { get; private set; }

    [ExecuteInEditMode]
    public override void Awake()
    {
        // DepthRenderTexture = new RenderTexture(size: Size, colorAttachment: false, depthAttachment: true);
        // DisplayDepthRenderTexture = new RenderTexture(size: Size, colorAttachment: true, depthAttachment: false);
        RenderPassDirectionalLightShadowDepth.I?.SetDirectionalLight(this);
        base.Awake();
    }

    public override void OnDisabled()
    {
        I = null;
        base.OnDisabled();
    }

    public override void OnEnabled()
    {
        I = this;
        base.OnEnabled();
    }

    public void Update()
    {
        if (Rotate)
        {
            Transform.Rotation = Transform.Rotation.Set(Mathf.SinAbs(Time.EditorElapsedTime * 0.5f) + 0.2f * 30,
                RotateOffset + (float)Math.Sin(Time.EditorElapsedTime * Speed) * 50);
        }
    }

    public void RenderDirectionalLightShadowDepth()
    {
        RefreshRate = Math.Clamp(RefreshRate, 1, 60);

        if (Time.EditorElapsedTicks % (60 / RefreshRate) != 0)
        {
            return;
        }

        ConfigureCameraForShadowMapping();

        Tofu.InstancedRenderingSystem.RenderShaderGroups(InstancingRenderMode.All);

        ConfigureCameraForSceneRender();
    }

    public override void OnDestroyed()
    {
        ConfigureCameraForSceneRender();

        base.OnDestroyed();
    }

    private void ConfigureCameraForShadowMapping()
    {
        _cameraBeforeTransformationWorldPosition = Camera.MainCamera.Transform.WorldPosition;
        _cameraBeforeTransformationRotation = Camera.MainCamera.Transform.Rotation;
        _cameraBeforeTransformationIsOrthographic = Camera.MainCamera.IsOrthographic;
        _cameraBeforeTransformationOrthographicSize = Camera.MainCamera.OrthographicSize;
        _cameraBeforeTransformationSize = Camera.MainCamera.Size;
        _cameraBeforeTransformationNearPlaneDistance = Camera.MainCamera.NearPlaneDistance;
        _cameraBeforeTransformationFarPlaneDistance = Camera.MainCamera.FarPlaneDistance;

        Camera.MainCamera.IsOrthographic = true;
        Camera.MainCamera.OrthographicSize = OrthographicSize;
        Camera.MainCamera.Transform.WorldPosition =
            Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
        Camera.MainCamera.Transform.Rotation = Transform.Rotation;
        Camera.MainCamera.Size = Size;
        Camera.MainCamera.NearPlaneDistance = NearPlaneDistance;
        Camera.MainCamera.FarPlaneDistance = FarPlaneDistance;
        Camera.MainCamera.UpdateMatrices();

        LightSpaceViewProjectionMatrix = Camera.MainCamera.GetLightViewMatrix() *
                                         Camera.MainCamera.GetLightProjectionMatrix(OrthographicSize);
    }
}