using ImGuiNET;

namespace Tofu3D;

public class EditorPanelConsole : EditorPanel
{
	public static EditorPanelConsole I { get; private set; }

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

		ImGui.SetNextWindowSize(new Vector2(800, Window.I.ClientSize.Y - Editor.SceneViewSize.Y + 1), ImGuiCond.FirstUseEver);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X - 800, Window.I.ClientSize.Y), ImGuiCond.FirstUseEver, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Console", Editor.ImGuiDefaultWindowFlags);

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