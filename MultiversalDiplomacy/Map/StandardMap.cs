using System.Reflection;

using MultiversalDiplomacy.Model;

namespace MultiversalDiplomacy.Map;

/// <summary>
/// The standard Diplomacy map.
/// </summary>
public class StandardMap : Map
{
    public override IEnumerable<Province> Provinces { get; }

    public static StandardMap Instance { get; } = new StandardMap();

    private StandardMap()
    {
        this.Provinces = new List<Province>()
        {
            Province.Empty("North Africa", "NAF")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Tunis", "TUN")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Bohemia", "BOH")
                .AddLandLocation(),
            Province.Supply("Budapest", "BUD")
                .AddLandLocation(),
            Province.Empty("Galacia", "GAL")
                .AddLandLocation(),
            Province.Supply("Trieste", "TRI")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Tyrolia", "TYR")
                .AddLandLocation(),
            Province.Time("Vienna", "VIE")
                .AddLandLocation(),
            Province.Empty("Albania", "ALB")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Bulgaria", "BUL")
                .AddLandLocation()
                .AddCoastLocation("east coast", "ec")
                .AddCoastLocation("south coast", "sc"),
            Province.Supply("Greece", "GRE")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Rumania", "RUM", "RMA")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Serbia", "SER")
                .AddLandLocation(),
            Province.Empty("Clyde", "CLY")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Edinburgh", "EDI")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Liverpool", "LVP", "LPL")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("London", "LON")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Wales", "WAL")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Yorkshire", "YOR")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Brest", "BRE")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Burgundy", "BUR")
                .AddLandLocation(),
            Province.Empty("Gascony", "GAS")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Marseilles", "MAR")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("Paris", "PAR")
                .AddLandLocation(),
            Province.Empty("Picardy", "PIC")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("Berlin", "BER")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Kiel", "KIE")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Munich", "MUN")
                .AddLandLocation(),
            Province.Empty("Prussia", "PRU")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Ruhr", "RUH", "RHR")
                .AddLandLocation(),
            Province.Empty("Silesia", "SIL")
                .AddLandLocation(),
            Province.Supply("Spain", "SPA")
                .AddLandLocation()
                .AddCoastLocation("north coast", "nc")
                .AddCoastLocation("south coast", "sc"),
            Province.Supply("Portugal", "POR")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Apulia", "APU")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Naples", "NAP")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Piedmont", "PIE")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("Rome", "ROM", "RME")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Tuscany", "TUS")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Venice", "VEN")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Belgium", "BEL")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Holland", "HOL")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Finland", "FIN")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Livonia", "LVN", "LVA")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("Moscow", "MOS")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Sevastopol", "SEV")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Saint Petersburg", "STP")
                .AddLandLocation()
                .AddCoastLocation("north coast", "nc")
                .AddCoastLocation("west coast", "wc"),
            Province.Empty("Ukraine", "UKR")
                .AddLandLocation(),
            Province.Supply("Warsaw", "WAR")
                .AddLandLocation(),
            Province.Supply("Denmark", "DEN")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Norway", "NWY")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Sweden", "SWE")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Ankara", "ANK")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Armenia", "ARM")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Time("Constantinople", "CON")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Supply("Smyrna", "SMY")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Syria", "SYR")
                .AddLandLocation()
                .AddCoastLocation(),
            Province.Empty("Barents Sea", "BAR")
                .AddOceanLocation(),
            Province.Empty("English Channel", "ENC", "ECH")
                .AddOceanLocation(),
            Province.Empty("Heligoland Bight", "HEL", "HGB")
                .AddOceanLocation(),
            Province.Empty("Irish Sea", "IRS", "IRI")
                .AddOceanLocation(),
            Province.Empty("Mid-Atlantic Ocean", "MAO", "MID")
                .AddOceanLocation(),
            Province.Empty("North Atlantic Ocean", "NAO", "NAT")
                .AddOceanLocation(),
            Province.Empty("North Sea", "NTH", "NTS")
                .AddOceanLocation(),
            Province.Empty("Norwegian Sea", "NWS", "NWG")
                .AddOceanLocation(),
            Province.Empty("Skagerrak", "SKA", "SKG")
                .AddOceanLocation(),
            Province.Empty("Baltic Sea", "BAL")
                .AddOceanLocation(),
            Province.Empty("Guld of Bothnia", "GOB", "BOT")
                .AddOceanLocation(),
            Province.Empty("Adriatic Sea", "ADS", "ADR")
                .AddOceanLocation(),
            Province.Empty("Aegean Sea", "AEG")
                .AddOceanLocation(),
            Province.Empty("Black Sea", "BLA")
                .AddOceanLocation(),
            Province.Empty("Eastern Mediterranean Sea", "EMS", "EAS")
                .AddOceanLocation(),
            Province.Empty("Gulf of Lyons", "GOL", "LYO")
                .AddOceanLocation(),
            Province.Empty("Ionian Sea", "IOS", "ION", "INS")
                .AddOceanLocation(),
            Province.Empty("Tyrrhenian Sea", "TYS", "TYN")
                .AddOceanLocation(),
            Province.Empty("Western Mediterranean Sea", "WMS", "WES")
                .AddOceanLocation(),
        };

        Land("NAF").AddBorder(Land("TUN"));
        Water("NAF").AddBorder(Water("MAO"));
        Water("NAF").AddBorder(Water("WES"));
        Water("NAF").AddBorder(Water("TUN"));

        Land("TUN").AddBorder(Land("NAF"));
        Water("TUN").AddBorder(Water("NAF"));
        Water("TUN").AddBorder(Water("WES"));
        Water("TUN").AddBorder(Water("TYS"));
        Water("TUN").AddBorder(Water("ION"));

        Land("BOH").AddBorder(Land("MUN"));
        Land("BOH").AddBorder(Land("SIL"));
        Land("BOH").AddBorder(Land("GAL"));
        Land("BOH").AddBorder(Land("VIE"));
        Land("BOH").AddBorder(Land("TYR"));

        Land("BUD").AddBorder(Land("VIE"));
        Land("BUD").AddBorder(Land("GAL"));
        Land("BUD").AddBorder(Land("RUM"));
        Land("BUD").AddBorder(Land("SER"));
        Land("BUD").AddBorder(Land("TRI"));

        Land("GAL").AddBorder(Land("BOH"));
        Land("GAL").AddBorder(Land("SIL"));
        Land("GAL").AddBorder(Land("WAR"));
        Land("GAL").AddBorder(Land("UKR"));
        Land("GAL").AddBorder(Land("RUM"));
        Land("GAL").AddBorder(Land("BUD"));
        Land("GAL").AddBorder(Land("VIE"));

        Land("TRI").AddBorder(Land("VEN"));
        Land("TRI").AddBorder(Land("TYR"));
        Land("TRI").AddBorder(Land("VIE"));
        Land("TRI").AddBorder(Land("BUD"));
        Land("TRI").AddBorder(Land("SER"));
        Land("TRI").AddBorder(Land("ALB"));
        Water("TRI").AddBorder(Water("ALB"));
        Water("TRI").AddBorder(Water("ADR"));
        Water("TRI").AddBorder(Water("VEN"));

        Land("TYR").AddBorder(Land("MUN"));
        Land("TYR").AddBorder(Land("BOH"));
        Land("TYR").AddBorder(Land("VIE"));
        Land("TYR").AddBorder(Land("TRI"));
        Land("TYR").AddBorder(Land("VEN"));
        Land("TYR").AddBorder(Land("PIE"));

        Land("VIE").AddBorder(Land("TYR"));
        Land("VIE").AddBorder(Land("BOH"));
        Land("VIE").AddBorder(Land("GAL"));
        Land("VIE").AddBorder(Land("BUD"));
        Land("VIE").AddBorder(Land("TRI"));

        Land("ALB").AddBorder(Land("TRI"));
        Land("ALB").AddBorder(Land("SER"));
        Land("ALB").AddBorder(Land("GRE"));
        Water("ALB").AddBorder(Water("TRI"));
        Water("ALB").AddBorder(Water("ADR"));
        Water("ALB").AddBorder(Water("ION"));
        Water("ALB").AddBorder(Water("GRE"));

        Land("BUL").AddBorder(Land("GRE"));
        Land("BUL").AddBorder(Land("SER"));
        Land("BUL").AddBorder(Land("RUM"));
        Land("BUL").AddBorder(Land("CON"));
        Coast("BUL", "ec").AddBorder(Water("BLA"));
        Coast("BUL", "ec").AddBorder(Water("CON"));
        Coast("BUL", "sc").AddBorder(Water("CON"));
        Coast("BUL", "sc").AddBorder(Water("AEG"));
        Coast("BUL", "sc").AddBorder(Water("GRE"));

        Land("GRE").AddBorder(Land("ALB"));
        Land("GRE").AddBorder(Land("SER"));
        Land("GRE").AddBorder(Land("BUL"));
        Water("GRE").AddBorder(Water("ALB"));
        Water("GRE").AddBorder(Water("ION"));
        Water("GRE").AddBorder(Water("AEG"));
        Water("GRE").AddBorder(Coast("BUL", "sc"));

        // TODO

        Water("IOS").AddBorder(Water("TUN"));
        Water("IOS").AddBorder(Water("TYS"));
        Water("IOS").AddBorder(Water("NAP"));
        Water("IOS").AddBorder(Water("APU"));
        Water("IOS").AddBorder(Water("ADR"));
        Water("IOS").AddBorder(Water("ALB"));
        Water("IOS").AddBorder(Water("GRE"));
        Water("IOS").AddBorder(Water("AEG"));

        // TODO
    }
}
