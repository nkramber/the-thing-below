## Session 305: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-104, round 1. Repository: the-thing-below. Branch: `fix/pr-104-boot-and-rules`. PR: the one PR intent of this branch, before GitHub gives a number. Role: author. Base: `4aad522`.

### What this session did, and why

- The owner answered each question of the findings in this PR (D-1098 to D-1108). G-8 does not bind this PR (D-1098).
- P2-2: a settings file that fails to load goes aside as settings.refused.json, and the start runs on the defaults with a message. A crash before the screen shows its message on a frame of the default display.
- P2-5: the reply of the evaluator scores the best legal strike, lesson strikes included. The cost fixture holds lessons.
- P3-1, P3-18, P3-19, P3-20: the enemy phase has a bound, six states fail at load or resume, a tile trigger plays before a step into an enemy, and a step into a marking enemy takes the side of the beat.
- P3-7 and P3-34: the crash message names its folder, and the absorb lines take the words of the owner.
- P3-27: G-17 states that a reader of Core counts.
- The simulation version rises to 28, and the identity file changes with it.

### The state of the build

- `make verify` passed on this Mac: 3340 tests, format, lint, the STE check, identity, content, atlas, and smoke.
- Each new regression test failed on `4aad522`, and the two lock tests of P3-1 hung there.

### What is in flight

- The first push, then the Gitar pass, the Deck run of `evaluator-cost` (D-961), and the review of the other provider.

### Traps and gotchas

- A collection expression of a `List` in Core reads `CollectionsMarshal`, and the reference test of G-1 fails. Use a collection initializer.
- The settings message names the whole file for a value outside its range, because the range check keeps no field.

### The questions that block progress

None for this PR. P2-3, P3-9, and P3-35 stay open in the report of the owner.

### The next concrete action

Push, then run the Gitar poll of the `gitar-review` skill.

## Session 304: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 9. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- Each CI check of `3559ee4` passed but `review-gate`, and Gitar approved it with every thread closed. The Gitar thread of round 8 has its reply with the fix commit.
- The repeat review gives `Ready for owner merge` for the effective head `3559ee4` in `docs/reviews/pr-80.md`, with no open finding. Every check of the record commit `670d8fa` passed, `review-gate` included.
- The report of the owner marks each of the fifteen findings of this PR as `COMPLETE - PR #80`.
- The owner confirmed the merge after the summary in four sections (D-933, D-942).
- The record commit `670d8fa` dropped the title line of `docs/session-handoff-archive.md`. This commit puts it back.

### The state of the build

- The effective head is `3559ee4`, and the review approves it. This entry is a commit of the metadata set, so the approval stands (D-610).
- The remote head holds this entry.

### What is in flight

- The Gitar pass of this commit, and then the gated auto-merge. The PR waits for the auto-merge.

### Traps and gotchas

- `make sheet` stops at its last step on `main` too: 108 captures make a sheet taller than 65535 pixels. The capture session itself writes every frame.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

After the merge, write the transitional prompt of step 6 of the `one-pr-one-session` skill.

## Session 303: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #80 (PR-103), round 2. Repository: the-thing-below. Local branch: `review/pr-80`; PR branch: `fix/pr-103-input-and-grace`. PR: #80. Role: reviewer. Base: `5c1db06`.

### What this session did, and why

- Re-reviewed effective head `3559ee4` and verified P2-1 against its original reproducer and exact-cap boundary. Both `FileTextTests` pass.
- Gitar's buffer-capacity suggestion is fixed in `3559ee4`. The review record gives `Ready for owner merge` for the effective head (T-4, D-17, D-929, D-964).

### The state of the build

- CI run `36107505237` passed every implementation check at `3559ee4`.
- CI run `36107505237` passed each implementation check at effective head `3559ee4`. A metadata run passed every applicable gate at the first publication. This final metadata update changes no implementation path.

### What is in flight

- The review record and handoff entry are committed together and pushed to `fix/pr-103-input-and-grace`.
- The fresh metadata checks for the final publication are in flight.

### Traps and gotchas

- The capped read uses one extra byte to detect growth past the limit. The capacity hint does not replace that counted check (F-129).
- A failing `review-gate` before publication reads the previous review record, not the verdict in this commit (D-15, D-610).

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Fetch the branch and read the fresh metadata checks. Then the author can continue with `docs/runbooks/merge.md`.

## Session 302: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 8. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- Each CI check of `8175992` passed but `review-gate`. Its faults are RG 4 and RG 5: the review record still names `29615cb` with `Changes required`, and the repeat review clears both.
- Gitar approved `8175992` with one suggestion: the buffer of `FileText.ReadStream` had no set size, so it doubled as it filled, and a file near its cap took up to twice its size in memory. The suggestion has merit. The buffer now takes the length of the stream, and the counted loop still holds the cap.

