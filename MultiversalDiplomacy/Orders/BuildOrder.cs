using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order to build a unit.
/// </summary>
public class BuildOrder : Order
{
    /// <summary>
    /// The location in which to build the unit.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// The type of unit to build.
    /// </summary>
    public UnitType Type { get; }

    public BuildOrder(Power power, Location location, UnitType type)
        : base (power)
    {
        this.Location = location;
        this.Type = type;
    }
}
