using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class HasPath : BinaryAdjudicationDecision
{
    public MoveOrder Order { get; }

    public override string ToString()
        => $"HasPath({Order})";

    public HasPath(MoveOrder order)
    {
        this.Order = order;
    }
}
