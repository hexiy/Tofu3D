namespace TofuEngine;

public interface IEditorWindow
{
    bool IsOpened { get; set; }
    void Toggle(bool tgl);
    void OnToggled(bool tgl);
}