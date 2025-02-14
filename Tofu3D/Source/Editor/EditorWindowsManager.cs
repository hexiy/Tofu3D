using System;
using System.Collections.Generic;

public class EditorWindowsManager
{
    private Stack<IEditorWindow> _openedWindows = new Stack<IEditorWindow>();

    public EditorWindowsManager()
    {
        // CameraController.I.RegisterInputCondition(() => _openedWindows.Count == 0);
    }

    public void ToggleWindow(IEditorWindow window, bool tgl)
    {
        if (tgl)
        {
            OpenWindow(window);
        }
        else
        {
            CloseWindow(window);
        }
    }

    public void OpenWindow(IEditorWindow window)
    {
        window.OnToggled(true);
        _openedWindows.Push(window);
    }

    public void CloseWindow(IEditorWindow window)
    {
        if (_openedWindows.TryPeek(out IEditorWindow result) && result == window)
        {
            window.OnToggled(false);
            _openedWindows.Pop();
        }
    }

    public bool IsInFront(IEditorWindow window)
    {
        return _openedWindows.TryPeek(out IEditorWindow result) && (result == window);
    }

    public int NumberOfActiveWindows => _openedWindows.Count;
    public bool AnyWindowOpen => _openedWindows.Count > 0;
}