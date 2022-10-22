using ImGuiNET;

namespace Tofu3D;

public class EditorPanel
{
	internal bool Active = true;

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
}