using ImGuiNET;

namespace TofuEngine;

public class EditorPanelToolbar : EditorPanel
{
    public static EditorPanelToolbar I { get; private set; }
    public override string Name => "Toolbar";
    public static int Height => 40;

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoScrollbar |
                                                              ImGuiWindowFlags.NoScrollWithMouse |
                                                              ImGuiWindowFlags.NoDecoration |
                                                              ImGuiWindowFlags.NoMove |
                                                              ImGuiWindowFlags.NoTitleBar |
                                                              ImGuiWindowFlags.NoResize |
                                                              ImGuiWindowFlags.NoDocking;


    public override void Init()
    {
        I = this;
    }

    protected override void BeforeWindowCreated()
    {
        ImGui.SetNextWindowSize(new Vector2(Tofu.Window.Size.X * Screen.Scale, Height), ImGuiCond.Always);
        ImGui.SetNextWindowPos(new Vector2(0, 30), ImGuiCond.Always, new Vector2(0, 0));

        base.BeforeWindowCreated();
    }

    protected override void ExecuteImGuiDrawCommands()
    {
        if (Global.EditorAttached)
        {
            Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
            Vector4 inactiveColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

            TofuImGui.PushStyleColor(ImGuiCol.Text, Playmode.GameRunning ? activeColor : inactiveColor);


            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2f - 25f);

            RuntimeTexture icon = Playmode.GameRunning
                ? Tofu.Editor.EditorTextures.PauseIcon
                : Tofu.Editor.EditorTextures.PlayIcon;

            ImGui.SetCursorPosY(Height / 2f - 30f / 2f);

            TofuImGui.ImageTexture2DArray(icon, new Vector2(30, 30));

            bool buttonClicked = ImGui.IsItemClicked();

            TofuImGui.PopStyleColor();

            if (buttonClicked)
            {
                if (Playmode.GameRunning)
                {
                    Playmode.PlayMode_Stop();
                }
                else
                {
                    Playmode.PlayMode_Start();
                }
            }

            ImGui.SameLine();

            ImGui.End();
        }
    }

    public override void Update()
    {
    }
}