using System.Collections.ObjectModel;

using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Model;

public class OrderHistory
{
    public List<UnitOrder> Orders;

    public Dictionary<Unit, bool> IsDislodgedOutcomes;

    public Dictionary<MoveOrder, bool> DoesMoveOutcomes;

    public OrderHistory()
        : this(new(), new(), new())
    {}

    public OrderHistory(
        List<UnitOrder> orders,
        Dictionary<Unit, bool> isDislodgedOutcomes,
        Dictionary<MoveOrder, bool> doesMoveOutcomes)
    {
        this.Orders = new(orders);
        this.IsDislodgedOutcomes = new(isDislodgedOutcomes);
        this.DoesMoveOutcomes = new(doesMoveOutcomes);
    }
}