using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;

namespace MultiversalDiplomacyTests;

public class Is : NUnit.Framework.Is
{
    public static OrderValidationConstraint Valid
        => new(true, ValidationReason.Valid);

    public static OrderValidationConstraint Invalid(ValidationReason expected)
        => new(false, expected);

    public static OrderBinaryAdjudicationConstraint<IsDislodged> Dislodged
        => new(true);

    public static OrderBinaryAdjudicationConstraint<IsDislodged> NotDislodged
        => new(false);

    public static OrderBinaryAdjudicationConstraint<DoesMove> Victorious
        => new(true);

    public static OrderBinaryAdjudicationConstraint<DoesMove> Repelled
        => new(false);
}
