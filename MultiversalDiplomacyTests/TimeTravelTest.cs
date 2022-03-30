using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class TimeTravelTest
{
    [Test]
    public void MoveIntoOwnPastForksTimeline()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // Hold to move into the future, then move back into the past.
        setup[(0, 0)]
            .GetReference(out Season s0)
            ["Germany"]
                .Army("Mun").Holds().GetReference(out var mun0)
            .Execute()
        [(1, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr", season: s0).GetReference(out var mun1);

        setup.ValidateOrders();
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun1, Is.Victorious);

        World world = setup.UpdateWorld();

        // Confirm that there are now three seasons: the root, a future off the root, and a fork.
        Assert.That(world.Seasons.Count, Is.EqualTo(3));
        Assert.That(world.Seasons.Where(s => s.Timeline != s0.Timeline).Count(), Is.EqualTo(1));
        Season fork = world.Seasons.Where(s => s.Timeline != s0.Timeline).Single();
        Assert.That(s0.Futures, Contains.Item(s1));
        Assert.That(s0.Futures, Contains.Item(fork));
        Assert.That(fork.Past, Is.EqualTo(s0));

        // Confirm that there is a unit in Tyr 1:1 originating from Mun 1:0
        Unit originalUnit = world.GetUnitAt("Mun", s0.Coord);
        Unit aMun0 = world.GetUnitAt("Mun", s1.Coord);
        Unit aTyr = world.GetUnitAt("Tyr", fork.Coord);
        Assert.That(aTyr.Past, Is.EqualTo(mun1.Order.Unit));
        Assert.That(aTyr.Past?.Past, Is.EqualTo(mun0.Order.Unit));

        // Confirm that there is a unit in Mun 1:1 originating from Mun 0:0
        Unit aMun1 = world.GetUnitAt("Mun", fork.Coord);
        Assert.That(aMun1.Past, Is.EqualTo(originalUnit));
    }

    [Test]
    public void SupportToRepelledPastMoveForksTimeline()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // Fail to dislodge on the first turn, then support the move so it succeeds.
        setup[(0, 0)]
            .GetReference(out Season s0)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr").GetReference(out var mun0)
            ["Austria"]
                .Army("Tyr").Holds().GetReference(out var tyr0);

        setup.ValidateOrders();
        Assert.That(mun0, Is.Valid);
        Assert.That(tyr0, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun0, Is.Repelled);
        Assert.That(tyr0, Is.NotDislodged);
        setup.UpdateWorld();

        setup[(1, 0)]
            ["Germany"]
                .Army("Mun").Supports.Army("Mun", season: s0).MoveTo("Tyr").GetReference(out var mun1)
            ["Austria"]
                .Army("Tyr").Holds();

        // Confirm that history is changed.
        setup.ValidateOrders();
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun1, Is.NotCut);
        Assert.That(mun0, Is.Victorious);
        Assert.That(tyr0, Is.Dislodged);

        // Confirm that an alternate future is created.
        World world = setup.UpdateWorld();
        Season fork = world.GetSeason(1, 1);
        Unit tyr1 = world.GetUnitAt("Tyr", fork.Coord);
        Assert.That(
            tyr1.Past,
            Is.EqualTo(mun0.Order.Unit),
            "Expected A Mun 0:0 to advance to Tyr 1:1");
        Assert.That(
            world.RetreatingUnits.Count,
            Is.EqualTo(1),
            "Expected A Tyr 0:0 to be in retreat");
        Assert.That(world.RetreatingUnits.First().Unit, Is.EqualTo(tyr0.Order.Unit));
    }
}
