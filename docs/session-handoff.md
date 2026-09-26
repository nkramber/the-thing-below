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

## Session 327: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-65, round 1. Repository: the-thing-below. Branch: `feat/pr-65-shop`. PR: the one PR of PR-65, with no GitHub number before the push. Role: author. Base: `8e81487`.

### What this session did, and why

- The owner answered OQ-121 and the questions that the work raised: D-1149 to D-1164. OQ-121 is resolved.
- Core: the shop file with the types and the stocks, a value on each item and gear record, the shop kind of a service, and the price of a rest. The buy and the sale rules, the stock of each shop in the save, and the gold of a fight.
- The save format rises to 16, and the simulation version rises to 32 (G-17). The `gold` debug command sets the gold (D-1162).
- Game: the shop window, the gold panel, the priced rest, and the gold line of a win. The captures add the buy list, the count, and the sale list at both body sizes.
- The session read each frame at 1x of `make sheet FIXTURE=menu`: the shop frames, the rest, and the main list. The frames read well.

### The state of the build

- Every local check passes: build, test, format, det-lint, atlas, STE, and the smoke session. Six tests fail alone: the baselines of the new shop frames, which come from the `screen-captures` artifact of CI.
- The remote head is the push of this round. The content hash and the replay identity file hold the new values.

### What is in flight

- The CI run of the first push. Take the six shop baselines and each changed menu baseline from the CI artifact, read each one, and commit them.
- The Gitar pass, then `make codex-review`.

### Traps and gotchas

- `make sheet` stops on this Mac at the first frame of 1080 rows, because the screen gives 1920 by 955. Read the frames at 1080 rows in the CI artifact.
- Each menu frame except the map now sends `gold 250` first, so each menu baseline changes.
- The fixture shop lists the bolt, which the start pack holds, so the list hides it (D-1153). The Core tests sell a lesson that the party lacks.

### The questions that block progress

None. The owner reads the shop captures before the review (D-1164). The text batch of the shop waits for the approval of the owner (D-57).

### The next concrete action

Open the PR, run the Gitar poll, and commit the baselines of the CI artifact.

## Session 326: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #84 (PR-14), round 4. Repository: the-thing-below. Branch: `feat/pr-14-hub`. PR: #84. Role: author. Base: `c7e6191`.

### What this session did, and why

- The review of `make codex-review` gives `Ready for owner merge` for the effective head `6c7837f` in `docs/reviews/pr-84.md`, with no open finding.
- Each check of `6c7837f` passed, and Gitar approved it with no thread. The author answered each claim of the CI analysis of Gitar.

### The state of the build

- The effective head is `6c7837f`, and the review approves it. This entry is a commit of the metadata set, so the approval stands (D-610).

### What is in flight

- The checks of this commit, and the merge confirmation of the owner (D-933, D-942).

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

None. OQ-249 blocks PR-17.

### The next concrete action

After the merge, write the transitional prompt of step 6 of the `one-pr-one-session` skill.

## Session 325: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #84 (PR-14). Repository: the-thing-below. Local branch: `review/pr-84`; PR branch: `feat/pr-14-hub`. PR: #84. Role: reviewer. Base: `c7e6191`.

### What this session did, and why

- Reviewed effective head `6c7837f` and wrote `docs/reviews/pr-84.md`.
- Verified the Gitar CI-analysis answer: RG 3 alone failed because the review record was absent. The author answered the item, and the CI log confirms the cause (D-964).
- `make verify` passed with 3,803 tests. Inspected the CI screen captures for the hub, Rest, Save, Party, and main-list frames (D-784).
- Corrected the PR Documents row to name `docs/reviews/pr-84.md`.

### The state of the build

- The effective head is `6c7837f`. The metadata tip is `ba92a31`, and its fresh `review-gate` check passed. All implementation checks passed on the effective head.

### What is in flight

- This entry and the review record are published on `feat/pr-14-hub`. The fresh Gitar pass has no item, and the live `review-gate` passed.

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

OQ-249 remains open for PR-17.

### The next concrete action

Verify the final metadata push, then end this review session.

## Session 324: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #84 (PR-14), round 3. Repository: the-thing-below. Branch: `feat/pr-14-hub`. PR: #84. Role: author. Base: `c7e6191`.

### What this session did, and why

- Gitar approved `ac14bc2`. The screen compare of `ac14bc2` matched 134 frames and missed the 4 baselines of `menu-rest` and `menu-save` alone.
- The author read those 4 frames of the artifact (D-784). The lead faces the keeper under the Rest window, and the lead faces the waystone under the Save window, with no clip. This round commits the 4 baselines (D-733).

### The state of the build

- The head of this push holds every baseline. Each check but `review-gate` waits for this push.

### What is in flight

- The checks and the Gitar pass of this push, then `make codex-review PR=84` (D-926).

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

None. OQ-249 blocks PR-17.

### The next concrete action

When each check but `review-gate` passes and Gitar completes, run `make codex-review PR=84` in the background.
