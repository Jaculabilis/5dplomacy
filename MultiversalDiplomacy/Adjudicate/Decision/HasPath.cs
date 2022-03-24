using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class HasPath : BinaryAdjudicationDecision
{
    public MoveOrder Order { get; }

    public HasPath(MoveOrder order)
    {
        this.Order = order;
    }
}
