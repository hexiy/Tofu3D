using System.Linq;
using BepuPhysics;
using Newtonsoft.Json;
using TofuEngine.Physics;

namespace Scripts;

[ExecuteInEditMode]
public class Rigidbody : Component
{
    [Hide]
    public new bool AllowMultiple = false;

    public bool IsStaticBody = true;

    [XmlIgnore]
    public List<Rigidbody> TouchingRigidbodies = new List<Rigidbody>();

    public Shape[] GetShapes()
    {
        return GetComponents<Shape>().ToArray();
    }

    [XmlIgnore]
    [JsonIgnore]
    private Shape _firstPhysicsEnabledShape;

    [XmlIgnore]
    [JsonIgnore]
    public BodyHandle? BodyHandle = null;

    [XmlIgnore]
    [JsonIgnore]
    public StaticHandle? StaticHandle = null;

    public Shape FirstPhysicsEnabledShape
    {
        get
        {
            // if (_firstPhysicsEnabledShape == null)
            // {
                _firstPhysicsEnabledShape = GetShapes().First(shape => shape.PhysicsEnabled);
            // }

            return _firstPhysicsEnabledShape;
        }
        set { _firstPhysicsEnabledShape = value; }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnEnabled()
    {
        CreateBody();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Tofu.PhysicsController.RemoveRigidbody(this);

        base.OnDisabled();
    }

    public void CreateBody()
    {
        Tofu.PhysicsController.AddRigidbody(this);
    }

    public override void OnNewComponentAdded(Component comp)
    {
        if (comp is Scripts.Shape shape)
        {
            CreateBody();
        }

        base.OnNewComponentAdded(comp);
    }

    public override void FixedUpdate()
    {
    }


    /*public override void OnDestroyed()
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
    }*/
}