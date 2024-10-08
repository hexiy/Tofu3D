using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelBrowser : EditorPanel
{
    private readonly int _itemsInRow = 8;
    private string[] _assets = Array.Empty<string>();

    private List<BrowserContextItem> _contextItems;
    private Texture _directoryIcon;

    private Texture _fileIcon;

    private readonly Vector2 _iconSize = new(200, 180);

    private readonly TextureLoadSettings _iconTextureLoadSettings = new(filterMode: TextureFilterMode.Point);

    private Texture[] _textures = Array.Empty<Texture>();

    public DirectoryInfo CurrentDirectory;

    public override Vector2 Size => new(Tofu.Window.ClientSize.X - 1600,
        Tofu.Window.ClientSize.Y - Tofu.Editor.SceneViewSize.Y + 1);

    public override Vector2 Position => new(0, Tofu.Window.ClientSize.Y);
    public override Vector2 Pivot => new(0, 1);

    public override string Name => "Browser";
    public static EditorPanelBrowser I { get; private set; }

    public override void Init()
    {
        I = this;

        Tofu.AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, "*");
        CreateContextItems();

        _fileIcon = Tofu.AssetManager.Load<Texture>("Resources/FileIcon.png", _iconTextureLoadSettings);

        _directoryIcon = Tofu.AssetManager.Load<Texture>("Resources/DirectoryIcon.png", _iconTextureLoadSettings);

        CurrentDirectory = new DirectoryInfo("Assets");

        RefreshAssets();
    }

    private void CreateContextItems()
    {
        BrowserContextItem createSceneContextItem = new("Create Scene", "scene", ".scene",
            filePath =>
            {
                Tofu.SceneManager.CurrentScene.SetupAndSaveEmptyScene(filePath);
                RefreshAssets();
            });
        BrowserContextItem createMaterialContextItem = new("Create Material", "mat", ".mat",
            filePath =>
            {
                Material createdMaterial = new();
                createdMaterial.Path = filePath;
                Tofu.AssetManager.Save<Material>(createdMaterial);
                RefreshAssets();
            });
        _contextItems = new List<BrowserContextItem> { createSceneContextItem, createMaterialContextItem };
    }

    public override void Update()
    {
    }

    private void OnFileChanged(FileChangedInfo fileChangedInfo)
    {
        var directoryName = Path.GetDirectoryName(fileChangedInfo.Path);
        var currentDirectoryAssetsRelativePath = Folders.GetPathRelativeToAssetsFolder(CurrentDirectory.FullName);
        var fileGotDeletedInCurrentDirectory =
            fileChangedInfo.Path ==
            currentDirectoryAssetsRelativePath; // when file is deleted, we only get the directory
        if (directoryName != currentDirectoryAssetsRelativePath && fileGotDeletedInCurrentDirectory == false)
        {
            return;
        }

        // file in currently selected folder changed
        RefreshAssets();
    }

    private void RefreshAssets()
    {
        if (Directory.Exists(CurrentDirectory.FullName) == false)
        {
            return;
        }

        var tmpAssets = Directory.GetDirectories(CurrentDirectory.FullName);
        var allAssets = tmpAssets
            .Concat(Directory.GetFiles(CurrentDirectory.FullName, "", SearchOption.TopDirectoryOnly)).ToList();

        for (var i = 0; i < allAssets.Count; i++)
        {
            if (Path.GetFileName(allAssets[i]).StartsWith('.'))
            {
                allAssets.RemoveAt(i);
                i--;
            }
        }

        _assets = allAssets.ToArray();

        for (var i = 0; i < _textures.Length; i++)
        {
            if (_textures[i] != null && _textures[i].Loaded)
            {
                _textures[i].Delete();
                _textures[i] = null;
            }
        }

        _textures = new Texture[_assets.Length];
        for (var i = 0; i < _assets.Length; i++)
        {
            var assetExtension = Path.GetExtension(_assets[i]).ToLower();

            if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") ||
                assetExtension.ToLower().Contains(".jpeg"))
                // _textures[i] = new Texture();
                // _textures[i].Load(path: _assets[i], loadSettings: _iconTextureLoadSettings);
            {
                _textures[i] = Tofu.AssetManager.Load<Texture>(_assets[i], _iconTextureLoadSettings);
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
            var saveBtnPressed = ImGui.Button("Save Prefab");
            if (saveBtnPressed)
            {
                Tofu.SceneSerializer.SaveGameObject(GameObjectSelectionManager.GetSelectedGameObject(),
                    Path.Combine("Assets", CurrentDirectory.Name,
                        GameObjectSelectionManager.GetSelectedGameObject().Name + ".prefab"));
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
        for (var assetIndex = 0; assetIndex < _assets.Length; assetIndex++)
        {
            if (assetIndex != 0 && assetIndex % _itemsInRow != 0)
            {
                ImGui.SameLine();
            }

            DirectoryInfo directoryInfo = new(_assets[assetIndex]);
            var isDirectory = directoryInfo.Exists;

            ImGui.BeginGroup();
            var assetName = Path.GetFileNameWithoutExtension(_assets[assetIndex]);
            var assetExtension = Path.GetExtension(_assets[assetIndex]).ToLower();

            PushNextId();

            //ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0,0,0,0));

            if (isDirectory)
            {
                ImGui.ImageButton(_directoryIcon.TextureId, _iconSize);
                //ImGui.ImageButton((IntPtr) 0, new Vector2(100, 90));
            }
            else
            {
                if (_textures[assetIndex] != null && _textures[assetIndex].Loaded)
                {
                    ImGui.ImageButton(_textures[assetIndex].TextureId, _iconSize);
                }
                else
                    //ImGui.ImageButton((IntPtr) fileIcon.id, new Vector2(100, 90));
                {
                    ImGui.ImageButton(_fileIcon.TextureId, _iconSize);
                }
            }
            //ImGui.PopStyleColor();


            if (assetExtension.ToLower().Contains(".jpg") || assetExtension.ToLower().Contains(".png") ||
                assetExtension.ToLower().Contains(".jpeg"))
            {
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
                {
                    var itemPath = _assets[assetIndex];
                    var stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

                    ImGui.SetDragDropPayload("CONTENT_BROWSER_TEXTURE", stringPointer,
                        (uint)(sizeof(char) * itemPath.Length));

                    var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                    ImGui.Image(_textures[assetIndex].TextureId, _iconSize);

                    //ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));
                    Marshal.FreeHGlobal(stringPointer);

                    ImGui.EndDragDropSource();
                }
            }

            if (assetExtension.ToLower().Contains(".mp3") || assetExtension.ToLower().Contains(".wav"))
            {
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
                {
                    var itemPath = _assets[assetIndex];
                    var stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

                    ImGui.SetDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", stringPointer,
                        (uint)(sizeof(char) * itemPath.Length));

                    var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                    ImGui.Image(_fileIcon.TextureId, _iconSize);


                    Marshal.FreeHGlobal(stringPointer);

                    ImGui.EndDragDropSource();
                }
            }

            if (assetExtension.ToLower().Contains(".obj"))
            {
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
                {
                    var itemPath = _assets[assetIndex];
                    var stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

                    ImGui.SetDragDropPayload("CONTENT_BROWSER_MODEL", stringPointer,
                        (uint)(sizeof(char) * itemPath.Length));

                    var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                    ImGui.Image(_fileIcon.TextureId, _iconSize);

                    Marshal.FreeHGlobal(stringPointer);

                    ImGui.EndDragDropSource();
                }
            }

            var isMaterial = assetExtension.ToLower().Contains(".mat");
            var isShader = assetExtension.ToLower().Contains(".glsl");
            var isPrefab = assetExtension.ToLower().Contains(".prefab");
            if (isShader || isMaterial)
            {
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
                {
                    var itemPath = _assets[assetIndex];
                    var stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

                    if (isMaterial)
                    {
                        ImGui.SetDragDropPayload("CONTENT_BROWSER_MATERIAL", stringPointer,
                            (uint)(sizeof(char) * itemPath.Length));
                    }

                    if (isShader)
                    {
                        ImGui.SetDragDropPayload("CONTENT_BROWSER_SHADER", stringPointer,
                            (uint)(sizeof(char) * itemPath.Length));
                    }

                    var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                    ImGui.Image(_fileIcon.TextureId, new Vector2(100, 90));

                    //ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

                    Marshal.FreeHGlobal(stringPointer);

                    ImGui.EndDragDropSource();
                }

                if (isShader)
                {
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        var assetsRelativePath =
                            Path.Combine("Assets", Path.GetRelativePath("Assets", _assets[assetIndex]));

                        Tofu.ShaderManager.QueueShaderReload(assetsRelativePath);
                        Debug.Log($"Reloaded shader:{assetName}");
                    }
                }
            }

            if (isPrefab)
            {
                if (ImGui.BeginDragDropSource())
                {
                    var itemPath = _assets[assetIndex];
                    var stringPointer = Marshal.StringToHGlobalAnsi(itemPath);

                    ImGui.SetDragDropPayload("PREFAB_PATH", stringPointer, (uint)(sizeof(char) * itemPath.Length));

                    //string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                    ImGui.Image(_fileIcon.TextureId, _iconSize);

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
                    var go = Tofu.SceneSerializer.LoadPrefab(_assets[assetIndex]);
                    // todo
                    // EditorPanelHierarchy.I.SelectGameObject(go.Id);
                }

                if (assetExtension == ".scene")
                {
                    Tofu.SceneManager.LoadScene(_assets[assetIndex]);
                }
            }

            //ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
            //ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);

            var maxCharsLimit = 15;
            var a = assetName.Substring(0, Math.Clamp(assetName.Length, 0, maxCharsLimit));
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
            for (var i = 0; i < _contextItems.Count; i++)
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

        for (var i = 0; i < _contextItems.Count; i++)
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
        RefreshAssets();
    }
}