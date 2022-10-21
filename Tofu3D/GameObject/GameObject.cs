using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Component = Scripts.Component;

namespace Tofu3D;

public class GameObject
{
	public delegate void ComponentAdded(GameObject gameObject, Component component);

	//Destroying 
	public delegate void Destroyed(GameObject gameObject);

	public bool ActiveSelf = true;
	public bool AlwaysUpdate = false;

	[DefaultValue(false)]
	public bool Awoken;

	//[System.Xml.Serialization.XmlArrayItem(type: typeof(Component))]
	[XmlIgnore]
	public List<Component> Components = new();
	object _componentsLock = new();
	bool _destroy;
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
					int index = Scene.I.GetGameObjectIndexInHierarchy(parentID);
					if (index != -1)
					{ return Scene.I.gameObjects[index]; }
					else
					{
						return null;
					}
				}
				set
				{
					int index = Scene.I.GetGameObjectIndexInHierarchy(parentID);

					parentID = (int)value.id;
					if (index != -1)
					{
						Scene.I.gameObjects[Scene.I.GetGameObjectIndexInHierarchy(parentID)] = value;
					}
				}
			}
			public int parentID { get; set; } = -1;*/

	public bool UpdateWhenDisabled = false;

	public GameObject()
	{
		OnDestroyed += RemoveFromLists;
		OnDestroyed += DestroyChildren;

		OnComponentAdded += LinkComponents;
		OnComponentAdded += InvokeOnComponentAddedOnComponents;
		OnComponentAdded += CheckForTransformComponent;
		OnComponentAdded += Scene.I.OnComponentAdded;
	}

	public bool ActiveInHierarchy
	{
		get
		{
			if (Transform.Parent == null)
			{
				return ActiveSelf;
			}

			return Transform.Parent.GameObject.ActiveInHierarchy && ActiveSelf;
		}
	}

	[XmlIgnore] public Transform Transform { get; set; }
	public event ComponentAdded OnComponentAdded;
	public event Destroyed OnDestroyed;

	//private List<Component> ComponentsWaitingToBePaired = new List<Component>();

	public void Setup()
	{
		if (Id == -1)
		{
			Id = DsManager.GameObjectNextId;
			DsManager.GameObjectNextId++;
		}

		if (Transform == null && GetComponent<Transform>() == null)
		{
			Transform = AddComponent<Transform>();
		}

		Scene.I.AddGameObjectToScene(this);
	}

	public static GameObject Create(Vector3? position = null, Vector3? scale = null, string name = "", bool linkComponents = true, bool silent = false)
	{
		GameObject go = new();

		go.Name = name;
		go.Silent = silent;
		go.Setup();
		if (position != null)
		{
			go.Transform.Position = position.Value;
		}

		if (scale != null)
		{
			go.Transform.Scale = scale.Value;
		}

		return go;
	}

	void DestroyChildren(GameObject go)
	{
		for (int i = 0; i < Transform.Children.Count; i++)
		{
			Transform.Children[i].GameObject.Destroy();
		}
	}

	void CheckForTransformComponent(GameObject gameObject, Component component)
	{
		if (component is Transform)
		{
			Transform = component as Transform;
		}
	}

	void InvokeOnComponentAddedOnComponents(GameObject go, Component comp)
	{
		for (int i = 0; i < Components.Count; i++)
		{
			Components[i].OnNewComponentAdded(comp);
		}
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
			{
				if (infos[i].FieldType == typeof(GameObject) && infos[i].Name != "gameObject")
				{
					GameObject goFieldValue = infos[i].GetValue(component) as GameObject;
					if (goFieldValue == null)
					{
						continue;
					}

					if (goFieldValue.IsPrefab)
					{
						GameObject loadedGo = Serializer.I.LoadPrefab(goFieldValue.PrefabPath, true);
						infos[i].SetValue(component, loadedGo);
					}
					else
					{
						GameObject foundGameObject = Scene.I.GetGameObject(goFieldValue.Id);
						infos[i].SetValue(component, foundGameObject);
					}
				}
			}

			for (int i = 0; i < infos.Length; i++)
			{
				if (infos[i].FieldType == typeof(List<GameObject>))
				{
					List<GameObject> gosFieldValue = infos[i].GetValue(component) as List<GameObject>;
					if (gosFieldValue == null)
					{
						continue;
					}

					for (int goIndex = 0; goIndex < gosFieldValue.Count; goIndex++)
					{
						gosFieldValue[goIndex] = Scene.I.GetGameObject(gosFieldValue[goIndex].Id);
						//	gosFieldValue[i].Components();
					}

					infos[i].SetValue(component, gosFieldValue);
				}
			}
		}
	}

	public void LinkComponents(GameObject gameObject, Component component)
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
	}

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
		{
			if (Components[i].Awoken == false && Components[i].Enabled)
			{
				if (Global.GameRunning == false)
				{
					bool foundMethod = CallComponentExecuteInEditModeMethod(Components[i], nameof(Awake));
				}
				else
				{
					Components[i].Awake();
				}
			}
		}

		Awoken = true;
	}

	bool CallComponentExecuteInEditModeMethod(Component comp, string methodName)
	{
		List<CustomAttributeData> compAttribs = comp.GetType().CustomAttributes.ToList();
		TypeAttributes x = comp.GetType().Attributes;
		bool compHasExecuteInEditModeAttrib = comp.GetType().GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;


		bool methodHasExecuteInEditModeAttrib = compHasExecuteInEditModeAttrib ?
			compHasExecuteInEditModeAttrib :
			comp.GetType().GetMethod(methodName).GetCustomAttribute(typeof(ExecuteInEditMode), true) != null;

		if (methodHasExecuteInEditModeAttrib)
		{
			// try
			// {
				comp.GetType().GetMethod(methodName).Invoke(comp, null);
			// }
			// catch (Exception ex)
			// {
			// 	throw ex;
			// }

			return true;
		}

		return false;
	}

	public virtual void PreSceneSave()
	{
		for (int i = 0; i < Components.Count; i++)
		{
			Components[i].PreSceneSave();
		}
	}

	public virtual void Start()
	{
		for (int i = 0; i < Components.Count; i++)
		{
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
		}

		Started = true;
	}

	void RemoveFromLists(GameObject gameObject)
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb != null)
		{
			for (int i = 0; i < rb.TouchingRigidbodies.Count; i++)
			{
				rb.TouchingRigidbodies[i].TouchingRigidbodies.Remove(rb);
			}
		}

		lock (_componentsLock)
		{
			for (int i = 0; i < Components.Count; i++)
			{
				Components[i].OnDestroyed();
			}

			Components.Clear();
		}

		if (Transform.Parent != null)
		{
			Transform.Parent.RemoveChild(Id);
		}

		Scene.I.OnGameObjectDestroyed(this);
	}

	public void Destroy(float? delay = null)
	{
		if (delay == null)
		{
			OnDestroyed?.Invoke(this);
		}
		else
		{
			_destroy = true;
			DestroyTimer = (float) delay;
		}
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

	public virtual void Update()
	{
		if (ActiveInHierarchy == false && UpdateWhenDisabled == false)
		{
			return;
		}

		if (Awoken == false)
		{
			return;
		}

		if (_destroy)
		{
			DestroyTimer -= Time.DeltaTime;
			if (DestroyTimer < 0)
			{
				_destroy = false;
				OnDestroyed?.Invoke(this);
				return;
			}
		}

		UpdateComponents();
	}

	public virtual void FixedUpdate()
	{
		if (ActiveInHierarchy == false && UpdateWhenDisabled == false)
		{
			return;
		}

		FixedUpdateComponents();
	}

	public Component AddExistingComponent(Component comp)
	{
		comp.GameObject = this;
		comp.GameObjectId = Id;

		Components.Add(comp);

		OnComponentAdded?.Invoke(this, comp);
		if (Awoken)
		{
			comp.Awake();
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
		return comp;
	}

	public TComponent AddComponent<TComponent>() where TComponent : Scripts.Component, new()
	{
		TComponent component = new();

		return AddComponent(component.GetType()) as TComponent;
	}

	public Component AddComponent(Type type)
	{
		/* if ((transform != null || GetComponent<Transform>() != null) && type == typeof(Transform)) { 
				return null;
		  }*/
		Component component = (Component) Activator.CreateInstance(type);

		if (component.AllowMultiple == false && GetComponent(type))
		{
			component = null;
			return GetComponent(type);
		}

		component.GameObject = this;
		component.GameObjectId = Id;

		Components.Add(component);

		OnComponentAdded?.Invoke(this, component);
		if (Awoken && component.Awoken == false)
		{
			component.Awake();
		}

		if (Started && component.Started == false)
		{
			component.Start();
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
		ActiveSelf = false;
		Components[index].OnDestroyed();
		Components.RemoveAt(index);
		ActiveSelf = true;
	}

	public void RemoveComponent(Component component)
	{
		ActiveSelf = false;
		component.OnDestroyed();
		Components.Remove(component);
		ActiveSelf = true;
	}

	public void RemoveComponent<T>() where T : Component
	{
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i] is T)
			{
				Components[i].OnDestroyed();
				Components.RemoveAt(i);
			}
		}
	}

	public void RemoveComponent(Type type)
	{
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i].GetType() == type)
			{
				Components[i].OnDestroyed();
				Components.RemoveAt(i);
				return;
			}
		}
	}

	public T GetComponent<T>(int? index = null) where T : Component
	{
		int k = index == null ? 0 : (int) index;
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i] is T)
			{
				if (k == 0)
				{
					return (T) Components[i];
				}

				k--;
			}
		}

		return null;
	}

	public bool HasComponent<T>() where T : Component
	{
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i] is T)
			{
				return true;
			}
		}

		return false;
	}

	public List<T> GetComponents<T>() where T : Component
	{
		List<T> componentsToReturn = new();
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i] is T)
			{
				componentsToReturn.Add(Components[i] as T);
			}
		}

		return componentsToReturn;
	}

	public Component GetComponent(Type type)
	{
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i].GetType() == type)
			{
				return Components[i];
			}
		}

		return null;
	}

	public List<Component> GetComponents(Type type)
	{
		List<Component> componentsToReturn = new();
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i].GetType() == type)
			{
				componentsToReturn.Add(Components[i]);
			}
		}

		return componentsToReturn;
	}

	void EditorUpdateComponents()
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
	}

	void UpdateComponents()
	{
		lock (_componentsLock)
		{
			for (int i = 0; i < Components.Count; i++)
			{
				if (Components[i].Enabled && Components[i].Awoken)
				{
					Components[i].Update();
				}
			}
		}
	}

	void UpdateRenderers()
	{
		lock (_componentsLock)
		{
			for (int i = 0; i < Components.Count; i++)
			{
				if (Components[i].Enabled && Components[i].Awoken && Components[i] is Renderer)
				{
					Components[i].Update();
				}
			}
		}
	}

	void FixedUpdateComponents()
	{
		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i].Enabled && Components[i].Awoken)
			{
				Components[i].FixedUpdate();
			}
		}
	}
	/* public void RegisterComponentPair(Component comp, Type type)
	 {
		   comp = GetComponent(type);
		   if (comp == null)
		   {
				 ComponentsWaitingToBePaired.Add(comp);
		   }
	 }*/

	public void Render()
	{
		if (ActiveInHierarchy == false)
		{
			return;
		}

		for (int i = 0; i < Components.Count; i++)
		{
			if (Components[i] is Renderer && Components[i].Enabled && Components[i].Awoken && ActiveInHierarchy)
			{
				(Components[i] as Renderer).Render();
			}
		}
	}

	public Vector3 TransformToWorld(Vector3 localPoint)
	{
		return localPoint + Transform.Position;
	}

	public Vector3 TransformToLocal(Vector3 worldPoint)
	{
		return worldPoint - Transform.Position;
	}
}