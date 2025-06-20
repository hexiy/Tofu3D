using ImGuiNET;

namespace TofuEngine;

public abstract class EditorPanel
{
    private int _currentId;

    internal bool IsVisible { get; set; } = true;

    internal bool IsPanelHovered;
    internal bool IsPanelFocused;
    public int WindowWidth;
    public virtual string Name => "";

    public Vector2 Size = new Vector2(Tofu.Window.ClientSize.X / 10f, Tofu.Window.ClientSize.Y / 10f);

    public virtual Vector2 Position { get; set; } = new Vector2(0, Tofu.Window.ClientSize.Y);
    public virtual Vector2 Pivot => new Vector2(0, 1);
    public virtual ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.None;
    public virtual bool CreatesWindow => true;
    public bool IsFullscreen { get; set; }

    internal void ResetId()
    {
        _currentId = 0;
    }

    internal void PushId(int id)
    {
        ImGui.PushID(id);
    }

    internal void PushNextId()
    {
        ImGui.PushID(_currentId++);
    }

    internal void PopAllIds()
    {
        for (int i = 0; i < _currentId; i++)
        {
            ImGui.PopID();
        }

        ResetId();
    }

    internal void PopId()
    {
        ImGui.PopID();
    }

    public virtual void Init()
    {
        EditorViewManager.AnyPanelFocused += OnAnyPanelFocused;
    }

    private void OnAnyPanelFocused(EditorPanel panel)
    {
        IsPanelFocused = panel == this;
    }

    public virtual void Update()
    {
    }

    protected abstract void ExecuteImGuiDrawCommands();

    public void Draw()
    {
        if (CreatesWindow)
        {
            BeforeWindowCreated();

            BeginWindowDefault();

            if (IsVisible == false)
            {
                ImGui.End();
                AfterWindowEnded();
                return;
            }

            DoChecksAfterWindowCreated();
        }

        ExecuteImGuiDrawCommands();

        if (CreatesWindow)
        {
            AfterWindowEnded();
        }
    }

    // private bool _a = false;

    protected virtual void BeforeWindowCreated()
    {
        // if (IsPanelFocused)
        // {
        //     // ImGui.PushStyleColor(ImGuiCol.WindowBg, Color.Red.ToVector4());
        //     _a = true;
        // }
        // else
        // {
        //     _a = false;
        // }
    }

    protected virtual void AfterWindowEnded()
    {
        // if (_a)
        // {
        //     ImGui.PopStyleColor();
        // }
    }

    protected virtual void OnVisibilityChanged()
    {
    }

    private void BeginWindowDefault()
    {
        ImGui.SetNextWindowSize(Size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        bool visibleBefore = IsVisible;
        IsVisible = ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | AdditionalWindowFlags);

        if (visibleBefore != IsVisible)
        {
            OnVisibilityChanged();
        }

        // DoChecksAfterWindowCreated();
    }

    private void DoChecksAfterWindowCreated()
    {
        bool isPanelHoveredBefore = IsPanelHovered;
        IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            if (IsPanelFocused == false && IsPanelHovered == true)
            {
                EditorViewManager.AnyPanelFocused.Invoke(this);
            }

            IsPanelFocused = IsPanelHovered;
        }

        if (isPanelHoveredBefore == false && IsPanelHovered == true)
        {
            EditorViewManager.AnyPanelHovered.Invoke(this);
        }

        Size = ImGui.GetWindowSize() / Screen.ScaleI;
        Position = ImGui.GetWindowPos() / Screen.ScaleI;

        CheckForTabOptionsClick();
    }


    private void CheckForTabOptionsClick()
    {
        ImGui.OpenPopupOnItemClick("TabOptions", ImGuiPopupFlags.MouseButtonRight);

        if (ImGui.BeginPopup("TabOptions"))
        {
            // bool selected = ImGui.Selectable("aaaaaaa");
            bool closeTabClicked = ImGui.MenuItem("[x] Close tab");
            if (closeTabClicked)
            {
                Tofu.Editor.AfterDraw += () =>
                {
                    OnClosed();
                    IsVisible = false;
                    Tofu.Editor.CloseWindow(this);
                };
            }

            ImGui.Separator();

            bool layoutMenuOpened = ImGui.BeginMenu("Add tab");
            if (layoutMenuOpened)
            {
                bool sceneViewItemClicked = ImGui.MenuItem("Scene view");
                if (sceneViewItemClicked)
                {
                    Tofu.Editor.OpenWindow(new EditorPanelSceneView());
                }

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    protected virtual void OnClosed()
    {
    }

    protected void EndWindow()
    {
        // IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);

        ImGui.End();
    }
}