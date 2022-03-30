using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class UnitTests
{
    [Test]
    public void MovementTest()
    {
        World world = World.WithStandardMap();
        Location Mun = world.GetLand("Mun"),
            Boh = world.GetLand("Boh"),
            Tyr = world.GetLand("Tyr");
        Power pw1 = world.GetPower("Austria");
        Season s1 = world.RootSeason;
        Unit u1 = Unit.Build(Mun, s1, pw1, UnitType.Army);

        Season s2 = s1.MakeNext();
        Unit u2 = u1.Next(Boh, s2);

        Season s3 = s2.MakeNext();
        Unit u3 = u2.Next(Tyr, s3);

        Assert.That(u3.Past, Is.EqualTo(u2), "Missing unit past");
        Assert.That(u2.Past, Is.EqualTo(u1), "Missing unit past");
        Assert.That(u1.Past, Is.Null, "Unexpected unit past");

        Assert.That(u1.Season, Is.EqualTo(s1), "Unexpected unit season");
        Assert.That(u2.Season, Is.EqualTo(s2), "Unexpected unit season");
        Assert.That(u3.Season, Is.EqualTo(s3), "Unexpected unit season");

        Assert.That(u1.Location, Is.EqualTo(Mun), "Unexpected unit location");
        Assert.That(u2.Location, Is.EqualTo(Boh), "Unexpected unit location");
        Assert.That(u3.Location, Is.EqualTo(Tyr), "Unexpected unit location");
    }
}