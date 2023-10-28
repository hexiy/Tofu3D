using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerAudioClip : InspectorFieldDrawable<AudioClip>
{
    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        AudioClip audioClip = GetValue(info, componentInspectorData);
        if (audioClip == null)
        {
            audioClip = new AudioClip();
            info.SetValue(componentInspectorData.Inspectable, audioClip);
        }

        string clipName = Path.GetFileName(audioClip?.Path);

        bool clicked = ImGui.Button(clipName,
            new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));


        if (ImGui.BeginDragDropTarget())
        {
            ImGui.AcceptDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", ImGuiDragDropFlags.None);
            string fileName = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && fileName.Length > 0)
            {
                // fileName = Path.GetRelativePath("Assets", fileName);

                audioClip.Path = fileName;
                SetValue(info, componentInspectorData, audioClip);
            }

            ImGui.EndDragDropTarget();
        }
    }
}