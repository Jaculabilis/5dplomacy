namespace MultiversalDiplomacy.Model;

/// <summary>
/// The global game state.
/// </summary>
public class World
{
    /// <summary>
    /// The game map.
    /// </summary>
    public IEnumerable<Province> Provinces { get; }

    /// <summary>
    /// The game powers.
    /// </summary>
    public IEnumerable<Power> Powers { get; }

    /// <summary>
    /// The state of the multiverse.
    /// </summary>
    public IEnumerable<Season> Seasons { get; }

    /// <summary>
    /// All units in the multiverse.
    /// </summary>
    public IEnumerable<Unit> Units { get; }

    public World(
        IEnumerable<Province> provinces,
        IEnumerable<Power> powers,
        IEnumerable<Season> seasons,
        IEnumerable<Unit> units)
    {
        this.Provinces = provinces;
        this.Powers = powers;
        this.Seasons = seasons;
        this.Units = units;
    }
}