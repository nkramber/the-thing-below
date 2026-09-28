## Session 397: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-52, round 1. Repository: the-thing-below. Branch: `feat/pr-52-map-preview`. PR: the one PR of PR-52, before GitHub gives a number. Role: author. Base: `2fc559a`.

### What this session did, and why

- Asked the four open points of PR-52. The owner took each recommended option, and D-1317 to D-1320 record them.
- Added the `preview` command and `make preview`. The command renders each map, or one map with `--map`, from the committed atlas (D-165, D-1319).
- The preview draws the tiles, the traps, and each sprite of the start of a map in the sort order of the map screen. It draws no light, no party, and no hidden part (D-1317, D-1318).
- Moved the map drawing uses and the drawn-kind list from `MapScreen` into `MapDrawings` of Core. Game, Tests, and Tools read one copy, and no rule reads them, so the simulation version stays (G-17).
- Added the preview rule to the `pr-review` skill (D-1320), and updated both roadmaps, the design phase list, and the `csharp-conventions` skill.

### The state of the build

- `make verify` passed on the branch. The remote head is `2fc559a` until the first push of this PR.
- The mutation check: a preview with no flip and a preview with the sort reversed each fail one test of `MapPreviewTests`.

### What is in flight

- The first push, the Gitar pass, then `make codex-review`.

### Traps and gotchas

- A new line in `CLAUDE.md` passes its 16 KB limit (SIZE 1), so the agent files do not name `make preview`.
- The overworld preview is 5120 by 4096 pixels and 643 KB, under the limit of 10 MB of GitHub (D-514).
- The preview draws a trap in its closed look, also a trap that the party sees only with the Theft drill (D-1317).

### The questions that block progress

None.

### The next concrete action

Push, attach the four previews to the PR description, and run the Gitar poll of the `gitar-review` skill.

## Session 396: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #97 (PR-51). Repository: the-thing-below. Branch: `review/pr-97`, tracking `origin/feat/pr-51-png-import`. Role: reviewer. Base: `e9294ec0bb29d30d0ebcdcc47fa5e251cf63cda7`.

### What this session did, and why

- Re-reviewed effective head `95048efd0ddaa39afdc5850f59e9141822f98b5d` and the full correction diff. `CreateNew` prevents output aliases from replacing a drawing, and failed writes remove their partial file (T-2, D-1313).
- Updated `docs/reviews/pr-97.md` to preserve the earlier verdicts and give `Ready for owner merge` for the effective head.
- Verified the Gitar item and its confirmation. The CI analysis names RG 4 and RG 5; the log confirms both were stale-record faults, and the updated record answers the claim (D-964).

### The state of the build

- The remote effective head is `95048efd0ddaa39afdc5850f59e9141822f98b5d`. Build, focused tests (9), STE, Gitar, and each CI check pass. The post-push `review-gate` passes on this metadata commit.

### What is in flight

- The review and handoff are published together as one metadata commit. The remote PR head matches the local commit, and `review-gate` passes.

### Traps and gotchas

- Push with `git push origin HEAD:feat/pr-51-png-import`. The metadata commit leaves the effective head unchanged (D-610).

### The questions that block progress

None.

### The next concrete action

End this review session for PR #97.

## Session 395: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 4. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: #97. Role: author. Base: `e9294ec`.

### What this session did, and why

- Gitar gave round 3 at `3aec94f` one item: a failed write of `frame-png` left a part of the PNG, and the next run refused the path. The item has full merit.
- `FramePngCommand.WriteNewFile` now removes the file that it made when the write fails. A failed removal gives a message that names the part (T-2).
- The fault prefix of `frame-png` is now `stopped`, because a failed removal leaves a part and "wrote nothing" is then false.
- Two tests cover the helper. The Gitar fix removed the file on an `IOException` alone. This fix covers a denied access and a failed removal too (D-1072).
- The CI analysis named RG 4 and RG 5 again. They wait for the repeat review, and the comment of round 2 answers them.

### The state of the build

- The remote head before this round is `3aec94f`. `make verify` passed on the Mac before the push.

### What is in flight

- The Gitar pass of round 4, then a repeat `make codex-review PR=97`. The review of P2-1 is in its third round, so a third open round gives the three-strike stop (D-929).

### Traps and gotchas

- No test makes the removal fail, because no portable way exists. The message path is plain code with no branch.

### The questions that block progress

None.

### The next concrete action

Reply on the Gitar thread with the commit, then run `make codex-review PR=97` in the background after CI.

## Session 394: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 3. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: #97. Role: author. Base: `e9294ec`.

### What this session did, and why

