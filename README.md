# 5D Diplomacy With Multiversal Time Travel

_5D Diplomacy with Multiversal Time Travel_ is a _Diplomacy_ variant that adds multiversal time travel in the style of its namesake, _5D Chess with Multiversal Time Travel_.

## Acknowledgements

This project was inspired by [Oliver Lugg's proof-of-concept version](https://github.com/Oliveriver/5d-diplomacy-with-multiverse-time-travel). The implementation is based on the algorithms described by Lucas B. Kruijswijk in the chapter "The Process of Adjudication" found in the [Diplomacy Adjudicator Test Cases](http://web.inter.nl.net/users/L.B.Kruijswijk/#5) as well as ["The Math of Adjudication"](http://uk.diplom.org/pouch/Zine/S2009M/Kruijswijk/DipMath_Chp1.htm). Some of the data model is inspired by that of Martin Bruse's [godip](https://github.com/zond/godip).

## Variant rules

### Multiversal time travel and timeline forks

_Diplomacy_ is played on a single board, on which are placed armies and fleets. Sequential sets of orders modify the positions of these units, changing the board as time progresses. This may be described as something like an "inner" view of a single timeline. Consider instead the view from "above" the timeline, from which each successive state of the game board is comprehended in sequence. From "above", each turn from the beginning of the game to the present can be considered separately. In _5D Diplomacy with Multiversal Time Travel_, units moving to another province may also move to another turn, potentially changing the past.

If the outcome of a battle in the past of a timeline is changed by time travel, then the subsequent future will be different. Since the future of the original outcome is already determined, history forks, and the alternate future proceeds in an alternate timeline.

Just as units in _Diplomacy_ may only move to adjacent spaces, units in _5D Diplomacy with Multiversal Time Travel_ may only move to adjacent times. For the purposes of attacking, supporting, or convoying, turns within one season of each other adjacent. Branching timelines and the timelines they branched off of are adjacent, as well as timelines that branched off of the same turn in the same timeline. A unit cannot move to the province it is currently in, but it can move to the same province in another turn or another timeline.

When a unit changes the outcome of a battle in the past, only the timeline of the battle forks. If an army from one timeline dislodges an army in the past of a second timeline that was supporting a move in a third timeline, an alternate future is created where the army in the second timeline is dislodged. The third timeline does not fork, since the support was given in the original timeline. Similarly, if a unit moves into another timeline and causes a previously-successful move from a third timeline to become a bounce, the destination timeline forks because the outcome of the move changed, but the newly-bounced unit's origin timeline does not fork because the move succeeded in the original timeline.

### Sustaining timelines and time centers

Since there are many ways to create new timelines, the game would rapidly expand beyond all comprehension if this were not counterbalanced in some way. This happens during the _sustain phase_, which occurs after the fall movement and retreat phases and before the winter buid/disband phase.

(TODO)

### Victory conditions

The Great Powers of Europe can only wage multiversal wars because they are lead by extradimensional beings masquerading as human politicians. When a country is eliminated in one timeline, its extradimensional leader is executed, killing them in all timelines.
