# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

## Session 107: 2026-09-18, Codex

Author: Codex
Session: review PR #27, PR-4, integer math, streams, state hash, and identity job. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. Role: reviewer. Base: `772468a`.

### What this session did, and why

- Reviewed the full 38-path diff from `772468a` to effective head `a963e4f` against section 7.11 and D-641 to D-645.
- Verified the malformed identity file correction from the trigger through the command and regression tests.
- Wrote `docs/reviews/pr-27.md` with verdict `Ready for owner merge` for `a963e4f`.

### The state of the build

- `main` and the merge base are `772468a`. The PR tip is `0cc4ddc`; its effective head is `a963e4f` (D-610).
- `make verify` passes: 290 tests, format, `det-lint`, `ste-check`, replay identity, and the Godot smoke session.
- Thirteen CI checks pass on tip `0cc4ddc`. `review-gate` reports only RG 3 because this review record is not yet on the branch.
- Gitar approves the current effective head. Its one finding is fixed, and its thread is resolved.

### What is in flight

The review record and this handoff entry are pushed as `7969d87`. The required `review-gate` result on the published record is pending.

### Traps and gotchas

- The code correction is `a963e4f`. The later commits change only handoff and review metadata, so they do not move the effective head.
- OQ-60, OQ-61, and OQ-62 close with D-641 to D-645.
- This PR changes code, so the `review-override` label does not apply (D-401).
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 108.

### The questions that block progress

None for PR #27. D-641 to D-645 answer the questions of section 7.11.

### The next concrete action

Commit and push the review record and this handoff entry. Check that `review-gate` passes.

## Session 106: 2026-09-18, Claude Code

Author: Claude Code
Session: the second round of PR-4, the answer to the Gitar pass. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. PR: #27. Role: author. Base: `772468a`.

### What this session did, and why

- The Gitar pass approved the head `1bb9125` with one finding, and the session read it as a claim.
- A run of the command against a malformed identity file reproduced the fault, so the finding has full merit.
- `IdentityFile.Read` throws `InvalidDataException`, which does not derive from `IOException`. The catch filter named `IOException` alone.
- A malformed line thus escaped as an unhandled exception with a stack trace, in place of the one-line fault report that the comment promised (T-2).
- The catch filter now names `InvalidDataException` too.
- Four new tests run the malformed lines and the absent file through the command path. The three malformed cases fail on the old filter (T-3).

### The state of the build

- `main` is `772468a`. The head of PR #27 is `3f8b803`, and the round before it was `1bb9125`.
- `make verify` passes: the build with 0 warnings, 290 tests, the format check, `det-lint`, `ste-check`, the identity check, and the smoke session.
- On `3f8b803`, 14 CI checks pass: the three build legs, the three smoke legs, the three `replay-identity` legs, `changed paths`, `ste-check`, `det-lint`, the coverage report, and the Gitar check.
- The three `replay-identity` legs give the same four hashes as this machine, which is exit test 5 of section 7.11.
- `review-gate` gives one fault, RG 3: the head holds no review record at `docs/reviews/pr-27.md`. The review of Codex clears it.
- The first `review-gate` run also gave RG 7, because the `docs/reviews/` row took no form of D-581. A correction of the PR description cleared it.

### What is in flight

The review of Codex. The Gitar pass approves the head `3f8b803`, and it gives no open finding.

- Automatic reviews stay paused on the Gitar trial, so the push wait of three minutes ended with no pass. The comment `Gitar review` at 14:36:38Z started a manual pass.
- Gitar replied `On it` at 14:37:02Z, and it then replaced the dashboard comment. The new id is `5731578321`, with the edit time 14:37:25Z.
- The edit time is later than the reply time, so the pass covers the head (D-603).
- The Gitar check on `3f8b803` completed with success in 44 seconds.
- The dashboard reads `Approved`, with 1 closed finding and none open. The one review thread is resolved.

### Traps and gotchas

- `InvalidDataException` derives from `SystemException`, and `FileNotFoundException` derives from `IOException`. A catch filter of file faults must name the first one itself.
- The vectors of PCG32 and xxHash64 are the published values of each reference implementation. A second implementation from the specification gave the same values on 2026-09-18.
- D-645 puts the SHA-256 of D-644 in PR-5, beside the content hash that calls it.
- A change of a hash in the identity file also needs a higher `SimulationVersion.Current` (G-17, D-504).
- This PR changes code, so the label of D-401 does not apply.
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 107.

### The questions that block progress

None. D-641 to D-645 answered every question of this PR.

