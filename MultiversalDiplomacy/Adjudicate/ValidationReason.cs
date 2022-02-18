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
    /// Another order was submitted that replaced this order.
    /// </summary>
    SupersededByLaterOrder = 3,
}