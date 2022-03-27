using MultiversalDiplomacy.Orders;
using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// An input handler for game phases.
/// </summary>
public interface IPhaseAdjudicator
{
    /// <summary>
    /// Given a list of orders, determine which orders are valid for this adjudicator and
    /// which should be rejected before adjudication. Adjudication should be performed on
    /// all orders in the output for which <see cref="OrderValidation.Valid"/> is true.
    /// </summary>
    /// <param name="world">The global game state.</param>
    /// <param name="orders">Orders to validate for adjudication.</param>
    /// <returns>
    /// A list of order validation results. Note that this list may be longer than the input
    /// list if illegal orders were replaced with hold orders, as there will be an invalid
    /// result for the illegal order and a valid result for the replacement order.
    /// </returns>
    public List<OrderValidation> ValidateOrders(World world, List<Order> orders);

    /// <summary>
    /// Given a list of valid orders, adjudicate the success and failure of the orders. The kinds
    /// of adjudication decisions returned depends on the phase adjudicator.
    /// </summary>
    /// <param name="world">The global game state.</param>
    /// <param name="orders">
    /// Orders to adjudicate. The order list should contain only valid orders, as validated by
    /// <see cref="ValidateOrders"/>, and should contain exactly one order for every unit able to
    /// be ordered.
    /// </param>
    /// <returns>
    /// A list of adjudication decicions. The decision types will be specific to the phase
    /// adjudicator and should be comprehensible to that adjudicator's <see cref="Update"/> method.
    /// </returns>
    public List<AdjudicationDecision> AdjudicateOrders(World world, List<Order> orders);

    /// <summary>
    /// Given a list of adjudications, update the world according to the adjudication results.
    /// </summary>
    /// <param name="world">The global game state.</param>
    /// <param name="decisions">
    /// The results of adjudication. Like <see cref="AdjudicateOrders"/>, all objects to be updated
    /// should have a relevant adjudication. The adjudication types will be specific to the phase
    /// adjudicator.
    /// </param>
    /// <returns>
    /// A new copy of the world, updated according to the adjudication.
    /// </returns>
    public World UpdateWorld(World world, List<AdjudicationDecision> decisions);
}
