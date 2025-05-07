using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TofuEngine;

public class EditorSettingsSceneView
{
    [Slider(5, 20)]
    public int SliderTest = 10;
    [Slider(5, 20)]
    public int SliderTest2 = 4;
    [Header("Scene Navigation")]
    [InspectorNameOverride("Move speed")]
    [SliderF(0.1f, 20f)]
    public float MoveSpeed = 5f;

    [InspectorNameOverride("Look sensitivity")]
    [SliderF(0.01f, 2f)]
    public float LookSensitivity = 0.2f;

    [Space]
    [Header("Editing")]
    [InspectorNameOverride("Show object outlines")]
    public bool ShowObjectOutlines;
}