using System.Collections;
using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class Inspector
{
    public Action<string> FieldChangedByUser = (fieldName) => { };
    public Action<string> FieldChangedByUserInspectableCallback = (fieldName) => { };

    private Action _actionQueue = () => { };
    private bool _anyActionQueued = false;
    private int _currentId;

    private Vector2 _size;

    public readonly List<InspectableData>
        CurrentInspectableDatas = new List<InspectableData>(); // cached inspectable data

    public bool _editing;

    public static List<Type> InspectorSupportedTypes;
    public static Dictionary<Type, IInspectorFieldDrawable> _inspectorFieldDrawables;

    public bool _refreshQueued;
    public int _refreshQueuedInspectableIndex = -1; // -1 = all
    // public float ContentMaxWidth;

    public bool HasInspectableData => CurrentInspectableDatas.Count > 0;

    // probably should be set per InspectableData
    private bool _drawInspectableHeader;

    public Inspector(bool drawInspectableHeader = true)
    {
        _drawInspectableHeader = drawInspectableHeader;
        if (_inspectorFieldDrawables == null)
        {
            PopulateFieldDrawablesCollection();
        }

        if (InspectorSupportedTypes == null)
        {
            PopulateInspectorSupportedTypesCollection();
        }


        Tofu.MouseInput.RegisterPassThroughEdgesCondition(() =>
            _editing && Tofu.MouseInput.IsButtonDown());
    }

    private void PopulateInspectorSupportedTypesCollection()
    {
        InspectorSupportedTypes = new List<Type>();
        foreach (KeyValuePair<Type, IInspectorFieldDrawable> keyValuePair in _inspectorFieldDrawables)
        {
            InspectorSupportedTypes.Add(keyValuePair.Key);
        }
    }

    private void PopulateFieldDrawablesCollection()
    {
        _inspectorFieldDrawables = new Dictionary<Type, IInspectorFieldDrawable>
        {
            { typeof(Vector2), new InspectorFieldDrawerVector2() },
            { typeof(Vector3), new InspectorFieldDrawerVector3() },
            { typeof(Vector4), new InspectorFieldDrawerVector4() },
            { typeof(GameObject), new InspectorFieldDrawerGameObject() },
            { typeof(Asset_Material), new InspectorFieldDrawerMaterial() },
            { typeof(Shader), new InspectorFieldDrawerShader() },
            { typeof(RuntimeTexture), new InspectorFieldDrawerTexture() },
            { typeof(RuntimeCubemapTexture), new InspectorFieldDrawerCubemapTexture() },
            { typeof(Color), new InspectorFieldDrawerColor() },
            { typeof(bool), new InspectorFieldDrawerBool() },
            { typeof(float), new InspectorFieldDrawerFloat() },
            { typeof(int), new InspectorFieldDrawerInt() },
            { typeof(uint), new InspectorFieldDrawerUInt() },
            { typeof(string), new InspectorFieldDrawerString() },
            { typeof(Action), new InspectorFieldDrawerAction() },
            { typeof(AudioClip), new InspectorFieldDrawerAudioClip() },
            { typeof(RuntimeMesh), new InspectorFieldDrawerMesh() },
            { typeof(Curve), new InspectorFieldDrawerCurve() },
            { typeof(Enum), new InspectorFieldDrawerEnum() },
            { typeof(CollectionWithSelection<string>), new InspectorFieldDrawerCollectionWithSelection<string>() },
        };
    }

    public void SelectInspectable(object inspectable, Action<string> anyValueChanged = null)
    {
        FieldChangedByUserInspectableCallback = anyValueChanged;
        SelectInspectables(new List<object> { inspectable });
    }


    public void SelectInspectables(IList inspectables)
    {
        ClearInspectableData();

        foreach (object? inspectable in inspectables)
        {
            if (inspectable == null)
            {
                continue;
            }
        }

        foreach (object? inspectable in inspectables)
        {
            if (inspectable == null)
            {
                continue;
            }
            
            InspectableData inspectableData = new InspectableData(inspectable, inspector: this);
            CurrentInspectableDatas.Add(inspectableData);
        }
    }

    /// <summary>
    /// Updates the InspectableData with brand new field infos
    /// </summary>
    public void Refresh()
    {
        foreach (var data in CurrentInspectableDatas)
        {
            data.InitInfos();
        }
    }

    private void RefreshInspectable(object inspectable)
    {
        CurrentInspectableDatas.FirstOrDefault(data => data.Inspectable == inspectable, null)?.InitInfos();
    }

    public void QueueRefresh()
    {
        _refreshQueuedInspectableIndex = -1;
        _refreshQueued = true;
    }

    public void QueueRefresh(InspectableData inspectableData)
    {
        _refreshQueuedInspectableIndex = CurrentInspectableDatas.IndexOf(inspectableData);
        _refreshQueued = true;
    }

    public void ClearInspectableData()
    {
        CurrentInspectableDatas.Clear();
    }

    public void AddActionToActionQueue(Action action)
    {
        _actionQueue += action;
        _anyActionQueued = true;
    }

    public void Update(Vector2 size)
    {
        _size = size;
        if (_anyActionQueued)
        {
            _actionQueue.Invoke();
            _actionQueue = () => { };
            _anyActionQueued = false;
        }

        if (HasInspectableData)
        {
            if (_refreshQueued)
            {
                _refreshQueued = false;

                if (_refreshQueuedInspectableIndex == -1)
                {
                    Refresh();
                }
                else
                {
                    RefreshInspectable(CurrentInspectableDatas[_refreshQueuedInspectableIndex]);
                }
            }
        }
    }

    public void Render(List<InspectableData> inspectableDatas)
    {
        ResetId();
        foreach (InspectableData componentInspectorData in inspectableDatas)
        {
            Component? component = componentInspectorData.Inspectable as Component;

            if (component)
            {
                PushNextId();
                if (component.CanBeDisabled)
                {
                    bool componentEnabled = component.EnabledSelf;
                    bool toggledComponent = ImGui.Checkbox("", ref componentEnabled);
                    if (toggledComponent)
                    {
                        component.EnabledSelf = componentEnabled;
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

            if (componentInspectorData.InspectableType == typeof(Asset_Material))
            {
                Vector4 headerColor = Color.Honeydew.ToVector4();

                Asset_Material material = componentInspectorData.Inspectable as Asset_Material;
                if (material is { IsRuntimeCopy: true })
                {
                    inspectableName += " | RUNTIME COPY";
                    headerColor = Color.Gold.ToVector4();
                }

                // TofuImGui.PushStyleColor(ImGuiCol.Header, headerColor);
            }

            bool headerClicked = true;
            if (_drawInspectableHeader)
            {
                headerClicked = ImGui.CollapsingHeader(inspectableName, ImGuiTreeNodeFlags.DefaultOpen);
            }

            if (componentInspectorData.InspectableType == typeof(Asset_Material))
            {
                // TofuImGui.PopStyleColor();
            }

            if (headerClicked)
            {
                if (componentInspectorData.InspectableType == typeof(Asset_Material))
                {
                    // DrawMaterialStuff(componentInspectorData);
                }

                foreach (FieldOrPropertyInfo info in componentInspectorData.Infos)
                {
                    // if (Global.Debug)
                    // {
                    // Debug.Log($"attempting to draw {info.Name} property of component {componentInspectorData.InspectableType}");
                    // }

                    bool drawn = DrawFieldOrProperty(info, componentInspectorData);
                    if (drawn == false)
                    {
                    }

                    //ImGui.PopID();
                }

                /*if (componentInspectorData.InspectableType == typeof(Asset_Material) && (_editing ||
                        Tofu.MouseInput.ButtonReleased(MouseButtons.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)))
                    // detect drag and drop texture too....
                    _actionQueue += () =>
                    {
                        Debug.Log("wip try to save texture");
                        Asset_Material assetMaterial = (componentInspectorData.Inspectable as Asset_Material);
                        string assetPathInLibrary = assetMaterial.PathToRawAsset.FromRawAssetFileNameToPathOfAssetInLibrary();
                        Tofu.AssetLoadManager.Save<Asset_Material>(assetPathInLibrary, assetMaterial);
                        // Tofu.AssetLoadManager.Save<Material>();.Save<Material>(componentInspectorData.Inspectable as Material);
                    };*/
            }
        }

        PopAllIds();
    }


    internal void ResetId()
    {
        _currentId = 0;
    }

    internal void PushNextId()
    {
        ImGui.PushID(_currentId++);
    }

    internal void PopAllIds()
    {
        for (int i = 0; i < _currentId; i++)
        {
            ImGui.PopID();
        }

        ResetId();
    }

    public bool DrawFieldOrProperty(FieldOrPropertyInfo info, InspectableData componentInspectorData)
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


        float itemWidth1 = _size.X / 1.6f;
        ImGui.SameLine(_size.X - itemWidth1);
        // ImGui.SetNextItemWidth(itemWidth1);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - TofuImGui.DefaultWindowPadding.X);

        if (info.IsGenericList)
        {
            object? obj = info.GetValue(componentInspectorData.Inspectable);
            IList? list = (IList)obj;


            if (ImGui.Button("+"))
            {
                object? newElement = Activator.CreateInstance(info.GenericParameterType);
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
                    // string name = isNull ? "<null>" : "name";


                    FieldOrPropertyInfo listElementFieldOrProperty =
                        new FieldOrPropertyInfo(list, j, componentInspectorData);
                    listElementFieldOrProperty.IsListElement = true;
                    DrawFieldOrProperty(listElementFieldOrProperty, componentInspectorData);
                    /*if (ImGui.BeginDragDropTarget())
                    {
                        ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

                        string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                        ImGuiPayloadPtr x = ImGui.GetDragDropPayload();
                        if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
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
            if (_inspectorFieldDrawables.TryGetValue(info.FieldOrPropertyType,
                    out IInspectorFieldDrawable? inspectorFieldDrawable))
            {
                inspectorFieldDrawable.Draw(info, componentInspectorData);
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