using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Represents the result of validating an order.
/// </summary>
public class OrderValidation
{
    /// <summary>
    /// The order that was validated.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Whether the order is valid.
    /// </summary>
    public bool Valid { get; }

    /// <summary>
    /// The reason for the order validation result.
    /// </summary>
    public ValidationReason Reason { get; }

    internal OrderValidation(Order order, bool valid, ValidationReason reason)
    {
        this.Order = order;
        this.Valid = valid;
        this.Reason = reason;
    }
}

public static class OrderValidationExtensions
{
    /// <summary>
    /// Create an <see cref="OrderValidation"/> accepting this order.
    /// </summary>
    public static OrderValidation Validate(this Order order, ValidationReason reason)
        => new OrderValidation(order, true, reason);

    /// <summary>
    /// Create an <see cref="OrderValidation"/> rejecting this order.
    /// </summary>
    public static OrderValidation Invalidate(this Order order, ValidationReason reason)
        => new OrderValidation(order, false, reason);
}
