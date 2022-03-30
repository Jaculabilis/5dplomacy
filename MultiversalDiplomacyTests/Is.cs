using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;

namespace MultiversalDiplomacyTests;

/// <summary>
/// Multiversal Diplomacy assertion constraint extension provider. "NotX" constraints are provided
/// because properties can't be added to Is.Not via extension.
/// </summary>
public class Is : NUnit.Framework.Is
{
    /// <summary>
    /// Returns a constraint that checks for a positive order validation.
    /// </summary>
    public static OrderValidationConstraint Valid
        => new(true, ValidationReason.Valid);

    /// <summary>
    /// Returns a constraint that checks for a negative order validation.
    /// </summary>
    public static OrderValidationConstraint Invalid(ValidationReason expected)
        => new(false, expected);

    /// <summary>
    /// Returns a constraint that checks for a positive <see cref="IsDislodged"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<IsDislodged> Dislodged
        => new(true);

    /// <summary>
    /// Returns a constraint that checks for a negative <see cref="IsDislodged"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<IsDislodged> NotDislodged
        => new(false);

    /// <summary>
    /// Returns a constraint that checks for a positive <see cref="DoesMove"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<DoesMove> Victorious
        => new(true);

    /// <summary>
    /// Returns a constraint that checks for a negative <see cref="DoesMove"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<DoesMove> Repelled
        => new(false);

    /// <summary>
    /// Returns a constraint that checks for a positive <see cref="GivesSupport"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<GivesSupport> NotCut
        => new(true);

    /// <summary>
    /// Returns a constraint that checks for a negative <see cref="GivesSupport"/> decision.
    /// </summary>
    public static OrderBinaryAdjudicationConstraint<GivesSupport> Cut
        => new(false);
}
