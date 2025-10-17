using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Scripts;

[DebuggerDisplay("Component of gameobject {GameObjectName} id {GameObjectId} | EnabledSelf:{EnabledSelf} | IsActive:{IsActive}")]
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
    public GameObject? GameObject;

    private string GameObjectName => GameObject?.Name ?? String.Empty;

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

    [XmlIgnore]
    public RectTransform? RectTransform

    {
        get => GameObject?.RectTransform;
        set
        {
            if (GameObject != null)
            {
                GameObject.Transform = value;
            }
        }
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
            methodHasExecuteInEditModeAttrib =
                type.GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;

            MethodInfo? info = type.GetMethod(methodName);
            if (methodHasExecuteInEditModeAttrib == false)
            {
                methodHasExecuteInEditModeAttrib =
                    info.GetCustomAttribute(typeof(ExecuteInEditModeAttribute), true) != null;
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

    public T? GetComponent<T>(int? index = null) where T : Component? => GameObject.GetComponent<T>(index);

    public T? GetComponent<T>(out T? component, int? index = null) where T : Component? =>
        GameObject.GetComponent<T>(out component, index);

    public T? GetComponentInParents<T>(int? index = null) where T : Component?
    {
        return GetComponentInParents<T>(out _, index);
    }

    public T? GetComponentInParents<T>(out T? component, int? index = null) where T : Component?
    {
        Transform? parent = Transform.Parent;
        while (parent != null)
        {
            parent.GetComponent<T>(out component, index);

            if (component != null)
            {
                return component;
            }
            else
            {
                parent = parent.Parent;
            }
        }

        component = null;
        return null;
    }


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
        LinkComponentFields();
    }

    /// <summary>
    ///     Called when component/gameobject is disabled
    /// </summary>
    public virtual void OnDisabled()
    {
        Scene.OnComponentDisabled(this);
    }

// public virtual void EditorUpdate()
// {
// }

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

    public void LinkComponentFields()
    {
        foreach (Component otherComponent in GameObject.Components)
        {
            if (otherComponent == this)
            {
                continue;
            }

            Type otherComponentType = otherComponent.GetType();

            List<MemberInfo> infos = GetType().GetPropertiesOrFields();
            for (int i = 0; i < infos.Count; i++)
            {
                LinkableComponent? linkableComponentAttribute = infos[i].GetCustomAttribute<LinkableComponent>();
                if (linkableComponentAttribute != null)
                {
                    if (infos[i].MemberType == MemberTypes.Field)
                    {
                        Type infoType = (infos[i] as FieldInfo).FieldType;
                        if (infoType == otherComponentType)
                        {
                            (infos[i] as FieldInfo).SetValue(this, otherComponent);
                        }
                    }

                    if (infos[i].MemberType == MemberTypes.Property)
                    {
                        Type infoType = (infos[i] as PropertyInfo).PropertyType;

                        if (infoType == otherComponentType)
                        {
                            (infos[i] as PropertyInfo).SetValue(this, otherComponent);
                        }
                    }

                    // Type parentType = sourceType1;
                    // while (parentType.BaseType != null &&
                    //        parentType.BaseType.Name.Equals("Component") ==
                    //        false) // while we  arent in component, go to parent class and find all fields there
                    // {
                    //     parentType = parentType.BaseType;
                    //
                    //     FieldInfo[] parentClassInfos = parentType.GetFields();
                    //     for (int j = 0; j < parentClassInfos.Length; j++)
                    //     {
                    //         if (parentClassInfos[j].GetCustomAttribute<LinkableComponent>() != null &&
                    //             infos[i].FieldType == sourceType2) // found linkable field in parent class
                    //         {
                    //             if (component.GetType() != Components[compIndex1].GetType())
                    //             {
                    //                 continue;
                    //             }
                    //
                    //             parentClassInfos[j].SetValue(Components[compIndex1], component);
                    //         }
                    //     }
                    // }
                }
            }
        }
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