- Gitar approved `7ca7b25` with no thread. Its CI analysis named RG 4 and RG 5, which waited for the repeat review, and a PR comment answered it (D-964).
- The repeat review kept P2-1 open: a symbolic link to the drawing passed the compare of paths. The trigger reproduced.
- `frame-png` now never writes over a file. The mode `CreateNew` refuses every name that exists, so no alias can reach a drawing (T-2).
- `docs/reviews/pr-97-response.md` records round 2. The runbook and section 7.52 name the rule.

### The state of the build

- The remote head before this round is `1bcb5c3`, the review record on `7ca7b25`. `make verify` passed on the Mac before the push.

### What is in flight

- The Gitar pass of round 3, then a repeat `make codex-review PR=97`. This is the second round of P2-1, so a third open round gives the three-strike stop (D-929).

### Traps and gotchas

- The symbolic link test runs on each CI leg. The Windows runner needs the right to make a link.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of round 3, then run `make codex-review PR=97` in the background.

## Session 393: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #97 (PR-51). Repository: the-thing-below. Branch: `review/pr-97`, tracking `origin/feat/pr-51-png-import`. Role: reviewer. Base: `e9294ec0bb29d30d0ebcdcc47fa5e251cf63cda7`.

### What this session did, and why

- Re-reviewed effective head `7ca7b25106e18b8ed8234b0c66824c9e2e21d411` and the full 25-path diff.
- Checked the fix for P2-1. A symlink output still truncates the drawing file, so P2-1 remains open (T-2, D-1313).
- Read the current Gitar CI claim and reply. The job log confirms RG 4 and RG 5 wait for this repeat review (D-964).

### The state of the build

- The remote head before this metadata commit is `7ca7b25106e18b8ed8234b0c66824c9e2e21d411`.
- The first metadata commit, `41f06f11738ea9f868fe1e1564a14eb4a76153f4`, is pushed and verified as the PR head.
- The CI build, test, format, smoke, bots, replay identity, screen-test, det-lint, night-gate, STE, and Gitar checks pass. `review-gate` fails RG 4 and RG 5 while P2-1 remains open.
- `make build` passed. The focused frame-png tests passed, 5 of 5. `make test` returned `No test projects were found`.

### What is in flight

- The author must prevent output aliases, including symlinks, from overwriting the drawing and add regression tests.

### Traps and gotchas

- `Path.GetFullPath` does not resolve a symlink. The `frame-png` writer follows it and replaces the drawing bytes.
- Push with `git push origin HEAD:feat/pr-51-png-import`.

### The questions that block progress

None.

### The next concrete action

Correct P2-1 for file aliases, then request a repeat review of the new effective head.

## Session 392: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 2. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: #97. Role: author. Base: `e9294ec`.

### What this session did, and why

- Gitar approved `0cbf3ca` with no thread. Its CI analysis named the RG 3 fault, which waited for the review record, and a PR comment answered it (D-964).
- Every other CI check passed on the three legs. `make codex-review PR=97` gave `Changes required` with P2-1: `frame-png` could write its PNG over the drawing file.
- P2-1 has full merit. The command now refuses an output path of the drawing file, and a regression test failed on `0cbf3ca` before the correction.
- `docs/reviews/pr-97-response.md` records the answer, and the runbook adds the message to its table of errors.

### The state of the build

- The remote head before this round is `7e49b7f`, the review record on `0cbf3ca`. `make verify` passed on the Mac before the push.

### What is in flight

- The Gitar pass of round 2, then a repeat `make codex-review PR=97`.

### Traps and gotchas

- The path compare ignores case, because the disk of the Mac ignores it.
- The Documents line of `docs/reviews/` now names the record and the response file.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of round 2, then run `make codex-review PR=97` in the background.

## Session 391: 2026-09-28, Codex

Author: Codex
Session: review PR #97 (PR-51). Repository: the-thing-below. Branch: `review/pr-97`, tracking `origin/feat/pr-51-png-import`. Role: reviewer. Base: `e9294ec0bb29d30d0ebcdcc47fa5e251cf63cda7`.

### What this session did, and why

- Reviewed effective head `0cbf3ca8ca9fef24187344199707fc723ee8b2e6`, the full 23-path diff, PR comments, decisions, and PR-51 exit tests.
- Found P2-1: `frame-png` overwrites its source drawing when `--drawing` and `--out` name the same path (D-1313, T-2).
- Verified the author's answer to Gitar's CI claim. RG 3 alone failed because the review record was absent. The clean approval has no item (D-964).
- `make verify` passed with 4,498 tests. Required CI checks passed except the expected `review-gate` RG 3 fault.

