using System.Collections.ObjectModel;

using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Adjudicate.Logging;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Adjudicator for the movement phase.
/// </summary>
public class MovementPhaseAdjudicator : IPhaseAdjudicator
{
    public static IPhaseAdjudicator Instance { get; } = new MovementPhaseAdjudicator(ConsoleLogger.Instance);

    private IAdjudicatorLogger logger { get; }

    public MovementPhaseAdjudicator(IAdjudicatorLogger logger)
    {
        this.logger = logger;
    }

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
        List<UnitOrder> unitOrders = orders.Cast<UnitOrder>().ToList();

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
                && order.Province.Locations.Any(loc => loc.Type == LocationType.Water),
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
                    adjLocation => adjLocation.Province == order.Target.Province)
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
            order => order.Unit.Province != order.Province,
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
                    adjLocation => adjLocation.Province == order.Province)
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

    public List<AdjudicationDecision> AdjudicateOrders(World world, List<Order> orders)
    {
        logger.Log(0, "Beginning adjudication");
        // Define all adjudication decisions to be made.
        MovementDecisions decisions = new(world, orders);

        // Adjudicate all decisions.
        bool progress = false;
        int loopNum = 1;
        do
        {
            logger.Log(1, "Beginning loop {0}", loopNum++);
            progress = false;
            foreach (AdjudicationDecision decision in decisions.Values)
            {
                // This will noop without progress if the decision is already resolved
                progress |= ResolveDecision(decision, world, decisions, depth: 2);
            }
        } while (progress && decisions.Values.Any(decision => !decision.Resolved));

        if (decisions.Values.Any(d => !d.Resolved))
        {
            throw new ApplicationException("Some orders not resolved!");
        }
        logger.Log(0, "Completed adjudication");

        return decisions.Values.ToList();
    }

    public World UpdateWorld(World world, List<AdjudicationDecision> decisions)
    {
        logger.Log(0, "Updating world");
        Dictionary<MoveOrder, DoesMove> moves = decisions
            .OfType<DoesMove>()
            .ToDictionary(dm => dm.Order);
        Dictionary<Unit, IsDislodged> dislodges = decisions
            .OfType<IsDislodged>()
            .ToDictionary(dm => dm.Order.Unit);

        // All moves to a particular season in a single phase result in the same future. Keep a
        // record of when a future season has been created.
        Dictionary<Season, Season> createdFutures = new();
        List<Unit> createdUnits = new();
        List<RetreatingUnit> retreats = new();

        // Populate createdFutures with the timeline fork decisions
        logger.Log(1, "Processing AdvanceTimeline decisions");
        foreach (AdvanceTimeline advanceTimeline in decisions.OfType<AdvanceTimeline>())
        {
            logger.Log(2, "{0} = {1}", advanceTimeline, advanceTimeline.Outcome?.ToString() ?? "?");
            if (advanceTimeline.Outcome == true)
            {
                // A timeline that doesn't have a future yet simply continues. Otherwise, it forks.
                createdFutures[advanceTimeline.Season] = !advanceTimeline.Season.Futures.Any()
                    ? advanceTimeline.Season.MakeNext()
                    : advanceTimeline.Season.MakeFork();
            }
        }

        // Successful move orders result in the unit moving to the destination and creating a new
        // future, while unsuccessful move orders are processed the same way as non-move orders.
        logger.Log(1, "Processing successful moves");
        foreach (DoesMove doesMove in moves.Values)
        {
            logger.Log(2, "{0} = {1}", doesMove, doesMove.Outcome?.ToString() ?? "?");
            Season moveSeason = doesMove.Order.Season;
            if (doesMove.Outcome == true && createdFutures.ContainsKey(moveSeason))
            {
                Unit next = doesMove.Order.Unit.Next(doesMove.Order.Location, createdFutures[moveSeason]);
                logger.Log(3, "Advancing unit to {0}", next);
                createdUnits.Add(next);
            }
        }

        // Process unsuccessful moves, all holds, and all supports.
        logger.Log(1, "Processing stationary orders");
        foreach (IsDislodged isDislodged in decisions.OfType<IsDislodged>())
        {
            UnitOrder order = isDislodged.Order;

            // Skip the move orders that were processed above.
            if (order is MoveOrder move && moves[move].Outcome == true)
            {
                continue;
            }

            logger.Log(2, "{0} = {1}", isDislodged, isDislodged.Outcome?.ToString() ?? "?");
            if (!createdFutures.ContainsKey(order.Unit.Season))
            {
                logger.Log(3, "Skipping order because no future was created");
                continue;
            }

            Season future = createdFutures[order.Unit.Season];
            if (isDislodged.Outcome == false)
            {
                // Non-dislodged units continue into the future.
                Unit next = order.Unit.Next(order.Unit.Location, future);
                logger.Log(3, "Advancing unit to {0}", next);
                createdUnits.Add(next);
            }
            else
            {
                // Create a retreat for each dislodged unit.
                // TODO check valid retreats and disbands
                logger.Log(3, "Creating retreat for {0}", order.Unit);
                var validRetreats = order.Unit.Location.Adjacents
                    .Select(loc => (future, loc))
                    .ToList();
                RetreatingUnit retreat = new(order.Unit, validRetreats);
                retreats.Add(retreat);
            }
        }

        // Record the adjudication results to the season's order history
        Dictionary<Season, OrderHistory> newHistory = new();
        foreach (UnitOrder unitOrder in decisions.OfType<IsDislodged>().Select(d => d.Order))
        {
            newHistory.Ensure(unitOrder.Unit.Season, () => new());
            OrderHistory history = newHistory[unitOrder.Unit.Season];
            // TODO does this add every order to every season??
            history.Orders.Add(unitOrder);
            history.IsDislodgedOutcomes[unitOrder.Unit] = dislodges[unitOrder.Unit].Outcome == true;
            if (unitOrder is MoveOrder moveOrder)
            {
                history.DoesMoveOutcomes[moveOrder] = moves[moveOrder].Outcome == true;
            }
        }

        // Log the new order history
        foreach ((Season season, OrderHistory history) in newHistory)
        {
            string verb = world.OrderHistory.ContainsKey(season) ? "Updating" : "Adding";
            logger.Log(1, "{0} history for {1}", verb, season);
            foreach (UnitOrder order in history.Orders)
            {
                logger.Log(2, "{0}", order);
            }
        }

        IEnumerable<KeyValuePair<Season, OrderHistory>> updatedHistory = world.OrderHistory
            .Where(kvp => !newHistory.ContainsKey(kvp.Key))
            .Concat(newHistory);

        // TODO provide more structured information about order outcomes

        World updated = world.Update(
            seasons: world.Seasons.Concat(createdFutures.Values),
            units: world.Units.Concat(createdUnits),
            retreats: retreats,
            orders: updatedHistory);

        logger.Log(0, "Completed update");

        return updated;
    }

    private bool LoggedUpdate(BinaryAdjudicationDecision decision, bool outcome, int depth, string message)
    {
        bool updated = decision.Update(outcome);
        if (updated)
        {
            logger.Log(depth, "{0}: {1}", outcome, message);
        }
        return updated;
    }

    private bool LoggedUpdate(NumericAdjudicationDecision decision, int min, int max, int depth, string message)
    {
        bool updated = decision.Update(min, max);
        if (updated)
        {
            logger.Log(depth, "{0}, {1}: {2}", min, max, message);
        }
        return updated;
    }

    /// <summary>
    /// Attempt to resolve an adjudication decision.
    /// </summary>
    /// <returns>
    /// If any adjudication was further determined, returns true. If nothing was further determined, returns false.
    /// </returns>
    private bool ResolveDecision(
        AdjudicationDecision decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "ResolveDecision({0})", decision);
        return decision.Resolved ? false : decision switch
        {
            AdvanceTimeline d => ResolveAdvanceTimeline(d, world, decisions, depth + 1),
            IsDislodged d => ResolveIsUnitDislodged(d, world, decisions, depth + 1),
            HasPath d => ResolveDoesMoveHavePath(d, world, decisions, depth + 1),
            GivesSupport d => ResolveIsSupportGiven(d, world, decisions, depth + 1),
            HoldStrength d => ResolveHoldStrength(d, world, decisions, depth + 1),
            AttackStrength d => ResolveAttackStrength(d, world, decisions, depth + 1),
            DefendStrength d => ResolveDefendStrength(d, world, decisions, depth + 1),
            PreventStrength d => ResolvePreventStrength(d, world, decisions, depth + 1),
            DoesMove d => ResolveDoesUnitMove(d, world, decisions, depth + 1),
            _ => throw new NotSupportedException($"Unknown decision type: {decision.GetType()}")
        };
    }

    private bool ResolveAdvanceTimeline(
        AdvanceTimeline decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "AdvanceTimeline({0})", decision.Season);
        bool progress = false;

        // A season at the head of a timeline always advances.
        if (!decision.Season.Futures.Any())
        {
            progress |= LoggedUpdate(decision, true, depth, "A timeline head always advances");
            return progress;
        }

        // The season target of a successful move always advances.
        IEnumerable<MoveOrder> incomingMoveOrders = decision.Orders
            .OfType<MoveOrder>()
            .Where(order => order.Season == decision.Season);
        logger.Log(depth, "decision.Orders = {0}", string.Join(", ", decision.Orders));
        logger.Log(depth, "incomingMoveOrders = {0}", string.Join(", ", incomingMoveOrders));
        foreach (MoveOrder moveOrder in incomingMoveOrders)
        {
            DoesMove doesMove = decisions.DoesMove[moveOrder];
            progress |= ResolveDecision(doesMove, world, decisions, depth + 1);
            if (doesMove.Outcome == true)
            {
                progress |= LoggedUpdate(decision, true, depth, $"Advanced by {doesMove.Order}");
                return progress;
            }
        }

        // TODO needs to also resolve true if anyone moves into this timeline

        // Seasons not at the head of a timeline advance if the outcome of a battle is changed.
        // The outcome of a battle is changed if:
        // 1. The outcome of a dislodge decision is changed,
        // 2. The outcome of an intra-timeline move decision is changed, or
        // 3. The outcome of an inter-timeline move decision with that season as the destination is
        //    changed.
        OrderHistory history = world.OrderHistory[decision.Season];
        bool anyUnresolved = false;
        foreach (UnitOrder order in decision.Orders)
        {
            // TODO these aren't timeline-specific
            IsDislodged dislodged = decisions.IsDislodged[order.Unit];
            progress |= ResolveDecision(dislodged, world, decisions, depth + 1);
            bool previous = history.IsDislodgedOutcomes[order.Unit];
            if (dislodged.Resolved && dislodged.Outcome != previous)
            {
                progress |= LoggedUpdate(
                    decision,
                    true,
                    depth,
                    $"History changed for {order.Unit}: dislodge {previous} => {dislodged.Outcome}");
                return progress;
            }
            anyUnresolved |= !dislodged.Resolved;

            if (order is MoveOrder moveOrder)
            {
                DoesMove moves = decisions.DoesMove[moveOrder];
                progress |= ResolveDecision(moves, world, decisions, depth + 1);
                bool previousMove = history.DoesMoveOutcomes[moveOrder];
                if (moves.Resolved && moves.Outcome != previousMove)
                {
                    progress |= LoggedUpdate(
                        decision,
                        true,
                        depth,
                        $"History changed for {order}: moves {previousMove} => {moves.Outcome}");
                    return progress;
                }
                anyUnresolved |= !moves.Resolved;
            }
        }

        if (!anyUnresolved)
        {
            progress |= LoggedUpdate(decision, false, depth, "No resolved changes to history");
        }

        return progress;
    }

    private bool ResolveIsUnitDislodged(
        IsDislodged decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "IsUnitDislodged({0})", decision.Order.Unit);
        bool progress = false;

        // If this unit was ordered to move and is doing so successfully, it cannot be dislodged
        // even if another unit will successfully move into the province.
        if (decision.Order is MoveOrder moveOrder)
        {
            DoesMove move = decisions.DoesMove[moveOrder];
            progress |= ResolveDecision(move, world, decisions, depth + 1);

            // If this unit received a move order and the move is successful, it cannot be
            // dislodged.
            if (move.Outcome == true)
            {
                progress |= LoggedUpdate(decision, false, depth, "Unit successfully moves");
                return progress;
            }

            // If the move is undecided, then the dislodge decision is undecidable until then.
            if (move.Outcome == null)
            {
                return progress;
            }
        }

        // If this unit isn't moving from its current province, then it is dislodged if another
        // unit has a successful move into its province, and it is not dislodged if every unit that
        // could move into its province fails to do so.
        bool potentialDislodger = false;
        foreach (MoveOrder dislodger in decision.Incoming)
        {
            DoesMove move = decisions.DoesMove[dislodger];
            progress |= ResolveDecision(move, world, decisions, depth + 1);

            // If at least one invader will move, this unit is dislodged.
            if (move.Outcome == true)
            {
                progress |= LoggedUpdate(decision, true, depth, "Invading unit successfully moves");
                return progress;
            }

            // If the invader could potentially move, the dislodge decision can't be resolved to
            // false.
            if (move.Outcome != false)
            {
                potentialDislodger = true;
            }
        }

        if (!potentialDislodger)
        {
            progress |= LoggedUpdate(decision, false, depth, "No invader can move");
        }

        return progress;
    }

    private bool ResolveDoesMoveHavePath(
        HasPath decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "DoesMoveHavePath({0})", decision.Order);
        bool progress= false;

        // If the origin and destination are adjacent, then there is a path.
        if (// Map adjacency
            decision.Order.Unit.Location.Adjacents.Contains(decision.Order.Location)
            // Turn adjacency
            && Math.Abs(decision.Order.Unit.Season.Turn - decision.Order.Season.Turn) <= 1
            // Timeline adjacency
            && decision.Order.Unit.Season.InAdjacentTimeline(decision.Order.Season))
        {
            bool update = LoggedUpdate(decision, true, depth, "Adjacent move");
            return progress | update;
        }

        // If the origin and destination are not adjacent, then the decision resolves to whether
        // there is a path of convoying fleets that (1) have matching orders and (2) are not
        // dislodged.

        // The adjudicator should have received a validated set of orders, so any illegal move
        // with no possible convoy path should have been invalidated.

        throw new NotImplementedException(); // TODO
    }

    private bool ResolveIsSupportGiven(
        GivesSupport decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "IsSupportGiven({0})", decision.Order);
        bool progress = false;

        // Support is cut when a unit moves into the supporting unit's province with nonzero
        // attack strength. Support is given when there is known to be no such unit.
        bool potentialNonzeroAttack = false;
        foreach (MoveOrder cut in decision.Cuts)
        {
            AttackStrength attack = decisions.AttackStrength[cut];
            progress |= ResolveDecision(attack, world, decisions, depth + 1);

            // If at least one attack has a nonzero minimum, the support decision can be resolved
            // to false.
            if (attack.MinValue > 0)
            {
                progress |= LoggedUpdate(decision, false, depth, "An attacker has nonzero attack strength");
                return progress;
            }

            // If at least one attack has a nonzero maximum, the support decision can't be resolved
            // to true.
            if (attack.MaxValue > 0)
            {
                potentialNonzeroAttack = true;
            }
        }

        // Support is also cut if the unit is dislodged.
        IsDislodged dislodge = decisions.IsDislodged[decision.Order.Unit];
        progress |= ResolveDecision(dislodge, world, decisions, depth + 1);
        if (dislodge.Outcome == true)
        {
            progress |= LoggedUpdate(decision, false, depth, "Unit dislodged");
            return progress;
        }

        // If no attack has potentially nonzero attack strength, and the dislodge decision is
        // resolved to false, then the support is given.
        if (!potentialNonzeroAttack && dislodge.Outcome == false)
        {
            progress |= LoggedUpdate(decision, true, depth, "No successful attack or dislodge");
            return progress;
        }

        // Otherwise, the support remains undecided.
        return progress;
    }

    private bool ResolveHoldStrength(
        HoldStrength decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "HoldStrength({0})", decision.Province);
        bool progress = false;

        // If no unit is in the province, the hold strength is zero.
        if (decision.Order == null)
        {
            progress |= LoggedUpdate(decision, 0, 0, depth, "No unit in the province");
            return progress;
        }

        // If a unit with a move order is in the province, the strength depends on the move success.
        if (decision.Order is MoveOrder move)
        {
            DoesMove moves = decisions.DoesMove[move];
            progress |= ResolveDecision(moves, world, decisions, depth + 1);
            progress |= LoggedUpdate(
                decision,
                min: moves.Outcome != false ? 0 : 1,
                max: moves.Outcome == true ? 0 : 1,
                depth,
                "Updated based on unit's move success");
            return progress;
        }
        // If a unit without a move order is in the province, add up the supports.
        else
        {
            int min = 1;
            int max = 1;
            foreach (SupportHoldOrder support in decision.Supports)
            {
                GivesSupport givesSupport = decisions.GivesSupport[support];
                progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
                if (givesSupport.Outcome == true) min += 1;
                if (givesSupport.Outcome != false) max += 1;
            }
            progress |= LoggedUpdate(decision, min, max, depth, "Updated based on unit's supports");
            return progress;
        }
    }

    private bool ResolveAttackStrength(
        AttackStrength decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "AttackStrength({0})", decision.Order);
        bool progress = false;

        // If there is no path, the attack strength is zero.
        var hasPath = decisions.HasPath[decision.Order];
        progress |= ResolveDecision(hasPath, world, decisions, depth + 1);
        if (hasPath.Outcome == false)
        {
            progress |= LoggedUpdate(decision, 0, 0, depth, "No path");
            return progress;
        }

        // If there is a head to head battle, a unit at the destination that isn't moving away, or
        // a unit at the destination that will fail to move away, then the attacking unit will have
        // to dislodge it.
        UnitOrder? destOrder = decisions.HoldStrength[decision.Order.Point].Order;
        DoesMove? destMoveAway = destOrder is MoveOrder moveAway
            ? decisions.DoesMove[moveAway]
            : null;
        if (destMoveAway != null)
        {
            progress |= ResolveDecision(destMoveAway, world, decisions, depth + 1);
        }
        if (// In any case here, there will have to be a unit at the destination with an order,
            // which means that destOrder will have to be populated. Including this in the if
            //condition lets the compiler know it won't be null in the if block.
            destOrder != null
            && (// Is head to head
                decision.OpposingMove != null
                // Is not moving away
                || destMoveAway == null
                // Is failing to move away
                || destMoveAway.Outcome == false))
        {
            Power destPower = destOrder.Unit.Power;
            if (decision.Order.Unit.Power == destPower)
            {
                // Cannot dislodge own unit.
                progress |= LoggedUpdate(decision, 0, 0, depth, "Cannot dislodge own unit");
                return progress;
            }
            else
            {
                // Supports won't help to dislodge units of the same power as the support.
                int min = 1;
                int max = 1;
                foreach (SupportMoveOrder support in decision.Supports)
                {
                    if (support.Unit.Power == destPower) continue;
                    GivesSupport givesSupport = decisions.GivesSupport[support];
                    progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
                    if (givesSupport.Outcome == true) min += 1;
                    if (givesSupport.Outcome != false) max += 1;
                }
                progress |= LoggedUpdate(decision, min, max, depth, "Updated with supports from other powers");
                return progress;
            }
        }
        else if (destMoveAway != null && destMoveAway.Outcome == null)
        {
            // If the unit at the destination has an undecided move order, then the minimum tracks
            // the case where it doesn't move and the attack strength is mitigated by supports not
            // helping to dislodge units of the same power as the support. The maximum tracks the
            // case where it does move and the attack strength is unmitigated.
            Power destPower = destMoveAway.Order.Unit.Power;
            int min = 1;
            int max = 1;
            foreach (SupportMoveOrder support in decision.Supports)
            {
                GivesSupport givesSupport = decisions.GivesSupport[support];
                progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
                if (support.Unit.Power != destPower && givesSupport.Outcome == true) min += 1;
                if (givesSupport.Outcome != false) max += 1;
            }
            // Force min to zero in case of an attempt to disloge a unit of the same power.
            if (decision.Order.Unit.Power == destPower) min = 0;
            progress |= LoggedUpdate(decision, min, max, depth, "Updated with supports");
            return progress;
        }
        else
        {
            // If the unit at the destination is going somewhere else, then attack strength
            // includes all supports from all powers.
            int min = 1;
            int max = 1;
            foreach (SupportMoveOrder support in decision.Supports)
            {
                GivesSupport givesSupport = decisions.GivesSupport[support];
                progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
                if (givesSupport.Outcome == true) min += 1;
                if (givesSupport.Outcome != false) max += 1;
            }
            progress |= LoggedUpdate(decision, min, max, depth, "Updated with supports from all powers");
            return progress;
        }
    }

    private bool ResolveDefendStrength(
        DefendStrength decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "DefendStrength({0})", decision.Order);
        bool progress = false;

        // The defend strength is equal to one plus, at least, the number of known successful
        // supports, and at most, also the unresolved supports were they to resolve to successes.
        int min = 1;
        int max = 1;
        foreach (SupportMoveOrder support in decision.Supports)
        {
            GivesSupport givesSupport = decisions.GivesSupport[support];
            progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
            if (givesSupport.Outcome == true) min += 1;
            if (givesSupport.Outcome != false) max += 1;
        }
        progress |= LoggedUpdate(decision, min, max, depth, "Updated based on unit's supports");

        return progress;
    }

    private bool ResolvePreventStrength(
        PreventStrength decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "PreventStrength({0})", decision.Order);
        bool progress = false;

        // If there is no path, the prevent strength is zero.
        var hasPath = decisions.HasPath[decision.Order];
        progress |= ResolveDecision(hasPath, world, decisions, depth + 1);
        if (hasPath.Outcome == false)
        {
            progress |= LoggedUpdate(decision, 0, 0, depth, "No path to prevent");
            return progress;
        }

        // If there's a head to head battle and the opposing unit succeeds in moving, the prevent
        // strength is zero.
        if (decision.OpposingMove != null
            && decisions.DoesMove[decision.OpposingMove].Outcome == true)
        {
            progress |= LoggedUpdate(decision, 0, 0, depth, "Cannot prevent in lost head-to-head");
            return progress;
        }

        // In all other cases, the prevent strength is equal to one plus, at least, the number of
        // known successful supports, and at most, also the unresolved supports were they to
        // resolve to successes.
        int min = 1;
        int max = 1;
        foreach (SupportMoveOrder support in decision.Supports)
        {
            GivesSupport givesSupport = decisions.GivesSupport[support];
            progress |= ResolveDecision(givesSupport, world, decisions, depth + 1);
            if (givesSupport.Outcome == true) min += 1;
            if (givesSupport.Outcome != false) max += 1;
        }

        // The minimum stays at zero if the path or head to head move decisions are unresolved, as
        // they may resolve to one of the conditions above that forces the prevent strength to zero.
        if (!hasPath.Resolved
            || (decision.OpposingMove != null
                && !decisions.DoesMove[decision.OpposingMove].Resolved))
        {
            min = 0;
        }

        progress |= LoggedUpdate(decision, min, max, depth, "Updated based on unit's supports");

        return progress;
    }

    private bool ResolveDoesUnitMove(
        DoesMove decision,
        World world,
        MovementDecisions decisions,
        int depth)
    {
        logger.Log(depth, "DoesUnitMove({0})", decision.Order);
        bool progress = false;

        // Resolve the move's attack strength.
        AttackStrength attack = decisions.AttackStrength[decision.Order];
        progress |= ResolveDecision(attack, world, decisions, depth + 1);

        // In a head to head battle, the threshold for the attack strength to beat is the opposing
        // defend strength. Outside a head to head battle, the threshold is the destination's hold
        // strength.
        NumericAdjudicationDecision defense = decision.OpposingMove != null
            ? decisions.DefendStrength[decision.OpposingMove]
            : decisions.HoldStrength[decision.Order.Point];
        progress |= ResolveDecision(defense, world, decisions, depth + 1);

        // If the attack doesn't beat the defense, resolve the move to false.
        if (attack.MaxValue <= defense.MinValue)
        {
            progress |= LoggedUpdate(decision, false, depth, "Attack can't beat defense");
            return progress;
        }

        // Check if a competing move will prevent this one.
        bool beatsAllCompetingMoves = true;
        foreach (MoveOrder order in decision.Competing)
        {
            PreventStrength prevent = decisions.PreventStrength[order];
            progress |= ResolveDecision(prevent, world, decisions, depth + 1);
            // If attack doesn't beat the prevent, resolve the move to false.
            if (attack.MaxValue <= prevent.MinValue)
            {
                progress |= LoggedUpdate(decision, false, depth, "Attack can't beat prevent");
                return progress;
            }
            // If the attack doesn't beat the prevent, it can't resolve to true.
            if (attack.MinValue <= prevent.MaxValue)
            {
                beatsAllCompetingMoves = false;
            }
        }

        // If the attack didn't resolve to false because the defense or a prevent beat it, then
        // attempt to resolve it to true based on whether it beat the defense and all prevents.
        progress |= LoggedUpdate(
            decision,
            attack.MinValue > defense.MaxValue && beatsAllCompetingMoves,
            depth,
            "Updated based on competing moves");
        return progress;
    }
}
