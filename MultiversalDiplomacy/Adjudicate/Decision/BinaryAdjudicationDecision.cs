namespace MultiversalDiplomacy.Adjudicate.Decision;

public abstract class BinaryAdjudicationDecision : AdjudicationDecision
{
    public bool? Outcome { get; private set; } = null;

    public override bool Resolved => this.Outcome != null;

    public bool Update(bool outcome)
    {
        if (this.Outcome == null)
        {
            this.Outcome = outcome;
            return true;
        }
        if (this.Outcome != outcome)
        {
            string name = this.GetType().Name;
            throw new ArgumentException(
                $"Cannot reverse adjudication of {name} from {this.Outcome} to {outcome}");
        }
        return false;
    }
}
