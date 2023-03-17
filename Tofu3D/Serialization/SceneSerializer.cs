using System.IO;
using System.Linq;

namespace Tofu3D;

public class SceneSerializer
{
	List<Type> _serializableTypes = new();

	public string LastScene
	{
		get { return PersistentData.GetString("lastOpenedScene", "Assets/Scenes/scene1.scene"); }
		set { PersistentData.Set("lastOpenedScene", value); }
	}
	XmlSerializer _xmlSerializer;

	void UpdateSerializableTypes()
	{
		if (_serializableTypes.Count == 0)
		{
			_serializableTypes = new List<Type>();

			_serializableTypes.AddRange(typeof(GameObject).Assembly.GetTypes()
			                                              .Where(type => type.IsSubclassOf(typeof(Component))));

			// delegates
			//SerializableTypes.AddRange(typeof(GameObject).Assembly.GetTypes()
			//                                             .Where(type => { return type.GetCustomAttribute<SerializableType>() != null; }));

			_serializableTypes.AddRange(typeof(Component).Assembly.GetTypes()
			                                             .Where(type => type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(GameObject))));
		}

		if (_xmlSerializer == null)
		{
			_xmlSerializer = new(typeof(SceneFile), _serializableTypes.ToArray());
		}
	}

	public void SaveGameObject(GameObject go, string prefabPath)
	{
		go.IsPrefab = true;
		go.PrefabPath = prefabPath;
		SceneFile prefabSceneFile = SceneFile.CreateForOneGameObject(go);

		SaveGameObjects(prefabSceneFile, prefabPath);
	}

	public void SaveClipboardGameObject(GameObject go)
	{
		if (Directory.Exists("Temp") == false)
		{
			Directory.CreateDirectory("Temp");
		}

		SceneFile prefabSceneFile = SceneFile.CreateForOneGameObject(go);

		SaveGameObjects(prefabSceneFile, Path.Combine("Temp", "clipboardGameObject"));
	}

	public GameObject LoadClipboardGameObject()
	{
		return LoadPrefab(Path.Combine("Temp", "clipboardGameObject"));
	}

	public GameObject LoadPrefab(string prefabPath, bool inBackground = false)
	{
		using (StreamReader sr = new(prefabPath))
		{
			UpdateSerializableTypes();

			XmlSerializer xmlSerializer = new(typeof(SceneFile), _serializableTypes.ToArray());

			SceneFile sceneFile = (SceneFile) xmlSerializer.Deserialize(sr);

			ConnectGameObjectsWithComponents(sceneFile);

			ConnectParentsAndChildren(sceneFile, true);

			GameObject mainGo = new();
			for (int i = 0; i < sceneFile.GameObjects.Count; i++)
			{
				for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
				{
					sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
				}

				GameObject go = sceneFile.GameObjects[i];

				if (i == 0)
				{
					mainGo = go;
				}

				if (inBackground == false)
				{
					Scene.I.AddGameObjectToScene(go);
					go.Awake();
				}
			}

			return mainGo;
		}
	}

	public void SaveGameObjects(SceneFile sceneFile, string scenePath)
	{
		using (StreamWriter sw = new(scenePath))
		{
			for (int i = 0; i < sceneFile.GameObjects.Count; i++)
			{
				sceneFile.GameObjects[i].Awoken = false;
				for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
				{
					sceneFile.GameObjects[i].Components[j].Awoken = false;
				}
			}

			UpdateSerializableTypes();

			XmlSerializer xmlSerializer = new(typeof(SceneFile), _serializableTypes.ToArray());
			xmlSerializer.Serialize(sw, sceneFile);

			for (int i = 0; i < sceneFile.GameObjects.Count; i++)
			{
				sceneFile.GameObjects[i].Awoken = true;
				for (int j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
				{
					sceneFile.GameObjects[i].Components[j].Awoken = true;
				}
			}
		}
	}

	public SceneFile LoadGameObjects(string scenePath)
	{
		UpdateSerializableTypes();
		/*string xml = "";
		using (StreamReader sr = new(scenePath))
		{
			xml = sr.ReadToEnd();
			/*List<int> componentNodesIndexes = xml.AllIndexesOf("<Component xsi:type=");
			for (int nodeIndex = 0; nodeIndex < componentNodesIndexes.Count; nodeIndex++)
			{
				int startIndex = componentNodesIndexes[nodeIndex];
				int length = xml.Substring(startIndex).IndexOf(">");
				string startTxt = xml.Substring(startIndex);
				string node = xml.Substring(startIndex, length - 1);
				string name = node.Substring(node.IndexOf("=") + 2);

				bool foundCorrespondingType = false;
				for (int i = 0; i < _serializableTypes.Count; i++)
				{
					string typeName = _serializableTypes[i].Name;
					if (typeName == name)
					{
						foundCorrespondingType = true;
					}
				}

				if (foundCorrespondingType == false)
				{
					string sbr = xml.Substring(startIndex - 4, 4);
					if (sbr != "<!--")
					{
						xml = xml.Replace(node, "<!--" + node);
					}

					int componentEndIndex = xml.Substring(startIndex).IndexOf("</Component>");
					xml = xml.Insert(startIndex + componentEndIndex + "</Component>".Length, "-->");
				}
			componentNodesIndexes = xml.AllIndexesOf("<Component xsi:type=");
			}#1#
		}*/

		/*using (StreamWriter sw = new(scenePath))
		{
			sw.Write(xml);
		}*/

		using StreamReader sr = new(scenePath);

		SceneFile a = (SceneFile) _xmlSerializer.Deserialize(sr);

		return a;
	}

	public void ConnectParentsAndChildren(SceneFile sf, bool newIDs = false)
	{
		GameObject[] gos = sf.GameObjects.ToArray();
		Component[] comps = sf.Components.ToArray();

		int[] goIndexes = new int[gos.Length];
		for (int i = 0; i < goIndexes.Length; i++)
		{
			goIndexes[i] = -1;
		}

		int[] ogIDs = new int[gos.Length];
		for (int i = 0; i < ogIDs.Length; i++)
		{
			ogIDs[i] = gos[i].Id;
		}

		for (int compIndex = 0; compIndex < comps.Length; compIndex++)
		{
			if (comps[compIndex].GetType() == typeof(Transform))
			{
				Transform tr = comps[compIndex] as Transform;
				for (int goIndex = 0; goIndex < gos.Length; goIndex++)
				{
					if (tr.ParentId == ogIDs[goIndex]) // found child/parent pair
					{
						if (newIDs)
						{
							// we change ID of a parent, but if theres multiple children, we change it again? that dont work
							if (goIndexes[goIndex] == -1)
							{
								gos[goIndex].Id = IDsManager.GameObjectNextId;
								IDsManager.GameObjectNextId++;

								goIndexes[goIndex] = gos[goIndex].Id;
							}
							else
							{
								gos[goIndex].Id = goIndexes[goIndex];
							}

							comps[compIndex].GameObject.Id = IDsManager.GameObjectNextId;
							IDsManager.GameObjectNextId++;
						}

						(comps[compIndex] as Transform).SetParent(gos[goIndex].Transform);
						(comps[compIndex] as Transform).ParentId = gos[goIndex].Id;
					}
				}
			}
		}

		for (int goIndex = 0; goIndex < gos.Length; goIndex++)
		{
			if (gos[goIndex].Components.Count == 0)
			{
				continue;
			}

			if (goIndex == gos.Length - 1 && gos[goIndex].Transform.Children.Count == 0)
			{
				if (newIDs)
				{
					gos[goIndex].Id = IDsManager.GameObjectNextId;
					IDsManager.GameObjectNextId++;
				}

				for (int i = 0; i < gos[goIndex].Transform.Children.Count; i++)
				{
					gos[goIndex].Transform.Children[i].ParentId = gos[goIndex].Id;
				}
			}
		}

		sf.GameObjects = gos.ToList();
		sf.Components = comps.ToList();
	}

	public void ConnectGameObjectsWithComponents(SceneFile sf)
	{
		GameObject[] gos = sf.GameObjects.ToArray();
		Component[] comps = sf.Components.ToArray();

		for (int i = 0; i < gos.Length; i++)
		{
			for (int j = 0; j < comps.Length; j++)
			{
				if (comps[j].GameObjectId == gos[i].Id)
				{
					gos[i].AddExistingComponent(comps[j]);
					// gos[i].LinkComponents(gos[i], comps[j]);
				}
			}
			// for (int j = 0; j < comps.Length; j++)
			// {
			// 	if (comps[j].GameObjectId == gos[i].Id && comps[j].GetType() == typeof(Transform)) // add transforms first
			// 	{
			// 		gos[i].AddExistingComponent(comps[j]);
			// 		// gos[i].LinkComponents(gos[i], comps[j]);
			// 	}
			// }
			//
			// for (int j = 0; j < comps.Length; j++)
			// {
			// 	if (comps[j].GameObjectId == gos[i].Id && comps[j].GetType() != typeof(Transform))
			// 	{
			// 		gos[i].AddExistingComponent(comps[j]);
			// 		// gos[i].LinkComponents(gos[i], comps[j]);
			// 	}
			// }
		}

		sf.GameObjects = gos.ToList();
		sf.Components = comps.ToList();
	}
}