using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class DATC_A
{
    private World StandardEmpty { get; } = World.WithStandardMap();

    [Test]
    public void DATC_6_A_1_MoveToAnAreaThatIsNotANeighbor()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup["England"]
            .Fleet("North Sea").MovesTo("Picardy").GetReference(out var order);

        // Order should fail.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(order, Is.Invalid(ValidationReason.UnreachableDestination));
    }

    [Test]
    public void DATC_6_A_2_MoveArmyToSea()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);

        // Order should fail.
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

        // Order should fail.
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

        // Program should not crash.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(order, Is.Invalid(ValidationReason.DestinationMatchesOrigin));
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
                .Fleet("London").MovesTo("Yorkshire").GetReference(out var orderLon)
                .Army("Wales").Supports.Fleet("London").MoveTo("Yorkshire");

        // The move of the army in Yorkshire is illegal. This makes the support of Liverpool also illegal.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderLon, Is.Valid);
        Assert.That(orderNth, Is.Invalid(ValidationReason.DestinationMatchesOrigin));
        Assert.That(orderYor, Is.Invalid(ValidationReason.DestinationMatchesOrigin));
        var orderYorRepl = orderYor.GetReplacementReference<HoldOrder>();

        // Without the support, the Germans have a stronger force. The army in London dislodges the army in Yorkshire.
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderLon, Is.Victorious);
        Assert.That(orderYorRepl, Is.Dislodged);
    }

    [Test]
    public void DATC_6_A_6_OrderingAUnitOfAnotherCountry()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Germany"]
                .Fleet("London", powerName: "England").MovesTo("North Sea").GetReference(out var order);

        // Order should fail.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(order, Is.Invalid(ValidationReason.InvalidUnitForPower));
    }

    [Test]
    public void DATC_6_A_7_OnlyArmiesCanBeConvoyed()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["England"]
                .Fleet("London").MovesTo("Belgium")
                .Fleet("North Sea").Convoys.Army("London").To("Belgium").GetReference(out var order);

        // Move from London to Belgium should fail.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(order, Is.Invalid(ValidationReason.InvalidOrderTypeForUnit));
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
        Assert.That(orderTri, Is.Invalid(ValidationReason.NoSelfSupport));
        var orderTriRepl = orderTri.GetReplacementReference<HoldOrder>();

        // The army in Trieste should be dislodged.
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderTriRepl, Is.Dislodged);
    }

    [Test]
    public void DATC_6_A_9_FleetsMustFollowCoastIfNotOnSea()
    {
        TestCaseBuilder setup = new TestCaseBuilder(StandardEmpty);
        setup
            ["Italy"]
                .Fleet("Rome").MovesTo("Venice").GetReference(out var order);

        // Move fails. An army can go from Rome to Venice, but a fleet can not.
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(order, Is.Invalid(ValidationReason.UnreachableDestination));
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
        Assert.That(orderRom, Is.Invalid(ValidationReason.UnreachableSupport));

        // Venice is not dislodged.
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderVen, Is.NotDislodged);
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
        Assert.That(orderVie, Is.Valid);
        Assert.That(orderVen, Is.Valid);

        // The two units bounce.
        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(orderVie, Is.Repelled);
        Assert.That(orderVie, Is.NotDislodged);
        Assert.That(orderVen, Is.Repelled);
        Assert.That(orderVen, Is.NotDislodged);
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
        Assert.That(orderVie, Is.Valid);
        Assert.That(orderMun, Is.Valid);
        Assert.That(orderVen, Is.Valid);

        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        // The three units bounce.
        Assert.That(orderVie, Is.Repelled);
        Assert.That(orderVie, Is.NotDislodged);
        Assert.That(orderMun, Is.Repelled);
        Assert.That(orderMun, Is.NotDislodged);
        Assert.That(orderVen, Is.Repelled);
        Assert.That(orderVen, Is.NotDislodged);
    }
}