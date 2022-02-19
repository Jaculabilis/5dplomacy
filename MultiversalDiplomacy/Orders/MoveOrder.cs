using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to move to another province.
/// </summary>
public class MoveOrder : UnitOrder
{
    /// <summary>
    /// The destination season to which the unit should move.
    /// </summary>
    public Season Season { get; }

    /// <summary>
    /// The destination location to which the unit should move.
    /// </summary>
    public Location Location { get; }

    public MoveOrder(Power power, Unit unit, Season season, Location location)
        : base (power, unit)
    {
        this.Season = season;
        this.Location = location;
    }
}
