using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using TofuEngine.Source;
using ImGui = ImGuiNET.ImGui;

namespace TofuEngine;

public class EditorPanelBrowser : EditorPanel
{
    private int _itemsInRow = 8;
    private string[] _assets = Array.Empty<string>();

    private List<BrowserContextItemCreateFile> _contextItems;
    private RuntimeTexture _directoryIcon;
    Dictionary<string, DirectoryInfo> directoryInfos = new Dictionary<string, DirectoryInfo>();

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

    private int _hoveredAssetIndex = -1;

    public DirectoryInfo CurrentDirectoryInfo;

    private string CurrentDirectoryPathCached
    {
        get
        {
            string path = PersistentData.GetString("CurrentDirectoryPath", Folders.Assets);
            if (Directory.Exists(path) == false)
            {
                CurrentDirectoryPathCached = Folders.Assets;
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

    public override Vector2 Position => new Vector2(0, Tofu.Window.ClientSize.Y);
    public override Vector2 Pivot => new Vector2(0, 1);

    public override string Name => "Browser";
    public static EditorPanelBrowser I { get; private set; }


    public override void Init()
    {
        I = this;

        Tofu.AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, "*");
        CreateContextItems();


        _fileIcon =
            Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/FileIcon_b.png"); //, _iconTextureLoadSettings);

        _directoryIcon =
            Tofu.AssetLoadManager.Get<RuntimeTexture>("Resources/DirectoryIcon_b.png"); //, _iconTextureLoadSettings);

        SetCurrentDirectory(CurrentDirectoryPathCached);

        RefreshAssets();
    }

    private void SetCurrentDirectory(string path)
    {
        SetCurrentDirectory(new DirectoryInfo(path));
    }

    private void SetCurrentDirectory(DirectoryInfo directoryInfo)
    {
        CurrentDirectoryPathCached = directoryInfo.FullName;
        CurrentDirectoryInfo = directoryInfo;
    }

    private void CreateContextItems()
    {
        BrowserContextItemCreateFile createSceneContextItemCreateFile = new BrowserContextItemCreateFile("Create Scene",
            "scene", ".scene",
            filePath =>
            {
                Tofu.SceneManager.CurrentScene.SetupAndSaveEmptyScene(filePath);
                RefreshAssets();
            });
        BrowserContextItemCreateFile createMaterialContextItemCreateFile = new BrowserContextItemCreateFile(
            "Create Material", "mat", ".mat",
            filePath =>
            {
                Asset_Material createdMaterial = new Asset_Material { PathInAssetsFolder = filePath };
                Tofu.AssetLoadManager.Save<Asset_Material>(filePath, createdMaterial);
                RefreshAssets();
            });
        BrowserContextItemCreateFile createFolderContextItemCreateFile = new BrowserContextItemCreateFile("New Folder",
            "folder", "",
            filePath =>
            {
                Directory.CreateDirectory(filePath);
                RefreshAssets();
            });
        BrowserContextItemCreateFile createScriptContextItemCreateFile = new BrowserContextItemCreateFile(
            "New C# Component", "MyComponent", ".cs",
            filePath =>
            {
                string scriptName = TofuPath.GetFileNameWithoutExtension(filePath);
                ScriptsManager.CreateCustomComponentFile(scriptName, filePath);
                RefreshAssets();
            });
        _contextItems = new List<BrowserContextItemCreateFile>
        {
            createSceneContextItemCreateFile,
            createMaterialContextItemCreateFile,
            createFolderContextItemCreateFile,
            createScriptContextItemCreateFile
        };
    }

    public override void Update()
    {
    }

    private void OnFileChanged(FileChangedInfo fileChangedInfo)
    {
        string? directoryName = Path.GetDirectoryName(fileChangedInfo.Path);
        string currentDirectoryAssetsRelativePath =
            Folders.GetPathRelativeToAssetsFolder(CurrentDirectoryInfo.FullName);
        bool fileGotDeletedInCurrentDirectory =
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

        string[] tmpAssets = Directory.GetDirectories(CurrentDirectoryInfo.FullName);
        List<string> allAssets = tmpAssets
            .Concat(Directory.GetFiles(CurrentDirectoryInfo.FullName, "", SearchOption.TopDirectoryOnly)).ToList();

        for (int i = 0; i < allAssets.Count; i++)
        {
            string fileName = Path.GetFileName(allAssets[i]);
            if (fileName.StartsWith('.') || AssetPathExtensions.IsAssetImportParametersFile(fileName) ||
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
        for (int i = 0; i < _assets.Length; i++)
        {
            if (AssetPathExtensions.IsFileTexture(_assets[i]))
                // _textures[i] = new Texture();
                // _textures[i].Load(path: _assets[i], loadSettings: _iconTextureLoadSettings);
            {
                _textures[_assets[i]] =
                    Tofu.AssetLoadManager.Get<RuntimeTexture>(_assets[i]); //, _iconTextureLoadSettings);
            }

            if (AssetPathExtensions.IsFileScene(_assets[i]))
            {
                // disabled, doesnt work with texture atlases for now
                // string thumbnailPath = Scene.GetThumbnailPath(_assets[i]);
                // if (File.Exists(thumbnailPath))
                // {
                //     _textures[_assets[i]] = Tofu.AssetLoadManager.Load<RuntimeTexture>(thumbnailPath);
                // }
            }
        }
    }


    protected override void ExecuteImGuiDrawCommands()
    {
        ResetId();

        if (ImGui.BeginPopupContextWindow("yeh"))
        {
            for (int i = 0; i < _contextItems.Count; i++)
            {
                _contextItems[i].ShowContextItem();
            }

            ImGui.EndPopup();
        }


        if (ImGui.Button("<")
            || (IsPanelHovered && KeyboardInput.IsKeyDown(Keys.Backspace))
            || (KeyboardInput.IsKeyDown(Keys.LeftCmd) && KeyboardInput.WasKeyJustPressed(Keys.Up)))
        {
            if (CurrentDirectoryInfo.Name.Equals("assets", StringComparison.OrdinalIgnoreCase) == false &&
                IsPanelHovered)
            {
                // SetCurrentDirectory(CurrentDirectoryInfo.Parent);
                CurrentDirectoryInfo = CurrentDirectoryInfo.Parent;
                RefreshAssets();
                CurrentDirectoryPathCached =
                    CurrentDirectoryInfo.Parent.FullName; // in case RefreshAssets throws an exception
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
            bool saveBtnPressed = ImGui.Button("Save Prefab");
            if (saveBtnPressed)
            {
                Tofu.SceneSerializer.SaveGameObject(Tofu.GameObjectSelectionManager.GetSelectedGameObject(),
                    TofuPath.Combine("Assets", CurrentDirectoryInfo.Name,
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
        _hoveredAssetIndex = -1;
        for (int assetIndex = 0; assetIndex < _assets.Length; assetIndex++)
        {
            string assetPath = _assets[assetIndex];
            DrawAsset(assetIndex, assetPath);


            //ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 25);
            //ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5);
        }

        // Debug.StatSetValue("Browser hovered asset index", $"Browser hovered asset index {hoveredAssetIndex}");

        for (int i = 0; i < _contextItems.Count; i++)
        {
            _contextItems[i].ShowPopupIfOpen();
        }

        PopAllIds();
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

        bool isDirectory = directoryInfo.Exists;
        ImGui.BeginGroup();

        ImGui.BeginGroup();
        IntPtr assetPathPointer = Marshal.StringToHGlobalAnsi(assetPath);

        string assetName = Path.GetFileNameWithoutExtension(assetPath);
        string assetExtension = Path.GetExtension(assetPath);


        FileType fileType = AssetPathExtensions.GetFileType(assetPath);

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
            TofuImGui.ImageTexture2DArray(_directoryIcon, _iconSize);
            // ImGui.ImageButton(_directoryIcon.AtlasGLTextureArrayId, _iconSize);
        }
        else
        {
            if (_textures.ContainsKey(assetPath)) // && _textures[assetIndex].Loaded)
            {
                // if (fileType is FileType.Texture)
                // {
                //     Vector2 pos = ImGui.GetCursorPos();
                //     TofuImGui.ImageTexture2DArray(Tofu.Editor.EditorTextures.Checkerboard, size: _iconSize);
                //     ImGui.SetCursorPos(pos);
                // }

                ImDrawListPtr dl = ImGui.GetWindowDrawList();
                Vector2 cursor = ImGui.GetCursorPos();
                Vector2 p_min = ImGui.GetCursorScreenPos();
                Vector2 p_max = new Vector2(p_min.X + _iconSize.X, p_min.Y + _iconSize.Y);


                Vector4 boundingBoxInAtlas = _textures[assetPath].BoundingBoxInAtlas;
                Vector2 uvMin = new Vector2(boundingBoxInAtlas.X, boundingBoxInAtlas.Y);
                Vector2 uvMax = new Vector2(boundingBoxInAtlas.Z, boundingBoxInAtlas.W);
                IntPtr encodedId = ImGuiController.EncodeTextureArrayId(_textures[assetPath]);
                dl.AddImageRounded(encodedId, p_min, p_max,
                    uvMin, uvMax,
                    ImGui.GetColorU32(new System.Numerics.Vector4(1, 1, 1, 1)),
                    10);

                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(1, 1, 1, 0));
                TofuImGui.ImageButtonTexture2DArray(_textures[assetPath], _iconSize, bg_col: Vector4.Zero,
                    tint_col: Vector4.Zero);
                // ImGui.ImageButton(_textures[assetPath].AtlasGLTextureArrayId, _iconSize, new System.Numerics.Vector2(0, 0),
                // new System.Numerics.Vector2(1, 1), 0, System.Numerics.Vector4.Zero, System.Numerics.Vector4.Zero);
                ImGui.PopStyleColor();
            }
            else
            {
                TofuImGui.ImageButtonTexture2DArray(_fileIcon, _iconSize);
                // ImGui.ImageButton(_fileIcon.StandaloneGLTextureId.Value, _iconSize);
            }
        }

        ImGui.PopStyleColor();
        if (_expandedAssets.Contains(assetIndex))
        {
            ImGui.PopStyleColor();
        }

        if (fileType is FileType.Texture)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                ImGui.SetDragDropPayload(DragDropPayloadTypes.Texture, assetPathPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                TofuImGui.ImageTexture2DArray(_textures[assetPath], _iconSize);

                //ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));
                Marshal.FreeHGlobal(assetPathPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (fileType is FileType.Audio)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                ImGui.SetDragDropPayload(DragDropPayloadTypes.AudioClip, assetPathPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                TofuImGui.ImageTexture2DArray(_fileIcon, _iconSize);

                Marshal.FreeHGlobal(assetPathPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (fileType is FileType.Model or FileType.Mesh)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                IntPtr stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                string payloadType = fileType is FileType.Mesh ? DragDropPayloadTypes.Mesh : DragDropPayloadTypes.Model;
                ImGui.SetDragDropPayload(payloadType, stringPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                // var payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                TofuImGui.ImageTexture2DArray(_fileIcon, _iconSize);

                Marshal.FreeHGlobal(stringPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (fileType is FileType.Shader or FileType.Material)
        {
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) // DRAG N DROP
            {
                IntPtr stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                if (fileType is FileType.Material)
                {
                    ImGui.SetDragDropPayload(DragDropPayloadTypes.Material, stringPointer,
                        (uint)(sizeof(char) * assetPath.Length));
                }

                if (fileType is FileType.Shader)
                {
                    ImGui.SetDragDropPayload(DragDropPayloadTypes.Shader, stringPointer,
                        (uint)(sizeof(char) * assetPath.Length));
                }

                string? payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

                TofuImGui.ImageTexture2DArray(_fileIcon, new Vector2(100, 90));

                //ImGui.Text(Path.GetFileNameWithoutExtension(itemPath));

                Marshal.FreeHGlobal(stringPointer);

                ImGui.EndDragDropSource();
            }

            if (fileType is FileType.Shader)
            {
                // if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                // {
                //     string assetsRelativePath =
                //         TofuPath.Combine("Assets", Path.GetRelativePath("Assets", assetPath));
                //
                //     Tofu.ShaderManager.QueueShaderReload(assetsRelativePath);
                //     Debug.Log($"Reloaded shader:{assetName}");
                // }
                if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    Tofu.UserCodeEditorOpener.OpenFile(assetPath, 0, 0);
                }
            }
        }

        if (fileType is FileType.Prefab)
        {
            if (ImGui.BeginDragDropSource())
            {
                IntPtr stringPointer = Marshal.StringToHGlobalAnsi(assetPath);

                ImGui.SetDragDropPayload(DragDropPayloadTypes.PrefabPath, stringPointer,
                    (uint)(sizeof(char) * assetPath.Length));

                //string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
                TofuImGui.ImageTexture2DArray(_fileIcon, _iconSize);

                Marshal.FreeHGlobal(stringPointer);

                ImGui.EndDragDropSource();
            }
        }

        if (ImGui.IsItemHovered() &&
            Tofu.MouseInput.ButtonReleased(MouseButtons.Left)) // released in case we want to drag and drop somemthing
        {
            if (fileType is FileType.Material)
            {
                EditorPanelInspector.I.OnMaterialSelected(assetPath);
            }

            if (fileType is FileType.Model)
            {
                string pathOfImportParametersOfSourceAssetFile =
                    AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetPath);
                Object importParameters =
                    Serializer.ReadFileJSON<AssetImportParameters_Model>(pathOfImportParametersOfSourceAssetFile);


                if (importParameters != null)
                {
                    EditorPanelInspector.I.SelectInspectable(importParameters,
                        anyValueChanged: (fieldName) =>
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

            if (fileType is FileType.Texture)
            {
                string pathOfImportParametersOfSourceAssetFile =
                    AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetPath);
                Object importParameters =
                    Serializer.ReadFileJSON<AssetImportParameters_Texture>(pathOfImportParametersOfSourceAssetFile);


                if (importParameters != null)
                {
                    EditorPanelInspector.I.SelectInspectable(importParameters,
                        anyValueChanged: (fieldName) =>
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

            if (fileType is FileType.Prefab)
            {
                GameObject go = Tofu.SceneSerializer.LoadPrefab(assetPath);
                // todo
                // EditorPanelHierarchy.I.SelectGameObject(go.Id);
            }

            if (fileType is FileType.Scene)
            {
                Tofu.SceneManager.LoadScene(assetPath);
            }

            if (fileType is FileType.Script or FileType.Text)
            {
                Tofu.UserCodeEditorOpener.OpenFile(assetPath, 0, 0);
            }
        }


        int maxCharsLimit = 15;

        // var text = assetName.Substring(0, Math.Clamp(assetName.Length, 0, maxCharsLimit));
        string text = assetName;
        Vector2 textSize = ImGui.CalcTextSize(text);

        if (textSize.X < _iconSize.X)
        {
            float spaceLeft = textSize.X - _iconSize.X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - spaceLeft / 2f);
        }
        else
        {
            if (ImGui.IsItemHovered())
            {
                text = assetName.Substring((int)Mathf.ClampMin(assetName.Length - 2 - maxCharsLimit, 0));
            }
            else
            {
                text = assetName.Substring(0, Math.Clamp(assetName.Length, 0, maxCharsLimit));
            }
        }


        ImGui.Text(text);

        if (assetName.Length > maxCharsLimit)
        {
            // ImGui.Text(assetName.Substring(maxCharsLimit, assetName.Length - maxCharsLimit));
        }

        ImGui.EndGroup();


        if (fileType is FileType.Model)
        {
            if (_expandedAssets.Contains(assetIndex))
            {
                Asset_Model assetModel = Tofu.AssetLoadManager.Get<Asset_Model>(assetPath);
                for (int meshIndex = 0; meshIndex < assetModel.PathsToMeshAssets.Count; meshIndex++)
                {
                    // ImGui.SameLine();
                    _subAssetsDrawnCount++;
                    DrawAsset(assetIndex, assetModel.PathsToMeshAssets[meshIndex]);
                }
            }
        }

        ImGui.EndGroup();
        if (ImGui.IsItemHovered() && _hoveredAssetIndex != assetIndex)
        {
            _hoveredAssetIndex = assetIndex;
        }

        if (fileType is not FileType.Mesh && isDirectory == false)
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