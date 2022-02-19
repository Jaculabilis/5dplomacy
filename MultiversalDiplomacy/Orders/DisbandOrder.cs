using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order to disband a unit.
/// </summary>
public class DisbandOrder : UnitOrder
{
    public DisbandOrder(Power power, Unit unit)
        : base (power, unit) {}
}
