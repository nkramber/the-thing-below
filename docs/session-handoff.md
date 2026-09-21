# Session handoff

## Session 191: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 5. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- CI run 35668891800 of `d31b896` passed each job except screen-test and review-gate. The screen-test job failed on 43 lit captures, because the wall faces and the figure shadows change each frame.
- The author read the map, walk, and battle frames of its `screen-captures` artifact. They match the local sheet, and this round commits them as the baseline (D-733).

### The state of the build

- `make verify` and `make smoke` pass. The remote head is this round.

### What is in flight

- Gitar on this head, and the repeat review of Codex. The review of `6095f70` is stale.
- The owner runs the Deck test of D-854.

### Traps and gotchas

- The review-gate check fails until the repeat review names the effective head.

### The questions that block progress

The Deck result of D-854.

### The next concrete action

Prove that the Gitar review of this head is current. Then tell the owner that PR #53 is ready for the repeat review.

## Session 190: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 4. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner read the lit map and asked for lit walls with no light behind a wall of one tile, for figures that cast shadows, and for 50% more light. D-852 to D-855 record the answers.
- Core gives the shape of each wall from the terrain: a south face of 24 pixels, other faces of 8, a strip of 2 in a wall of one tile, and a full tile at a corner.
- Each source is a pair of Godot lights, and each figure blocks light at its feet. The light row is 24, and each source counts two.
- The Deck test on `spike/deck-test` (commit `3f9fdda`) gains the stages `pairs-4` to `pairs-24` and `full-load-24`. A run on the Mac proved that each stage runs.
- The owner asked for CI to skip docs-only changes. D-856 and PR-93 hold it, and OQ-219 blocks it.

### The state of the build

- `make verify` passes with 1,862 tests, and `make smoke` passes. The remote head is this round.

### What is in flight

- The owner runs the Deck test. PR-56 merges only when `pairs-24` and `full-load-24` hold 60 frames per second under Mobile (D-854, G-14).
- The Codex review of `6095f70` gave `Ready for owner merge`, and this round moves the effective head, so the review repeats.
- The screen-test job fails on this push, because the walls and the shadows change each lit frame. The next round commits the new baseline.

### Traps and gotchas

- One Godot light cannot light a figure and keep the shadow of that figure off it. The pair and the light masks of `WorldLights` solve it.
- macOS has no `timeout` command.

### The questions that block progress

The Deck result of D-854.

### The next concrete action

Commit the baseline from the `screen-captures` artifact, then answer Gitar.

## Session 189: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-53, round 1. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: reviewer. Base: `e0cc485`.

### What this session did, and why

- Reviewed the complete PR-53 diff from merge base `e0cc485` through effective head `6095f70`.
- Verified the opposite-provider gate, light content and budget contracts, normal-map atlas wiring, map and battle lighting, documents, tests, CI results, and changed screen baselines.
- Added `docs/reviews/pr-53.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,856 non-smoke tests and all local gates. The remote implementation head is `6095f70`.

### What is in flight

- The review record and this handoff entry need commit and push.

### Traps and gotchas

- The review-gate check remains red until the metadata commit reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the review record and both handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 188: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 3. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- Gitar approved the code of `96298b7` with no thread. Its CI note named a real failure: `CaptureColorsTests` read each pixel of the lit map and walk baselines as a palette color.
- Light blends to any color (D-181), so a lit capture holds colors outside the palette. The test now reads the unlit world capture, `picture-1x.png`, alone. That capture still guards the sRGB conversion of PR-55 and the Nearest filter.
- The old test fails on the new baseline, which proves the change.

### The state of the build

- 1,856 tests pass locally against the new baseline. The remote head is this round.

### What is in flight

- The push wait of Gitar on this head, then the Codex review of `docs/reviews/pr-53.md`.

### Traps and gotchas

- Run the tests after a new baseline lands. `make verify` read the old baseline and passed.

### The questions that block progress

None.

### The next concrete action

Prove that the Gitar review of this head is current, then tell the owner that PR #53 is ready for the Codex review.

## Session 187: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 2. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The first CI run passed each build, smoke, det-lint, identity, and STE job on every leg. The screen-test job failed on 43 captures, because the light changes every map, walk, and battle frame.
- The author read the map, walk, and battle frames of the `screen-captures` artifact of run 35664815392. They match the local sheet, and this round commits them as the baseline (D-733).
- The UI and picture captures match the old baseline, because they take no scene light (D-210).

### The state of the build

- `make verify` and `make smoke` pass. The remote head is this round.

### What is in flight

- Gitar reviews this head, and the Codex review of `docs/reviews/pr-53.md` follows.

### Traps and gotchas

- The `review-gate` check fails until the review record lands.

### The questions that block progress

None.

### The next concrete action

Wait for Gitar with the push wait, prove that the review is current, and answer each finding.

## Session 186: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, the light and the shadows. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner answered OQ-94 to OQ-97 and added two requests: the party carries a torch, and the art target is the HD-2D look. D-842 to D-851 record each answer.
- PR-91 (the torch item, after PR-13) and PR-92 (three HD-2D passes, after PR-59) join the roadmaps. OQ-217 and OQ-218 block PR-91.
- Core reads the decor kinds, the decor files, the light setups, the carried light, and the effect budget. The load checks each file against the maps and the palette. It also checks the worst view of the Deck and the 15 lights of one canvas item.
- The simulation version is 12, and the identity file is new (G-17).
- Game draws the lit atlas pages, the full-tile wall shadows, the torches, the carried light, and the key light of a battle. The `torch` command of the console switches the carried light with no intent.

### The state of the build

- `make verify` passes with 1,891 tests. `make smoke` passes. `make sheet` shows the lit map, walk, and battle frames.
- The remote head is the first push of this branch.

### What is in flight

- The screen-test job fails on the first push, because every map, walk, and battle frame changes. The next round commits the baseline from the `screen-captures` artifact (D-733).
- Gitar and the Codex review wait for the PR.

### Traps and gotchas

- Do not send the output of `make smoke` to `artifacts/smoke.log`. The target writes that file and then `cat`s it, so the file grows with no end.
- `OccluderPolygon2D.CullMode` Clockwise keeps each wall dark. CounterClockwise lights the wall tile of each torch alone, as a flat block.
- The collection expression `[0, last]` in Core pulls in `System.Runtime.InteropServices`, and the Core reference test fails.
- zsh arrays count from 1.

### The questions that block progress

None for PR-56. OQ-217 records a clash for PR-91: D-566 puts no fog of war on a map, and D-848 makes the torch needed to see in the dark.

### The next concrete action

Download the `screen-captures` artifact of the first CI run, read each frame, and commit the new baseline. Then answer gitar.

## Session 185: 2026-09-21, Codex

Author: Codex
Session: review PR-48, normal maps. Repository: the-thing-below. Branch: `feat/pr-48-normal-maps`. PR: #52. Role: reviewer. Base: `9e9dc59`.

### What this session did, and why

- Reviewed the complete PR-52 diff from merge base `9e9dc59` through effective head `2f41276`.
- Verified the opposite-provider gate, normal-map generation, override validation, atlas integration, capture input fix, documents, and tests.
- Added `docs/reviews/pr-52.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,839 tests and all local gates.
- GitHub checks pass at `2f41276` except the review gate, which waits for this review record.

