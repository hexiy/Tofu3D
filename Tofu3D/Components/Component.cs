using System.ComponentModel;
using System.Reflection;

namespace Scripts;

public class Component : IDestroyable, ICloneable
{
    private bool _enabled = true;

    private readonly Dictionary<string, MethodInfo> _executeInEditModeMethods = new();

    [Hide] public bool AllowMultiple = true;

    [XmlIgnore] [DefaultValue(false)] public bool Awoken;

    [XmlIgnore] public GameObject GameObject;

    public int GameObjectId;
    public bool Started;

    public Component()
    {
        var info = GetType().GetMethod("Update");
        if (info == null)
        {
            return;
        }

        CanExecuteUpdateInEditMode = GetType().GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;
        CanExecuteUpdateInEditMode = CanExecuteUpdateInEditMode ||
                                     info.GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;
    }
#if DEBUG
    public float UpdateTime { get; set; } // how long in ms it took to update this gameobject
#endif

    [XmlIgnore] public bool CanExecuteUpdateInEditMode { get; }

    public bool Enabled
    {
        get => _enabled;
        set => SetEnabled(value);
    }

    public virtual bool CanBeDisabled => true;

    public bool IsActive => GameObject.ActiveInHierarchy && Enabled;

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
        Scene.ComponentRemoved(this);
    }

    public bool CallComponentExecuteInEditModeMethod(string methodName)
    {
        if (methodName == "Update")
        {
            return CanExecuteUpdateInEditMode;
        }

        var type = GetType();
        var typeString = type.ToString();
        var typeAndMethodString = string.Concat(typeString, methodName);

        var methodHasExecuteInEditModeAttrib = false;
        if (_executeInEditModeMethods.ContainsKey(typeAndMethodString) == false)
        {
            methodHasExecuteInEditModeAttrib = type.GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;

            var info = type.GetMethod(methodName);
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

    private void SetEnabled(bool tgl)
    {
        if (CanBeDisabled == false && tgl == false)
        {
            return;
        }

        var changedState = Enabled != tgl;
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

    public T GetComponent<T>(int? index = null) where T : Component => GameObject.GetComponent<T>(index);

    public TComponent AddComponent<TComponent>() where TComponent : Component, new()
    {
        TComponent component = new();

        return GameObject.AddComponent<TComponent>();
    }

    public bool HasComponent<T>() where T : Component => GameObject.HasComponent<T>();

    public List<T> GetComponents<T>() where T : Component => GameObject.GetComponents<T>();

    // Doesnt respect rotation
    public Vector3 TransformToWorld(Vector3 localPoint) => localPoint + Transform.WorldPosition;

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
    ///     Called when component/gameobject is enabled(including creation after Awake() and Start()
    /// </summary>
    public virtual void OnEnabled()
    {
        Scene.ComponentEnabled(this);
    }

    /// <summary>
    ///     Called when component/gameobject is disabled
    /// </summary>
    public virtual void OnDisabled()
    {
        Scene.ComponentDisabled(this);
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