### The next concrete action

Hand PR #27 to Codex for the review of T-4. This PR changes code, so the label of D-401 does not apply.

## Session 105: 2026-09-18, Claude Code

Author: Claude Code
Session: the first round of PR-4. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. PR: the one PR of PR-4, which GitHub numbers at the push. Role: author. Base: `772468a`.

### What this session did, and why

- The session asked OQ-60, OQ-61, and OQ-62 before any code, and D-641 to D-645 hold the answers.
- Core gains its first rules: `BasisPoints`, `Pcg32`, `RandomStreams`, `XxHash64`, `StateHasher`, and `IdentitySet`.
- `RunContext`, `SimulationException`, and `CoreAssert` carry the context of every error (T-2, G-18).
- `SimulationVersion.Current` starts at 1, and each later Core change raises it (G-17).
- The `replay-identity` command of Tools compares each state hash with the committed identity file, and `--write` writes that file again.
- The `replay-identity` job runs on the three CI legs, and `make verify` gains the same check.
- Tools now references Core, because the command computes the hashes.

### The state of the build

- `main` is `772468a`, and this branch starts from it.
- `make verify` passes: the build with 0 warnings, 286 tests, the format check, `det-lint`, `ste-check`, the identity check, and the smoke session.
- The Release build gives the same four hashes, and it proves that the assertion helper still throws (exit test 8).
- The identity file is `TheThingBelow.Tests/identity/replay-identity.txt`, with four runs.
- Two commits hold the work: the code, then the documents.

### What is in flight

The push, the Gitar pass, and the review of Codex. This PR changes code, so the label of D-401 does not apply.

### Traps and gotchas

- The vectors of PCG32 and xxHash64 are the published values of each reference implementation. A second implementation from the specification gave the same values on 2026-09-18.
- D-645 puts the SHA-256 of D-644 in PR-5, beside the content hash that calls it. PR-4 ships xxHash64 alone.
- A change of a hash in the identity file also needs a higher `SimulationVersion.Current` (G-17, D-504).
- `make verify` runs `build` before `identity`, because the command reads the build output of Tools.
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 106.

### The questions that block progress

None. D-641 to D-645 answered every question of this PR.

### The next concrete action

Push the branch, open the PR, and answer the Gitar pass. Then hand the PR to Codex for the review of T-4.

## Session 104: 2026-09-18, Codex

Author: Codex
Session: review PR #26, PR-82, the Mobile renderer. Repository: the-thing-below. Branch: `feat/pr-82-mobile-renderer`. Role: reviewer. Base: `e54810a`.

### What this session did, and why

- Reviewed the renderer setting and its regression tests against D-616 and the exit tests of section 7.3.
- Wrote `docs/reviews/pr-26.md` with verdict `Ready for owner merge` for effective head `6b7bde6`.
- Confirmed the provider gate, the current Gitar approval, and that no comment thread needs an answer.

### The state of the build

- `main` and the merge base are `e54810a`. The PR tip is `9de398a`; its effective head is `6b7bde6` (D-610).
- Local build, 189 tests, format, `det-lint`, `ste-check`, Godot editor build, and smoke session pass.
- All required CI jobs pass on `9de398a`. `review-gate` fails because the review record is not yet on the branch.
- Gitar approves `6b7bde6`. Its dashboard comment was edited after the implementation push, and no review threads exist.

### What is in flight

The review record and handoff entry need a commit and push. Then verify `review-gate` on the metadata tip.

### Traps and gotchas

- The pull request branch name says PR-82, while GitHub numbers it PR #26. Use the GitHub number in review records (D-17).
- Commits `2419ce4` and `9de398a` change only handoff metadata, so the effective head remains `6b7bde6` (D-610).
- Gitar functional validation is not enabled.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 105.

### The questions that block progress

None. D-616 selects Mobile, and section 7.3 names the renderer work.

### The next concrete action

Commit and push the review record and this handoff entry. Check that `review-gate` passes.

## Session 103: 2026-09-18, Claude Code

Author: Claude Code
Session: the first round of PR-82. Repository: the-thing-below. Branch: `feat/pr-82-mobile-renderer`. PR: the one PR of PR-82, which GitHub numbers at the push. Role: author. Base: `e54810a`.

### What this session did, and why

