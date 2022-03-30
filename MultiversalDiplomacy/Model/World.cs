using System.Collections.ObjectModel;

using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacy.Model;

/// <summary>
/// The global game state.
/// </summary>
public class World
{
    /// <summary>
    /// The game map.
    /// </summary>
    public ReadOnlyCollection<Province> Provinces { get; }

    /// <summary>
    /// The game powers.
    /// </summary>
    public ReadOnlyCollection<Power> Powers { get; }

    /// <summary>
    /// The state of the multiverse.
    /// </summary>
    public ReadOnlyCollection<Season> Seasons { get; }

    /// <summary>
    /// The first season of the game.
    /// </summary>
    public Season RootSeason { get; }

    /// <summary>
    /// All units in the multiverse.
    /// </summary>
    public ReadOnlyCollection<Unit> Units { get; }

    /// <summary>
    /// All retreating units in the multiverse.
    /// </summary>
    public ReadOnlyCollection<RetreatingUnit> RetreatingUnits { get; }

    /// <summary>
    /// Orders given to units in each season.
    /// </summary>
    public ReadOnlyDictionary<Season, ReadOnlyCollection<Order>> GivenOrders { get; }

    /// <summary>
    /// Immutable game options.
    /// </summary>
    public Options Options { get; }

    /// <summary>
    /// Create a new World, providing all state data.
    /// </summary>
    private World(
        ReadOnlyCollection<Province> provinces,
        ReadOnlyCollection<Power> powers,
        ReadOnlyCollection<Season> seasons,
        Season rootSeason,
        ReadOnlyCollection<Unit> units,
        ReadOnlyCollection<RetreatingUnit> retreatingUnits,
        ReadOnlyDictionary<Season, ReadOnlyCollection<Order>> givenOrders,
        Options options)
    {
        this.Provinces = provinces;
        this.Powers = powers;
        this.Seasons = seasons;
        this.RootSeason = rootSeason;
        this.Units = units;
        this.RetreatingUnits = retreatingUnits;
        this.GivenOrders = givenOrders;
        this.Options = options;
    }

    /// <summary>
    /// Create a new World from a previous one, replacing some state data.
    /// </summary>
    private World(
        World previous,
        ReadOnlyCollection<Province>? provinces = null,
        ReadOnlyCollection<Power>? powers = null,
        ReadOnlyCollection<Season>? seasons = null,
        ReadOnlyCollection<Unit>? units = null,
        ReadOnlyCollection<RetreatingUnit>? retreatingUnits = null,
        ReadOnlyDictionary<Season, ReadOnlyCollection<Order>>? givenOrders = null,
        Options? options = null)
        : this(
            provinces ?? previous.Provinces,
            powers ?? previous.Powers,
            seasons ?? previous.Seasons,
            previous.RootSeason,  // Can't change the root season
            units ?? previous.Units,
            retreatingUnits ?? previous.RetreatingUnits,
            givenOrders ?? previous.GivenOrders,
            options ?? previous.Options)
    {
    }

    /// <summary>
    /// Create a new world with specified provinces and powers and an initial season.
    /// </summary>
    public static World WithMap(IEnumerable<Province> provinces, IEnumerable<Power> powers)
    {
        Season root = Season.MakeRoot();
        return new World(
            new(provinces.ToList()),
            new(powers.ToList()),
            new(new List<Season> { root }),
            root,
            new(new List<Unit>()),
            new(new List<RetreatingUnit>()),
            new(new Dictionary<Season, ReadOnlyCollection<Order>>()),
            new Options());
    }

    /// <summary>
    /// Create a new world with the standard Diplomacy provinces and powers.
    /// </summary>
    public static World WithStandardMap()
        => WithMap(StandardProvinces, StandardPowers);

    public World Update(
        IEnumerable<Season>? seasons = null,
        IEnumerable<Unit>? units = null,
        IEnumerable<RetreatingUnit>? retreats = null,
        IEnumerable<KeyValuePair<Season, ReadOnlyCollection<Order>>>? orders = null)
        => new World(
            previous: this,
            seasons: seasons == null
                ? this.Seasons
                : new(seasons.ToList()),
            units: units == null
                ? this.Units
                : new(units.ToList()),
            retreatingUnits: retreats == null
                ? this.RetreatingUnits
                : new(retreats.ToList()),
            givenOrders: orders == null
                ? this.GivenOrders
                : new(orders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));

