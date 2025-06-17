using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TofuEngine;

public class EditorSettingsGeneral
{
    [SplitWords]
    [Slider(3, 25)]
    public int FontSize = 12;

    [Show]
    [InspectorNameOverride("Font")]
    [CollectionWithSelection_BrowsePath]
    public CollectionWithSelection<string> FontPathsCollection;

    [SplitWords]
    [Dropdown_SelectOnHover]
    public EditorThemeEnum EditorTheme;

    [JsonIgnore]
    [SplitWords]
    public Action DeletePersistentData => () => { PersistentData.DeleteAll(); };

    public void Init()
    {
        FillFontPathsCollection();
    }

    private void FillFontPathsCollection()
    {
        // to keep what we added but also load new fonts
        string[] fonts = Directory.GetFiles(Folders.EngineResourcesFonts, "*.ttf");
        HashSet<string> fontsHashSet = new HashSet<string>();
        for (int i = 0; i < fonts.Length; i++)
        {
            fontsHashSet.Add(fonts[i]);
        }

        for (int i = 0; i < FontPathsCollection?.Items.Count; i++)
        {
            fontsHashSet.Add(FontPathsCollection.Items[i]);
        }

        FontPathsCollection = new CollectionWithSelection<string>();
        FontPathsCollection.Items = fontsHashSet.ToArray();
        FontPathsCollection.SelectFirst();
    }
}