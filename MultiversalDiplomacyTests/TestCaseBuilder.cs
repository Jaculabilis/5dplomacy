using System.Collections.ObjectModel;

using MultiversalDiplomacy.Adjudicate;
using MultiversalDiplomacy.Model;
using MultiversalDiplomacy.Orders;

namespace MultiversalDiplomacyTests;

/// <summary>
/// A fluent interface for defining adjudication test cases.
/// </summary>
public class TestCaseBuilder
{
    /// <summary>
    /// Context for choosing a season to define orders for.
    /// </summary>
    public interface ISeasonContext
    {
        /// <summary>
        /// Choose a new season to define orders for.
        /// </summary>
        public ISeasonContext this[(int turn, int timeline) seasonCoord] { get; }

        /// <summary>
        /// Get the context for defining the orders for a power.
        /// </summary>
        public IPowerContext this[string powerName] { get; }

        /// <summary>
        /// Save a reference to this season.
        /// </summary>
        public ISeasonContext GetReference(out Season season);
    }

    /// <summary>
    /// Context for defining orders given by a power.
    /// </summary>
    public interface IPowerContext
    {
        /// <summary>
        /// Choose a new season to define orders for.
        /// </summary>
        public ISeasonContext this[(int turn, int timeline) seasonCoord] { get; }

        /// <summary>
        /// Get the context for defining the orders for another power.
        /// </summary>
        public IPowerContext this[string powerName] { get; }

        /// <summary>
        /// Define an order for an army in a province.
        /// </summary>
        public IUnitContext Army(string provinceName, string? powerName = null);

        /// <summary>
        /// Define an order for a fleet in a province, optionally on a specific coast.
        /// </summary>
        public IUnitContext Fleet(string provinceName, string? coast = null, string? powerName = null);
    }

    /// <summary>
    /// Context for defining an order given to a unit.
    /// </summary>
    public interface IUnitContext
    {
        /// <summary>
        /// Ensure the unit exists, but don't create an order for it.
        /// </summary>
        public IPowerContext Exists();

        /// <summary>
        /// Give the unit a hold order.
        /// </summary>
        public IOrderDefinedContext<HoldOrder> Holds();

        /// <summary>
        /// Give the unit a move order.
        /// </summary>
        /// <param name="season">
        /// The destination season. If not specified, defaults to the same season as the unit.
        /// </param>
        public IOrderDefinedContext<MoveOrder> MovesTo(
            string provinceName,
            Season? season = null,
            string? coast = null);

        /// <summary>
        /// Give the unit a convoy order.
        /// </summary>
        public IConvoyContext Convoys { get; }

        /// <summary>
        /// Give the unit a support order.
        /// </summary>
        public ISupportContext Supports { get; }
    }

    /// <summary>
    /// Context for defining a convoy order.
    /// </summary>
    public interface IConvoyContext
    {
        /// <summary>
        /// Make the convoy order target an army.
        /// </summary>
        public IConvoyDestinationContext Army(string provinceName, string? powerName = null);

        /// <summary>
        /// Make the convoy order target a fleet.
        /// </summary>
        public IConvoyDestinationContext Fleet(
            string provinceName,
            string? coast = null,
            string? powerName = null);
    }

    /// <summary>
    /// Context for defining the destination of a convoy order.
    /// </summary>
    public interface IConvoyDestinationContext
    {
        /// <summary>
        /// Define the destination of the convoy order.
        /// </summary>
        public IOrderDefinedContext<ConvoyOrder> To(string provinceName);
    }

    /// <summary>
    /// Context for defining a support order.
    /// </summary>
    public interface ISupportContext
    {
        /// <summary>
        /// Make the support order target an army.
        /// </summary>
        public ISupportTypeContext Army(string provinceName, string? powerName = null);

        /// <summary>
        /// Make the support order target a fleet.
        /// </summary>
        public ISupportTypeContext Fleet(
            string provinceName,
            string? coast = null,
            string? powerName = null);
    }

    /// <summary>
    /// Context for defining the type of support order.
    /// </summary>
    public interface ISupportTypeContext
    {
        /// <summary>
        /// Give the unit an order to support the target's hold order.
        /// </summary>
        public IOrderDefinedContext<SupportHoldOrder> Hold();