### The state of the build

- The tests of Storage and the format check pass on this machine.
- The remote head holds this entry.

### What is in flight

- The CI and the Gitar pass of the round 8 push. Then `make codex-review PR=80` for the repeat review.

### Traps and gotchas

- The reviewer adds its own handoff entry, so the author reads the top session number again before it writes an entry.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check and the CI of the round 8 push. When each check but `review-gate` passes, run `make codex-review PR=80` in the background.

## Session 301: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 7. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- Each CI check of `29615cb` passed but `review-gate`, which faulted on RG 3 alone, and Gitar approved it with no open finding.
- `make codex-review PR=80` gave `Changes required` for `29615cb`, with one finding, P2-1: a file that another program grew between the length check and the read of `FileText.Read` passed the cap.
- The finding has full merit. `FileText` now opens one stream and reads it in counted chunks, and it stops at the first byte past the cap. `FileTextTests` holds the regression and the boundary. `docs/reviews/pr-80-response.md` holds the answer.

### The state of the build

- The tests of Storage pass on this machine, with the new `FileTextTests`.
- The remote head holds this entry.

### What is in flight

- The CI and the Gitar pass of the round 7 push. Then `make codex-review PR=80` for the repeat review.

### Traps and gotchas

- A check of a length before a second open of the file leaves a gap for another program. Read the file through the one open stream.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check and the CI of the round 7 push. When each check but `review-gate` passes, run `make codex-review PR=80` in the background.

## Session 300: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #80, round 1. Repository: the-thing-below. Local branch: `review/pr-80`; PR branch: `fix/pr-103-input-and-grace`. PR: #80. Role: reviewer. Base: `5c1db06`.

### What this session did, and why

- Reviewed effective head `29615cb` against PR-103 and its exit tests. Finding P2-1 shows that concurrent file growth bypasses the read cap.
- Verified that the prior Gitar cleanup finding is fixed. The current Gitar check passes, and the old RG 3 claim waits for this record.
- `make verify` passed locally with 3307 tests. CI passed each implementation check at `29615cb`, including screen-test and Gitar.

### The state of the build

- Base and merge base `5c1db06`; effective head `29615cb`.
- The remote branch head before this metadata commit is `29615cb`. A fresh review-gate result follows publication.

### What is in flight

- This review record and handoff are committed together and pushed to `fix/pr-103-input-and-grace`.

### Traps and gotchas

- `FileText.Read` checks `FileInfo.Length` before `File.ReadAllBytes`. A file can grow between these operations.
- The review verdict is Changes required. The author must correct P2-1 and request a clean re-review.

### The questions that block progress

None for this PR.

### The next concrete action

Publish the review metadata. The author then fixes P2-1 and runs a new Codex review.

## Session 299: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 6. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- Every CI check of `401c36d` passed but `screen-test` and `review-gate`. The `review-gate` fault is RG 3 alone: the review record waits for the review of the other provider.
- `screen-test` found 101 of 108 captures changed. The author read a frame of each group against its baseline (D-784):
  - The map, walk, still, scroll, pit, menu, notice, settings, and transition frames draw the map. Each one loses the halo of the wall torch (D-1097), and the carried light changes (D-1091).
  - The battle frames move their backdrop drift and their effects a few pixels, because the fight counts the world tick now (D-1083), and a menu of the capture walk holds that tick back.
- The captures of CI run 36104175330 are the new baseline (D-733). The `screens` command of Tools gives a match for each of the 108 captures.
- Gitar approved `401c36d` with no open finding.

### The state of the build

- Each CI check of `401c36d` but `screen-test` and `review-gate` passed. This round changes the baseline alone.
- The remote head holds this entry.

### What is in flight

- The CI and the Gitar pass of the round 6 push. Then `make codex-review PR=80`.

### Traps and gotchas

- `gh pr checks --watch` ends at the first failed check, so a failed `review-gate` ends the watch early. Poll until no check is pending.
- No capture shows the pause of a fight or the line of an item. The smoke session reads both.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check and the CI of the round 6 push. When each check but `review-gate` passes, run `make codex-review PR=80` in the background.

## Session 298: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 5. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- The owner asked to remove the glow of the wall torches for a playtest, and the session set its strength to 0 in the working tree alone. After the playtest, the owner chose no glow (D-1097).
- The wall torch now holds a glow of strength 0, so it draws no halo (D-912). The halo code, the size of 168 pixels, and the curve of D-1095 stay for a later fire that glows. D-1097 supersedes D-1096 and revises D-1075 and D-1092 in part.
- `NeitherTheWallTorchNorTheCarriedTorchGlows` replaces the test that the wall torch glows, and `TheReaderTakesAHaloOfSixTilesAtMost` keeps the reader bound of D-1092.
- Gitar approved `a8e9cdb` with no open finding. Its CI claim names RG 3 alone: the review record waits for the review of the other provider.

