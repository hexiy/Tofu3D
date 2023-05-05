using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ImGuiNET;
using OpenTK.Windowing.Common;

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
	bool _wasMaxScrollLastFrame = true;

	private static LogCategoryFilter _currentLogCategoryFilter = LogCategoryFilter.All;
	string _searchFilter = "";

	public static void SetLogCategoryFilter(LogCategoryFilter logCategoryFilter)
	{
		_currentLogCategoryFilter = logCategoryFilter;
	}

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
			LogCategory randomCategory = LogCategory.Info;
			int rnd = Random.Range(0, 4);
			if (rnd == 0) randomCategory = LogCategory.Error;
			if (rnd == 1) randomCategory = LogCategory.Warning;
			if (rnd == 2) randomCategory = LogCategory.Info;
			if (rnd == 3) randomCategory = LogCategory.Timer;
			Debug.Log("yo!", logCategory: randomCategory);
		}

		// category filters
		LogCategoryFilter[] toggleableFilters = new[]
		                                        {
			                                        LogCategoryFilter.Info, LogCategoryFilter.Warning, LogCategoryFilter.Error, LogCategoryFilter.Timer
		                                        };
		foreach (LogCategoryFilter filter in toggleableFilters)
		{
			ImGui.SameLine();

			bool hasFlag = (_currentLogCategoryFilter & filter) == filter;
			// ImGui.RadioButton(filter.ToString(), hasFlag); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
			int textureId = EditorTextures.I.LogCategoryInfoIcon.TextureId;
			if (filter == LogCategoryFilter.Error)
			{
				textureId = EditorTextures.I.LogCategoryErrorIcon.TextureId;
			}

			if (filter == LogCategoryFilter.Warning)
			{
				textureId = EditorTextures.I.LogCategoryWarningIcon.TextureId;
			}

			if (filter == LogCategoryFilter.Timer)
			{
				textureId = EditorTextures.I.LogCategoryTimerIcon.TextureId;
			}

			ImGui.Image(textureId, new System.Numerics.Vector2(30, 30), new Vector2(0, 0), new Vector2(1, 1), hasFlag ? new Vector4(1, 1, 1, 1) : new Vector4(1, 1, 1, 0.3f)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
			// ImGui.ImageButton(textureId, new System.Numerics.Vector2(30, 30)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
			bool filterButtonClicked = ImGui.IsItemClicked();
			if (filterButtonClicked)
			{
				_currentLogCategoryFilter = _currentLogCategoryFilter;
				if (hasFlag)
				{
					_currentLogCategoryFilter &= ~ filter;
				}
				else
				{
					_currentLogCategoryFilter |= filter;
				}
			}
		}

		ImGui.SameLine();
		ImGui.InputTextWithHint("Search", "Search", ref _searchFilter, 100);
		_searchFilter = _searchFilter.ToLower();

		ImGui.BeginChildFrame(2, ImGui.GetContentRegionAvail() * new System.Numerics.Vector2(1, _selectedMessageIndex == -1 ? 1 : 0.7f));
		int logsCount = Debug.GetLogsRef().Count;
		int drawnLogsCounter = 0;
		for (int i = 0; i < Mathf.Min(logsCount, Debug.Limit - 1); i++)
		{
			LogEntry log = Debug.GetLogsRef()[i];

			if (_searchFilter.Length > 0 && log.Message.ToLower().Contains(_searchFilter) == false)
			{
				continue;
			}

			if (((LogCategory) _currentLogCategoryFilter & log.LogCategory) != log.LogCategory)
			{
				continue;
			}

			ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY() - 3));

			ImGui.Separator();

			// ImGui.Button("", size: new Vector2(ImGui.GetContentRegionAvail().X, 50));
			ImGui.Selectable("", _selectedMessageIndex == i, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 30));
			bool clicked = ImGui.IsItemClicked();
			if (clicked)
			{
				Tofu.I.Window.ClipboardString = log.Message;
				_selectedMessageIndex = _selectedMessageIndex == i ? -1 : i;
			}


			int textureId = EditorTextures.I.LogCategoryInfoIcon.TextureId;
			if (log.LogCategory == LogCategory.Error)
			{
				textureId = EditorTextures.I.LogCategoryErrorIcon.TextureId;
			}

			if (log.LogCategory == LogCategory.Warning)
			{
				textureId = EditorTextures.I.LogCategoryWarningIcon.TextureId;
			}

			if (log.LogCategory == LogCategory.Timer)
			{
				textureId = EditorTextures.I.LogCategoryTimerIcon.TextureId;
			}

			ImGui.SameLine();

			// ImGui.SetCursorPos(new Vector2(10, 25 + drawnLogsCounter * 50));
			ImGui.SetCursorPos(new Vector2(10, ImGui.GetCursorPosY()));
			ImGui.Image(textureId, new System.Numerics.Vector2(25, 25)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
			ImGui.SameLine();
			// ImGui.SetCursorPos(new Vector2(100, 25 + drawnLogsCounter * 50));
			// ImGui.SetCursorPos(new Vector2(0, 25 + drawnLogsCounter * 50));

			Color color = GetLogCategoryTextColor(log.LogCategory);
			ImGui.TextColored(color.ToVector4(), log.Time);
			// ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Time);
			ImGui.SameLine();
			ImGui.TextWrapped(log.Message);
			// ImGui.Button(log.Substring(log.IndexOf("]") + 1));
			drawnLogsCounter++;
		}

		if (logsCount > _lastFrameMessagesCount && _wasMaxScrollLastFrame)
		{
			// ImGui.SetScrollY(ImGui.GetScrollMaxY());
			ImGui.SetScrollY(logsCount * 35);

		}

		_lastFrameMessagesCount = logsCount;
		_wasMaxScrollLastFrame = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

		ImGui.EndChildFrame();

		if (_selectedMessageIndex >= Debug.GetLogsRef().Count)
		{
			_selectedMessageIndex = -1;
		}

		if (_selectedMessageIndex != -1)
		{
			ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y * 0.72f);
			ImGui.Separator();
			Vector4 cBeige = new(1f, 0.96f, 0.90f, 1.00f);

			Vector4 cBeigeMid = new(0.97f, 0.94f, 0.88f, 1f); // greenish
			ImGui.PushStyleColor(ImGuiCol.FrameBg, cBeigeMid);
			ImGui.PushStyleColor(ImGuiCol.HeaderHovered, cBeige);
			// ImGui.BeginChildFrame(1, ImGui.GetContentRegionMax());
			ImGui.BeginChildFrame(1, ImGui.GetContentRegionAvail());
			LogEntry log = Debug.GetLogsRef()[_selectedMessageIndex];
			Color color = GetLogCategoryTextColor(log.LogCategory);
			ImGui.TextColored(color.ToVector4(), log.Time);
			ImGui.SameLine();
			ImGui.TextWrapped(log.Message);
			// ImGui.Separator();
			for (int i = 0; i < log.StackTrace.Frames.Length; i++)
			{
				ImGui.Selectable(log.StackTrace.Frames[i].Text);
				// ImGui.TextWrapped(log.StackTrace.Frames[i].Text);
				bool clicked = ImGui.IsItemClicked();
				ImGui.SameLine();
				ImGui.TextColored(Color.ForestGreen.ToVector4(), $"   /{log.StackTrace.Frames[i].FileShort}({log.StackTrace.Frames[i].Line},{log.StackTrace.Frames[i].Column})");
				clicked = clicked || ImGui.IsItemClicked();
				if (clicked)
				{
					RiderIDE.OpenStackTrace(log.StackTrace.Frames[i]);
				}
			}
			// ImGui.TextWrapped(log.StackTrace);

			ImGui.EndChildFrame();

			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
		}

		if (logsCount > 0)
		{
			ImGui.Separator();
		}
		//ResetID();


		ImGui.End();
	}

	private Color GetLogCategoryTextColor(LogCategory logCategory)
	{
		if (logCategory == LogCategory.Info)
		{
			return EditorColors.LogCategoryInfo;
		}

		if (logCategory == LogCategory.Error)
		{
			return EditorColors.LogCategoryError;
		}

		if (logCategory == LogCategory.Warning)
		{
			return EditorColors.LogCategoryWarning;
		}

		if (logCategory == LogCategory.Timer)
		{
			return EditorColors.LogCategoryTimer;
		}

		return EditorColors.LogCategoryInfo;
	}

	public void Update()
	{
	}
}