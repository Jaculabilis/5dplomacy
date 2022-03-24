namespace MultiversalDiplomacy.Adjudicate.Decision;

/// <summary>
/// Base class for adjudication decisions. The decision-based adjudication algorithm is based
/// on DATC section 5 and "The Math of Adjudication" by Lucas Kruijswijk, respectively found at
/// http://web.inter.nl.net/users/L.B.Kruijswijk/#5 and
/// http://uk.diplom.org/pouch/Zine/S2009M/Kruijswijk/DipMath_Chp1.htm
/// </summary>
public abstract class AdjudicationDecision
{
    public abstract bool Resolved { get; }
}
