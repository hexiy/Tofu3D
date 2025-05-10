namespace TofuEngine;

[ExecuteInEditMode]
public class DirectionalLight : LightBase
{
    public static Matrix4x4 LightSpaceViewProjectionMatrix { get; private set; } = Matrix4x4.Identity;


    // [XmlIgnore] public static RenderTexture DepthRenderTexture { get; private set; }
    // [XmlIgnore] public static RenderTexture DisplayDepthRenderTexture { get; private set; }
    [PositiveNumber]
    public float FarPlaneDistance = 1000;


    [PositiveNumber]
    public float NearPlaneDistance = 0.0001f;

    [PositiveNumber]
    public float OrthographicSize = 40;

    [PositiveNumber]
    public int RefreshRate = 60;

    public bool Rotate = false;

    public float RotateOffset = 0;
    public Vector2 Size = new Vector2(4096, 4096);

    [Show]
    [PositiveNumber]
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
        _cameraBeforeTransformationWorldPosition = Camera.GameViewCamera.Transform.WorldPosition;
        _cameraBeforeTransformationRotation = Camera.GameViewCamera.Transform.Rotation;
        _cameraBeforeTransformationIsOrthographic = Camera.GameViewCamera.IsOrthographic;
        _cameraBeforeTransformationOrthographicSize = Camera.GameViewCamera.OrthographicSize;
        _cameraBeforeTransformationSize = Camera.GameViewCamera.Size;
        _cameraBeforeTransformationNearPlaneDistance = Camera.GameViewCamera.NearPlaneDistance;
        _cameraBeforeTransformationFarPlaneDistance = Camera.GameViewCamera.FarPlaneDistance;

        Camera.GameViewCamera.IsOrthographic = true;
        Camera.GameViewCamera.OrthographicSize = OrthographicSize;
        Camera.GameViewCamera.Transform.WorldPosition =
            Transform.WorldPosition; // * new Vector3(1, 1, 1); // TODO: well this is weird
        Camera.GameViewCamera.Transform.Rotation = Transform.Rotation;
        Camera.GameViewCamera.Size = Size;
        Camera.GameViewCamera.NearPlaneDistance = NearPlaneDistance;
        Camera.GameViewCamera.FarPlaneDistance = FarPlaneDistance;
        Camera.GameViewCamera.UpdateMatrices();

        LightSpaceViewProjectionMatrix = Camera.GameViewCamera.GetLightViewMatrix() *
                                         Camera.GameViewCamera.GetLightProjectionMatrix(OrthographicSize);
    }
}