namespace TofuEngine;

public class EditorViewManager
{
    public static EditorPanelGenericView? LastUsedView { get; set; }
    public static EditorPanelGenericView? LastHoveredView { get; set; }
    public static EditorPanelGenericView? CurrentlyHoveredView { get; set; }
    
    public static Action<EditorPanel> AnyPanelFocused = (panel) => { };
    public static Action<EditorPanel> AnyPanelHovered = (panel) => { };
    public static bool IsAnyPanelHovered => CurrentlyHoveredView != null;

    public EditorViewManager()
    {


        AnyPanelFocused += OnAnyPanelFocused;
        AnyPanelHovered += OnAnyEditorPanelHoverChanged;

    }
    
    private void OnAnyPanelFocused(EditorPanel panel)
    {
        if (panel is EditorPanelGenericView v)
        {
            LastUsedView = v;
        }
    }

    private void OnAnyEditorPanelHoverChanged(EditorPanel panel)
    {
        if (panel is EditorPanelGenericView v)
        {
            // Debug.Log($"Hovered view : {v.Name}");
            LastHoveredView = v;
            CurrentlyHoveredView = v;
        }
        else
        {
            CurrentlyHoveredView = null;
        }
    }
}