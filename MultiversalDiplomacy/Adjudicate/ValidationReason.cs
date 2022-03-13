namespace MultiversalDiplomacy.Adjudicate;

public enum ValidationReason
{
    /// <summary>
    /// The order is valid.
    /// </summary>
    Valid = 0,

    /// <summary>
    /// The order type is not valid for the current phase of the game.
    /// </summary>
    InvalidOrderTypeForPhase = 1,

    /// <summary>
    /// A hold order was created to replace an illegal order.
    /// </summary>
    IllegalOrderReplacedWithHold = 2,

    /// <summary>
    /// The power that issued this order does not control the ordered unit.
    /// </summary>
    InvalidUnitForPower = 3,

    /// <summary>
    /// The ordered unit received conflicting orders and all orders were invalidated.
    /// </summary>
    DuplicateOrders = 4,

    /// <summary>
    /// An army was ordered into the sea or a fleet was ordered onto the land.
    /// </summary>
    IllegalDestinationType = 5,

    /// <summary>
    /// A unit was ordered to move to where it already is.
    /// </summary>
    DestinationMatchesOrigin = 6,

    /// <summary>
    /// The destination of the move is not reachable from the origin.
    /// </summary>
    UnreachableDestination = 7,

    /// <summary>
    /// The order type is not valid for the unit.
    /// </summary>
    InvalidOrderTypeForUnit = 8,

    /// <summary>
    /// A unit was ordered to support itself.
    /// </summary>
    NoSelfSupport = 9,

    /// <summary>
    /// A unit was ordered to support a location it could not reach.
    /// </summary>
    UnreachableSupport = 10,

    /// <summary>
    /// A unit was ordered to support a move to its own province.
    /// </summary>
    NoSupportMoveAgainstSelf = 11,

    /// <summary>
    /// A unit was ordered that is not currently eligible to receive orders.
    /// </summary>
    IneligibleForOrder = 12,
}