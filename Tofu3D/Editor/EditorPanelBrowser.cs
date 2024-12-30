using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImGui = ImGuiNET.ImGui;

namespace Tofu3D;

public class EditorPanelBrowser : EditorPanel
{
    private int _itemsInRow = 8;
    private string[] _assets = Array.Empty<string>();

    private List<BrowserContextItem> _contextItems;
    private RuntimeTexture _directoryIcon;
    Dictionary<string, DirectoryInfo> directoryInfos = new();

    private RuntimeTexture _fileIcon;
    private Vector2 _iconSize => new Vector2(100, 90) * Screen.Scale;

    private int _subAssetsDrawnCount = 0;
    // private readonly TextureLoadSettings _iconTextureLoadSettings = new(filterMode: TextureFilterMode.Point);

    private Dictionary<string, RuntimeTexture>
        _textures = new Dictionary<string, RuntimeTexture>(); // path, and texture

    /// <summary>
    /// Stores paths
    /// </summary>
    private List<int> _expandedAssets = new List<int>();

    public DirectoryInfo CurrentDirectoryInfo;

    private string CurrentDirectoryPath
    {
        get
        {
            string path = PersistentData.GetString("CurrentDirectoryPath", Folders.Assets);
            if (Directory.Exists(path) == false)
            {
                CurrentDirectoryPath = Folders.Assets;
                return Folders.Assets;
            }

            return path;
        }
        set
        {
            // string relativePath = Path.GetRelativePath(Folders.Assets, value);
            PersistentData.Set("CurrentDirectoryPath", value);
        }
    }

    public override Vector2 Position => new(0, Tofu.Window.ClientSize.Y);
    public override Vector2 Pivot => new(0, 1);

    public override string Name => "Browser";
    public static EditorPanelBrowser I { get; private set; }


    public override void Init()
    {
        I = this;

        Tofu.AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, "*");
        CreateContextItems();

        _fileIcon =
            Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/FileIcon_b.png"); //, _iconTextureLoadSettings);

        _directoryIcon =
            Tofu.AssetLoadManager.Load<RuntimeTexture>("Resources/DirectoryIcon_b.png"); //, _iconTextureLoadSettings);

        SetCurrentDirectory(CurrentDirectoryPath);

