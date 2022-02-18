using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacyTests;

public class TestAdjudicator : IPhaseAdjudicator
{
    private Func<IEnumerable<Order>, IEnumerable<OrderValidation>> ValidateOrdersCallback;

    public TestAdjudicator(
        Func<IEnumerable<Order>, IEnumerable<OrderValidation>> validateOrdersCallback)
    {
        this.ValidateOrdersCallback = validateOrdersCallback;
    }

    public IEnumerable<OrderValidation> ValidateOrders(IEnumerable<Order> orders)
        => this.ValidateOrdersCallback.Invoke(orders);
}