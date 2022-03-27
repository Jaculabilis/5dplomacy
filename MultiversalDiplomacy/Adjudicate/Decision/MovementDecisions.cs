using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class MovementDecisions
{
    public Dictionary<Unit, IsDislodged> IsDislodged { get; }
    public Dictionary<MoveOrder, HasPath> HasPath { get; }
    public Dictionary<SupportOrder, GivesSupport> GivesSupport { get; }
    public Dictionary<Province, HoldStrength> HoldStrength { get; }
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

    public MovementDecisions(List<Order> orders)
    {
        this.IsDislodged = new();
        this.HasPath = new();
        this.GivesSupport = new();
        this.HoldStrength = new();
        this.AttackStrength = new();
        this.DefendStrength = new();
        this.PreventStrength = new();
        this.DoesMove = new();

        foreach (UnitOrder order in orders.Cast<UnitOrder>())
        {
            // Create a dislodge decision for this unit.
            List<MoveOrder> incoming = orders
                .OfType<MoveOrder>()
                .Where(move => move.Location.Province == order.Unit.Location.Province)
                .ToList();
            this.IsDislodged[order.Unit] = new(order, incoming);

            // Ensure a hold strength decision exists. Overwrite any previous once, since it may
            // have been created without an order by a previous move or support.
            Province province = order.Unit.Location.Province;
            this.HoldStrength[province] = new(province, order);

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
                    .Where(other => other.Location.Province == move.Location.Province)
                    .ToList();

                // Create the move-related decisions.
                this.HasPath[move] = new(move);
                this.AttackStrength[move] = new(move, supports, opposingMove);
                this.DefendStrength[move] = new(move, supports);
                this.PreventStrength[move] = new(move, supports, opposingMove);
                this.DoesMove[move] = new(move, opposingMove, competing);

                // Ensure a hold strength decision exists for the destination.
                Province dest = move.Location.Province;
                if (!this.HoldStrength.ContainsKey(dest))
                {
                    this.HoldStrength[dest] = new(dest);
                }
            }
            else if (order is SupportOrder support)
            {
                // Create the support decision.
                this.GivesSupport[support] = new(support, incoming);

                // Ensure a hold strength decision exists for the target's province.
                Province target = support.Target.Location.Province;
                if (!this.HoldStrength.ContainsKey(target))
                {
                    this.HoldStrength[target] = new(target);
                }

                if (support is SupportHoldOrder supportHold)
                {
                    this.HoldStrength[target].Supports.Add(supportHold);
                }
                else if (support is SupportMoveOrder supportMove)
                {
                    // Ensure a hold strength decision exists for the target's destination.
                    Province dest = supportMove.Location.Province;
                    if (!this.HoldStrength.ContainsKey(dest))
                    {
                        this.HoldStrength[dest] = new(dest);
                    }
                }
            }
        }
    }
}