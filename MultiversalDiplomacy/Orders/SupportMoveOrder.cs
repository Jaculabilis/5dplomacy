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

    /// <summary>
    /// The destination province to which the target is moving.
    /// </summary>
    public Province Province => this.Location.Province;

    /// <summary>
    /// The target's destination's spatiotemporal location as a province-season tuple.
    /// </summary>
    public (Province province, Season season) Point => (this.Province, this.Season);

    public SupportMoveOrder(Power power, Unit unit, Unit target, Season season, Location location)
        : base(power, unit, target)
    {
        this.Season = season;
        this.Location = location;
    }

    public override string ToString()
    {
        return $"{this.Unit} S {this.Target} -> {(this.Province, this.Season).ToShort()}";
    }

    public bool IsSupportFor(MoveOrder move)
        => this.Target == move.Unit
        && this.Season == move.Season
        && this.Location == move.Location;
}