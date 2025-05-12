using ImGuiNET;

namespace TofuEngine;

public static class EditorThemeing
{
    public static void SetTheme(EditorThemeEnum editorThemeEnum = EditorThemeEnum.Light)
    {
        SetTheme((int)editorThemeEnum);
    }

    public static void SetTheme(int themeIndex = 0)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        style.WindowMenuButtonPosition = ImGuiDir.None; // no arrow to "hide tab bar"
        style.WindowRounding = 0;
        style.WindowBorderSize = 0.2f;
        
        RangeAccessor<System.Numerics.Vector4> colors = style.Colors;

        if (themeIndex == 0)
        {
            Vector4 cTransparent = new Vector4(0, 0, 0, 0);
            Vector4 cBeige = new Vector4(1f, 0.96f, 0.90f, 1.00f);
            Vector4 cBeigeMid = new Vector4(0.97f, 0.94f, 0.88f, 1f);
            Vector4 cBeigeDarker = new Vector4(0.94f, 0.91f, 0.85f, 1f);
            Vector4 cBeigeEvenDarker = cBeigeDarker * new Vector4(0.92f, 0.92f, 0.92f, 1f);
            Vector4 cScrollbar = new Vector4(0.74f, 0.71f, 0.65f, 0.8f);
            Vector4 cScrollbarDarker = new Vector4(0.64f, 0.61f, 0.55f, 1f);

            colors[(int)ImGuiCol.Text] = new Vector4(0.23f, 0.29f, 0.40f, 1f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.23f, 0.29f, 0.40f, 0.5f);
            colors[(int)ImGuiCol.WindowBg] = cBeige;
            colors[(int)ImGuiCol.ChildBg] = cBeige;
            colors[(int)ImGuiCol.PopupBg] = cBeige;
            colors[(int)ImGuiCol.Border] = new Vector4(0.81f, 0.79f, 0.74f, 1);
            colors[(int)ImGuiCol.BorderShadow] = cTransparent;
            colors[(int)ImGuiCol.FrameBg] = cBeige;
            colors[(int)ImGuiCol.FrameBgHovered] = cBeige;
            colors[(int)ImGuiCol.FrameBgActive] = cBeige;
            colors[(int)ImGuiCol.TitleBg] = cBeigeEvenDarker;
            colors[(int)ImGuiCol.TitleBgActive] = cBeigeEvenDarker;
            colors[(int)ImGuiCol.TitleBgCollapsed] = cBeigeEvenDarker;
            colors[(int)ImGuiCol.MenuBarBg] = cBeige;
            colors[(int)ImGuiCol.ScrollbarBg] = cBeige;
            colors[(int)ImGuiCol.ScrollbarGrab] = cScrollbar;
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = cScrollbarDarker;
            colors[(int)ImGuiCol.ScrollbarGrabActive] = cScrollbarDarker;
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.43f, 0.43f, 0.43f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.63f, 0.63f, 0.63f, 0.78f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.43f, 0.44f, 0.46f, 0.78f);
            colors[(int)ImGuiCol.Button] = cBeigeDarker;
            colors[(int)ImGuiCol.ButtonHovered] = cBeigeMid;
            colors[(int)ImGuiCol.ButtonActive] = cBeigeDarker;
            colors[(int)ImGuiCol.Header] = cBeigeDarker;
            colors[(int)ImGuiCol.HeaderHovered] = cBeigeMid;
            colors[(int)ImGuiCol.HeaderActive] = cBeigeDarker;
            colors[(int)ImGuiCol.Separator] = new Vector4(0.81f, 0.79f, 0.74f, 1);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.80f, 0.80f, 0.80f, 0.56f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
            colors[(int)ImGuiCol.Tab] = cBeige;
            colors[(int)ImGuiCol.TabActive] = cBeige;
            colors[(int)ImGuiCol.TabUnfocused] = cBeige;
            colors[(int)ImGuiCol.TabUnfocusedActive] = cBeige;
            colors[(int)ImGuiCol.TabHovered] = cBeige;
            // colors[(int)ImGuiCol.CloseButton]            = new Vector4(0.59f, 0.59f, 0.59f, 0.50f);
            // colors[(int)ImGuiCol.ButtonHovered]     = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
            // colors[(int)ImGuiCol.ButtonActive]      = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.39f, 0.39f, 0.39f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Color(224, 203, 126, 255).ToVector4();

            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.71f, 0.72f, 0.73f, 0.57f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.16f, 0.16f, 0.17f, 0.95f);
        }

        if (themeIndex == 1)
        {
            colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.13f, 0.13f, 1.000f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.3f, 0.3f, 0.3f, 0f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.08f, 0.50f, 0.72f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.67f, 0.67f, 0.67f, 0.39f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.41f, 0.42f, 0.44f, 1.00f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.29f, 0.30f, 0.31f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.08f, 0.08f, 0.09f, 0.83f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.33f, 0.34f, 0.36f, 0.83f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.23f, 0.23f, 0.24f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.08f, 0.08f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.13f, 0.14f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.26f, 0.59f, 0.98f, 0.70f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.11f, 0.64f, 0.92f, 1.00f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
            colors[(int)ImGuiCol.Separator] = colors[(int)ImGuiCol.Border];
        }

        if (themeIndex == 2)
        {
            colors[(int)ImGuiCol.Text] = new Vector4(0.00f, 0.00f, 0.00f, 0.85f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.94f, 0.94f, 0.94f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.98f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.00f, 0.00f, 0.00f, 0.44f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.64f, 0.65f, 0.66f, 0.40f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.64f, 0.65f, 0.66f, 0.40f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.82f, 0.82f, 0.82f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.71f, 0.70f, 0.70f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 1.00f, 1.00f, 0.51f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.86f, 0.86f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.98f, 0.98f, 0.98f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.69f, 0.69f, 0.69f, 0.80f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.49f, 0.49f, 0.49f, 0.80f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.49f, 0.49f, 0.49f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.43f, 0.43f, 0.43f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.63f, 0.63f, 0.63f, 0.78f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.43f, 0.44f, 0.46f, 0.78f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.61f, 0.61f, 0.62f, 0.40f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.57f, 0.57f, 0.57f, 0.52f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.61f, 0.63f, 0.64f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.64f, 0.64f, 0.65f, 0.31f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.58f, 0.58f, 0.59f, 0.55f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.52f, 0.52f, 0.52f, 0.55f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.17f, 0.17f, 0.17f, 0.89f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.80f, 0.80f, 0.80f, 0.56f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.39f, 0.39f, 0.40f, 0.67f);
            // colors[(int)ImGuiCol.CloseButton]            = new Vector4(0.59f, 0.59f, 0.59f, 0.50f);
            // colors[(int)ImGuiCol.ButtonHovered]     = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
            // colors[(int)ImGuiCol.ButtonActive]      = new Vector4(0.98f, 0.39f, 0.36f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.39f, 0.39f, 0.39f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.78f, 0.78f, 0.78f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.71f, 0.72f, 0.73f, 0.57f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.16f, 0.16f, 0.17f, 0.95f);
        }

        if (themeIndex == 3)
        {
            colors[(int)ImGuiCol.Text] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.500f, 0.500f, 0.500f, 1.000f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.280f, 0.280f, 0.280f, 0.000f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.266f, 0.266f, 0.266f, 1.000f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.000f, 0.000f, 0.000f, 0.000f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.200f, 0.200f, 0.200f, 1.000f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.280f, 0.280f, 0.280f, 1.000f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.148f, 0.148f, 0.148f, 1.000f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.160f, 0.160f, 0.160f, 1.000f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.277f, 0.277f, 0.277f, 1.000f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.300f, 0.300f, 0.300f, 1.000f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.Button] = new Vector4(1.000f, 1.000f, 1.000f, 0.000f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(1.000f, 1.000f, 1.000f, 0.391f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.313f, 0.313f, 0.313f, 1.000f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.391f, 0.391f, 0.391f, 1.000f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.000f, 1.000f, 1.000f, 0.250f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.000f, 1.000f, 1.000f, 0.670f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.352f, 0.352f, 0.352f, 1.000f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.098f, 0.098f, 0.098f, 1.000f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.195f, 0.195f, 0.195f, 1.000f);
            colors[(int)ImGuiCol.DockingPreview] = new Vector4(1.000f, 0.391f, 0.000f, 0.781f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.180f, 0.180f, 0.180f, 1.000f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.469f, 0.469f, 0.469f, 1.000f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.586f, 0.586f, 0.586f, 1.000f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(1.000f, 1.000f, 1.000f, 0.156f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.000f, 0.391f, 0.000f, 1.000f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.000f, 0.000f, 0.000f, 0.586f);
            colors[(int)ImGuiCol.Separator] = colors[(int)ImGuiCol.Border];
        }
        else if (themeIndex == 4)
        {
            style.Alpha = 1.0f;
            // style.DisabledAlpha = 1.0f;
            style.WindowPadding = new Vector2(12.0f, 12.0f);
            // style.WindowRounding = 11.5f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(20.0f, 20.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            // style.FramePadding = new Vector2(20.0f, 3.400000095367432f);
            // style.FrameRounding = 11.89999961853027f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(4.300000190734863f, 5.5f);
            style.ItemInnerSpacing = new Vector2(7.099999904632568f, 1.799999952316284f);
            style.CellPadding = new Vector2(12.10000038146973f, 9.199999809265137f);
            style.IndentSpacing = 0.0f;
            style.ColumnsMinSpacing = 4.900000095367432f;
            style.ScrollbarSize = 11.60000038146973f;
            style.ScrollbarRounding = 15.89999961853027f;
            style.GrabMinSize = 3.700000047683716f;
            // style.GrabRounding = 20.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] =
                new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] =
                new Vector4(0.09250493347644806f, 0.100297249853611f, 0.1158798336982727f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] =
                new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBg] =
                new Vector4(0.1120669096708298f, 0.1262156516313553f, 0.1545064449310303f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] =
                new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] =
                new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] =
                new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] =
                new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] =
                new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] =
                new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] =
                new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.971993625164032f, 1.0f, 0.4980392456054688f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] =
                new Vector4(1.0f, 0.7953379154205322f, 0.4980392456054688f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] =
                new Vector4(0.1821731775999069f, 0.1897992044687271f, 0.1974248886108398f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] =
                new Vector4(0.1545050293207169f, 0.1545048952102661f, 0.1545064449310303f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] =
                new Vector4(0.1414651423692703f, 0.1629818230867386f, 0.2060086131095886f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] =
                new Vector4(0.1072951927781105f, 0.107295036315918f, 0.1072961091995239f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] =
                new Vector4(0.1293079704046249f, 0.1479243338108063f, 0.1931330561637878f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] =
                new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] =
                new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] =
                new Vector4(0.1459212601184845f, 0.1459220051765442f, 0.1459227204322815f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] =
                new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] =
                new Vector4(0.999999463558197f, 1.0f, 0.9999899864196777f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] =
                new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] =
                new Vector4(0.1249424293637276f, 0.2735691666603088f, 0.5708154439926147f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] =
                new Vector4(0.5215686559677124f, 0.6000000238418579f, 0.7019608020782471f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] =
                new Vector4(0.03921568766236305f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] =
                new Vector4(0.8841201663017273f, 0.7941429018974304f, 0.5615870356559753f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] =
                new Vector4(0.9570815563201904f, 0.9570719599723816f, 0.9570761322975159f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] =
                new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.0470588244497776f, 0.05490196123719215f,
                0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] =
                new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] =
                new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] =
                new Vector4(0.9356134533882141f, 0.9356129765510559f, 0.9356223344802856f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] =
                new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.266094446182251f, 0.2890366911888123f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] =
                new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f,
                0.5450980663299561f, 0.501960813999176f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f,
                0.5450980663299561f, 0.501960813999176f);
        }

        // style.WindowPadding = new Vector2(15, 15);
        // style.WindowRounding = 5.0f;
        // style.FramePadding = new System.Numerics.Vector2(5, 5);
        // style.FrameRounding = 4.0f;
        // style.ItemSpacing = new Vector2(12, 8);
        // style.ItemInnerSpacing = new Vector2(8, 6);
        // style.IndentSpacing = 25.0f;
        // style.ScrollbarSize = 15.0f;
        // style.ScrollbarRounding = 9.0f;
        // style.GrabMinSize = 5.0f;
        // style.GrabRounding = 3.0f;
    }
}