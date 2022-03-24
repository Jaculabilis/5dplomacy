using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class AttackStrength : NumericAdjudicationDecision
{
    public MoveOrder Order { get; }
    public List<SupportMoveOrder> Supports { get; }
    public MoveOrder? OpposingMove { get; }

    public AttackStrength(MoveOrder order, IEnumerable<SupportMoveOrder> supports, MoveOrder? opposingMove = null)
    {
        this.Order = order;
        this.Supports = supports.ToList();
        this.OpposingMove = opposingMove;
    }
}
