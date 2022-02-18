using MultiversalDiplomacy.Map;
using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacyTests;

public class TestMap : Map
{
    public override IEnumerable<Province> Provinces { get; }

    public static TestMap Instance { get; } = new TestMap();

    private TestMap()
    {
        Provinces = new List<Province>()
        {
            Province.Supply("Left", "LEF")
                .AddLandLocation(),
            Province.Supply("Right", "RIG")
                .AddLandLocation(),
            Province.Empty("Center", "CEN")
                .AddLandLocation(),
        };
        Land("LEF").AddBorder(Land("RIG"));
        Land("LEF").AddBorder(Land("CEN"));

        Land("RIG").AddBorder(Land("LEF"));
        Land("RIG").AddBorder(Land("CEN"));

        Land("CEN").AddBorder(Land("LEF"));
        Land("CEN").AddBorder(Land("RIG"));
    }
}
