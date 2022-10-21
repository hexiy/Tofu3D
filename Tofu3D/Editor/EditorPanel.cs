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
		ImGui.PushID(_currentId++);
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