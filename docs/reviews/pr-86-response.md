# PR 86 response

The author answer to the review of `cd1d45298ec5f2952a4864e74e10cd9c76658704` in `docs/reviews/pr-86.md`. The review gave `Changes required` for one finding.

## P2-1: a repeated line can retain the previous speaker

Disposition: full merit.

Evidence: the trigger reproduced. At `cd1d452`, `DialogueBox.Show` called `ShowSpeaker` only when the line id changed. Two say steps with one line id and two speakers are valid content (D-997), and the second step kept the portrait and the name plate of the first speaker. D-223 puts the name plate of the speaker over the portrait.

Correction:

- `ScenePlay.LineStep` gives the index of the say step whose line shows, and `ScenePlay.Scene` gives the story scene on screen.
- `DialogueChange` decides each redraw with no engine type. The line draws again on each new say step, and the portrait and the name plate draw again on each change of the speaker, the same line included.
- `DialogueBox.Show` draws the parts that `DialogueChange` gives.

Regression check:

- `ScenePlayTests.ASecondSpeakerOfTheSameLineDrawsItsPortraitAndItsNameAgain` gives the line of the stranger to the barmaid on the next step. The second step must report a new speaker and a new line.
- With the rule of `cd1d452` in `DialogueChange` (a redraw on a new line id alone), the test failed: "The second speaker of the same line drew no portrait and no name." With the correction, it passes.
- The full suite passes: 3,938 tests. `dotnet format` and det-lint report no finding.

## Ids

No new D-# id and no new F-# id.

## Final head

The head of this answer is the commit that holds this file. The change draws no capture differently: each fixture line of a capture has its own speaker, so each baseline stands.
