# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

## Session 97: 2026-09-18, Claude Code

Author: Claude Code
Session: the answer to the review of PR #24, in the same session as Session 93 (D-582). Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. PR: #24. Role: author. Base: `2874b70`.

### What this session did, and why

- The review of `7426dc9` gives `Changes required` with two findings. P2-2 was fixed in the PR description by the reviewer.
- P2-1 has full merit: D-618 retired the CRT, and four places still held a live contract for it.
- The trigger reproduces in D-172, in D-214, and in two lines of section 7.2 of `docs/roadmaps/area-effects.md`.
- A sweep of `docs/decisions.md` for the same cause found four more rows: D-139, D-161, D-520, and D-526.
- The first sweep of this PR read every live document except that register, which is why those rows stayed.
- Six rows now carry the effect of D-618, and the frame section names no retired PR and no old-monitor pass.
- `docs/reviews/pr-24-response.md` holds each disposition, the regression check, and the evidence.

### The state of the build

- `main` is `2874b70`, and the head before this round was `3c728cc`.
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The review records `make verify` as inconclusive with `Build FAILED`. That result did not reproduce here.

### What is in flight

The repeat review of Codex at the effective head `5f5129a`. The Gitar pass approved that head, with 1 closed finding and no open one.

- The push was at 03:46:08Z, and the request at 03:49:29Z.
- Gitar replied `On it` at 03:49:54Z, and it wrote the dashboard comment `5724880333` at 03:50:16Z.
- Each of the three times is later than the one before it, so the pass covers the effective head (D-603).

### Traps and gotchas

- A sweep for stale text must read `docs/decisions.md` too. The REF 3 rule of the checker skips that file (D-606), so a stale row there passes every check.
- PR-37 keeps its retired entry in `phase-2-first-playable.md`, because the register of the `PR-` prefix reads that heading.
- The option list of OQ-37 still names PR-37. That list is the record of the options of 2026-09-12, and D-172 closed the question.
- `deck-test/` stays untracked, as it was before this session.
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 98.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34, and the probe of D-621 answers it after the merge.

### The next concrete action

Ask Codex for the repeat review of the effective head `5f5129a`. The record of that review sets the verdict for `review-gate`.

## Session 96: 2026-09-18, Codex

Author: Codex
Session: review of PR #24 at effective head `7426dc9`. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. Role: reviewer. Base: `2874b70`.

### What this session did, and why

- Reviewed the full 17-path documentation diff and the current Gitar pass.
- Found that D-618 retired the CRT, but the effects roadmap still named PR-37 and described the old-monitor pass.
- Found remaining CRT references in D-172, D-214, and `area-effects.md`.
- Found a separate failure in the PR description: its Documents lines did not use the required forms of D-581. The reviewer corrected the lines and recorded each edit.
- The review record gives the verdict and evidence for this head.

### The state of the build

- `main` and the merge base are `2874b70`. Effective head: `7426dc9`. Remote metadata tip at verification: `a20b9c4`.
- `ste-check` passes with 0 findings. `make verify` stopped at a build with no diagnostic output, so its result is inconclusive.
- CI skips build, test, format, coverage, lint, and smoke for this documentation-only PR under D-600.
- The current Gitar pass approves `7426dc9`, with one closed finding and no open finding. No inline thread is open.
- `deck-test/` remains untracked and untouched.

### What is in flight

The review record and this handoff are published. The author needs to correct the remaining CRT contracts before the owner can merge.

### Traps and gotchas

- The review commit changes only metadata paths, so the effective head remains `7426dc9` (D-610).
- The Gitar summary repeats older decision ids, but the push, request, reply, and dashboard times prove the pass covers this head (D-603).
- D-618 supersedes the CRT parts of D-88, D-105, D-120, and D-240; D-619 retains two transition names only.
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 97.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34.

### The next concrete action

The author corrects P2-1, then requests a repeat review of the new effective head.
