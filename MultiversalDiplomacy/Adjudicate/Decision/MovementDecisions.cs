using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class MovementDecisions
{
    public Dictionary<Unit, IsDislodged> IsDislodged { get; }
    public Dictionary<MoveOrder, HasPath> HasPath { get; }
    public Dictionary<SupportOrder, GivesSupport> GivesSupport { get; }
    public Dictionary<(Province, Season), HoldStrength> HoldStrength { get; }
    public Dictionary<MoveOrder, AttackStrength> AttackStrength { get; }
    public Dictionary<MoveOrder, DefendStrength> DefendStrength { get; }
    public Dictionary<MoveOrder, PreventStrength> PreventStrength { get; }
    public Dictionary<MoveOrder, DoesMove> DoesMove { get; }
    public Dictionary<Season, AdvanceTimeline> AdvanceTimeline { get; }

    public IEnumerable<AdjudicationDecision> Values =>
        IsDislodged.Values.Cast<AdjudicationDecision>()
        .Concat(HasPath.Values)
        .Concat(GivesSupport.Values)
        .Concat(HoldStrength.Values)
        .Concat(AttackStrength.Values)
        .Concat(DefendStrength.Values)
        .Concat(PreventStrength.Values)
        .Concat(DoesMove.Values)
        .Concat(AdvanceTimeline.Values);

    public MovementDecisions(World world, List<Order> orders)
    {
        IsDislodged = new();
        HasPath = new();
        GivesSupport = new();
        HoldStrength = new();
        AttackStrength = new();
        DefendStrength = new();
        PreventStrength = new();
        DoesMove = new();
        AdvanceTimeline = new();

        // The orders argument only contains the submitted orders. The adjudicator will need to adjudicate not only
        // presently submitted orders, but also previously submitted orders if present orders affect the past. This
        // necessitates doing some lookups to find all affected seasons.

        // At a minimum, the submitted orders imply a dislodge decision for each unit, which affects every season those
        // orders were given to.
        var submittedOrdersBySeason = orders.Cast<UnitOrder>().ToLookup(order => order.Unit.Season);
        foreach (var group in submittedOrdersBySeason)
        {
            AdvanceTimeline[group.Key] = new(group.Key, group);
        }

        // Create timeline decisions for each season potentially affected by the submitted orders.
        // Since adjudication is deterministic and pure, if none of the affecting orders succeed,
        // the adjudication decisions for the extra seasons will resolve the same way and the
        // advance decision for the timeline will resolve false.
        foreach (Order order in orders)
        {
            switch (order)
            {
                case MoveOrder move:
                    AdvanceTimeline.Ensure(
                        move.Season,
                        () => new(move.Season, world.OrderHistory[move.Season].Orders));
                    AdvanceTimeline[move.Season].Orders.Add(move);
                    break;

                case SupportHoldOrder supportHold:
                    AdvanceTimeline.Ensure(
                        supportHold.Target.Season,
                        () => new(supportHold.Target.Season, world.OrderHistory[supportHold.Target.Season].Orders));
                    AdvanceTimeline[supportHold.Target.Season].Orders.Add(supportHold);
                    break;

                case SupportMoveOrder supportMove:
                    AdvanceTimeline.Ensure(
                        supportMove.Target.Season,
                        () => new(supportMove.Target.Season, world.OrderHistory[supportMove.Target.Season].Orders));
                    AdvanceTimeline[supportMove.Target.Season].Orders.Add(supportMove);
                    AdvanceTimeline.Ensure(
                        supportMove.Season,
                        () => new(supportMove.Season, world.OrderHistory[supportMove.Season].Orders));
                    AdvanceTimeline[supportMove.Season].Orders.Add(supportMove);
                    break;
            }
        }

        // Get the orders in the affected timelines.
        List<UnitOrder> relevantOrders = AdvanceTimeline.Values
            .SelectMany(at => at.Orders)
            .Distinct()
            .ToList();

        // Create a hold strength decision with an associated order for every province with a unit.
        foreach (UnitOrder order in relevantOrders)
        {
            HoldStrength[order.Unit.Point] = new(order.Unit.Point, order);
        }

        // Create all other relevant decisions for each order in the affected timelines.
        foreach (UnitOrder order in relevantOrders)
        {
            // Create a dislodge decision for this unit.
            List<MoveOrder> incoming = relevantOrders
                .OfType<MoveOrder>()
                .Where(order.IsIncoming)
                .ToList();
            IsDislodged[order.Unit] = new(order, incoming);

            if (order is MoveOrder move)
            {
                // Find supports corresponding to this move.
                List<SupportMoveOrder> supports = relevantOrders
                    .OfType<SupportMoveOrder>()
                    .Where(support => support.IsSupportFor(move))
                    .ToList();

                // Determine if this move is a head-to-head battle.
                MoveOrder? opposingMove = relevantOrders
                    .OfType<MoveOrder>()
                    .FirstOrDefault(other => other!.IsOpposing(move), null);

                // Find competing moves.
                List<MoveOrder> competing = relevantOrders
                    .OfType<MoveOrder>()
                    .Where(move.IsCompeting)
                    .ToList();

                // Create the move-related decisions.
                HasPath[move] = new(move);
                AttackStrength[move] = new(move, supports, opposingMove);
                DefendStrength[move] = new(move, supports);
                PreventStrength[move] = new(move, supports, opposingMove);
                DoesMove[move] = new(move, opposingMove, competing);

                // Ensure a hold strength decision exists for the destination.
                HoldStrength.Ensure(move.Point, () => new(move.Point));
            }
            else if (order is SupportOrder support)
            {
                // Create the support decision.
                GivesSupport[support] = new(support, incoming);

                // Ensure a hold strength decision exists for the target's province.
                HoldStrength.Ensure(support.Target.Point, () => new(support.Target.Point));

                if (support is SupportHoldOrder supportHold)
                {
                    HoldStrength[support.Target.Point].Supports.Add(supportHold);
                }
                else if (support is SupportMoveOrder supportMove)
                {
                    // Ensure a hold strength decision exists for the target's destination.
                    HoldStrength.Ensure(supportMove.Point, () => new(supportMove.Point));
                }
            }
        }
    }
}