using System.IO;
using ImGuiNET;

namespace Tofu3D;

public class BrowserContextItem
{
	Action<string> _confirmAction;
	string _defaultFileName;
	string _fileExtension;
	string _itemName;
	public bool ShowPopup;

	public BrowserContextItem(string itemName, string defaultFileName, string fileExtension, Action<string> confirmAction)
	{
		this._itemName = itemName;
		this._defaultFileName = defaultFileName;
		this._fileExtension = fileExtension;
		this._confirmAction = confirmAction;
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
					string filePath = Path.Combine(EditorPanelBrowser.I.CurrentDirectory.FullName, _defaultFileName + _fileExtension);
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