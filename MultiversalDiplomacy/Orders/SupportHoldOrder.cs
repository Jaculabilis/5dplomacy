using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to support another unit's hold order.
/// </summary>
public class SupportHoldOrder : SupportOrder
{
    public SupportHoldOrder(Power power, Unit unit, Unit target)
        : base (power, unit, target)
    {
    }

    public override string ToString()
    {
        return $"{this.Unit} S {this.Target}";
    }
}