using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Map;

/// <summary>
/// A collection of provinces. Provides shortcut functions for referencing provinces.
/// </summary>
public abstract class Map
{
    public abstract IEnumerable<Province> Provinces { get; }

    /// <summary>
    /// Returns the sole army-accessible location of a province.
    /// </summary>
    public Location Land(string provinceName)
        => Provinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Type == LocationType.Land);

    /// <summary>
    /// Returns the sole fleet-accessible location of a province.
    /// </summary>
    public Location Water(string provinceName)
        => Provinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Type == LocationType.Water);

    /// <summary>
    /// Returns the specified fleet-accessible location of a province with distinct coasts.
    /// </summary>
    public Location Coast(string provinceName, string coastName)
        => Provinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Name == coastName || l.Abbreviation == coastName);
}