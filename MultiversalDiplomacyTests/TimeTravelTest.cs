using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class TimeTravelTest
{
    [Test]
    public void MoveIntoOwnPast()
    {
        TestCaseBuilder setup = new(World.WithStandardMap());

        // Hold once so the timeline has a past.
        setup[(0, 0)]
            .GetReference(out Season s0)
            ["Germany"]
                .Army("Mun").Holds().GetReference(out var mun0);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        setup = new(setup.UpdateWorld(MovementPhaseAdjudicator.Instance));

        // Move into the past of the same timeline.
        setup[(1, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr", season: s0).GetReference(out var mun1);

        setup.ValidateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders(MovementPhaseAdjudicator.Instance);
        Assert.That(mun1, Is.Victorious);

        World world = setup.UpdateWorld(MovementPhaseAdjudicator.Instance);

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
}
