using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class PreventStrength : NumericAdjudicationDecision
{
    public MoveOrder Order { get; }
    public List<SupportMoveOrder> Supports { get; }
    public MoveOrder? OpposingMove { get; }

    public PreventStrength(MoveOrder order, IEnumerable<SupportMoveOrder> supports, MoveOrder? opposingMove = null)
    {
        this.Order = order;
        this.Supports = supports.ToList();
        this.OpposingMove = opposingMove;
    }
}
