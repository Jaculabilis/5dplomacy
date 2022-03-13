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
}
