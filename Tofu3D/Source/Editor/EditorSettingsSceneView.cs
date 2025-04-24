using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TofuEngine;

public class EditorSettingsSceneView
{
    [Header("Scene Navigation")]
    [InspectorNameOverride("Move speed")]
    public float MoveSpeed;

    [InspectorNameOverride("Look sensitivity")]
    public float LookSensitivity;

    [Space]
    [Header("Editing")]
    [InspectorNameOverride("Show object outlines")]
    public bool ShowObjectOutlines;
}