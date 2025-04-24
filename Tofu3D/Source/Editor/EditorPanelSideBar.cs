using System.Linq;
using ImGuiNET;

namespace TofuEngine;

public class EditorPanelSideBar
{
    public event Action<int> SelectedItemChanged = (i) => { };

    public EditorPanelSideBar(EditorPanelSideBarButtonDefinition[] items)
    {
        Items = items;
        _itemsLabels = items.Select(a => a.Label).ToArray();
    }

    public EditorPanelSideBarButtonDefinition[] Items { get; private set; }
    private string[] _itemsLabels;
    private int _selectedItemIndex = 0;
    // private int SelectedItemIndex => _selectedItemIndex;

    private int _indexBefore = 0;

    public void Draw(float height)
    {
        // ImGui.SetNextWindowSize(new Vector2(250, height));
        // ImGui.Begin("sidebar", ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);
        // using TofuImGuiSetFramePaddingGuard setFramePaddingGuard = new TofuImGuiSetFramePaddingGuard(newPadding: 0);

        // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        Vector2 size = new Vector2(250, ImGui.GetContentRegionAvail().Y + TofuImGui.WindowMenuBarHeight - 12f);
        // ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero);
        ImGui.SetCursorPos(new Vector2(0, TofuImGui.WindowMenuBarHeight));

        // BeginChild doesnt span full height
        ImGui.BeginChildFrame(1, size,
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

        ImGui.SetNextItemWidth(size.X);
        ImGui.ListBox("", ref _selectedItemIndex, _itemsLabels, _itemsLabels.Length);
        // ImGui.End();
        // ImGui.PopStyleVar();

        ImGui.EndChildFrame();
        // ImGui.PopStyleVar();

        if (_selectedItemIndex != _indexBefore)
        {
            _indexBefore = _selectedItemIndex;

            SelectedItemChanged.Invoke(_selectedItemIndex);
        }
    }
}