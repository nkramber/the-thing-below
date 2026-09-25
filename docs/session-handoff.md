## Session 309: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 3. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Re-reviewed PR #81 at effective head `f66e314`.
- Verified the Steam Deck measurement and each author answer to the Gitar dashboard and CI analysis items.
- The CI analysis names three review-gate jobs. Each fails RG 4 because the review record says `Blocked`.
- The implementation checks pass, and the review record now gives `Ready for owner merge`.

### The state of the build

- Effective head `f66e314`. Metadata head `c18ed2f` has green implementation checks, STE, and Gitar. The review-gate job fails RG 4 because the prior record says `Blocked`.

### What is in flight

- This review record and handoff entry will be committed together and pushed to `fix/pr-104-boot-and-rules`.

### Traps and gotchas

- RG 4 correctly rejects the earlier `Blocked` verdict. The next gate run must read this updated record.

### The questions that block progress

None.

### The next concrete action

Commit and push the review metadata, then verify the remote head and review-gate result.

## Session 308: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 2. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Re-reviewed PR #81 at effective head `f66e314`.
- Verified the owner Steam Deck measurement in the author response and the prior Gitar dashboard answer.
- The Gitar CI analysis claim names review-gate jobs. The current log shows only RG 4 fails because the review verdict is `Blocked`.
- The review remains `Blocked` until the author answers this Gitar item (D-964).

### The state of the build

- Effective head `f66e314`. Metadata head `5297e6c` has green implementation checks, STE, and Gitar. `review-gate` fails RG 4 because the verdict is `Blocked`.

### What is in flight

- This review record and handoff entry are committed and pushed to `fix/pr-104-boot-and-rules`.
- The author must answer the Gitar CI analysis item before a repeat review.

### Traps and gotchas

- The Gitar CI analysis calls the RG 4 result a validation fault. The log shows the gate correctly rejects the current `Blocked` verdict.

### The questions that block progress

None. The author answer to the Gitar item is required evidence under D-964.

### The next concrete action

The author answers the Gitar CI analysis item, then requests a repeat review.

## Session 307: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #81 (PR-104), round 2. Repository: the-thing-below. Branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: author. Base: `4aad522`.

### What this session did, and why

- The screen test of `830f146` found three fight captures that moved: the enemies read the lessons of Marrek, so the fight ends on another tick (D-1101). The author read each frame, and `f66e314` takes them as new baselines (D-733, D-784).
- The owner asked for the Deck run of D-961. On the Steam Deck in Release, one enemy turn took a p95 of 128 us and 120 us in two runs, below the limit of 1000 us.
- Gitar approved `f66e314` with no thread. Each Gitar item has its answer on the PR.
- The review of the other provider found no defect and gave `Blocked` for the Deck run and a Gitar note. `docs/reviews/pr-81-response.md` answers both.

### The state of the build

- Every check of `f66e314` passed but `review-gate`, which waited for the record. The effective head is `f66e314`.

### What is in flight

- The repeat review through `make codex-review PR=81`.

### Traps and gotchas

- A rule change of the evaluator moves the fight captures, and the screen test then fails until new baselines land.
- The review commit dropped the title line of `docs/session-handoff-archive.md`, as on PR #80. This commit puts it back.

### The questions that block progress

None.

### The next concrete action

Read the outcome of the repeat review.

## Session 306: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 1. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Reviewed all 50 changed paths from merge base `4aad522` through effective head `f66e314`.
- Verified the changed battle captures against the CI artifact. No visual fault appeared.
- `make verify` passed locally with 3340 tests. The implementation CI checks passed at `f66e314`.
- The review record gives `Blocked`: D-961 requires an owner run of `evaluator-cost` on the Steam Deck before merge, and one Gitar dashboard finding has no author answer (D-964).

### The state of the build

- Effective head `f66e314`. CI run `36154860584` passed the implementation checks. Metadata run `36156272679` passed its applicable checks. The live `review-gate` passes RG 3, RG 5 to RG 8, and fails RG 4 because the verdict is `Blocked`.

### What is in flight

- The review record and this handoff entry are committed together and pushed to `fix/pr-104-boot-and-rules`.
- The owner Steam Deck measurement and the author answer to the Gitar dashboard finding remain pending.

### Traps and gotchas

- D-1099 accepts replacement of an older refused settings file. The author still needs to answer Gitar's matching dashboard item (D-964).

### The questions that block progress

None. The Steam Deck measurement and the Gitar answer are required evidence, not open design questions.

### The next concrete action

The author answers the Gitar item and the owner runs `evaluator-cost` on the Steam Deck. Then request a repeat review.
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
