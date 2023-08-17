using ImGuiNET;

namespace Tofu3D;

public class EditorDialog
{
    public EditorDialogHandle Handle { get; private set; }
    private EditorDialogParams _dialogParams;
    public bool IsActive { get; private set; }

    public EditorDialog(int id)
    {
        Handle = new EditorDialogHandle(id);
    }

    public EditorDialogHandle Show(EditorDialogParams dialogParams)
    {
        _dialogParams = dialogParams;
        IsActive = true;
        return Handle;
    }

    public void Hide()
    {
        IsActive = false;
    }

    public void Update()
    {
    }

    public void Draw()
    {
        Vector2 bgPanelSize = Screen.Size * 1.3f;
        // ImGui.SetNextWindowSize(bgPanelSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(Screen.Center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.Begin("Dialog",
            ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoBackground);
        ImGui.Image(Tofu.I.Editor.EditorTextures.WhitePixel.TextureId, bgPanelSize, uv0: Vector2.Zero, Vector2.One,
            tint_col: new Vector4(0f, 0f, 0f, 0.9f));


        Vector2 panelSize = new Vector2(300, 200);
        ImGui.SetCursorScreenPos(Screen.Center - panelSize / 2);
        ImGui.Image(Tofu.I.Editor.EditorTextures.WhitePixel.TextureId, panelSize, uv0: Vector2.Zero, Vector2.One,
            tint_col: Color.White.ToVector4());
        bool hoveringPanel = ImGui.IsItemHovered();
        ImGui.SetCursorScreenPos(Screen.Center + new Vector2(0,-50));
        TofuGUI.Text(_dialogParams.message);

        int index = 0;
        foreach (EditorDialogButtonDefinition button in _dialogParams.buttons)
        {
            ImGui.SetCursorScreenPos(Screen.Center + new Vector2(0,index*TofuGUI.ButtonSize.Y+(index*20)));
            index++;
            bool btnClicked =TofuGUI.Button(button.text);
            
            // bool btnClicked = ImGui.Button(button.text);
            if (btnClicked)
            {
                button.clicked?.Invoke();
                if (button.closeOnClick)
                {
                    Hide();
                }
            }
            // ImGui.NewLine();
            // ImGui.SameLine();
        }
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly) && hoveringPanel==false )
        {
            Hide();
        }
        ImGui.End();

        


    }
}