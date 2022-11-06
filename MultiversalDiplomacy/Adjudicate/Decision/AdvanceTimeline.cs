using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate.Decision;

public class AdvanceTimeline : BinaryAdjudicationDecision
{
    public Season Season { get; }
    public List<UnitOrder> Orders { get; }

    public AdvanceTimeline(Season season, IEnumerable<UnitOrder> orders)
    {
        this.Season = season;
        this.Orders = orders.ToList();
    }
}