        RefreshAssets();
    }

    private void SetCurrentDirectory(string path)
    {
        SetCurrentDirectory(new DirectoryInfo(path));
    }

    private void SetCurrentDirectory(DirectoryInfo directoryInfo)
    {
        CurrentDirectoryPath = directoryInfo.FullName;
        CurrentDirectoryInfo = directoryInfo;
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
                Asset_Material createdMaterial = new();
                createdMaterial.PathToRawAsset = filePath;
                Tofu.AssetLoadManager.Save<Asset_Material>(filePath, createdMaterial);
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
        var currentDirectoryAssetsRelativePath = Folders.GetPathRelativeToAssetsFolder(CurrentDirectoryInfo.FullName);
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
        if (Directory.Exists(CurrentDirectoryInfo.FullName) == false)
        {
            return;
        }

        var tmpAssets = Directory.GetDirectories(CurrentDirectoryInfo.FullName);
        var allAssets = tmpAssets
            .Concat(Directory.GetFiles(CurrentDirectoryInfo.FullName, "", SearchOption.TopDirectoryOnly)).ToList();

        for (var i = 0; i < allAssets.Count; i++)
        {
            string fileName = Path.GetFileName(allAssets[i]);
            if (fileName.StartsWith('.') || AssetFileExtensions.IsAssetImportParametersFile(fileName) ||
                fileName.EndsWith(".mtl", StringComparison.OrdinalIgnoreCase))
            {
                allAssets.RemoveAt(i);
                i--;
            }
        }

        _assets = allAssets.ToArray();

        // for (var i = 0; i < _textures.Length; i++)
        // {
        //     if (_textures[i] != null) // && _textures[i].Loaded)
        //     {
        //         _textures[i].Delete();
        //         _textures[i] = null;
        //     }
        // }

        _textures = new Dictionary<string, RuntimeTexture>();
        for (var i = 0; i < _assets.Length; i++)
        {
            if (AssetFileExtensions.IsFileTexture(_assets[i]))
                // _textures[i] = new Texture();
                // _textures[i].Load(path: _assets[i], loadSettings: _iconTextureLoadSettings);
            {
                _textures[_assets[i]] =
                    Tofu.AssetLoadManager.Load<RuntimeTexture>(_assets[i]); //, _iconTextureLoadSettings);
            }

            if (AssetFileExtensions.IsFileScene(_assets[i]))
            {
                string thumbnailPath = Scene.GetThumbnailPath(_assets[i]);
                if (File.Exists(thumbnailPath))
                {
                    _textures[_assets[i]] = Tofu.AssetLoadManager.Load<RuntimeTexture>(thumbnailPath);
                }
            }
        }
    }

    private int hoveredAssetIndex = -1;

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        BeginWindowDefault();
        if (ImGui.BeginPopupContextWindow("yeh"))
        {
            for (var i = 0; i < _contextItems.Count; i++)
            {
                _contextItems[i].ShowContextItem();
            }

            ImGui.EndPopup();
        }

        ResetId();

        if (ImGui.Button("<")
            || (IsPanelHovered && KeyboardInput.IsKeyDown(Keys.Backspace))
            || (KeyboardInput.IsKeyDown(Keys.LeftCmd) && KeyboardInput.WasKeyJustPressed(Keys.Up)))
        {
            if (CurrentDirectoryInfo.Name.Equals("assets", StringComparison.OrdinalIgnoreCase) == false &&
                IsPanelHovered)
            {
                SetCurrentDirectory(CurrentDirectoryInfo.Parent);
                RefreshAssets();
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Open in Finder"))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = CurrentDirectoryInfo.FullName,
                UseShellExecute = true,
                Verb = "open"
            });
        }


        if (Tofu.GameObjectSelectionManager.GetSelectedGameObject() != null)
        {
            ImGui.SameLine();

            // ResetId();

            PushNextId();
            var saveBtnPressed = ImGui.Button("Save Prefab");
            if (saveBtnPressed)
            {
                Tofu.SceneSerializer.SaveGameObject(Tofu.GameObjectSelectionManager.GetSelectedGameObject(),
                    Path.Combine("Assets", CurrentDirectoryInfo.Name,
                        Tofu.GameObjectSelectionManager.GetSelectedGameObject().Name + ".prefab"));

                EditorPanelBrowser.I.RefreshAssets();
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
        _subAssetsDrawnCount = 0;
        hoveredAssetIndex = -1;
        for (var assetIndex = 0; assetIndex < _assets.Length; assetIndex++)
        {
            string assetPath = _assets[assetIndex];
            DrawAsset(assetIndex, assetPath);


            //ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
            //ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
        }

        Debug.StatSetValue("Browser hovered asset index", $"Browser hovered asset index {hoveredAssetIndex}");

        for (var i = 0; i < _contextItems.Count; i++)
        {
            _contextItems[i].ShowPopupIfOpen();
        }


        ImGui.End();

        base.Draw();
    }


    private void DrawAsset(int assetIndex, string assetPath)
    {
        _itemsInRow = (int)MathF.Floor(ImGui.GetWindowSize().X / (_iconSize.X + 10));
        _itemsInRow = Mathf.Max(_itemsInRow, 1);

        if (assetIndex != 0 && (assetIndex + _subAssetsDrawnCount) % _itemsInRow != 0)
        {
            ImGui.SameLine();
        }

        directoryInfos.TryGetValue(assetPath, out DirectoryInfo directoryInfo);
        if (directoryInfo == null)
        {
            directoryInfo = new DirectoryInfo(assetPath);
            directoryInfos.Add(assetPath, directoryInfo);
        }

        var isDirectory = directoryInfo.Exists;
        ImGui.BeginGroup();

        ImGui.BeginGroup();
        var assetPathPointer = Marshal.StringToHGlobalAnsi(assetPath);

        var assetName = Path.GetFileNameWithoutExtension(assetPath);
        var assetExtension = Path.GetExtension(assetPath);


        bool isMesh = AssetFileExtensions.IsFileMesh(assetPath);
        var isModel = AssetFileExtensions.IsFileModel(assetPath);
        var isMaterial = AssetFileExtensions.IsFileMaterial(assetPath);
        var isShader = AssetFileExtensions.IsFileShader(assetPath);
        var isPrefab = AssetFileExtensions.IsFilePrefab(assetPath);
        var isTexture = AssetFileExtensions.IsFileTexture(assetPath);
        var isScene = AssetFileExtensions.IsFileScene(assetPath);

        PushNextId();

        //ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0,0,0,0));

        ImGui.PushStyleColor(ImGuiCol.Button, Color.Transparent.ToVector4());
        if (_expandedAssets.Contains(assetIndex))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Color.AntiqueWhite.ToVector4());
            // ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.MidnightBlue.ToVector4());
        }

        //
        //     {
        //         
        //         
        //         Vector4 cBeige = new(1f, 0.96f, 0.90f, 1.00f);
        //         Vector4 cBeigeMid = new(0.97f, 0.94f, 0.88f, 1f);
        //         Vector4 cBeigeDarker = new(0.94f, 0.91f, 0.85f, 1f);
        //         Vector4 cScrollbar = new(0.74f, 0.71f, 0.65f, 0.8f);
        //         Vector4 cScrollbarDarker = new(0.64f, 0.61f, 0.55f, 1f);
        //         
        //     ImDrawListPtr dl = ImGui.GetWindowDrawList();
        //     Vector2 cursor = ImGui.GetCursorPos();
        //     Vector2 p_min = ImGui.GetCursorScreenPos();
        //     Vector2 p_max = new Vector2(p_min.X + _iconSize.X, p_min.Y + _iconSize.Y+35);
        //     dl.AddImageRounded(Tofu.Editor.EditorTextures.WhitePixel.TextureId, p_min, p_max,
        //         new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1),
        //         ImGui.GetColorU32(cBeigeDarker),
        //         10);
        // }


        if (isDirectory)
        {
            ImGui.ImageButton(_directoryIcon.TextureId, _iconSize);
            //ImGui.ImageButton((IntPtr) 0, new Vector2(100, 90));
        }
        else
        {
            if (_textures.ContainsKey(assetPath)) // && _textures[assetIndex].Loaded)
            {
                // if (isScene)
                // {
                //     ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,10);
                // }
                // ImGui.ImageButton(_textures[assetPath].TextureId, _iconSize);
                // if (isScene)
                // {
                //     ImGui.PopStyleVar(1);
                // }

                ImDrawListPtr dl = ImGui.GetWindowDrawList();
                Vector2 cursor = ImGui.GetCursorPos();
                Vector2 p_min = ImGui.GetCursorScreenPos();
                Vector2 p_max = new Vector2(p_min.X + _iconSize.X, p_min.Y + _iconSize.Y);
                dl.AddImageRounded(_textures[assetPath].TextureId, p_min, p_max,
                    new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1),
                    ImGui.GetColorU32(new System.Numerics.Vector4(1, 1, 1, 1)),
                    10);

                // ImGui.Dummy(_iconSize);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(1, 1, 1, 0));
                ImGui.ImageButton(_textures[assetPath].TextureId, _iconSize, new System.Numerics.Vector2(0, 0),
                    new System.Numerics.Vector2(1, 1), 0, System.Numerics.Vector4.Zero, System.Numerics.Vector4.Zero);
                ImGui.PopStyleColor();
            }
            else
                //ImGui.ImageButton((IntPtr) fileIcon.id, new Vector2(100, 90));
            {
                ImGui.ImageButton(_fileIcon.TextureId, _iconSize);

                // ImGui.ImageButton(_fileIcon.TextureId, _iconSize);
            }
        }

        ImGui.PopStyleColor();
        if (_expandedAssets.Contains(assetIndex))
        {
            ImGui.PopStyleColor();
        }

        if (isTexture)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                ImGui.SetDragDropPayload("CONTENT_BROWSER_TEXTURE", assetPathPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                ImGui.Image(_textures[assetPath].TextureId, _iconSize);

                //ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));
                Marshal.FreeHGlobal(assetPathPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (assetExtension.Contains(".mp3", StringComparison.OrdinalIgnoreCase) ||
            assetExtension.Contains(".wav", StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                ImGui.SetDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", assetPathPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                ImGui.Image(_fileIcon.TextureId, _iconSize);


                Marshal.FreeHGlobal(assetPathPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (isModel || isMesh)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                var stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                string payloadType = isMesh ? DragDropPayloadTypes.Mesh : DragDropPayloadTypes.Model;
                ImGui.SetDragDropPayload(payloadType, stringPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                // var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                ImGui.Image(_fileIcon.TextureId, _iconSize);

                Marshal.FreeHGlobal(stringPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (isShader || isMaterial)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                var stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                if (isMaterial)
                {
                    ImGui.SetDragDropPayload("CONTENT_BROWSER_MATERIAL", stringPointer,
                        (uint)(sizeof(char) * assetPath.Length));
                }

                if (isShader)
                {
                    ImGui.SetDragDropPayload("CONTENT_BROWSER_SHADER", stringPointer,
                        (uint)(sizeof(char) * assetPath.Length));
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
                        Path.Combine("Assets", Path.GetRelativePath("Assets", assetPath));

                    Tofu.ShaderManager.QueueShaderReload(assetsRelativePath);
                    Debug.Log($"Reloaded shader:{assetName}");
                }
            }
        }

        if (isPrefab)
        {
            if (ImGui.BeginDragDropSource())
            {
                var stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                ImGui.SetDragDropPayload("PREFAB_PATH", stringPointer, (uint)(sizeof(char) * assetPath.Length));

                //string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                ImGui.Image(_fileIcon.TextureId, _iconSize);

                Marshal.FreeHGlobal(stringPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (ImGui.IsItemHovered() &&
            ImGui.IsMouseReleased(ImGuiMouseButton.Left)) // released in case we want to drag and drop somemthing
        {
            if (isMaterial)
            {
                EditorPanelInspector.I.OnMaterialSelected(assetPath);
            }

            if (isModel)
            {
                var pathOfImportParametersOfSourceAssetFile = assetPath.GetPathOfImportParametersOfSourceAssetFile();
                Object importParameters =
                    Serializer.ReadFileJSON<AssetImportParameters_Model>(pathOfImportParametersOfSourceAssetFile);


                if (importParameters != null)
                {
                    EditorPanelInspector.I.SelectInspectable(importParameters,
                        anyValueChanged: () =>
                        {
                            Serializer.SaveFileJSON<AssetImportParameters_Model>(
                                pathOfImportParametersOfSourceAssetFile, importParameters);
                        });
                }

                if (_expandedAssets.Contains(assetIndex) == false)
                {
                    _expandedAssets.Add(assetIndex);
                }
                else
                {
                    _expandedAssets.Remove(assetIndex);
                }
            }

            if (isTexture)
            {
                var pathOfImportParametersOfSourceAssetFile = assetPath.GetPathOfImportParametersOfSourceAssetFile();
                Object importParameters =
                    Serializer.ReadFileJSON<AssetImportParameters_Texture>(pathOfImportParametersOfSourceAssetFile);


                if (importParameters != null)
                {
                    EditorPanelInspector.I.SelectInspectable(importParameters,
                        anyValueChanged: () =>
                        {
                            Serializer.SaveFileJSON<AssetImportParameters_Texture>(
                                pathOfImportParametersOfSourceAssetFile, importParameters);
                        });
                }
            }
        }

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            if (isDirectory)
            {
                SetCurrentDirectory(directoryInfo);

                RefreshAssets();
                return;
            }

            if (isPrefab)
            {
                var go = Tofu.SceneSerializer.LoadPrefab(assetPath);
                // todo
                // EditorPanelHierarchy.I.SelectGameObject(go.Id);
            }

            if (assetExtension == ".scene")
            {
                Tofu.SceneManager.LoadScene(assetPath);
            }
        }


        var maxCharsLimit = 15;
        var a = assetName.Substring(0, Math.Clamp(assetName.Length, 0, maxCharsLimit));
        Vector2 textSize = ImGui.CalcTextSize(a);

        if (textSize.X < _iconSize.X)
        {
            float spaceLeft = textSize.X - _iconSize.X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - spaceLeft / 2f);
        }

        ImGui.Text(a);


        if (assetName.Length > maxCharsLimit)
        {
            // ImGui.Text(assetName.Substring(maxCharsLimit, assetName.Length - maxCharsLimit));
        }

        ImGui.EndGroup();


        if (isModel)
        {
            if (_expandedAssets.Contains(assetIndex))
            {
                Asset_Model assetModel = Tofu.AssetLoadManager.Load<Asset_Model>(assetPath);
                for (int meshIndex = 0; meshIndex < assetModel.PathsToMeshAssets.Count; meshIndex++)
                {
                    // ImGui.SameLine();
                    _subAssetsDrawnCount++;
                    DrawAsset(assetIndex, assetModel.PathsToMeshAssets[meshIndex]);
                }
            }
        }

        ImGui.EndGroup();
        if (ImGui.IsItemHovered() && hoveredAssetIndex != assetIndex)
        {
            hoveredAssetIndex = assetIndex;
        }

        if (isMesh == false && isDirectory == false)
        {
            if (ImGui.BeginPopupContextItem("item_context", ImGuiPopupFlags.MouseButtonRight))
            {
                if (ImGui.MenuItem("Reimport this"))
                {
                    Tofu.AssetImportManager.ImportAsset(assetPath, reimportIfExists: true);
                }

                if (ImGui.MenuItem("Reimport all"))
                {
                    Tofu.AssetImportManager.ImportAllAssets(reimportIfExists: true);
                }

                ImGui.EndPopup();
            }
        }
    }

    public void GoToFile(string directory)
    {
        if (File.Exists(directory) == false)
        {
            return;
        }

        SetCurrentDirectory(directoryInfo: Directory.GetParent(directory));
        RefreshAssets();
    }
}