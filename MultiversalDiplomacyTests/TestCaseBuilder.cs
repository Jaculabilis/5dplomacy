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
    /// Context for defining orders given by a power.
    /// </summary>
    public interface IPowerContext
    {
        /// <summary>
        /// Get the context for defining the orders for another power.
        /// </summary>
        public IPowerContext this[string powerName] { get; }

        /// <summary>
        /// Define an order for an army in a province.
        /// </summary>
        public IUnitContext Army(string provinceName);

        /// <summary>
        /// Define an order for a fleet in a province, optionally on a specific coast.
        /// </summary>
        public IUnitContext Fleet(string provinceName, string? coast = null);
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
        public IOrderDefinedContext<MoveOrder> MovesTo(string provinceName, string? coast = null);

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
        /// Get the context for defining the orders for another power.
        /// </summary>
        public IPowerContext this[string powerName] { get; }

        /// <summary>
        /// Define an order for a new army in a province.
        /// </summary>
        public IUnitContext Army(string provinceName);

        /// <summary>
        /// Define an order for a new fleet in a province, optionally on a specific coast.
        /// </summary>
        public IUnitContext Fleet(string provinceName, string? coast = null);

        /// <summary>
        /// Save a reference to the order just defined.
        /// </summary>
        public IOrderDefinedContext<OrderType> GetReference(out OrderReference<OrderType> order);
    }

    public World World { get; private set; }
    public ReadOnlyCollection<Order> Orders { get; }
    private List<Order> OrderList;
    private Season Season;
    public List<OrderValidation>? ValidationResults { get; private set; }

    /// <summary>
    /// Create a test case builder that will operate on a world.
    /// </summary>
    public TestCaseBuilder(World world, Season? season = null)
    {
        this.World = world;
        this.OrderList = new();
        this.Orders = new(this.OrderList);
        this.Season = season ?? this.World.Seasons.First();
        this.ValidationResults = null;
    }

    /// <summary>
    /// Get the context for defining the orders for a power.
    /// </summary>
    public IPowerContext this[string powerName]
    {
        get
        {
            Power power = this.World.GetPower(powerName);
            return new PowerContext(this, power);
        }
    }

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
                && unit.Location == location
                && unit.Season == season)
            {
                return unit;
            }
        }

        // Not found
        Unit newUnit = Unit.Build(location, season, power, type);
        this.World = this.World.WithUnits(this.World.Units.Append(newUnit));
        return newUnit;
    }

    public List<OrderValidation> ValidateOrders(IPhaseAdjudicator adjudicator)
    {
        this.ValidationResults = adjudicator.ValidateOrders(this.World, this.Orders.ToList());
        return this.ValidationResults;
    }

    private class PowerContext : IPowerContext
    {
        public TestCaseBuilder Builder;
        public Power Power;

        public PowerContext(TestCaseBuilder Builder, Power Power)
        {
            this.Builder = Builder;
            this.Power = Power;
        }

        public IPowerContext this[string powerName]
            => this.Builder[powerName];

        public IUnitContext Army(string provinceName)
        {
            Location location = this.Builder.World.GetLand(provinceName);
            Unit unit = this.Builder.GetOrBuildUnit(
                this.Power, location, this.Builder.Season, UnitType.Army);
            return new UnitContext(this, unit);
        }

        public IUnitContext Fleet(string provinceName, string? coast = null)
        {
            Location location = this.Builder.World.GetWater(provinceName, coast);
            Unit unit = this.Builder.GetOrBuildUnit(
                this.Power, location, this.Builder.Season, UnitType.Fleet);
            return new UnitContext(this, unit);
        }
    }

    private class UnitContext : IUnitContext
    {
        public TestCaseBuilder Builder;
        public PowerContext PowerContext;
        public Unit Unit;

        public UnitContext(PowerContext powerContext, Unit unit)
        {
            this.Builder = powerContext.Builder;
            this.PowerContext = powerContext;
            this.Unit = unit;
        }

        /// <summary>
        /// Declare that a unit exists without giving it an order.
        /// </summary>
        public IPowerContext Exists()
            => this.PowerContext;

        /// <summary>
        /// Order a unit to hold.
        /// </summary>
        public IOrderDefinedContext<HoldOrder> Holds()
        {
            HoldOrder order = new HoldOrder(this.PowerContext.Power, this.Unit);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<HoldOrder>(this, order);
        }

        /// <summary>
        /// Order a unit to move to a destination.
        /// </summary>
        public IOrderDefinedContext<MoveOrder> MovesTo(string provinceName, string? coast = null)
        {
            Location destination = this.Unit.Type == UnitType.Army
                ? this.Builder.World.GetLand(provinceName)
                : this.Builder.World.GetWater(provinceName, coast);
            MoveOrder moveOrder = new MoveOrder(
                this.PowerContext.Power,
                this.Unit,
                this.Builder.Season,
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
        public PowerContext PowerContext;
        public UnitContext UnitContext;

        public ConvoyContext(UnitContext unitContext)
        {
            this.Builder = unitContext.Builder;
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
                power, location, this.Builder.Season, UnitType.Army);
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
                power, location, this.Builder.Season, UnitType.Fleet);
            return new ConvoyDestinationContext(this, unit);
        }
    }

    private class ConvoyDestinationContext : IConvoyDestinationContext
    {
        public TestCaseBuilder Builder;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public Unit Target;

        public ConvoyDestinationContext(ConvoyContext convoyContext, Unit target)
        {
            this.Builder = convoyContext.Builder;
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
                this.Builder.Season,
                location);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<ConvoyOrder>(this.UnitContext, order);
        }
    }

    private class SupportContext : ISupportContext
    {
        public TestCaseBuilder Builder;
        public PowerContext PowerContext;
        public UnitContext UnitContext;

        public SupportContext(UnitContext unitContext)
        {
            this.Builder = unitContext.Builder;
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
                power, location, this.Builder.Season, UnitType.Army);
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
                power, location, this.Builder.Season, UnitType.Fleet);
            return new SupportTypeContext(this, unit);
        }
    }

    private class SupportTypeContext : ISupportTypeContext
    {
        public TestCaseBuilder Builder;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public Unit Target;

        public SupportTypeContext(SupportContext supportContext, Unit target)
        {
            this.Builder = supportContext.Builder;
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
                this.Builder.Season,
                destination);
            this.Builder.OrderList.Add(order);
            return new OrderDefinedContext<SupportMoveOrder>(this.UnitContext, order);
        }
    }

    private class OrderDefinedContext<OrderType> : IOrderDefinedContext<OrderType> where OrderType : Order
    {
        public TestCaseBuilder Builder;
        public PowerContext PowerContext;
        public UnitContext UnitContext;
        public OrderType Order;

        public OrderDefinedContext(UnitContext unitContext, OrderType order)
        {
            this.Builder = unitContext.Builder;
            this.PowerContext = unitContext.PowerContext;
            this.UnitContext = unitContext;
            this.Order = order;
        }

        public IPowerContext this[string powerName] => this.PowerContext[powerName];

        public IUnitContext Army(string provinceName) => this.PowerContext.Army(provinceName);

        public IUnitContext Fleet(string provinceName, string? coast = null)
            => this.PowerContext.Fleet(provinceName);

        public IOrderDefinedContext<OrderType> GetReference(out OrderReference<OrderType> order)
        {
            order = new OrderReference<OrderType>(this.Builder, this.Order);
            return this;
        }
    }
}
