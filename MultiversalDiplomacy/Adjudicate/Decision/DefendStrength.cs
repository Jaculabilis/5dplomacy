using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class DefendStrength : NumericAdjudicationDecision
{
    public MoveOrder Order { get; }
    public List<SupportMoveOrder> Supports { get; }

    public override string ToString()
        => $"DefendStrength({Order})";

    public DefendStrength(MoveOrder order, IEnumerable<SupportMoveOrder> supports)
    {
        this.Order = order;
        this.Supports = supports.ToList();
    }
}
