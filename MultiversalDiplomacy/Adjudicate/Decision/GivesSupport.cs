using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class GivesSupport : BinaryAdjudicationDecision
{
    public SupportOrder Order { get; }
    public List<MoveOrder> Cuts { get; }

    public override string ToString()
        => $"GivesSupport({Order})";

    public GivesSupport(SupportOrder order, IEnumerable<MoveOrder> cuts)
    {
        this.Order = order;
        this.Cuts = cuts.ToList();
    }
}
