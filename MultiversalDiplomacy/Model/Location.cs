namespace MultiversalDiplomacy.Model;

/// <summary>
/// Represents a locus of connectivity in a province. Land-locked and ocean/sea provinces
/// have one location. Coastal provinces have a land location and some number of named
/// water locations.
/// </summary>
public class Location
{
    /// <summary>
    /// The province to which this location belongs.
    public Province Province { get; }

    /// <summary>
    /// The location's full human-readable name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The location's shorthand abbreviation.
    /// </summary>
    public string? Abbreviation { get; }

    /// <summary>
    /// The location's type.
    /// </summary>
    public LocationType Type { get; }

    /// <summary>
    /// The locations that border this location.
    /// </summary>
    public IEnumerable<Location> Adjacents => this.AdjacentList;
    private List<Location> AdjacentList { get; set; }

    public Location(Province province, string? name, string? abbreviation, LocationType type)
    {
        this.Province = province;
        this.Name = name;
        this.Abbreviation = abbreviation;
        this.Type = type;
        this.AdjacentList = new List<Location>();
    }

    public override string ToString()
    {
        return this.Name == null
            ? $"{this.Province.Name} ({this.Type})"
            : $"{this.Province.Name} ({this.Type}:{this.Name}]";
    }

    /// <summary>
    /// Set another location as bordering this location.
    /// </summary>
    public void AddBorder(Location other)
    {
        if (!this.AdjacentList.Contains(other)) this.AdjacentList.Add(other);
        if (!other.AdjacentList.Contains(this)) other.AdjacentList.Add(this);
    }
}
