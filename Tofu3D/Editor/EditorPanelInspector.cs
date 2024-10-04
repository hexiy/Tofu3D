using System.Collections;
using System.IO;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelInspector : EditorPanel
{
    public static List<Type> InspectorSupportedTypes; /* = new()
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
    };*/

    private Action _actionQueue = () => { };

    private string _addComponentPopupText = "";

    // if its a list, simply draw it like any other value but under the list row
    private List<Type> _componentTypes;
    private int _contentMaxWidth;

    private readonly List<InspectableData> _currentInspectableDatas = new(); // cached inspectable data
    private bool _editing;

    private Dictionary<Type, IInspectorFieldDrawable> _inspectorFieldDrawables;

    private bool _refreshQueued;
    private int _refreshQueuedInspectableIndex = -1; // -1 = all
    public override Vector2 Size => new(700, Tofu.Editor.SceneViewSize.Y);
    public override Vector2 Position => new(Tofu.Window.ClientSize.X - I.WindowWidth, 0);
    public override Vector2 Pivot => new(1, 0);

    public override string Name => "Inspector";

    public static EditorPanelInspector I { get; private set; }

    private bool HasInspectableData => _currentInspectableDatas.Count > 0;

    public void AddActionToActionQueue(Action action)
    {
        _actionQueue += action;
    }

    public void QueueRefresh()
    {
        _refreshQueuedInspectableIndex = -1;
        _refreshQueued = true;
    }

    public void QueueRefresh(InspectableData inspectableData)
    {
        _refreshQueuedInspectableIndex = _currentInspectableDatas.IndexOf(inspectableData);
        _refreshQueued = true;
    }

    public override void Init()
    {
        I = this;

        _inspectorFieldDrawables = new Dictionary<Type, IInspectorFieldDrawable>
        {
            { typeof(Vector2), new InspectorFieldDrawerVector2() },
            { typeof(Vector3), new InspectorFieldDrawerVector3() },
            { typeof(GameObject), new InspectorFieldDrawerGameObject() },
            { typeof(Material), new InspectorFieldDrawerMaterial() },
            { typeof(Texture), new InspectorFieldDrawerTexture() },
            { typeof(CubemapTexture), new InspectorFieldDrawerCubemapTexture() },
            { typeof(Color), new InspectorFieldDrawerColor() },
            { typeof(bool), new InspectorFieldDrawerBool() },
            { typeof(float), new InspectorFieldDrawerFloat() },
            { typeof(int), new InspectorFieldDrawerInt() },
            { typeof(string), new InspectorFieldDrawerString() },
            { typeof(Action), new InspectorFieldDrawerAction() },
            { typeof(AudioClip), new InspectorFieldDrawerAudioClip() },
            { typeof(Mesh), new InspectorFieldDrawerMesh() },
            { typeof(Curve), new InspectorFieldDrawerCurve() },
            { typeof(Enum), new InspectorFieldDrawerEnum() }
        };

        InspectorSupportedTypes = new List<Type>();
        foreach (var keyValuePair in _inspectorFieldDrawables)
        {
            InspectorSupportedTypes.Add(keyValuePair.Key);
        }

        _componentTypes = typeof(Component).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
        Scene.ComponentAwoken += OnComponentAddedToScene;
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
        Tofu.MouseInput.RegisterPassThroughEdgesCondition(() =>
            _editing && Tofu.MouseInput.IsButtonDown());
        Global.DebugStateChanged += b => QueueInspectorRefresh();
    }

    private void OnComponentAddedToScene(Component comp)
    {
        foreach (var currentInspectableData in _currentInspectableDatas)
        {
            var c = currentInspectableData.Inspectable as Component;
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
        _currentInspectableDatas.ForEach(data => data.InitInfos());
    }

    private void RefreshInspectable(object inspectable)
    {
        _currentInspectableDatas.FirstOrDefault(data => data.Inspectable == inspectable, null)?.InitInfos();
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


        var go = Tofu.SceneManager.CurrentScene.GetGameObject(ids[0]);
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
        SelectInspectables(new List<object> { inspectable });
    }

    public void SelectInspectables(IList inspectables)
    {
        ClearInspectableDatas();

        foreach (var inspectable in inspectables)
        {
            InspectableData inspectableData = new(inspectable);
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

                if (_refreshQueuedInspectableIndex == -1)
                {
                    RefreshInspector();
                }
                else
                {
                    RefreshInspectable(_currentInspectableDatas[_refreshQueuedInspectableIndex]);
                }
            }

            // properties with ShowIf and ShowIfNot attributes need to be reevaluated to show or not
            // if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            // {
            // 	UpdateCurrentComponentsCache();
            // }
        }


        ImGui.End();
    }


    private void DrawInspectables(List<InspectableData> inspectableDatas)
    {
        var gameObject = (inspectableDatas[0].Inspectable as Component)?.GameObject;
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

            var gameObjectName = gameObject.Name;
            var gameObjectActiveSelf = gameObject.ActiveSelf;
            ImGui.Checkbox("", ref gameObjectActiveSelf);
            gameObject.SetActive(gameObjectActiveSelf);
            ImGui.SameLine();

            var wasStatic = gameObject.IsStaticSelf;
            if (gameObject.IsStaticSelf)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Color.Purple.ToVector4());
            }

            var staticButtonClicked = ImGui.Button("STATIC");

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
        foreach (var componentInspectorData in inspectableDatas)
        {
            var component = componentInspectorData.Inspectable as Component;

            if (component)
            {
                PushNextId();
                if (component.CanBeDisabled)
                {
                    var componentEnabled = component.Enabled;
                    var toggledComponent = ImGui.Checkbox("", ref componentEnabled);
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

            var inspectableName = componentInspectorData.InspectableType.Name;
            if (componentInspectorData.InspectableType.IsSubclassOf(typeof(Component)))
            {
                inspectableName = (Global.Debug ? $"[{component.GameObjectId}] " : "") +
                                  componentInspectorData.InspectableType.Name;
            }

            if (componentInspectorData.InspectableType == typeof(Material))
            {
                ImGui.PushStyleColor(ImGuiCol.Header, Color.Honeydew.ToVector4());
            }

            var headerClicked = ImGui.CollapsingHeader(inspectableName, ImGuiTreeNodeFlags.DefaultOpen);
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


                foreach (var info in componentInspectorData.Infos)
                {
                    var drawn = DrawFieldOrProperty(info, componentInspectorData);
                    if (drawn == false)
                    {
                    }

                    //ImGui.PopID();
                }

                /*if (componentInspectorData.InspectableType == typeof(Material) && (_editing ||
                        ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)))
                    // detect drag and drop texture too....
                    _actionQueue += () =>
                    {
                        Tofu.AssetManager.Save<Material>(componentInspectorData.Inspectable as Material);
                    };*/
            }

            if (componentInspectorData.InspectableType.IsSubclassOf(typeof(Renderer)))
            {
                materialToShowAtTheBottom =
                    new InspectableData((componentInspectorData.Inspectable as Renderer).Material);
            }
        }


        if (gameObject)
        {
            var justOpened = false;
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

                var enterPressed = ImGui.InputText("", ref _addComponentPopupText, 100,
                    ImGuiInputTextFlags.EnterReturnsTrue);


                if (_addComponentPopupText.Length > 0)
                {
                    for (var i = 0; i < _componentTypes.Count; i++)
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
                    for (var i = 0; i < _componentTypes.Count; i++)
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
    }


    private bool DrawFieldOrProperty(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        if (info.HasSpaceAttribute)
        {
            ImGui.NewLine();
        }

        if (info.HeaderText != null)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            ImGui.TextColored(Color.Chocolate.ToVector4(), info.HeaderText);
            // ImGui.NewLine();
        }

        if (info.CanShowInEditor == false)
        {
            return false;
        }

        PushNextId();

        var hovering = false;
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
            var obj = info.GetValue(componentInspectorData.Inspectable);
            var list = (IList)obj;


            if (ImGui.Button("+"))
            {
                var newElement = Activator.CreateInstance(info.GenericParameterType);
                list.Add(newElement);
                info.SetValue(componentInspectorData.Inspectable, list);
            }

            ImGui.SameLine();
            if (ImGui.CollapsingHeader($"List<{info.GenericParameterType.Name}>",
                    ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (var j = 0; j < list.Count; j++)
                {
                    PushNextId();
                    var xClicked = ImGui.Button("x",
                        new System.Numerics.Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()));

                    if (xClicked)
                    {
                        list.RemoveAt(j);
                        info.SetValue(componentInspectorData.Inspectable, list);
                        continue;
                    }

                    ImGui.SameLine();

                    var isNull = list[j] == null;
                    var name = isNull ? "<null>" : "name";


                    FieldOrPropertyInfo listElementFieldOrProperty = new(list, j);
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

        if (info.FieldOrPropertyType.BaseType == typeof(Enum))
        {
            _inspectorFieldDrawables[typeof(Enum)].Draw(info, componentInspectorData);
        }
        else
        {
            if (_inspectorFieldDrawables.ContainsKey(info.FieldOrPropertyType))
            {
                _inspectorFieldDrawables[info.FieldOrPropertyType].Draw(info, componentInspectorData);
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