        /// <summary>
        /// Give the unit an order to support the target's move order.
        /// </summary>
        public IOrderDefinedContext<SupportMoveOrder> MoveTo(string provinceName, string? coast = null);
    }

    /// <summary>
    /// Context for additional operations on a defined order or defining another order. This
    /// context mimics the <see cref="IPowerContext"/> with additional functionality related to
    /// the order that was just defined.
    /// </summary>
    public interface IOrderDefinedContext<OrderType> where OrderType : Order
    {
        /// <summary>
        /// Choose a new season to define orders for.
        /// </summary>
        public ISeasonContext this[(int turn, int timeline) seasonCoord] { get; }

        /// <summary>
        /// Get the context for defining the orders for another power.
        /// </summary>
        public IPowerContext this[string powerName] { get; }

        /// <summary>
        /// Define an order for a new army in a province.
        /// </summary>
        public IUnitContext Army(string provinceName, string? powerName = null);

        /// <summary>
        /// Define an order for a new fleet in a province, optionally on a specific coast.
        /// </summary>
        public IUnitContext Fleet(string provinceName, string? coast = null, string? powerName = null);

        /// <summary>
        /// Save a reference to the order just defined.
        /// </summary>
        public IOrderDefinedContext<OrderType> GetReference(out OrderReference<OrderType> order);
    }

    public World World { get; private set; }
    public ReadOnlyCollection<Order> Orders { get; }
    private List<Order> OrderList;
    public List<OrderValidation>? ValidationResults { get; private set; }
    public List<AdjudicationDecision>? AdjudicationResults { get; private set; }

    /// <summary>
    /// Create a test case builder that will operate on a world.
    /// </summary>
    public TestCaseBuilder(World world)
    {
        this.World = world;
        this.OrderList = new();
        this.Orders = new(this.OrderList);
        this.ValidationResults = null;
        this.AdjudicationResults = null;
    }

    /// <summary>
    /// Get the context for defining the orders for a power. Defaults to the root season.
    /// </summary>
    public IPowerContext this[string powerName] => this[(0, 0)][powerName];

    /// <summary>
    /// Get the context for defining the orders for a season.
    /// </summary>
    public ISeasonContext this[(int turn, int timeline) seasonCoord]
        => new SeasonContext(this, this.World.GetSeason(seasonCoord.turn, seasonCoord.timeline));

    /// <summary>
    /// Get a unit matching a description. If no such unit exists, one is created and added to the
    /// <see cref="World"/>.
    /// </summary>
    /// <param name="type">
    /// The unit type to create if the unit does not exist.
    /// Per DATC 4.C.1-2, mismatching unit designations should not invalidate an order, which
    /// effectively makes the designations superfluous. To support this, the test case builder
    /// returns a unit that matches the power, location, and season even if the unit found is not
    /// of this type.
    /// </param>
    private Unit GetOrBuildUnit(
        Power power,
        Location location,
        Season season,
        UnitType type)
    {
        foreach (Unit unit in this.World.Units)
        {
            if (unit.Power == power
                && unit.Location.Province == location.Province
                && unit.Season == season)
            {
                return unit;
            }
        }

        // Not found
        Unit newUnit = Unit.Build(location, season, power, type);
        this.World = this.World.Update(units: this.World.Units.Append(newUnit));
        return newUnit;
    }

    public List<OrderValidation> ValidateOrders(IPhaseAdjudicator adjudicator)
    {
        this.ValidationResults = adjudicator.ValidateOrders(this.World, this.Orders.ToList());
        this.OrderList.Clear();
        return this.ValidationResults;
    }

    public List<AdjudicationDecision> AdjudicateOrders(IPhaseAdjudicator adjudicator)
    {
        if (this.ValidationResults == null)
        {
            throw new InvalidOperationException("Cannot adjudicate before validation");
        }

        List<Order> orders = this.ValidationResults
            .Where(validation => validation.Valid)
            .Select(validation => validation.Order)
            .ToList();
        this.AdjudicationResults = adjudicator.AdjudicateOrders(this.World, orders);
        this.ValidationResults = null;
        return this.AdjudicationResults;
    }