    /// <summary>
    /// Create a new world with new units created from unit specs. Units specs are in the format
    /// "<power> <A/F> <province> [<coast>]". If the province or coast name has a space in it, the
    /// abbreviation should be used. Unit specs always describe units in the root season.
    /// </summary>
    public World AddUnits(params string[] unitSpecs)
    {
        IEnumerable<Unit> units = unitSpecs.Select(spec =>
        {
            string[] splits = spec.Split(' ', 4);
            Power power = this.GetPower(splits[0]);
            UnitType type = splits[1] switch
            {
                "A" => UnitType.Army,
                "F" => UnitType.Fleet,
                _ => throw new ApplicationException($"Unknown unit type {splits[1]}")
            };
            Location location = type == UnitType.Army
                ? this.GetLand(splits[2])
                : splits.Length == 3
                    ? this.GetWater(splits[2])
                    : this.GetWater(splits[2], splits[3]);
            Unit unit = Unit.Build(location, this.RootSeason, power, type);
            return unit;
        });
        return this.Update(units: units);
    }

    /// <summary>
    /// Create a new world with standard Diplomacy initial unit placements.
    /// </summary>
    public World AddStandardUnits()
    {
        return this.AddUnits(
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

    /// <summary>
    /// A standard Diplomacy game setup.
    /// </summary>
    public static World Standard => World
        .WithStandardMap()
        .AddStandardUnits();

    /// <summary>
    /// Get a province by name. Throws if the province is not found.
    /// </summary>
    private Province GetProvince(string provinceName)
        => GetProvince(provinceName, this.Provinces);

    /// <summary>
    /// Get a province by name. Throws if the province is not found.
    /// </summary>
    private static Province GetProvince(string provinceName, IEnumerable<Province> provinces)
    {
        string provinceNameUpper = provinceName.ToUpperInvariant();
        Province? foundProvince = provinces.SingleOrDefault(
            p => p != null &&
                (p.Name.ToUpperInvariant() == provinceNameUpper
                || p.Abbreviations.Any(a => a.ToUpperInvariant() == provinceNameUpper)),
            null);
        if (foundProvince == null) throw new KeyNotFoundException(
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
        if (foundLocation == null) throw new KeyNotFoundException(
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
    /// Get a season by coordinate. Throws if the season is not found.
    /// </summary>
    public Season GetSeason(int turn, int timeline)
    {
        Season? foundSeason = this.Seasons.SingleOrDefault(
            s => s != null && s.Turn == turn && s.Timeline == timeline,
            null);
        if (foundSeason == null) throw new KeyNotFoundException(
            $"Season {turn}:{timeline} not found");
        return foundSeason;
    }

    /// <summary>
    /// Get a power by name. Throws if there is not exactly one such power.
    /// </summary>
    public Power GetPower(string powerName)
    {
        Power? foundPower = this.Powers.SingleOrDefault(
            p =>
                p != null
                && (p.Name == powerName || p.Name.StartsWith(powerName)),
            null);
        if (foundPower == null) throw new KeyNotFoundException(
            $"Power {powerName} not found");
        return foundPower;
    }

    /// <summary>
    /// Returns a unit in a province. Throws if there are duplicate units.
    /// </summary>
    public Unit GetUnitAt(string provinceName, (int turn, int timeline)? seasonCoord = null)
    {
        Province province = GetProvince(provinceName);
        seasonCoord ??= (this.RootSeason.Turn, this.RootSeason.Timeline);
        Season season = GetSeason(seasonCoord.Value.turn, seasonCoord.Value.timeline);
        Unit? foundUnit = this.Units.SingleOrDefault(
            u => u != null && u.Province == province && u.Season == season,
            null);
        if (foundUnit == null) throw new KeyNotFoundException(
            $"Unit at {province} at {season} not found");
        return foundUnit;
    }

    /// <summary>
    /// The standard Diplomacy provinces.
    /// </summary>
    public static ReadOnlyCollection<Province> StandardProvinces
    {
        get
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
            Location Land(string provinceName) => GetProvince(provinceName, standardProvinces)
                .Locations.Single(l => l.Type == LocationType.Land);
            Location Water(string provinceName) => GetProvince(provinceName, standardProvinces)
                .Locations.Single(l => l.Type == LocationType.Water);
            Location Coast(string provinceName, string coastName)
                => GetProvince(provinceName, standardProvinces)
                    .Locations.Single(l => l.Name == coastName || l.Abbreviation == coastName);

            static void AddBordersTo(Location location, Func<string, Location> LocationType, params string[] borders)
            {
                foreach (string bordering in borders)
                {
                    location.AddBorder(LocationType(bordering));
                }
            }
            void AddBorders(string provinceName, Func<string, Location> LocationType, params string[] borders)
                => AddBordersTo(LocationType(provinceName), LocationType, borders);

            AddBorders("NAF", Land, "TUN");
            AddBorders("NAF", Water, "MAO", "WES", "TUN");

            AddBorders("TUN", Land, "NAF");
            AddBorders("TUN", Water, "NAF", "WES", "TYS", "ION");

            AddBorders("BOH", Land, "MUN", "SIL", "GAL", "VIE", "TYR");

            AddBorders("BUD", Land, "VIE", "GAL", "RUM", "SER", "TRI");

            AddBorders("GAL", Land, "BOH", "SIL", "WAR", "UKR", "RUM", "BUD", "VIE");

            AddBorders("TRI", Land, "TYR", "VIE", "BUD", "SER", "ALB");
            AddBorders("TRI", Water, "ALB", "ADR", "VEN");

            AddBorders("TYR", Land, "MUN", "BOH", "VIE", "TRI", "VEN", "PIE");

            AddBorders("VIE", Land, "TYR", "BOH", "GAL", "BUD", "TRI");

            AddBorders("ALB", Land, "TRI", "SER", "GRE");
            AddBorders("ALB", Water, "TRI", "ADR", "ION", "GRE");

            AddBorders("BUL", Land, "GRE", "SER", "RUM", "CON");
            AddBordersTo(Coast("BUL", "ec"), Water, "BLA", "CON");
            AddBordersTo(Coast("BUL", "sc"), Water, "CON", "AEG", "GRE");

            AddBorders("GRE", Land, "ALB", "SER", "BUL");
            AddBorders("GRE", Water, "ALB", "ION", "AEG");
            Water("GRE").AddBorder(Coast("BUL", "sc"));

            AddBorders("RUM", Land, "BUL", "SER", "BUD", "GAL", "UKR", "SEV");
            AddBorders("RUM", Water, "SEV", "BLA");
            Water("RUM").AddBorder(Coast("BUL", "ec"));

            AddBorders("SER", Land, "BUD", "RUM", "BUL", "GRE", "ALB", "TRI");

            AddBorders("CLY", Land, "EDI", "LVP");
            AddBorders("CLY", Water, "LVP", "NAO", "NWG", "EDI");

            AddBorders("EDI", Land, "YOR", "LVP", "CLY");
            AddBorders("EDI", Water, "CLY", "NWG", "NTH", "YOR");

            AddBorders("LVP", Land, "CLY", "EDI", "YOR", "WAL");
            AddBorders("LVP", Water, "WAL", "IRS", "NAO", "CLY");

            AddBorders("LON", Land, "WAL", "YOR");
            AddBorders("LON", Water, "WAL", "ENC", "NTH", "YOR");

            AddBorders("WAL", Land, "LVP", "YOR", "LON");
            AddBorders("WAL", Water, "LON", "ENC", "IRS", "LVP");

            AddBorders("YOR", Land, "LON", "WAL", "LVP", "EDI");
            AddBorders("YOR", Water, "EDI", "NTH", "LON");

            AddBorders("BRE", Land, "PIC", "PAR", "GAS");
            AddBorders("BRE", Water, "GAS", "MAO", "ENC", "PIC");

            AddBorders("BUR", Land, "BEL", "RUH", "MUN", "MAR", "GAS", "PAR", "PIC");

            AddBorders("GAS", Land, "BRE", "PAR", "BUR", "MAR", "SPA");
            AddBorders("GAS", Water, "MAO", "BRE");
            Water("GAS").AddBorder(Coast("SPA", "nc"));

            AddBorders("MAR", Land, "SPA", "GAS", "BUR", "PIE");
            AddBorders("MAR", Water, "LYO", "PIE");
            Water("MAR").AddBorder(Coast("SPA", "sc"));

            AddBorders("PAR", Land, "PIC", "BUR", "GAS", "BRE");

            AddBorders("PIC", Land, "BEL", "BUR", "PAR", "BRE");
            AddBorders("PIC", Water, "BRE", "ENC", "BEL");

            AddBorders("BER", Land, "PRU", "SIL", "MUN", "KIE");
            AddBorders("BER", Water, "KIE", "BAL", "PRU");

            AddBorders("KIE", Land, "BER", "MUN", "RUH", "HOL", "DEN");
            AddBorders("KIE", Water, "HOL", "HEL", "DEN", "BAL", "BER");

            AddBorders("MUN", Land, "BUR", "RUH", "KIE", "BER", "SIL", "BOH", "TYR");

            AddBorders("PRU", Land, "LVN", "WAR", "SIL", "BER");
            AddBorders("PRU", Water, "BER", "BAL", "LVN");

            AddBorders("RUH", Land, "KIE", "MUN", "BUR", "BEL", "HOL");

            AddBorders("SIL", Land, "PRU", "WAR", "GAL", "BOH", "MUN", "BER");

            AddBorders("SPA", Land, "POR", "GAS", "MAR");
            AddBordersTo(Coast("SPA", "nc"), Water, "POR", "MAO", "GAS");
            AddBordersTo(Coast("SPA", "sc"), Water, "POR", "MAO", "WES", "LYO", "MAR");

            AddBorders("POR", Land, "SPA");
            AddBorders("POR", Water, "MAO");
            Water("POR").AddBorder(Coast("SPA", "nc"));
            Water("POR").AddBorder(Coast("SPA", "sc"));

            AddBorders("APU", Land, "NAP", "ROM", "VEN");
            AddBorders("APU", Water, "VEN", "ADR", "IOS", "NAP");

            AddBorders("NAP", Land, "ROM", "APU");
            AddBorders("NAP", Water, "APU", "IOS", "TYS", "ROM");

            AddBorders("PIE", Land, "MAR", "TYR", "VEN", "TUS");
            AddBorders("PIE", Water, "TUS", "LYO", "MAR");

            AddBorders("ROM", Land, "TUS", "VEN", "APU", "NAP");
            AddBorders("ROM", Water, "NAP", "TYS", "TUS");

            AddBorders("TUS", Land, "PIE", "VEN", "ROM");
            AddBorders("TUS", Water, "ROM", "TYS", "LYO", "PIE");

            AddBorders("VEN", Land, "APU", "ROM", "TUS", "PIE", "TYR", "TRI");
            AddBorders("VEN", Water, "TRI", "ADR", "APU");

            AddBorders("BEL", Land, "HOL", "RUH", "BUR", "PIC");
            AddBorders("BEL", Water, "PIC", "ENC", "NTH", "HOL");

            AddBorders("HOL", Land, "BEL", "RUH", "KIE");
            AddBorders("HOL", Water, "NTH", "HEL");

            AddBorders("FIN", Land, "SWE", "NWY", "STP");
            AddBorders("FIN", Water, "SWE", "BOT");
            Water("FIN").AddBorder(Coast("STP", "wc"));

            AddBorders("LVN", Land, "STP", "MOS", "WAR", "PRU");
            AddBorders("LVN", Water, "PRU", "BAL", "BOT");
            Water("LVN").AddBorder(Coast("STP", "wc"));

            AddBorders("MOS", Land, "SEV", "UKR", "WAR", "LVN", "STP");

            AddBorders("SEV", Land, "RUM", "UKR", "MOS", "ARM");
            AddBorders("SEV", Water, "ARM", "BLA", "RUM");

            AddBorders("STP", Land, "MOS", "LVN", "FIN");
            AddBordersTo(Coast("STP", "nc"), Water, "BAR", "NWY");
            AddBordersTo(Coast("STP", "wc"), Water, "LVN", "BOT", "FIN");

            AddBorders("UKR", Land, "MOS", "SEV", "RUM", "GAL", "WAR");

            AddBorders("WAR", Land, "PRU", "LVN", "MOS", "UKR", "GAL", "SIL");

            AddBorders("DEN", Land, "KIE", "SWE");
            AddBorders("DEN", Water, "KIE", "HEL", "NTH", "SKA", "BAL", "SWE");

            AddBorders("NWY", Land, "STP", "FIN", "SWE");
            AddBorders("NWY", Water, "BAR", "NWG", "NTH", "SKA", "SWE");
            Water("NWY").AddBorder(Coast("STP", "nc"));

            AddBorders("SWE", Land, "NWY", "FIN", "DEN");
            AddBorders("SWE", Water, "FIN", "BOT", "BAL", "DEN", "SKA", "NWY");

            AddBorders("ANK", Land, "ARM", "SMY", "CON");
            AddBorders("ANK", Water, "CON", "BLA", "ARM");

            AddBorders("ARM", Land, "SEV", "SYR", "SMY", "ANK");
            AddBorders("ARM", Water, "ANK", "BLA", "SEV");

            AddBorders("CON", Land, "BUL", "ANK", "SMY");
            AddBorders("CON", Water, "BLA", "ANK", "SMY", "AEG");
            Water("CON").AddBorder(Coast("BUL", "ec"));
            Water("CON").AddBorder(Coast("BUL", "sc"));

            AddBorders("SMY", Land, "CON", "ANK", "ARM", "SYR");
            AddBorders("SMY", Water, "SYR", "EAS", "AEG", "CON");

            AddBorders("SYR", Land, "SMY", "ARM");
            AddBorders("SYR", Water, "EAS", "SMY");

            AddBorders("BAR", Water, "NWG", "NWY");
            Water("BAR").AddBorder(Coast("STP", "nc"));

            AddBorders("ENC", Water, "LON", "NTH", "BEL", "PIC", "BRE", "MAO", "IRS", "WAL");

            AddBorders("HEL", Water, "NTH", "DEN", "BAL", "KIE", "HOL");

            AddBorders("IRS", Water, "NAO", "LVP", "WAL", "ENC", "MAO");

            AddBorders("MAO", Water, "NAO", "IRS", "ENC", "BRE", "GAS", "POR", "NAF");
            Water("MAO").AddBorder(Coast("SPA", "nc"));
            Water("MAO").AddBorder(Coast("SPA", "sc"));

            AddBorders("NAO", Water, "NWG", "CLY", "LVP", "IRS", "MAO");

            AddBorders("NTH", Water, "NWG", "NWY", "SKA", "DEN", "HEL", "HOL", "BEL", "ENC", "LON", "YOR", "EDI");

            AddBorders("NWG", Water, "BAR", "NWY", "NTH", "EDI", "CLY", "NAO");

            AddBorders("SKA", Water, "NWY", "SWE", "BAL", "DEN", "NTH");

            AddBorders("BAL", Water, "BOT", "LVN", "PRU", "BER", "KIE", "HEL", "DEN", "SWE");

            AddBorders("BOT", Water, "LVN", "BAL", "SWE", "FIN");
            Water("BOT").AddBorder(Coast("STP", "wc"));

            AddBorders("ADR", Water, "IOS", "APU", "VEN", "TRI", "ALB");

            AddBorders("AEG", Water, "CON", "SMY", "EAS", "IOS", "GRE");
            Water("AEG").AddBorder(Coast("BUL", "sc"));

            AddBorders("BLA", Water, "RUM", "SEV", "ARM", "ANK", "CON");
            Water("BLA").AddBorder(Coast("BUL", "ec"));

            AddBorders("EAS", Water, "IOS", "AEG", "SMY", "SYR");

            AddBorders("LYO", Water, "MAR", "PIE", "TUS", "TYS", "WES");
            Water("LYO").AddBorder(Coast("SPA", "sc"));

            AddBorders("IOS", Water, "TUN", "TYS", "NAP", "APU", "ADR", "ALB", "GRE", "AEG");

            AddBorders("TYS", Water, "LYO", "TUS", "ROM", "NAP", "IOS", "TUN", "WES");

            AddBorders("WES", Water, "LYO", "TYS", "TUN", "NAF", "MAO");
            Water("WES").AddBorder(Coast("SPA", "sc"));

            return new(standardProvinces);
        }
    }

    /// <summary>
    /// The standard Diplomacy powers.
    /// </summary>
    public static ReadOnlyCollection<Power> StandardPowers
    {
        get => new(new List<Power>
        {
            new Power("Austria"),
            new Power("England"),
            new Power("France"),
            new Power("Germany"),
            new Power("Italy"),
            new Power("Russia"),
            new Power("Turkey"),
        });
    }
}