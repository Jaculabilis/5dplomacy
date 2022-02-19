using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a dislodged unit to retreat to an adjacent province.
/// </summary>
public class RetreatOrder : UnitOrder
{
    /// <summary>
    /// The destination location to which the unit should retreat.
    /// </summary>
    public Location Location { get; }

    public RetreatOrder(Power power, Unit unit, Location location)
        : base (power, unit)
    {
        this.Location = location;
    }
}
