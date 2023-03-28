using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scripts;

public class Component : IDestroyable
{
	static Dictionary<string, MethodInfo> _executeInEditModeMethods = new Dictionary<string, MethodInfo>();
	[XmlIgnore] public bool CanExecuteUpdateInEditMode { get; private set; } = false;

	public bool CallComponentExecuteInEditModeMethod(string methodName)
	{
		if (methodName == "Update")
		{
			return CanExecuteUpdateInEditMode;
		}
		Type type = this.GetType();
		string typeString = type.ToString();
		string typeAndMethodString = string.Concat(typeString, methodName);

		bool methodHasExecuteInEditModeAttrib = false;
		if (_executeInEditModeMethods.ContainsKey(typeAndMethodString) == false)
		{
			methodHasExecuteInEditModeAttrib = type.GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;

			MethodInfo info = type.GetMethod(methodName);
			if (methodHasExecuteInEditModeAttrib == false)
			{
				methodHasExecuteInEditModeAttrib = info.GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;
			}

			_executeInEditModeMethods[typeAndMethodString] = methodHasExecuteInEditModeAttrib ? info : null;
		}
		else
		{
			methodHasExecuteInEditModeAttrib = _executeInEditModeMethods[typeAndMethodString] != null;
		}


		if (methodHasExecuteInEditModeAttrib)
		{
			try
			{
			_executeInEditModeMethods[typeAndMethodString]?.Invoke(this, null);
			// type.GetMethod(methodName)?.Invoke(this, null);
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
				// throw ex;
			}

			return true;
		}

		return false;
	}

	public Component()
	{
		MethodInfo info = this.GetType().GetMethod("Update");
		CanExecuteUpdateInEditMode = this.GetType().GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;
		CanExecuteUpdateInEditMode = CanExecuteUpdateInEditMode || info.GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;
	}

	public bool AllowMultiple = true;

	[XmlIgnore]
	[DefaultValue(false)]
	public bool Awoken;
	public bool Enabled = true;
	public bool IsActive
	{
		get { return GameObject.ActiveInHierarchy && Enabled; }
	}

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

	public TComponent AddComponent<TComponent>() where TComponent : Scripts.Component, new()
	{
		TComponent component = new();

		return GameObject.AddComponent<TComponent>();
	}

	public bool HasComponent<T>() where T : Component
	{
		return GameObject.HasComponent<T>();
	}

	public List<T> GetComponents<T>() where T : Component
	{
		return GameObject.GetComponents<T>();
	}

// Doesnt respect rotation
	public Vector3 TransformToWorld(Vector3 localPoint)
	{
		return localPoint + Transform.WorldPosition;
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

	public virtual void Dispose()
	{
	}
}