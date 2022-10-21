using System.ComponentModel;

namespace Scripts;

public class Component : IDestroyable
{
	public bool AllowMultiple = true;

	[XmlIgnore]
	[DefaultValue(false)]
	public bool Awoken;

	public bool Enabled = true;
	[XmlIgnore]
	public GameObject GameObject;

	public int GameObjectId;
	public bool Started;

	[XmlIgnore]
	public Transform Transform
	{
		get { return GameObject.Transform; }
		set { GameObject.Transform = value; }
	}

	public virtual void OnDestroyed()
	{
	}

	public T GetComponent<T>(int? index = null) where T : Component
	{
		return GameObject.GetComponent<T>(index);
	}

	public bool HasComponent<T>() where T : Component
	{
		return GameObject.HasComponent<T>();
	}

	public List<T> GetComponents<T>() where T : Component
	{
		return GameObject.GetComponents<T>();
	}

	public Vector3 TransformToWorld(Vector3 localPoint)
	{
		return localPoint + Transform.Position;
	}

	public virtual void Awake()
	{
		Awoken = true;
	}

	public virtual void Start()
	{
		Started = true;
	}

	public virtual void EditorUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void PreSceneSave()
	{
	}

	public virtual void OnCollisionEnter(Rigidbody rigidbody)
	{
	}

	public virtual void OnCollisionExit(Rigidbody rigidbody)
	{
	}

	public virtual void OnTriggerEnter(Rigidbody rigidbody)
	{
	}

	public virtual void OnTriggerExit(Rigidbody rigidbody)
	{
	}

	public virtual void OnNewComponentAdded(Component comp)
	{
	}

	public int CompareTo(bool other)
	{
		if (this == null)
		{
			return 0;
		}

		return 1;
	}

	public static implicit operator bool(Component instance)
	{
		if (instance == null)
		{
			return false;
		}

		return true;
	}
}