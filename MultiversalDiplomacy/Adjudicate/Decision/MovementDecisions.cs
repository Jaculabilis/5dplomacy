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

    public IEnumerable<AdjudicationDecision> Values =>
        this.IsDislodged.Values.Cast<AdjudicationDecision>()
        .Concat(this.HasPath.Values)
        .Concat(this.GivesSupport.Values)
        .Concat(this.HoldStrength.Values)
        .Concat(this.AttackStrength.Values)
        .Concat(this.DefendStrength.Values)
        .Concat(this.PreventStrength.Values)
        .Concat(this.DoesMove.Values);

    public MovementDecisions(World world, List<Order> orders)
    {
        this.IsDislodged = new();
        this.HasPath = new();
        this.GivesSupport = new();
        this.HoldStrength = new();
        this.AttackStrength = new();
        this.DefendStrength = new();
        this.PreventStrength = new();
        this.DoesMove = new();

        // Record which seasons are referenced by the order set.
        HashSet<Season> orderedSeasons = new();
        foreach (UnitOrder order in orders.Cast<UnitOrder>())
        {
            _ = orderedSeasons.Add(order.Unit.Season);
        }

        // Expand the order list to include any other seasons that are potentially affected.
        // In the event that those seasons don't end up affected (all moves to it fail, all
        // supports to it are cut), it is still safe to re-adjudicate everything because
        // adjudication is deterministic and doesn't produce side effects.
        HashSet<Season> affectedSeasons = new();
        foreach (Order order in orders)
        {
            switch (order)
            {
                case MoveOrder move:
                    if (!orderedSeasons.Contains(move.Season))
                    {
                        affectedSeasons.Add(move.Season);
                    }
                    break;

                case SupportHoldOrder supportHold:
                    if (!orderedSeasons.Contains(supportHold.Target.Season))
                    {
                        affectedSeasons.Add(supportHold.Target.Season);
                    }
                    break;

                case SupportMoveOrder supportMove:
                    if (!orderedSeasons.Contains(supportMove.Target.Season))
                    {
                        affectedSeasons.Add(supportMove.Target.Season);
                    }
                    break;
            }
        }
        foreach (Season season in affectedSeasons)
        {
            orders.AddRange(world.GivenOrders[season]);
        }

        // Create the relevant decisions for each order.
        foreach (UnitOrder order in orders.Cast<UnitOrder>())
        {
            // Create a dislodge decision for this unit.
            List<MoveOrder> incoming = orders
                .OfType<MoveOrder>()
                .Where(move => move.Province == order.Unit.Province)
                .ToList();
            this.IsDislodged[order.Unit] = new(order, incoming);

            // Ensure a hold strength decision exists. Overwrite any previous once, since it may
            // have been created without an order by a previous move or support.
            this.HoldStrength[order.Unit.Point] = new(order.Unit.Point, order);

            if (order is MoveOrder move)
            {
                // Find supports corresponding to this move.
                List<SupportMoveOrder> supports = orders
                    .OfType<SupportMoveOrder>()
                    .Where(support => support.IsSupportFor(move))
                    .ToList();

                // Determine if this move is a head-to-head battle.
                MoveOrder? opposingMove = orders
                    .OfType<MoveOrder>()
                    .FirstOrDefault(other => other != null && other.IsOpposing(move), null);

                // Find competing moves.
                List<MoveOrder> competing = orders
                    .OfType<MoveOrder>()
                    .Where(other
                        => other != move
                        && other.Province == move.Province)
                    .ToList();

                // Create the move-related decisions.
                this.HasPath[move] = new(move);
                this.AttackStrength[move] = new(move, supports, opposingMove);
                this.DefendStrength[move] = new(move, supports);
                this.PreventStrength[move] = new(move, supports, opposingMove);
                this.DoesMove[move] = new(move, opposingMove, competing);

                // Ensure a hold strength decision exists for the destination.
                if (!this.HoldStrength.ContainsKey(move.Point))
                {
                    this.HoldStrength[move.Point] = new(move.Point);
                }
            }
            else if (order is SupportOrder support)
            {
                // Create the support decision.
                this.GivesSupport[support] = new(support, incoming);

                // Ensure a hold strength decision exists for the target's province.
                if (!this.HoldStrength.ContainsKey(support.Target.Point))
                {
                    this.HoldStrength[support.Target.Point] = new(support.Target.Point);
                }

                if (support is SupportHoldOrder supportHold)
                {
                    this.HoldStrength[support.Target.Point].Supports.Add(supportHold);
                }
                else if (support is SupportMoveOrder supportMove)
                {
                    // Ensure a hold strength decision exists for the target's destination.
                    if (!this.HoldStrength.ContainsKey(supportMove.Point))
                    {
                        this.HoldStrength[supportMove.Point] = new(supportMove.Point);
                    }
                }
            }
        }
    }
}