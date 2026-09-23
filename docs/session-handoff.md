## Session 233: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #63, round 1. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: reviewer. Base: `097ea32`.

### What this session did, and why

- Reviewed all 37 changed paths from base `097ea32` through effective head `ca9dd85`.
- Confirmed Claude Code authored the PR, and the Codex reviewer passes the provider gate (T-4, D-17).
- Found that the Gitar pass check reads only 100 review threads, and that duplicate finding IDs split the three-strike count (D-14, D-929).
- Ran `make verify`: 2,359 tests passed, and each local check passed.
- Read the live CI checks. Each implementation check passed. Review-gate failed RG 3 because the review record was absent.
- Added `docs/reviews/pr-63.md` with two open findings and the verdict `Changes required`.

### The state of the build

- The base and merge base are `097ea32`. The effective code head is `ca9dd85`.
- Local verification passed. Live build, test, format, coverage, lint, identity, screen-test, smoke, and STE checks passed on their CI legs.
- The Gitar check passed on `ca9dd85`. The live merge settings match `docs/runbooks/branch-protection.json` (D-931).

### What is in flight

- The author needs to fix P2-1 and P2-2 in `docs/reviews/pr-63.md`, then request a repeat review.
- This metadata commit publishes the review record and this entry. The `docs/reviews/` Documents row needs correction after publication.

### Traps and gotchas

- Metadata commits do not change effective head `ca9dd85` (D-610).
- The pre-publication review-gate failure is RG 3 because the review record is absent. RG 4 and RG 5 skip until the record lands.

### The questions that block progress

None. The PR-95 roadmap lists no open question.

### The next concrete action

The author fixes both findings and requests a repeat review of PR #63.

## Session 232: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #63, round 1. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: author. Base: `097ea32`.

### What this session did, and why

- Checked the precondition: PR #62 merged as `097ea32`. `docs/reviews/pr-62-response.md` is on `main` through PR #62, so no untracked file of PR #62 stayed in the checkout.
- Asked the owner six questions, and recorded the direction and the answers as D-926 to D-931. The effect columns of D-8, D-576, D-578, D-582, and D-601 name each revision.
- Built `make codex-review PR=<n>` and the `codex-review` command of Tools, with 79 tests. It covers the install and the version of the CLI, the model probe, the start checks, the Gitar pass, the worktree, the outcome codes, and the three-strike count.
- Added the `Open at:` line to the record format, and the loop to `CLAUDE.md`, `AGENTS.md`, the skills, the PR template, `docs/runbooks/merge.md`, and `docs/runbooks/branch-protection.json`.
- Added PR-95 to section 8 of `docs/design.md`, to the phase-2 file as section 7.22, and to `area-ci.md` as section 7.20. G-15 names the auto-merge.

### The state of the build

- `main` is `097ea32`. The branch holds one commit on it, and this entry is in that commit.
- `make verify` passed on this machine before the commit.
- A run of `make codex-review PR=62` installed CLI 0.156.1, passed the probe, and refused PR #62 with four reasons, as the command must.

### What is in flight

- The Gitar pass of PR #63, then the first live run `make codex-review PR=63`, then the answers.
- The three settings of D-931 wait for the approval of the owner, before the hand-over. The owner merges PR #63 by hand.

### Traps and gotchas

- Make gives exit 2 for each failed target. Read the last line of the command: `codex-review: outcome <name> (exit <code>)`.
- The pre-commit hook refuses a checkout with no branch. Thus the worktree takes the local branch `review/pr-<n>`, and the reviewer pushes with `HEAD:<branch>`.
- `CLAUDE.md` holds 16,333 bytes of the limit of 16,384.
- The record of this PR needs an `Open at:` line in each finding, because the command reads it.
- This PR changes no workflow file. The review applies because Tools and the Makefile are outside the override set.

### The questions that block progress

None. D-926 to D-931 hold the answers of 2026-09-23.

### The next concrete action

Answer the Gitar pass of PR #63. Then run `make codex-review PR=63` in the background.

## Session 231: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #62, round 2. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: reviewer. Base: `05dcc3d`.

### What this session did, and why

