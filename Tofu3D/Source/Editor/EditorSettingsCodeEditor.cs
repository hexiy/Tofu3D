using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Tofu3D;

public class EditorSettingsCodeEditor
{
    [PathString(displayNameOnly: true)]
    [SplitWords]
    [CollectionWithSelectionAttrib_BrowsePath]
    [InspectorNameOverride("Code Editor")]
    public CollectionWithSelection<string> CodeEditorPaths;

    [ShowIf(nameof(ShowEditorArgs))]
    [InspectorNameOverride("Code Editor Args")]
    public string EditorArgs;

    private bool ShowEditorArgs
    {
        get
        {
            if (CodeEditorPaths == null)
            {
                return false;
            }

            string selectedEditorName = CodeEditorPaths?.GetFirstSelectedItem();
            CodeEditorInfo editorInfo = Tofu.UserCodeEditorOpener.GetEditorInfoByName(selectedEditorName);
            if (editorInfo.IsAddedByUser)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public void Init()
    {
        FillCodeEditorPathsCollection();
    }

    private void FillCodeEditorPathsCollection()
    {
        // to keep what user added but also load default editors
        string[] editorNames = Tofu.UserCodeEditorOpener.GetValidEditorNames();
        HashSet<string> editorNamesHashSet = new HashSet<string>();
        for (int i = 0; i < editorNames.Length; i++)
        {
            editorNamesHashSet.Add(editorNames[i]);
        }

        for (int i = 0; i < CodeEditorPaths?.Items.Count; i++)
        {
            editorNamesHashSet.Add(CodeEditorPaths.Items[i]);
        }

        CodeEditorPaths = new CollectionWithSelection<string>();
        CodeEditorPaths.Items = editorNamesHashSet.ToArray();
        CodeEditorPaths.SelectFirst();
    }
}