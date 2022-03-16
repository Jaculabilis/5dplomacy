using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

class TestCaseBuilderTest
{
    [Test]
    public void BuilderCreatesUnits()
    {
        TestCaseBuilder setup = new(World.WithStandardMap().WithInitialSeason());

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

        Unit armyLON = setup.World.GetUnitAt("London")
            ?? throw new AssertionException("Expected a unit in London");
        Assert.That(armyLON.Power.Name, Is.EqualTo("England"), "Unit created with wrong power");
        Assert.That(armyLON.Type, Is.EqualTo(UnitType.Army), "Unit created with wrong type");

        Unit fleetIRI = setup.World.GetUnitAt("Irish Sea")
            ?? throw new AssertionException("Expected a unit in Irish Sea");
        Assert.That(fleetIRI.Power.Name, Is.EqualTo("England"), "Unit created with wrong power");
        Assert.That(fleetIRI.Type, Is.EqualTo(UnitType.Fleet), "Unit created with wrong type");

        Unit fleetSTP = setup.World.GetUnitAt("Saint Petersburg")
            ?? throw new AssertionException("Expected a unit in Saint Petersburg");
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
        TestCaseBuilder setup = new(World.WithStandardMap().WithInitialSeason());

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
            => order => order.Unit.Location.Province.Name == name;

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
}
