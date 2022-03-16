namespace MultiversalDiplomacy.Model;

/// <summary>
/// Represents a single province as it exists across all timelines.
/// </summary>
public class Province
{
    /// <summary>
    /// The province's full human-readable name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The province's shorthand abbreviation.
    /// </summary>
    public string[] Abbreviations { get; }

    /// <summary>
    /// Whether the province contains a supply center.
    /// </summary>
    public bool IsSupplyCenter { get; }

    /// <summary>
    /// Whether the province contains a time center.
    /// </summary>
    public bool IsTimeCenter { get; }

    /// <summary>
    /// The occupiable locations in this province. Only one location can be occupied at a time.
    /// </summary>
    public IEnumerable<Location> Locations => LocationList;
    private List<Location> LocationList { get; set; }

    public Province(string name, string[] abbreviations, bool isSupply, bool isTime)
    {
        this.Name = name;
        this.Abbreviations = abbreviations;
        this.IsSupplyCenter = isSupply;
        this.IsTimeCenter = isTime;
        this.LocationList = new List<Location>();
    }

    public override string ToString()
    {
        return this.Name;
    }

    /// <summary>
    /// Create a new province with no supply center.
    /// </summary>
    public static Province Empty(string name, params string[] abbreviations)
        => new Province(name, abbreviations, isSupply: false, isTime: false);

    /// <summary>
    /// Create a new province with a supply center.
    /// </summary>
    public static Province Supply(string name, params string[] abbreviations)
        => new Province(name, abbreviations, isSupply: true, isTime: false);

    /// <summary>
    /// Create a new province with a time center.
    /// </summary>
    public static Province Time(string name, params string[] abbreviations)
        => new Province(name, abbreviations, isSupply: true, isTime: true);

    /// <summary>
    /// Create a new land location in this province.
    /// </summary>
    public Province AddLandLocation()
    {
        Location location = new Location(this, name: null, abbreviation: null, LocationType.Land);
        this.LocationList.Add(location);
        return this;
    }

    /// <summary>
    /// Create a new ocean location.
    /// </summary>
    public Province AddOceanLocation()
    {
        Location location = new Location(this, name: null, abbreviation: null, LocationType.Water);
        this.LocationList.Add(location);
        return this;
    }

    /// <summary>
    /// Create a new coastal location. Coastal locations must have names to disambiguate them
    /// from the single land location in coastal provinces.
    /// </summary>
    public Province AddCoastLocation()
    {
        // Use a default name for provinces with only one coastal location
        Location location = new Location(this, "coast", "c", LocationType.Water);
        this.LocationList.Add(location);
        return this;
    }

    /// <summary>
    /// Create a new coastal location. Coastal locations must have names to disambiguate them
    /// from the single land location in coastal provinces.
    /// </summary>
    public Province AddCoastLocation(string name, string abbreviation)
    {
        Location location = new Location(this, name, abbreviation, LocationType.Water);
        this.LocationList.Add(location);
        return this;
    }
}
