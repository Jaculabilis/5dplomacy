using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order to move another unit via convoy.
/// </summary>
public class ConvoyOrder : UnitOrder
{
    /// <summary>
    /// The unit to convoy.
    /// </summary>
    public Unit Target { get; }

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

    public ConvoyOrder(Power power, Unit unit, Unit target, Season season, Location location)
        : base (power, unit)
    {
        this.Target = target;
        this.Season = season;
        this.Location = location;
    }

    public override string ToString()
    {
        return $"{this.Unit} C {this.Target} -> {(this.Province, this.Season).ToShort()}";
    }
}
