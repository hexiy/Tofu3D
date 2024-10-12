using ImGuiNET;

namespace Tofu3D;

public class EditorPanelConsole : EditorPanel
{
    private static LogCategoryFilter _currentLogCategoryFilter = LogCategoryFilter.All;
    private int _lastFrameMessagesCount = -1;
    private string _searchFilter = "";
    private int _selectedMessageIndex = -1;
    private bool _wasMaxScrollLastFrame = true;
    public override Vector2 Size => new(800, Tofu.Window.ClientSize.Y - Tofu.Editor.SceneViewSize.Y + 1);
    public override Vector2 Position => new(Tofu.Window.ClientSize.X - 800, Tofu.Window.ClientSize.Y);
    public override Vector2 Pivot => new(1, 1);

    public override ImGuiWindowFlags AdditionalWindowFlags =>
        ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar;

    public static EditorPanelConsole I { get; private set; }
    public override string Name => "Console";

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
        var activeColor = Color.ForestGreen.ToVector4();
        Vector4 inactiveColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];
        ImGui.PushStyleColor(ImGuiCol.Text, Debug.Paused ? activeColor : inactiveColor);
        var pauseBtnClicked = ImGui.Button("Pause");
        if (pauseBtnClicked)
        {
            Debug.Paused = !Debug.Paused;
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();

        var testMessageBtnClicked = ImGui.Button("Test message");
        if (testMessageBtnClicked)
        {
            var randomCategory = LogCategory.Info;
            var rnd = Random.Range(0, 4);
            if (rnd == 0)
            {
                randomCategory = LogCategory.Error;
            }

            if (rnd == 1)
            {
                randomCategory = LogCategory.Warning;
            }

            if (rnd == 2)
            {
                randomCategory = LogCategory.Info;
            }

            if (rnd == 3)
            {
                randomCategory = LogCategory.Timer;
            }

            Debug.Log("yo!", randomCategory);
        }

        // category filters
        var toggleableFilters = new[]
        {
            LogCategoryFilter.Info, LogCategoryFilter.Warning, LogCategoryFilter.Error, LogCategoryFilter.Timer
        };
        foreach (var filter in toggleableFilters)
        {
            ImGui.SameLine();

            var hasFlag = (_currentLogCategoryFilter & filter) == filter;
            // ImGui.RadioButton(filter.ToString(), hasFlag); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
            var textureId = Tofu.Editor.EditorTextures.LogCategoryInfoIcon.TextureId;
            if (filter == LogCategoryFilter.Error)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryErrorIcon.TextureId;
            }

            if (filter == LogCategoryFilter.Warning)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryWarningIcon.TextureId;
            }

            if (filter == LogCategoryFilter.Timer)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryTimerIcon.TextureId;
            }

            ImGui.Image(textureId, new System.Numerics.Vector2(30, 30), new Vector2(0, 0), new Vector2(1, 1),
                hasFlag
                    ? new Vector4(1, 1, 1, 1)
                    : new Vector4(1, 1, 1, 0.3f)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
            // ImGui.ImageButton(textureId, new System.Numerics.Vector2(30, 30)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
            var filterButtonClicked = ImGui.IsItemClicked();
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

        ImGui.BeginChildFrame(2,
            ImGui.GetContentRegionAvail() * new System.Numerics.Vector2(1, _selectedMessageIndex == -1 ? 1 : 0.7f));
        var logsCount = Debug.GetLogsRef().Count;
        var drawnLogsCounter = 0;
        for (var i = 0; i < Mathf.Min(logsCount, Debug.Limit - 1); i++)
        {
            var log = Debug.GetLogsRef()[i];

            if (_searchFilter.Length > 0 && log.Message.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }

            if (((LogCategory)_currentLogCategoryFilter & log.LogCategory) != log.LogCategory)
            {
                continue;
            }

            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY() - 3));

            ImGui.Separator();

            // ImGui.Button("", size: new Vector2(ImGui.GetContentRegionAvail().X, 50));
            ImGui.Selectable("", _selectedMessageIndex == i, ImGuiSelectableFlags.None,
                new Vector2(ImGui.GetContentRegionAvail().X, 30));
            var clicked = ImGui.IsItemClicked();
            if (clicked)
            {
                Tofu.Window.ClipboardString = log.Message;
                _selectedMessageIndex = _selectedMessageIndex == i ? -1 : i;
            }


            var textureId = Tofu.Editor.EditorTextures.LogCategoryInfoIcon.TextureId;
            if (log.LogCategory == LogCategory.Error)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryErrorIcon.TextureId;
            }

            if (log.LogCategory == LogCategory.Warning)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryWarningIcon.TextureId;
            }

            if (log.LogCategory == LogCategory.Timer)
            {
                textureId = Tofu.Editor.EditorTextures.LogCategoryTimerIcon.TextureId;
            }

            ImGui.SameLine();

            // ImGui.SetCursorPos(new Vector2(10, 25 + drawnLogsCounter * 50));
            ImGui.SetCursorPos(new Vector2(10, ImGui.GetCursorPosY()));
            ImGui.Image(textureId,
                new System.Numerics.Vector2(25, 25)); //|| _currentLogCategoryFilter.HasFlag(LogCategoryFilter.All));
            ImGui.SameLine();
            // ImGui.SetCursorPos(new Vector2(100, 25 + drawnLogsCounter * 50));
            // ImGui.SetCursorPos(new Vector2(0, 25 + drawnLogsCounter * 50));

            var color = GetLogCategoryTextColor(log.LogCategory);
            ImGui.TextColored(color.ToVector4(), log.Time);
            // ImGui.TextColored(new Vector4(0.74f, 0.33f, 0.16f, 1), log.Time);
            ImGui.SameLine();
            ImGui.TextWrapped(log.Message);
            // ImGui.Button(log.Substring(log.IndexOf("]") + 1));
            drawnLogsCounter++;
        }

        if (logsCount > _lastFrameMessagesCount && _wasMaxScrollLastFrame)
            // ImGui.SetScrollY(ImGui.GetScrollMaxY());
        {
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
            var log = Debug.GetLogsRef()[_selectedMessageIndex];
            var color = GetLogCategoryTextColor(log.LogCategory);
            ImGui.TextColored(color.ToVector4(), log.Time);
            ImGui.SameLine();
            ImGui.TextWrapped(log.Message);
            // ImGui.Separator();
            for (var i = 0; i < log.StackTrace.Frames.Length; i++)
            {
                ImGui.Selectable(log.StackTrace.Frames[i].Text);
                // ImGui.TextWrapped(log.StackTrace.Frames[i].Text);
                var clicked = ImGui.IsItemClicked();
                ImGui.SameLine();
                ImGui.TextColored(Color.ForestGreen.ToVector4(),
                    $"   /{log.StackTrace.Frames[i].FileShort}({log.StackTrace.Frames[i].Line},{log.StackTrace.Frames[i].Column})");
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

    public override void Update()
    {
    }
}