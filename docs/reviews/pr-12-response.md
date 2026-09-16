# PR-12 review response

Date: 2026-09-16

Author: Claude Code. This file answers `docs/reviews/pr-12.md`, verdict `Changes required` for head `84b4128`.

## Summary

Six findings. Five have full merit, and one has partial merit. Every correction landed. The owner answered two questions: the fit at 1920 by 1080 (D-573), and the place of PR-27, which the owner asked the session to examine (D-574). A wider scan found six more stale contracts of the kinds that P2-1 and P2-2 name, and those corrections landed too.

## P1-1: The 1080p pixel rule cannot be satisfied

Disposition: **full merit.**

The trigger reproduces. At a scale of 1.5, an even pixel width needs pixels of 1.5 screen pixels, so a method either mixes samples or alternates widths of 1 and 2. No option of OQ-105 met both halves of D-568.

The cause was the wording of D-568, which the session wrote. The owner said "It won't be perfect pixel scaling, but it MUST look good." The session turned that into "no blur and no uneven pixels", an absolute that the owner never stated.

Correction:

- The owner chose the tradeoff: even pixel sizes, with a slight softness at pixel edges (D-573). The method scales up by the smallest whole number that reaches the height of the screen, with the Nearest filter, then scales down with a linear filter.
- D-573 revises D-568 in part and resolves OQ-105. D-232 keeps the whole-number setting with bars for a player who wants exact pixels.
- `area-ui-input.md` section 7.2, the PR-61 scope, review focus, and questions, the PR-41 review focus, and the Phase 2 table of section 9 now cite D-573. OQ-105 carries its resolution.

Regression check: for each display, the rule is exact.

| Display | First step | Second step | Result |
|---|---|---|---|
| 1280 by 800, the Deck | 1x | none | Exact pixels, with bars |
| 1920 by 1080 | 2x, to 2560 by 1440 | linear, to 1080 rows | Even pixel sizes, slight softness |
| 2560 by 1440 | 2x | none | Exact pixels |

No rule now asks for equal pixel widths and no mixed samples at a scale that is not a whole number. A search for "no blur" and "uneven pixels" in the live documents finds only D-568 and D-573, which quote the old rule as the rule that changed.

## P2-1: Superseded frame contracts remain active

Disposition: **full merit.**

The trigger reproduces. The cause of the miss is recorded here, because the PR description claimed the opposite. The scan of the session left out `docs/decisions.md`, and it skipped each line that already named D-568. OQ-94 held both D-480 and D-568 on one line, so the scan passed it.

Correction:

- D-240, D-516, and D-524 gained marks that name D-568. The note of D-524 said "The rest stands", which kept the superseded frame alive. It now names the parts that D-568 changed and the parts that stand.
- OQ-94 now says that every screen shows the same frame (D-568).
- A corrected scan read every live document with the decision register, and it showed each hit with its context. It found five more stale contracts, which the review did not list:
  - D-160: the Deck test ran at 1280 by 800. It now names D-568.
  - D-180: its note said "The frame is 1280 by 800 since D-228". It now names D-568.
  - D-229: its note named D-480 as the superseding decision. It now names D-568 too.
  - D-551: a screenshot came "straight from the 16:9 view". It now names D-568.
  - F-24: its status bound the Deck test "at 1280 by 800". It now binds the frame of 1280 by 720.
- The PR description no longer states the false claim.

Regression check: the corrected scan searches `1280 by 800`, `1422`, `16:9 view`, `two views`, and `D-480`. Every remaining hit is a dated record, a superseded or revised row that carries its mark, the size of the Deck screen (1280 by 800, a true external fact), or the Valve rule of 9 pixels at 1280 by 800.

## P2-2: Three new decisions leave their old rules in current consumers

Disposition: **full merit.**

Correction:

- OQ-114 no longer says that fog covers a tile. It says that the game has no fog of war, so the rule of sight hides no tile from the party (D-566).
- OQ-135 now says that a save point restores MP once for the place, until a story event reopens it (D-555).
- The PR-30 exit test and Gate 4 now name the band that the owner set after M-4, before Gate 2 (D-571).
- D-42 gained a mark for D-555, because its note still said "once per visit".

