namespace Scripts;

[ExecuteInEditMode]
public abstract class Renderer : Component, IComparable<Renderer>, IComponentRenderable, IComponentUpdateable
{
    [Hide] public bool AutomaticallyFindBoxShape = true;

    //[LinkableComponent]
    [XmlIgnore] public BoxShape BoxShape;

    public Color Color = Color.White;

    [XmlIgnore] public RendererInstancingData InstancingData;
    // public float DistanceFromCamera;

    [Show] public Asset_Material Material;
    
    public RuntimeMesh RuntimeMesh;

    public RenderMode RenderMode = RenderMode.Opaque;

    // internal bool OnScreen = true;
    public float Layer { get; set; }

    [XmlIgnore] public Matrix4x4 LatestModelViewProjection { get; private set; }

    [Hide] public virtual bool CanRender => true; // && Enabled && GameObject.Awoken && GameObject.ActiveInHierarchy;

    private Matrix4x4 ScalePivotRotationMatrix
    {
        get
        {
            var scale = Matrix4x4.CreateScale(BoxShape.Size * Transform.WorldScale);
            return scale * IdentityPivotRotationMatrix;
        }
    }

    private Matrix4x4 IdentityPivotRotationMatrix
    {
        get
        {
            var worldPositionPivotOffset =
                BoxShape.Size * Transform.WorldScale * (Vector3.One - Transform.Pivot * 2);

            var pivot = Matrix4x4.CreateTranslation(worldPositionPivotOffset);

            var rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.WorldRotation.Y / 180 * Mathf.Pi,
                Transform.WorldRotation.X / 180 * Mathf.Pi,
                Transform.WorldRotation.Z / 180 * Mathf.Pi);

            return Matrix4x4.Identity * pivot * rotation;
        }
    }

    public int CompareTo(Renderer comparePart)
    {
        // A null value means that this object is greater.
        if (comparePart == null)
        {
            return 1;
        }

        // return (GameObject.IndexInHierarchy * 1e-15f + Layer).CompareTo(comparePart.GameObject.IndexInHierarchy * 1e-15f + comparePart.Layer);
        // return (comparePart.DistanceFromCamera + (comparePart.GameObject.IndexInHierarchy * 1e-15f + comparePart.Layer)).CompareTo(DistanceFromCamera + (GameObject.IndexInHierarchy * 1e-15f + Layer));
        return (comparePart.GameObject.IndexInHierarchy * 1e-15f + comparePart.Layer).CompareTo(
            GameObject.IndexInHierarchy * 1e-15f + Layer);

        //return Layer.CompareTo(comparePart.Layer + comparePart.LayerFromHierarchy);
    }

    public abstract void Render();

    public void Update()
    {
        /*if (Material != null && Material.IsValid == false)
        {
            Debug.LogError("Material invalid, reloading");
            Material = Tofu.AssetManager.Load<Asset_Material>(Material.AssetPath);
            // Material.IsValid = true;
        }*/

        UpdateMvp();

        // DistanceFromCamera = CalculateDistanceFromCamera();
        if (Color.A != 255)
        {
            if (RenderMode != RenderMode.Transparent)
            {
                RenderMode = RenderMode.Transparent;
            }
        }
        else
        {
            if (RenderMode != RenderMode.Opaque)
            {
                RenderMode = RenderMode.Opaque;
            }
        }

        if (BoxShape == null)
        {
        }
        //if (Time.elapsedTicks % 10 == 0) onScreen = Camera.I.RectangleVisible(boxShape);

        // if (OnScreen)
        // {
        // 	UpdateMvp();
        // }
    }

    // public int CompareTo(Renderer comparePart)
    // {
    // 	// A null value means that this object is greater.
    // 	if (comparePart == null)
    // 	{
    // 		return 1;
    // 	}
    //
    // 	return comparePart.distanceFromCamera.CompareTo(distanceFromCamera);
    // }

    public override void Awake()
    {
        if (AutomaticallyFindBoxShape)
        {
            BoxShape = GetComponent<BoxShape>();
        }

        SetDefaultMaterial();

        base.Awake();
    }

    public virtual void SetDefaultMaterial()
    {
        Material.InitShader();
    }

    // private Matrix4x4 GetModelViewProjectionOld()
    // {
    // 	Vector2 pivotOffset = -(boxShape.size * transform.scale) / 2 + new Vector2(boxShape.size.X * transform.scale.X * transform.pivot.X, boxShape.size.Y * transform.scale.Y * transform.pivot.Y);
    // 	Matrix4x4 _translation = Matrix4x4.CreateTranslation(transform.position + boxShape.offset * transform.scale - pivotOffset);
    //
    // 	Matrix4x4 _rotation = Matrix4x4.CreateFromYawPitchRoll(transform.rotation.Y / 180 * Mathf.Pi,
    // 	                                                       transform.rotation.X / 180 * Mathf.Pi,
    // 	                                                       transform.rotation.Z / 180 * Mathf.Pi);
    // 	Matrix4x4 _scale = Matrix4x4.CreateScale(boxShape.size.X * transform.scale.X, boxShape.size.Y * transform.scale.Y, 1);
    //
    // 	return _scale * Matrix4x4.Identity * _rotation * _translation * Camera.I.viewMatrix * Camera.I.projectionMatrix;
    // }

    public virtual Matrix4x4 GetModelViewProjectionFromBoxShape() =>
        GetModelMatrix() * Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix;

    internal void GL_DrawArrays(PrimitiveType primitiveType, int first, int count)
    {
        GL.DrawArrays(primitiveType, first, count);
        // Debug.StatAddValue("Total vertices:", count);
        DebugHelper.LogDrawCall();
        DebugHelper.LogVerticesDrawCall(verticesCount: count);
    }

    internal void RenderWireframe(int indicesCount)
    {
        if (Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible)
        {
            Material.Shader.SetColor("u_rendererColor", Color.Black);
            // GL.LineWidth(RenderSettings.CurrentWireframeRenderSettings.WireframeLineWidth / (DistanceFromCamera * 10));

            // float s = Mathf.SinAbs(Time.EditorElapsedTime * 0.5f);
            GL_DrawArrays(PrimitiveType.LineLoop, 0, indicesCount);
            // GL.DrawArrays(PrimitiveType.LineLoop, 0, (int) (verticesCount * s));
            // Material.Shader.SetColor("u_rendererColor", Color);
        }
    }

    public Matrix4x4 GetModelMatrix()
    {
        // Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale + (GameObject.IndexInHierarchy * Vector3.One * 0.0001f));
        var translation =
            Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale);
        return ScalePivotRotationMatrix * translation;
    }

    public Matrix4x4 GetModelMatrixForCanvasObject()
    {
        var translation = Matrix4x4.CreateTranslation(Transform.WorldPosition - Camera.MainCamera.Size / 2 +
                                                      BoxShape.Offset * Transform.WorldScale +
                                                      GameObject.IndexInHierarchy * Vector3.One * 0.0001f);
        return ScalePivotRotationMatrix * translation *
               Matrix4x4.CreateScale(2f / Camera.MainCamera.Size.X, 2f / Camera.MainCamera.Size.Y, 0);
    }

    // public Matrix4x4 GetModelMatrixForLight()
    // {
    // 	Vector3 pivotOffset = -(BoxShape.Size * Transform.WorldScale) / 2
    // 	                    + new Vector3(BoxShape.Size.X * Transform.WorldScale.X * Transform.Pivot.X,
    // 	                                  BoxShape.Size.Y * Transform.WorldScale.Y * Transform.Pivot.Y,
    // 	                                  BoxShape.Size.Z * Transform.WorldScale.Z * Transform.Pivot.Z);
    //
    // 	Matrix4x4 pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
    // 	Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale + (GameObject.IndexInHierarchy * Vector3.One * 0.0001f)) * Matrix4x4.CreateScale(1, -1, -1);
    //
    // 	Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
    // 	                                                      -Transform.Rotation.X / 180 * Mathf.Pi,
    // 	                                                      -Transform.Rotation.Z / 180 * Mathf.Pi);
    // 	Matrix4x4 scale = Matrix4x4.CreateScale(BoxShape.Size.X * Transform.WorldScale.X, BoxShape.Size.Y * Transform.WorldScale.Y, Transform.WorldScale.Z * BoxShape.Size.Z);
    // 	return scale * Matrix4x4.Identity * pivot * rotation * translation * Matrix4x4.CreateScale(Units.OneWorldUnit);
    // }

    public Matrix4x4 GetMvpForOutline()
    {
        // float outlineThickness = 0.002f * ((float) MathHelper.Sin(Time.EditorElapsedTime * 2) + 1.3f) * DistanceFromCamera * BoxShape.Size.Length();
        var outlineThickness = 0.002f * ((float)MathHelper.Sin(Time.EditorElapsedTime * 2) + 1.3f) *
                               BoxShape.Size.Length();
        // float outlineThickness = 0.04f * Mathf.ClampMin(MathHelper.Abs((float) MathHelper.Sin(Time.EditorElapsedTime*5)),0) * DistanceFromCamera * 0.3f;
        var translation = Matrix4x4.CreateTranslation(Transform.WorldPosition +
                                                      BoxShape.Offset * Transform.WorldScale +
                                                      GameObject.IndexInHierarchy * Vector3.One * 0.0001f);

        var scale = Matrix4x4.CreateScale(BoxShape.Size * Transform.WorldScale + new Vector3(outlineThickness));

        return scale * IdentityPivotRotationMatrix * translation * Camera.MainCamera.ViewMatrix *
               Camera.MainCamera.ProjectionMatrix;
    }

    public Matrix4x4 GetCanvasMvpForOutline()
    {
        ///////////////////////////
        var worldPositionPivotOffset = BoxShape.Size * Transform.WorldScale * (Vector3.One - Transform.Pivot * 2);

        var pivot = Matrix4x4.CreateTranslation(worldPositionPivotOffset);
        var translation =
            Matrix4x4.CreateTranslation(Transform.WorldPosition - Camera.MainCamera.Size / 2 +
                                        BoxShape.Offset * Transform.WorldScale +
                                        GameObject.IndexInHierarchy * Vector3.One * 0.0001f) *
            Matrix4x4.CreateScale(1, 1, 1);

        var rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.WorldRotation.Y / 180 * Mathf.Pi,
            -Transform.WorldRotation.X / 180 * Mathf.Pi,
            -Transform.WorldRotation.Z / 180 * Mathf.Pi);

        var outlineThickness = 1 * ((float)MathHelper.Sin(Time.EditorElapsedTime * 7) + 1.3f);
        // float outlineThickness = 0.04f * Mathf.ClampMin(MathHelper.Abs((float) MathHelper.Sin(Time.EditorElapsedTime*5)),0) * DistanceFromCamera * 0.3f;
        var scale = Matrix4x4.CreateScale(BoxShape.Size * Transform.WorldScale + Vector3.One * outlineThickness);

        return scale * Matrix4x4.Identity * pivot * rotation * translation *
               Matrix4x4.CreateScale(2f / Camera.MainCamera.Size.X, 2f / Camera.MainCamera.Size.Y, 0) *
               Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix;
    }

    public Vector4 GetSize() =>
        new(BoxShape.Size.X * Transform.LocalScale.X, BoxShape.Size.Y * Transform.LocalScale.Y, 1, 1);

    private float CalculateDistanceFromCamera() =>
        Vector3.Distance(Transform.WorldPosition, Camera.MainCamera.Transform.WorldPosition);

    internal void UpdateMvp()
    {
        if (BoxShape == null)
        {
            return;
        }

        if (InstancingData.MatrixDirty || GameObject.IsStatic == false)
        {
            LatestModelViewProjection = GetModelViewProjectionFromBoxShape();
            InstancingData.MatrixDirty = false;
        }
    }
}