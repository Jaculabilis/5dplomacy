using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to support another unit's hold order.
/// </summary>
public class SupportHoldOrder : UnitOrder
{
    /// <summary>
    /// The unit to support.
    /// </summary>
    public Unit Target { get; }

    public SupportHoldOrder(Power power, Unit unit, Unit target)
        : base (power, unit)
    {
        this.Target = target;
    }
}