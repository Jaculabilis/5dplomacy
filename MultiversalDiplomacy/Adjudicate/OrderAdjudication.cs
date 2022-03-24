using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Represents the result of adjudicating an order.
/// </summary>
public class OrderAdjudication
{
    /// <summary>
    /// The order that was adjudicated.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Whether the order succeeded or failed.
    /// </summary>
    public bool Success { get; }

    // /// <summary>
    // /// The reason for the order's outcome.
    // /// </summary>
    // public string Reason { get; }

    public OrderAdjudication(Order order, bool success/*, string reason*/)
    {
        this.Order = order;
        this.Success = success;
        // this.Reason = reason;
    }
}

public static class OrderAdjudicationExtensions
{
    /// <summary>
    /// Create an <see cref="OrderAdjudication"/> accepting this order.
    /// </summary>
    public static OrderAdjudication Succeed(this Order order)
        => new OrderAdjudication(order, true);
}