using ImGuiNET;

namespace TofuEngine;

public class EditorPanelToolbar : EditorPanel
{
    public static EditorPanelToolbar I { get; private set; }
    public override string Name => "Toolbar";
    public static int Height => 64;


    public override void Init()
    {
        I = this;
    }

    public override void Draw()
    {
        if (IsActive == false)
        {
            return;
        }

        if (Global.EditorAttached)
        {
            Vector2 barHeightVector = new Vector2(0, Height);

            ImGui.SetNextWindowSize(new Vector2(Tofu.Window.Size.X * Screen.Scale, Height), ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, 30), ImGuiCond.Always, new Vector2(0, 0));


            ImGuiWindowFlags flags = Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse |
                                     ImGuiWindowFlags.NoDecoration |
                                     ImGuiWindowFlags.NoMove |
                                     ImGuiWindowFlags.NoTitleBar |
                                     ImGuiWindowFlags.NoResize |
                                     ImGuiWindowFlags.NoDocking
                ;

            // ImGui.PushStyleColor(ImGuiCol.WindowBg, Color.Red.ToVector4());
            // ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar);
            ImGui.Begin(Name, flags);

            Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
            Vector4 inactiveColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

            ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);


            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2f - 25f);


            IntPtr textureId = ImGuiController.EncodeTextureArrayId(
                Tofu.Editor.EditorTextures.PlayIcon.AtlasGLTextureArrayId,
                Tofu.Editor.EditorTextures.PlayIcon.IndexInAtlasTextureArray);
            Vector4 textureBoundsInAtlas = Tofu.Editor.EditorTextures.PlayIcon.BoundingBoxInAtlas;

            ImGui.Image(textureId, new System.Numerics.Vector2(50, 50), textureBoundsInAtlas.XW,
                textureBoundsInAtlas.ZY,
                new Vector4(1, 1, 1, 1));

            // ImGui.ImageButton(textureId, new System.Numerics.Vector2(50, 50), textureBoundsInAtlas.XW,
            //     textureBoundsInAtlas.ZY, 0,
            //     new Vector4(1, 1, 1, 1));

            bool buttonClicked = ImGui.IsItemClicked();

            ImGui.PopStyleColor();

            if (buttonClicked)
            {
                if (Global.GameRunning)
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