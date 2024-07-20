using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Component = Scripts.Component;

namespace Tofu3D;

public class GameObject : IEqualityComparer<GameObject>, IComparable<bool>
{
    public bool IsStatic => IsStaticSelf || Transform?.Parent?.GameObject.IsStatic == true;
    public bool IsStaticSelf = false;

    private bool _activeSelf = true;

    public bool ActiveSelf
    {
        get => _activeSelf;
        set => SetActive(value);
    }

    public bool AlwaysUpdate = false;

    [DefaultValue(false)]
    public bool Awoken;

    //[System.Xml.Serialization.XmlArrayItem(type: typeof(Component))]
    [XmlIgnore]
    public List<Component> Components = new();

    private object _componentsLock = new();
    private bool _destroyTimerRunning;
    public float DestroyTimer = 2;
    public bool DynamicallyCreated = false;
    public int Id = -1;

    [Hide]
    public int IndexInHierarchy = 0;

    public bool IsPrefab = false;
    public string Name = "";
    public string PrefabPath = "";
    public bool Selected = false;
    public bool Silent;
    public bool Started;

    /*		[XmlIgnore]
            public GameObject Parent
            {
                get
                {
                    int index = Tofu.SceneManager.CurrentScene.GetGameObjectIndexInHierarchy(parentID);
                    if (index != -1)
                    { return Tofu.SceneManager.CurrentScene.gameObjects[index]; }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    int index = Tofu.SceneManager.CurrentScene.GetGameObjectIndexInHierarchy(parentID);

                    parentID = (int)value.id;
                    if (index != -1)
                    {
                        Tofu.SceneManager.CurrentScene.gameObjects[Tofu.SceneManager.CurrentScene.GetGameObjectIndexInHierarchy(parentID)] = value;
                    }
                }
            }
            public int parentID { get; set; } = -1;*/

    public bool UpdateWhenDisabled = false;

    public GameObject()
    {
        // OnComponentAdded += LinkComponents;
    }

    public void SetActive(bool tgl)
    {
        bool stateChanged = ActiveSelf != tgl;
        _activeSelf = tgl;
        if (stateChanged && Started)
        {
            if (_activeSelf)
                OnEnable();
            else
                OnDisable();
        }
    }

    private void OnEnable()
    {
        Components.ForEach(c => c.OnEnabled());
        Transform?.Children.ForEach(child => child.GameObject.OnEnable());
    }

    private void OnDisable()
    {
        Components.ForEach(c => c.OnDisabled());
        Transform?.Children.ForEach(child => child.GameObject.OnDisable());
    }

    public bool ActiveInHierarchy
    {
        get
        {
            if (Transform.Parent == null) return ActiveSelf;

            if (Transform.Parent.GameObject.ActiveSelf == false) return false;

            return Transform.Parent.GameObject.ActiveInHierarchy && ActiveSelf;
        }
    }

    [XmlIgnore]
    public Transform Transform { get; set; }
    //private List<Component> ComponentsWaitingToBePaired = new List<Component>();

    public static GameObject Create(Vector3? position = null, Vector3? scale = null, string name = "",
        bool linkComponents = true, bool silent = false, bool addToScene = true)
    {
        GameObject go = new();

        go.Name = name;
        go.Silent = silent;


        if (go.Id == -1) go.AssignNewId();

        if (go.Transform == null && go.GetComponent<Transform>() == null) go.Transform = go.AddComponent<Transform>();

        if (addToScene) Tofu.SceneManager.CurrentScene.AddGameObjectToScene(go);

        if (position != null) go.Transform.WorldPosition = position.Value;

        if (scale != null) go.Transform.LocalScale = scale.Value;

        return go;
    }

    public void AssignNewId()
    {
        Id = IDsManager.GameObjectNextId;
        IDsManager.GameObjectNextId++;


        for (int i = 0; i < Components.Count; i++)
        {
            Components[i].GameObjectId = Id;
            Components[i].GameObject = this;
        }
    }

    private void DestroyChildren()
    {
        for (int i = 0; i < Transform.Children.Count; i++) Transform.Children[i].GameObject.Destroy();
    }

    private void CheckForTransformComponent(Component component)
    {
        if (component is Transform) Transform = component as Transform;
    }

