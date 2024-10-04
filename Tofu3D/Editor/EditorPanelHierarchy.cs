using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelHierarchy : EditorPanel
{
    private bool _canDelete = true;

    private GameObject _clipboardGameObject;

    private float _currentSpaceHeight;
    private List<int> _gameObjectsIndexesSelectedBefore = new();
    private List<int> _selectedGameObjectsIDs = new();
    private bool _showUpdatePrefabPopup;
    public override Vector2 Size => new(700, Tofu.Editor.SceneViewSize.Y);
    public override Vector2 Position => new(Tofu.Window.ClientSize.X - EditorPanelInspector.I.WindowWidth, 0);
    public override Vector2 Pivot => new(1, 0);

    public override string Name => "Hierarchy";

    public override void Init()
    {
        Scene.AnySceneLoaded += ResetGameObjectSelection;
    }

    public void Update()
    {
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && IsPanelHovered)
        {
            _currentSpaceHeight = 5;
        }
        else
        {
            _currentSpaceHeight = 0;
        }

        if (KeyboardInput.IsKeyDown(Keys.Delete) && _canDelete)
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
            if (GameObjectSelectionManager.GetSelectedGameObject() != null)
            {
                _clipboardGameObject = GameObjectSelectionManager.GetSelectedGameObject();
                Tofu.SceneSerializer.SaveClipboardGameObject(_clipboardGameObject);
            }
        }

        if (KeyboardInput.IsKeyDown(Keys.LeftControl) && KeyboardInput.WasKeyJustReleased(Keys.V))
        {
            if (_clipboardGameObject != null)
            {
                var loadedGo = Tofu.SceneSerializer.LoadClipboardGameObject();
                SelectGameObject(loadedGo.Id);
            }
        }
    }

    private void DestroySelectedGameObjects()
    {
        var firstSelectedGameObjectIndex =
            Tofu.SceneManager.CurrentScene.GetGameObject(_selectedGameObjectsIDs[0]).IndexInHierarchy;
        foreach (var selectedGameObject in GameObjectSelectionManager.GetSelectedGameObjects())
        {
            _selectedGameObjectsIDs.Remove(selectedGameObject.Id);
            selectedGameObject.Destroy();

            GameObjectSelectionManager.SelectGameObjects(_selectedGameObjectsIDs);
        }

        var distance = int.MaxValue;
        var closestGameObjectId = -1;
        foreach (var gameObject in Tofu.SceneManager.CurrentScene.GameObjects)
        {
            if (gameObject.Silent)
            {
                continue;
            }

            if (Mathf.Distance(gameObject.IndexInHierarchy, firstSelectedGameObjectIndex) < distance)
            {
                closestGameObjectId = gameObject.Id;
            }
        }

        if (closestGameObjectId != -1)
        {
            SelectGameObject(closestGameObjectId);
        }
    }

    private void MoveSelectedGameObject(int addToIndex = 1)
    {
        var direction = addToIndex;
        if (GameObjectSelectionManager.GetSelectedGameObjects().Count == 0)
        {
            return;
        }

        var go = GameObjectSelectionManager.GetSelectedGameObjects()[0];
        var oldIndex = go.IndexInHierarchy;

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

    public void ResetGameObjectSelection()
    {
        _selectedGameObjectsIDs.Clear();
        GameObjectSelectionManager.SelectGameObjects(null);
    }

    public void SelectGameObject(int id)
    {
        ResetGameObjectSelection();
        AddGameObjectToSelection(id);
    }

    public void AddGameObjectToSelection(int id)
    {
        if (_selectedGameObjectsIDs.Contains(id))
        {
            return;
        }

        _selectedGameObjectsIDs.Add(id);
        GameObjectSelectionManager.SelectGameObjects(_selectedGameObjectsIDs);
    }

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        ResetId();

        SetWindow();

        if (ImGui.Button("+"))
        {
            var go = GameObject.Create(name: "GameObject");
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
            foreach (var gameObjectId in _selectedGameObjectsIDs)
            {
                var go = GameObject.Create(name: "Child");
                go.Awake();
                go.Transform.SetParent(Tofu.SceneManager.CurrentScene.GetGameObject(gameObjectId).Transform);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear scene"))
        {
            List<GameObject> toDestroy = new();
            foreach (var go in Tofu.SceneManager.CurrentScene.GameObjects)
            {
                if (go != Camera.MainCamera.GameObject && go.Silent == false)
                {
                    toDestroy.Add(go);
                }
            }

            foreach (var go in toDestroy)
            {
                go.Destroy();
            }
        }

        for (var goIndex = 0; goIndex < Tofu.SceneManager.CurrentScene.GameObjects.Count; goIndex++)
        {
            var gameObjectID = Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Id;
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

            if (ImGui.IsItemVisible() == false)
            {
                ImGui.Dummy(new System.Numerics.Vector2(100, 50));
            }
            else
            {
                DrawGameObjectRow(gameObjectID);
            }
        }

        EndWindow();
    }


    private void DrawGameObjectRow(int gameObjectID, bool isChild = false)
    {
        // if (isChild == false)
        // PushNextId(Tofu.SceneManager.CurrentScene.GameObjects[goIndex].Id.ToString());
        PushNextId();


// TODO very slow
        var currentGameObject = Tofu.SceneManager.CurrentScene.GameObjects.First(go => go.Id == gameObjectID);
        if (currentGameObject.Transform.Parent != null &&
            isChild == false) // only draw children from recursive DrawGameObjectRow calls
        {
            return;
        }

        if (currentGameObject.Silent && Global.Debug == false)
        {
            return;
        }

        if (gameObjectID == 0)
        {
            DrawSpaceBetween(currentGameObject, false);
        }

        //bool hasAnyChildren = false;
        var hasAnyChildren = currentGameObject.Transform.Children?.Count > 0;
        var flags =
            (_selectedGameObjectsIDs.Contains(currentGameObject.Id) ? ImGuiTreeNodeFlags.Selected : 0) |
            ImGuiTreeNodeFlags.OpenOnArrow;
        if (hasAnyChildren == false)
        {
            flags = (_selectedGameObjectsIDs.Contains(currentGameObject.Id) ? ImGuiTreeNodeFlags.Selected : 0) |
                    ImGuiTreeNodeFlags.Leaf;
        }


        Vector4 nameColor = currentGameObject.ActiveInHierarchy
            ? ImGui.GetStyle().Colors[(int)ImGuiCol.Text]
            : ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

        if (currentGameObject.IsPrefab)
        {
            nameColor = currentGameObject.ActiveInHierarchy
                ? Color.SkyBlue.ToVector4()
                : new Color(135, 206, 235, 130).ToVector4();
        }

        if (currentGameObject.Silent)
        {
            nameColor = currentGameObject.ActiveInHierarchy
                ? Color.Purple.ToVector4()
                : new Color(70, 0, 70, 130).ToVector4();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, nameColor);

        var rowText = (Global.Debug ? $"[{currentGameObject.Id}] " : "") + currentGameObject.Name;
        flags |= ImGuiTreeNodeFlags.SpanFullWidth;
        flags |= ImGuiTreeNodeFlags.OpenOnDoubleClick;
        var opened = ImGui.TreeNodeEx(rowText, flags);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            currentGameObject.SetActive(!currentGameObject.ActiveSelf);
        }

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && false) // todo remove false
        {
            Tofu.SceneViewController.MoveToGameObject(GameObjectSelectionManager.GetSelectedGameObject());
        }


        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
        {
            if (_selectedGameObjectsIDs != _gameObjectsIndexesSelectedBefore)
            {
                _selectedGameObjectsIDs = _gameObjectsIndexesSelectedBefore;
                GameObjectSelectionManager.SelectGameObjects(_selectedGameObjectsIDs);
            }

            // select gameobject selected before
            var gameObjectId = currentGameObject.Id.ToString();
            var stringPointer = Marshal.StringToHGlobalAnsi(gameObjectId);

            ImGui.SetDragDropPayload("GAMEOBJECT", stringPointer, (uint)(sizeof(char) * gameObjectId.Length));

            var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

            Marshal.FreeHGlobal(stringPointer);

            ImGui.Text(currentGameObject.Name);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

            var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
            {
                var foundGo = Tofu.SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
                SetParent(foundGo.Transform, currentGameObject.Transform);
            }

            ImGui.EndDragDropTarget();
        }

        ImGui.PopStyleColor();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && KeyboardInput.IsKeyDown(Keys.LeftShift))
        {
            if (_selectedGameObjectsIDs.Count > 0)
            {
                // get 1st selected gameobject
                // find all gameobjects that have indexInHierarchy between that one and clicked on
                // select them all


                var alreadySelectedGameObjectIndex = Tofu.SceneManager.CurrentScene
                    .GetGameObject(_selectedGameObjectsIDs[0]).IndexInHierarchy;
                var newlySelectedGameObjectIndex = currentGameObject.IndexInHierarchy;

                var selectionStartGameObjectIndex =
                    Math.Min(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);
                var selectionEndGameObjectIndex =
                    Math.Max(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);

                foreach (var gameObject in Tofu.SceneManager.CurrentScene.GameObjects)
                {
                    for (var i = 0; i < Tofu.SceneManager.CurrentScene.GameObjects.Count; i++)
                    {
                        if (gameObject.IndexInHierarchy >= selectionStartGameObjectIndex &&
                            gameObject.IndexInHierarchy <= selectionEndGameObjectIndex)
                        {
                            AddGameObjectToSelection(gameObject.Id);
                        }
                    }
                }
            }
        }

        else if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && KeyboardInput.IsKeyDown(Keys.LeftSuper))
        {
            AddGameObjectToSelection(currentGameObject.Id);
        }
        else if (ImGui.IsItemHovered() && Tofu.MouseInput.ButtonReleased())
        {
            _gameObjectsIndexesSelectedBefore = _selectedGameObjectsIDs;
            SelectGameObject(currentGameObject.Id);
        }

        if (opened)
        {
            var children = currentGameObject.Transform.Children;

            for (var childrenIndex = 0; childrenIndex < children.Count; childrenIndex++)
            {
                DrawGameObjectRow(children[childrenIndex].GameObject.Id, true);
            }

            //ImGui.TreePop();
            ImGui.TreePop();
        }

        DrawSpaceBetween(currentGameObject);
    }

    private void DrawSpaceBetween(GameObject currentGameObject, bool after = true)
    {
        float height = 0;
        if (Mathf.Distance(ImGui.GetCursorPosY(), ImGui.GetMousePos().Y) < 50 &&
            ImGui.GetCursorPosY() - ImGui.GetMousePos().Y < 50)
        {
            height = _currentSpaceHeight;
        }

        ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, height));
        if (ImGui.BeginDragDropTarget())
        {
            ImGui.PushStyleColor(ImGuiCol.DragDropTarget, Color.MediumPurple.ToVector4());

            ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

            var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
            {
                var droppedGameObject = Tofu.SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
                Tofu.SceneManager.CurrentScene.GameObjects.RemoveAt(droppedGameObject.IndexInHierarchy);
                Tofu.SceneManager.CurrentScene.GameObjects.Insert(currentGameObject.IndexInHierarchy + (after ? 1 : 0),
                    droppedGameObject);

                droppedGameObject.Transform.SetParent(currentGameObject.Transform.Parent);
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