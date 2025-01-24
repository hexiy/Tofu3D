using Tofu3D.Physics;

namespace Scripts;

[ExecuteInEditMode]
public class Rigidbody : Component
{
    [Hide]
    public new bool AllowMultiple = false;

    public float AngularDrag = 1f;
    public Vector2 BodyPos;

    [XmlIgnore]
    public List<Rigidbody> TouchingRigidbodies = new();

    [XmlIgnore]
    // //[LinkableComponent]
    public Shape Shape => GetComponent<Shape>();

    public override void Awake()
    {
        CreateBody();

        base.Awake();
    }

    public void CreateBody()
    {
        var boxShape = GetComponent<BoxShape>();

        if (boxShape != null)
        {
            Tofu.PhysicsController.AddRigidbody(this);
        }
    }

    public override void OnNewComponentAdded(Component comp)
    {
        if (comp is BoxShape)
        {
            CreateBody();
        }

        base.OnNewComponentAdded(comp);
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
        for (var i = 0; i < TouchingRigidbodies.Count; i++)
        {
            TouchingRigidbodies[i].OnCollisionExit(this);
            OnCollisionExit(TouchingRigidbodies[i]);
        }
    }

    public override void
        OnCollisionEnter(
            Rigidbody rigidbody)
    {
        TouchingRigidbodies.Add(rigidbody);

        // Call callback on components that implement interface IPhysicsCallbackListener
        for (var i = 0; i < GameObject.Components.Count; i++)
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

        for (var i = 0; i < GameObject.Components.Count; i++)
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
        for (var i = 0; i < GameObject.Components.Count; i++)
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

        for (var i = 0; i < GameObject.Components.Count; i++)
        {
            if (GameObject.Components[i] is Rigidbody == false)
            {
                GameObject.Components[i].OnTriggerExit(rigidbody);
            }
        }
    }
}