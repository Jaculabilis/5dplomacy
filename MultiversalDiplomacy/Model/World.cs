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

    /// <summary>
    /// Immutable game options.
    /// </summary>
    public Options Options { get; }

    public World(
        IEnumerable<Province> provinces,
        IEnumerable<Power> powers,
        IEnumerable<Season> seasons,
        IEnumerable<Unit> units,
        Options options)
    {
        this.Provinces = provinces;
        this.Powers = powers;
        this.Seasons = seasons;
        this.Units = units;
        this.Options = options;
    }

    /// <summary>
    /// Create a new world with no map, powers, or units, and a root season.
    /// </summary>
    public static World Empty => new World(
        new List<Province>(),
        new List<Power>(),
        new List<Season> { Season.MakeRoot() },
        new List<Unit>(),
        new Options());

    /// <summary>
    /// Create a world with a standard map, powers, and initial unit placements.
    /// </summary>
    public static World Standard => Empty
        .WithStandardMap()
        .WithStandardPowers()
        .WithStandardUnits();

    /// <summary>
    /// Get a province by name. Throws if the province is not found.
    /// </summary>
    private Province GetProvince(string provinceName)
    {
        string provinceNameUpper = provinceName.ToUpperInvariant();
        Province? foundProvince = this.Provinces.FirstOrDefault(
            p => p != null &&
                (p.Name.ToUpperInvariant() == provinceNameUpper
                || p.Abbreviations.Any(a => a.ToUpperInvariant() == provinceNameUpper)),
            null);
        if (foundProvince == null) throw new ArgumentOutOfRangeException(
            $"Province {provinceName} not found");
        return foundProvince;
    }

    /// <summary>
    /// Get the location in a province matching a predicate. Throws if there is not exactly one
    /// such location.
    /// </summary>
    private Location GetLocation(string provinceName, Func<Location, bool> predicate)
    {
        Location? foundLocation = GetProvince(provinceName).Locations.SingleOrDefault(
            l => l != null && predicate(l), null);
        if (foundLocation == null) throw new ArgumentException(
            $"No such location in {provinceName}");
        return foundLocation;
    }

    /// <summary>
    /// Get the sole land location of a province.
    /// </summary>
    public Location GetLand(string provinceName)
        => GetLocation(provinceName, l => l.Type == LocationType.Land);

    /// <summary>
    /// Get the sole water location of a province, optionally specifying a named coast.
    /// </summary>
    public Location GetWater(string provinceName, string? coastName = null)
        => coastName == null
            ? GetLocation(provinceName, l => l.Type == LocationType.Water)
            : GetLocation(provinceName, l => l.Name == coastName || l.Abbreviation == coastName);

    /// <summary>
    /// Get a power by name. Throws if the power is not found.
    /// </summary>
    public Power GetPower(string powerName)
    {
        Power? foundPower = this.Powers.FirstOrDefault(
            p =>
                p != null
                && (p.Name == powerName || p.Name.StartsWith(powerName)),
            null);
        if (foundPower == null) throw new ArgumentOutOfRangeException(
            $"Power {powerName} not found");
        return foundPower;
    }

    /// <summary>
    /// Create a new world from this one with new provinces.
    /// </summary>
    public World WithMap(IEnumerable<Province> provinces)
    {
        if (this.Units.Any()) throw new InvalidOperationException(
            "Provinces cannot be changed once units have been placed on the map");
        return new World(provinces, this.Powers, this.Seasons, this.Units, this.Options);
    }

    /// <summary>
    /// Create a new world from this one with the standard Diplomacy provinces.
    /// </summary>
    public World WithStandardMap()
    {
        // Define the provinces of the standard world map.
        List<Province> standardProvinces = new List<Province>
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

        // Declare some helpers for border definitions
        Location Land(string provinceName) => standardProvinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Type == LocationType.Land);
        Location Water(string provinceName) => standardProvinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Type == LocationType.Water);
        Location Coast(string provinceName, string coastName) => standardProvinces
            .Single(p => p.Name == provinceName || p.Abbreviations.Contains(provinceName))
            .Locations.Single(l => l.Name == coastName || l.Abbreviation == coastName);

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

        return this.WithMap(standardProvinces);
    }

    /// <summary>
    /// Create a new world from this one with new powers.
    /// </summary>
    public World WithPowers(IEnumerable<Power> powers)
        => new World(this.Provinces, powers, this.Seasons, this.Units, this.Options);

    /// <summary>
    /// Create a new world from this one with new powers created with the given names.
    /// </summary>
    public World WithPowers(IEnumerable<string> powerNames)
        => WithPowers(powerNames.Select(name => new Model.Power(name)));

    /// <summary>
    /// Create a new world from this one with new powers created with the given names.
    /// </summary>
    public World WithPowers(params string[] powerNames)
        => WithPowers(powerNames.AsEnumerable());

    /// <summary>
    /// Create a new world from this one with the standard Diplomacy powers.
    /// </summary>
    public World WithStandardPowers()
        => WithPowers("Austria", "England", "France", "Germany", "Italy", "Russia", "Turkey");

    /// <summary>
    /// Create a new world from this one with new units created from unit specs. Units specs are
    /// in the format "<power> <A/F> <province> [<coast>]". If the province or coast name has a
    /// space in it, the abbreviation should be used.
    /// </summary>
    public World WithUnits(IEnumerable<string> unitSpec)
    {
        IEnumerable<Unit> units = unitSpec.Select(spec =>
        {
            string[] splits = spec.Split(' ', 4);
            Power power = this.GetPower(splits[0]);
            UnitType type = splits[1] switch
            {
                "A" => UnitType.Army,
                "F" => UnitType.Fleet,
                _ => throw new ArgumentOutOfRangeException($"Unknown unit type {splits[1]}")
            };
            Location location = type == UnitType.Army
                ? this.GetLand(splits[2])
                : splits.Length == 3
                    ? this.GetWater(splits[2])
                    : this.GetWater(splits[2], splits[3]);
            Unit unit = Unit.Build(location, this.Seasons.First(), power, type);
            return unit;
        });
        return new World(this.Provinces, this.Powers, this.Seasons, units, this.Options);
    }

    /// <summary>
    /// Create a new world from this one with new units created from unit specs. Units specs are
    /// in the format "<power> <A/F> <province> [<coast>]".
    /// </summary>
    public World WithUnits(params string[] unitSpec)
        => this.WithUnits(unitSpec.AsEnumerable());

    /// <summary>
    /// Create a new world from this one with new units created according to the standard Diplomacy
    /// initial unit deployments.
    /// </summary>
    public World WithStandardUnits()
    {
        return this.WithUnits(
            "Austria A Bud",
            "Austria A Vir",
            "Austria F Tri",
            "England A Lvp",
            "England F Edi",
            "England F Lon",
            "France A Mar",
            "France A Par",
            "France F Bre",
            "Germany A Ber",
            "Germany A Mun",
            "Germany F Kie",
            "Italy A Rom",
            "Italy A Ven",
            "Italy F Nap",
            "Russia A Mos",
            "Russia A War",
            "Russia F Sev",
            "Russia F Stp wc",
            "Turkey A Con",
            "Turkey A Smy",
            "Turkey F Ank"
        );
    }
}