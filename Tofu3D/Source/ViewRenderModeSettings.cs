using Newtonsoft.Json;

namespace Tofu3D;

public class ViewRenderModeSettings
{
    [JsonIgnore]
    public ViewRenderMode CurrentRenderMode = ViewRenderMode.Regular;
    // some other parameters?
}