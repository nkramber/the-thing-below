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

Regression check: the archive headings run from Session 29 down to Session 1, in strict descending order, with no gap and no repeat. Session 29 is at the top, because the handoff step of this answer archived it there.

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
- The archive check: Session 29 down to Session 1, strictly descending.
- The corrected scan of P2-1 and the searches of P2-2 and P2-3, as each section above states.
- Build, test, and format commands: not run. The repository holds no code until PR-1 (G-16).

## The new head

The corrections change `docs/decisions.md`, `docs/design.md`, `docs/questions.md`, three roadmap files, and `docs/world/places.md`, which sit outside the metadata set. So the effective head moves off `84b4128` to the commit that holds this response.

## Repeat review: P2-6

The repeat review of Session 40 set P1-1 and P2-1 to P2-5 to `fixed in d31ae6b`, and it added P2-6 for head `d31ae6b`.

### P2-6: The sealed gallery conflicts with the four-dungeon contract

Disposition: **full merit.**

The trigger reproduces. D-56 reads "Two hubs, four dungeons", D-244 names the four, and D-313 orders "the four dungeons". D-562 called the gallery "a dungeon map", and the session tagged it `Dungeon` in the table of `docs/world/places.md`. So the documents counted a fifth place of that kind, and no row recorded a revision. The words "dungeon map" named either the map format or the kind of place that D-56 counts.

Correction:

- The owner classified the gallery as the fifth dungeon, with the full dungeon contract (D-575). The session recommended a passage that only uses the dungeon-map format, and the owner chose a dungeon.
- D-56, D-244, D-313, D-562, and D-564 gained marks that name D-575. D-56 keeps its two hubs, its one arc, and its target of six to eight hours.
- A scan for the old count found three more rows that the review did not list: D-327, D-346, and D-369 each stated "four dungeons" in the Effect column. Each now names D-575.
- The PR-81 entry of `phase-4-region-one.md` gains the dungeon contract: the enemies, one boss with its phases, and the treasure, the puzzles, and the secrets. It gains a boss test and a string test.
- `docs/world/places.md`, `docs/world/arc.md`, the thesis and section 7 of `docs/design.md`, `area-battle.md`, `area-exploration.md`, and the PR-20 and PR-21 entries of Phase 3 now count five dungeons. The arc table numbers the fort as the fourth dungeon and the ice crossing as the fifth.
- A fifth dungeon needs a boss, and no document named one. OQ-180 holds that question and blocks PR-81 (D-487).

Regression check: a scan of every live document for `four dungeons`, `third dungeon`, `fourth dungeon`, `fifth dungeon`, `five dungeons`, and `dungeon map with its own`. Each current contract says five dungeons, and the gallery is the fifth. Each decision row that still states four dungeons carries a mark that names D-575.

### New ids of this round

- D-575: the sealed gallery is a dungeon.
- OQ-180: the boss of the sealed gallery.

### The new head

The correction changes `docs/decisions.md`, `docs/design.md`, `docs/questions.md`, four roadmap files, and two world files, which sit outside the metadata set. So the effective head moves off `d31ae6b` to the commit that holds this round.

## Second repeat review: P2-7

The second repeat review of Session 42 set P2-6 to `fixed in 0fbfa82`, and it added P2-7 for head `0fbfa82`.

### P2-7: PR-81 does not carry the complete art and effect contract

Disposition: **full merit.**

The trigger reproduces. The art table of `area-art.md` and the effects table of `area-effects.md` gave the later places to PR-23 to PR-27 alone. The budget exit test of PR-81 read "each map of the place", while the test of PR-23 to PR-26 read "each map and each battle place". D-575 gave the gallery a boss, so it has a battle place.

Correction:

- The art row and the effects row now read PR-23 to PR-27 and PR-81, with the sealed gallery named, and each cites D-575.
- The budget exit test of PR-81 now reads "each map and each battle place", the same boundary as the other dungeon builds.
- A scan for the same class found two more lines that the review did not list. Phase 2 put "the other nodes of region one" out of the scope of PR-35, and "the other places" out of the scope of PR-17, each as PR-23 to PR-27. The sealed door is a story gate on the region map, so the gallery is a node and a place too. Both lines now name PR-81.

Regression check: a scan of the roadmaps and the design doc for each line that names PR-23 to PR-26 or PR-27 and not PR-81. Four lines remain, and each is right as it stands: the finding F-29 in `docs/design.md` and in `phase-4-region-one.md`, which records the four ids of that time; the heading of the PR-23 to PR-26 entry, which names its own four PRs; and the exploration table, where PR-81 has its own row. The three budget tests of Phase 4 read "each map and each battle place" or "every place of region one".

### The new head

The correction changes three roadmap files outside the metadata set, so the effective head moves off `0fbfa82` to the commit that holds this round.
