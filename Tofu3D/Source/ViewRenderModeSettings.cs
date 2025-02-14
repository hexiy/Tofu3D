using Newtonsoft.Json;

namespace TofuEngine;

public class ViewRenderModeSettings
{
    [JsonIgnore]
    public ViewRenderMode CurrentRenderMode = ViewRenderMode.Regular;
    // some other parameters?
}