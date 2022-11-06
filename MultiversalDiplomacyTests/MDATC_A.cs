using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Adjudicate.Decision;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class TimeTravelTest
{
    [Test]
    public void MDATC_3_A_1_MoveIntoOwnPastForksTimeline()
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

        // Confirm that there are now four seasons: three in the main timeline and one in a fork.
        Assert.That(
            world.Seasons.Where(s => s.Timeline == s0.Timeline).Count(),
            Is.EqualTo(3),
            "Failed to advance main timeline after last unit left");
        Assert.That(
            world.Seasons.Where(s => s.Timeline != s0.Timeline).Count(),
            Is.EqualTo(1),
            "Failed to fork timeline when unit moved in");

        // Confirm that there is a unit in Tyr 1:1 originating from Mun 1:0
        Season fork = world.GetSeason(1, 1);
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
    public void MDATC_3_A_2_SupportToRepelledPastMoveForksTimeline()
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

    [Test]
    public void MDATC_3_A_3_FailedMoveDoesNotForkTimeline()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // Hold to create a future, then attempt to attack in the past.
        setup[(0, 0)]
            .GetReference(out Season s0)
            ["Germany"]
                .Army("Mun").Holds()
            ["Austria"]
                .Army("Tyr").Holds().GetReference(out var tyr0)
            .Execute()
        [(1, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr", season: s0).GetReference(out var mun1)
            ["Austria"]
                .Army("Tyr").Holds();

        setup.ValidateOrders();
        Assert.That(mun1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun1, Is.Repelled);
        // The order to Tyr 0:0 should have been pulled into the adjudication set, so the reference
        // should not throw when accessing it.
        Assert.That(tyr0, Is.NotDislodged);

        // There should only be three seasons, all in the main timeline, since the move failed to
        // change the past and therefore did not create a new timeline.
        World world = setup.UpdateWorld();
        Assert.That(
            s0.Futures.Count(),
            Is.EqualTo(1),
            "A failed move incorrectly forked the timeline");
        Assert.That(s1.Futures.Count(), Is.EqualTo(1));
        Season s2 = world.GetSeason(s1.Turn + 1, s1.Timeline);
        Assert.That(s2.Futures.Count(), Is.EqualTo(0));
    }

    [Test]
    public void MDATC_3_A_4_SuperfluousSupportDoesNotForkTimeline()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // Move, then support the past move even though it succeeded already.
        setup[(0, 0)]
            .GetReference(out Season s0)
            ["Germany"]
                .Army("Mun").MovesTo("Tyr").GetReference(out var mun0)
                .Army("Boh").Holds();

        setup.ValidateOrders();
        Assert.That(mun0, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun0, Is.Victorious);
        setup.UpdateWorld();

        setup[(1, 0)]
            .GetReference(out Season s1)
            ["Germany"]
                .Army("Tyr").Holds()
                .Army("Boh").Supports.Army("Mun", season: s0).MoveTo("Tyr").GetReference(out var boh1);

        // The support does work and the move does succeed, but...
        setup.ValidateOrders();
        Assert.That(boh1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(boh1, Is.NotCut);
        Assert.That(mun0, Is.Victorious);

        // ...since it succeeded anyway, no fork is created.
        World world = setup.UpdateWorld();
        Assert.That(
            s0.Futures.Count(),
            Is.EqualTo(1),
            "A superfluous support incorrectly forked the timeline");
        Assert.That(s1.Futures.Count(), Is.EqualTo(1));
        Season s2 = world.GetSeason(s1.Turn + 1, s1.Timeline);
        Assert.That(s2.Futures.Count(), Is.EqualTo(0));
    }

    [Test]
    public void MDATC_3_A_5_CrossTimelineSupportDoesNotForkHead()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // London creates two timelines by moving into the past.
        setup[(0, 0)]
            .GetReference(out var s0_0)
            ["England"].Army("Lon").Holds()
            ["Austria"].Army("Tyr").Holds()
            ["Germany"].Army("Mun").Holds()
            .Execute()
        [(1, 0)]
            ["England"].Army("Lon").MovesTo("Yor", s0_0)
            .Execute()
        // Head seasons: 2:0  1:1
        // Both contain copies of the armies in Mun and Tyr.
        // Now Germany dislodges Austria in Tyr by supporting the move across timelines.
        [(2, 0)]
            .GetReference(out var s2_0)
            ["Germany"].Army("Mun").MovesTo("Tyr").GetReference(out var mun2_0)
            ["Austria"].Army("Tyr").Holds().GetReference(out var tyr2_0)
        [(1, 1)]
            .GetReference(out var s1_1)
            ["Germany"].Army("Mun").Supports.Army("Mun", s2_0).MoveTo("Tyr").GetReference(out var mun1_1)
            ["Austria"].Army("Tyr").Holds();

        // The attack against Tyr 2:0 succeeds.
        setup.ValidateOrders();
        Assert.That(mun2_0, Is.Valid);
        Assert.That(tyr2_0, Is.Valid);
        Assert.That(mun1_1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(mun2_0, Is.Victorious);
        Assert.That(tyr2_0, Is.Dislodged);
        Assert.That(mun1_1, Is.NotCut);

        // Since both seasons were at the head of their timelines, there should be no forking.
        World world = setup.UpdateWorld();
        Assert.That(
            s2_0.Futures.Count(),
            Is.EqualTo(1),
            "A cross-timeline support incorrectly forked the head of the timeline");
        Assert.That(
            s1_1.Futures.Count(),
            Is.EqualTo(1),
            "A cross-timeline support incorrectly forked the head of the timeline");
        Season s3_0 = world.GetSeason(s2_0.Turn + 1, s2_0.Timeline);
        Assert.That(s3_0.Futures.Count(), Is.EqualTo(0));
        Season s2_1 = world.GetSeason(s1_1.Turn + 1, s1_1.Timeline);
        Assert.That(s2_1.Futures.Count(), Is.EqualTo(0));
    }

    [Test]
    public void MDATC_3_A_6_CuttingCrossTimelineSupportDoesNotFork()
    {
        TestCaseBuilder setup = new(World.WithStandardMap(), MovementPhaseAdjudicator.Instance);

        // As above, only now London creates three timelines.
        setup[(0, 0)]
            .GetReference(out var s0_0)
            ["England"].Army("Lon").Holds()
            ["Austria"].Army("Boh").Holds()
            ["Germany"].Army("Mun").Holds()
            .Execute()
        [(1, 0)]
            ["England"].Army("Lon").MovesTo("Yor", s0_0)
            .Execute()
        // Head seasons: 2:0  1:1
        [(2, 0)]
        [(1, 1)]
            ["England"].Army("Yor").MovesTo("Edi", s0_0)
            .Execute()
        // Head seasons: 3:0  2:1  1:2
        // All three of these contain copies of the armies in Mun and Tyr.
        // As above, Germany dislodges Austria in Tyr 3:0 by supporting the move from 2:1.
        [(3, 0)]
            .GetReference(out var s3_0)
            ["Germany"].Army("Mun").MovesTo("Tyr")
            ["Austria"].Army("Tyr").Holds()
        [(2, 1)]
            .GetReference(out Season s2_1)
            ["Germany"].Army("Mun").Supports.Army("Mun", s3_0).MoveTo("Tyr").GetReference(out var mun2_1)
            ["Austria"].Army("Tyr").Holds()
        [(1, 2)]
            ["Germany"].Army("Mun").Holds()
            ["Austria"].Army("Tyr").Holds()
            .Execute()
        // Head seasons: 4:0 3:1 2:2
        // Now Austria cuts the support in 2:1 by attacking from 2:2.
        [(4, 0)]
            ["Germany"].Army("Tyr").Holds()
        [(3, 1)]
            ["Germany"].Army("Mun").Holds()
            ["Austria"].Army("Tyr").Holds()
        [(2, 2)]
            ["Germany"].Army("Mun").Holds()
            ["Austria"].Army("Tyr").MovesTo("Mun", s2_1).GetReference(out var tyr2_2);

        // The attack on Mun 2:1 is repelled, but the support is cut.
        setup.ValidateOrders();
        Assert.That(tyr2_2, Is.Valid);
        Assert.That(mun2_1, Is.Valid);
        setup.AdjudicateOrders();
        Assert.That(tyr2_2, Is.Repelled);
        Assert.That(mun2_1, Is.NotDislodged);
        Assert.That(mun2_1, Is.Cut);

        // Though the support was cut, the timeline doesn't fork because the outcome of a battle
        // wasn't changed in this timeline.
        World world = setup.UpdateWorld();
        Assert.That(
            s3_0.Futures.Count(),
            Is.EqualTo(1),
            "A cross-timeline support cut incorrectly forked the timeline");
        Assert.That(
            s2_1.Futures.Count(),
            Is.EqualTo(1),
            "A cross-timeline support cut incorrectly forked the timeline");
    }
}
