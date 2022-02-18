namespace MultiversalDiplomacy.Model;

/// <summary>
/// The type of a unit.
/// </summary>
public enum UnitType
{
    /// <summary>
    /// A unit that moves on land.
    /// </summary>
    Army = 0,

    /// <summary>
    /// A unit that moves in oceans and along coasts and can convoy armies.
    /// </summary>
    Fleet = 1,
}
