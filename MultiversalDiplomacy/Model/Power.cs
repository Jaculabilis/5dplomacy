namespace MultiversalDiplomacy.Model;

/// <summary>
/// One of the rival nations vying for control of the map.
/// </summary>
public class Power
{
    /// <summary>
    /// The power's name.
    /// </summary>
    public string Name { get; }

    public Power(string name)
    {
        this.Name = name;
    }

    public override string ToString()
    {
        return this.Name;
    }
}
