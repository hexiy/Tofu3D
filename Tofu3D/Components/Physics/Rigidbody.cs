using Tofu3D.Physics;

namespace Scripts;

public class Rigidbody : Component
{
	[Hide]
	public new bool AllowMultiple = false;

	public float AngularDrag = 1f;
	public Vector2 BodyPos;
	[XmlIgnore]
	public List<Rigidbody> TouchingRigidbodies = new();

	[XmlIgnore]
	[LinkableComponent]
	public Shape Shape
	{
		get { return GetComponent<Shape>(); }
	}

	public override void Awake()
	{
		CreateBody();

		base.Awake();
	}

	public void CreateBody()
	{
		BoxShape boxShape = GetComponent<BoxShape>();

		if (boxShape != null)
		{
			BoxShape shape = new();

			lock (PhysicsController.World)
			{
				PhysicsController.World.AddBody(this);
			}
		}
	}

	public override void FixedUpdate()
	{
	}

	// public void UpdateTransform()
	// {
	// 	if (body == null)
	// 	{
	// 		return;
	// 	}
	//
	// 	transform.position = new Vector2(body.Position.X, body.Position.Y) * Physics.WORLD_SCALE;
	// 	transform.Rotation = new Vector3(transform.Rotation.X, transform.Rotation.Y, body.Rotation * Mathf.TwoPi * 2);
	// }

	public override void OnDestroyed()
	{
		for (int i = 0; i < TouchingRigidbodies.Count; i++)
		{
			TouchingRigidbodies[i].OnCollisionExit(this);
			OnCollisionExit(TouchingRigidbodies[i]);
		}
	}

	public override void OnCollisionEnter(Rigidbody rigidbody) // TODO-TRANSLATE CURRENT VELOCITY TO COLLIDED RIGIDBODY, ADD FORCE (MassRatio2/MassRatio1)
	{
		TouchingRigidbodies.Add(rigidbody);

		// Call callback on components that implement interface IPhysicsCallbackListener
		for (int i = 0; i < GameObject.Components.Count; i++)
		{
			if (GameObject.Components[i] is Rigidbody == false)
			{
				GameObject.Components[i].OnCollisionEnter(rigidbody);
			}
		}
	}

	public override void OnCollisionExit(Rigidbody rigidbody)
	{
		if (TouchingRigidbodies.Contains(rigidbody))
		{
			TouchingRigidbodies.Remove(rigidbody);
		}

		for (int i = 0; i < GameObject.Components.Count; i++)
		{
			if (GameObject.Components[i] is Rigidbody == false)
			{
				GameObject.Components[i].OnCollisionExit(rigidbody);
			}
		}
	}

	public override void OnTriggerEnter(Rigidbody rigidbody)
	{
		TouchingRigidbodies.Add(rigidbody);

		// Call callback on components that implement interface IPhysicsCallbackListener
		for (int i = 0; i < GameObject.Components.Count; i++)
		{
			if (GameObject.Components[i] is Rigidbody == false)
			{
				GameObject.Components[i].OnTriggerEnter(rigidbody);
			}
		}
	}

	public override void OnTriggerExit(Rigidbody rigidbody)
	{
		if (TouchingRigidbodies.Contains(rigidbody))
		{
			TouchingRigidbodies.Remove(rigidbody);
		}

		for (int i = 0; i < GameObject.Components.Count; i++)
		{
			if (GameObject.Components[i] is Rigidbody == false)
			{
				GameObject.Components[i].OnTriggerExit(rigidbody);
			}
		}
	}
}