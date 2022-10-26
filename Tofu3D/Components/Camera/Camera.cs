namespace Tofu3D;

[ExecuteInEditMode]
public class Camera : Component
{
	//public int antialiasingStrength = 0;
	public Color Color = new(34, 34, 34);
	public float FarPlaneDistance = 50;
	[ShowIfNot(nameof(IsOrthographic))]
	public float FieldOfView = 2;
	public bool IsOrthographic = true;
	public float NearPlaneDistance = 1;

	[ShowIf(nameof(IsOrthographic))]
	public float OrthographicSize = 2;

	//public float cameraSize = 0.1f;
	[XmlIgnore]
	public Matrix4x4 ProjectionMatrix;

	public Vector2 Size = new(1380, 900);
	[XmlIgnore]
	public Matrix4x4 TranslationMatrix;
	[XmlIgnore]
	public Matrix4x4 ViewMatrix;
	//[XmlIgnore] public RenderTarget2D renderTarget;

	public static Camera I { get; private set; }

	public override void Awake()
	{
		I = this;

		GameObject.AlwaysUpdate = true;
		if (Global.EditorAttached == false)
		{
			Size = new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y);
		}

		ProjectionMatrix = GetProjectionMatrix();
		ViewMatrix = GetViewMatrix();
		TranslationMatrix = GetTranslationRotationMatrix();

		base.Awake();
	}

	public override void Update()
	{
		Transform.LocalScale = Vector3.One * OrthographicSize;
		UpdateMatrices();
		base.Update();
	}

	public void UpdateMatrices()
	{
		ProjectionMatrix = GetProjectionMatrix();
		ViewMatrix = GetViewMatrix();
		TranslationMatrix = GetTranslationRotationMatrix();
	}

	Matrix4x4 GetViewMatrix()
	{
		// Vector3 pos = Transform.WorldPosition;
		// Vector3 forward = Transform.TransformDirection(Vector3.Forward);
		// Vector3 up = Vector3.Up;
		//
		// Matrix4x4 view = Matrix4x4.CreateWorld(pos, forward, up);
		return Matrix4x4.Identity;
	}

	Matrix4x4 GetProjectionMatrix()
	{
		if (IsOrthographic)
		{
			return GetOrthographicProjectionMatrix();
		}

		return GetPerspectiveProjectionMatrix();
	}

	Matrix4x4 GetPerspectiveProjectionMatrix()
	{
		FieldOfView = Mathf.ClampMin(FieldOfView, 0.0001f);
		NearPlaneDistance = Mathf.Clamp(NearPlaneDistance, 0.001f, FarPlaneDistance);
		FarPlaneDistance = Mathf.Clamp(FarPlaneDistance, NearPlaneDistance + 0.001f, Mathf.Infinity);
		Matrix4x4 pm = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), Size.X / Size.Y, NearPlaneDistance, FarPlaneDistance);

		return TranslationMatrix * pm;
	}

	Matrix4x4 GetOrthographicProjectionMatrix()
	{
		float left = -Size.X / 2;
		float right = Size.X / 2;
		float bottom = -Size.Y / 2;
		float top = Size.Y / 2;

		Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.00001f, 10000000f);

		return TranslationMatrix * orthoMatrix * GetScaleMatrix();
	}

	Matrix4x4 GetTranslationRotationMatrix()
	{
		Matrix4x4 tr = Matrix4x4.CreateTranslation(-Transform.LocalPosition.X * Units.OneWorldUnit, -Transform.LocalPosition.Y * Units.OneWorldUnit, Transform.LocalPosition.Z * Units.OneWorldUnit);
		Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                            -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                            -Transform.Rotation.Z / 180 * Mathf.Pi);
		return tr * rotationMatrix;
	}

	Matrix4x4 GetScaleMatrix()
	{
		Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(1 / OrthographicSize);
		return scaleMatrix;
	}

	public void Move(Vector2 moveVector)
	{
		Transform.WorldPosition += moveVector;
	}

	public Vector2 WorldToScreen(Vector2 worldPosition)
	{
		return Vector2.Transform(worldPosition, GetTranslationRotationMatrix());
	}

	public Vector2 ScreenToWorld(Vector2 screenPosition)
	{
		Vector3 worldPos;
		if (I.IsOrthographic)
		{
			worldPos = Vector3.Transform(screenPosition / Size * 2,
			                             Matrix.Invert(ProjectionMatrix))
			         - Size * OrthographicSize / 2;

			worldPos = worldPos / Units.OneWorldUnit;
		}

		else
		{
			worldPos = Vector3.Transform(screenPosition / Size * 2, Matrix.Invert(ProjectionMatrix));
			worldPos = worldPos / Units.OneWorldUnit;
		}

		// worldPos = Vector2.Transform(screenPosition / size * 2,Matrix.Invert(projectionMatrix)) - size;
		// worldPos = worldPos / Units.OneWorldUnit;

		//Debug.Log($"SCREEN:{screenPosition} | WORLD:{worldPos}");
		return worldPos;
	}

	public Vector2 CenterOfScreenToWorld()
	{
		return ScreenToWorld(new Vector2(Size.X / 2, Size.Y / 2));
	}

	public bool RectangleVisible(BoxShape shape)
	{
		bool isIn = Vector2.Distance(shape.Transform.WorldPosition, Transform.WorldPosition) < Size.X * 1.1f * (OrthographicSize / 2) + shape.Size.X / 2 * shape.Transform.LocalScale.MaxVectorMember();

		return isIn;
	}
}