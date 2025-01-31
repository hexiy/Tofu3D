using System.Collections;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public interface IHasInspector
{
    public void SelectInspectable(object inspectable, Action? anyValueChanged = null);

    public void SelectInspectables(IList inspectables);


    // public void Refresh();
    
    // public void QueueRefresh();

    // public void QueueRefresh(InspectableData inspectableData);
    
    // public void AddActionToActionQueue(Action action);
}