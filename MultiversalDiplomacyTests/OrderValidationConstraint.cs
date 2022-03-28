using MultiversalDiplomacy.Adjudicate;

using NUnit.Framework.Constraints;

namespace MultiversalDiplomacyTests;

public class OrderValidationConstraint : Constraint
{
    private bool valid;
    private ValidationReason expectedReason;

    public override string Description
    {
        get => this.valid ? "Valid" : $"Invalid ({this.expectedReason})";
    }

    public OrderValidationConstraint(bool valid, ValidationReason expected)
    {
        this.valid = valid;
        this.expectedReason = expected;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        bool success = actual switch
        {
            OrderReference reference
                => reference.Validation.Valid == this.valid
                && reference.Validation.Reason == this.expectedReason,
            OrderValidation validation
                => validation.Valid == this.valid
                && validation.Reason == this.expectedReason,
            _ => false,
        };
        return new ConstraintResult(this, actual, success);
    }
}
