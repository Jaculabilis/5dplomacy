using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Orders;

/// <summary>
/// An order given to a specific unit.
/// </summary>
public abstract class UnitOrder : Order
{
    /// <summary>
    /// The ordered unit.
    /// </summary>
    public Unit Unit { get; }

    public UnitOrder(Power power, Unit unit) : base(power)
    {
        this.Unit = unit;
    }

    /// <summary>
    /// Returns whether a move order is moving into this order's unit's province.
    /// </summary>
    public bool IsIncoming(MoveOrder other)
        => this != other
        && other.Season == this.Unit.Season
        && other.Province == this.Unit.Province;
}