- D-616 picked the Mobile renderer from the Deck test, and PR-82 writes it into the Game project.
- `TheThingBelow.Game/project.godot` now sets `renderer/rendering_method="mobile"`.
- The feature tag list now names `Mobile` in place of `Forward Plus`, because the Godot editor reads that list.
- The header comment names the Deck test and its result, in place of the provisional text of D-599.
- `GameProjectRendererTests` reads the committed project file and locks both lines.
- The test guards the setting, because the Godot editor writes this file and can write the default of Godot into it again.

### The state of the build

- `main` is `e54810a`, and this branch starts from it.
- `make verify` passes: the build with 0 warnings, 189 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The smoke session prints `smoke: the renderer is mobile`, which is exit test 2 of section 7.3 of the phase file.
- The two new tests fail on the setting of PR-1, which is the regression check of T-3.
- The Godot editor build ran on the new setting, and it wrote no change into the project file.
- PR #26 holds this work, and its head is `2419ce4`.
- The 12 CI checks pass on `2419ce4`: the three build legs, the three smoke legs, `changed paths`, `ste-check`, `det-lint`, the coverage report, and the Gitar check.
- `review-gate` gives one fault, RG 3: the head holds no review record at `docs/reviews/pr-26.md`. The review of Codex clears it.

### What is in flight

The review of Codex. The Gitar pass approves the head `2419ce4`, and it gives no finding.

- The push of `2419ce4` was at 07:47:30Z, and the dashboard comment `5726936964` has the edit time 07:49:12Z.
- The edit time is later than the push time, so the pass covers the head (D-603).
- The Gitar check passed in 1 minute and 9 seconds, and the summary names this diff.
- The thread list of the pull request is empty, so no comment waits for an answer.
- The dashboard carries the pause note of the Gitar trial beside the approval.

### Traps and gotchas

- The setting `renderer/rendering_method.mobile` stays as it is. It serves Android and iOS, which D-481 excludes.
- HDR 2D works under Mobile, so the glow of D-188 stays live (D-188, D-616).
- The screen tests of CI keep the Compatibility renderer, whatever this PR sets (D-172).
- This PR changes code, so the label of D-401 does not apply, and Codex reviews it.
- The first `review-gate` run gave RG 7 too, because the `docs/reviews/` line of the PR took no form of D-581. A correction of the PR description cleared it.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 104.

### The questions that block progress

None. The Deck test answered D-160, and D-616 holds the pick.

### The next concrete action

Hand PR #26 to Codex for the review of T-4. This PR changes code, so the label of D-401 does not apply.

## Session 102: 2026-09-18, Codex

Author: Codex
Session: repeat review PR #25 at effective head `ac34b5f`. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: reviewer. Base: `b3ec2b4`.

### What this session did, and why

- Rechecked P2-1 against its original trigger and the ordered-list sweep.
- Confirmed both Phase 1 lists now run from 1 to 23 with no duplicate item number.
- Confirmed the stale section 7.19 reference now names 7.20, and the round ends at D-640.
- Read both Gitar claims, both author replies, and Gitar's confirmation. Both threads are resolved.
- Updated `docs/reviews/pr-25.md` to close P2-1 for effective head `ac34b5f`.

### The state of the build

- `main` and the merge base are `b3ec2b4`. The PR tip is `118551e`; its effective head is `ac34b5f` (D-610).
- `make verify` passes at the local tip: build with 0 warnings, 187 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub reports `changed paths` and `ste-check` as passing. Docs-only build, test, format, coverage, lint, and smoke jobs skip (D-600).
- Gitar approves `ac34b5f`, with 2 closed findings and none open. Its current dashboard follows the `On it` reply.
- The review record and handoff were published as `301b507`. `review-gate`, `changed paths`, and `ste-check` pass on that head.
- Docs-only build, test, format, coverage, det-lint, and smoke jobs skip under D-600. The `review-gate` check is green.

### What is in flight

The verdict and handoff are published. The owner can merge PR #25.

### Traps and gotchas

- The new effective head changes two roadmap paths. The later Gitar and handoff commits change only metadata (D-610).
- The ordered-list sweep checks a class of defects that `ste-check` does not read.
- `deck-test/` and `screen-scale-probe/` remain untracked and outside the PR.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 103.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639.

### The next concrete action

The review applies to effective head `ac34b5f`; the owner can merge PR #25.

## Session 101: 2026-09-18, Claude Code

Author: Claude Code
Session: the answer to the review of PR #25, in the same session as Session 99 (D-582). Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: author. Base: `b3ec2b4`.

### What this session did, and why

