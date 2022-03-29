namespace MultiversalDiplomacy.Model;

/// <summary>
/// Represents a unit at a particular point in time.
/// </summary>
public class Unit
{
    /// <summary>
    /// The previous iteration of a unit. This is null if the unit was just built.
    /// </summary>
    public Unit? Past { get; }

    /// <summary>
    /// The location on the map where the unit is.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// The season in time when the unit is.
    /// </summary>
    public Season Season { get; }

    /// <summary>
    /// The allegiance of the unit.
    /// </summary>
    public Power Power { get; }

    /// <summary>
    /// The type of unit.
    /// </summary>
    public UnitType Type { get; }

    /// <summary>
    /// The unit's spatiotemporal location as a province-season tuple.
    /// </summary>
    public (Province province, Season season) Point => (this.Location.Province, this.Season);

    private Unit(Unit? past, Location location, Season season, Power power, UnitType type)
    {
        this.Past = past;
        this.Location = location;
        this.Season = season;
        this.Power = power;
        this.Type = type;
    }

    public override string ToString()
    {
        return $"{this.Power} {this.Type} {this.Location.Province} {this.Season}";
    }

    /// <summary>
    /// Create a new unit. No validation is performed; the adjudicator should only call this
    /// method after accepting a build order.
    /// </summary>
    public static Unit Build(Location location, Season season, Power power, UnitType type)
        => new Unit(past: null, location, season, power, type);

    /// <summary>
    /// Advance this unit's timeline to a new location and season.
    /// </summary>
    public Unit Next(Location location, Season season)
        => new Unit(past: this, location, season, this.Power, this.Type);
}
