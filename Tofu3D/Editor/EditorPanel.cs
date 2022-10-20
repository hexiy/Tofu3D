using ImGuiNET;

namespace Tofu3D;

public class EditorPanel
{
	internal bool active = true;
	public int windowWidth;

	private int currentID;

	internal void ResetID()
	{
		currentID = 0;
	}

	internal void PushNextID()
	{
		ImGui.PushID(currentID++);
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