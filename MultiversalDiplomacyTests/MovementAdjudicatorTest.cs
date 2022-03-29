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
}
