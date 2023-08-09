using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelBrowser : EditorPanel
{
	public override Vector2 Size => new Vector2(Tofu.I.Window.ClientSize.X - 1600, Tofu.I.Window.ClientSize.Y - Tofu.I.Editor.SceneViewSize.Y + 1);
	public override Vector2 Position => new Vector2(0, Tofu.I.Window.ClientSize.Y);
	public override Vector2 Pivot => new Vector2(0, 1);

	public override string Name => "Browser";
	string[] _assets = Array.Empty<string>();

	List<BrowserContextItem> _contextItems;

	public DirectoryInfo CurrentDirectory;
	Texture _directoryIcon;

	Texture _fileIcon;

	Vector2 _iconSize = new Vector2(200, 180);

	Texture[] _textures = Array.Empty<Texture>();
	public static EditorPanelBrowser I { get; private set; }

	TextureLoadSettings _iconTextureLoadSettings = new TextureLoadSettings(filterMode: TextureFilterMode.Point);
	readonly int _itemsInRow = 8;

	public override void Init()
	{
		I = this;

		Tofu.I.AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, "*");
		CreateContextItems();

		_fileIcon = Tofu.I.AssetManager.Load<Texture>(path: "Resources/FileIcon.png", loadSettings: _iconTextureLoadSettings);

		_directoryIcon = Tofu.I.AssetManager.Load<Texture>(path: "Resources/DirectoryIcon.png", loadSettings: _iconTextureLoadSettings);

		CurrentDirectory = new DirectoryInfo("Assets");

		RefreshAssets();
	}

	void CreateContextItems()
	{
		BrowserContextItem createSceneContextItem = new("Create Scene", "scene", ".scene", filePath =>
		{
			Tofu.I.SceneManager.CurrentScene.SetupAndSaveEmptyScene(filePath);
			RefreshAssets();
		});
		BrowserContextItem createMaterialContextItem = new("Create Material", "mat", ".mat", filePath =>
		{
			Material createdMaterial = new();
			createdMaterial.Path = filePath;
			// Tofu.I.AssetManager.Save<Material>(createdMaterial);
			RefreshAssets();
		});
		_contextItems = new List<BrowserContextItem>
		                {createSceneContextItem, createMaterialContextItem};
	}

	public override void Update()
	{
	}

	void OnFileChanged(FileChangedInfo fileChangedInfo)
	{
		string directoryName = Path.GetDirectoryName(fileChangedInfo.Path);
		string currentDirectoryAssetsRelativePath = Folders.GetPathRelativeToAssetsFolder(CurrentDirectory.FullName);
		bool fileGotDeletedInCurrentDirectory = fileChangedInfo.Path == currentDirectoryAssetsRelativePath; // when file is deleted, we only get the directory
		if (directoryName != currentDirectoryAssetsRelativePath && fileGotDeletedInCurrentDirectory == false)
		{
			return;
		}

		// file in currently selected folder changed
		RefreshAssets();
	}

	void RefreshAssets()
	{
		if (Directory.Exists(CurrentDirectory.FullName) == false)
		{
			return;
		}

		string[] tmpAssets = Directory.GetDirectories(CurrentDirectory.FullName);
		List<string> allAssets = tmpAssets.Concat(Directory.GetFiles(CurrentDirectory.FullName, "", SearchOption.TopDirectoryOnly)).ToList();

		for (int i = 0; i < allAssets.Count; i++)
		{
			if (Path.GetFileName(allAssets[i]).StartsWith('.'))
			{
				allAssets.RemoveAt(i);
				i--;
			}
		}

		_assets = allAssets.ToArray();

		for (int i = 0; i < _textures.Length; i++)
		{
			if (_textures[i] != null && _textures[i].Loaded)
			{
				_textures[i].Delete();
				_textures[i] = null;
			}
		}

		_textures = new Texture[_assets.Length];
		for (int i = 0; i < _assets.Length; i++)
		{
			string assetExtension = Path.GetExtension(_assets[i]).ToLower();

			if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") || assetExtension.ToLower().Contains(".jpeg"))
			{
				// _textures[i] = new Texture();
				// _textures[i].Load(path: _assets[i], loadSettings: _iconTextureLoadSettings);
				_textures[i] = Tofu.I.AssetManager.Load<Texture>(path: _assets[i], loadSettings: _iconTextureLoadSettings);
			}
		}
	}

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		SetWindow();

		ResetId();

		if (ImGui.Button("<"))
		{
			if (CurrentDirectory.Name.ToLower() != "assets")
			{
				CurrentDirectory = CurrentDirectory.Parent;
				RefreshAssets();
			}
		}

		ImGui.SameLine();

		if (ImGui.Button("Open in Finder"))
		{
			Process.Start(new ProcessStartInfo
			              {
				              FileName = CurrentDirectory.FullName,
				              UseShellExecute = true,
				              Verb = "open"
			              });
		}


		if (GameObjectSelectionManager.GetSelectedGameObject() != null)
		{
			ImGui.SameLine();

			// ResetId();

			PushNextId();
			bool saveBtnPressed = ImGui.Button("Save Prefab");
			if (saveBtnPressed)
			{
				Tofu.I.SceneSerializer.SaveGameObject(GameObjectSelectionManager.GetSelectedGameObject(), Path.Combine("Assets", CurrentDirectory.Name, GameObjectSelectionManager.GetSelectedGameObject().Name + ".prefab"));
			}
		}


		//for (int i = 0; i < assets.Length; i++)
		//{
		//	if (i > 0)
		//	{
		//		ImGui.SameLine();
		//	}
		//	ImGui.BeginGroup();
		//	string directoryName = new DirectoryInfo(directories[i]).Name;
		//	PushNextID();
		//
		//
		//	ImGui.PushStyleColor(ImGuiCol.Button, new Color(13, 27, 30).ToVector4());
		//	bool directoryClicked = ImGui.Button("FOLDER", new Vector2(100, 100));
		//	ImGui.PopStyleColor();
		//	if (directoryClicked)
		//	{
		//		currentDirectory = new DirectoryInfo(directories[i]);
		//		RefreshAssets();
		//		return;
		//	}
		//
		//	ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
		//	ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
		//
		//	string a = directoryName.Substring(0, Math.Clamp(directoryName.Length, 1, 12));
		//	ImGui.Text(a);
		//
		//
		//	ImGui.EndGroup();
		//}
		for (int assetIndex = 0; assetIndex < _assets.Length; assetIndex++)
		{
			if (assetIndex != 0 && assetIndex % _itemsInRow != 0)
			{
				ImGui.SameLine();
			}

			DirectoryInfo directoryInfo = new(_assets[assetIndex]);
			bool isDirectory = directoryInfo.Exists;

			ImGui.BeginGroup();
			string assetName = Path.GetFileNameWithoutExtension(_assets[assetIndex]);
			string assetExtension = Path.GetExtension(_assets[assetIndex]).ToLower();

			PushNextId();

			//ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0,0,0,0));

			if (isDirectory)
			{
				ImGui.ImageButton((IntPtr) _directoryIcon.TextureId, _iconSize);
				//ImGui.ImageButton((IntPtr) 0, new Vector2(100, 90));
			}
			else
			{
				if (_textures[assetIndex] != null && _textures[assetIndex].Loaded)
				{
					ImGui.ImageButton((IntPtr) _textures[assetIndex].TextureId, _iconSize);
				}
				else
				{
					//ImGui.ImageButton((IntPtr) fileIcon.id, new Vector2(100, 90));
					ImGui.ImageButton((IntPtr) _fileIcon.TextureId, _iconSize);
				}
			}
			//ImGui.PopStyleColor();


			if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") || assetExtension.ToLower().Contains(".jpeg"))
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = _assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("CONTENT_BROWSER_TEXTURE", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) _textures[assetIndex].TextureId, _iconSize);

					//ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (assetExtension.ToLower().Contains(".mp3") || assetExtension.ToLower().Contains(".wav"))
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = _assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) _fileIcon.TextureId, _iconSize);


					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (assetExtension.ToLower().Contains(".obj"))
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = _assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("CONTENT_BROWSER_MODEL", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) _fileIcon.TextureId, _iconSize);

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			bool isMaterial = assetExtension.ToLower().Contains(".mat");
			bool isShader = assetExtension.ToLower().Contains(".glsl");
			bool isPrefab = assetExtension.ToLower().Contains(".prefab");
			if (isShader || isMaterial)
			{
				if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
				{
					string itemPath = _assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					if (isMaterial)
					{
						ImGui.SetDragDropPayload("CONTENT_BROWSER_MATERIAL", stringPointer, (uint) (sizeof(char) * itemPath.Length));
					}

					if (isShader)
					{
						ImGui.SetDragDropPayload("CONTENT_BROWSER_SHADER", stringPointer, (uint) (sizeof(char) * itemPath.Length));
					}

					string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) _fileIcon.TextureId, new Vector2(100, 90));

					//ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}

				if (isShader)
				{
					if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
					{
						string assetsRelativePath = Path.Combine("Assets", Path.GetRelativePath("Assets", _assets[assetIndex]));

						Tofu.I.ShaderManager.QueueShaderReload(assetsRelativePath);
						Debug.Log($"Reloaded shader:{assetName}");
					}
				}
			}

			if (isPrefab)
			{
				if (ImGui.BeginDragDropSource())
				{
					string itemPath = _assets[assetIndex];
					IntPtr stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

					ImGui.SetDragDropPayload("PREFAB_PATH", stringPointer, (uint) (sizeof(char) * itemPath.Length));

					//string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

					ImGui.Image((IntPtr) _fileIcon.TextureId, _iconSize);

					Marshal.FreeHGlobal(stringPointer);

					ImGui.EndDragDropSource();
				}
			}

			if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
			{
				if (assetExtension.ToLower().Contains(".mat"))
				{
					EditorPanelInspector.I.OnMaterialSelected(_assets[assetIndex]);
				}
			}

			if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
			{
				if (isDirectory)
				{
					CurrentDirectory = directoryInfo;
					RefreshAssets();
					return;
				}

				if (assetExtension == ".prefab")
				{
					GameObject go = Tofu.I.SceneSerializer.LoadPrefab(_assets[assetIndex]);
					EditorPanelHierarchy.I.SelectGameObject(go.Id);
				}

				if (assetExtension == ".scene")
				{
					Tofu.I.SceneManager.LoadScene(_assets[assetIndex]);
				}
			}

			//ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
			//ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

			int maxCharsLimit = 15;
			string a = assetName.Substring(0, Math.Clamp(assetName.Length, 0, maxCharsLimit));
			ImGui.Text(a);
			if (assetName.Length > maxCharsLimit)
			{
				ImGui.Text(assetName.Substring(maxCharsLimit, assetName.Length - maxCharsLimit));
			}

			ImGui.EndGroup();
		}


		// show prefabs as btns from array that updates in Update()
		if (ImGui.BeginPopupContextWindow("BrowserPopup"))
		{
			for (int i = 0; i < _contextItems.Count; i++)
			{
				_contextItems[i].ShowContextItem();
			}
			/*if (ImGui.Button("New Scene"))
			{
				showCreateScenePopup = true;
				ImGui.CloseCurrentPopup();
			}

			if (ImGui.Button("New Material"))
			{
				showCreateMaterialPopup = true;
				ImGui.CloseCurrentPopup();
			}*/

			ImGui.EndPopup();
		}

		for (int i = 0; i < _contextItems.Count; i++)
		{
			_contextItems[i].ShowPopupIfOpen();
		}


		ImGui.End();

		base.Draw();
	}

	public void GoToFile(string directory)
	{
		if (File.Exists(directory) == false)
		{
			return;
		}

		CurrentDirectory = Directory.GetParent(directory);
	}
}