using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order to sustain a timeline in existence.
/// </summary>
public class SustainOrder : Order
{
    /// <summary>
    /// The ordered time center.
    /// </summary>
    public Location TimeCenter { get; }

    /// <summary>
    /// The timeline to sustain.
    /// </summary>
    public int Timeline { get; }

    public SustainOrder(Power power, Location timeCenter, int timeline)
        : base (power)
    {
        this.TimeCenter = timeCenter;
        this.Timeline = timeline;
    }
}
