using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Tofu3D;

public class EditorSettingsGeneral
{
    [SplitWords]
    [Slider(3, 25)]
    public int FontSize = 12;

    [Show]
    [InspectorNameOverride("Font")]
    [CollectionWithSelectionAttrib_BrowsePath()]
    private CollectionWithSelection<string> FontPathsCollection;

    [SplitWords]
    public EditorThemeEnum EditorTheme;

    [JsonIgnore]
    [SplitWords]
    public Action DeletePersistentData => () => { PersistentData.DeleteAll(); };

    public EditorSettingsGeneral()
    {
        if (FontPathsCollection == null)
        {
            string[] fonts = Directory.GetFiles(Folders.FontsInResources, "*.ttf");
            FontPathsCollection = new CollectionWithSelection<string>();
            FontPathsCollection.Items = fonts;
            FontPathsCollection.SelectFirst();
        }
    }
}