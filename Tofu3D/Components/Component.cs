using System.ComponentModel;
using System.Reflection;

namespace Scripts;

public class Component : IDestroyable, ICloneable
{
    Dictionary<string, MethodInfo> _executeInEditModeMethods = new Dictionary<string, MethodInfo>();
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
				Debug.Log(ex.InnerException);
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
	bool _enabled = true;
	public bool Enabled
	{
		get { return _enabled; }
		set { SetEnabled(value); }
	}
	public virtual bool CanBeDisabled => true;

	private void SetEnabled(bool tgl)
	{
		if (CanBeDisabled == false && tgl == false)
		{
			return;
		}

		bool changedState = Enabled != tgl;
		_enabled = tgl;
		if (changedState)
		{
			if (Enabled)
			{
				OnEnabled();
			}
			else
			{
				OnDisabled();
			}
		}
	}

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
		Scene.ComponentAwoken(this);
	}

	public virtual void Start()
	{
		Started = true;
	}

	/// <summary>
	/// Called when component/gameobject is enabled(including creation after Awake() and Start()
	/// </summary>
	public virtual void OnEnabled()
	{
		Scene.ComponentEnabled(this);
	}

	/// <summary>
	/// Called when component/gameobject is disabled
	/// </summary>
	public virtual void OnDisabled()
	{
		Scene.ComponentDisabled(this);
	}

	public virtual void OnDestroyed()
	{
		Scene.ComponentRemoved(this);
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

	public object Clone()
	{
		return this.MemberwiseClone();

		/*object memberwiseClone = this.MemberwiseClone();
		Component clone = (Component) memberwiseClone;
		
		clone.GameObjectId = -1;
		clone.GameObject = null;
		
		return (object) clone;*/
	}
}