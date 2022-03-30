using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

class TestCaseBuilderTest
{
    [Test]
    public void BuilderCreatesUnits()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());

        Assert.That(setup.World.Powers.Count(), Is.EqualTo(7), "Unexpected power count");
        Assert.That(setup.World.Units, Is.Empty, "Expected no units to be created yet");

        setup
            ["England"]
                .Army("London").Exists()
                .Fleet("Irish Sea").Exists()
            ["Russia"]
                .Fleet("Saint Petersburg", "west coast").Exists();

        Assert.That(setup.Orders, Is.Empty, "Expected no orders to be created yet");
        Assert.That(setup.World.Units, Is.Not.Empty, "Expected units to be created");

        Unit armyLON = setup.World.GetUnitAt("London");
        Assert.That(armyLON.Power.Name, Is.EqualTo("England"), "Unit created with wrong power");
        Assert.That(armyLON.Type, Is.EqualTo(UnitType.Army), "Unit created with wrong type");

        Unit fleetIRI = setup.World.GetUnitAt("Irish Sea");
        Assert.That(fleetIRI.Power.Name, Is.EqualTo("England"), "Unit created with wrong power");
        Assert.That(fleetIRI.Type, Is.EqualTo(UnitType.Fleet), "Unit created with wrong type");

        Unit fleetSTP = setup.World.GetUnitAt("Saint Petersburg");
        Assert.That(fleetSTP.Power.Name, Is.EqualTo("Russia"), "Unit created with wrong power");
        Assert.That(fleetSTP.Type, Is.EqualTo(UnitType.Fleet), "Unit created with wrong type");
        Assert.That(
            fleetSTP.Location,
            Is.EqualTo(setup.World.GetWater("STP", "wc")),
            "Unit created on wrong coast");
    }

    [Test]
    public void BuilderCreatesOrders()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());

        Assert.That(setup.World.Powers.Count(), Is.EqualTo(7), "Unexpected power count");
        Assert.That(setup.World.Units, Is.Empty, "Expected no units to be created yet");
        Assert.That(setup.Orders, Is.Empty, "Expected no orders to be created yet");

        setup
            ["Germany"]
                .Army("Berlin").MovesTo("Kiel")
                .Army("Prussia").Holds()
            ["England"]
                .Fleet("North Sea").Convoys.Army("London").To("Holland")
            ["France"]
                .Army("Kiel").Supports.Army("London", powerName: "England").MoveTo("Holland")
                .Army("Munich").Supports.Army("Kiel").Hold();

        Assert.That(setup.Orders, Is.Not.Empty, "Expected orders to be created");
        Assert.That(setup.World.Units, Is.Not.Empty, "Expected units to be created");
        List<UnitOrder> orders = setup.Orders.OfType<UnitOrder>().ToList();

        Func<UnitOrder, bool> OrderForProvince(string name)
            => order => order.Unit.Province.Name == name;

        UnitOrder orderBer = orders.Single(OrderForProvince("Berlin"));
        Assert.That(orderBer, Is.InstanceOf<MoveOrder>(), "Unexpected order type");
        Assert.That(
            (orderBer as MoveOrder)?.Location,
            Is.EqualTo(setup.World.GetLand("Kiel")),
            "Unexpected move order destination");

        UnitOrder orderPru = orders.Single(OrderForProvince("Prussia"));
        Assert.That(orderPru, Is.InstanceOf<HoldOrder>(), "Unexpected order type");

        UnitOrder orderNth = orders.Single(OrderForProvince("North Sea"));
        Assert.That(orderNth, Is.InstanceOf<ConvoyOrder>(), "Unexpected order type");
        Assert.That(
            (orderNth as ConvoyOrder)?.Target,
            Is.EqualTo(setup.World.GetUnitAt("London")),
            "Unexpected convoy order target");
        Assert.That(
            (orderNth as ConvoyOrder)?.Location,
            Is.EqualTo(setup.World.GetLand("Holland")),
            "Unexpected convoy order destination");

        UnitOrder orderKie = orders.Single(OrderForProvince("Kiel"));
        Assert.That(orderKie, Is.InstanceOf<SupportMoveOrder>(), "Unexpected order type");
        Assert.That(
            (orderKie as SupportMoveOrder)?.Target,
            Is.EqualTo(setup.World.GetUnitAt("London")),
            "Unexpected convoy order target");
        Assert.That(
            (orderKie as SupportMoveOrder)?.Location,
            Is.EqualTo(setup.World.GetLand("Holland")),
            "Unexpected convoy order destination");

        UnitOrder orderMun = orders.Single(OrderForProvince("Munich"));
        Assert.That(orderMun, Is.InstanceOf<SupportHoldOrder>(), "Unexpected order type");
        Assert.That(
            (orderMun as SupportHoldOrder)?.Target,
            Is.EqualTo(setup.World.GetUnitAt("Kiel")),
            "Unexpected convoy order target");

        Assert.That(orders.Where(OrderForProvince("London")), Is.Empty, "Unexpected order");
    }

    [Test]
    public void BuilderProvidesReferencesForValidation()
    {
        IPhaseAdjudicator rubberStamp = new TestAdjudicator(validate: TestAdjudicator.RubberStamp);

        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var orderMun);

        Assert.That(orderMun, Is.Not.Null, "Expected order reference");
        Assert.That(
            orderMun.Order.Power,
            Is.EqualTo(setup.World.GetPower("Germany")),
            "Wrong power");
        Assert.That(
            orderMun.Order.Unit.Location,
            Is.EqualTo(setup.World.GetLand("Mun")),
            "Wrong unit");

        Assert.That(
            code: () => _ = orderMun.Validation,
            Throws.Exception,
            "Validation property should be inaccessible before validation actually happens");
        setup.ValidateOrders(rubberStamp);
        Assert.That(
            code: () => _ = orderMun.Validation,
            Throws.Nothing,
            "Validation property should be accessible after validation");

        Assert.That(
            orderMun.Validation.Order,
            Is.EqualTo(orderMun.Order),
            "Validation for wrong order");
        Assert.That(
            orderMun.Validation.Valid,
            Is.True,
            "Unexpected validation result");
        Assert.That(
            orderMun.Validation.Reason,
            Is.EqualTo(ValidationReason.Valid),
            "Unexpected validation reason");
    }

    public void BuilderProvidesReferencesForAdjudication()
    {
        IPhaseAdjudicator rubberStamp = new TestAdjudicator(
            validate: TestAdjudicator.RubberStamp,
            adjudicate: TestAdjudicator.NoMoves);

        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var orderMun);

        Assert.That(
            code: () => _ = orderMun.Adjudications,
            Throws.Exception,
            "Adjudication property should be inaccessible before validation");
        Assert.That(
            code: () => _ = orderMun.Retreat,
            Throws.Exception,
            "Retreat property should be inaccessible before validation");

        setup.ValidateOrders(rubberStamp);
        Assert.That(
            code: () => _ = orderMun.Adjudications,
            Throws.Exception,
            "Adjudication property should be inaccessible before adjudication");
        Assert.That(
            code: () => _ = orderMun.Retreat,
            Throws.Exception,
            "Retreat property should be inaccessible before adjudication");

        var decisions = setup.AdjudicateOrders(rubberStamp);
        Assert.That(
            code: () => _ = orderMun.Adjudications,
            Throws.Nothing,
            "Adjudication property should be accessible after adjudication");
        Assert.That(
            code: () => _ = orderMun.Retreat,
            Throws.Nothing,
            "Retreat property should be accessible after validation");

        Assert.That(orderMun.Retreat, Is.Null, "Noop adjudicator shouldn't cause retreats");
        Assert.That(
            orderMun.Adjudications.Count,
            Is.EqualTo(1),
            "Unexpected number of adjudications");
        AdjudicationDecision decision = orderMun.Adjudications.First();
        Assert.That(decision.Resolved, Is.True, "Unexpected unresolved decision");
        Assert.That(
            decision,
            Is.AssignableTo<IsDislodged>(),
            "Noop adjudicator should provide a dislodge decision for a hold");
        CollectionAssert.Contains(
            decisions,
            decision,
            "Expected the adjudicated decision to be provided by the order reference");
    }
}
