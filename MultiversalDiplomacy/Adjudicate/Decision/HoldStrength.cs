using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class HoldStrength : NumericAdjudicationDecision
{
    public Province Province { get; }
    public Season Season { get; }
    public UnitOrder? Order { get; }
    public List<SupportHoldOrder> Supports { get; }

    public override string ToString()
        => Order is null
        ? $"HoldStrength({Province.Abbreviations[0]})"
        : $"HoldStrength({Order.Unit})";

    public HoldStrength(Province province, Season season, UnitOrder? order = null)
    {
        this.Province = province;
        this.Season = season;
        this.Order = order;
        this.Supports = new();
    }

    public HoldStrength((Province province, Season season) point, UnitOrder? order = null)
        : this(point.province, point.season, order)
    {
    }
}
