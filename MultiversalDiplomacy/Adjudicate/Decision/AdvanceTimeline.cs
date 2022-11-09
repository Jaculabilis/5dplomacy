using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class AdvanceTimeline : BinaryAdjudicationDecision
{
    public Season Season { get; }
    public List<UnitOrder> Orders { get; }

    public override string ToString()
        => $"AdvanceTimeline({Season})";

    public AdvanceTimeline(Season season, IEnumerable<UnitOrder> orders)
    {
        this.Season = season;
        this.Orders = orders.ToList();
    }
}