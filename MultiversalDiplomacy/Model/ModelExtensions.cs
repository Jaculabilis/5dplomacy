namespace MultiversalDiplomacy.Model;

public static class ModelExtensions
{
    /// <summary>
    /// Short representation of a <see cref="UnitType"/>.
    /// </summary>
    public static string ToShort(this UnitType unitType)
        => unitType switch
        {
            UnitType.Army => "A",
            UnitType.Fleet => "F",
            _ => throw new NotSupportedException($"Unknown unit type {unitType}"),
        };

    /// <summary>
    /// Short representation of a multiversal location.
    /// </summary>
    public static string ToShort(this (Province province, Season season) coord)
    {
        return $"{coord.season.Timeline}-{coord.province.Abbreviations[0]}@{coord.season.Turn}";
    }
}