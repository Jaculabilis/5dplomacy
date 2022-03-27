using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacyTests;

public class TestAdjudicator : IPhaseAdjudicator
{
    public static List<OrderValidation> RubberStamp(World world, List<Order> orders)
    {
        return orders.Select(o => o.Validate(ValidationReason.Valid)).ToList();
    }

    public static List<AdjudicationDecision> NoMoves(
        World world,
        List<Order> orders)
    {
        List<AdjudicationDecision> results = new();
        foreach (Order order in orders)
        {
            switch (order)
            {
                case MoveOrder move:
                {
                    var doesMove = new DoesMove(move, null, new List<MoveOrder>());
                    doesMove.Update(false);
                    results.Add(doesMove);
                    var dislodged = new IsDislodged(move, new List<MoveOrder>());
                    dislodged.Update(false);
                    results.Add(dislodged);
                    break;
                }

                default:
                {
                    if (order is not UnitOrder unitOrder)
                    {
                        throw new ArgumentException(order.GetType().Name);
                    }
                    var dislodged = new IsDislodged(unitOrder, new List<MoveOrder>());
                    dislodged.Update(false);
                    results.Add(dislodged);
                    break;
                }
            }
        }
        return results;
    }

    public static World Noop(World world, List<AdjudicationDecision> decisions)
        => world;

    private static List<OrderValidation> NoValidate(World world, List<Order> orders)
        => throw new NotImplementedException();

    private static List<AdjudicationDecision> NoAdjudicate(World world, List<Order> orders)
        => throw new NotImplementedException();

    private static World NoUpdate(World world, List<AdjudicationDecision> decisions)
        => throw new NotImplementedException();

    private Func<World, List<Order>, List<OrderValidation>> ValidateOrdersCallback;
    private Func<World, List<Order>, List<AdjudicationDecision>> AdjudicateOrdersCallback;
    private Func<World, List<AdjudicationDecision>, World> UpdateWorldCallback;

    public TestAdjudicator(
        Func<World, List<Order>, List<OrderValidation>>? validate = null,
        Func<World, List<Order>, List<AdjudicationDecision>>? adjudicate = null,
        Func<World, List<AdjudicationDecision>, World>? update = null)
    {
        this.ValidateOrdersCallback = validate ?? NoValidate;
        this.AdjudicateOrdersCallback = adjudicate ?? NoAdjudicate;
        this.UpdateWorldCallback = update ?? NoUpdate;
    }

    public List<OrderValidation> ValidateOrders(World world, List<Order> orders)
        => this.ValidateOrdersCallback.Invoke(world, orders);

    public List<AdjudicationDecision> AdjudicateOrders(World world, List<Order> orders)
        => this.AdjudicateOrdersCallback(world, orders);

    public World UpdateWorld(World world, List<AdjudicationDecision> decisions)
        => this.UpdateWorldCallback(world, decisions);
}