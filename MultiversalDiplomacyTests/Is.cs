using MultiversalDiplomacy.Adjudicate;

namespace MultiversalDiplomacyTests;

public class Is : NUnit.Framework.Is
{
    public static OrderValidationConstraint Valid
        => new OrderValidationConstraint(true, ValidationReason.Valid);

    public static OrderValidationConstraint Invalid(ValidationReason expected)
        => new OrderValidationConstraint(false, expected);
}
