using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacyTests;

public class TestAdjudicator : IPhaseAdjudicator
{
    private Func<World, List<Order>, List<OrderValidation>> ValidateOrdersCallback;

    public TestAdjudicator(
        Func<World, List<Order>, List<OrderValidation>> validateOrdersCallback)
    {
        this.ValidateOrdersCallback = validateOrdersCallback;
    }

    public List<OrderValidation> ValidateOrders(World world, List<Order> orders)
        => this.ValidateOrdersCallback.Invoke(world, orders);
}