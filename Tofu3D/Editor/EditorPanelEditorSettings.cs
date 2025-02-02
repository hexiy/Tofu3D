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

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoDocking |
                                                              ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.MenuBar;


    public static EditorPanelEditorSettings I { get; private set; }
    private int _padding = 0;

    public override void Init()
    {
        I = this;
        _inspector = new Inspector(drawInspectableHeader: false);
        _inspector.FieldChangedByUser += OnAnyFieldChangedByUser;

        SelectInspectable(Tofu.EditorSettingsAll.EditorSettingsGeneral);

        Active = false;
    }

    public override void Update()
    {
        _inspector.Update();
        _inspector.Size = Size;
        _inspector.ContentMaxWidth = Size.Xi - (int)ImGui.GetStyle().WindowPadding.X;

        if (KeyboardInput.WasKeyJustPressed(Keys.Escape))
        {
            if (Tofu.EditorWindowsManager.IsInFront(this))
            {
                Tofu.Editor.ActionQueue +=
                    () => Tofu.EditorWindowsManager.CloseWindow(this);
            }
        }
    }

    public void AddActionToActionQueue(Action action)
    {
        _inspector.AddActionToActionQueue(action);
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
        // BeginWindowDefault();
        ResetId();
        ImGui.SetScrollX(0);
        _padding = (int)ImGui.GetStyle().WindowPadding.X;
        // Ensure we disable horizontal scrolling and clip overflow
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0);

        if (_inspector.HasInspectableData)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);

            // if (ImGui.BeginChild("InspectorChild",
            // ImGui.GetContentRegionAvail() - new System.Numerics.Vector2(_padding, 0), false,
            // ImGuiWindowFlags.NoScrollbar))
            // {
            DrawInspectables(_inspector.CurrentInspectableDatas);
            // }

            ImGui.PopStyleVar(1);


            // properties with ShowIf and ShowIfNot attributes need to be reevaluated to show or not
            // if (Tofu.MouseInput.ButtonReleased(MouseButtons.Left))
            // {
            // 	UpdateCurrentComponentsCache();
            // }
        }

        ImGui.PopStyleVar(2); // Restore all styles

        ImGui.End();
    }

    private void BeginWindow()
    {
        Vector2 size = Screen.Size / 2;
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);
        ImGui.SetNextWindowPos(Position, ImGuiCond.Always, Pivot);
        ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | AdditionalWindowFlags);
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