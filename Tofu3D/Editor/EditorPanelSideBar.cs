using ImGuiNET;

namespace Tofu3D;

public class EditorPanelSideBar
{
    public event Action<int> SelectedItemChanged = (i) => { };

    public EditorPanelSideBar(string[] itemsLabels)
    {
        _itemsLabels = itemsLabels;
    }

    private string[] _itemsLabels;
    private int _selectedItemIndex = 0;
    // private int SelectedItemIndex => _selectedItemIndex;

    private int _indexBefore = 0;
    public void Draw(float height)
    {
        ImGui.SetNextWindowSize(new Vector2(250, height));
        ImGui.Begin("sidebar", ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove);
        
        ImGui.ListBox("", ref _selectedItemIndex, _itemsLabels, _itemsLabels.Length);
        ImGui.End();

        if (_selectedItemIndex != _indexBefore)
        {
            _indexBefore = _selectedItemIndex;
            
            SelectedItemChanged.Invoke(_selectedItemIndex);
        }
    }
    
}