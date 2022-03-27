using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

/// <summary>
/// An object that provides a view into an order's fate during a test case.
/// </summary>
public class OrderReference<OrderType> where OrderType : Order
{
    private TestCaseBuilder Builder { get; }

    /// <summary>
    /// The order.
    /// </summary>
    public OrderType Order { get; }

    /// <summary>
    /// The validation result for the order. Throws if validation has not occurred.
    /// </summary>
    public OrderValidation Validation
    {
        get
        {
            if (this.Builder.ValidationResults == null)
            {
                throw new InvalidOperationException("Validation has not been done yet");
            }
            var orderValidation = this.Builder.ValidationResults.Where(v => this.Order == v.Order);
            if (!orderValidation.Any())
            {
                throw new AssertionException($"Missing validation for {this.Order}");
            }
            return orderValidation.Single();
        }
    }

    /// <summary>
    /// The order that replaced this order, if any. Throws if validation has not occurred.
    /// </summary>
    public OrderValidation? Replacement
    {
        get
        {
            if (this.Builder.ValidationResults == null)
            {
                throw new InvalidOperationException("Validation has not been done yet");
            }
            if (this.Order is UnitOrder unitOrder)
            {
                var replacementOrder = this.Builder.ValidationResults.Where(
                    v => v.Order is UnitOrder uo && uo != unitOrder && uo.Unit == unitOrder.Unit);
                if (replacementOrder.Any())
                {
                    return replacementOrder.Single();
                }
            }
            return null;
        }
    }

    public List<AdjudicationDecision> Adjudications
    {
        get
        {
            if (this.Builder.AdjudicationResults == null)
            {
                throw new InvalidOperationException("Adjudication has not been done yet");
            }
            var adjudications = this.Builder.AdjudicationResults.Where(ad => ad switch
            {
                IsDislodged dislodged => dislodged.Order == this.Order,
                DoesMove moves => moves.Order == this.Order,
                _ => false,
            }).ToList();
            return adjudications;
        }
    }

    public RetreatingUnit? Retreat
    {
        get
        {
            if (this.Builder.AdjudicationResults == null)
            {
                throw new InvalidOperationException("Adjudication has not been done yet");
            }
            if (this.Order is UnitOrder unitOrder)
            {
                var retreat = this.Builder.World.RetreatingUnits.Where(
                    ru => ru.Unit == unitOrder.Unit);
                if (retreat.Any())
                {
                    return retreat.Single();
                }
            }
            return null;
        }
    }

    public OrderReference(TestCaseBuilder builder, OrderType order)
    {
        this.Builder = builder;
        this.Order = order;
    }
}