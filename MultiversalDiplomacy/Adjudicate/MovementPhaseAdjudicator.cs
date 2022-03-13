using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Adjudicator for the movement phase.
/// </summary>
internal class MovementPhaseAdjudicator : IPhaseAdjudicator
{
    public List<OrderValidation> ValidateOrders(World world, List<Order> orders)
    {
        // The basic workflow of this function will be to look for invalid orders, remove these
        // from the working set of orders, and then perform one final check for duplicate orders
        // at the end. This is to comply with DATC 4.D.3's requirement that a unit that receives
        // a legal and an illegal order follows the legal order rather than holding.
        List<OrderValidation> validationResults = new List<OrderValidation>();

        // Invalidate any orders that aren't a legal type for this phase and remove them from the
        // working set.
        AdjudicatorHelpers.InvalidateWrongTypes(
            new List<Type>
            {
                typeof(HoldOrder),
                typeof(MoveOrder),
                typeof(ConvoyOrder),
                typeof(SupportHoldOrder),
                typeof(SupportMoveOrder)
            },
            ref orders,
            ref validationResults);

        // Invalidate any orders by a power that were given to another power's units and remove
        // them from the working set.
        AdjudicatorHelpers.InvalidateWrongPower(orders, ref orders, ref validationResults);

        // Since all the order types in this phase are UnitOrders, downcast to get the Unit.
        List<UnitOrder> unitOrders = orders.OfType<UnitOrder>().ToList();

        // Invalidate any order given to a unit in the past.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => !order.Unit.Season.Futures.Any(),
            ValidationReason.IneligibleForOrder,
            ref unitOrders,
            ref validationResults);

        /***************
         * HOLD ORDERS *
         ***************/
        // Hold orders are always valid.
        List<HoldOrder> holdOrders = unitOrders.OfType<HoldOrder>().ToList();

        /***************
         * MOVE ORDERS *
         ***************/
        // Move order validity is far more complicated, due to multiversal time travel and convoys.
        List<MoveOrder> moveOrders = unitOrders.OfType<MoveOrder>().ToList();

