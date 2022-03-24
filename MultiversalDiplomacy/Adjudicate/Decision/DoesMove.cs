using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class DoesMove : BinaryAdjudicationDecision
{
    public MoveOrder Order { get; }
    public MoveOrder? OpposingMove { get; }
    public List<MoveOrder> Competing { get; }

    public DoesMove(MoveOrder order, MoveOrder? opposingMove, IEnumerable<MoveOrder> competing)
    {
        this.Order = order;
        this.OpposingMove = opposingMove;
        this.Competing = competing.ToList();
    }
}
