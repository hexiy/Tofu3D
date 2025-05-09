using System.IO;
using System.Linq;
using ImGuiNET;
using NativeFileDialogSharp;

namespace TofuEngine;

public class InspectorFieldDrawerCollectionWithSelection<T> : InspectorFieldDrawable<CollectionWithSelection<T>>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        CollectionWithSelection<T> fieldValue = GetValue(info, componentInspectorData);

        bool hasBrowserPathAttrib =
            info.GetCustomAttribute<CollectionWithSelection_BrowsePathAttribute>(
                out CollectionWithSelection_BrowsePathAttribute? browserPathAttrib);

        List<string> collectionValuesAsStrings = fieldValue.Items.Cast<string>().ToList();

        if (hasBrowserPathAttrib)
        {
            collectionValuesAsStrings.Add("------Add------");
        }

        string[] collectionValuesAsStringsArray = collectionValuesAsStrings.ToArray();

        info.GetCustomAttribute<PathStringAttribute>(out PathStringAttribute? pathStringAttrib);
        if (pathStringAttrib != null && pathStringAttrib.DisplayNameOnly)
        {
            for (int i = 0; i < collectionValuesAsStringsArray.Length; i++)
            {
                collectionValuesAsStringsArray[i] = Path.GetFileName(collectionValuesAsStringsArray[i]);
            }
        }


        int _firstSelectedIndex = 0;
        IReadOnlyList<int> selectedIndexes = fieldValue.GetSelectedIndices();
        if (selectedIndexes.Count > 0)
        {
            _firstSelectedIndex = selectedIndexes[0];
        }

        bool clicked = ImGui.Combo(string.Empty, ref _firstSelectedIndex, collectionValuesAsStringsArray,
            collectionValuesAsStringsArray.Length);
        if (clicked)
        {
            if (hasBrowserPathAttrib && _firstSelectedIndex == collectionValuesAsStrings.Count - 1)
            {
                DialogResult dialogResult = Dialog.FileOpen(browserPathAttrib.FileFilter);

                bool pathDoesntExistInCollection = collectionValuesAsStrings.Contains(dialogResult.Path) == false;
                if (dialogResult.IsOk && pathDoesntExistInCollection)
                {
                    collectionValuesAsStrings[^1] = dialogResult.Path;

                    fieldValue.Items = (IReadOnlyList<T>)collectionValuesAsStrings;
                    SetValue(info, componentInspectorData, fieldValue);
                }
                else
                {
                    _firstSelectedIndex--;
                }
            }

            fieldValue.SelectItems([_firstSelectedIndex]);
            SetValue(info, componentInspectorData, fieldValue);

            componentInspectorData.Inspector.QueueRefresh(componentInspectorData);
        }
    }
}