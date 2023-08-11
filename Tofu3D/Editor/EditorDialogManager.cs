namespace Tofu3D;

public class EditorDialogManager
{
    private List<EditorDialog> _dialogs;

    public EditorDialogManager()
    {
        _dialogs = new List<EditorDialog>();
        
        EditorDialog dialog1 = new EditorDialog(_dialogs.Count);
        _dialogs.Add(dialog1);
    }

    public EditorDialogHandle ShowDialog(EditorDialogParams dialogParams)
    {
        foreach (EditorDialog dialog in _dialogs)
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
        foreach (EditorDialog editorDialog in _dialogs)
        {
            if (editorDialog.Handle == dialogHandle)
            {
                return editorDialog;
            }
        }

        return null;
    }

    public bool IsDialogActive(EditorDialogHandle dialogHandle)
    {
        return GetDialogByHandle(dialogHandle)?.IsActive ??false;
    }

    public void Update()
    {
        foreach (EditorDialog editorDialog in _dialogs)
        {
            if (editorDialog.IsActive)
            {
                editorDialog.Update();
            }
        }
    }

    public void Draw()
    {
        foreach (EditorDialog editorDialog in _dialogs)
        {
            if (editorDialog.IsActive)
            {
                editorDialog.Draw();
            }
        }
    }
}