        // Trivial check: armies cannot move to water and fleets cannot move to land.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => (order.Unit.Type == UnitType.Army && order.Location.Type == LocationType.Land)
                || (order.Unit.Type == UnitType.Fleet && order.Location.Type == LocationType.Water),
            ValidationReason.IllegalDestinationType,
            ref moveOrders,
            ref validationResults);

        // Trivial check: a unit cannot move to where it already is.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => !(order.Location == order.Unit.Location && order.Season == order.Unit.Season),
            ValidationReason.DestinationMatchesOrigin,
            ref moveOrders,
            ref validationResults);

        // If the unit is moving to a valid destination that isn't where it already is, then the
        // move order is valid if there is a path from the origin to the destination. In the easy
        // case, the destination is directly adjacent to the origin with respect to the map, the
        // turn, and the timeline. These moves are valid. Any other move must be checked for
        // potential validity as a convoy move.
        ILookup<bool, MoveOrder> moveOrdersByAdjacency = moveOrders
            .ToLookup(order =>
                // Map adjacency
                order.Unit.Location.Adjacents.Contains(order.Location)
                // Turn adjacency
                && Math.Abs(order.Unit.Season.Turn - order.Season.Turn) <= 1
                // Timeline adjacency
                && order.Unit.Season.InAdjacentTimeline(order.Season));
        List<MoveOrder> adjacentMoveOrders = moveOrdersByAdjacency[true].ToList();
        List<MoveOrder> nonAdjacentMoveOrders = moveOrdersByAdjacency[false].ToList();

        // Only armies can move to non-adjacent destinations, since fleets cannot be convoyed.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => order.Unit.Type == UnitType.Army,
            ValidationReason.UnreachableDestination,
            ref nonAdjacentMoveOrders,
            ref validationResults);

        // For all remaining convoyable move orders, check if there is a path between the origin
        // and the destination.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => PathFinder.ConvoyPathExists(world, order),
            ValidationReason.UnreachableDestination,
            ref nonAdjacentMoveOrders,
            ref validationResults);

        /*****************
         * CONVOY ORDERS *
         *****************/
        // A convoy order must be to a fleet and target an army.
        List<ConvoyOrder> convoyOrders = unitOrders.OfType<ConvoyOrder>().ToList();
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => order.Unit.Type == UnitType.Fleet && order.Target.Type == UnitType.Army,
            ValidationReason.InvalidOrderTypeForUnit,
            ref convoyOrders,
            ref validationResults);

        // A convoy for an illegal move is illegal, which means all the move validity checks
        // now need to be repeated for the convoy target.

        // Trivial check: cannot convoy to non-coastal province.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => order.Location.Type == LocationType.Land
                && order.Location.Province.Locations.Any(loc => loc.Type == LocationType.Water),
            ValidationReason.IllegalDestinationType,
            ref convoyOrders,
            ref validationResults);

        // Trivial check: cannot convoy a unit to its own location
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => !(
                order.Location == order.Target.Location
                && order.Season == order.Target.Season),
            ValidationReason.DestinationMatchesOrigin,
            ref convoyOrders,
            ref validationResults);

        // By definition, the move enabled by a convoy order is a convoyable move order, so it
        // should be checked for a convoy path.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => PathFinder.ConvoyPathExists(world, order),
            ValidationReason.UnreachableDestination,
            ref convoyOrders,
            ref validationResults);

        /***********************
         * SUPPORT-HOLD ORDERS *
         ***********************/
        // Support-hold orders are typically valid if the supporting unit can move to the
        // destination.
        List<SupportHoldOrder> supportHoldOrders = unitOrders.OfType<SupportHoldOrder>().ToList();

        // Support-hold orders are invalid if the unit supports itself.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => order.Unit != order.Target,
            ValidationReason.NoSelfSupport,
            ref supportHoldOrders,
            ref validationResults);

        // Support-hold orders are invalid if the supporting unit couldn't move to the destination
        // without a convoy. This is the same direct adjacency calculation as above, except that
        // the supporting unit only needs to be able to move to the *province*, even if the target
        // is holding in a location within that province that the supporting unit couldn't move to.
        // The reverse is not true: a unit cannot support another province if that province is only
        // reachable from a different location in the unit's province.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order =>
                // Map adjacency with respect to province
                order.Unit.Location.Adjacents.Any(
                    adjLocation => adjLocation.Province == order.Target.Location.Province)
                // Turn adjacency
                && Math.Abs(order.Unit.Season.Turn - order.Target.Season.Turn) <= 1
                // Timeline adjacency
                && order.Unit.Season.InAdjacentTimeline(order.Target.Season),
            ValidationReason.UnreachableSupport,
            ref supportHoldOrders,
            ref validationResults);

        /***********************
         * SUPPORT-MOVE ORDERS *
         ***********************/
        // Support-move orders, like support-hold orders, are typically valid if the supporting
        // unit can move to the destination.
        List<SupportMoveOrder> supportMoveOrders = unitOrders.OfType<SupportMoveOrder>().ToList();

        // Support-move orders are invalid if the unit supports a move to any location in its own
        // province.
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order => order.Unit.Location.Province != order.Location.Province,
            ValidationReason.NoSupportMoveAgainstSelf,
            ref supportMoveOrders,
            ref validationResults);

        // Support-move orders, like support-hold orders, are valid only if the supporting unit
        // can reach the destination *province* of the move, even if the destination *location*
        // is unreachable (DATC 6.B.4). The same is not true of reachability from another location
        // in the supporting unit's province (DATC 6.B.5).
        AdjudicatorHelpers.InvalidateIfNotMatching(
            order =>
                // Map adjacency with respect to province
                order.Unit.Location.Adjacents.Any(
                    adjLocation => adjLocation.Province == order.Location.Province)
                // Turn adjacency
                && Math.Abs(order.Unit.Season.Turn - order.Season.Turn) <= 1
                // Timeline adjacency
                && order.Unit.Season.InAdjacentTimeline(order.Season),
            ValidationReason.UnreachableSupport,
            ref supportMoveOrders,
            ref validationResults);

        // One more edge case: support-move orders by a fleet for an army are illegal if that army
        // requires a convoy and the supporting fleet is a part of the only convoy path (DATC
        // 6.D.31).
        // TODO: support convoy path check with "as if this fleet were missing"

        // Collect the valid orders together
        unitOrders =
            holdOrders.Cast<UnitOrder>()
            .Concat(adjacentMoveOrders)
            .Concat(nonAdjacentMoveOrders)
            .Concat(convoyOrders)
            .Concat(supportHoldOrders)
            .Concat(supportMoveOrders)
            .ToList();

        // DATC 4.D.3 prefers that multiple orders to the same unit in the same order set be
        // replaced by a hold order. Since this function only takes one combined list of orders,
        // it is assumed that the caller has combined the order sets from all powers in a way that
        // is compliant with DATC 4.D.1-2. If there are still duplicate orders in the input, they
        // were not addressed by 4.D.1-2 and will be handled according to 4.D.3, i.e. replaced with
        // hold orders. Note that this happens last, after all other invalidations have been
        // applied in order to comply with what 4.D.3 specifies about illegal orders.
        List<Unit> duplicateOrderedUnits = unitOrders
            .GroupBy(o => o.Unit)
            .Where(orderGroup => orderGroup.Count() > 1)
            .Select(orderGroup => orderGroup.Key)
            .ToList();
        List<UnitOrder> duplicateOrders = unitOrders
            .Where(o => duplicateOrderedUnits.Contains(o.Unit))
            .ToList();
        List<UnitOrder> validOrders = unitOrders.Except(duplicateOrders).ToList();
        validationResults = validationResults
            .Concat(duplicateOrders.Select(o => o.Invalidate(ValidationReason.DuplicateOrders)))
            .Concat(validOrders.Select(o => o.Validate(ValidationReason.Valid)))
            .ToList();

        // Finally, add implicit hold orders for units without legal orders.
        List<Unit> allOrderableUnits = world.Units
            .Where(unit => !unit.Season.Futures.Any())
            .ToList();
        HashSet<Unit> orderedUnits = validOrders.Select(order => order.Unit).ToHashSet();
        List<Unit> unorderedUnits = allOrderableUnits
            .Where(unit => !orderedUnits.Contains(unit))
            .ToList();
        List<HoldOrder> implicitHolds = unorderedUnits
            .Select(unit => new HoldOrder(unit.Power, unit))
            .ToList();
        validationResults = validationResults
            .Concat(implicitHolds.Select(o => o.Validate(ValidationReason.Valid)))
            .ToList();

        return validationResults;
    }
}
