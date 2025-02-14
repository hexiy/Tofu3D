using ImGuiNET;

namespace Tofu3D;

public class BrowserContextItemCreateFile
{
    private readonly Action<string> _confirmAction;
    private string _fileName;
    private readonly string _fileExtension;
    private readonly string _itemName;
    public bool ShowPopup;

    public BrowserContextItemCreateFile(string itemName, string fileName, string fileExtension,
        Action<string> confirmAction)
    {
        _itemName = itemName;
        _fileName = fileName;
        _fileExtension = fileExtension;
        _confirmAction = confirmAction;
    }

    public void ShowContextItem()
    {
        if (ImGui.MenuItem(_itemName))
        {
            ShowPopup = true;
            ImGui.CloseCurrentPopup();
        }
        // if (ImGui.Button(_itemName))
        // {
        //     ShowPopup = true;
        //     ImGui.CloseCurrentPopup();
        // }
    }

    public void ShowPopupIfOpen()
    {
        if (ShowPopup)
        {
            ImGui.OpenPopup(_itemName);

            if (ImGui.BeginPopupContextWindow(_itemName))
            {
                ImGui.InputText("", ref _fileName, 100);
                if (ImGui.Button("Save"))
                {
                    string filePath = TofuPath.Combine(EditorPanelBrowser.I.CurrentDirectoryInfo.FullName,
                        _fileName + _fileExtension);
                    _confirmAction.Invoke(filePath);

                    ShowPopup = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Cancel"))
                {
                    ShowPopup = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            else
            {
                ShowPopup = false;
            }
        }
    }
}