- The review of `83c17f7` gives `Changes required` with one finding, P2-1.
- P2-1 has full merit. The PR-86 entry added a line to the Phase 1 sequence and left each later number as it was, so two steps held the number 11.
- A sweep of every ordered list found the same defect in the Phase 1 list of `docs/design.md`, which the review did not name. Two steps held the number 10 there.
- The same sweep found two stale references of the insert: the gate line named section 7.19, and section 7.8 named the round as D-626 to D-638.
- `docs/reviews/pr-25-response.md` holds each disposition, the regression check, and the evidence.
- This session published the review record and the Session 100 entry, because the review session had a read-only `.git` directory (D-602).

### The state of the build

- `main` and the merge base are `b3ec2b4`. The head before this round was `9c13790`.
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The review records `make verify` as a failure at build after 5:00. That result did not reproduce here, and Session 98 recorded the same stop on a repeat review.
- The ordered-list sweep now prints no duplicate item number in any list.

### What is in flight

The repeat review of Codex at the effective head `ac34b5f`. This round changed two roadmap paths, so it moved the effective head (D-610).

The Gitar pass approves that head, with 2 closed findings and none open.

- No automatic review started on the new head, because the trial of Gitar paused them. The Gitar check was absent, and the dashboard kept the edit time of the head before it.
- The push was at 07:16:37Z, the request at 07:20:10Z, and the reply `On it` at 07:20:33Z.
- Gitar replaced the dashboard comment, and the new id `5726614038` has the edit time 07:20:54Z.
- Each time is later than the one before it, so the pass covers the effective head (D-603).
- The summary of the pass repeats the words of the pass before it, which a pass with no new finding can do. The three times prove that it is current, so this round asked for no second review.
- `review-gate` gives two faults, RG 4 and RG 5. The record holds the verdict `Changes required` for the head before this round. The repeat review clears both.

### Traps and gotchas

- A renumber of one ordered list can break a sibling list and a self-reference. The PR-86 insert broke three places, and the review named one. A sweep of every ordered list catches the class, and `ste-check` reads no item number.
- Gate 1 moved from section 7.19 to section 7.20 when the PR-86 entry took 7.9. A citation of a section number of a phase file needs a check after any insert.
- The review could not export the PR comments, so it did not read the two inline replies to the Gitar findings. Both threads are resolved.
- `deck-test/` and `screen-scale-probe/` stay untracked, and the two probe exports stay out of git.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 102.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639.

### The next concrete action

Push the round, answer the Gitar pass on the new head, and ask Codex for the repeat review.

## Session 100: 2026-09-18, Codex

Author: Codex
Session: review PR #25, roadmap PR-86, the screen scale answers. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: reviewer. Base: `b3ec2b4`.

### What this session did, and why

- Confirmed that Claude Code authored the PR, so Codex meets the other-provider gate (T-4, D-17).
- Reviewed the 15-path documentation diff, the PR description, the screen scale decisions, and the PR-86 roadmap entry.
- Found P2-1: the Phase 1 sequence numbers the owner step and PR-46 as item 11.
- Wrote `docs/reviews/pr-25.md` with verdict `Changes required` for effective head `83c17f7`.

### The state of the build

- `main` and the merge base are `b3ec2b4`. The PR tip is `9c13790`; its effective head is `83c17f7` (D-610).
- GitHub reports `changed paths` and `ste-check` as passing. The build, tests, format, coverage, det-lint, and smoke checks skip for this docs-only PR (D-600).
- `review-gate` fails because the review record is not yet on the branch.
- `make verify` failed at build after 5:00 with 0 warnings, 0 errors, and no diagnostics. The author reports a successful run in Session 99.
- `ste-check` passes with 0 findings after the review record and handoff edits.
- The Gitar dashboard approves `83c17f7` with two closed findings and none open. Its inline replies were not available in this session.

### What is in flight

The author needs to correct P2-1. Git metadata is read-only in this environment, so the review record and this entry need publication from a writable session.

### Traps and gotchas

- The metadata tip `9c13790` changes only the handoff. The effective head remains `83c17f7` (D-610).
- `git fetch` could not write `.git/FETCH_HEAD`. The GitHub API also failed during the required complete comment export.
- `deck-test/` and `screen-scale-probe/` remain untracked and outside the PR.
- Session 90 moved to the archive to keep ten entries in this file (D-18).
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 101.

### The questions that block progress

None. The open item is P2-1, which requires a correction to the sequence.

### The next concrete action

Correct the Phase 1 sequence, publish the review record and this entry, then ask Codex to review the corrected effective head.

## Session 99: 2026-09-18, Claude Code

Author: Claude Code
Session: the answers of the screen scale probe. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: author. Base: `b3ec2b4`.

