using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// A submitted action by a power.
/// </summary>
public abstract class Order
{
    /// <summary>
    /// The power that submitted this order.
    /// </summary>
    public Power Power { get; }

    public Order(Power power)
    {
        this.Power = power;
    }
}
