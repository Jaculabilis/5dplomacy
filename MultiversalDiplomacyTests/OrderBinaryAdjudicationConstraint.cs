using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;

using NUnit.Framework.Constraints;

namespace MultiversalDiplomacyTests;

public class OrderBinaryAdjudicationConstraint<DecisionType> : Constraint
    where DecisionType : BinaryAdjudicationDecision
{
    private bool expectedOutcome;

    public override string Description
    {
        get => $"{typeof(DecisionType).Name}={expectedOutcome}";
    }

    public OrderBinaryAdjudicationConstraint(bool outcome)
    {
        this.expectedOutcome = outcome;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is OrderReference orderRef)
        {
            DecisionType decision = orderRef.GetDecision<DecisionType>();
            return new ConstraintResult(this, decision, decision.Outcome == this.expectedOutcome);
        }
        return new ConstraintResult(this, actual, false);
    }
}