### The state of the build

- The remote head before this metadata commit is `0cbf3ca8ca9fef24187344199707fc723ee8b2e6`.
- The review record gives `Changes required` for P2-1. The `review-gate` check failed RG 3 before the record existed.

### What is in flight

- The author must reject a `frame-png` output path that matches the drawing path and add a regression test.

### Traps and gotchas

- A same-path reproduction returned success and replaced drawing JSON with a valid PNG.
- Push with `git push origin HEAD:feat/pr-51-png-import`.
- Session 381 moves to `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Fix P2-1, then request a repeat review of the new effective head.

## Session 390: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 1. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: opened by this round. Role: author. Base: `e9294ec`.

### What this session did, and why

- Asked the owner seven questions that the roadmap and D-686 to D-689 left open, then recorded D-1310 to D-1316.
- D-1310 revises D-689 in part: the target drawing file sets the frame. A scan of the 84 spike pictures at `e3c50b4` found 26 of 42 map sprites above 32 pixels.
- Built the `import` command, with the hand-edit mode and the generator mode, and the `frame-png` command (D-688, D-1313).
- Each import replaces the rows of one frame alone, and a read back through the reader of Core guards the write (D-1311, T-2).
- Added 40 tests, one for each exit test of section 7.52 and more. Added `docs/runbooks/art-import.md` and three glossary terms.

### The state of the build

- The remote head of `main` is `e9294ec`. This round pushes the branch and opens the PR.
- `make verify` ran on the Mac before the push. The PR description records the result.

### What is in flight

- The Gitar pass of round 1, then `make codex-review`.

### Traps and gotchas

- The word export names a build of the Game (D-481), so the command of D-1313 is `frame-png`.
- The generator mode centers the content (D-1312). A character of less than the full height stands above the bottom row, unlike the current Marrek drawings.
- The fixture drawing files hold each frame on one line. A write puts one row on each line, and a file of the repository keeps each byte.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of round 1, then run `make codex-review PR=<n>` in the background.

## Session 389: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #96 (PR-111). Repository: the-thing-below. Branch: `review/pr-96`, tracking `origin/feat/pr-111-overworld-treasure`. Role: reviewer. Base: `2ac46b6e7280a5000c9397b4b592fcbf1889949c`.

### What this session did, and why

- Re-reviewed effective head `d56432a9dd281d48bcbce2a411862392aaa4d325`. The author added both required art sheets to the PR description.
- Opened and checked both sheets. They show the cairn drawings at the required scales, grounds, and light directions (D-514, D-521, D-668).
- Closed P2-1 under the unchanged-head rule of D-1303. The record now gives `Ready for owner merge`.
- Verified the author's answer to Gitar's RG 4 analysis. Its clean code approval has no item (D-964).

### The state of the build

- The effective head stays `d56432a9dd281d48bcbce2a411862392aaa4d325`. The remote tip before this metadata commit is `1a8bbd0d64fc638e9bb16d9e43db48fd190dc1bb`.
- Implementation checks passed on macOS, Ubuntu, and Windows. The review-gate failed RG 4 because the prior verdict remained in the record.

### What is in flight

- The review record and this entry need one metadata commit and a push to the PR branch.

### Traps and gotchas

- Push with `git push origin HEAD:feat/pr-111-overworld-treasure`.
- Session 379 moves to the archive to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Commit the review record and this entry together. Push, fetch, check the remote head, then read the new review-gate result.

## Session 388: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-111, round 2. Repository: the-thing-below. Branch: `feat/pr-111-overworld-treasure`. PR: #96. Role: author. Base: `2ac46b6`.

### What this session did, and why

- Gitar approved `d56432a` with no thread. Its CI analysis named the RG 3 fault, which waits for the review record, and a PR comment answered it (D-964).
- Every CI check passed except `review-gate`. `make codex-review PR=96` gave `Changes required` with P2-1: no art review sheet in the description.
- P2-1 has full merit. `atlas --sheets` rendered the sheets, and `gh pr edit --attach` put sheet 2 of the map sprites and normal-map sheet 3 into the description. `docs/reviews/pr-96-response.md` records the answer.

### The state of the build

- The effective head stays `d56432a`, and this round changes the metadata set alone (D-610). CI on `d56432a` passed on every leg.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=96` for the repeat review.

### Traps and gotchas

- A PR that adds a drawing attaches the sheets of `atlas --sheets` to its description: the color sheet and the normal-map sheet (D-514, D-521, D-668). PR-95 got the same finding.

### The questions that block progress

None.

### The next concrete action

Run the Gitar poll, answer each item, then run `make codex-review PR=96` in the background.
