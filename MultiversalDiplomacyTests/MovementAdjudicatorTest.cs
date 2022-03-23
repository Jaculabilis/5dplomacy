using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class MovementAdjudicatorTest
{
    [Test]
    public void Validation_ValidHold()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap().WithInitialSeason());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var order);

        setup.ValidateOrders(new MovementPhaseAdjudicator());

        Assert.Multiple(() =>
        {
            Assert.That(order.Validation.Valid, Is.True, "Unexpected validation result");
            Assert.That(
                order.Validation.Reason,
                Is.EqualTo(ValidationReason.Valid),
                "Unexpected validation reason");
            Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
        });
    }

    [Test]
    public void Validation_ValidMove()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap().WithInitialSeason());
        setup["Germany"]
            .Army("Mun").MovesTo("Tyr").GetReference(out var order);

        setup.ValidateOrders(new MovementPhaseAdjudicator());

        Assert.Multiple(() =>
        {
            Assert.That(order.Validation.Valid, Is.True, "Unexpected validation result");
            Assert.That(
                order.Validation.Reason,
                Is.EqualTo(ValidationReason.Valid),
                "Unexpected validation reason");
            Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
        });
    }

    [Test]
    public void Validation_ValidConvoy()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap().WithInitialSeason());
        setup["Germany"]
            .Fleet("Nth").Convoys.Army("Hol").To("Lon").GetReference(out var order);

        setup.ValidateOrders(new MovementPhaseAdjudicator());

        Assert.Multiple(() =>
        {
            Assert.That(order.Validation.Valid, Is.True, "Unexpected validation result");
            Assert.That(
                order.Validation.Reason,
                Is.EqualTo(ValidationReason.Valid),
                "Unexpected validation reason");
            Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
        });
    }

    [Test]
    public void Validation_ValidSupportHold()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap().WithInitialSeason());
        setup["Germany"]
            .Army("Mun").Supports.Army("Kie").Hold().GetReference(out var order);

        setup.ValidateOrders(new MovementPhaseAdjudicator());

        Assert.Multiple(() =>
        {
            Assert.That(order.Validation.Valid, Is.True, "Unexpected validation result");
            Assert.That(
                order.Validation.Reason,
                Is.EqualTo(ValidationReason.Valid),
                "Unexpected validation reason");
            Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
        });
    }

    [Test]
    public void Validation_ValidSupportMove()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap().WithInitialSeason());
        setup["Germany"]
            .Army("Mun").Supports.Army("Kie").MoveTo("Ber").GetReference(out var order);

        setup.ValidateOrders(new MovementPhaseAdjudicator());

        Assert.Multiple(() =>
        {
            Assert.That(order.Validation.Valid, Is.True, "Unexpected validation result");
            Assert.That(
                order.Validation.Reason,
                Is.EqualTo(ValidationReason.Valid),
                "Unexpected validation reason");
            Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
        });
    }
}