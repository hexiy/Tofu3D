using System.Collections;
using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class EditorPanelEditorSettings : EditorPanel, IHasInspector, IEditorWindow
{
    private Inspector _inspector;

    public bool IsOpened { get; set; }

    public override Vector2 Position => Screen.Center;
    public override Vector2 Pivot => Vector2.Half;

    public override string Name => "Editor Settings";

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoDocking;


    // public static EditorPanelEditorSettings I { get; private set; }
    private EditorPanelSideBar _sideBar;
    private Vector2 InspectorSize => Size - new Vector2(_sidebarWidth, 0);
    private const float _sidebarWidth = 250;

    public override void Init()
    {
        // I = this;

        _inspector = new Inspector(drawInspectableHeader: false);
        _inspector.FieldChangedByUser += OnAnyFieldChangedByUser;

        _sideBar = new EditorPanelSideBar([
            new EditorPanelSideBarButtonDefinition("General", Tofu.EditorSettingsAll.EditorSettingsGeneral),
            new EditorPanelSideBarButtonDefinition("Code Editor", Tofu.EditorSettingsAll.EditorSettingsCodeEditor),
            new EditorPanelSideBarButtonDefinition("Scene View", Tofu.EditorSettingsAll.EditorSettingsSceneView),
            new EditorPanelSideBarButtonDefinition("Analysis", null),
            new EditorPanelSideBarButtonDefinition("Assets", null),
            new EditorPanelSideBarButtonDefinition("Graphics", null),
            new EditorPanelSideBarButtonDefinition("Cache", null),
        ]);
        _sideBar.SelectedItemChanged += OnSidebarSelectedItemChanged;
        SelectInspectable(Tofu.EditorSettingsAll.EditorSettingsGeneral);
    }

    private void OnSidebarSelectedItemChanged(int itemIndex)
    {
        SelectInspectable(_sideBar.Items[itemIndex].Inspectable);
    }

    public override void Update()
    {
        _inspector.Update(InspectorSize);
        // _inspector.ContentMaxWidth = Size.Xi - (int)ImGui.GetStyle().WindowPadding.X;

        if (KeyboardInput.WasKeyJustPressed(Keys.Escape))
        {
            if (Tofu.EditorWindowsManager.IsInFront(this))
            {
                Tofu.Editor.AfterDraw +=
                    () => Tofu.EditorWindowsManager.CloseWindow(this);
            }
        }
    }

    public void SelectInspectable(object inspectable, Action<string>? anyValueChanged = null)
    {
        _inspector.SelectInspectable(inspectable, anyValueChanged);
    }


    public void SelectInspectables(IList inspectables)
    {
        _inspector.SelectInspectables(inspectables);
    }

    public override void Draw()
    {
        BeginWindow();
        ResetId();
        ImGui.SetScrollX(0);
        // _padding = (int)ImGui.GetStyle().WindowPadding.X;

        // using TofuImGuiSetWindowPaddingGuard windowPaddingGuard =
        // TofuImGui.SetTemporaryWindowPaddingForCurrentScope(TofuImGui.DefaultWindowPadding + new Vector2(50));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0);


        float topY = ImGui.GetCursorPosY();
        _sideBar.Draw(height: Size.Y);

        Vector2 pos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(_sidebarWidth, topY));

        ImGui.SetNextWindowSize(InspectorSize);
        ImGui.Begin("main", ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);

        if (_inspector.HasInspectableData)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);

            DrawInspectables(_inspector.CurrentInspectableDatas);

            ImGui.PopStyleVar(1);
        }

        ImGui.PopStyleVar(2); // Restore all styles
        PopAllIds();
        ImGui.End();
        ImGui.End();
    }

    private void BeginWindow()
    {
        bool imguiOpened = true;

        Vector2 size = Screen.Size / 2;
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(Vector2.One * 200, Vector2.One * 99999);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        ImGui.Begin(Name, ref imguiOpened, AdditionalWindowFlags | ImGuiWindowFlags.NoCollapse);
        IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);
        Size = ImGui.GetWindowSize();

        Toggle(imguiOpened);
    }


    private void DrawInspectables(List<InspectableData> inspectableDatas)
    {
        _inspector._editing = false;

        _inspector.Render(inspectableDatas);
    }


    public void OnAnyFieldChangedByUser(string fieldName)
    {
        if (fieldName == nameof(EditorSettingsGeneral.FontSize) ||
            fieldName == nameof(EditorSettingsGeneral.FontPathsCollection))
        {
            Tofu.ImGuiController.LoadFont(Tofu.EditorSettingsAll.EditorSettingsGeneral.FontSize,
                Tofu.EditorSettingsAll.EditorSettingsGeneral.FontPathsCollection.GetFirstSelectedItem());
        }

        if (fieldName == nameof(EditorSettingsCodeEditor.EditorArgs))
        {
            string editorPathOrName =
                Tofu.EditorSettingsAll.EditorSettingsCodeEditor.CodeEditorPaths.GetFirstSelectedItem();
            CodeEditorInfo editorInfo = Tofu.UserCodeEditorOpener.GetEditorInfoByName(editorPathOrName);
            editorInfo.ArgsTemplate = Tofu.EditorSettingsAll.EditorSettingsCodeEditor.EditorArgs;
        }

        if (fieldName == nameof(EditorSettingsCodeEditor.CodeEditorPaths))
        {
            string editorPathOrName =
                Tofu.EditorSettingsAll.EditorSettingsCodeEditor.CodeEditorPaths.GetFirstSelectedItem();
            CodeEditorInfo editorInfo = Tofu.UserCodeEditorOpener.GetEditorInfoByName(editorPathOrName);
            if (editorInfo == null) // doesnt exist yet
            {
                if (OperatingSystem.IsMacOS) // on macos user can select the app bundle which is just a directory
                {
                    editorPathOrName = StringExtensions.GetExecutablePathFromMacosAppBundlePath(editorPathOrName);
                }

                editorInfo = Tofu.UserCodeEditorOpener.AddEditor(editorPathOrName);

                if (editorInfo == null)
                {
                    Debug.LogError("Error adding editor");
                }
            }

            if (editorInfo != null)
            {
                Tofu.UserCodeEditorOpener.SetEditorToUse(editorInfo);
            }
        }

        if (fieldName == nameof(EditorSettingsGeneral.EditorTheme))
        {
            EditorThemeing.SetTheme(Tofu.EditorSettingsAll.EditorSettingsGeneral.EditorTheme);
        }

        Tofu.EditorSettingsAll.SaveData();
    }


    public void Toggle(bool tgl)
    {
        if (IsOpened == tgl)
        {
            return;
        }

        if (tgl == false)
        {
            Tofu.Editor.CloseWindow(this);
        }

        Tofu.EditorWindowsManager.ToggleWindow(this, tgl);
    }

    public void OnToggled(bool tgl)
    {
        IsOpened = tgl;
    }
}