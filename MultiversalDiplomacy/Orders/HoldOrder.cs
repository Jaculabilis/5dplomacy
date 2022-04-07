using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order for a unit to hold its current province.
/// </summary>
public class HoldOrder : UnitOrder
{
    public HoldOrder(Power power, Unit unit)
        : base (power, unit) {}

    public override string ToString()
    {
        return $"{this.Unit} holds";
    }
}
