using System.Collections;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelInspector : EditorPanel
{
    private Inspector _inspector;
    private string _addComponentPopupText = "";

    // if its a list, simply draw it like any other value but under the list row


    private InspectableData _materialToShowAtTheBottom = null;
    public override Vector2 Position => new Vector2(Tofu.Window.ClientSize.X - I.WindowWidth, 0);
    public override Vector2 Pivot => new Vector2(1, 0);

    public override string Name => "Inspector";


    public static EditorPanelInspector I { get; private set; }
    public List<Type> _componentTypesForAddComponentPopup;
    private int _padding = 0;


    public override void Init()
    {
        I = this;
        _inspector = new Inspector();
        _inspector.FieldChangedByUser += OnAnyFieldChangedByUser;

        _componentTypesForAddComponentPopup = typeof(Component).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
        _componentTypesForAddComponentPopup.AddRange(ScriptsManager.ScriptsAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract));


        Scene.ComponentAwoken += OnComponentAddedToScene;
        Scene.ComponentRemoved += c => _inspector.QueueRefresh();
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;

        Global.DebugStateChanged += b => _inspector.QueueRefresh();
    }

    private void OnComponentAddedToScene(Component comp)
    {
        foreach (InspectableData currentInspectableData in _inspector.CurrentInspectableDatas)
        {
            Component? c = currentInspectableData.Inspectable as Component;
            if (c?.GameObject == comp.GameObject)
            {
                SelectInspectables(comp.GameObject.Components); // RefreshInspector();
                return;
            }
        }
    }

    public override void Update()
    {
        _inspector.Update();
        _inspector.Size = Size;
        _inspector.ContentMaxWidth = Size.Xi - (int)ImGui.GetStyle().WindowPadding.X;
    }

    public void AddActionToActionQueue(Action action)
    {
        _inspector.AddActionToActionQueue(action);
    }


    private void OnGameObjectsSelected(List<GameObject> gameObjects)
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

        if (gameObjects.Count == 0 || gameObjects.FirstOrDefault() == null)
        {
            _inspector.ClearInspectableData();

            return;
        }

        SelectInspectables(gameObjects[0].Components);
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
    public void SelectInspectable(object inspectable, Action? anyValueChanged = null)
    {
        _inspector.FieldChangedByUserInspectableCallback = anyValueChanged;
        SelectInspectables(new List<object> { inspectable });
    }


    public void SelectInspectables(IList inspectables)
    {
        _inspector.ClearInspectableData();
        _materialToShowAtTheBottom = null;

        foreach (object? inspectable in inspectables)
        {
            InspectableData inspectableData = new InspectableData(inspectable, inspector: _inspector);
            _inspector.CurrentInspectableDatas.Add(inspectableData);
        }
    }

    public void OnMaterialSelected(string materialPath)
    {
        object materialInspectable = Tofu.AssetLoadManager.Load<Asset_Material>(materialPath);

        EditorPanelInspector.I.SelectInspectable(materialInspectable,
            anyValueChanged: () =>
            {
                Serializer.SaveFileJSON<Asset_Material>(
                    materialPath, materialInspectable);
            });
    }


    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        //WindowWidth = 800;
        BeginWindowDefault();
        ResetId();
        ImGui.SetScrollX(0);
        _padding = (int)ImGui.GetStyle().WindowPadding.X;
        // Ensure we disable horizontal scrolling and clip overflow
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0);

        if (_inspector.HasInspectableData)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);

            if (ImGui.BeginChild("InspectorChild",
                    ImGui.GetContentRegionAvail() - new System.Numerics.Vector2(_padding, 0), false,
                    ImGuiWindowFlags.NoScrollbar))
            {
                DrawInspectables(_inspector.CurrentInspectableDatas);
            }

            ImGui.PopStyleVar(1);


            // properties with ShowIf and ShowIfNot attributes need to be reevaluated to show or not
            // if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left))
            // {
            // 	UpdateCurrentComponentsCache();
            // }
        }

        ImGui.PopStyleVar(2); // Restore all styles

        ImGui.End();
    }


    /// <summary>
    /// EditorPanelInspector specific
    /// </summary>
    /// <param name="inspectableDatas"></param>
    private void DrawInspectables(List<InspectableData> inspectableDatas)
    {
        GameObject? gameObject = (inspectableDatas[0].Inspectable as Component)?.GameObject;
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

        _inspector._editing = false;
        if (gameObject)
        {
            PushNextId();

            string? gameObjectName = gameObject.Name;
            bool gameObjectActiveSelf = gameObject.ActiveSelf;
            ImGui.Checkbox("", ref gameObjectActiveSelf);
            gameObject.SetActive(gameObjectActiveSelf);
            ImGui.SameLine();

            bool wasStatic = gameObject.IsStaticSelf;
            if (gameObject.IsStaticSelf)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, EditorColors.StaticLabel.ToVector4());
            }

            bool staticButtonClicked = ImGui.Button("STATIC");

            if (staticButtonClicked)
            {
                gameObject.IsStaticSelf = !gameObject.IsStaticSelf;

                foreach (Transform child in gameObject.Transform.Children)
                {
                    child.GameObject.IsStaticSelf = gameObject.IsStaticSelf;
                }
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

        // _materialToShowAtTheBottom = null;
        _inspector.Render(inspectableDatas);


        foreach (InspectableData inspectableData in _inspector.CurrentInspectableDatas)
        {
            if (inspectableData.Inspectable is IHasMaterial hasMaterial)
            {
                Asset_Material material = hasMaterial.GetMaterial;
                if (material != null)
                {
                    _materialToShowAtTheBottom =
                        new InspectableData(material, _inspector);
                }
            }
        }

        if (gameObject)
        {
            bool justOpened = false;
            if (ImGui.Button("[+] Add Component"))
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
                    for (int i = 0; i < _componentTypesForAddComponentPopup.Count; i++)
                    {
                        if (_componentTypesForAddComponentPopup[i].Name
                            .Contains(_addComponentPopupText, StringComparison.OrdinalIgnoreCase))
                        {
                            if (ImGui.Button(_componentTypesForAddComponentPopup[i].Name) || enterPressed)
                            {
                                gameObject.AddComponent(_componentTypesForAddComponentPopup[i]);
                                _inspector.QueueRefresh();
                                // this.RefreshInspector();
                                // this.QueueInspectorRefresh();
                                ImGui.CloseCurrentPopup();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _componentTypesForAddComponentPopup.Count; i++)
                    {
                        if (ImGui.Button(_componentTypesForAddComponentPopup[i].Name))
                        {
                            gameObject.AddComponent(_componentTypesForAddComponentPopup[i]);
                            // this.RefreshInspector();
                            // this.QueueInspectorRefresh();

                            ImGui.CloseCurrentPopup();
                        }
                    }
                }

                ImGui.EndPopup();
            }
        }

        if (_materialToShowAtTheBottom != null && inspectableDatas.Contains(_materialToShowAtTheBottom) == false)
        {
            ImGui.Dummy(new Vector2(0, 50));
            DrawInspectables(new List<InspectableData>
            {
                _materialToShowAtTheBottom
            });
        }
    }


    public void OnAnyFieldChangedByUser()
    {
        if (_materialToShowAtTheBottom != null)
        {
            // crashed when dragged mesh
            Asset_Material material = (_materialToShowAtTheBottom.Inspectable as Asset_Material);
            if (material == null || /*material?.IsRuntimeCopy == true || */material?.AnyPath == null)
            {
                return;
            }

            if (material.PathInAssetsFolder != null)
            {
                Serializer.SaveFileJSON<Asset_Material>(material.PathInAssetsFolder, material);
                Tofu.AssetImportManager.ImportAsset(material.PathInAssetsFolder, reimportIfExists: true);
            }
            else
            {
                Serializer.SaveFileJSON<Asset_Material>(material.PathInLibraryFolder, material);
                Tofu.AssetImportManager.ImportAsset(material.PathInLibraryFolder, reimportIfExists: true);
            }
        }
    }
}