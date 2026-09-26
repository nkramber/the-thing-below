## Session 337: 2026-09-26, Codex

Author: Codex
Session: repeat review PR #86 (PR-36). Repository: the-thing-below. Local branch: `review/pr-86`; PR branch: `feat/pr-36-dialogue`. PR: #86. Role: reviewer. Base: `6f02d3d`.

### What this session did, and why

- Re-reviewed effective head `6b1b7d5`. The same line now redraws for each say step, and the portrait and name redraw when the speaker changes (D-223, D-997).
- The regression test reaches two speakers of the same line id. It passes with the correction. P2-1 is fixed in `docs/reviews/pr-86.md`.
- The CI-analysis claim of Gitar names RG 4 and RG 5. The workflow log confirms that the prior review record still held the old verdict and head (D-964).
- The record keeps the earlier `Changes required` verdict and gives `Ready for owner merge` for the current effective head (T-4, D-17).

### The state of the build

- `make verify` passes with 3,938 tests, format, det-lint, STE, replay identity, content hash, atlas, and smoke. CI run `36273874604` passes all checks except `review-gate`, which waits for this review record. The remote head before the metadata commit is `6b1b7d5`.

### What is in flight

- This session commits the review record and handoff as one metadata commit, then verifies the push.
- The PR waits for the owner merge (D-930).

### Traps and gotchas

- The correction changes no capture content. The CI artifact of run `36271984962` remains the visual evidence for the earlier screen changes (D-733).
- The clean Gitar approval has no item. The actionable CI analysis names RG 4 and RG 5 (D-964).

### The questions that block progress

None. OQ-250 blocks no PR yet.

### The next concrete action

The owner reads the review record and confirms the merge.

## Session 336: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #86 (PR-36), round 2. Repository: the-thing-below. Branch: `feat/pr-36-dialogue`. PR: #86. Role: author. Base: `6f02d3d`.

### What this session did, and why

- The review of `cd1d452` gave `Changes required` for P2-1: the box drew the speaker again on a new line id alone, so a second speaker of the same line kept the first portrait (D-223, D-997).
- Full merit. `DialogueChange` now decides each redraw with no engine type: the line on each new say step, and the speaker on each change. `ScenePlay.LineStep` names the say step of the line.
- `ScenePlayTests.ASecondSpeakerOfTheSameLineDrawsItsPortraitAndItsNameAgain` failed on the old rule and passes now. `docs/reviews/pr-86-response.md` records the answer.
- Gitar approved `e9ff621` and `cd1d452` with no thread. Its CI analysis made four claims, and two PR comments answer them: the baselines, RG 3 two times, and the coverage job.

### The state of the build

- The full suite passes: 3,938 tests. `dotnet format` and det-lint report no finding. CI run `36271984962` passed every job of `cd1d452` but `review-gate`. The remote head is the push of this round.

### What is in flight

- The Gitar pass and CI of this round, then `make codex-review PR=86`.

### Traps and gotchas

- The correction draws no capture differently, so each baseline stands.
- `DialogueChange` resets when the box hides, so the next line draws in full.

### The questions that block progress

None. OQ-250 blocks no PR yet.

### The next concrete action

When CI is green but `review-gate` and Gitar completes, run `make codex-review PR=86` in the background.

## Session 335: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #86. Repository: the-thing-below. Branch: `review/pr-86`. PR: #86. Role: reviewer. Base: `6f02d3d`.

### What this session did, and why

- The review found P2-1: a repeated line id can leave the previous speaker's portrait and name on screen (D-223, D-997).
- The review records `Changes required` for effective head `cd1d452` (T-4, D-17).
- The `docs/reviews/` Documents row now names `docs/reviews/pr-86.md` (D-581).

### The state of the build

- `make verify` passes on macOS with 3,937 tests. CI run `36271984962` passes every job except `review-gate`, which awaited this review record. The remote head before the metadata commit is `cd1d452`.

### What is in flight

- The author must correct P2-1, add its regression test, and request a repeat review.

### Traps and gotchas

- The CI capture artifact is the visual source for screen review (D-733). The local renderer does not reproduce its baselines.
- The clean Gitar approval has no item. The CI analysis claim is RG 3 alone (D-964).

### The questions that block progress

None. OQ-250 concerns the pause of a story-scene fight and blocks no PR yet.

### The next concrete action

The author corrects P2-1 and requests a repeat review of PR #86.

## Session 334: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-36, round 1. Repository: the-thing-below. Branch: `feat/pr-36-dialogue`. PR: #86. Role: author. Base: `6f02d3d`.

### What this session did, and why

