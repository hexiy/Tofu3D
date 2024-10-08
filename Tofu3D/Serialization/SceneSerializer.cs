using System.IO;
using System.Linq;

namespace Tofu3D;

public class SceneSerializer
{
    private List<Type> _serializableTypes = new();

    private XmlSerializer _xmlSerializer;

    public SceneSerializer()
    {
        UpdateSerializableTypes();
    }

    public XmlSerializer MainXmlSerializer
    {
        get
        {
            if (_xmlSerializer == null)
            {
                UpdateSerializableTypes();
            }

            return _xmlSerializer;
        }
    }

    // update serializable types only on file watch script changed
    private void UpdateSerializableTypes()
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
            _xmlSerializer = new XmlSerializer(typeof(SceneFile), _serializableTypes.ToArray());
        }
    }

    public void SaveGameObject(GameObject go, string prefabPath)
    {
        go.IsPrefab = true;
        go.PrefabPath = prefabPath;
        var prefabSceneFile = SceneFile.CreateForOneGameObject(go);

        SaveGameObjects(prefabSceneFile, prefabPath);
    }

    public void SaveClipboardGameObject(GameObject go)
    {
        if (Directory.Exists("Temp") == false)
        {
            Directory.CreateDirectory("Temp");
        }

        var prefabSceneFile = SceneFile.CreateForOneGameObject(go);

        SaveGameObjects(prefabSceneFile, Path.Combine("Temp", "clipboardGameObject"));
    }

    public GameObject LoadClipboardGameObject() => LoadPrefab(Path.Combine("Temp", "clipboardGameObject"));

    public GameObject LoadPrefab(string prefabPath, bool inBackground = false)
    {
        // string timerName = $"LoadPrefab()";
        // Debug.StartTimer(timerName);


        StreamReader sr = new(prefabPath);
        // maybe cache streamreader in a dictionary and close it after few frames if not used?
        var sceneFile = (SceneFile)_xmlSerializer.Deserialize(sr);
        sr.Close();

        // float duration = Debug.EndTimer(timerName);
        // Debug.Log($"LoadPrefab() took {duration}");

        ConnectGameObjectsWithComponents(sceneFile);

        ConnectParentsAndChildren(sceneFile, true);

        GameObject mainGo = null;
        for (var i = 0; i < sceneFile.GameObjects.Count; i++)
        {
            for (var j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
            {
                sceneFile.GameObjects[i].Components[j].GameObjectId = sceneFile.GameObjects[i].Id;
            }

            var go = sceneFile.GameObjects[i];

            if (i == 0)
            {
                mainGo = go;
            }

            if (inBackground == false)
            {
                Tofu.SceneManager.CurrentScene.AddGameObjectToScene(go);
                go.Awake();
                go.Start();
            }
        }

        return mainGo;
    }

    public void SaveGameObjects(SceneFile sceneFile, string scenePath)
    {
        File.Create(scenePath).Close();
        using (StreamWriter sw = new(scenePath))
        {
            for (var i = 0; i < sceneFile.GameObjects.Count; i++)
            {
                sceneFile.GameObjects[i].Awoken = false;
                for (var j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
                {
                    sceneFile.GameObjects[i].Components[j].Awoken = false;
                }
            }

            // XmlSerializer xmlSerializer = new(typeof(SceneFile), _serializableTypes.ToArray());
            _xmlSerializer.Serialize(sw, sceneFile);

            for (var i = 0; i < sceneFile.GameObjects.Count; i++)
            {
                sceneFile.GameObjects[i].Awoken = true;
                for (var j = 0; j < sceneFile.GameObjects[i].Components.Count; j++)
                {
                    sceneFile.GameObjects[i].Components[j].Awoken = true;
                }
            }
        }
    }

    public SceneFile LoadSceneFile(string scenePath)
    {
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

        if (File.Exists(scenePath))
        {
            using StreamReader sr = new(scenePath);

            var sceneText = sr.ReadToEnd();
            var finalSceneText = sceneText;
            var xmlString = "<Component xsi:type=";

            var componentLineIndexes = sceneText.AllIndexesOf(xmlString).ToArray();

            var allComponentTypes = typeof(Component).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
            var allComponentStrings = new string[allComponentTypes.Count];
            for (var i = 0; i < allComponentTypes.Count; i++)
            {
                allComponentStrings[i] = allComponentTypes[i].Name;
            }

            // Find components that no longer exist and replace them with MissingComponent component, we save that, and if that component is brought back we recover the component

            foreach (var componentLineIndex in componentLineIndexes)
            {
                var str = sceneText.Substring(componentLineIndex);
                var length = sceneText.IndexOf('"', componentLineIndex + xmlString.Length + 1) - componentLineIndex -
                             xmlString.Length - 2;
                var componentName = sceneText.Substring(componentLineIndex + xmlString.Length + 1, length + 1);


                var startIndex = componentLineIndex;
                var lengthOfComponentString = sceneText.IndexOf("</Component>", componentLineIndex) -
                    componentLineIndex + "</Component>".Length;

                var wholeComponentString = finalSceneText.Substring(startIndex, lengthOfComponentString);

                if (allComponentStrings.Contains(componentName) == false && componentName != nameof(MissingComponent))
                {
                    Debug.LogError($"Found invalid component:{componentName}, removing it");

                    var missingComponent = new MissingComponent();
                    missingComponent.SetMissingComponentXML(componentName, wholeComponentString);

                    var ind1 = wholeComponentString.IndexOf("<GameObjectId>") + "<GameObjectId>".Length;
                    var ind2 = wholeComponentString.IndexOf("</GameObjectId>");
                    var gameObjectIDString =
                        wholeComponentString.Substring(ind1, ind2 - ind1);
                    var gameObjectID = int.Parse(gameObjectIDString);

                    missingComponent.GameObjectId = gameObjectID;
                    var missingComponentXML = missingComponent.GetXMLOfThisComponent();
                    finalSceneText = finalSceneText.Replace(wholeComponentString, missingComponentXML);
                }

                if (componentName == nameof(MissingComponent))
                {
                    var xmlSerializer = new XmlSerializer(typeof(Component), new[] { typeof(MissingComponent) });
                    var abc = "<Component xsi:type=\"MissingComponent\">";
                    var abc2 =
                        "<Component xsi:type=\"MissingComponent\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
                    var wholeComponentStringNew = wholeComponentString.Replace(abc, abc2);
                    var stringReader = new StringReader(wholeComponentStringNew);
                    var missingComponent = (MissingComponent)xmlSerializer.Deserialize(stringReader);
                    // get _oldComponentTypeName from MissingComponent as xml
                    var nameOfOldComponentTypeName = missingComponent._oldComponentTypeName;

                    var itExistsNow = false; // we want to bring back the missing component
                    for (var i = 0; i < allComponentTypes.Count; i++)
                    {
                        if (allComponentTypes[i].Name == nameOfOldComponentTypeName)
                        {
                            itExistsNow = true;
                            break;
                        }
                    }

                    if (itExistsNow)
                    {
                        Debug.Log($"brought back component {missingComponent._oldComponentTypeName}");
                        finalSceneText = finalSceneText.Replace(wholeComponentString,
                            missingComponent._oldComponentXMLString);
                    }
                }
            }

            File.WriteAllText(scenePath, finalSceneText);


            using StreamReader sr2 = new(scenePath);

            var sceneFile = (SceneFile)_xmlSerializer.Deserialize(sr2);
            return sceneFile;
        }

        return new SceneFile { GameObjects = new List<GameObject>(), Components = new List<Component>() };
    }

    public void ConnectParentsAndChildren(SceneFile sf, bool newIDs = false)
    {
        var gos = sf.GameObjects.ToArray();
        var comps = sf.Components.ToArray();

        var goIndexes = new int[gos.Length];
        for (var i = 0; i < goIndexes.Length; i++)
        {
            goIndexes[i] = -1;
        }

        var ogIDs = new int[gos.Length];
        for (var i = 0; i < ogIDs.Length; i++)
        {
            ogIDs[i] = gos[i].Id;
        }

        for (var compIndex = 0; compIndex < comps.Length; compIndex++)
        {
            if (comps[compIndex].GetType() == typeof(Transform))
            {
                var tr = comps[compIndex] as Transform;
                for (var goIndex = 0; goIndex < gos.Length; goIndex++)
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

        for (var goIndex = 0; goIndex < gos.Length; goIndex++)
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

                for (var i = 0; i < gos[goIndex].Transform.Children.Count; i++)
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
        var gos = sf.GameObjects.ToArray();
        var comps = sf.Components.ToArray();

        for (var i = 0; i < gos.Length; i++)
        for (var j = 0; j < comps.Length; j++)
        {
            if (comps[j].GameObjectId == gos[i].Id)
            {
                gos[i].AddExistingComponent(comps[j]);
            }
        }

        // gos[i].LinkComponents(gos[i], comps[j]);
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
        sf.GameObjects = gos.ToList();
        sf.Components = comps.ToList();
    }
}