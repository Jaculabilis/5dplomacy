namespace MultiversalDiplomacy.Adjudicate.Decision;

public abstract class NumericAdjudicationDecision : AdjudicationDecision
{
    public int MinValue { get; private set; } = 0;
    public int MaxValue { get; private set; } = 99;

    public override bool Resolved => this.MinValue == this.MaxValue;

    public bool Update(int min, int max)
    {
        if (min < this.MinValue || max > this.MaxValue)
        {
            string name = this.GetType().Name;
            throw new ArgumentException(
                $"Cannot reverse adjudication of {name} from ({this.MinValue},{this.MaxValue})"
                + $" to ({min},{max})");
        }
        bool updated = this.MinValue != min || this.MaxValue != max;
        this.MinValue = min;
        this.MaxValue = max;
        return updated;
    }
}
