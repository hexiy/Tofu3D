namespace Tofu3D;

public class EditorDialogManager
{
    private readonly List<EditorDialog> _dialogs;

    public EditorDialogManager()
    {
        _dialogs = new List<EditorDialog>();

        EditorDialog dialog1 = new(_dialogs.Count);
        _dialogs.Add(dialog1);
    }

    public EditorDialogHandle ShowDialog(EditorDialogParams dialogParams)
    {
        foreach (var dialog in _dialogs)
        {
            if (dialog.IsActive == false)
            {
                return dialog.Show(dialogParams);
            }
        }

        return null;
    }

    public void HideDialog(EditorDialogHandle dialogHandle)
    {
        GetDialogByHandle(dialogHandle)?.Hide();
    }

    private EditorDialog? GetDialogByHandle(EditorDialogHandle dialogHandle)
    {
        foreach (var editorDialog in _dialogs)
        {
            if (editorDialog.Handle == dialogHandle)
            {
                return editorDialog;
            }
        }

        return null;
    }

    public bool IsDialogActive(EditorDialogHandle dialogHandle) => GetDialogByHandle(dialogHandle)?.IsActive ?? false;

    public void Update()
    {
        foreach (var editorDialog in _dialogs)
        {
            if (editorDialog.IsActive)
            {
                editorDialog.Update();
            }
        }
    }

    public void Draw()
    {
        foreach (var editorDialog in _dialogs)
        {
            if (editorDialog.IsActive)
            {
                editorDialog.Draw();
            }
        }
    }
}