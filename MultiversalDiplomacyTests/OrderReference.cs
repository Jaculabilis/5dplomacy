using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

/// <summary>
/// An object that provides a view into an order's fate during a test case. This object is
/// stateless and provides data by encapsulating queries on the state of the origin test case
/// builder.
/// </summary>
public abstract class OrderReference
{
    protected TestCaseBuilder Builder { get; }

    protected Order Order { get; }

    public OrderReference(TestCaseBuilder builder, Order order)
    {
        this.Builder = builder;
        this.Order = order;
    }

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

    /// <summary>
    /// Returns an <see cref="OrderReference"/> for the order that replaced this order.
    /// </summary>
    public OrderReference<ReplacementOrderType> GetReplacementReference<ReplacementOrderType>()
        where ReplacementOrderType : Order
    {
        if (this.Replacement == null)
        {
            throw new InvalidOperationException("This order was not replaced");
        }
        Assert.That(
            this.Replacement.Order,
            Is.AssignableTo(typeof(ReplacementOrderType)),
            "Unexpected replacement order type");
        ReplacementOrderType replacementOrder = (ReplacementOrderType)this.Replacement.Order;
        return new(this.Builder, replacementOrder);
    }

    /// <summary>
    /// A list of all adjudication decisions related to this order. Throws if adjudication has not
    /// occurred.
    /// </summary>
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
                GivesSupport supports => supports.Order == this.Order,
                HasPath path => path.Order == this.Order,
                AttackStrength attack => attack.Order == this.Order,
                DefendStrength defend => defend.Order == this.Order,
                PreventStrength prevent => prevent.Order == this.Order,
                HoldStrength hold => this.Order is UnitOrder unitOrder
                    ? hold.Province == unitOrder.Unit.Location.Province
                    : false,
                _ => false,
            }).ToList();
            return adjudications;
        }
    }

    /// <summary>
    /// Returns an adjudication decision of a specified type for this order. Throws if adjudication
    /// has not occurred.
    /// </summary>
    public DecisionType GetDecision<DecisionType>()
        where DecisionType : AdjudicationDecision
    {
        var typeAdjudications = this.Adjudications.OfType<DecisionType>();
        Assert.That(typeAdjudications.Any(), Is.True, $"No {typeof(DecisionType)} decision found");
        return typeAdjudications.Single();
    }

    /// <summary>
    /// If this order is a unit order and the unit was dislodged, the <see cref="RetreatingUnit"/>
    /// representing the retreat. Throws if adjudication has not occurred.
    /// </summary>
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
}

/// <summary>
/// An object that provides a view into an order's fate during a test case. This object is
/// stateless and provides data by encapsulating queries on the state of the origin test case
/// builder.
/// </summary>
public class OrderReference<OrderType> : OrderReference where OrderType : Order
{
    /// <summary>
    /// The order.
    /// </summary>
    new public OrderType Order { get; }

    public OrderReference(TestCaseBuilder builder, OrderType order)
        : base(builder, order)
    {
        this.Order = order;
    }
}
