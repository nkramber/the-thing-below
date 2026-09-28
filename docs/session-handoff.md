## Session 402: 2026-09-28, Claude Code

Author: Claude Code
Session: author of PR-17. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- Asked the owner every question of PR-17. D-1328 to D-1361 hold the answers. PR-17 split: the places as rule content and text with stand-in drawings, then PR-112 to PR-115 for the art and the effects (D-1328, D-1329).
- OQ-249 now blocks PR-23, because no character of the first playable waits in reserve (D-1330). OQ-245 is resolved as the cover (D-1352). New questions: OQ-255 and OQ-256.
- Built six Core changes by owner choice: the pay step (D-1335), a gate on every map (D-1347), the time of a map set by a flag with save format 21 and record format 6 (D-1349), the kit of a newcomer (D-1350), the cover (D-1352), and the cap of 9,999 (D-1357). Also the battle line of two lines that scrolls in a box of one line (D-1356, D-1359), the capital of a common name (D-1358), and the wait fix of D-1360.
- Wrote the content: Ostby, Ostby Pasture, Gruvhald, and the Hanging Cells on two floors, the enemies, the cast kits, the lessons, the gear, the items, the shop, the inn, the scenes, and the night after the end. The owner approved three text batches (D-1354, D-1355, D-1361).

### The state of the build

- Local head `eb280ee` and later commits. Local checks pass: 4719 tests, format, det-lint, STE, content hash, atlas, identity, overworld, edges, and `make smoke`.
- The remote PR branch holds nothing yet. `origin/main` is `9a567d6`.

### What is in flight

- The first push, the PR, the map previews, and the Gitar pass. The screen baselines change with the battle line and the new start, and the CI artifact gives the new baselines.

### Traps and gotchas

- The first run starts in `map.village` now. `GameRun.StartFixture` keeps the fixture dungeon and the fixture kit for the smoke session and the screen fixtures.
- An exit to a map that is not the overworld arrives on the spawn point of that map. The town spawn sits by the chapel stair.
- The whole-run test needs 34 clean runs of 40, and it gets 34. A weaker party can tip it.
- The generator of the overworld rewrites the terrain, the zone grid, and the places of its things alone. The pasture entrance and the arrival markers are hand-written.

### The questions that block progress

None. OQ-255 and OQ-256 block no step of PR-17.

### The next concrete action

Read each frame of `make sheet`, push, open the PR with the Documents section, the three text batches, and the map previews, then run the Gitar poll.

## Session 401: 2026-09-28, Codex

Author: Codex
Session: review PR #99 (PR-53). Repository: the-thing-below. Branch: `review/pr-99`, tracking `origin/feat/pr-53-tile-edges`. Role: reviewer. Base: `f0db11584f575a90ac628f0d9aaa99115ac969c9`.

### What this session did, and why

- Reviewed effective head `eee30909a50681222d09553b82db4a1361bb827b`, the complete 62-path diff, the PR comments, the decisions, and the PR-53 exit tests.
- Verified both Gitar items. The repeated-piece finding is fixed and confirmed by Gitar. RG 3 alone failed because the review record was absent (D-964).
- Found no defect. All 4,583 tests passed in `make verify`. The screen-test artifact shows the overworld shore at each changed size.

### The state of the build

- CI on `eee30909a50681222d09553b82db4a1361bb827b` passed every check except review-gate RG 3. Required checks passed on macOS, Ubuntu, and Windows. Local `make verify` passed.
- The remote PR head before this metadata commit is `eee30909a50681222d09553b82db4a1361bb827b`.

### What is in flight

- The review record and this entry form one metadata commit (D-610).

### Traps and gotchas

