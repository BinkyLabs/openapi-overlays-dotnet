namespace BinkyLabs.OpenApi.Overlays;

public class OverlayDocument
{
    public OverlayInfo? Info { get; set; }
    public IList<OverlayAction>? Actions { get; set; }
}