- The owner answered OQ-150 and OQ-151, folded the playtest fixes of PR-65 into PR-36, and set the text batch and the scope (D-1172 to D-1178). OQ-250 is new.
- Core: a choice holds two to four options, and the simulation version rises to 33 (D-1175, G-17).
- Game: `ScenePlay` follows each step from the ticks and sends one wait intent at its end. `DialogueBox` draws the line, the portrait, the name plate, and the choices. The map walks each actor, and the story pause dims the frame (D-1009, D-1013).
- A set fight takes its transition with no encounter: the boss flag, then the largest body of the group (D-788, D-937).
- Fixes: WASD and Backspace in menus, the "Quantity" label, sharp titles (F-153), the empty line of a service window, and the blur band on the lead or the camera marker (D-1173, D-1177).
- Content: two fixture story scenes on tiles of the fixture hub, three fixture portraits, and the new portraits atlas page.

### The state of the build

- `make verify` passes locally. CI run `36271514085` failed on the baselines alone. This round commits 108 changed baselines and 5 new `scene-*` baselines from its `screen-captures` artifact (D-733). The remote head is the push of this round.

### What is in flight

- Gitar approved `e9ff621` with no finding. Its CI analysis named the baselines, RG 3, and the coverage job, and a PR comment answers each claim. The baseline push needs a new Gitar pass and green CI.

### Traps and gotchas

- Core runs a whole move at once. `ScenePlay.TryWalk` walks the actor back from its end tile along the path.
- Core refuses an intent that no step waits for. `ScenePlay` checks the queue and the queued pause before each intent.
- The stranger trigger stands at (4, 7), off every walk to a service. A test walks each capture route and fails if a route crosses a trigger.

### The questions that block progress

None. OQ-250, the pause of the fight of a story scene, blocks no PR yet.

### The next concrete action

When CI is green except `review-gate` and Gitar completes, run `make codex-review PR=86` in the background.

## Session 333: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #85 (PR-65). Repository: the-thing-below. Branch: `review/pr-85`, pushed to `feat/pr-65-shop`. Role: reviewer. Base: `8e81487`.

### What this session did, and why

- The review found no defect in the shop, gold, save, replay, or screen changes. The record gives `Ready for owner merge` for effective head `4edc5e5` (T-4, D-17).
- The latest Gitar CI analysis named ten missing shop baselines. The head contains all ten, and the current screen-test passes. The record answers this claim and the earlier RG 3 claim (D-964).
- The review inspected all 130 changed paths and the CI screen artifact. No visual fault appeared (D-784).

### The state of the build

- `make verify` passes with 3,905 tests. CI run `36266126601` passes all build, test, format, replay, screen, smoke, and STE checks at effective head `4edc5e5d3ee4f1fbe93bed28fb19e0b5ae98d42a`. The metadata commit is the remote head after push.

### What is in flight

- The PR waits for the owner merge (D-930).

### Traps and gotchas

- `review-gate` failed RG 3 before this record existed. Check the fresh result after the metadata push.
- Gitar's missing-baseline claim was true before the latest head. The ten CI baselines and the screen-test now pass.

### The questions that block progress

None. OQ-121 is resolved by D-1149 to D-1155.

### The next concrete action

The owner reads the review record and confirms the merge.
## Session 332: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #85 (PR-65), round 6. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: #85. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner asked for an empty line between the line of the values and the caption of the list in the gear window (D-1171). The head of the gear window takes six lines.
- The owner then approved the frames of the shop and of the gear window, and directed the Codex review after a green CI run (D-1171).
- The session read the gear frames of `make sheet FIXTURE=menu`.

### The state of the build

- Every local check passes. Ten tests fail alone: the baselines of the ten shop frames, which come from the `screen-captures` artifact of CI.
- Gitar approved round 5 with no thread. The remote head is the push of this round.

### What is in flight

- The CI run of this push, then the baselines of its `screen-captures` artifact: each new shop frame and each menu frame that changed.
- The answer to the CI claim of Gitar (RG 3 before the review), and `make codex-review PR=85` after a green CI run.

### Traps and gotchas

- The review-gate check fails on RG 3 alone until the review record lands. Each other check turns green with the baselines.
- Perl substitutions with braces in C# text fail. Use the Edit tool for each change of C# text.

### The questions that block progress

None. The text batch in the PR waits for the approval of the owner at the merge summary (D-57).

### The next concrete action

Commit the baselines of the CI artifact, push, wait for green checks, and run `make codex-review PR=85`.

## Session 331: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #85 (PR-65), round 5. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: #85. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner read the gear window of round 4 and changed the line of the character (D-1170).
- The level stands in a column of its own close to the name, after a hyphen: "Marrek  -  Level 1". The string `menu.dash` holds the hyphen.
- An empty line stands between that line and the line of the stat names, so the head of the gear window takes five lines.
- The session read the gear frames of `make sheet FIXTURE=menu` at 1x and at 1080 rows.

### The state of the build

- Every local check passes. Ten tests fail alone: the baselines of the ten shop frames, which come from the `screen-captures` artifact of CI.
- Gitar approved round 4 with no thread. The remote head is the push of this round.

### What is in flight

- The approval of the owner for the frames of round 5 in the PR description.
- Then the baselines of the CI artifact, the answer to the CI claim of Gitar (RG 3 before the review), and `make codex-review`.