- Recomputed the effective head. The PR tip is `3c3de99`, and its changes since `238bafe` are review and handoff metadata only.
- Read the author's answers to each claim of the Gitar CI analysis and checked their cited commits and job results.
- Rechecked the prior review, the complete implementation scope, and the current Gitar pass. The code review approves the implementation, and there are no open review threads.
- Ran `make verify`: 2,277 tests passed; format, det-lint, ste-check, replay identity, content hash, atlas, and smoke passed.
- Updated `docs/reviews/pr-62.md`, retained the earlier `Blocked` verdict, and gave `Ready for owner merge` for effective head `238bafe`.

### The state of the build

- `main` is `05dcc3d`. Before this review commit, the remote PR head was `3c3de99` and the effective head was `238bafe`.
- CI run 35823669111 passed the implementation checks on `238bafe`. Metadata run 35826831792 passed applicable checks, with implementation legs skipped. Review-gate run 35826840307 failed only RG 4 because the previous record still said `Blocked`.
- Review-gate run 35828245270 passed RG 1 to RG 8 after publication at `d366f7e`. Metadata run 35828245787 and Gitar passed on that head; the Gitar review approves the code.

### What is in flight

- The review is ready for the owner to merge.

### Traps and gotchas

- Metadata-only pushes skip platform build, replay identity, and screen-test legs. The implementation evidence remains run 35823669111 on `238bafe`.
- The Gitar analysis on `3c3de99` approves the code. Its reported CI fault is RG 4 reading the prior review verdict.

### The questions that block progress

None. OQ-241 asks which later PR adds ceiling shafts and blocks no progress here (D-924).

### The next concrete action

The owner merges PR #62.

## Session 230: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 3. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- The review of session 229 gave `Blocked` for `238bafe`, with no finding in the code. The author had handed over with the CI analysis of the Gitar dashboard unanswered (D-14, D-67).
- Answered each claim of that analysis in a PR comment, and wrote `docs/reviews/pr-62-response.md`.
- The Gitar pass on this PR: 6 claims of the CI analysis over 4 heads, and no review thread. 3 claims had merit: the absent baselines (`5747c01`, `8b299b3`), the RG 7 line (PR description), and RG 4 (this answer). RG 6, RG 3, and the coverage failure had no merit for the author.
- Every code review of the pass approved, from `2d67459` to `26405dd`.

### The state of the build

- `main` is `05dcc3d`. The effective head is `238bafe`, and this commit is in the metadata set (D-610).
- CI passes every implementation check on `238bafe`. Review-gate passes all but RG 4, which reads the `Blocked` verdict.

### What is in flight

- The other provider repeats the review of `238bafe` with this response.

### Traps and gotchas

- The CI analysis in the Gitar dashboard is a comment of the pass, also when the code review approves. Answer each claim on the PR before the hand-over, and record the counts here.

### The questions that block progress

None. OQ-241 blocks no progress on PR #62.

### The next concrete action

The other provider repeats the review of PR #62.

## Session 229: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #62, round 1. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: reviewer. Base: `05dcc3d`.

### What this session did, and why

- Reviewed the full diff from `05dcc3d` through effective head `238bafe`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the readers, render path, shaders, effect budget, decisions, tests, CI, and all 74 screen captures.
- Added `docs/reviews/pr-62.md`. The verdict is blocked because the author has not answered the current Gitar comment.

### The state of the build

- `main` is `05dcc3d`. The remote PR head is `09f2281`, and the effective head is `238bafe`.
- The focused tests pass with 230 tests. CI run 35823669111 passes each implementation check on the configured legs.
- Metadata CI run 35825994991 passes its applicable checks. Review-gate run 35825994798 passes RG 1 to RG 3 and RG 5 to RG 8. RG 4 reads the required `Blocked` verdict.
- Gitar passes on `09f2281`; its dashboard still reports the RG 4 failure. The author has not replied.

### What is in flight

- The PR waits for the author to answer the Gitar comment, and for the next review of the response.

### Traps and gotchas

- The first Gitar pass flagged the absent review path. The latest pass reports the `Blocked` verdict under RG 4.
- OQ-241 asks which later PR adds ceiling shafts, and blocks nothing here.

### The questions that block progress

None. OQ-241 blocks no progress on PR #62.

### The next concrete action

The author answers the Gitar comment, then the other provider reviews the response.