### The state of the build

- `make build`, `make test` (3307), `make format`, `make content`, `make ste-check`, and `make smoke` pass on this machine.
- The remote head holds this entry.

### What is in flight

- The Gitar pass and the CI of the round 5 push.
- The `screen-test` job fails on the map frames of the torch, and on the item line. The author reads each changed frame of the CI artifact and commits the new baseline (D-733). Then `make codex-review PR=80`.

### Traps and gotchas

- The citation of a superseded decision names the decision that superseded it, or ste-check fails on REF 3.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check of the round 5 push, and answer each Gitar item. Commit the new screen baseline from the CI artifact, and then run `make codex-review PR=80` when each CI check but `review-gate` passes.

## Session 297: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 4. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- The owner sent a screenshot of a wall torch and asked for a glow 30% softer (D-1096). The halo of a wall torch takes a strength of 2678 in place of 3825. The size and the curve of D-1095 stand.
- Round 3 pushed `1c0c3c2`. The Gitar thread of round 2 has its reply with the fix commit, and it is resolved. The CI claim of round 2 has its answer.

### The state of the build

- The light tests, the content hash, and the STE check pass on this machine. The rest of the checks of round 3 ran on `1c0c3c2`, and this round changes one content value and its test.
- The remote head holds this entry.

### What is in flight

- The Gitar pass and the CI of the round 4 push.
- The `screen-test` job fails on the map frames of the torch and the halo, and on the item line. The author reads each changed frame of the CI artifact and commits the new baseline (D-733). Then `make codex-review PR=80`.
- The playtest of the owner of the carried light and the halo (D-1091, D-1095, D-1096).

### Traps and gotchas

- The halo adds linear light, so a change of its strength reads larger on a dark wall than on the floor.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check of the round 4 push, and answer each Gitar item. Commit the new screen baseline from the CI artifact, and then run `make codex-review PR=80` when each CI check but `review-gate` passes.

## Session 296: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #80 (PR-103), round 3. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: #80. Role: author. Base: `5c1db06`.

### What this session did, and why

- Gitar approved `e804ab3` with one finding: a failed removal of an old crash file also skipped the removal of the temporary file of a torn write. The fix gives each cleanup its own try block, and `ARemovalThatFailsStillRemovesTheTemporaryFileOfATornWrite` fails on `e804ab3`. Its CI claim names the `review-gate` fault, which waits for the review record.
- The owner asked for a gentler edge of the wall torch halo (D-1095). The halo is one less the square of the distance, to the power 4. It meets 0 at its edge with no slope, and its texture holds half floats. The curve of D-1092 stopped at 1% of its middle, and the sRGB curve of the screen showed that stop as a faint ring.
- The owner then asked for five more findings. D-1090 now holds fifteen. The session chose five that need no owner answer, and each one reproduced at `5c1db06`:
  - P3-12: the safe write reads its temporary file back with the reader of its kind before the rename, and a refused rename tries five times (F-126).
  - P3-14: the record reader checks both versions of line 1 before line 2, the crash error carries the crash line, and the record format rises to 3 (F-127).
  - P3-16: the fill of a string refuses a value with no place (F-128).
  - P3-21: one read of Storage takes a size cap for each kind of file and a strict UTF-8 decode (F-129).
  - P3-38: the start view of a fight reads the party from before the tick that started it (F-130).
- Each new test fails on the old code: a mutation back to it, or the code of the round before.

### The state of the build

- The checks of this round ran on this machine before the push. The PR description gives the results.
- The identity file changes in six runs, because the record format rose to 3.
- The remote head holds this entry.

### What is in flight

- The Gitar pass and the CI of the round 3 push.
- The `screen-test` job fails on the map frames of the torch and the halo, and on the item line. The author reads each changed frame of the CI artifact and commits the new baseline (D-733). Then `make codex-review PR=80`.
- The playtest of the owner of the carried light and the halo (D-1091, D-1095).

### Traps and gotchas

- A resumed snapshot with an encounter of the enemy side is the short way to an ambush in a test (`BattleTurnsTests.Encountered`).
- `Label.Text` read in Game is a player string to det-lint (DL 8). The smoke check of D-1093 reads the string id instead.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check of the round 3 push, and answer each Gitar item. Commit the new screen baseline from the CI artifact, and then run `make codex-review PR=80` when each CI check but `review-gate` passes.