Regression check: searches for `fog covers`, `once for each visit`, and `Gate 2 sign-off` find no current contract. The remaining hits are D-555, D-566, D-571, the rows that they revise with their marks, and OQ-116, which D-566 resolved.

## P2-3: The scene-term rewrite assigns test instrumentation to a story scene

Disposition: **full merit.**

The cause was the conversion of the session. Its special cases caught "The scene measures itself" in OQ-93 and missed the second sentence of the same option.

Correction: OQ-93 now says "The test scene needs its own measurement code".

Regression check: a read of every "story scene" in a line that names a test, a measurement, Godot, a shader, a particle, an export, a renderer, a capture, or a screenshot. Five hits remain, and each is a real story scene: the credits roll, the runner in Core, and a sprite that a story scene moves.

## P2-4: The PR description attributes the critic work to an agent

Disposition: **partial merit.**

The part with merit: the description said that the `design-critic` agent read the plan and produced the reports. That wording names an agent as the source of work, against T-6 and D-22. The new dated line of this PR in `docs/design.md` said the same thing.

Correction: the description and the dated line now name the design-critic pass of D-484, which read the plan in five slices.

The part with no merit: the name of the pass itself is not attribution. D-484 and D-490 name "the design-critic pass" as a plan item, and the design doc has named it since 2026-09-13. The `pr-review` skill states that a reading which condemns the decision register is too broad. So the dated records from before this PR keep their words, and F-57 keeps the name of the pass.

Regression check: the full PR description names no provider, agent, harness, or model as the source of the work.

## P2-5: The archived handoff sessions are not newest first

Disposition: **full merit.**

The cause was the archive step of the session, twice. In Session 35 and in Session 37, the step inserted the moved session above a named older session, not at the top of the archive. So Session 25 landed below Session 24, and Session 27 below Sessions 26 and 24.

Correction: the archive blocks now run in strict descending order, with no text changed. A check of the total length of the text passed before and after the move. The archive step of this session inserts at the top.

Regression check: the archive headings run from Session 28 down to Session 1, in strict descending order, with no gap and no repeat.

## The place of PR-27

The owner asked the session to examine the order of PR-27.

- The second hub is the refuge of the old faith. `docs/world/arc.md` says "the refuge of the old faith opens as the second hub late in the region", and D-243 names the mining town and the cave community as the two hubs.
- The party reaches the refuge in the flight, right after the sealed gallery (D-331). D-356 names the refuge as the place to swap before the border fort and the ice crossing.
- The sequence put PR-27 after the ice crossing. That broke the claim of the Phase 4 file that its order follows the order of play, and it built the fort and the ice before the hub that serves them.

The owner chose to move PR-27 right after PR-81 (D-574). The Phase 4 order is now PR-23, PR-24, PR-81, PR-27, PR-25, and PR-26 in the phase file, in sections 7 and 8 of `docs/design.md`, in `area-exploration.md`, and in `docs/world/places.md`. D-304 still holds, because PR-42 lands after PR-27.

## New ids

- D-573: the fit at 1920 by 1080.
- D-574: the place of PR-27.
- No new question or finding. OQ-105 closed.

## Verification

- `python3 docs/tools/ste-check.py $(git ls-files '*.md' | grep -v -e '^docs/reviews/' -e '^docs/session-handoff' -e '^docs/archive/')`: 0 findings.
- `git diff --check`: clean.
- `cmp -s AGENTS.md CLAUDE.md`: identical.
- The order check: the phase lists of section 7, the sequence of section 8, and the five phase sequences give the same 79 PR items.
- The archive check: Session 28 down to Session 1, strictly descending.
- The corrected scan of P2-1 and the searches of P2-2 and P2-3, as each section above states.
- Build, test, and format commands: not run. The repository holds no code until PR-1 (G-16).

## The new head

The corrections change `docs/decisions.md`, `docs/design.md`, `docs/questions.md`, three roadmap files, and `docs/world/places.md`, which sit outside the metadata set. So the effective head moves off `84b4128` to the commit that holds this response.