    private void InvokeOnComponentAddedOnComponents(Component comp)
    {
        for (int i = 0; i < Components.Count; i++) Components[i].OnNewComponentAdded(comp);
    }

    public void LinkGameObjectFieldsInComponents()
    {
        // find "GameObject" members and find them in the scene
        for (int c = 0; c < Components.Count; c++)
        {
            Component component = Components[c];

            Type sourceType1 = component.GetType();

            FieldInfo[] infos = sourceType1.GetFields();
            for (int i = 0; i < infos.Length; i++)
                if (infos[i].FieldType == typeof(GameObject) && infos[i].Name != "gameObject")
                {
                    GameObject goFieldValue = infos[i].GetValue(component) as GameObject;
                    if (goFieldValue == null) continue;

                    if (goFieldValue.IsPrefab)
                    {
                        GameObject loadedGo = Tofu.SceneSerializer.LoadPrefab(goFieldValue.PrefabPath, true);
                        infos[i].SetValue(component, loadedGo);
                    }
                    else
                    {
                        GameObject foundGameObject = Tofu.SceneManager.CurrentScene.GetGameObject(goFieldValue.Id);
                        infos[i].SetValue(component, foundGameObject);
                    }
                }

            for (int i = 0; i < infos.Length; i++)
                if (infos[i].FieldType == typeof(List<GameObject>))
                {
                    List<GameObject> gosFieldValue = infos[i].GetValue(component) as List<GameObject>;
                    if (gosFieldValue == null) continue;

                    for (int goIndex = 0; goIndex < gosFieldValue.Count; goIndex++)
                    {
                        if (gosFieldValue[goIndex] == null) continue;

                        gosFieldValue[goIndex] =
                            Tofu.SceneManager.CurrentScene.GetGameObject(gosFieldValue[goIndex].Id);
                        //	gosFieldValue[i].Components();
                    }

                    infos[i].SetValue(component, gosFieldValue);
                }
        }
    }

    /*public void LinkComponents(GameObject gameObject, Component component)
    {
        for (int compIndex1 = 0; compIndex1 < Components.Count; compIndex1++)
        {
            if (Components[compIndex1] == component)
            {
                continue;
            }

            // BoxRenderer -> Renderer -> Component
            // BoxShape    -> Shape    -> Component

            //                Renderer containts BoxShape

            // so go through component AND all of the parent classes, if we find a fitting type of comp2 type and it's linkable, assign it, and continue

            // shape might be added first, not linked to anything that was added before, so do the same but reversed- for component

            {
                Type sourceType1 = Components[compIndex1].GetType();
                Type sourceType2 = component.GetType();

                FieldInfo[] infos = sourceType1.GetFields();
                for (int i = 0; i < infos.Length; i++)
                {
                    LinkableComponent a = infos[i].GetCustomAttribute<LinkableComponent>();
                    Type b = infos[i].GetType();
                    if (infos[i].GetCustomAttribute<LinkableComponent>() != null
                     && infos[i].FieldType == sourceType2) // we found field that can be connected- its LinkableComponent attributed and has a type of component2
                    {
                        infos[i].SetValue(Components[compIndex1], component);

                        Type parentType = sourceType1;
                        while (parentType.BaseType != null && parentType.BaseType.Name.Equals("Component") == false) // while we  arent in component, go to parent class and find all fields there
                        {
                            parentType = parentType.BaseType;

                            FieldInfo[] parentClassInfos = parentType.GetFields();
                            for (int j = 0; j < parentClassInfos.Length; j++)
                            {
                                if (parentClassInfos[j].GetCustomAttribute<LinkableComponent>() != null && infos[i].FieldType == sourceType2) // found linkable field in parent class
                                {
                                    if (component.GetType() != Components[compIndex1].GetType())
                                    {
                                        continue;
                                    }

                                    parentClassInfos[j].SetValue(Components[compIndex1], component);
                                }
                            }
                        }
                    }
                }
            }

            {
                Type sourceType1 = component.GetType();
                Type sourceType2 = Components[compIndex1].GetType();

                FieldInfo[] infos = sourceType1.GetFields();
                for (int i = 0; i < infos.Length; i++)
                {
                    if (infos[i].GetCustomAttribute<LinkableComponent>() != null
                     && infos[i].FieldType == sourceType2) // we found field that can be connected- its LinkableComponent attributed and has a type of component2
                    {
                        infos[i].SetValue(component, Components[compIndex1]);

                        Type parentType = sourceType2;
                        while (parentType.BaseType != null && parentType.BaseType.Name.Equals("Component") == false) // while we  arent in component, go to parent class and find all fields there
                        {
                            parentType = parentType.BaseType;

                            FieldInfo[] parentClassInfos = parentType.GetFields();
                            for (int j = 0; j < parentClassInfos.Length; j++)
                            {
                                if (parentClassInfos[j].GetCustomAttribute<LinkableComponent>() != null && infos[i].FieldType == sourceType2) // found linkable field in parent class
                                {
                                    parentClassInfos[j].SetValue(component, Components[compIndex1]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }*/

