using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to support another unit's move order.
/// </summary>
public class SupportMoveOrder : SupportOrder
{
    /// <summary>
    /// The destination season to which the target is moving.
    /// </summary>
    public Season Season { get; }

    /// <summary>
    /// The destination location to which the target is moving.
    /// </summary>
    public Location Location { get; }

    public SupportMoveOrder(Power power, Unit unit, Unit target, Season season, Location location)
        : base(power, unit, target)
    {
        this.Season = season;
        this.Location = location;
    }
}