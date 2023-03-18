using ImGuiNET;

namespace Tofu3D;

public class EditorPanelConsole : EditorPanel
{
	public override Vector2 Size => new Vector2(800, Tofu.I.Window.ClientSize.Y - Editor.SceneViewSize.Y + 1);
	public override Vector2 Position => new Vector2(Tofu.I.Window.ClientSize.X - 800, Tofu.I.Window.ClientSize.Y);
	public override Vector2 Pivot => new Vector2(1, 1);
	public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar;

	public static EditorPanelConsole I { get; private set; }
	public override string Name => "Console";
	int _selectedMessageIndex = -1;
	int _lastFrameMessagesCount = -1;
	bool _wasMaxScrollLastFrame = false;

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

		ImGui.SameLine();

		bool testMessageBtnClicked = ImGui.Button("Test message");
		if (testMessageBtnClicked)
		{
			Debug.Log("yo!");
		}

		ImGui.BeginChildFrame(2, ImGui.GetContentRegionAvail() * new System.Numerics.Vector2(1, _selectedMessageIndex == -1 ? 1 : 0.7f));
		int logsCount = Debug.GetLogsRef().Count;
		for (int i = 0; i < Mathf.Min(logsCount, Debug.LogLimit - 1); i++)
		{
			ImGui.Separator();

			// ImGui.Button("", size: new Vector2(ImGui.GetContentRegionAvail().X, 50));
			ImGui.Selectable("", _selectedMessageIndex == i, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 50));
			bool clicked = ImGui.IsItemClicked();
			if (clicked)
			{
				_selectedMessageIndex = _selectedMessageIndex == i ? -1 : i;
			}

			ImGui.SetCursorPos(new Vector2(0, 25 + i * 50));

			LogEntry log = Debug.GetLogsRef()[i];
			ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Time);
			ImGui.SameLine();
			ImGui.TextWrapped(log.Message);
			// ImGui.Button(log.Substring(log.IndexOf("]") + 1));
		}

		if (logsCount > _lastFrameMessagesCount && _wasMaxScrollLastFrame)
		{
			// ImGui.SetScrollY(ImGui.GetScrollMaxY());
			ImGui.SetScrollY(int.MaxValue);
		}

		_lastFrameMessagesCount = logsCount;
		_wasMaxScrollLastFrame = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

		ImGui.EndChildFrame();

		if (_selectedMessageIndex != -1)
		{
			ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y * 0.72f);
			ImGui.Separator();
			ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Beige.ToVector4());
			// ImGui.BeginChildFrame(1, ImGui.GetContentRegionMax());
			ImGui.BeginChildFrame(1, ImGui.GetContentRegionAvail());
			LogEntry log = Debug.GetLogsRef()[_selectedMessageIndex];
			ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Time);
			ImGui.SameLine();
			ImGui.TextWrapped(log.Message);
			ImGui.Separator();
			ImGui.TextWrapped(log.StackTrace);

			ImGui.EndChildFrame();

			ImGui.PopStyleColor();
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