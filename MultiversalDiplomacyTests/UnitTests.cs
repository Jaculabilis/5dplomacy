using MultiversalDiplomacy.Map;
using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class UnitTests
{
    [Test]
    public void MovementTest()
    {
        Map map = TestMap.Instance;
        Location left = map.Land("LEF"), right = map.Land("RIG"), center = map.Land("CEN");
        Power pw1 = new Power("First");
        Season s1 = Season.MakeRoot();
        Unit u1 = Unit.Build(left, s1, pw1, UnitType.Army);

        Season s2 = s1.MakeNext();
        Unit u2 = u1.Next(right, s2);

        Season s3 = s2.MakeNext();
        Unit u3 = u2.Next(center, s3);

        Assert.That(u3.Past, Is.EqualTo(u2), "Missing unit past");
        Assert.That(u2.Past, Is.EqualTo(u1), "Missing unit past");
        Assert.That(u1.Past, Is.Null, "Unexpected unit past");

        Assert.That(u1.Season, Is.EqualTo(s1), "Unexpected unit season");
        Assert.That(u2.Season, Is.EqualTo(s2), "Unexpected unit season");
        Assert.That(u3.Season, Is.EqualTo(s3), "Unexpected unit season");

        Assert.That(u1.Location, Is.EqualTo(left), "Unexpected unit location");
        Assert.That(u2.Location, Is.EqualTo(right), "Unexpected unit location");
        Assert.That(u3.Location, Is.EqualTo(center), "Unexpected unit location");
    }
}