### What this session did, and why

- Read `screen-scale-probe/handover.md` on the spike branch, which holds the owner answers of the probe.
- Found that answer 2 and answer 3 of OQ-183 disagree on a 1920 by 1080 screen, which D-568 makes a screen that must look good.
- Found that the probe computed the fit with integer division, so it could never draw the 1.5x fit of that screen.
- Patched the spike with two fit modes, and proved both in a window of 1920 by 1080 on the Mac (D-638).
- The owner then ran the new Windows build on a 27-inch 1080p screen and picked the UI at 2x.
- Wrote D-626 to D-640, F-68 to F-77, G-28, the M-8 table of four screens, and the close of OQ-183.
- Applied the answers to nine live documents, four skills, and the PR-86 roadmap entry.

### The state of the build

- `main` is `b3ec2b4`. PR #25 is open, and the effective head is `83c17f7`.
- The spike branch `spike/screen-scale-probe` is at `f314243`, and it never merges (D-597, D-621).
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The `ste-check`, `changed paths`, and `Gitar` checks pass. The docs-only jobs skip under D-600.
- `review-gate` gives one fault, RG 3, because the head holds no record at `docs/reviews/pr-25.md`. The Codex review clears it.

### What is in flight

The Codex review. This PR changes decision rows, so the `review-override` label does not apply (D-401, D-609).

The Gitar pass approves the effective head `83c17f7`, with 2 closed findings and none open.

- The push of `83c17f7` was at 06:46:44Z, and the Gitar check on it started at 06:47:16Z.
- Gitar replaced the dashboard comment, and the new id `5726292453` has the edit time 06:47:54Z.
- Each time is later than the one before it, so the pass covers the effective head (D-603).
- Both review threads are resolved, and Gitar resolved each one itself.

### Traps and gotchas

- Two claims of the spike handover did not survive the check, and the record now holds the refutation. D-508 refutes the claim that the export of the game needs an include filter, because Game embeds content and each font in its own assembly. The 1080p run moved the UI boundary from a fit of 1x to a fit of 2x.
- A 27-inch 1080p screen and a 27-inch 4K screen give one line of body text the same apparent size, and the owner picked two different UI values. The count of device pixels sets the value, not the apparent size (F-77). A later session must not read the arcminute numbers of M-8 as the rule.
- The PR-86 entry moved 11 sections of `docs/roadmaps/phase-1-foundations.md` by one. No document outside this PR cites a section number of that file above 7.8.
- The option list of OQ-183 still names the frame at 40 by 22.5 tiles. That list records the options of 2026-09-17, and D-633 closed the question.
- The row of D-599 keeps the bare name `project.godot`, because a decision row is a record of one day (D-637).
- `deck-test/` and `screen-scale-probe/` stay untracked on this branch. The two probe exports are above 150 MB and stay out of git.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 100.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639, which unblocks PR-7, PR-34, and PR-61.

### The next concrete action

Push the branch, open the PR, answer the Gitar pass, and hand the PR to Codex for the review.

## Session 98: 2026-09-18, Codex

Author: Codex
Session: repeat review of PR #24 at effective head `5f5129a`. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. Role: reviewer. Base: `2874b70`.

### What this session did, and why

- Rechecked P2-1 against its original trigger and regression check.
- Verified the author revised D-139, D-161, D-172, D-214, D-520, and D-526.
- Verified the effects roadmap no longer schedules PR-37 or describes the CRT pass.
- Set P2-1 and P2-2 to fixed in the review record. The verdict is ready for owner merge.

### The state of the build

- `main` and the merge base are `2874b70`. Effective head: `5f5129a`. The review record was published at `6826312`.
- The author reports `make verify` passed with 187 tests. The local repeat-review build produced no output for 7:41 and was stopped.
- The changed-path and `ste-check` jobs pass. Docs-only build, test, format, coverage, lint, and smoke jobs skip under D-600.
- The current Gitar pass approves `5f5129a`, with one closed finding and no open finding. The live inline-comment query returned zero.
- `deck-test/` remains untracked and untouched.

### What is in flight

The review record and this handoff are published. CI and `review-gate` pass on `6826312`.

### Traps and gotchas

- The metadata commit does not move the effective head (D-610).
- D-618 retires the CRT pass and PR-37. D-619 keeps two transition names only.
- A later metadata commit must retain the current verdict and head for `review-gate` (D-610).
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 99.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34.

### The next concrete action

The owner can merge PR #24 after reading the current review verdict.
