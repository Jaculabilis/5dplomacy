using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class SeasonTests
{
    [Test]
    public void TimelineForking()
    {
        Season a0 = Season.MakeRoot();
        Season a1 = a0.MakeNext();
        Season a2 = a1.MakeNext();
        Season a3 = a2.MakeNext();
        Season b1 = a1.MakeFork();
        Season b2 = b1.MakeNext();
        Season c1 = a1.MakeFork();
        Season d1 = a2.MakeFork();

        Assert.That(a0.Timeline, Is.EqualTo(0), "Unexpected trunk timeline number");
        Assert.That(a1.Timeline, Is.EqualTo(0), "Unexpected trunk timeline number");
        Assert.That(a2.Timeline, Is.EqualTo(0), "Unexpected trunk timeline number");
        Assert.That(a3.Timeline, Is.EqualTo(0), "Unexpected trunk timeline number");
        Assert.That(b1.Timeline, Is.EqualTo(1), "Unexpected first alt number");
        Assert.That(b2.Timeline, Is.EqualTo(1), "Unexpected first alt number");
        Assert.That(c1.Timeline, Is.EqualTo(2), "Unexpected second alt number");
        Assert.That(d1.Timeline, Is.EqualTo(3), "Unexpected third alt number");

        Assert.That(a0.Turn, Is.EqualTo(Season.FIRST_TURN + 0), "Unexpected first turn number");
        Assert.That(a1.Turn, Is.EqualTo(Season.FIRST_TURN + 1), "Unexpected next turn number");
        Assert.That(a2.Turn, Is.EqualTo(Season.FIRST_TURN + 2), "Unexpected next turn number");
        Assert.That(a3.Turn, Is.EqualTo(Season.FIRST_TURN + 3), "Unexpected next turn number");
        Assert.That(b1.Turn, Is.EqualTo(Season.FIRST_TURN + 2), "Unexpected fork turn number");
        Assert.That(b2.Turn, Is.EqualTo(Season.FIRST_TURN + 3), "Unexpected fork turn number");
        Assert.That(c1.Turn, Is.EqualTo(Season.FIRST_TURN + 2), "Unexpected fork turn number");
        Assert.That(d1.Turn, Is.EqualTo(Season.FIRST_TURN + 3), "Unexpected fork turn number");

        Assert.That(a0.TimelineRoot(), Is.EqualTo(a0), "Expected timeline root to be reflexive");
        Assert.That(a3.TimelineRoot(), Is.EqualTo(a0), "Expected trunk timeline to have root");
        Assert.That(b1.TimelineRoot(), Is.EqualTo(b1), "Expected alt timeline root to be reflexive");
        Assert.That(b2.TimelineRoot(), Is.EqualTo(b1), "Expected alt timeline to root at first fork");
        Assert.That(c1.TimelineRoot(), Is.EqualTo(c1), "Expected alt timeline root to be reflexive");
        Assert.That(d1.TimelineRoot(), Is.EqualTo(d1), "Expected alt timeline root to be reflexive");

        Assert.That(b2.InAdjacentTimeline(a3), Is.True, "Expected alts to be adjacent to origin");
        Assert.That(b2.InAdjacentTimeline(c1), Is.True, "Expected alts with common origin to be adjacent");
        Assert.That(b2.InAdjacentTimeline(d1), Is.False, "Expected alts from different origins not to be adjacent");
    }

    [Test]
    public void LookupTest()
    {
        World world = World.WithStandardMap();
        Season s2 = world.RootSeason.MakeNext();
        Season s3 = s2.MakeNext();
        Season s4 = s2.MakeFork();
        World updated = world.Update(seasons: world.Seasons.Append(s2).Append(s3).Append(s4));

        Assert.That(updated.GetSeason(Season.FIRST_TURN, 0), Is.EqualTo(updated.RootSeason));
        Assert.That(updated.GetSeason(Season.FIRST_TURN + 1, 0), Is.EqualTo(s2));
        Assert.That(updated.GetSeason(Season.FIRST_TURN + 2, 0), Is.EqualTo(s3));
        Assert.That(updated.GetSeason(Season.FIRST_TURN + 2, 1), Is.EqualTo(s4));
    }
}