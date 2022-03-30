using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class MovementAdjudicatorTest
{
    [Test]
    public void Validation_ValidHold()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order, Is.Valid, "Unexpected validation result");
        Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
    }

    [Test]
    public void Validation_ValidMove()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").MovesTo("Tyr").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order, Is.Valid, "Unexpected validation result");
        Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
    }

    [Test]
    public void Validation_ValidConvoy()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Fleet("Nth").Convoys.Army("Hol").To("Lon").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order, Is.Valid, "Unexpected validation result");
        Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
    }

    [Test]
    public void Validation_ValidSupportHold()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Supports.Army("Kie").Hold().GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order, Is.Valid, "Unexpected validation result");
        Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
    }

    [Test]
    public void Validation_ValidSupportMove()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Supports.Army("Kie").MoveTo("Ber").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);

        Assert.That(order, Is.Valid, "Unexpected validation result");
        Assert.That(order.Replacement, Is.Null, "Unexpected order replacement");
    }

    [Test]
    public void Adjudication_Hold()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);

        var adjMun = order.Adjudications;
        Assert.That(adjMun.All(adj => adj.Resolved), Is.True);
        Assert.That(adjMun.OfType<IsDislodged>().Count(), Is.EqualTo(1));

        IsDislodged isDislodged = adjMun.OfType<IsDislodged>().Single();
        Assert.That(isDislodged.Order, Is.EqualTo(order.Order));
        Assert.That(isDislodged.Outcome, Is.False);
        Assert.That(isDislodged.Incoming, Is.Empty);
    }

    [Test]
    public void Adjudication_Move()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").MovesTo("Tyr").GetReference(out var order);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);

        var adjMun = order.Adjudications;
        Assert.That(adjMun.All(adj => adj.Resolved), Is.True);
        Assert.That(adjMun.OfType<IsDislodged>().Count(), Is.EqualTo(1));
        Assert.That(adjMun.OfType<DoesMove>().Count(), Is.EqualTo(1));

        IsDislodged dislodged = adjMun.OfType<IsDislodged>().Single();
        Assert.That(dislodged.Order, Is.EqualTo(order.Order));
        Assert.That(dislodged.Outcome, Is.False);

        DoesMove moves = adjMun.OfType<DoesMove>().Single();
        Assert.That(moves.Order, Is.EqualTo(order.Order));
        Assert.That(moves.Outcome, Is.True);
        Assert.That(moves.Competing, Is.Empty);
        Assert.That(moves.OpposingMove, Is.Null);
    }

    [Test]
    public void Adjudication_Support()
    {
        TestCaseBuilder setup = new TestCaseBuilder(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").MovesTo("Tyr").GetReference(out var move)
            .Army("Boh").Supports.Army("Mun").MoveTo("Tyr").GetReference(out var support);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);

        var adjBoh = support.Adjudications;
        Assert.That(adjBoh.All(adj => adj.Resolved), Is.True);
        Assert.That(adjBoh.OfType<IsDislodged>().Count(), Is.EqualTo(1));
        Assert.That(adjBoh.OfType<GivesSupport>().Count(), Is.EqualTo(1));

        IsDislodged dislodgeBoh = adjBoh.OfType<IsDislodged>().Single();
        Assert.That(dislodgeBoh.Order, Is.EqualTo(support.Order));
        Assert.That(dislodgeBoh.Outcome, Is.False);

        GivesSupport supportBoh = adjBoh.OfType<GivesSupport>().Single();
        Assert.That(supportBoh.Order, Is.EqualTo(support.Order));
        Assert.That(supportBoh.Outcome, Is.True);

        var adjMun = move.Adjudications;
        Assert.That(adjMun.All(adj => adj.Resolved), Is.True);
        Assert.That(adjMun.OfType<AttackStrength>().Count(), Is.EqualTo(1));

        AttackStrength attackMun = adjMun.OfType<AttackStrength>().Single();
        Assert.That(attackMun.Order, Is.EqualTo(move.Order));
        Assert.That(attackMun.MinValue, Is.EqualTo(2));
        Assert.That(attackMun.MaxValue, Is.EqualTo(2));
    }

    [Test]
    public void Update_SingleHold()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());
        setup["Germany"]
            .Army("Mun").Holds().GetReference(out var mun);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun, Is.Valid);

        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun, Is.NotDislodged);

        World updated = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);

        // Confirm the future was created
        Assert.That(updated.Seasons.Count, Is.EqualTo(2));
        Season future = updated.Seasons.Single(s => s != updated.RootSeason);
        Assert.That(future.Past, Is.EqualTo(updated.RootSeason));
        Assert.That(future.Futures, Is.Empty);
        Assert.That(future.Timeline, Is.EqualTo(updated.RootSeason.Timeline));
        Assert.That(future.Turn, Is.EqualTo(Season.FIRST_TURN + 1));

        // Confirm the unit was created
        Assert.That(updated.Units.Count, Is.EqualTo(2));
        Unit second = updated.Units.Single(u => u.Past != null);
        Assert.That(second.Location, Is.EqualTo(mun.Order.Unit.Location));
    }

    [Test]
    public void Update_DoubleHold()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());
        setup[(0, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Mun").Holds().GetReference(out var mun1);

        Assert.That(mun1.Order.Unit.Season, Is.EqualTo(s1));
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.NotDislodged);
        World updated = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);

        // Confirm the future was created
        Season s2 = updated.GetSeason(1, 0);
        Assert.That(s2.Past, Is.EqualTo(s1));
        Assert.That(s2.Futures, Is.Empty);
        Assert.That(s2.Timeline, Is.EqualTo(s1.Timeline));
        Assert.That(s2.Turn, Is.EqualTo(s1.Turn + 1));

        // Confirm the unit was created in the future
        Unit u2 = updated.GetUnitAt("Mun", s2.Coord);
        Assert.That(updated.Units.Count, Is.EqualTo(2));
        Assert.That(u2, Is.Not.EqualTo(mun1.Order.Unit));
        Assert.That(u2.Past, Is.EqualTo(mun1.Order.Unit));
        Assert.That(u2.Season, Is.EqualTo(s2));

        setup[(1, 0)]
            ["Germany"]
                .Army("Mun").Holds().GetReference(out var mun2);

        // Validate the second set of orders
        var validations = setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(validations.Count, Is.EqualTo(1));
        Assert.That(mun2, Is.Valid);

        // Adjudicate the second set of orders
        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun2, Is.NotDislodged);

        // Update the world again
        updated = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);
        Season s3 = updated.GetSeason(s2.Turn + 1, s2.Timeline);
        Unit u3 = updated.GetUnitAt("Mun", s3.Coord);
        Assert.That(u3.Past, Is.EqualTo(mun2.Order.Unit));
    }

    [Test]
    public void Update_DoubleMove()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());
        setup[(0, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr").GetReference(out var mun1);

        Assert.That(mun1.Order.Unit.Season, Is.EqualTo(s1));
        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.Victorious);
        World updated = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);

        // Confirm the future was created
        Season s2 = updated.GetSeason(s1.Turn + 1, s1.Timeline);
        Assert.That(s2.Past, Is.EqualTo(s1));
        Assert.That(s2.Futures, Is.Empty);
        Assert.That(s2.Timeline, Is.EqualTo(s1.Timeline));
        Assert.That(s2.Turn, Is.EqualTo(s1.Turn + 1));

        // Confirm the unit was created in the future
        Unit u2 = updated.GetUnitAt("Tyr", s2.Coord);
        Assert.That(updated.Units.Count, Is.EqualTo(2));
        Assert.That(u2, Is.Not.EqualTo(mun1.Order.Unit));
        Assert.That(u2.Past, Is.EqualTo(mun1.Order.Unit));
        Assert.That(u2.Season, Is.EqualTo(s2));

        setup[(1, 0)]
            ["Germany"]
                .Army("Tyr").MovesTo("Mun").GetReference(out var tyr2);

        // Validate the second set of orders
        var validations = setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(validations.Count, Is.EqualTo(1));
        Assert.That(tyr2, Is.Valid);

        // Adjudicate the second set of orders
        var adjudications = setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(tyr2, Is.Victorious);

        // Update the world again
        updated = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);
        Season s3 = updated.GetSeason(s2.Turn + 1, s2.Timeline);
        Unit u3 = updated.GetUnitAt("Mun", s3.Coord);
        Assert.That(u3.Past, Is.EqualTo(u2));
    }
}