    /// <summary>
    ///         give every found component in class its gameobject and transform reference
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="component"></param>
    public void InitializeMemberComponents(Component component)
    {
        component.Transform = Transform;
        component.GameObject = this;
        return;
        Type sourceType = component.GetType();

        // fields that are derived from Component
        List<FieldInfo> componentFields = new();

        // Find all fields that derive from Component
        componentFields.AddRange(sourceType.GetFields().Where(info => info.FieldType.IsSubclassOf(typeof(Component))));

        List<FieldInfo> gameObjectFields = new();
        List<FieldInfo> transformFields = new();
        for (int i = 0; i < componentFields.Count; i++)
        {
            PropertyInfo gameObjectFieldInfo = componentFields[0].FieldType.GetProperty("gameObject");
            PropertyInfo transformFieldInfo = componentFields[0].FieldType.GetProperty("transform");

            gameObjectFieldInfo?.SetValue(component, this);
            transformFieldInfo?.SetValue(component, Transform);
        }
    }

    public virtual void Awake()
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].Awoken == false) // && Components[i].Enabled)
            {
                if (Global.GameRunning == false)
                {
                    bool foundMethod = CallComponentExecuteInEditModeMethod(Components[i], nameof(Awake));
                }
                else
                {
                    Components[i].Awake();
                }

                // if (Components[i].Awoken == false)
                //     Debug.LogError(
                //         $"Couldn't awaken component [{Components[i].GetType().ToString()}] with gameobjectID {Id}");
            }

        Awoken = true;
    }

    public bool CallComponentExecuteInEditModeMethod(Component component, string methodName)
    {
        return component.CallComponentExecuteInEditModeMethod(methodName);
    }

    public virtual void PreSceneSave()
    {
        for (int i = 0; i < Components.Count; i++) Components[i].PreSceneSave();
    }

    public virtual void Start()
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].Enabled)
            {
                if (Global.GameRunning == false)
                {
                    bool foundMethod = CallComponentExecuteInEditModeMethod(Components[i], nameof(Start));
                }
                else
                {
                    Components[i].Start();
                }
            }

        if (ActiveInHierarchy) OnEnable();

        Started = true;
    }

    private void RemoveFromLists()
    {
        // Rigidbody rb = GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        // 	for (int i = 0; i < rb.TouchingRigidbodies.Count; i++)
        // 	{
        // 		rb.TouchingRigidbodies[i].TouchingRigidbodies.Remove(rb);
        // 	}
        // }

        // lock (_componentsLock)
        // {
        for (int i = 0; i < Components.Count; i++) Components[i].OnDestroyed();

        // Components.Clear();
        // }

        if (Transform.Parent != null) Transform.Parent.RemoveChild(Id);

        Tofu.SceneManager.CurrentScene.OnGameObjectDestroyed(this);
    }

    public void Destroy()
    {
        SetActive(false);
        RemoveFromLists();
        DestroyChildren();
        Id = -1;
    }

    public void DestroyDelayed(float delay)
    {
        _destroyTimerRunning = true;
        DestroyTimer = (float)delay;
    }

    /*
    public virtual void EditorUpdate()
    {
        if (activeInHierarchy == false && updateWhenDisabled == false)
        {
            return;
        }

        if (awoken == false)
        {
            return;
        }


        EditorUpdateComponents();
    }
    */

    // public virtual void Update()
    // {
    //     if (ActiveInHierarchy == false && UpdateWhenDisabled == false) return;
    //
    //     if (Awoken == false) return;
    //
    //     if (_destroyTimerRunning)
    //     {
    //         DestroyTimer -= Time.DeltaTime;
    //         if (DestroyTimer < 0)
    //         {
    //             _destroyTimerRunning = false;
    //             Destroy();
    //             return;
    //         }
    //     }
    //
    //     // UpdateComponents();
    // }

    public virtual void FixedUpdate()
    {
        if (ActiveInHierarchy == false && UpdateWhenDisabled == false) return;

        FixedUpdateComponents();
    }

    public Component AddExistingComponent(Component comp)
    {
        comp.GameObject = this;
        comp.GameObjectId = Id;

        Components.Add(comp);

        if (Awoken) comp.Awake();

        InvokeOnComponentAddedOnComponents(comp);
        CheckForTransformComponent(comp);
        /* for (int i = 0; i < ComponentsWaitingToBePaired.Count; i++)
         {
               if (ComponentsWaitingToBePaired[i].GetType() == type)
               {
                     ComponentsWaitingToBePaired[i] = component;
                     ComponentsWaitingToBePaired.RemoveAt(i);
                     break;
               }
         }*/
        return comp;
    }

    public TComponent AddComponent<TComponent>() where TComponent : Component, new()
    {
        TComponent component = new();

        return AddComponent(component.GetType()) as TComponent;
    }

    public Component AddComponent(Type type)
    {
        /* if ((transform != null || GetComponent<Transform>() != null) && type == typeof(Transform)) {
                return null;
          }*/
        Component component = (Component)Activator.CreateInstance(type);

        if (component.AllowMultiple == false && GetComponent(type))
        {
            component = null;
            return GetComponent(type);
        }

        component.GameObject = this;
        component.GameObjectId = Id;

        Components.Add(component);

        if (Awoken && component.Awoken == false)
        {
            if (Global.GameRunning == false)
            {
                bool foundMethod = CallComponentExecuteInEditModeMethod(component, nameof(Awake));
            }
            else
            {
                component.Awake();
            }
        }

        if (Started && component.Started == false)
        {
            if (Global.GameRunning == false)
            {
                bool foundMethod = CallComponentExecuteInEditModeMethod(component, nameof(Start));
            }
            else
            {
                component.Start();
            }
        }

        /* for (int i = 0; i < ComponentsWaitingToBePaired.Count; i++)
         {
               if (ComponentsWaitingToBePaired[i].GetType() == type)
               {
                     ComponentsWaitingToBePaired[i] = component;
                     ComponentsWaitingToBePaired.RemoveAt(i);
                     break;
               }
         }*/
        return component;
    }

    public void RemoveComponent(int index)
    {
        RemoveComponent(Components[index]);
    }

    public void RemoveComponent(Component component)
    {
        // SetActive(false);
        Components.Remove(component);
        component.OnDestroyed();
        // SetActive(true);
    }

    public void RemoveComponent<T>() where T : Component
    {
        RemoveComponent(typeof(T));
    }

    public void RemoveComponent(Type type)
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].GetType() == type)
            {
                RemoveComponent(Components[i]);
                return;
            }
    }

    public T GetComponent<T>(int? index = null) where T : Component
    {
        int k = index == null ? 0 : (int)index;
        for (int i = 0; i < Components.Count; i++)
            if (Components[i] is T)
            {
                if (k == 0) return (T)Components[i];

                k--;
            }

        return null;
    }

    public bool HasComponent<T>() where T : Component
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i] is T)
                return true;

        return false;
    }

    public List<T> GetComponents<T>() where T : Component
    {
        List<T> componentsToReturn = new();
        for (int i = 0; i < Components.Count; i++)
            if (Components[i] is T)
                componentsToReturn.Add(Components[i] as T);

        return componentsToReturn;
    }

    public Component GetComponent(Type type)
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].GetType() == type)
                return Components[i];

        return null;
    }

    public List<Component> GetComponents(Type type)
    {
        List<Component> componentsToReturn = new();
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].GetType() == type)
                componentsToReturn.Add(Components[i]);

        return componentsToReturn;
    }

    /*void EditorUpdateComponents()
    {
        lock (_componentsLock)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].Enabled && Components[i].Awoken)
                {
                    Components[i].EditorUpdate();
                }
            }
        }
    }*/

    // components are now updated independently of gameobject, in the IUpdateableComponentQueue
    // private void UpdateComponents()
    // {
    //     lock (_componentsLock)
    //     {
    //         for (int i = 0; i < Components.Count; i++)
    //             if (Components[i].Enabled && Components[i].Awoken)
    //             {
    //                 if (Global.GameRunning == false)
    //                 {
    //                     if (Components[i].CanExecuteUpdateInEditMode) Components[i].Update();
    //                     // bool foundMethod = CallComponentExecuteInEditModeMethod(Components[i], nameof(Update));
    //                 }
    //                 else
    //                 {
    //                     Components[i].Update();
    //                 }
    //             }
    //     }
    // }

    // public void Update_Test1()
    // {
    //     if (ActiveInHierarchy == false && UpdateWhenDisabled == false) return;
    //
    //     if (Awoken == false) return;
    //
    //     if (_destroyTimerRunning)
    //     {
    //         DestroyTimer -= Time.DeltaTime;
    //         if (DestroyTimer < 0)
    //         {
    //             _destroyTimerRunning = false;
    //             Destroy();
    //             return;
    //         }
    //     }
    //
    //     UpdateComponents_Test1();
    // }
    //
    // public void UpdateComponents_Test1()
    // {
    //     for (int i = 0; i < Components.Count; i++)
    //         if (Components[i].Enabled && Components[i].Awoken)
    //         {
    //             if (Global.GameRunning == false)
    //             {
    //                 if (Components[i].CanExecuteUpdateInEditMode) Components[i].Update();
    //                 // bool foundMethod = CallComponentExecuteInEditModeMethod(Components[i], nameof(Update));
    //             }
    //             else
    //             {
    //                 Components[i].Update();
    //             }
    //         }
    // }

    // private void UpdateRenderers()
    // {
    //     lock (_componentsLock)
    //     {
    //         for (int i = 0; i < Components.Count; i++)
    //             if (Components[i].Enabled && Components[i].Awoken && Components[i] is Renderer)
    //                 Components[i].Update();
    //     }
    // }

    private void FixedUpdateComponents()
    {
        for (int i = 0; i < Components.Count; i++)
            if (Components[i].Enabled && Components[i].Awoken)
                Components[i].FixedUpdate();
    }
    /* public void RegisterComponentPair(Component comp, Type type)
     {
           comp = GetComponent(type);
           if (comp == null)
           {
                 ComponentsWaitingToBePaired.Add(comp);
           }
     }*/

    public Vector3 TransformToWorld(Vector3 localPoint)
    {
        return localPoint + Transform.WorldPosition;
    }

    public Vector3 TransformToLocal(Vector3 worldPoint)
    {
        return worldPoint - Transform.WorldPosition;
    }

    public bool Equals(GameObject x, GameObject y)
    {
        return x?.Id == y?.Id;
    }

    public int GetHashCode(GameObject obj)
    {
        return obj.Id;
    }

    public int CompareTo(bool other)
    {
        if (this == null) return 0;

        return 1;
    }

    public static implicit operator bool(GameObject instance)
    {
        if (instance == null) return false;

        return true;
    }

    public object Clone(bool active = true)
    {
        object memberwiseClone = MemberwiseClone();
        GameObject clone = (GameObject)memberwiseClone;


        clone.Components = new List<Component>();

        for (int i = 0; i < Components.Count; i++)
        {
            Component componentClone = (Component)Components[i].Clone();
            if (componentClone is Renderer)
            {
                (componentClone as Renderer).InstancingData.InstancedRenderingStartingIndexInBuffer = -1;
                (componentClone as Renderer).InstancingData.InstancedRenderingDefinitionIndex = -1;
            }

            clone.AddExistingComponent(componentClone);
            // clone.Components.Add(componentClone);
        }

        clone.AssignNewId();

        clone.Transform = clone.GetComponent<Transform>();

        clone.Awoken = false;
        clone.Started = false;

        for (int i = 0; i < clone.Components.Count; i++)
        {
            clone.Components[i].Awoken = false;
            clone.Components[i].Started = false;
        }

        clone._activeSelf = active;
        clone.Awake();
        clone.Start();

        return (object)clone;
    }
}