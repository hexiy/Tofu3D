using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelInspector : EditorPanel
{
    public override Vector2 Size => new Vector2(700, Tofu.Editor.SceneViewSize.Y);
    public override Vector2 Position => new Vector2(Tofu.Window.ClientSize.X - EditorPanelInspector.I.WindowWidth, 0);
    public override Vector2 Pivot => new Vector2(1, 0);

    public override string Name => "Inspector";

    string _addComponentPopupText = "";
    int _contentMaxWidth;
    private int draggingPointIndex=-1;

    public static EditorPanelInspector I { get; private set; }

    Action _actionQueue = () => { };
    List<InspectableData> _currentInspectableDatas = new List<InspectableData>(); // cached inspectable data
    bool _editing;

    bool _refreshQueued = false;

    private bool HasInspectableData
    {
        get { return _currentInspectableDatas.Count > 0; }
    }

    public override void Init()
    {
        I = this;

        _componentTypes = typeof(Component).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
        Scene.ComponentAwoken += OnComponentAddedToScene;
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
        Tofu.MouseInput.RegisterPassThroughEdgesCondition(() =>
            _editing && Tofu.MouseInput.IsButtonDown(MouseButtons.Left));
        Global.DebugStateChanged += (b) => QueueInspectorRefresh();
    }

    void OnComponentAddedToScene(Component comp)
    {
        foreach (InspectableData currentInspectableData in _currentInspectableDatas)
        {
            Component c = currentInspectableData.Inspectable as Component;
            if (c?.GameObject == comp.GameObject)
            {
                SelectInspectables(comp.GameObject.Components); // RefreshInspector();
                return;
            }
        }
    }

    public override void Update()
    {
        _actionQueue.Invoke();
        _actionQueue = () => { };
    }

    public void ClearInspectableDatas()
    {
        _currentInspectableDatas.Clear();
    }

    public void QueueInspectorRefresh()
    {
        _refreshQueued = true;
    }

    private void RefreshInspector()
    {
        _currentInspectableDatas.ForEach((data => data.InitInfos()));
    }

    private void OnGameObjectsSelected(List<int> ids)
    {
        // if (ids.Count != 1)
        // {
        // 	_selectedInspectable = null;
        // }
        // else
        // {
        // 	_selectedInspectable = Tofu.SceneManager.CurrentScene.GetGameObject(ids[0]);
        // 	UpdateCurrentComponentsCache();
        // 	_selectedMaterial = null;
        // }

        if (ids.Count == 0 || ids.FirstOrDefault(-1) == -1)
        {
            ClearInspectableDatas();

            return;
        }


        GameObject go = Tofu.SceneManager.CurrentScene.GetGameObject(ids[0]);
        SelectInspectables(go.Components);
    }

    /*private void UpdateCurrentComponentsCache()
    {
        // if (_selectedInspectable == null)
        // {
        // 	return;
        // }
        //
        // _currentInspectableDatas.Clear();
        // for (int componentIndex = 0; componentIndex < _selectedInspectable.Components.Count; componentIndex++)
        // {
        // 	InspectableData data = new InspectableData(_selectedInspectable.Components[componentIndex]);
        // 	_currentInspectableDatas.Add(data);
        // }
    }*/
    public void SelectInspectable(object inspectable)
    {
        SelectInspectables(new List<object>() { inspectable });
    }

    public void SelectInspectables(IList inspectables)
    {
        ClearInspectableDatas();

        foreach (object inspectable in inspectables)
        {
            InspectableData inspectableData = new InspectableData(inspectable);
            _currentInspectableDatas.Add(inspectableData);
        }
    }

    public void OnMaterialSelected(string materialPath)
    {
        object materialInspectable = Tofu.AssetManager.Load<Material>(Path.GetFileName(materialPath));
        SelectInspectable(materialInspectable);
    }

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        WindowWidth = 800;
        _contentMaxWidth = WindowWidth - (int)ImGui.GetStyle().WindowPadding.X * 1;
        ImGui.SetNextItemWidth(WindowWidth);
        ImGui.SetNextWindowPos(new Vector2(Tofu.Window.ClientSize.X, 0), ImGuiCond.FirstUseEver, new Vector2(1, 0));
        ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar);


        ResetId();

        if (HasInspectableData)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
            DrawInspectables(_currentInspectableDatas);
            ImGui.PopStyleVar(1);

            if (_refreshQueued)
            {
                _refreshQueued = false;
                RefreshInspector();
            }

            // properties with ShowIf and ShowIfNot attributes need to be reevaluated to show or not
            // if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            // {
            // 	UpdateCurrentComponentsCache();
            // }
        }

        // if (_selectedMaterial != null)
        // {
        // 	DrawMaterialInspector();
        // }

        ImGui.End();
    }

    public static List<Type> InspectorSupportedTypes = new()
    {
        typeof(GameObject),
        typeof(Material),
        typeof(Vector3),
        typeof(Vector2),
        typeof(Texture),
        typeof(CubemapTexture),
        typeof(Color),
        typeof(bool),
        typeof(float),
        typeof(int),
        typeof(string),
        typeof(Action),
        typeof(AudioClip),
        typeof(Mesh),
        typeof(Shader),
        typeof(Curve)
    };

    // if its a list, simply draw it like any other value but under the list row
    List<Type> _componentTypes;

    void DrawInspectables(List<InspectableData> inspectableDatas)
    {
        GameObject gameObject = (inspectableDatas[0].Inspectable as Component)?.GameObject;
        if (gameObject?.IsPrefab == true)
        {
            if (ImGui.Button("Update prefab"))
            {
                Tofu.SceneSerializer.SaveGameObject(gameObject, gameObject.PrefabPath);
            }

            ImGui.SameLine();
            if (ImGui.Button("Delete prefab"))
            {
                gameObject.IsPrefab = false;
            }
        }

        _editing = false;

        if (gameObject)
        {
            PushNextId();
            ImGui.SetScrollX(0);

            string gameObjectName = gameObject.Name;
            bool gameObjectActiveSelf = gameObject.ActiveSelf;
            ImGui.Checkbox("", ref gameObjectActiveSelf);
            gameObject.SetActive(gameObjectActiveSelf);
            ImGui.SameLine();

            bool wasStatic = gameObject.IsStaticSelf;
            if (gameObject.IsStaticSelf)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Color.Purple.ToVector4());
            }

            bool staticButtonClicked = ImGui.Button("STATIC");

            if (staticButtonClicked)
            {
                gameObject.IsStaticSelf = !gameObject.IsStaticSelf;
            }

            if (wasStatic)
            {
                ImGui.PopStyleColor();
            }

            ImGui.SameLine();


            PushNextId();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputText("", ref gameObjectName, 100))
            {
                gameObject.Name = gameObjectName;
            }
        }

        InspectableData materialToShowAtTheBottom = null;
        foreach (InspectableData componentInspectorData in inspectableDatas)
        {
            Component component = componentInspectorData.Inspectable as Component;

            if (component)
            {
                PushNextId();
                if (component.CanBeDisabled)
                {
                    bool componentEnabled = component.Enabled;
                    bool toggledComponent = ImGui.Checkbox("", ref componentEnabled);
                    if (toggledComponent)
                    {
                        component.Enabled = componentEnabled;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("-"))
                    {
                        component.GameObject.RemoveComponent(component);
                        continue;
                    }

                    ImGui.SameLine();
                }
            }

            PushNextId();

            string inspectableName = componentInspectorData.InspectableType.Name;
            if (componentInspectorData.InspectableType.IsSubclassOf(typeof(Component)))
            {
                inspectableName = (Global.Debug ? $"[{component.GameObjectId}] " : "") +
                                  componentInspectorData.InspectableType.Name;
            }

            if (componentInspectorData.InspectableType == typeof(Material))
            {
                ImGui.PushStyleColor(ImGuiCol.Header, Color.Honeydew.ToVector4());
            }

            bool headerClicked = ImGui.CollapsingHeader(inspectableName, ImGuiTreeNodeFlags.DefaultOpen);
            if (componentInspectorData.InspectableType == typeof(Material))
            {
                ImGui.PopStyleColor();
            }

            if (headerClicked)
            {
                if (componentInspectorData.InspectableType == typeof(Material))
                {
                    // DrawMaterialStuff(componentInspectorData);
                }


                foreach (FieldOrPropertyInfo info in componentInspectorData.Infos)
                {
                    bool drawn = DrawFieldOrProperty(info, componentInspectorData);
                    if (drawn == false)
                    {
                        continue;
                    }

                    //ImGui.PopID();
                }

                if (componentInspectorData.InspectableType == typeof(Material) && (_editing ||
                        ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)))
                {
                    // detect drag and drop texture too....
                    _actionQueue += () =>
                    {
                        Tofu.AssetManager.Save<Material>(componentInspectorData.Inspectable as Material);
                    };
                }
            }

            if (componentInspectorData.InspectableType.IsSubclassOf(typeof(Renderer)))
            {
                materialToShowAtTheBottom =
                    new InspectableData((componentInspectorData.Inspectable as Renderer).Material);
            }
        }


        if (gameObject)
        {
            bool justOpened = false;
            if (ImGui.Button("+"))
            {
                ImGui.OpenPopup("AddComponentPopup");
                justOpened = true;
            }

            if (ImGui.BeginPopupContextWindow("AddComponentPopup"))
            {
                if (justOpened)
                {
                    ImGui.SetKeyboardFocusHere(0);
                }

                bool enterPressed = ImGui.InputText("", ref _addComponentPopupText, 100,
                    ImGuiInputTextFlags.EnterReturnsTrue);


                if (_addComponentPopupText.Length > 0)
                {
                    for (int i = 0; i < _componentTypes.Count; i++)
                    {
                        if (_componentTypes[i].Name.ToLower().Contains(_addComponentPopupText.ToLower()))
                        {
                            if (ImGui.Button(_componentTypes[i].Name) || enterPressed)
                            {
                                gameObject.AddComponent(_componentTypes[i]);
                                ImGui.CloseCurrentPopup();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _componentTypes.Count; i++)
                    {
                        if (ImGui.Button(_componentTypes[i].Name))
                        {
                            gameObject.AddComponent(_componentTypes[i]);
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }

                ImGui.EndPopup();
            }
        }

        if (materialToShowAtTheBottom != null)
        {
            ImGui.Dummy(new Vector2(0, 50));
            DrawInspectables(new List<InspectableData>
            {
                materialToShowAtTheBottom
            });
        }
        // void DrawMaterialStuff(InspectableData componentInspectorData)
        // {
        // 	PushNextId();
        // 	Material selectedMaterial = componentInspectorData.Inspectable as Material;
        // 	// bool saveMaterialClicked = ImGui.Button("Save");
        // 	// if (saveMaterialClicked)
        // 	// {
        // 	// 	// Tofu.AssetManager.Save<Material>(selectedMaterial);
        // 	// }
        //
        // 	string materialName = Path.GetFileNameWithoutExtension(selectedMaterial.Path);
        // 	ImGui.Text(materialName);
        //
        // 	ImGui.Text("Shader");
        // 	float itemWidth = 400;
        // 	ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth);
        // 	ImGui.SetNextItemWidth(itemWidth);
        //
        // 	string shaderPath = selectedMaterial.Shader?.Path ?? "";
        // 	string shaderName = Path.GetFileName(shaderPath);
        // 	bool clicked = ImGui.Button(shaderName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        // 	if (clicked)
        // 	{
        // 		EditorPanelBrowser.I.GoToFile(shaderPath);
        // 	}
        //
        // 	if (ImGui.BeginDragDropTarget())
        // 	{
        // 		ImGui.AcceptDragDropPayload("SHADER", ImGuiDragDropFlags.None);
        //
        // 		shaderPath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
        // 		if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && shaderPath.Length > 0)
        // 		{
        // 			//shaderPath = Path.GetRelativePath("Assets", shaderPath);
        // 			Shader shader = new(shaderPath);
        //
        // 			selectedMaterial.Shader = shader;
        // 			_actionQueue += () => { Tofu.AssetManager.Save<Material>(componentInspectorData.Inspectable as Material); };
        // 			// Tofu.AssetManager.Save<Material>(selectedMaterial);
        // 		}
        //
        // 		ImGui.EndDragDropTarget();
        // 	}
        //
        // 	if (selectedMaterial.Shader == null)
        // 	{
        // 		return;
        // 	}
        // }

        // void DrawMaterialStuff(InspectableData componentInspectorData)
        // {
        // 	PushNextId();
        // 	Material selectedMaterial = componentInspectorData.Inspectable as Material;
        // 	// bool saveMaterialClicked = ImGui.Button("Save");
        // 	// if (saveMaterialClicked)
        // 	// {
        // 	// 	// AssetManager.Save<Material>(selectedMaterial);
        // 	// }
        //
        // 	string materialName = Path.GetFileNameWithoutExtension(selectedMaterial.AssetPath);
        // 	ImGui.Text(materialName);
        //
        // 	ImGui.Text("Shader");
        // 	float itemWidth = 400;
        // 	ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth);
        // 	ImGui.SetNextItemWidth(itemWidth);
        //
        // 	string shaderPath = selectedMaterial.Shader?.Path ?? "";
        // 	string shaderName = Path.GetFileName(shaderPath);
        // 	bool clicked = ImGui.Button(shaderName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
        // 	if (clicked)
        // 	{
        // 		EditorPanelBrowser.I.GoToFile(shaderPath);
        // 	}
        //
        // 	if (ImGui.BeginDragDropTarget())
        // 	{
        // 		ImGui.AcceptDragDropPayload("SHADER", ImGuiDragDropFlags.None);
        //
        // 		shaderPath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
        // 		if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && shaderPath.Length > 0)
        // 		{
        // 			//shaderPath = Path.GetRelativePath("Assets", shaderPath);
        // 			Shader shader = new(shaderPath);
        //
        // 			selectedMaterial.Shader = shader;
        // 			_actionQueue += () => { AssetManager.Save<Material>(componentInspectorData.Inspectable as Material); };
        // 			// AssetManager.Save<Material>(selectedMaterial);
        // 		}
        //
        // 		ImGui.EndDragDropTarget();
        // 	}
        //
        // 	if (selectedMaterial.Shader == null)
        // 	{
        // 		return;
        // 	}
        // }
    }


    bool DrawFieldOrProperty(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        if (info.CanShowInEditor == false)
        {
            return false;
        }

        PushNextId();

        // ReSharper disable once ReplaceWithSingleAssignment.False
        bool hovering = false;
        if (ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(),
                ImGui.GetCursorScreenPos() +
                new System.Numerics.Vector2(1500, ImGui.GetFrameHeightWithSpacing())))
        {
            hovering = true;
        }

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);

        if (info.IsListElement == false)
        {
            if (info.IsReadonly)
            {
                ImGui.BeginDisabled();
            }

            if (hovering)
            {
                ImGui.TextColored(new Vector4(0.7f, 0.4f, 0.6f, 1), info.Name);
            }
            else
            {
                ImGui.Text(info.Name);
            }
        }

        float itemWidth1 = 400;
        ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth1);
        ImGui.SetNextItemWidth(itemWidth1);

        if (info.IsGenericList)
        {
            object obj = info.GetValue(componentInspectorData.Inspectable);
            IList list = (IList)obj;


            if (ImGui.Button("+"))
            {
                object newElement = Activator.CreateInstance(info.GenericParameterType);
                list.Add(newElement);
                info.SetValue(componentInspectorData.Inspectable, list);
            }

            ImGui.SameLine();
            if (ImGui.CollapsingHeader($"List<{info.GenericParameterType.Name}>",
                    ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    PushNextId();
                    bool xClicked = ImGui.Button("x",
                        new System.Numerics.Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()));

                    if (xClicked)
                    {
                        list.RemoveAt(j);
                        info.SetValue(componentInspectorData.Inspectable, list);
                        continue;
                    }

                    ImGui.SameLine();

                    bool isNull = list[j] == null;
                    string name = isNull ? "<null>" : "name";


                    FieldOrPropertyInfo listElementFieldOrProperty = new FieldOrPropertyInfo(list, j);
                    listElementFieldOrProperty.IsListElement = true;
                    DrawFieldOrProperty(listElementFieldOrProperty, componentInspectorData);
                    /*if (ImGui.BeginDragDropTarget())
                    {
                        ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

                        string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                        ImGuiPayloadPtr x = ImGui.GetDragDropPayload();
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                        {
                            GameObject foundGo = Tofu.SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
                            list[j] = foundGo;
                            info.SetValue(componentInspectorData.Inspectable, list);
                        }

                        ImGui.EndDragDropTarget();
                    }*/
                }

                info.SetValue(componentInspectorData.Inspectable, list);
                // FieldInfo info;
                // info.get
                // info.SetValue(componentInspectorData.InspectableType, obj);
            }
        }

        if (info.FieldOrPropertyType == typeof(Vector3))
        {
            System.Numerics.Vector3 systemv3 = (Vector3)info.GetValue(componentInspectorData.Inspectable);
            if (ImGui.DragFloat3("", ref systemv3, 0.01f))
            {
                info.SetValue(componentInspectorData.Inspectable, (Vector3)systemv3);
            }
        }
        else if (info.FieldOrPropertyType == typeof(Vector2))
        {
            System.Numerics.Vector2 systemv2 = (Vector2)info.GetValue(componentInspectorData.Inspectable);
            if (ImGui.DragFloat2("", ref systemv2, 0.01f))
            {
                info.SetValue(componentInspectorData.Inspectable, (Vector2)systemv2);
            }
        }
        else
        {
            if (info.FieldOrPropertyType == typeof(Curve))
            {
                Curve curve = (Curve)info.GetValue(componentInspectorData.Inspectable);
                float v = curve._points[0];

                Vector2 pos = ImGui.GetCursorPos();
                Vector2 screenPos = ImGui.GetCursorScreenPos() / Tofu.Window.MonitorScale;
                Vector2 graphSize = new(300, 100);
                ImGui.PlotLines(string.Empty, ref curve._points[0], curve._points.Length, 0,
                    string.Empty,
                    0, 1,
                    graphSize);

                for (int i = 0; i < curve.DefiningPoints.Count; i++)
                {
                    ImGui.SameLine();

                    Vector2 newPos = new Vector2(pos.X + (curve.DefiningPoints[i].X * graphSize.X),
                        pos.Y + (1 - curve.DefiningPoints[i].Y) * graphSize.Y);
                    ImGui.SetCursorPos(newPos - new Vector2(15));

                    Texture texture = Tofu.AssetManager.Load<Texture>("Resources/dot.png");
                    ImGui.Image(texture.TextureId, new(30), new(0), new(1), Color.Purple.ToVector4());


                    if (draggingPointIndex!=-1 && ImGui.IsMouseDown(ImGuiMouseButton.Left) == false)
                    {
                        draggingPointIndex = -1;
                    }
                    else if (draggingPointIndex == -1)
                    {
                        bool x = ImGui.IsItemHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Left);
                        x = x || ImGui.IsItemClicked();
                        if (x)
                        {
                            draggingPointIndex = i;
                        }
                    }

                    if (draggingPointIndex==i)
                    {
                        curve.DefiningPoints[i] += Tofu.MouseInput.ScreenDelta / graphSize * 2;
                        curve.RecalculateCurve();
                    }

                    bool doubleClicked = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
                    if (doubleClicked)
                    {
                        Vector2 cursorPosRelativeToCurveSpace =
                            (Tofu.MouseInput.ScreenPosition - screenPos) / graphSize * 2;

                        Debug.Log($"Added point to curve :{cursorPosRelativeToCurveSpace}");
                        curve.AddDefiningPoint(cursorPosRelativeToCurveSpace);
                        break;
                    }
                }
            }
            else if (info.FieldOrPropertyType == typeof(Mesh))
            {
                Mesh mesh = (Mesh)info.GetValue(componentInspectorData.Inspectable);

                string assetName = Path.GetFileName(mesh?.Path) ?? "";

                bool clicked = ImGui.Button(assetName,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MODEL", ImGuiDragDropFlags.None);
                    string filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
                    {
                        // fileName = Path.GetRelativePath("Assets", fileName);

                        mesh = Tofu.AssetManager.Load<Mesh>(filePath);
                        // gameObject.GetComponent<Renderer>().Material.Vao = Mesh.Vao; // materials are shared
                        info.SetValue(componentInspectorData.Inspectable, mesh);
                    }

                    ImGui.EndDragDropTarget();
                }
            }
            else if (info.FieldOrPropertyType == typeof(AudioClip))
            {
                AudioClip audioClip = (AudioClip)info.GetValue(componentInspectorData.Inspectable);
                if (audioClip == null)
                {
                    audioClip = new AudioClip();
                    info.SetValue(componentInspectorData.Inspectable, audioClip);
                }

                string clipName = Path.GetFileName(audioClip?.Path);

                bool clicked = ImGui.Button(clipName,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));


                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", ImGuiDragDropFlags.None);
                    string fileName = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && fileName.Length > 0)
                    {
                        // fileName = Path.GetRelativePath("Assets", fileName);

                        audioClip.Path = fileName;
                        info.SetValue(componentInspectorData.Inspectable, audioClip);
                    }

                    ImGui.EndDragDropTarget();
                }
            }
            else if (info.FieldOrPropertyType == typeof(Action))
            {
                Action action = (Action)info.GetValue(componentInspectorData.Inspectable);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
                if (ImGui.Button($"> {info.Name} <",
                        new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())))
                {
                    action?.Invoke();
                }

                ImGui.PopStyleColor(1);
            }
            else if (info.FieldOrPropertyType == typeof(Texture))
            {
                Texture texture = (Texture)info.GetValue(componentInspectorData.Inspectable);
                string textureName = texture == null ? "" : Path.GetFileName(texture.Path);

                int posX = (int)ImGui.GetCursorPosX();

                if (texture == null)
                {
                    ImGui.Dummy(new Vector2(150, 150));
                }
                else
                {
                    ImGui.Image(texture.TextureId, new Vector2(150, 150));
                }

                ImGui.SetCursorPosX(posX);
                bool clicked = ImGui.Button(textureName,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
                bool rightMouseClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
                //ImiGui.Text(textureName);
                if (clicked)
                {
                    // Debug.Log("TODO");
                    _actionQueue += () => { EditorPanelBrowser.I.GoToFile(texture.Path); };
                }

                if (rightMouseClicked)
                {
                    info.SetValue(componentInspectorData.Inspectable, null);
                }

                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
                    string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                    {
                        payload = Path.GetRelativePath(Folders.EngineFolderPath, payload);

                        textureName = payload;

                        Texture loadedTexture = Tofu.AssetManager.Load<Texture>(textureName);

                        info.SetValue(componentInspectorData.Inspectable, loadedTexture);
                    }

                    ImGui.EndDragDropTarget();
                }
            }
            else if (info.FieldOrPropertyType == typeof(CubemapTexture))
            {
                CubemapTexture cubemapTexture = info.ListElement as CubemapTexture;
                string textureName = Path.GetFileName(cubemapTexture.Path);

                bool clicked = ImGui.Button(textureName,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
                if (clicked)
                {
                    EditorPanelBrowser.I.GoToFile(cubemapTexture.Path);
                }
            }
            else if (info.FieldOrPropertyType == typeof(Material))
            {
                string materialPath =
                    Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material.Path);

                materialPath = materialPath ?? "";
                bool clicked = ImGui.Button(materialPath,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
                if (clicked)
                {
                    _actionQueue += () =>
                        SelectInspectable((componentInspectorData.Inspectable as Renderer).Material);


                    // EditorPanelBrowser.I.GoToFile(materialPath);
                }

                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MATERIAL", ImGuiDragDropFlags.None);
                    string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                    {
                        payload = payload;
                        string materialName = Path.GetFileName(payload);

                        Material draggedMaterial = Tofu.AssetManager.Load<Material>(payload);
                        if (draggedMaterial.Shader == null)
                        {
                            Debug.Log("No Shader attached to material.");
                        }
                        else
                        {
                            (componentInspectorData.Inspectable as Renderer).Material = draggedMaterial;
                        }
                        // load new material
                    }

                    ImGui.EndDragDropTarget();
                }
            }
            else if (info.FieldOrPropertyType == typeof(GameObject))
            {
                GameObject goObject = info.GetValue(componentInspectorData.Inspectable) as GameObject;
                string fieldGoName = goObject?.Name ?? "";
                bool clicked = ImGui.Button(fieldGoName,
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
                if (clicked && goObject != null)
                {
                    // todo
                    // EditorPanelHierarchy.I.SelectGameObject(goObject.Id);
                    return true;
                }

                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("PREFAB_PATH", ImGuiDragDropFlags.None);
                    string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                        .Replace("\0", string.Empty);
                    if (dataType == "PREFAB_PATH")
                    {
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                        {
                            GameObject loadedGo = Tofu.SceneSerializer.LoadPrefab(payload, true);
                            info.SetValue(componentInspectorData.Inspectable, loadedGo);
                        }
                    }

                    ImGui.EndDragDropTarget();
                }

                if (ImGui.BeginDragDropTarget())
                {
                    ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);
                    string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII()
                        .Replace("\0", string.Empty);

                    if (dataType == "GAMEOBJECT")
                    {
                        //	string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
                        {
                            GameObject foundGo = Tofu.SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
                            info.SetValue(componentInspectorData.Inspectable, foundGo);
                        }
                    }

                    ImGui.EndDragDropTarget();
                }
            }
            else if (info.FieldOrPropertyType == typeof(Color))
            {
                System.Numerics.Vector4 fieldValue =
                    ((Color)info.GetValue(componentInspectorData.Inspectable)).ToVector4();

                bool hasColor3Attribute =
                    info.CustomAttributes.Count(data => data.AttributeType == typeof(Color3Attrib)) > 0;
                bool changed = false;
                if (hasColor3Attribute)
                {
                    System.Numerics.Vector3 vec3 = Extensions.ToVector3(fieldValue);

                    changed = ImGui.ColorEdit3("", ref vec3);
                    fieldValue = new System.Numerics.Vector4(vec3.X, vec3.Y, vec3.Z, fieldValue.W);
                }
                else
                {
                    changed = ImGui.ColorEdit4("", ref fieldValue);
                }

                if (changed)
                {
                    info.SetValue(componentInspectorData.Inspectable, fieldValue.ToColor());
                }
            }
            else if (info.FieldOrPropertyType == typeof(bool))
            {
                ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetContentRegionAvail().X / 2);

                bool fieldValue = (bool)info.GetValue(componentInspectorData.Inspectable);

                if (ImGui.Checkbox("", ref fieldValue))
                {
                    info.SetValue(componentInspectorData.Inspectable, fieldValue);
                    _refreshQueued = true;
                }
            }
            else if (info.FieldOrPropertyType == typeof(float))
            {
                float fieldValue = (float)info.GetValue(componentInspectorData.Inspectable);

                SliderF sliderAttrib = null;
                List<CustomAttributeData> a = info.FieldOrPropertyType.CustomAttributes.ToList();
                for (int i = 0; i < info.CustomAttributes.Count(); i++)
                {
                    if (info.CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(SliderF))
                    {
                        FieldInfo fieldType = componentInspectorData.Inspectable.GetType().GetField(info.Name);
                        if (fieldType != null)
                        {
                            sliderAttrib = fieldType.GetCustomAttribute<SliderF>();
                        }
                        else
                        {
                            PropertyInfo propertyType =
                                componentInspectorData.Inspectable.GetType().GetProperty(info.Name);
                            sliderAttrib = propertyType.GetCustomAttribute<SliderF>();
                        }
                    }
                }

                if (sliderAttrib != null)
                {
                    if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
                    {
                        info.SetValue(componentInspectorData.Inspectable, fieldValue);
                    }
                }
                else
                {
                    if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity,
                            "%.05f"))
                    {
                        info.SetValue(componentInspectorData.Inspectable, fieldValue);
                    }
                }
            }
            else if (info.FieldOrPropertyType == typeof(int))
            {
                int fieldValue = (int)info.GetValue(componentInspectorData.Inspectable);


                if (ImGui.DragInt("", ref fieldValue))
                {
                    info.SetValue(componentInspectorData.Inspectable, fieldValue);
                }
            }
            else if (info.FieldOrPropertyType == typeof(string))
            {
                string fieldValue = info.GetValue(componentInspectorData.Inspectable)?.ToString();

                if (ImGui.InputTextMultiline("", ref fieldValue, 100,
                        new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 200)))
                {
                    info.SetValue(componentInspectorData.Inspectable, fieldValue);
                }
            }
        }

        if (info.IsReadonly)
        {
            ImGui.EndDisabled();
        }

        if (ImGui.IsItemEdited())
        {
            _editing = true;
        }

        return true;
    }
}