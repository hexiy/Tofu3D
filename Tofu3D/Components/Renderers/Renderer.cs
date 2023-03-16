namespace Scripts;

[ExecuteInEditMode]
public class Renderer : Component, IComparable<Renderer>
{
	//[LinkableComponent]
	public BoxShape BoxShape;
	public Color Color = Color.White;
	public float DistanceFromCamera;

	[Show]
	public Material Material;
	internal bool OnScreen = true;
	public float Layer { get; set; }
	[XmlIgnore] public Matrix4x4 LatestModelViewProjection { get; private set; }

	public int CompareTo(Renderer comparePart)
	{
		// A null value means that this object is greater.
		if (comparePart == null)
		{
			return 1;
		}

		return (GameObject.IndexInHierarchy * 1e-15f + Layer).CompareTo(comparePart.GameObject.IndexInHierarchy * 1e-15f + comparePart.Layer);
		//return Layer.CompareTo(comparePart.Layer + comparePart.LayerFromHierarchy);
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
	public override void OnDestroyed()
	{
		Scene.I.RenderQueueChanged();
		base.OnDestroyed();
	}

	public override void Awake()
	{
		SetDefaultMaterial();

		base.Awake();
	}

	public virtual void SetDefaultMaterial()
	{
		Material.SetShader(Material.Shader);
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

	public virtual Matrix4x4 GetModelViewProjectionFromBoxShape()
	{
		return GetModelMatrix() * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix;
	}

	public Matrix4x4 GetModelMatrix()
	{
		Vector3 pivotOffset = -(BoxShape.Size * Transform.WorldScale) / 2
		                    + new Vector3(BoxShape.Size.X * Transform.WorldScale.X * Transform.Pivot.X,
		                                  BoxShape.Size.Y * Transform.WorldScale.Y * Transform.Pivot.Y,
		                                  BoxShape.Size.Z * Transform.WorldScale.Z * Transform.Pivot.Z);

		Matrix4x4 pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale + (GameObject.IndexInHierarchy * Vector3.One * 0.0001f)) * Matrix4x4.CreateScale(1, -1, 1);

		Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.Z / 180 * Mathf.Pi);
		Matrix4x4 scale = Matrix4x4.CreateScale(BoxShape.Size.X * Transform.WorldScale.X, BoxShape.Size.Y * Transform.WorldScale.Y, Transform.WorldScale.Z * BoxShape.Size.Z);
		return scale * Matrix4x4.Identity * pivot * rotation * translation * Matrix4x4.CreateScale(Units.OneWorldUnit);
	}

	public Matrix4x4 GetModelMatrixForLight()
	{
		Vector3 pivotOffset = -(BoxShape.Size * Transform.WorldScale) / 2
		                    + new Vector3(BoxShape.Size.X * Transform.WorldScale.X * Transform.Pivot.X,
		                                  BoxShape.Size.Y * Transform.WorldScale.Y * Transform.Pivot.Y,
		                                  BoxShape.Size.Z * Transform.WorldScale.Z * Transform.Pivot.Z);

		Matrix4x4 pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale + (GameObject.IndexInHierarchy * Vector3.One * 0.0001f)) * Matrix4x4.CreateScale(1, -1, -1);

		Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.Z / 180 * Mathf.Pi);
		Matrix4x4 scale = Matrix4x4.CreateScale(BoxShape.Size.X * Transform.WorldScale.X, BoxShape.Size.Y * Transform.WorldScale.Y, Transform.WorldScale.Z * BoxShape.Size.Z);
		return scale * Matrix4x4.Identity * pivot * rotation * translation * Matrix4x4.CreateScale(Units.OneWorldUnit);
	}

	public Matrix4x4 GetMvpForOutline()
	{
		Vector3 pivotOffset = -(BoxShape.Size * Transform.WorldScale) / 2
		                    + new Vector3(BoxShape.Size.X * Transform.WorldScale.X * Transform.Pivot.X,
		                                  BoxShape.Size.Y * Transform.WorldScale.Y * Transform.Pivot.Y,
		                                  BoxShape.Size.Z * Transform.WorldScale.Z * Transform.Pivot.Z);

		Matrix4x4 pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 translation = Matrix4x4.CreateTranslation(Transform.WorldPosition + BoxShape.Offset * Transform.WorldScale + (GameObject.IndexInHierarchy * Vector3.One * 0.0001f)) * Matrix4x4.CreateScale(1, -1, 1);

		Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                      -Transform.Rotation.Z / 180 * Mathf.Pi);

		float outlineThickness = 0.03f * Mathf.ClampMin(MathHelper.Abs((float) MathHelper.Sin(Time.EditorElapsedTime)), 0.5f) * DistanceFromCamera * 0.3f;

		Matrix4x4 scale = Matrix4x4.CreateScale(Vector3.One * BoxShape.Size * Transform.WorldScale + Vector3.One * outlineThickness);
		return scale * Matrix4x4.Identity * pivot * rotation * translation * Matrix4x4.CreateScale(Units.OneWorldUnit) * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix;
	}

	public Vector4 GetSize()
	{
		return new Vector4(BoxShape.Size.X * Transform.LocalScale.X, BoxShape.Size.Y * Transform.LocalScale.Y, 1, 1);
	}

	public override void Update()
	{
		UpdateMvp();

		DistanceFromCamera = CalculateDistanceFromCamera();

		if (BoxShape == null)
		{
			return;
		}
		//if (Time.elapsedTicks % 10 == 0) onScreen = Camera.I.RectangleVisible(boxShape);

		// if (OnScreen)
		// {
		// 	UpdateMvp();
		// }

		base.Update();
	}

	float CalculateDistanceFromCamera()
	{
		return Vector2.Distance(Transform.WorldPosition, Camera.I.Transform.WorldPosition);
	}

	internal void UpdateMvp()
	{
		if (BoxShape == null)
		{
			return;
		}

		LatestModelViewProjection = GetModelViewProjectionFromBoxShape();
	}

	public virtual void Render()
	{
	}
}