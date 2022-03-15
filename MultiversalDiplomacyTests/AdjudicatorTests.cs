using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class AdjudicatorTests
{
    [Test]
    public void OrderValidationTest()
    {
        IPhaseAdjudicator rubberStamp = new TestAdjudicator((world, orders) =>
        {
            return orders.Select(o => o.Validate(ValidationReason.Valid)).ToList();
        });
        World world = World.WithStandardMap().WithInitialSeason();
        Power power = world.GetPower("Austria");

        Order order = new NullOrder(power);
        List<Order> orders = new List<Order> { order };
        IEnumerable<OrderValidation> results = rubberStamp.ValidateOrders(world, orders);

        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.First().Order, Is.EqualTo(order));
        Assert.That(results.First().Reason, Is.EqualTo(ValidationReason.Valid));
        Assert.That(results.First().Valid, Is.True);
    }
}