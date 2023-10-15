using ImGuiNET;

namespace Tofu3D;

public class EditorPanel
{
	public virtual string Name => "";

	public virtual Vector2 Size => new Vector2(Tofu.Window.ClientSize.X - 1600, Tofu.Window.ClientSize.Y - Tofu.Editor.SceneViewSize.Y + 1);
	public virtual Vector2 Position => new Vector2(0, Tofu.Window.ClientSize.Y);
	public virtual Vector2 Pivot => new Vector2(0, 1);
	public virtual ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.None;

	internal bool Active = true;

	internal bool IsPanelHovered;

	int _currentId;
	public int WindowWidth;

	internal void ResetId()
	{
		_currentId = 0;
	}

	internal void PushNextId()
	{
		ImGui.PushID(int_id: _currentId++);
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