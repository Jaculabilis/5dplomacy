using MultiversalDiplomacy.Model;

using NUnit.Framework;

namespace MultiversalDiplomacyTests;

public class MapTests
{
    IEnumerable<Location> LocationClosure(Location location)
    {
        IEnumerable<Location> visited = new List<Location>();
        IEnumerable<Location> toVisit = new List<Location>() { location };

        while (toVisit.Any())
        {
            Location next = toVisit.First();
            toVisit = toVisit.Skip(1);
            visited = visited.Append(next);
            foreach (Location other in next.Adjacents)
            {
                if (!visited.Contains(other)) toVisit = toVisit.Append(other);
            }
        }

        return visited;
    }

    [Test]
    public void MapCreation()
    {
        Province left = Province.Empty("Left", "Lef")
            .AddLandLocation();
        Province center = Province.Empty("Center", "Cen")
            .AddLandLocation();
        Province right = Province.Empty("Right", "Rig")
            .AddLandLocation();

        Location leftL = left.Locations.First();
        Location centerL = center.Locations.First();
        Location rightL = right.Locations.First();
        centerL.AddBorder(leftL);
        rightL.AddBorder(centerL);

        IEnumerable<Location> closure = LocationClosure(leftL);
        Assert.That(closure.Contains(leftL), Is.True, "Expected Left in closure");
        Assert.That(closure.Contains(centerL), Is.True, "Expected Center in closure");
        Assert.That(closure.Contains(rightL), Is.True, "Expected Right in closure");
    }

    [Test]
    public void LandAndSeaBorders()
    {
        World map = World.WithStandardMap();
        Assert.That(
            map.GetLand("NAF").Adjacents.Count(),
            Is.EqualTo(1),
            "Expected 1 bordering land province");
        Assert.That(
            map.GetWater("NAF").Adjacents.Count(),
            Is.EqualTo(3),
            "Expected 3 bordering sea provinces");
    }
}