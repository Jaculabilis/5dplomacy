using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to support another unit.
/// </summary>
public abstract class SupportOrder : UnitOrder
{
    /// <summary>
    /// The unit to support.
    /// </summary>
    public Unit Target { get; }

    public SupportOrder(Power power, Unit unit, Unit target)
        : base (power, unit)
    {
        this.Target = target;
    }
}