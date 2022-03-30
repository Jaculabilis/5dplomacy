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

    /// <summary>
    /// The destination province to which the unit should move.
    /// </summary>
    public Province Province => this.Location.Province;

    /// <summary>
    /// The destination's spatiotemporal location as a province-season tuple.
    /// </summary>
    public (Province province, Season season) Point => (this.Province, this.Season);

    public MoveOrder(Power power, Unit unit, Season season, Location location)
        : base (power, unit)
    {
        this.Season = season;
        this.Location = location;
    }

    public bool IsOpposing(MoveOrder other)
        => this.Season == other.Unit.Season
        && other.Season == this.Unit.Season
        && this.Province == other.Unit.Province
        && other.Province == this.Unit.Province;
}
