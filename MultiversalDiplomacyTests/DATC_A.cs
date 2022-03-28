using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class DATC_A
{
    private World StandardEmpty { get; } = World.WithStandardMap().WithInitialSeason();

    [Test]
    public void DATC_6_A_1_MoveToAnAreaThatIsNotANeighbor()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup["England"]
            .Fleet("North Sea").MovesTo("Picardy").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order.Validation, Is.Invalid(ValidationReason.UnreachableDestination));
    }

    [Test]
    public void DATC_6_A_2_MoveArmyToSea()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);

        Assert.That(
            () =>
            {
                setup["England"]
                    .Army("Liverpool").MovesTo("Irish Sea");
            },
            Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void DATC_6_A_3_MoveFleetToLand()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);

        Assert.That(
            () =>
            {
                setup["Germany"]
                    .Fleet("Kiel").MovesTo("Munich");
            },
            Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void DATC_6_A_4_MoveToOwnSector()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup["Germany"]
            .Fleet("Kiel").MovesTo("Kiel").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order.Validation, Is.Invalid(ValidationReason.DestinationMatchesOrigin));
    }

    [Test]
    public void DATC_6_A_5_MoveToOwnSectorWithConvoy()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["England"]
                .Fleet("North Sea").Convoys.Army("Yorkshire").To("Yorkshire").GetReference(out var orderNth)
                .Army("Yorkshire").MovesTo("Yorkshire").GetReference(out var orderYor)
                .Army("Liverpool").Supports.Army("Yorkshire").MoveTo("Yorkshire")
            ["Germany"]
                .Fleet("London").MovesTo("Yorkshire")
                .Army("Wales").Supports.Fleet("London").MoveTo("Yorkshire");

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(orderNth.Validation, Is.Invalid(ValidationReason.DestinationMatchesOrigin));
        Assert.That(orderYor.Validation, Is.Invalid(ValidationReason.DestinationMatchesOrigin));

        // TODO assert dislodge
    }

    [Test]
    public void DATC_6_A_6_OrderingAUnitOfAnotherCountry()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Germany"]
                .Fleet("London", powerName: "England").MovesTo("North Sea").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order.Validation, Is.Invalid(ValidationReason.InvalidUnitForPower));
    }

    [Test]
    public void DATC_6_A_7_OnlyArmiesCanBeConvoyed()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["England"]
                .Fleet("London").MovesTo("Belgium")
                .Fleet("North Sea").Convoys.Army("London").To("Belgium").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order.Validation, Is.Invalid(ValidationReason.InvalidOrderTypeForUnit));
    }

    [Test]
    public void DATC_6_A_8_SupportToHoldYourselfIsNotPossible()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Italy"]
                .Army("Venice").MovesTo("Trieste")
                .Army("Tyrolia").Supports.Army("Venice").MoveTo("Trieste")
            ["Austria"]
                .Fleet("Trieste").Supports.Fleet("Trieste").Hold().GetReference(out var orderTri);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderTri.Validation, Is.Invalid(ValidationReason.NoSelfSupport));

        // The army in Trieste should be dislodged.
        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        // The order reference captures the invalidated order, but the unit is the same.
        var dislodgeTri = adjudications
            .OfType<IsDislodged>()
            .Single(adj => adj.Order.Unit == orderTri.Order.Unit);
        Assert.That(dislodgeTri.Outcome, Is.True, "Expected F Tri to be dislodged");
    }

    [Test]
    public void DATC_6_A_9_FleetsMustFollowCoastIfNotOnSea()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Italy"]
                .Fleet("Rome").MovesTo("Venice").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order.Validation, Is.Invalid(ValidationReason.UnreachableDestination));
    }

    [Test]
    public void DATC_6_A_10_SupportOnUnreachableDestinationNotPossible()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Austria"]
                .Army("Venice").Holds().GetReference(out var orderVen)
            ["Italy"]
                .Army("Apulia").MovesTo("Venice")
                .Fleet("Rome").Supports.Army("Apulia").MoveTo("Venice").GetReference(out var orderRom);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        // The support of Rome is illegal, because Venice can not be reached from Rome by a fleet.
        Assert.That(orderRom.Validation, Is.Invalid(ValidationReason.UnreachableSupport));

        // Venice is not dislodged.
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderVen.Adjudications.OfType<IsDislodged>().Count(), Is.EqualTo(1));
        Assert.That(orderVen.Adjudications.OfType<IsDislodged>().First().Outcome, Is.False);
    }

    [Test]
    public void DATC_6_A_11_SimpleBounce()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Austria"]
                .Army("Vienna").MovesTo("Tyrolia").GetReference(out var orderVie)
            ["Italy"]
                .Army("Venice").MovesTo("Tyrolia").GetReference(out var orderVen);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderVie.Validation, Is.Valid);
        Assert.That(orderVen.Validation, Is.Valid);

        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        // The two units bounce.
        Assert.That(orderVie.Adjudications.OfType<DoesMove>().Count(), Is.EqualTo(1));
        Assert.That(orderVie.Adjudications.OfType<DoesMove>().First().Outcome, Is.False);
        Assert.That(orderVen.Adjudications.OfType<DoesMove>().Count(), Is.EqualTo(1));
        Assert.That(orderVen.Adjudications.OfType<DoesMove>().First().Outcome, Is.False);
    }

    [Test]
    public void DATC_6_A_12_BounceOfThreeUnits()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Austria"]
                .Army("Vienna").MovesTo("Tyrolia").GetReference(out var orderVie)
            ["Germany"]
                .Army("Munich").MovesTo("Tyrolia").GetReference(out var orderMun)
            ["Italy"]
                .Army("Venice").MovesTo("Tyrolia").GetReference(out var orderVen);

        var validations = setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderVie.Validation, Is.Valid);
        Assert.That(orderMun.Validation, Is.Valid);
        Assert.That(orderVen.Validation, Is.Valid);

        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        // The three units bounce.
        Assert.That(orderVie.Adjudications.OfType<DoesMove>().Count(), Is.EqualTo(1));
        Assert.That(orderVie.Adjudications.OfType<DoesMove>().First().Outcome, Is.False);
        Assert.That(orderMun.Adjudications.OfType<DoesMove>().Count(), Is.EqualTo(1));
        Assert.That(orderMun.Adjudications.OfType<DoesMove>().First().Outcome, Is.False);
        Assert.That(orderVen.Adjudications.OfType<DoesMove>().Count(), Is.EqualTo(1));
        Assert.That(orderVen.Adjudications.OfType<DoesMove>().First().Outcome, Is.False);
    }
}