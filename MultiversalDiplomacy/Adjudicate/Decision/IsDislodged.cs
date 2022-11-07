using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class IsDislodged : BinaryAdjudicationDecision
{
    public UnitOrder Order { get; }
    public List<MoveOrder> Incoming { get; }

    public override string ToString()
        => $"IsDislodged({Order.Unit})";

    public IsDislodged(UnitOrder order, IEnumerable<MoveOrder> incoming)
    {
        this.Order = order;
        this.Incoming = incoming.ToList();
    }
}
