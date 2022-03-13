using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Adjudicate;

/// <summary>
/// Helper class encapsulating the convoy pathfindind code.
/// </summary>
public static class PathFinder
{
    /// <summary>
    /// Determines if a convoy path exists for a move in a convoy order.
    /// </summary>
    public static bool ConvoyPathExists(World world, ConvoyOrder order)
        => ConvoyPathExists(world, order.Target, order.Location, order.Season);

    /// <summary>
    /// Determines if a convoy path exists for a move order.
    /// </summary>
    public static bool ConvoyPathExists(World world, MoveOrder order)
        => ConvoyPathExists(world, order.Unit, order.Location, order.Season);

    private static bool ConvoyPathExists(
        World world,
        Unit movingUnit,
        Location unitLocation,
        Season unitSeason)
    {
        // A convoy path exists between two locations if both are land locations in provinces that
        // also have coasts, and between those coasts there is a path of adjacent sea provinces
        // (not coastal) that are occupied by fleets. The move order is valid even if the fleets
        // belong to another power or were not given convoy orders; it will simply fail.
        IDictionary<(Location location, Season season), Unit> fleets = world.Units
            .Where(unit => unit.Type == UnitType.Fleet)
            .ToDictionary(unit => (unit.Location, unit.Season));

        // Verify that the origin is a coastal province.
        if (movingUnit.Location.Type != LocationType.Land) return false;
        IEnumerable<Location> originCoasts = movingUnit.Location.Province.Locations
            .Where(location => location.Type == LocationType.Water);
        if (!originCoasts.Any()) return false;

        // Verify that the destination is a coastal province.
        if (unitLocation.Type != LocationType.Land) return false;
        IEnumerable<Location> destCoasts = unitLocation.Province.Locations
            .Where(location => location.Type == LocationType.Water);
        if (!destCoasts.Any()) return false;

        // Seed the to-visit set with the origin coasts. Coastal locations will be filtered out of
        // locations added to the to-visit set, but the logic will still work with these as
        // starting points.
        Queue<(Location location, Season season)> toVisit = new(
            originCoasts.Select(location => (location, unitSeason)));
        HashSet<(Location, Season)> visited = new();

        // Begin pathfinding.
        while (toVisit.Any())
        {
            // Visit the next point in the queue.
            (Location currentLocation, Season currentSeason) = toVisit.Dequeue();
            visited.Add((currentLocation, currentSeason));

            var adjacents = GetAdjacentPoints(currentLocation, currentSeason);
            foreach ((Location adjLocation, Season adjSeason) in adjacents)
            {
                // If the destination is adjacent, then a path exists.
                if (destCoasts.Contains(adjLocation) && unitSeason == adjSeason) return true;

                // If not, add this location to the to-visit set if it isn't a coast, has a fleet,
                // and hasn't already been visited.
                if (!adjLocation.Province.Locations.Any(l => l.Type == LocationType.Land)
                    && fleets.ContainsKey((adjLocation, adjSeason))
                    && !visited.Contains((adjLocation, adjSeason)))
                {
                    toVisit.Enqueue((adjLocation, adjSeason));
                }
            }
        }

        // If the destination was never reached, then no path exists.
        return false;
    }

    private static List<(Location, Season)> GetAdjacentPoints(Location location, Season season)
    {
        List<(Location, Season)> adjacentPoints = new();
        List<Location> adjacentLocations = location.Adjacents.ToList();
        List<Season> adjacentSeasons = season.GetAdjacentSeasons().ToList();

        foreach (Location adjacentLocation in adjacentLocations)
        {
            adjacentPoints.Add((adjacentLocation, season));
        }
        foreach (Season adjacentSeason in adjacentSeasons)
        {
            adjacentPoints.Add((location, adjacentSeason));
        }
        foreach (Location adjacentLocation in adjacentLocations)
        {
            foreach (Season adjacentSeason in adjacentSeasons)
            {
                adjacentPoints.Add((adjacentLocation, adjacentSeason));
            }
        }

        return adjacentPoints;
    }
}