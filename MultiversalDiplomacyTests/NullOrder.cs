using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacyTests;

public class NullOrder : Order
{
    public NullOrder(Power power) : base(power) {}
}