    public World UpdateWorld(IPhaseAdjudicator adjudicator)
    {
        if (this.AdjudicationResults == null)
        {
            throw new InvalidOperationException("Cannot update before adjudication");
        }

        this.World = adjudicator.UpdateWorld(this.World, this.AdjudicationResults);
        this.AdjudicationResults = null;
        return this.World;
    }

    private class SeasonContext : ISeasonContext
    {
        public TestCaseBuilder Builder;
        public Season Season;

        public SeasonContext(TestCaseBuilder Builder, Season season)
        {
            this.Builder = Builder;
            this.Season = season;
        }

        public ISeasonContext this[(int turn, int timeline) seasonCoord]
            => this.Builder[(seasonCoord.turn, seasonCoord.timeline)];

        public IPowerContext this[string powerName]
            => new PowerContext(this, this.Builder.World.GetPower(powerName));

        public ISeasonContext GetReference(out Season season)
        {
            season = this.Season;
            return this;
        }
    }

    private class PowerContext : IPowerContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public Power Power;

        public PowerContext(SeasonContext seasonContext, Power Power)
        {
            this.Builder = seasonContext.Builder;
            this.SeasonContext = seasonContext;
            this.Power = Power;
        }

        public ISeasonContext this[(int turn, int timeline) seasonCoord]
            => this.SeasonContext[seasonCoord];

        public IPowerContext this[string powerName]
            => this.SeasonContext[powerName];

