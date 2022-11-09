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
    /// The season's spatial location as a turn-timeline tuple.
    /// </summary>
    public (int Turn, int Timeline) Coord => (this.Turn, this.Timeline);

    /// <summary>
    /// The shared timeline number generator.
    /// </summary>
    private TimelineFactory Timelines { get; }

    /// <summary>
    /// Future seasons created directly from this season.
    /// </summary>
    public IEnumerable<Season> Futures => this.FutureList;
    private List<Season> FutureList { get; }

    private Season(Season? past, int turn, int timeline, TimelineFactory factory)
    {
        this.Past = past;
        this.Turn = turn;
        this.Timeline = timeline;
        this.Timelines = factory;
        this.FutureList = new();

        if (past != null)
        {
            past.FutureList.Add(this);
        }
    }

    public override string ToString()
    {
        return $"{this.Timeline}@{this.Turn}";
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
    /// Seasons are considered to be in adjacent timelines if they are in the same timeline,
    /// one is in a timeline that branched from the other's timeline, or both are in timelines
    /// that branched from the same point.
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

    /// <summary>
    /// Returns all seasons that are adjacent to this season.
    /// </summary>
    public IEnumerable<Season> GetAdjacentSeasons()
    {
        List<Season> adjacents = new();

        // The immediate past and all immediate futures are adjacent.
        if (this.Past != null) adjacents.Add(this.Past);
        adjacents.AddRange(this.FutureList);

        // Find all adjacent timelines by finding all timelines that branched off of this season's
        // timeline, i.e. all futures of this season's past that have different timelines. Also
        // include any timelines that branched off of the timeline this timeline branched off from.
        List<Season> adjacentTimelineRoots = new();
        Season? current;
        for (current = this;
            current?.Past?.Timeline != null && current.Past.Timeline == current.Timeline;
            current = current.Past)
        {
            adjacentTimelineRoots.AddRange(
                current.FutureList.Where(s => s.Timeline != current.Timeline));
        }

        // At the end of the for loop, if this season is part of the first timeline, then current
        // is the root season (current.past == null); if this season is in a branched timeline,
        // then current is the branch timeline's root season (current.past.timeline !=
        // current.timeline). There are co-branches if this season is in a branched timeline, since
        // the first timeline by definition cannot have co-branches.
        if (current?.Past != null)
        {
            IEnumerable<Season> cobranchRoots = current.Past.FutureList
                .Where(s => s.Timeline != current.Timeline && s.Timeline != current.Past.Timeline);
            adjacentTimelineRoots.AddRange(cobranchRoots);
        }

        // Walk up all alternate timelines to find seasons within one turn of this season.
        foreach (Season timelineRoot in adjacentTimelineRoots)
        {
            for (Season? branchSeason = timelineRoot;
                branchSeason != null && branchSeason.Turn <= this.Turn + 1;
                branchSeason = branchSeason.FutureList
                    .FirstOrDefault(s => s!.Timeline == branchSeason.Timeline, null))
            {
                if (branchSeason.Turn >= this.Turn - 1) adjacents.Add(branchSeason);
            }
        }

        return adjacents;
    }
}