- RG 3 failed before this review record existed. The other review-gate rules passed.
- Push with `git push origin HEAD:feat/pr-53-tile-edges`.
- Session 391 moves to `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Push the metadata commit, fetch, check the branch status and PR head, then confirm that review-gate passes.

## Session 400: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-53, round 2. Repository: the-thing-below. Branch: `feat/pr-53-tile-edges`. PR: #99. Role: author. Base: `f0db115`.

### What this session did, and why

- Read the Gitar pass of `7c9a40a`: approved with one finding and one CI claim.
- The finding had merit. A piece at two places of one rule, or a piece of two rules, loaded and then failed each load of the edge file, and Godot made one tile two times with a log line alone. `EdgeRule.Read` and `EdgeContent.Load` now refuse both, with the file and both places or both rules.
- Two tests fail on the old code and pass on the fix: `EdgeRuleTests.APieceAtTwoPlacesFailsWithTheFileAndBothPlaces` and `EdgeContentTests.APieceOfTwoRulesFailsWithBothRules`.
- The CI claim: RG 3 alone failed, because the review record does not exist before the review.
- Took the three `overworld` baselines from the screen-test artifact of run 36464212466 (D-733). The author read each frame: the lake shows its shore. No other capture changed.

### The state of the build

- Each CI job of `7c9a40a` passed on every leg but the screen-test, which failed on the three `overworld` frames alone, and the review-gate (RG 3).

### What is in flight

- The push of round 2, the Gitar pass, then `make codex-review PR=99`.

### Traps and gotchas

- The rules load in the order of their kind, so a piece of two rules fails on the rule of the later kind.

### The questions that block progress

None.

### The next concrete action

Reply on the Gitar thread with the commit, run the Gitar poll, then run `make codex-review PR=99` when every check of the head is green but the review-gate.

## Session 399: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-53, round 1. Repository: the-thing-below. Branch: `feat/pr-53-tile-edges`. PR: the one PR of PR-53, before GitHub gives a number. Role: author. Base: `f0db115`.

### What this session did, and why

- Asked the seven open points of PR-53 in two batches. The owner took each recommended option, and D-1321 to D-1327 record them.
- The model: 8 edge pieces for each kind with edges, 4 sides and 4 inner corners, drawn over the tiles of that kind (D-1321, D-1322). A neighbour outside the map joins (D-1324).
- Added the edge records to Core (`EdgeRule`, `EdgeFile`, `EdgeContent`), which the content set loads and no rule reads (D-501, D-517).
- Added the `edges` command and `make edges`. They write one edge file for each map, and `--check` compares.
- Drew placeholder pieces for the water and the gorge, and their edge rules. The bridge joins the water (D-1323, D-1325). This keeps D-1290, which the roadmap line on the art contradicted.
- Game draws 4 edge layers over the ground, and the map preview draws the same pieces.

### The state of the build

- `make verify` passed with 4,581 tests. The remote head is `f0db115` until the first push of this PR.
- The mutation check: an outside neighbour that does not join fails 4 tests of `EdgePickerTests`.

### What is in flight

- The first push, the Gitar pass, the new screen baselines from CI, then `make codex-review`.

### Traps and gotchas

- A change of a map needs `make edges`, and `make overworld` too for the overworld. The test of the committed edge files fails until the command runs.
- The `edges` command reads the map files and the rules alone, because a stale edge file fails the load of the content set.
- The screen baselines of `overworld` and `region-one` change with the shore and the lip. CI gives the new frames (D-733).

### The questions that block progress

None.

### The next concrete action

Push, attach the previews of the two overworld maps to the PR description, and run the Gitar poll of the `gitar-review` skill.

## Session 398: 2026-09-28, Codex

Author: Codex
Session: review PR #98 (PR-52). Repository: the-thing-below. Branch: `review/pr-98`, tracking `origin/feat/pr-52-map-preview`. Role: reviewer. Base: `2fc559a17cd5bc55c0a12ecf270cddd18f9a167e`.

### What this session did, and why

- Reviewed effective head `e9d5bb8a37a65d829c1d3fac44f07ae5dd734115`, the full diff, the PR comments, the roadmap exit tests, and the applicable contracts.
- Built the solution and ran 19 focused preview tests, format, and STE. Each passed.
- Verified Gitar's CI-analysis item against the failed `review-gate` log. RG 3 alone failed because this review record was absent. The record answers the item (D-964).
- Found no defect. The PR description contains the four fixture previews required by exit test 4.

### The state of the build

- CI on effective head `e9d5bb8a37a65d829c1d3fac44f07ae5dd734115` passed build, test, format, smoke, replay identity, and bots on all three platforms. STE, det-lint, screen-test, and night-gate passed.
- The remote head before this metadata commit is `e9d5bb8a37a65d829c1d3fac44f07ae5dd734115`.

### What is in flight

- The review record and this handoff entry form one metadata commit (D-610).

### Traps and gotchas

- The pre-record `review-gate` run failed RG 3 because no record existed yet. RG 4 and RG 5 skipped, and RG 6 to RG 8 passed.
- Push with `git push origin HEAD:feat/pr-52-map-preview`.
- Session 388 moved to the top of `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Verify the pushed metadata commit, the remote PR head, and the post-push `review-gate` result.

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

