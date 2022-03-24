using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class HoldStrength : NumericAdjudicationDecision
{
    public Province Province { get; }
    public UnitOrder? Order { get; }
    public List<SupportHoldOrder> Supports { get; }

    public HoldStrength(Province province, UnitOrder? order = null)
    {
        this.Province = province;
        this.Order = order;
        this.Supports = new();
    }
}
