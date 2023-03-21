﻿using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelHierarchy : EditorPanel
{
	public override Vector2 Size => new Vector2(700, Editor.SceneViewSize.Y);
	public override Vector2 Position => new Vector2(Tofu.I.Window.ClientSize.X - EditorPanelInspector.I.WindowWidth, 0);
	public override Vector2 Pivot => new Vector2(1, 0);

	public override string Name => "Hierarchy";

	bool _canDelete = true;

	GameObject _clipboardGameObject;
	List<int> _gameObjectsIndexesSelectedBefore = new List<int>();
	List<int> _selectedGameObjectsIDs = new List<int>();
	bool _showUpdatePrefabPopup;
	public static EditorPanelHierarchy I { get; private set; }

	public override void Init()
	{
		I = this;
		Scene.SceneLoaded += ResetGameObjectSelection;
	}

	public override void Update()
	{
		if (KeyboardInput.IsKeyDown(Keys.Delete) && _canDelete)
		{
			_canDelete = false;
			DestroySelectedGameObjects();
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.IsKeyUp(Keys.Backspace))
		{
			_canDelete = true;
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.IsKeyUp(Keys.C))
		{
			if (GameObjectSelectionManager.GetSelectedGameObject() != null)
			{
				_clipboardGameObject = GameObjectSelectionManager.GetSelectedGameObject();
				AssetSerializer.SaveClipboardGameObject(_clipboardGameObject);
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftSuper) && KeyboardInput.IsKeyUp(Keys.V))
		{
			if (_clipboardGameObject != null)
			{
				GameObject loadedGo = AssetSerializer.LoadClipboardGameObject();
				SelectGameObject(loadedGo.Id);
			}
		}
	}

	void DestroySelectedGameObjects()
	{
		int firstSelectedGameObjectIndex = SceneManager.CurrentScene.GetGameObject(_selectedGameObjectsIDs[0]).IndexInHierarchy;
		foreach (GameObject selectedGameObject in GameObjectSelectionManager.GetSelectedGameObjects())
		{
			_selectedGameObjectsIDs.Remove(selectedGameObject.Id);
			selectedGameObject.Destroy();

			GameObjectSelectionManager.SelectGameObjects(_selectedGameObjectsIDs);
		}

		int distance = int.MaxValue;
		int closestGameObjectId = -1;
		foreach (GameObject gameObject in SceneManager.CurrentScene.GameObjects)
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

	void MoveSelectedGameObject(int addToIndex = 1)
	{
		int direction = addToIndex;
		if (GameObjectSelectionManager.GetSelectedGameObjects().Count == 0)
		{
			return;
		}

		GameObject go = GameObjectSelectionManager.GetSelectedGameObjects()[0];
		int oldIndex = go.IndexInHierarchy;

		if (oldIndex + direction >= SceneManager.CurrentScene.GameObjects.Count || oldIndex + direction < 0)
		{
			return;
		}

		while (SceneManager.CurrentScene.GameObjects[oldIndex + direction].Transform.Parent != null)
		{
			direction += addToIndex;
		}

		SceneManager.CurrentScene.GameObjects.RemoveAt(oldIndex);
		SceneManager.CurrentScene.GameObjects.Insert(oldIndex + direction, go);


		//_selectedGameObjectsIndexes = oldIndex + direction;
		//GameObjectsSelected.Invoke(SceneManager.CurrentScene.GameObjects[oldIndex + direction].Id);
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
			GameObject go = GameObject.Create(name: "GameObject");
			go.Awake();
			go.Transform.WorldPosition = Camera.I.CenterOfScreenToWorld();
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
			foreach (int gameObjectId in _selectedGameObjectsIDs)
			{
				GameObject go = GameObject.Create(name: "Child");
				go.Awake();
				go.Transform.SetParent(SceneManager.CurrentScene.GetGameObject(gameObjectId).Transform);
			}
		}

		for (int goIndex = 0; goIndex < SceneManager.CurrentScene.GameObjects.Count; goIndex++)
		{
			//PushNextId();
			DrawGameObjectRow(goIndex);
		}

		ImGui.End();
	}

	void DrawGameObjectRow(int goIndex, bool isChild = false)
	{
		if (isChild == false)
		{
			PushNextId(SceneManager.CurrentScene.GameObjects[goIndex].Id.ToString());
		}


		GameObject currentGameObject = SceneManager.CurrentScene.GameObjects[goIndex];
		if (currentGameObject.Transform.Parent != null && isChild == false) // only draw children from recursive DrawGameObjectRow calls
		{
			return;
		}

		if (currentGameObject.Silent && Global.Debug == false)
		{
			return;
		}

		//bool hasAnyChildren = false;
		bool hasAnyChildren = currentGameObject.Transform.Children?.Count > 0;
		ImGuiTreeNodeFlags flags = (_selectedGameObjectsIDs.Contains(currentGameObject.Id) ? ImGuiTreeNodeFlags.Selected : 0) | ImGuiTreeNodeFlags.OpenOnArrow;
		if (hasAnyChildren == false)
		{
			flags = (_selectedGameObjectsIDs.Contains(currentGameObject.Id) ? ImGuiTreeNodeFlags.Selected : 0) | ImGuiTreeNodeFlags.Leaf;
		}


		Vector4 nameColor = currentGameObject.ActiveInHierarchy ? ImGui.GetStyle().Colors[(int) ImGuiCol.Text] : ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled];

		if (currentGameObject.IsPrefab)
		{
			nameColor = currentGameObject.ActiveInHierarchy ? Color.SkyBlue.ToVector4() : new Color(135, 206, 235, 130).ToVector4();
		}

		ImGui.PushStyleColor(ImGuiCol.Text, nameColor);

		string rowText = (Global.Debug ? $"[{currentGameObject.Id}] " : "") + currentGameObject.Name;
		flags |= ImGuiTreeNodeFlags.SpanFullWidth;
		flags |= ImGuiTreeNodeFlags.OpenOnDoubleClick;
		bool opened = ImGui.TreeNodeEx(rowText, flags);


		if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && false) // todo remove false
		{
			SceneViewNavigation.I.MoveToGameObject(GameObjectSelectionManager.GetSelectedGameObject());
		}


		if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
		{
			if (_selectedGameObjectsIDs != _gameObjectsIndexesSelectedBefore)
			{
				_selectedGameObjectsIDs = _gameObjectsIndexesSelectedBefore;
				GameObjectSelectionManager.SelectGameObjects(_selectedGameObjectsIDs);
			}

			// select gameobject selected before
			string gameObjectId = currentGameObject.Id.ToString();
			IntPtr stringPointer = Marshal.StringToHGlobalAnsi(gameObjectId);

			ImGui.SetDragDropPayload("GAMEOBJECT", stringPointer, (uint) (sizeof(char) * gameObjectId.Length));

			string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

			Marshal.FreeHGlobal(stringPointer);

			ImGui.Text(currentGameObject.Name);
			ImGui.EndDragDropSource();
		}

		if (ImGui.BeginDragDropTarget())
		{
			ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

			string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
			if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
			{
				GameObject foundGo = SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
				SetParent(child: foundGo.Transform, currentGameObject.Transform);
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


				int alreadySelectedGameObjectIndex = SceneManager.CurrentScene.GetGameObject(_selectedGameObjectsIDs[0]).IndexInHierarchy;
				int newlySelectedGameObjectIndex = currentGameObject.IndexInHierarchy;

				int selectionStartGameObjectIndex = Math.Min(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);
				int selectionEndGameObjectIndex = Math.Max(alreadySelectedGameObjectIndex, newlySelectedGameObjectIndex);

				foreach (GameObject gameObject in SceneManager.CurrentScene.GameObjects)
				{
					for (int i = 0; i < SceneManager.CurrentScene.GameObjects.Count; i++)
					{
						if (gameObject.IndexInHierarchy >= selectionStartGameObjectIndex && gameObject.IndexInHierarchy <= selectionEndGameObjectIndex)
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
		else if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
		{
			_gameObjectsIndexesSelectedBefore = _selectedGameObjectsIDs;
			SelectGameObject(currentGameObject.Id);
		}

		if (opened)
		{
			List<Transform> children = currentGameObject.Transform.Children;

			for (int childrenIndex = 0; childrenIndex < children.Count; childrenIndex++)
			{
				DrawGameObjectRow(children[childrenIndex].GameObject.IndexInHierarchy, true);
				//ImGui.TreePop();
			}

			ImGui.TreePop();
		}
	}

	private void SetParent(Transform child, Transform parent)
	{
		child.Transform.SetParent(parent);
	}
}

enum MoveDirection
{
	Up,
	Down
}