### Traps and gotchas

- The name column of the gear window holds 10 characters. A longer name of PR-17 needs a wider column.
- Perl substitutions with braces in C# text fail. Use the Edit tool for each change of C# text.

### The questions that block progress

The owner approves the frames before the review (D-1164). The text batch waits for the approval of the owner (D-57).

### The next concrete action

Wait for the approval of the owner. Then commit the baselines of the CI artifact, and run `make codex-review PR=85`.

## Session 330: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #85 (PR-65), round 4. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: #85. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner read the gear window of round 3 and removed a redundancy (D-1169). D-1060 is revised in part again.
- The line above the values of the gear window shows the stat names alone, dim. Each value stands under its name, with its change after it.
- A value that holds takes the plain color, in the gear window and in the popup of the shop, because the names above stand dim. A gain stays green, and a loss stays red.
- The gear window no longer takes the string table. The session read the gear and shop frames of `make sheet FIXTURE=menu`.

### The state of the build

- Every local check passes. Ten tests fail alone: the baselines of the ten shop frames, which come from the `screen-captures` artifact of CI.
- Gitar approved round 3 with no thread. The remote head is the push of this round.

### What is in flight

- The approval of the owner for the frames of round 4 in the PR description.
- Then the baselines of the CI artifact, the answer to the CI claim of Gitar (RG 3 before the review), and `make codex-review`.

### Traps and gotchas

- The plain color of a value that holds is a choice of the session under D-1169. The owner reads it in the frames.
- Perl substitutions with braces in C# text fail. Use the Edit tool for each change of C# text.

### The questions that block progress

The owner approves the frames before the review (D-1164). The text batch waits for the approval of the owner (D-57).

### The next concrete action

Wait for the approval of the owner. Then commit the baselines of the CI artifact, and run `make codex-review PR=85`.

## Session 329: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #85 (PR-65), round 3. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: #85. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner read the frames of round 2 and changed the popups and the change cells (D-1168). D-1060 is revised in part.
- "Equip it now?" is a small box in the middle of the screen. The popup of the characters stands in the middle of the screen, as wide as its columns and as tall as its rows, with a gap under its title.
- A cell of a change shows the whole stat first and the change after it, such as "8 +2". The strings `menu.gear_gain` and `menu.gear_loss` change, so the gear window shows the same form.
- The session read the shop frames and the gear frames of `make sheet FIXTURE=menu`.

### The state of the build

- Every local check passes. Ten tests fail alone: the baselines of the ten shop frames, which come from the `screen-captures` artifact of CI.
- Gitar approved round 2 with no thread. The remote head is the push of this round.

### What is in flight

- The approval of the owner for the frames of round 3 in the PR description.
- Then the baselines of the CI artifact, the answer to the CI claim of Gitar (RG 3 before the review), and `make codex-review`.

### Traps and gotchas

- The level-up text of a fight ("+3 ATK") shows the gain alone, with no whole stat, so D-1168 leaves it as it is.
- Perl substitutions with braces in C# text fail. Use the Edit tool for each change of C# text.

### The questions that block progress

The owner approves the shop frames before the review (D-1164). The text batch waits for the approval of the owner (D-57).

### The next concrete action

Wait for the approval of the owner. Then commit the baselines of the CI artifact, and run `make codex-review PR=85`.

## Session 328: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #85 (PR-65), round 2. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: #85. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner read the shop frames in the PR description and changed the list window (D-1165 to D-1167). The records revise D-1158, D-1159, and D-1164 in part.
- The list stands under the title, each value stands in a column, and a piece shows its own stats at the bottom left. The gold reads "250 gold".
- An entry at its stack limit leaves the list (Core, D-1166). An entry that the gold cannot pay takes no dim and no line.
- After a buy of gear, a popup asks "Equip it now?" for each copy. Yes lists the party with the red and green change, and two full accessory slots ask "Replace which one?".
- The captures add `shop-equip` and `shop-who` at both body sizes. The session read each shop frame of `make sheet FIXTURE=menu`, the frames of 1080 rows included.

### The state of the build

- Every local check passes. Ten tests fail alone: the baselines of the ten shop frames, which come from the `screen-captures` artifact of CI.
- Gitar approved round 1 with no thread. The remote head is the push of this round.

### What is in flight

- The approval of the owner for the new shop frames in the PR description.
- Then the baselines of the CI artifact, the answer to the CI claim of Gitar (RG 3 before the review), and `make codex-review`.

### Traps and gotchas

- Perl substitutions with braces in C# text fail. Use the Edit tool for each change of C# text.
- The fixture party is Marrek alone, so the character list of the equip step shows one row.

### The questions that block progress

The owner approves the shop frames before the review (D-1164). The text batch waits for the approval of the owner (D-57).

### The next concrete action

Wait for the approval of the owner. Then commit the baselines of the CI artifact, and run `make codex-review PR=85`.