## Session 228: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 2. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- The owner read the fixture shafts and said that a beam from a bare wall makes no sense. D-924 keeps wall shafts only under an opening, such as a window (OQ-240).
- The owner kept the still beam. D-925 removes the shimmer, its fields, the shimmer shaft, and the stepped still captures, and `LightWave` went back into `GlowPass`.
- A shaft kind now needs a drawing, and the map draws it on the wall. The fixture dungeon holds one barred window at (6, 1), with `drawing.decor_fixture_window` in the atlas.
- The owner wants ceiling shafts later. OQ-241 asks which PR adds them.

### The state of the build

- `main` is `05dcc3d`. The tests, format, lint, identity, content, atlas, and smoke pass on this machine. The author read `make sheet` for the map fixture.
- CI run 35821954258 on `efbf7dd`: the two capture runs matched on all 74 captures. The 48 baselines that the window changed come from its artifact (D-733).
- Gitar approved `efbf7dd` with no finding. CI and gitar passed on `8b299b3`, with review-gate waiting for the review record.
- The owner turned on SSH on the Deck, and this session ran the sweep of `spike/deck-test` there at the owner's request. The results are `a6f9f0b` on that branch, and they hold the pass row of 6 (F-106).

### What is in flight

- The PR waits for CI on the baseline commit and the review of the other provider.

### Traps and gotchas

- The window sits on the brick face of the wall tile, rows 16 to 27. The cap above the face is rows 0 to 15.
- A shaft sprite draws unshaded, as a torch does, so the opening stays bright in the dark.
- Over SSH, the Deck takes no `.bashrc`, so set `DOTNET_ROOT`, `PATH`, and `GODOT` by hand, and `DISPLAY=:0` for the sweep. Godot then falls back to Wayland.
- The owner should stop SSH on the Deck after this PR: `sudo systemctl stop sshd`.

### The questions that block progress

None. OQ-241 blocks no PR yet.

### The next concrete action

The other provider reviews PR #62.

## Session 227: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 1. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- Asked the questions of PR-92 and recorded OQ-233 to OQ-239 and D-917 to D-923. D-917, D-919, and D-923 revise D-622, D-916, and D-617 in part.
- Built the tilt-shift blur, the vignette, and the light shafts, each in a smooth mode and a stepped mode (D-917). `content/effects/hd2d.json` holds the mode.
- A shaft is a shaft kind in `content/decor/shafts/`, and a decor file places it on a wall (D-918). The fixture dungeon holds a still shaft and a shaft that shimmers (D-921).
- The shafts draw in the overlay. A scene view joins the world and the overlay, and the frame draws it with the blur and the vignette. The marks moved to a mark view above them (D-919).
- The budget counts the passes of each map, a map with no weather included, and the pass row is 6 (D-920, D-923). The simulation version is 15.
- Added the stages `pass-look`, `full-load-24-look`, and `budget-rows` to `spike/deck-test` as `6bb1595` (D-922).
- Added stepped captures of the map, a fight, and the still fixture. The author read the frames of `make sheet`.

### The state of the build

- `main` is `05dcc3d`. `make verify` passed on this machine except the 6 new baselines.
- CI run 35820532779 on `2d67459`: the two capture runs matched on all 78 captures. The 72 baselines that changed come from its artifact (D-733). The 5 ui captures and the picture capture did not change.
- Gitar approved `2d67459` with no finding.

### What is in flight

- The owner runs the Deck sweep of `spike/deck-test`. The pass row of 6 stands only when `full-load-24-look` and `budget-rows` hold 60 frames per second (G-14).
- The PR waits for CI on the baseline commit and the review of the other provider.

### Traps and gotchas

- Godot takes no default value for a uniform array. The spike copy of the shaft shader uses constants.
- Two walk fixtures can follow each other in the capture list, so the session rebuilds the run when the fixture changes.
- The view of the scene reads with a linear filter. The blur shader reads each sharp pixel at the middle of its art pixel.

### The questions that block progress

None. The Deck sweep is a measurement, not a question.

### The next concrete action

Read the Deck reports, commit them to `spike/deck-test`, and record the numbers in the PR. Then commit the baselines from the CI artifact.

## Session 226: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #61, round 1. Repository: the-thing-below. Branch: `feat/pr-59-glow`. PR: #61. Role: reviewer. Base: `430c8e9`.

### What this session did, and why

