using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Helper class for common operations shared between adjudicators.
/// </summary>
internal static class AdjudicatorHelpers
{
    /// <summary>
    /// Invalidate all orders that do not match a predicate.
    /// </summary>
    /// <param name="predicate">A predicate that invalid orders will fail to match.</param>
    /// <param name="reason">The reason to be given for order invalidation.</param>
    /// <param name="orders">The set of orders to check.</param>
    /// <param name="validOrders">The list of orders that passed the predicate.</param>
    /// <param name="invalidOrders">
    /// A list of order validation results. Orders invalidated by the predicate will be appended
    /// to this list.
    /// </param>
    public static void InvalidateIfNotMatching<OrderType>(
        Func<OrderType, bool> predicate,
        ValidationReason reason,
        ref List<OrderType> orders,
        ref List<OrderValidation> invalidOrders)
        where OrderType : Order
    {
        ILookup<bool, OrderType> results = orders.ToLookup<OrderType, bool>(predicate);
        invalidOrders = invalidOrders
            .Concat(results[false].Select(order => order.Invalidate(reason)))
            .ToList();
        orders = results[true].ToList();
    }

    /// <summary>
    /// Invalidate all orders that are not of an allowed order type.
    /// </summary>
    /// <param name="orders">The set of orders to check.</param>
    /// <param name="validOrderTypes">A list of <see cref="Order"/> types that are allowed.</param>
    /// <param name="validOrders">The list of orders of allowed types.</param>
    /// <param name="invalidOrders">
    /// A list of order validation results. Orders of invalid types will be appended to this list.
    /// </param>
    public static void InvalidateWrongTypes(
        List<Type> validOrderTypes,
        ref List<Order> orders,
        ref List<OrderValidation> invalidOrders)
    {
        List<Type> nonOrderTypes = validOrderTypes
            .Where(t => !t.IsSubclassOf(typeof(Order)))
            .ToList();
        if (nonOrderTypes.Any())
        {
            throw new ArgumentException(
                $"Unknown order type: {nonOrderTypes.Select(t => t.FullName).First()}");
        }

        InvalidateIfNotMatching(
            order => validOrderTypes.Contains(order.GetType()),
            ValidationReason.InvalidOrderTypeForPhase,
            ref orders,
            ref invalidOrders);
    }

    /// <summary>
    /// Invalidate all orders for units not owned by the ordering power.
    /// </summary>
    /// <param name="orders">The set of orders to check.</param>
    /// <param name="validOrders">The list of orders with valid targets.</param>
    /// <param name="invalidOrders">
    /// A list of order validation results. Orders by the wrong powers will be appended to this list.
    /// </param>
    public static void InvalidateWrongPower(
        List<Order> orders,
        ref List<Order> validOrders,
        ref List<OrderValidation> invalidOrders)
    {
        InvalidateIfNotMatching(
            order => order switch {
                ConvoyOrder convoy => convoy.Power == convoy.Unit.Power,
                DisbandOrder disband => disband.Power == disband.Unit.Power,
                HoldOrder hold => hold.Power == hold.Unit.Power,
                MoveOrder move => move.Power == move.Unit.Power,
                RetreatOrder retreat => retreat.Power == retreat.Unit.Power,
                SupportHoldOrder support => support.Power == support.Unit.Power,
                SupportMoveOrder support => support.Power == support.Unit.Power,
                // Any order not given to a unit, by definition, cannot be given to a unit of the
                // wrong power
                _ => true,
            },
            ValidationReason.InvalidUnitForPower,
            ref validOrders,
            ref invalidOrders);
    }
}