### What is in flight

- This review record and this handoff entry need commit and push.

### Traps and gotchas

- Gitar reports no code issue, but functional validation is disabled.
- The review-gate failure is expected until this record reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the review record and handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 184: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-48, round 1. Repository: the-thing-below. Branch: `feat/pr-48-normal-maps`. PR: #52. Role: author. Base: `9e9dc59`.

### What this session did, and why

- Built the normal maps of PR-48 (D-183, D-184, D-502). The `atlas` command writes `sprites/normal-map-<page>.png` beside each page of tiles, map sprites, battle sprites, and pieces, and `--check` compares them by pixel (F-19). Portraits and the UI take none (D-210).
- Asked the owner four questions and recorded the answers: the height of each palette color (D-838), the override grid (D-839), the rim of 4 pixels (D-840), and the form of the review sheet (D-841).
- Fixed the second concern that the owner approved in Session 182. `Boot` now checks the run before it reads the held steps, so a key in the capture window writes no InputMap error line. The capture session pushes one stray step key as its regression test.
- Raised the simulation version to 11, because the content reader refuses new shapes (G-17). The identity file changed with it (D-504).

### The state of the build

- `make verify` passes on this machine. The remote head is the commit of this entry on `feat/pr-48-normal-maps`.
- The capture of the old `Boot` code failed on "The InputMap action "step_north" doesn't exist". With the fix, `make sheet` passes with no error line.
- The branch captures match the captures of `main` pixel for pixel, 49 of 49. Six captures of this Mac differ from the committed baselines by one color level on `main` too, so this PR does not cause that difference.

### What is in flight

- The Gitar pass of this push, and the review of the other provider.
- The owner approval of the normal-map sheets in the PR description (exit test 5, G-25).

### Traps and gotchas

- The row of nine copies of a 64-pixel piece is wider than 1600 pixels. D-841 records the wrong count of the question.
- The drawing reader refuses a tile that is not 32 by 32, so a small test drawing goes on the `map_sprites` page.
- A picture or an atlas fixture of a lit page needs its `normal-map-` page, or the content set refuses the set.

### The questions that block progress

None. The owner approval of the sheets is exit test 5.

### The next concrete action

Follow the `gitar-review` skill for this push. Ask the owner to approve the normal-map sheets. Then tell the owner that PR #52 is ready for the other provider.

## Session 183: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-51, round 1. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: reviewer. Base: `8b10888`.

### What this session did, and why

- Reviewed the complete PR-51 diff from merge base `8b10888` through effective head `3859a74`.
- Verified the opposite-provider gate, the battle event flow, the command menu, the string table, the screen layout, and the tests.
- Added `docs/reviews/pr-51.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passed with 1781 tests and all local gates.
- GitHub checks pass for `3859a74` except the review gate, which waits for the review record.

### What is in flight

- This review record and this handoff entry need commit and push.

### Traps and gotchas

- Gitar reports no issues, but its functional validation is disabled. The review uses the local contract-specific tests and smoke output.

### The questions that block progress

None.

### The next concrete action

Commit the review record and handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 182: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-10, round 5. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: author. Base: `8b10888`.

### What this session did, and why

- The owner approved the text batch, with one change: a resisted hit reads "{target} takes {amount}. Barely a mark." (D-837).
- Round 4 head `5b0ab39` passed Gitar with no issue and every CI job except the review gate.

### The state of the build

- This round changes one string, a decision row, the design pass line, and this entry. No capture shows a resisted hit, so no baseline changes.
- 1781 tests pass on this machine. The review gate waits for `docs/reviews/pr-51.md`.

### What is in flight

- The CI run and the Gitar pass of the round-5 push, then the review of the other provider.

### Traps and gotchas

- `make sheet` can fail on "The InputMap action ... doesn't exist" when an input event reaches the capture window. The capture session builds no input map, and `Boot` reads the held steps before it checks the run. CI has no input.
- The owner approved a second concern for the next PR: the fix of the fault above. The next PR carries it beside its own concern. G-8 yields to this owner approval, and the fix needs no decision row. Name both concerns in the PR description.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill for the round-5 push. Then tell the owner that PR #51 is ready for the other provider.