- Reviewed the complete diff from `430c8e9` to effective head `0d55d93`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the glow reader, light bound, rendering path, overlay, budget, tests, decisions, and captures.
- Added `docs/reviews/pr-61.md` with no finding, and published the review record.

### The state of the build

- The remote head of `main` is `430c8e9`. Metadata commit `89a606a` reached the branch, and the effective head stays `0d55d93`.
- `make verify` passed with 2,221 tests. `make sheet` wrote all 72 captures.
- CI run 35815610824 passed every applicable implementation check on every leg. Gitar approved the effective head.
- Metadata run 35816940918 and review-gate run 35816940027 passed. The fresh gate passed RG 1 to RG 8.

### What is in flight

- PR #61 is ready for owner merge.

### Traps and gotchas

- The first review-gate run failed because this review record was not in the diff yet. The metadata run passed after the record reached the branch.
- The fog, hit bursts, and marks use the overlay view above the glow (D-916).

### The questions that block progress

None. OQ-102 and OQ-232 are resolved by D-910 and D-915.

### The next concrete action

The owner merges PR #61.

## Session 225: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-59, rounds 1 to 3. Repository: the-thing-below. Branch: `feat/pr-59-glow`. PR: #61. Role: author. Base: `430c8e9`.

### What this session did, and why

- Asked OQ-102, and the owner chose HDR 2D with a threshold and a smooth bloom, fire first (D-910 to D-912).
- Round 1 built HDR 2D, the bound of the lit art below the threshold, and a seed loop against the light curve of Game (F-103, F-104).
- The owner asked for the glow pass of our own, a stepped mode, and a pulse in place of the flicker. Round 2 built them (D-913, D-914, F-105).
- The owner compared both and kept HDR 2D at half intensity (D-915, OQ-232). The pulse stays on a glow rectangle of 8 by 8 over each wall torch flame.
- The owner said the fog must not glow. The fog, the hit bursts, and the marks now draw in an overlay view with no HDR 2D, above the glow (D-916). The reader refuses a lit fog.

### The state of the build

- `main` is `430c8e9`. The branch holds rounds 1 to 3. `make verify` passes on this machine, and `make sheet` wrote every capture with no error line.
- CI run 35815275241 on `6b612f2` passed every job but the baseline step of screen-test. Its two runs matched on all 72 captures. The 66 world baselines that changed come from its artifact (D-733).

### What is in flight

- PR #61 waits for CI on the baseline commit, gitar, and the review of the other provider.

### Traps and gotchas

- The glow of Godot averages its first step over about 8 by 8 art pixels. A smaller or dimmer source gives no glow.
- A view draws an item only when each parent shares its layer (F-105). `GlowPass.LiftAboveGlow` sets the layer on the node, its children, and its parents.
- An object initializer of a texture rect sets `ExpandMode` before `Size`.
- Do not redirect `make smoke` output into `artifacts/smoke.log`. The target writes that file itself, and the loop filled 20 GB.

### The questions that block progress

None. OQ-102 and OQ-232 are resolved.

### The next concrete action

Answer each gitar finding on PR #61, then hand the PR to the other provider.

## Session 224: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #60, round 3. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: reviewer. Base: `a20d32f`.

### What this session did, and why

- Re-reviewed PR #60 at effective head `bff8622`.
- The owner’s later instruction in the feedback says to keep the dated records. D-909 records this override and ends the pause.
- Verified that the PR #58 and PR #59 review records, and the PR #59 handoff entries, match `origin/main`. Sessions 209 and 210 remain in the archive.
- Marked P2-1 fixed and changed the current verdict to `Ready for owner merge`.

### The state of the build

- The remote head of `main` is `a20d32f`. The effective head of PR #60 is `bff8622`.
- `make ste-check` passed with 0 findings. CI passed all applicable implementation checks, and Gitar approved the head. Review-gate failed RG 4 and RG 5 because the review record still named the prior verdict and head.
- The updated review and this handoff need commit and push. A fresh review-gate result must be checked.

### What is in flight

- PR #60 waits for the review-gate result on the updated review record.

### Traps and gotchas

- The initial owner choice removed the dated records. The later choice kept them, and D-909 records that resolution.
- The effective head includes the restored decision and history files. Metadata commits do not change it.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review. Then verify the branch head and fresh review-gate result.
