﻿//using Quaternion = Engine.Quaternion;

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

	private Vector3 _worldPosition = Vector3.Zero;
	[Hide]
	public Vector3 WorldPosition
	{
		get { return _worldPosition; }
		set
		{
			_worldPosition = value;

			Vector3 calculatedLocalPos = TranslateWorldToLocal(_worldPosition);
			if (LocalPosition != calculatedLocalPos)
			{
				//LocalPosition = calculatedLocalPos;
			}

			UpdateChildrenPositions();
		}
	}

	private Vector3 _localPosition;
	public Vector3 LocalPosition
	{
		get { return _localPosition; }
		set
		{
			_localPosition = value;

			Vector3 calculatedWorldPos = TranslateLocalToWorld(_localPosition);
			if (WorldPosition != calculatedWorldPos)
			{
				WorldPosition = calculatedWorldPos;
			}

			UpdateChildrenPositions();
		}
	}

	Vector3 _scale = Vector3.One;
	public Vector3 Scale
	{
		get { return _scale; }
		set
		{
			_scale = value;
			UpdateChildrenPositions();
		}
	}

	[Hide]
	public Vector3 WorldScale
	{
		get
		{
			Transform pr = Parent;
			Vector3 scl = Transform.Scale;

			while (pr != null)
			{
				scl = scl * pr.Scale;
				pr = pr.Parent;
			}

			return scl;
		}
	}

	Vector3 _rotation = Vector3.Zero;
	public Vector3 Rotation
	{
		get { return _rotation; }
		set { _rotation = new Vector3(value.X % 360, value.Y % 360, value.Z % 360); }
	}
	[Hide] public Vector3 Forward { get; set; }

	private void UpdateChildrenPositions()
	{
		for (int i = 0; i < Children.Count; i++)
		{
			//Children[i].LocalPosition = Children[i].TranslateWorldToLocal(Children[i].WorldPosition);
			Children[i].WorldPosition = Children[i].TranslateLocalToWorld(Children[i].LocalPosition);
		}
	}

	private Vector3 TranslateLocalToWorld(Vector3 localPos)
	{
		Vector3 worldPos;
		if (Parent)
		{
			worldPos = localPos * Parent.Scale + Parent._worldPosition;
		}
		else
		{
			worldPos = localPos;
		}

		return worldPos;
	}

	private Vector3 TranslateWorldToLocal(Vector3 worldPos)
	{
		Vector3 localPos;
		if (Parent)
		{
			localPos = worldPos * Parent.Scale - Parent._worldPosition;
		}
		else
		{
			localPos = worldPos;
		}

		return localPos;
	}

	public override void Awake()
	{
		LocalPosition = LocalPosition;
		base.Awake();
	}

	public override void EditorUpdate()
	{
		Update();
	}

	public override void Update()
	{
		if (Parent)
		{
			//WorldPosition = TranslateLocalToWorld(LocalPosition);
			//LocalPosition = TranslateWorldToLocal(WorldPosition);
		}
		// if (_lastFramePosition == null)
		// {
		// 	_lastFramePosition = WorldPosition;
		// }
		//
		// Vector3 positionDelta = WorldPosition - _lastFramePosition.Value;
		//
		// if (Children.Count > 0)
		// {
		// 	for (int i = 0; i < Children.Count; i++)
		// 	{
		// 		Children[i].Transform.WorldPosition += positionDelta;
		// 	}
		// }
		//
		// _lastFramePosition = WorldPosition;
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
			WorldPosition = par.Transform.WorldPosition + (par.Transform.WorldPosition - Transform.WorldPosition);
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
			return Parent.Transform.WorldPosition;
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