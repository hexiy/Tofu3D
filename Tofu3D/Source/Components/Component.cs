using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Scripts;

public class Component : IDestroyable, ICloneable
{
    private bool _enabledSelfSelf = true;

    private readonly Dictionary<string, MethodInfo> _executeInEditModeMethods = new Dictionary<string, MethodInfo>();

    [Hide]
    public bool AllowMultiple = true;

    [XmlIgnore]
    [DefaultValue(false)]
    public bool Awoken;

    [XmlIgnore]
    public GameObject GameObject;

    public int GameObjectId;
    public bool Started;

    public Component()
    {
        MethodInfo? info = GetType().GetMethod("Update");
        if (info == null)
        {
            return;
        }

        CanExecuteUpdateInEditMode = GetType().GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;
        CanExecuteUpdateInEditMode = CanExecuteUpdateInEditMode ||
                                     info.GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;
    }
#if DEBUG
    public float UpdateTime { get; set; } // how long in ms it took to update this gameobject
#endif

    [XmlIgnore]
    public bool CanExecuteUpdateInEditMode { get; }

    public bool EnabledSelf
    {
        get => _enabledSelfSelf;
        set => SetEnabled(value);
    }

    public virtual bool CanBeDisabled => true;

    public bool IsActive => GameObject.ActiveInHierarchy && EnabledSelf;

    [XmlIgnore]
    public Transform Transform

    {
        get => GameObject.Transform;
        set => GameObject.Transform = value;
    }

    public object Clone() => MemberwiseClone();

    /*object memberwiseClone = this.MemberwiseClone();
        Component clone = (Component) memberwiseClone;

        clone.GameObjectId = -1;
        clone.GameObject = null;

        return (object) clone;*/
    public virtual void OnDestroyed()
    {
        Scene.OnComponentRemoved(this);
    }

    public bool CallComponentExecuteInEditModeMethod(string methodName)
    {
        if (methodName == "Update")
        {
            return CanExecuteUpdateInEditMode;
        }

        Type type = GetType();
        string typeString = type.ToString();
        string typeAndMethodString = string.Concat(typeString, methodName);

        bool methodHasExecuteInEditModeAttrib = false;
        if (_executeInEditModeMethods.ContainsKey(typeAndMethodString) == false)
        {
            methodHasExecuteInEditModeAttrib = type.GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;

            MethodInfo? info = type.GetMethod(methodName);
            if (methodHasExecuteInEditModeAttrib == false)
            {
                methodHasExecuteInEditModeAttrib = info.GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;
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

    private void SetEnabled(bool tgl)
    {
        if (CanBeDisabled == false && tgl == false)
        {
            return;
        }

        bool changedState = EnabledSelf != tgl;
        _enabledSelfSelf = tgl;
        if (changedState)
        {
            if (EnabledSelf)
            {
                OnEnabled();
            }
            else
            {
                OnDisabled();
            }
        }
    }

    public T GetComponent<T>(int? index = null) where T : Component => GameObject.GetComponent<T>(index);

    public T GetComponent<T>(out T component, int? index = null) where T : Component =>
        GameObject.GetComponent<T>(out component, index);

    public TComponent AddComponent<TComponent>() where TComponent : Component, new()
    {
        TComponent component = new TComponent();

        return GameObject.AddComponent<TComponent>();
    }

    public bool HasComponent<T>() where T : Component => GameObject.HasComponent<T>();

    public List<T> GetComponents<T>() where T : Component => GameObject.GetComponents<T>();

    // Doesnt respect rotation
    public Vector3 TransformToWorld(Vector3 localPoint) => localPoint + Transform.WorldPosition;

    public virtual void Awake()
    {
        Awoken = true;
        Scene.OnComponentAwoken(this);
        // OnEnabled();
    }

    public virtual void Start()
    {
        Started = true;
        if (GameObject.ActiveInHierarchy)
        {
            OnEnabled();
        }
    }

    /// <summary>
    ///     Called when component/gameobject is enabled(including creation after Awake() and Start()
    /// </summary>
    public virtual void OnEnabled()
    {
        Scene.OnComponentEnabled(this);
    }

    /// <summary>
    ///     Called when component/gameobject is disabled
    /// </summary>
    public virtual void OnDisabled()
    {
        Scene.OnComponentDisabled(this);
    }

    public virtual void EditorUpdate()
    {
    }

    // public virtual void Update()
    // {
    // }

    public virtual void FixedUpdate()
    {
    }

    public virtual void PreSceneSave()
    {
    }

    public virtual void OnSelectedChanged(bool isSelected)
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
        if (ReferenceEquals(this, null))
        {
            return 0;
        }

        return 1;
    }

    public static implicit operator bool(Component? instance)
    {
        if (ReferenceEquals(instance, null))
        {
            return false;
        }

        return true;
    }

    public static bool operator ==(Component? left, Component? right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(Component? left, Component? right)
    {
        return !(left == right);
    }

    protected void StartCoroutine(IEnumerator routine)
    {
        Tofu.CoroutineManager.StartCoroutine(routine);
    }
}