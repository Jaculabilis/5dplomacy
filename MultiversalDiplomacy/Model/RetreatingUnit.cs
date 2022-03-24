using System.Collections.ObjectModel;

namespace MultiversalDiplomacy.Model;

/// <summary>
/// Represents a unit that was dislodged and must retreat to another province.
/// </summary>
public class RetreatingUnit
{
    /// <summary>
    /// The unit that was dislodged.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// Locations to which the dislodged unit may retreat. A dislodged unit may not retreat into
    /// a province contested or held by another unit, nor into the province from which originated
    /// the dislodging unit.
    /// </summary>
    public ReadOnlyCollection<(Season season, Location location)> ValidRetreats { get; }

    public RetreatingUnit(Unit unit, List<(Season season, Location location)> validRetreats)
    {
        this.Unit = unit;
        this.ValidRetreats = new(validRetreats);
    }

    public override string ToString()
    {
        return $"{this.Unit} (retreating)";
    }
}
