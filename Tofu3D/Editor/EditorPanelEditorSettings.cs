using System.Collections;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelEditorSettings : EditorPanel, IHasInspector, IEditorWindow
{
    private Inspector _inspector;

    public bool IsOpened { get; set; }

    internal override bool Active
    {
        get => IsOpened;
        set => Toggle(value);
    }

    public override Vector2 Position => Screen.Center;
    public override Vector2 Pivot => Vector2.Half;

    public override string Name => "Editor Settings";

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoDocking;


    public static EditorPanelEditorSettings I { get; private set; }
    private EditorPanelSideBar _sideBar;

    public override void Init()
    {
        I = this;

        _inspector = new Inspector(drawInspectableHeader: false);
        _inspector.FieldChangedByUser += OnAnyFieldChangedByUser;

        _sideBar = new EditorPanelSideBar(["General", "Code Editor", "Scene", "Gizmos", "Assets", "Graphics", "Cache"]);
        _sideBar.SelectedItemChanged += OnSidebarSelectedItemChanged;
        SelectInspectable(Tofu.EditorSettingsAll.EditorSettingsGeneral);


        Active = false;
    }

    private void OnSidebarSelectedItemChanged(int itemIndex)
    {
        object[] inspectorInspectables = new object[]
        {
            Tofu.EditorSettingsAll.EditorSettingsGeneral,
            Tofu.EditorSettingsAll.EditorSettingsCodeEditor,
            null,
            null,
            null,
            null,
            null,
        };

        SelectInspectable(inspectorInspectables[itemIndex]);
    }

    public override void Update()
    {
        _inspector.Update(Size);
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

    public void Refresh()
    {
        _inspector.Refresh();
    }

    public void QueueRefresh()
    {
        throw new NotImplementedException();
    }

    public void QueueRefresh(InspectableData inspectableData)
    {
        throw new NotImplementedException();
    }

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        BeginWindow();
        ResetId();
        ImGui.SetScrollX(0);
        // _padding = (int)ImGui.GetStyle().WindowPadding.X;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0);


        float topY = ImGui.GetCursorPosY();
        _sideBar.Draw(height: Size.Y);

        Vector2 pos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(250, topY));

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
        Vector2 size = Screen.Size / 2;
        ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(Vector2.One * 200, Vector2.One * 99999);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        ImGui.Begin(Name, AdditionalWindowFlags | ImGuiWindowFlags.NoCollapse);
        IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);
        Size = ImGui.GetWindowSize();
    }


    private void DrawInspectables(List<InspectableData> inspectableDatas)
    {
        _inspector._editing = false;

        _inspector.Render(inspectableDatas);
    }


    public void OnAnyFieldChangedByUser(string fieldName)
    {
        if (fieldName == nameof(EditorSettingsGeneral.FontSize))
        {
            Tofu.ImGuiController.UpdateFontSize(Tofu.EditorSettingsAll.EditorSettingsGeneral.FontSize);
        }

        if (fieldName == nameof(EditorSettingsGeneral.EditorTheme))
        {
            EditorThemeing.SetTheme(Tofu.EditorSettingsAll.EditorSettingsGeneral.EditorTheme);
        }

        Tofu.EditorSettingsAll.SaveData();
    }


    public void Toggle(bool tgl)
    {
        Tofu.EditorWindowsManager.ToggleWindow(this, tgl);
    }

    public void OnToggled(bool tgl)
    {
        IsOpened = tgl;
    }
}