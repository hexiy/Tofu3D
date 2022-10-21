//using Quaternion = Engine.Quaternion;

namespace Scripts;

[ExecuteInEditMode]
public class Transform : Component
{
	[XmlIgnore]
	public List<Transform> Children = new();
	public List<int> ChildrenIDs = new();

	Vector3? _lastFramePosition;
	//[Hide] public Vector3 localPosition { get { return position - GetParentPosition(); } set { position = GetParentPosition() + value; } }
	//[Hide] public Vector3 initialAngleDifferenceFromParent = Vector3.Zero;
	//[Hide] public Vector3 up { get { return position + TransformVector(new Vector3(0, 1, 0)); } }

	/*[ShowInEditor]
	public Vector3 LocalPosition
	{
		get { return transform.position - GetParentPosition(); }
		set
		{
			position = value + GetParentPosition();
			localPosition = value;
		}
	}*/
	[XmlIgnore]
	public Transform Parent;
	[Hide]
	public int ParentId = -1;

	public Vector3 Pivot = new(0, 0, 0);
	public Vector3 Position = Vector3.Zero;
	Vector3 _rotation = Vector3.Zero;

	//public new bool enabled { get { return true; } }

	public Vector3 Scale = Vector3.One;
	public Vector3 Rotation
	{
		get { return _rotation; }
		set { _rotation = new Vector3(value.X % 360, value.Y % 360, value.Z % 360); }
	}
	[Hide] public Vector3 Forward { get; set; }

	public override void EditorUpdate()
	{
		Update();
	}

	public override void Update()
	{
		if (_lastFramePosition == null)
		{
			_lastFramePosition = Position;
		}

		Vector3 positionDelta = Position - _lastFramePosition.Value;

		if (Children.Count > 0)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].Transform.Position += positionDelta;
			}
		}

		_lastFramePosition = Position;
	}

	public void RemoveChild(int id)
	{
		for (int i = 0; i < Children.Count; i++)
		{
			if (Children[i].GameObjectId == id)
			{
				Children.RemoveAt(i);
				break;
			}
		}

		for (int i = 0; i < ChildrenIDs.Count; i++)
		{
			if (ChildrenIDs[i] == id)
			{
				ChildrenIDs.RemoveAt(i);
				break;
			}
		}
	}

	public void SetParent(Transform par, bool updateTransform = false)
	{
		if (ParentId != -1 && Scene.I.GetGameObject(ParentId) != null)
		{
			Scene.I.GetGameObject(ParentId).Transform.RemoveChild(GameObjectId);
		}

		if (updateTransform)
		{
			Rotation -= par.Transform.Rotation;
			Position = par.Transform.Position + (par.Transform.Position - Transform.Position);
			//initialAngleDifferenceFromParent = rotation - par.transform.rotation;
		}

		Parent = par;
		ParentId = Parent.GameObjectId;

		par.Children.Add(this);
		par.ChildrenIDs.Add(GameObjectId);
	}

	public Vector3 GetParentPosition()
	{
		if (Parent != null)
		{
			return Parent.Transform.Position;
		}

		return Vector3.Zero;
	}

	public Vector3 TransformDirection(Vector3 dir)
	{
		Vector3 forward = (Matrix4x4.CreateTranslation(new Vector3(dir.X, dir.Y, -dir.Z))
		                 * Matrix4x4.CreateFromYawPitchRoll(Transform.Rotation.Y / 180 * Mathf.Pi,
		                                                    -Transform.Rotation.X / 180 * Mathf.Pi,
		                                                    -Transform.Rotation.Z / 180 * Mathf.Pi)).Translation;
		return forward;
	}

	// public Vector3 TransformVector(Vector3 dir)
	// {
	// 	Vector3 direction = new Vector3(
	// 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(transform.Rotation.Y))
	// 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.X))),
	// 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(transform.Rotation.X))),
	// 	                                (float) (MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.Y))
	// 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.X)))
	// 	                               );
	//
	// 	direction = direction.Normalized();
	//
	// 	Matrix4x4 mat = Matrix4x4.CreateTranslation(direction) * Matrix4x4.CreateLookAt(Vector3.Zero, dir, Vector3.Up);
	//
	// 	direction = mat.Translation;
	// 	direction = direction.Normalized();
	// 	return dir;
	// }
}