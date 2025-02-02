using ImGuiNET;

namespace Tofu3D;

public abstract class EditorPanel
{
    private int _currentId;

    internal virtual bool Active { get; set; }= true;

    internal bool IsPanelHovered;
    public int WindowWidth;
    public virtual string Name => "";

    public Vector2 Size = new Vector2(Tofu.Window.ClientSize.X / 10f, Tofu.Window.ClientSize.Y / 10f);

    public virtual Vector2 Position => new Vector2(0, Tofu.Window.ClientSize.Y);
    public virtual Vector2 Pivot => new Vector2(0, 1);
    public virtual ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.None;
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
    }

    public virtual void Update()
    {
    }

    public virtual void Draw()
    {
    }

    public void BeginWindowDefault()
    {
        ImGui.SetNextWindowSize(Size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | AdditionalWindowFlags);
        IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);
        Size = ImGui.GetWindowSize();
    }

    public void EndWindow()
    {
        // IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);

        ImGui.End();
    }
}