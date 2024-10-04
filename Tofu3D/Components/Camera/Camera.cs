namespace Tofu3D;

[ExecuteInEditMode]
public class Camera : Component, IComponentUpdateable
{
    public static Action<Vector2> CameraSizeChanged = newSize => { };

    //public int antialiasingStrength = 0;
    public Color Color = new(34, 34, 34);
    public float FarPlaneDistance = 50;

    [ShowIfNot(nameof(IsOrthographic))] public float FieldOfView = 60;

    public bool IsOrthographic = false;
    public float NearPlaneDistance = 1;

    [ShowIf(nameof(IsOrthographic))] public float OrthographicSize = 2;

    //public float cameraSize = 0.1f;
    [XmlIgnore] public Matrix4x4 ProjectionMatrix;

    public Vector2 Size = new(1380, 900);

    [XmlIgnore] public Matrix4x4 TranslationMatrix;

    [XmlIgnore] public Matrix4x4 ViewMatrix;
    //[XmlIgnore] public RenderTarget2D renderTarget;

    public static Camera MainCamera { get; private set; }

    public void Update()
    {
        if (IsOrthographic)
        {
            Transform.LocalScale = Vector3.One * OrthographicSize;
        }

        else
        {
            Transform.LocalScale = Vector3.One;
        }

        UpdateMatrices();
    }

    public override void Awake()
    {
        // if (MainCamera == null)
        // {
        MainCamera = this;
        // }

        GameObject.AlwaysUpdate = true;
        // if (Global.EditorAttached == false)
        // {
        // 	Size = new Vector2(Tofu.Window.ClientSize.X, Tofu.Window.ClientSize.Y);
        // }

        UpdateMatrices();

        base.Awake();
    }

    public override void Start()
    {
        CameraSizeChanged.Invoke(Size);
        base.Start();
    }

    public void SetSize(Vector2 newSize)
    {
        Size = newSize;

        CameraSizeChanged.Invoke(newSize);
    }

    public void UpdateMatrices()
    {
        ProjectionMatrix = GetProjectionMatrix();
        ViewMatrix = GetViewMatrix();
        TranslationMatrix = GetTranslationRotationMatrix();
    }

    private Matrix4x4 GetViewMatrix()
    {
        //  const float radius = 500.0f;
        //  float camX = (float) Math.Sin(Time.EditorElapsedTime) * radius; //sin(glfwGetTime()) * radius;
        //  float camZ = (float) Math.Cos(Time.EditorElapsedTime) * radius; //cos(glfwGetTime()) * radius;
        //  float camY = (float) Math.Cos(Time.EditorElapsedTime) * radius; //cos(glfwGetTime()) * radius;
        //  Matrix4x4 view = Matrix4x4.CreateLookAt(new Vector3(camX, camY, camZ), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        // return view;


        var forwardWorld =
            Transform.WorldPosition + Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 1));
        var upLocal = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 1, 0));

        //Debug.Log($"Forward{forwardWorld}");
        //Debug.Log($"Up{upLocal}");
        var view = Matrix4x4.CreateLookAt(Transform.WorldPosition, forwardWorld, upLocal)
                   * Matrix4x4.CreateScale(-1, 1, 1);
        ; // * Matrix4x4.CreateTranslation(Transform.WorldPosition * Units.OneWorldUnit * new Vector3(-1, -1, 1));
        return view;
    }

    private Matrix4x4 GetProjectionMatrix()
    {
        if (IsOrthographic)
        {
            return GetOrthographicProjectionMatrix();
        }

        return GetPerspectiveProjectionMatrix();
    }

    private Matrix4x4 GetPerspectiveProjectionMatrix()
    {
        FieldOfView = Mathf.ClampMin(FieldOfView, 0.0001f);
        NearPlaneDistance = Mathf.Clamp(NearPlaneDistance, 0.00001f, FarPlaneDistance);
        FarPlaneDistance = Mathf.Clamp(FarPlaneDistance, NearPlaneDistance + 0.001f, Mathf.Infinity);
        var perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView),
            Size.X / Size.Y, NearPlaneDistance, FarPlaneDistance);

        // .CreatePerspective gives us great depth, but fieldofview doesnt?....
        return perspectiveMatrix;
    }

    private Matrix4x4 GetOrthographicProjectionMatrix()
    {
        var left = -OrthographicSize;
        var right = OrthographicSize;
        var bottom = -OrthographicSize;
        var top = OrthographicSize;

        var orthoMatrix =
            Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, NearPlaneDistance, FarPlaneDistance);

        return orthoMatrix;
    }


    private Matrix4x4 GetTranslationRotationMatrix()
    {
        var tr = Matrix4x4.CreateTranslation(-Transform.LocalPosition.X, -Transform.LocalPosition.Y,
            Transform.LocalPosition.Z);
        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
            -Transform.Rotation.X / 180 * Mathf.Pi,
            -Transform.Rotation.Z / 180 * Mathf.Pi);
        return tr * rotationMatrix;
    }

    public void Move(Vector2 moveVector)
    {
        Transform.WorldPosition += moveVector;
    }

    public Vector2 WorldToScreen(Vector2 worldPosition) =>
        Vector2.Transform(worldPosition, GetTranslationRotationMatrix());

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 worldPos;
        if (IsOrthographic)
        {
            worldPos = Vector3.Transform(screenPosition / Size * 2,
                           Matrix.Invert(ProjectionMatrix))
                       - Size * OrthographicSize / 2;

            worldPos = worldPos;
            worldPos = worldPos + Transform.WorldPosition;
        }

        else
        {
            worldPos = Vector3.Transform(screenPosition / Size * 2, Matrix.Invert(ProjectionMatrix));
            worldPos = worldPos;
        }

        // worldPos = Vector2.Transform(screenPosition / size * 2,Matrix.Invert(projectionMatrix)) - size;
        // worldPos = worldPos / Units.OneWorldUnit;

        //Debug.Log($"SCREEN:{screenPosition} | WORLD:{worldPos}");
        return worldPos;
    }

    public Vector2 CenterOfScreenToWorld() => ScreenToWorld(new Vector2(Size.X / 2, Size.Y / 2));

    public bool RectangleVisible(BoxShape shape)
    {
        var isIn = Vector2.Distance(shape.Transform.WorldPosition, Transform.WorldPosition) <
                   Size.X * 1.1f * (OrthographicSize / 2) +
                   shape.Size.X / 2 * shape.Transform.LocalScale.MaxVectorMember();

        return isIn;
    }

    public Matrix4x4 GetLightProjectionMatrix(float lightOrthographicSize)
    {
        var left = -lightOrthographicSize;
        var right = lightOrthographicSize;
        var bottom = -lightOrthographicSize;
        var top = lightOrthographicSize;

        var orthoMatrix =
            Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, NearPlaneDistance, FarPlaneDistance);

        return orthoMatrix;
    }

    public Matrix4x4 GetLightViewMatrix()
    {
        // Vector3 oldRotation = Transform.Rotation;
        // oldRotation = oldRotation * new Vector3(1, 1, 0);
        // Transform.Rotation = -oldRotation;

        var forwardWorld =
            Transform.WorldPosition + Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 1));
        var upLocal = Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 1, 0));


        var view = Matrix4x4.CreateLookAt(Transform.WorldPosition, forwardWorld, upLocal)
                   * Matrix4x4.CreateScale(-1, 1, 1);

        // Transform.Rotation = oldRotation;

        return view;
    }
}