        public IUnitContext Army(string provinceName, string? powerName = null)
        {
            Power power = powerName == null
                ? this.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetLand(provinceName);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Army);
            return new UnitContext(this, unit);
        }

        public IUnitContext Fleet(string provinceName, string? coast = null, string? powerName = null)
        {
            Power power = powerName == null
                ? this.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetWater(provinceName, coast);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Fleet);
            return new UnitContext(this, unit);
        }
    }

    private class UnitContext : IUnitContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public Unit Unit;

        public UnitContext(PowerContext powerContext, Unit unit)
        {
            this.Builder = powerContext.Builder;
            this.SeasonContext = powerContext.SeasonContext;
            this.PowerContext = powerContext;
            this.Unit = unit;
        }

        public IPowerContext Exists()
            => this.PowerContext;

        public IOrderDefinedContext<HoldOrder> Holds()
        {
            HoldOrder order = new HoldOrder(this.PowerContext.Power, this.Unit);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<HoldOrder>(this, order);
        }

        public IOrderDefinedContext<MoveOrder> MovesTo(
            string provinceName,
            Season? season = null,
            string? coast = null)
        {
            Location destination = this.Unit.Type == UnitType.Army
                ? this.Builder.World.GetLand(provinceName)
                : this.Builder.World.GetWater(provinceName, coast);
            Season destSeason = season ?? this.SeasonContext.Season;
            MoveOrder moveOrder = new MoveOrder(
                this.PowerContext.Power,
                this.Unit,
                destSeason,
                destination);
            this.Builder.OrderList.Add(moveOrder);
            return new OrderDefinedContext<MoveOrder>(this, moveOrder);
        }

        public IConvoyContext Convoys
            => new ConvoyContext(this);

        public ISupportContext Supports
            => new SupportContext(this);
    }

    private class ConvoyContext : IConvoyContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public UnitContext UnitContext;

        public ConvoyContext(UnitContext unitContext)
        {
            this.Builder = unitContext.Builder;
            this.SeasonContext = unitContext.SeasonContext;
            this.PowerContext = unitContext.PowerContext;
            this.UnitContext = unitContext;
        }

        public IConvoyDestinationContext Army(string provinceName, string? powerName = null)
        {
            Power power = powerName == null
                ? this.PowerContext.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetLand(provinceName);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Army);
            return new ConvoyDestinationContext(this, unit);
        }

        public IConvoyDestinationContext Fleet(
            string provinceName,
            string? coast = null,
            string? powerName = null)
        {
            Power power = powerName == null
                ? this.PowerContext.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetWater(provinceName, coast);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Fleet);
            return new ConvoyDestinationContext(this, unit);
        }
    }

    private class ConvoyDestinationContext : IConvoyDestinationContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public Unit Target;

        public ConvoyDestinationContext(ConvoyContext convoyContext, Unit target)
        {
            this.Builder = convoyContext.Builder;
            this.SeasonContext = convoyContext.SeasonContext;
            this.PowerContext = convoyContext.PowerContext;
            this.UnitContext = convoyContext.UnitContext;
            this.Target = target;
        }

        public IOrderDefinedContext<ConvoyOrder> To(string provinceName)
        {
            Location location = this.Builder.World.GetLand(provinceName);
            ConvoyOrder order = new ConvoyOrder(
                this.PowerContext.Power,
                this.UnitContext.Unit,
                this.Target,
                this.SeasonContext.Season,
                location);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<ConvoyOrder>(this.UnitContext, order);
        }
    }

    private class SupportContext : ISupportContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public UnitContext UnitContext;

        public SupportContext(UnitContext unitContext)
        {
            this.Builder = unitContext.Builder;
            this.SeasonContext = unitContext.SeasonContext;
            this.PowerContext = unitContext.PowerContext;
            this.UnitContext = unitContext;
        }

        public ISupportTypeContext Army(string provinceName, string? powerName = null)
        {
            Power power = powerName == null
                ? this.PowerContext.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetLand(provinceName);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Army);
            return new SupportTypeContext(this, unit);
        }

        public ISupportTypeContext Fleet(
            string provinceName,
            string? coast = null,
            string? powerName = null)
        {
            Power power = powerName == null
                ? this.PowerContext.Power
                : this.Builder.World.GetPower(powerName);
            Location location = this.Builder.World.GetWater(provinceName, coast);
            Unit unit = this.Builder.GetOrBuildUnit(
                power, location, this.SeasonContext.Season, UnitType.Fleet);
            return new SupportTypeContext(this, unit);
        }
    }

    private class SupportTypeContext : ISupportTypeContext
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public Unit Target;

        public SupportTypeContext(SupportContext supportContext, Unit target)
        {
            this.Builder = supportContext.Builder;
            this.SeasonContext = supportContext.SeasonContext;
            this.PowerContext = supportContext.PowerContext;
            this.UnitContext = supportContext.UnitContext;
            this.Target = target;
        }

        public IOrderDefinedContext<SupportHoldOrder> Hold()
        {
            SupportHoldOrder order = new SupportHoldOrder(
                this.PowerContext.Power,
                this.UnitContext.Unit,
                this.Target);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<SupportHoldOrder>(this.UnitContext, order);
        }

        public IOrderDefinedContext<SupportMoveOrder> MoveTo(string provinceName, string? coast = null)
        {
            Location destination = this.Target.Type == UnitType.Army
                ? this.Builder.World.GetLand(provinceName)
                : this.Builder.World.GetWater(provinceName, coast);
            SupportMoveOrder order = new SupportMoveOrder(
                this.PowerContext.Power,
                this.UnitContext.Unit,
                this.Target,
                this.SeasonContext.Season,
                destination);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<SupportMoveOrder>(this.UnitContext, order);
        }
    }

    private class OrderDefinedContext<OrderType> : IOrderDefinedContext<OrderType> where OrderType : Order
    {
        public TestCaseBuilder Builder;
        public SeasonContext SeasonContext;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public OrderType Order;

        public OrderDefinedContext(UnitContext unitContext, OrderType order)
        {
            this.Builder = unitContext.Builder;
            this.SeasonContext = unitContext.SeasonContext;
            this.PowerContext = unitContext.PowerContext;
            this.UnitContext = unitContext;
            this.Order = order;
        }

        public ISeasonContext this[(int turn, int timeline) seasonCoord]
            => this.SeasonContext[seasonCoord];

        public IPowerContext this[string powerName]
            => this.SeasonContext[powerName];

        public IUnitContext Army(string provinceName, string? powerName = null)
            => this.PowerContext.Army(provinceName, powerName);

        public IUnitContext Fleet(string provinceName, string? coast = null, string? powerName = null)
            => this.PowerContext.Fleet(provinceName, coast, powerName);

        public IOrderDefinedContext<OrderType> GetReference(out OrderReference<OrderType> order)
        {
            order = new OrderReference<OrderType>(this.Builder, this.Order);
            return this;
        }
    }
}
