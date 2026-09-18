# PR-24 review response

Date: 2026-09-18

Author response to `docs/reviews/pr-24.md`, which gives the verdict `Changes required` for head `7426dc9`.

## P2-1: CRT retirement leaves live contracts behind

Disposition: full merit.

The trigger reproduces. Each of the four cited places holds a live contract after D-618:

- `docs/decisions.md` D-172: the Effect column reads "Binds the gates of PR-10 and PR-37", and PR-37 is retired.
- `docs/decisions.md` D-214: the Effect column asks for a reduced form of "the flicker of D-105", which no effect draws now.
- `docs/roadmaps/area-effects.md` section 7.2: the build line named PR-37 as a builder of the frame.
- The same section: the plain-English paragraph described "the old-monitor look over all of it".

A sweep of `docs/decisions.md` for the same cause found four more rows. The first sweep of this PR read every live document except that register, which is why these rows stayed. The correction covers each one:

| Row or line | The correction |
|---|---|
| D-139 | The plan holds the color flash of D-96 alone of that pair. |
| D-161 | The target reads every effect that the plan holds. |
| D-172 | Revised in part: the decision binds the gate of PR-10 alone. The two baselines, the pinned Mesa, and the contact sheet stand. |
| D-214 | Revised in part: the flicker leaves the list. The shake, the flash, and the color split keep their reduced forms. |
| D-520 | Revised in part: PR-37 is retired and holds no place in the sequence. The other effect PRs keep their ids and their order. |
| D-526 | PR-63 still lands right before PR-57, and the reason no longer rests on PR-37. |
| `area-effects.md` section 7.2 | The build line names PR-7, PR-10, PR-56, PR-59, and PR-60. The paragraph names a transition in place of the old-monitor look. |

Regression check, run on the corrected tree:

- A search for `PR-37` in every live document returns the retired entry of `phase-2-first-playable.md`, the retired paragraph of `docs/design.md`, the dated findings F-23 and F-26, the resolved questions OQ-22, OQ-37, and OQ-103, and the scope line of PR-85. No live line schedules PR-37.
- A search for `CRT` in the design doc and the roadmaps returns dated findings, retired entries, lines that record the removal, and the two lines that state what the Deck test measured before the removal. No line plans a pass.
- The option list of OQ-37 keeps the words "The gates of PR-10 and PR-37 need a rewrite". That question closed on 2026-09-12 with D-172, and the list is the record of the options of that day.
- `ste-check`: 0 findings.
- `make verify`: passed. The build succeeded with 0 warnings, 187 tests passed, the format check passed, `det-lint` gave 0 findings, `ste-check` gave 0 findings, and the smoke session ended with no error.

## P2-2: PR description Documents lines do not meet D-581

Disposition: full merit, and the reviewer fixed it in the PR description.

The author read the current Documents section. Each of the 12 lines starts with its row name, and each takes one of the three forms of D-581. The handoff line names the entries and the archived entry, and it changes again with the entry of this round.

## The verification of the reviewer that stayed open

The review records `make verify` as inconclusive, with `Build FAILED` after 5:01 and no error line. That result did not reproduce on the machine of the author. The run above passed with an exit code of 0. A docs-only diff changes no build input, so the two runs read the same code.

## Ids and head

- New decision ids: none. The corrections apply D-618, which this PR already holds.
- New finding, question, guardrail, or measurement ids: none.
- The final head of this round goes in the handoff entry of the round.
