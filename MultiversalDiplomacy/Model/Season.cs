namespace MultiversalDiplomacy.Model;

/// <summary>
/// Represents a state of the map produced by a set of move orders on a previous season.
/// </summary>
public class Season
{
    /// <summary>
    /// A shared counter for handing out new timeline numbers.
    /// </summary>
    private class TimelineFactory
    {
        private int nextTimeline = 0;

        public int NextTimeline() => nextTimeline++;
    }

    /// <summary>
    /// The first turn number.
    /// </summary>
    public const int FIRST_TURN = 0;

    /// <summary>
    /// The season immediately preceding this season.
    /// If this season is an alternate timeline root, the past is from the origin timeline.
    /// The initial season does not have a past.
    /// </summary>
    public Season? Past { get; }

    /// <summary>
    /// The current turn, beginning at 0. Each season (spring and fall) is one turn.
    /// Phases that only occur after the fall phase occur when Turn % 2 == 1.
    /// The current year is (Turn / 2) + 1901.
    /// </summary>
    public int Turn { get; }

    /// <summary>
    /// The timeline to which this season belongs.
    /// </summary>
    public int Timeline { get; }

    /// <summary>
    /// The shared timeline number generator.
    /// </summary>
    private TimelineFactory Timelines { get; }

    private Season(Season? past, int turn, int timeline, TimelineFactory factory)
    {
        this.Past = past;
        this.Turn = turn;
        this.Timeline = timeline;
        this.Timelines = factory;
    }

    /// <summary>
    /// Create a root season at the beginning of time.
    /// </summary>
    public static Season MakeRoot()
    {
        TimelineFactory factory = new TimelineFactory();
        return new Season(
            past: null,
            turn: FIRST_TURN,
            timeline: factory.NextTimeline(),
            factory: factory);
    }

    /// <summary>
    /// Create a season immediately after this one in the same timeline.
    /// </summary>
    public Season MakeNext()
        => new Season(this, this.Turn + 1, this.Timeline, this.Timelines);

    /// <summary>
    /// Create a season immediately after this one in a new timeline.
    /// </summary>
    public Season MakeFork()
        => new Season(this, this.Turn + 1, this.Timelines.NextTimeline(), this.Timelines);

    /// <summary>
    /// Returns the first season in this season's timeline. The first season is the
    /// root of the first timeline. The earliest season in each alternate timeline is
    /// the root of that timeline.
    /// </summary>
    public Season TimelineRoot()
        => this.Past != null && this.Timeline == this.Past.Timeline
            ? this.Past.TimelineRoot()
            : this;

    /// <summary>
    /// Returns whether this season is in an adjacent timeline to another season.
    /// </summary>
    public bool InAdjacentTimeline(Season other)
    {
        // Timelines are adjacent to themselves. Early out in that case.
        if (this.Timeline == other.Timeline) return true;

        // If the timelines aren't identical, one of them isn't the initial trunk.
        // They can still be adjacent if one of them branched off of the other, or
        // if they both branched off of the same point.
        Season thisRoot = this.TimelineRoot();
        Season otherRoot = other.TimelineRoot();
        return // One branched off the other
                thisRoot.Past?.Timeline == other.Timeline
            || otherRoot.Past?.Timeline == this.Timeline
                // Both branched off of the same point
            || thisRoot.Past == otherRoot.Past;
    }
}
