using System.IO;
using ImGuiNET;

namespace Tofu3D;

public class BrowserContextItem
{
    private Action<string> _confirmAction;
    private string _defaultFileName;
    private string _fileExtension;
    private string _itemName;
    public bool ShowPopup;

    public BrowserContextItem(string itemName, string defaultFileName, string fileExtension,
        Action<string> confirmAction)
    {
        _itemName = itemName;
        _defaultFileName = defaultFileName;
        _fileExtension = fileExtension;
        _confirmAction = confirmAction;
    }

    public void ShowContextItem()
    {
        if (ImGui.Button(_itemName))
        {
            ShowPopup = true;
            ImGui.CloseCurrentPopup();
        }
    }

    public void ShowPopupIfOpen()
    {
        if (ShowPopup)
        {
            ImGui.OpenPopup(_itemName);

            if (ImGui.BeginPopupContextWindow(_itemName))
            {
                ImGui.InputText("", ref _defaultFileName, 100);
                if (ImGui.Button("Save"))
                {
                    string filePath = Path.Combine(EditorPanelBrowser.I.CurrentDirectory.FullName,
                        _defaultFileName + _fileExtension);
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