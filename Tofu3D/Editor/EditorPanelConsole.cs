using ImGuiNET;

namespace Tofu3D;

public class EditorPanelConsole : EditorPanel
{
	public override Vector2 Size => new Vector2(800, Tofu.I.Window.ClientSize.Y - Editor.SceneViewSize.Y + 1);
	public override Vector2 Position => new Vector2(Tofu.I.Window.ClientSize.X - 800, Tofu.I.Window.ClientSize.Y);
	public override Vector2 Pivot => new Vector2(1, 1);

	public static EditorPanelConsole I { get; private set; }
	public override string Name => "Console";

	public override void Init()
	{
		I = this;
	}

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		SetWindow();

		if (ImGui.Button("Clear"))
		{
			Debug.ClearLogs();
		}

		ImGui.SameLine();
		Vector4 activeColor = Color.ForestGreen.ToVector4();
		Vector4 inactiveColor = ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled];
		ImGui.PushStyleColor(ImGuiCol.Text, Debug.Paused ? activeColor : inactiveColor);
		bool pauseBtnClicked = ImGui.Button("Pause");
		if (pauseBtnClicked)
		{
			Debug.Paused = !Debug.Paused;
		}

		ImGui.PopStyleColor();

		int logsCount = Debug.GetLogsRef().Count;
		for (int i = 0; i < Mathf.Min(logsCount, Debug.LogLimit - 1); i++)
		{
			ImGui.Separator();

			string log = Debug.GetLogsRef()[logsCount - i - 1];
			ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Substring(0, log.IndexOf("]") + 1));
			ImGui.SameLine();

			ImGui.TextWrapped(log.Substring(log.IndexOf("]") + 1));
		}

		if (logsCount > 0)
		{
			ImGui.Separator();
		}
		//ResetID();

		ImGui.End();
	}

	public void Update()
	{
	}
}