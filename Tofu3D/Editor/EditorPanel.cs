using ImGuiNET;

namespace Tofu3D;

public class EditorPanel
{
    private int _currentId;

    internal bool Active = true;

    internal bool IsPanelHovered;
    public int WindowWidth;
    public virtual string Name => "";

    public virtual Vector2 Size => new(Tofu.Window.ClientSize.X - 1600,
        Tofu.Window.ClientSize.Y - Tofu.Editor.SceneViewSize.Y + 1);

    public virtual Vector2 Position => new(0, Tofu.Window.ClientSize.Y);
    public virtual Vector2 Pivot => new(0, 1);
    public virtual ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.None;

    internal void ResetId()
    {
        _currentId = 0;
    }

    internal void PushNextId()
    {
        ImGui.PushID(_currentId++);
    }

    internal void PushNextId(string id)
    {
        ImGui.PushID(id);
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

    public void SetWindow()
    {
        ImGui.SetNextWindowSize(Size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | AdditionalWindowFlags);
    }

    public void EndWindow()
    {
        IsPanelHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly);

        ImGui.End();
    }
}