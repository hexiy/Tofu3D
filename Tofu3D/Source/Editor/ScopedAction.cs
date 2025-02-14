using ImGuiNET;

namespace TofuEngine;

public class ScopedAction : IDisposable
{
    private Action _callOnScopeExit;

    public ScopedAction(Action callOnScopeExit)
    {
        _callOnScopeExit = callOnScopeExit;
    }

    public void Dispose()
    {
       _callOnScopeExit.Invoke();
    }
}