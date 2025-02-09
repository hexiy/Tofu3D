using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelHierarchy : EditorPanel
{
    public static EditorPanelHierarchy I { get; private set; }
    private bool _canDelete = true;

    private GameObject _clipboardGameObject;

    private readonly float _spaceHeight = 4;
    private List<GameObject> _gameObjectsIndexesSelectedBefore = new List<GameObject>();
    private List<GameObject> _selectedGameObjects = new List<GameObject>();
    private List<GameObject> _selectedGameObjectsParents = new List<GameObject>();
    private bool _showUpdatePrefabPopup;
    public override Vector2 Position => new Vector2(Tofu.Window.ClientSize.X - EditorPanelInspector.I.WindowWidth, 0);
    public override Vector2 Pivot => new Vector2(1, 0);

    public override string Name => "Hierarchy";

    public override void Init()
    {
        I = this;
        // Scene.AnySceneLoaded += ResetGameObjectSelection;
        GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
    }

    private void OnGameObjectsSelected(List<GameObject> gameObjects)
    {
        _selectedGameObjects.Clear();
        _selectedGameObjectsParents.Clear();
        for (int i = 0; i < gameObjects.Count; i++)
        {
            AddGameObjectToSelection(gameObjects[i]);

            Transform parent = gameObjects[i].Transform.Parent;
            while (parent != null)
            {
                _selectedGameObjectsParents.Add(parent.GameObject);
                parent = parent.Parent;
            }
        }

    }

    public override void Update()
    {
        if (_canDelete && KeyboardInput.IsKeyDown(Keys.Delete))
        {
            _canDelete = false;
            DestroySelectedGameObjects();
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.IsKeyUp(Keys.Backspace))
        {
            _canDelete = true;
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustReleased(Keys.C))
        {
            if (Tofu.GameObjectSelectionManager.GetSelectedGameObject() != null)
            {
                _clipboardGameObject = Tofu.GameObjectSelectionManager.GetSelectedGameObject();
                Tofu.SceneSerializer.SaveClipboardGameObject(_clipboardGameObject);
            }
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustReleased(Keys.V))
        {
            if (_clipboardGameObject != null)
            {
                GameObject loadedGo = Tofu.SceneSerializer.LoadClipboardGameObject();

                Tofu.Editor.AfterDraw += () => Tofu.GameObjectSelectionManager.SelectGameObject(loadedGo);
            }
        }


        // if (IsPanelHovered && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        // {
        //     _spaceHeight = 4;
        // }
        // else
        // {
        //     _spaceHeight = 4;
        // }
    }

    private void DestroySelectedGameObjects()
    {
        if (_selectedGameObjects.Count == 0)
        {
            return;
        }

        int firstSelectedGameObjectIndex =
            Tofu.SceneManager.CurrentScene.GetGameObjectByID(_selectedGameObjects[0].Id).IndexInHierarchy;
        foreach (GameObject selectedGameObject in Tofu.GameObjectSelectionManager.GetSelectedGameObjects())
        {
            _selectedGameObjects.Remove(selectedGameObject);
            selectedGameObject.Destroy();

            Tofu.GameObjectSelectionManager.SelectGameObjects(_selectedGameObjects);
        }

        int distance = int.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in Tofu.SceneManager.CurrentScene.GameObjects)
        {
            if (gameObject.VisibleInHierarchy == false)
            {
                continue;
            }

            int dist = (int)Mathf.Distance(gameObject.IndexInHierarchy, firstSelectedGameObjectIndex);
            if (dist < distance)
            {
                distance = dist;
                closestGameObject = gameObject;
            }
        }

        if (closestGameObject != null)
        {
            Tofu.Editor.AfterDraw += () => Tofu.GameObjectSelectionManager.SelectGameObject(closestGameObject);
        }
    }

    private void MoveSelectedGameObject(int addToIndex = 1)
    {
        int direction = addToIndex;
        if (Tofu.GameObjectSelectionManager.GetSelectedGameObjects().Count == 0)
        {
            return;
        }

        GameObject go = Tofu.GameObjectSelectionManager.GetSelectedGameObjects()[0];
        int oldIndex = go.IndexInHierarchy;

        if (oldIndex + direction >= Tofu.SceneManager.CurrentScene.GameObjects.Count ||
            oldIndex + direction < 0)
        {
            return;
        }

        while (Tofu.SceneManager.CurrentScene.GameObjects[oldIndex + direction].Transform.Parent != null)
        {
            direction += addToIndex;
        }

        Tofu.SceneManager.CurrentScene.GameObjects.RemoveAt(oldIndex);
        Tofu.SceneManager.CurrentScene.GameObjects.Insert(oldIndex + direction, go);


        //_selectedGameObjectsIndexes = oldIndex + direction;
        //GameObjectsSelected.Invoke(Tofu.SceneManager.CurrentScene.GameObjects[oldIndex + direction].Id);
    }

    private void AddGameObjectToSelection(GameObject go)
    {
        if (_selectedGameObjects.Contains(go))
        {
            return;
        }

        _selectedGameObjects.Add(go);
    }

    public override void Draw()
    {
        if (IsActive == false)
        {
            return;
        }

        ResetId();

        BeginWindowDefault();

        if (ImGui.Button("+"))
        {
            GameObject go = GameObject.Create(name: "GameObject");
            go.Awake();
            go.Transform.WorldPosition = Camera.MainCamera.CenterOfScreenToWorld();
        }

        ImGui.SameLine();

        if (ImGui.Button("-"))
        {
            DestroySelectedGameObjects();
        }

        ImGui.SameLine();
        ImGui.Dummy(new Vector2(15, 0));
        ImGui.SameLine();
        if (ImGui.Button("^"))
        {
            MoveSelectedGameObject(-1);
        }

        ImGui.SameLine();
        if (ImGui.Button("V"))
        {
            MoveSelectedGameObject();
        }

        ImGui.SameLine();
        if (ImGui.Button("Add children"))
        {
            foreach (GameObject gameObject in _selectedGameObjects)
            {
                GameObject go = GameObject.Create(name: "Child");
                go.Awake();
                go.Transform.SetParent(gameObject.Transform);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear scene"))
        {
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (GameObject go in Tofu.SceneManager.CurrentScene.GameObjects)
            {
                if (go != Camera.MainCamera.GameObject) // && go.VisibleInHierarchy)
                {
                    toDestroy.Add(go);
                }
            }

            foreach (GameObject go in toDestroy)
            {
                go.Destroy();
            }

            Tofu.SceneManager.CurrentScene.GameObjects.Clear();
        }

        for (int goIndex = 0; goIndex < Tofu.SceneManager.CurrentScene.GameObjects.Count; goIndex++)
        {
            // PushNextId();
            if (Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Transform.Parent != null)
            {
                /*// TODO fix this nonsense, i just quickly did this so closed gameobject with 130k children doesnt suck cpu cycles
                 // yeah this crashes the engine when adding a child lawl
                goIndex =
                    Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Transform.Parent.GameObject.IndexInHierarchy +
                    Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Transform.Parent.ChildrenIDs.Count - 1;*/
                continue;
            }

            // if (ImGui.IsItemVisible() == false)
            // {
            //     ImGui.Dummy(new System.Numerics.Vector2(100, 50));
            // }
            // else
            // {
            DrawGameObjectRow(goIndex);
            // }
        }

        PopAllIds();

        EndWindow();
    }


    private void DrawGameObjectRow(int goIndex, bool isChild = false)
    {
        int gameObjectID = Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Id;

        // if (isChild == false)
        // PushNextId(Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Id.ToString());
        PushNextId();


// TODO very slow
        GameObject currentGameObject = Tofu.SceneManager.CurrentScene.GameObjects.First(go => go.Id == gameObjectID);
        if (currentGameObject.Transform.Parent != null &&
            isChild == false) // only draw children from recursive DrawGameObjectRow calls
        {
            return;
        }

        if (currentGameObject.VisibleInHierarchy == false && Global.Debug == false)
        {
            return;
        }

        if (goIndex == 0)
        {
            DrawSpaceBetween(currentGameObject, false);
        }

        //bool hasAnyChildren = false;
        bool hasAnyChildren = currentGameObject.Transform.Children?.Count > 0;
        // bool isSelected = _selectedGameObjects.Contains(currentGameObject);
        bool isSelected = currentGameObject.Selected;

        ImGuiTreeNodeFlags flags =
            (isSelected ? ImGuiTreeNodeFlags.Selected : 0) |
            ImGuiTreeNodeFlags.OpenOnArrow;
        if (hasAnyChildren == false)
        {
            flags = (isSelected ? ImGuiTreeNodeFlags.Selected : 0) |
                    ImGuiTreeNodeFlags.Leaf;
        }


        Vector4 nameColor = currentGameObject.ActiveInHierarchy
            ? ImGui.GetStyle().Colors[(int)ImGuiCol.Text]
            : ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

        if (currentGameObject.IsStatic)
        {
            nameColor = currentGameObject.ActiveInHierarchy
                ? EditorColors.StaticLabel
                : EditorColors.StaticLabelInactive;
        }

        if (currentGameObject.IsPrefab)
        {
            nameColor = currentGameObject.ActiveInHierarchy
                ? Color.SkyBlue.ToVector4()
                : new Color(135, 206, 235, 130).ToVector4();
        }

        if (currentGameObject.VisibleInHierarchy == false)
        {
            nameColor = currentGameObject.ActiveInHierarchy
                ? Color.Purple.ToVector4()
                : new Color(70, 0, 70, 130).ToVector4();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, nameColor);

        string rowText = (Global.Debug ? $"[{currentGameObject.Id}] " : "") + currentGameObject.Name;
        flags |= ImGuiTreeNodeFlags.SpanFullWidth;
        flags |= ImGuiTreeNodeFlags.OpenOnDoubleClick;
        bool opened = ImGui.TreeNodeEx(rowText, flags);

        if (_selectedGameObjectsParents.Contains(currentGameObject))
        {
            opened = true;
        }
        // set opened if a children is selected

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            currentGameObject.SetActive(!currentGameObject.ActiveSelf);
        }

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && false) // todo remove false
        {
            Tofu.SceneViewController.MoveToGameObject(Tofu.GameObjectSelectionManager.GetSelectedGameObject());
        }


        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
        {
            if (_selectedGameObjects != _gameObjectsIndexesSelectedBefore)
            {
                _selectedGameObjects = _gameObjectsIndexesSelectedBefore;
                Tofu.GameObjectSelectionManager.SelectGameObjects(_selectedGameObjects);
            }

            // select gameobject selected before
            string gameObjectId = currentGameObject.Id.ToString();
            IntPtr stringPointer = Marshal.StringToHGlobalAnsi(gameObjectId);

            ImGui.SetDragDropPayload(DragDropPayloadTypes.GameObject, stringPointer,
                (uint)(sizeof(char) * gameObjectId.Length));

            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

            Marshal.FreeHGlobal(stringPointer);

            ImGui.Text(currentGameObject.Name);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.GameObject, ImGuiDragDropFlags.None);

            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
            {
                GameObject foundGo = Tofu.SceneManager.CurrentScene.GetGameObjectByID(int.Parse(payload));
                SetParent(foundGo.Transform, currentGameObject.Transform);
            }

            ImGui.EndDragDropTarget();
        }

        ImGui.PopStyleColor();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && KeyboardInput.IsKeyDown(Keys.LeftShift))
        {
            if (_selectedGameObjects.Count > 0)
            {
                // get 1st selected gameobject
                // find all gameobjects that have indexInHierarchy between that one and clicked on
                // select them all


                int alreadySelectedGameObjectIndex = _selectedGameObjects[0].IndexInHierarchy;
                int newlySelectedGameObjectIndex = currentGameObject.IndexInHierarchy;

                int selectionStartGameObjectIndex =
                    Math.Min(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);
                int selectionEndGameObjectIndex =
                    Math.Max(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);

                foreach (GameObject gameObject in Tofu.SceneManager.CurrentScene.GameObjects)
                {
                    for (int i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
                    {
                        if (gameObject.IndexInHierarchy >= selectionStartGameObjectIndex &&
                            gameObject.IndexInHierarchy <= selectionEndGameObjectIndex)
                        {
                            AddGameObjectToSelection(gameObject);
                        }
                    }
                }
            }
        }

        else if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && KeyboardInput.IsKeyDown(Keys.LeftSuper))
        {
            AddGameObjectToSelection(currentGameObject);
        }
        // else if (ImGui.IsItemHovered() && Tofu.MouseInput.ButtonReleased()) // doesnt work on very high fps since we update input 30k times per second but draw editor 120 times per second
        else if (ImGui.IsItemHovered() && Tofu.MouseInput.IsButtonDown())
        {
            _gameObjectsIndexesSelectedBefore = _selectedGameObjects;
            Tofu.Editor.AfterDraw += () => Tofu.GameObjectSelectionManager.SelectGameObject(currentGameObject);
        }

        DrawSpaceBetween(currentGameObject);
        if (opened)
        {
            List<Transform>? children = currentGameObject.Transform.Children;

            for (int childrenIndex = 0; childrenIndex < children.Count; childrenIndex++)
            {
                DrawGameObjectRow(children[childrenIndex].GameObject.IndexInHierarchy, true);
            }

            //ImGui.TreePop();
            ImGui.TreePop();
            if (children.Count > 0)
            {
                DrawSpaceBetween(currentGameObject, after: true);
            }
        }
    }

    private void DrawSpaceBetween(GameObject currentGameObject, bool after = true,
        bool currentGameObjectIsParent = true)
    {
        float height = _spaceHeight * Tofu.ImGuiController.FontSizeFactorRelativeToDefault;
        // if (Mathf.Distance(ImGui.GetCursorPosY(), ImGui.GetMousePos().Y) < 50 &&
        //     ImGui.GetCursorPosY() - ImGui.GetMousePos().Y < 50)
        // {
        //     height = _currentSpaceHeight;
        // }
        // height = 40;


        ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, height));
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.PushStyleColor(ImGuiCol.DragDropTarget, Color.MediumPurple.ToVector4());

            ImGui.AcceptDragDropPayload(DragDropPayloadTypes.GameObject, ImGuiDragDropFlags.None);

            string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left) && payload.Length > 0)
            {
                GameObject droppedGameObject = Tofu.SceneManager.CurrentScene.GetGameObjectByID(int.Parse(payload));
                if (droppedGameObject.IndexInHierarchy != currentGameObject.IndexInHierarchy)
                {
                    bool x = droppedGameObject.IndexInHierarchy < currentGameObject.IndexInHierarchy;
                    Tofu.SceneManager.CurrentScene.GameObjects.Remove(droppedGameObject);
                    Tofu.SceneManager.CurrentScene.GameObjects.Insert(
                        currentGameObject.IndexInHierarchy + (after ? 1 : 0) - (x ? 1 : 0),
                        droppedGameObject);

                    droppedGameObject.Transform.SetParent(currentGameObjectIsParent
                        ? currentGameObject.Transform.Parent
                        : currentGameObject.Transform.Parent.Parent);

                    Tofu.SceneManager.CurrentScene.UpdateGameobjectsIndexInHierarchy();
                }
            }

            ImGui.EndDragDropTarget();
            ImGui.PopStyleColor();
        }
    }

    private void SetParent(Transform child, Transform parent)
    {
        child.Transform.SetParent(parent);
    }
}