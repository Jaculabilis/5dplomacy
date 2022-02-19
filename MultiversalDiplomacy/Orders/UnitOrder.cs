using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order given to a specific unit.
/// </summary>
public abstract class UnitOrder : Order
{
    /// <summary>
    /// The ordered unit.
    /// </summary>
    public Unit Unit { get; }

    public UnitOrder(Power power, Unit unit) : base(power)
    {
        this.Unit = unit;
    }
}