using ImGuiNET;

namespace Tofu3D;

public class EditorPanelTextureViewer : EditorPanel
{
    public override Vector2 Position => new(0, 0);
    public override Vector2 Pivot => new(1, 1);

    public override ImGuiWindowFlags AdditionalWindowFlags =>
        ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar;

    public static EditorPanelTextureViewer I { get; private set; }
    public override string Name => "|Framebuffer Visualiser|";

    private static List<TextureViewerTextureData> _textures = new List<TextureViewerTextureData>();

    public override void Init()
    {
        I = this;
    }

    public static void AddTexture(TextureViewerTextureData data)
    {
        if (_textures.Contains(data))
        {
            return;
        }

        _textures.Add(data);
    }

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        if (Global.EditorAttached == false)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                                 ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.Modal;
        ImGui.SetNextWindowSize(Size, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);
        ImGui.Begin(Name, flags);

        ImGui.SetCursorPos(new Vector2(0, 0));

        float yAvailable = ImGui.GetContentRegionAvail().Y;
        float yAvailablePerOneTexture = yAvailable / (float)_textures.Count;
        foreach (TextureViewerTextureData textureData in _textures)
        {
            var ratio = textureData.Texture.Size.Y /
                        textureData.Texture.Size.X;

            Vector2 size = Vector2.Zero;

            size.X = Mathf.ClampMax(textureData.Texture.Size.X, ImGui.GetContentRegionAvail().X);
            size.Y = size.X * ratio;
            size.Y = Mathf.ClampMax(size.Y, yAvailablePerOneTexture);
            size.X = size.Y / ratio;

            Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
            Vector2 textureEnd = cursorScreenPos + size;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(
                cursorScreenPos,
                textureEnd,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f)) // Background color (RGBA)
            );

            ImGui.Image(textureData.Texture.TextureId,
                size,
                new Vector2(0, 1), new Vector2(1, 0));
            ImGui.NewLine();
        }

        ImGui.PopStyleVar(2);
    }

    public override void Update()
    {
    }
}