# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

- `main` is `b3ec2b4`. The PR head is `5ccbaa2`, and the handoff commit follows it.
- The spike branch `spike/screen-scale-probe` is at `f314243`, and it never merges (D-597, D-621).
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- `ste-check` gives 0 findings, so each new id resolves.

### What is in flight

The push, the Gitar pass, and the Codex review. This PR changes decision rows, so the `review-override` label does not apply (D-401, D-609).

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

## Session 95: 2026-09-18, Claude Code

Author: Claude Code
Session: the time of the screen scale probe, in the same session as Session 93 (D-582). Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. PR: #24. Role: author. Base: `2874b70`.

### What this session did, and why

- The owner set the time of the screen scale probe: the next clean session after the merge of PR #24.
- D-625 records it, and the transitional prompt of the merge names the probe (D-601).
- The probe moved up the order of Phase 1 in `docs/design.md` and in `docs/roadmaps/phase-1-foundations.md`.
- The Sprite Fusion test keeps its place before PR-34, because it waits for the plan of the owner.
- The probe entry gained three facts: one build for each machine, the font of D-263 from its OFL release, and the trap of a picture viewer on the 4K screen of the Mac.
- The frame fits both desktop screens at a whole number, 3x at 3840 by 2160 and 2x at 2560 by 1440.

### The state of the build

- `main` is `2874b70`, and the head of this round changes documents alone.
- The `ste-check` command passes with 0 findings.
- The Gitar pass of `741e8db` approved that head, and this round moves the effective head, so the PR needs a new pass.

### What is in flight

The review of Codex. The Gitar pass approved the head `7426dc9`, with 1 closed finding and no open one.

- The push of `7426dc9` was at 03:19:22Z, and the request at 03:22:49Z.
- Gitar replied at 03:23:09Z, and it wrote the dashboard comment `5724679546` at 03:25:01Z.
- Each of the three times is later than the one before it, so the pass covers the effective head (D-603).
- The summary of the pass names D-616 to D-624 and not D-625. A summary can repeat the words of an older pass, and the three times prove this pass current.

### Traps and gotchas

- The probe needs the owner and three machines, so it cannot run in a session of its own without the owner.
- macOS maps a picture pixel to a point on a 4K screen, so the probe draws at the native size and never in a viewer.
- The order lists of the two files hold the same steps, and a change to one needs the same change in the other.
- `deck-test/` stays untracked, as it was before this session.
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 96.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34, and the probe answers it right after the merge.

### The next concrete action

Hand PR #24 to Codex for the review of T-4. This PR adds decision rows, so the label of D-401 does not apply.

## Session 94: 2026-09-18, Claude Code

Author: Claude Code
Session: the answer to the Gitar pass of PR #24, in the same session as Session 93 (D-582). Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. PR: #24. Role: author. Base: `2874b70`.

### What this session did, and why

- The Gitar pass of `589b45b` gave one finding, and the finding has full merit.
- Three places named 5.00 ms as the worst stage under Mobile. That number is the Forward+ value.
- The report of the run gives 4.55 ms at the 95th percentile for the worst stage under Mobile.
- Corrected M-7 and F-66 in `docs/design.md`, and the Effect column of D-617.
- D-617 also derived 30 percent of the frame budget from the wrong number, and 4.55 ms gives 27 percent.
- Each corrected line now names both renderers, so a later reader cannot mix the two again.

### The state of the build

- `main` is `2874b70`, and the PR head before this round was `589b45b`.
- The `ste-check` command passes with 0 findings, and `make verify` passed on `5b6c14e`.
- On `589b45b` the CI legs pass, and `review-gate` fails RG 3, because no review record exists yet.

### What is in flight

The review of Codex. The repeat Gitar pass approved the head `741e8db`, with 1 closed finding and no open one.

- The push of `741e8db` was at 03:07:43Z, and the request at 03:11:05Z.
- Gitar replied `On it` at 03:11:41Z, and it wrote the dashboard comment `5724580560` at 03:11:49Z.
- Each of the three times is later than the one before it, so the pass covers the effective head (D-603).

### Traps and gotchas

- The two reports differ by stage, and the Mobile column is the one that binds the plan (D-616).
- Gitar replaced its dashboard comment during the first pass, so the old comment id gave HTTP 404.
- The flag `--attach` of `gh` refuses a text file, so each report lives in the text of the PR (D-624).
- `deck-test/` stays untracked, as it was before this session.
- The next ids are D-625, OQ-184, F-68, L-16, G-28, M-9, and Session 95.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34, and the probe of D-621 answers it.

### The next concrete action

Hand PR #24 to Codex for the review of T-4. This PR adds decision rows, so the label of D-401 does not apply.

## Session 93: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-85, the result of the Deck test and the look. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. PR: #24, which is PR-85 of the roadmap. Role: author. Base: `2874b70`.

### What this session did, and why

- The owner ran the Deck test of D-160 on an OLED Deck and gave the reports to this session.
- Recorded the result and eight more owner answers as D-616 to D-624.
- D-616: the game uses the Mobile renderer, which won each of the 20 stages. PR-82 writes it.
- D-617: the first effect budget, 15 lights, 8192 particles, and 3 full-screen passes. F-66 marks each row as a floor.
- D-618: the CRT leaves the plan. PR-37 is retired, and 13 documents lost the pass, the toggle, and its captures.
- D-620 and D-621: two tests before PR-34, the Sprite Fusion art test and the screen scale probe.
- D-622 and G-27: every effect draws with the palette and hard edges, which answers the question of the owner about the fog.
- D-623: the Mac is the venue of each visual test, and the Deck takes the answers that need the Deck.
- F-67 and OQ-183: a 32-pixel sprite covers 4.0 mm on the Deck, and the scale of the frame is now an open question.

### The state of the build

- `main` is `2874b70`, and PR #23 merged before this session.
- The `ste-check` command passes on every live document, with 0 findings.
- The branch holds one commit, and it changes documents alone.

### What is in flight

The push, the Gitar pass, and the review of Codex. This PR revises decision rows, so the label of D-401 does not apply.

### Traps and gotchas

- Each citation of D-88, D-105, D-120, or D-240 must name D-618, or REF 3 fails.
- PR-37 keeps a retired entry in `phase-2-first-playable.md`, which keeps the id in the register (G-10).
- Section 7 of `phase-1-foundations.md` gained three entries, so each later section number moved.
- The flag `--attach` of `gh` refuses a text file, so the reports go in the description and in a comment (D-624).
- `deck-test/` stays untracked, as it was before this session.
- The next ids are D-625, OQ-184, F-68, L-16, G-28, M-9, and Session 94.

### The questions that block progress

OQ-183 blocks PR-7 and PR-34. The probe of D-621 answers it, and that probe needs the owner and three screens.

### The next concrete action

Answer the Gitar pass on PR #24, then hand the PR to Codex for the review of T-4.

## Session 92: 2026-09-17, Codex

Author: Codex
Session: re-review PR #23, roadmap PR-46, the `det-lint` command. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. Role: reviewer. Base: `0fbecab`.

### What this session did, and why

- Re-reviewed P2-1 at effective head `70df5ef` against the trigger from the earlier review.
- Verified the fix in `SceneTextRule`, the regression test, the response file, and the live command probe.
- The regression probe now reports one DL 9 finding and exits 1. `make verify` passes with 187 tests.
- Verified the current Gitar pass after its request, reply, and dashboard update. No finding or open thread exists.
- Updated `docs/reviews/pr-23.md` to close P2-1 and assess effective head `70df5ef`.

### The state of the build

- `main` and the merge base are `0fbecab`. The current PR head is `f62113e`; its effective head is `70df5ef` (D-610).
- `make verify` passes: build with 0 warnings, 187 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub checks pass except `review-gate`, which fails RG 4 and RG 5 while the published review record still holds the old verdict and head.
- The updated review record and this entry were published as `2e4cc38`. All 11 checks pass on that head, including `review-gate`.
- This follow-up metadata commit records the publication and check verification. The effective head remains `70df5ef` (D-610).

### What is in flight

The repeat review is published and `review-gate` passes. The owner can merge PR #23.

### Traps and gotchas

- The correction changes the effective head from `f5eba68` to `70df5ef`. Later handoff commits do not change it.
- The current Gitar summary repeats the old count of 186 tests. The local suite and the author response verify 187.
- `deck-test/` remains untracked and untouched.

### The questions that block progress

None for PR #23. The finding is fixed, and the review gate waits for the published verdict.

### The next concrete action

The owner can merge PR #23. The review applies to effective head `70df5ef`.

## Session 91: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #23, in the same session as Session 89 (D-582). Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. PR: #23. Role: author. Base: `0fbecab`.

### What this session did, and why

- The review of `f5eba68` gives the verdict `Changes required` with one finding, P2-1.
- The finding: DL 9 read the value of a scene property as `[^"]*`, which stops at the first quote. Godot writes a quote inside a string value as `\"`, so a line such as `text = "Say \"hello\""` matched no part of the pattern, and the rule read no property.
- The claim reproduces. The probe gave `0 finding(s)` and an exit code of 0 on the old code.
- The finding has full merit. A player string with a quote in a scene file is a supported case of D-499 and G-7.
- The correction: the value part of the pattern is now `(?:[^"\\]|\\.)*`, which reads an escaped character as one unit. One line of `TheThingBelow.Tools/DetLint/SceneTextRule.cs` changes.
- One regression test, `ATextValueWithAnEscapedQuoteFails`. It fails on the old code with 186 tests and passes on the new code with 187.
- The probe of the reviewer now gives one DL 9 finding, and the command exits 1.
- `docs/reviews/pr-23-response.md` holds the disposition and the evidence.

### The state of the build

- `main` is `0fbecab`. The head before this round is `44f99a8`, and the PR is #23.
- `make verify` passes: the build with 0 warnings, 187 tests, the format check, `det-lint` with 0 findings, `ste-check` with 0 findings, and the smoke session.
- On `70df5ef`, every CI check passes except `review-gate`: three build legs, three smoke legs, the changed paths job, the coverage report, `ste-check`, `det-lint`, and the Gitar check. RG 4 fails, because the record still gives the verdict `Changes required`.

### What is in flight

The repeat review of Codex at the effective head `70df5ef`. The Gitar pass of that head gives the verdict `Approved` with no finding and no open thread.

### Traps and gotchas

- The correction moves the effective head, so the review of `f5eba68` no longer covers the head. The repeat review reads the new head (D-603).
- RG 4 stays red until the record of the repeat review gives `Ready for owner merge`.
- A scene file has no comment syntax, so the pattern of DL 9 reads a whole line and needs no comment rule.
- `deck-test/` stays untracked, as it was before this session.
- Gitar deleted the dashboard comment of the first pass and posted a new one with the id 5721871935. The summary of the new pass repeats the words of the first one, and its three times prove that the pass is current.
- The push wait and each poll of Gitar run in the background, and never in the foreground.
- The next ids are D-616, OQ-183, F-66, L-16, G-27, PR-85, M-8, and Session 92.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

The repeat review of Codex at the effective head `70df5ef`. The record at `docs/reviews/pr-23.md` sets P2-1 to closed and gives a verdict for that head, and RG 4 passes with it.

## Session 90: 2026-09-17, Codex

Author: Codex
Session: review PR #23, roadmap PR-46, the `det-lint` command. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. Role: reviewer. Base: `0fbecab`.

### What this session did, and why

- Reviewed the complete change from effective head `f5eba68` against section 7.7 of the phase roadmap and the affected contracts.
- Confirmed that Claude Code authored the change and Codex meets the opposite-provider gate (T-4, D-17).
- Found P2-1: DL 9 misses a scene text value when the value contains an escaped quote. The probe exited 0 with no finding, against D-499 and G-7.
- Corrected the stale PR description snapshot. It now names head `764390c` and the checks that GitHub reports.
- Wrote `docs/reviews/pr-23.md` with verdict `Changes required` for `f5eba68`.

### The state of the build

- `main` is `0fbecab`. The PR head is `764390c`, and its effective head is `f5eba68` (D-610).
- `make verify` passes locally: build, 186 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub checks pass on `764390c` except `review-gate`, which fails RG 3 because the review record was absent before this commit.
- The review record, handoff entry, and archived Session 80 were published as `b63cfb0`. GitHub confirms that head. All checks pass except `review-gate`, which fails RG 4 because the verdict is `Changes required`.
- This follow-up metadata commit records the publication verification. The effective head stays `f5eba68` (D-610).

### What is in flight

The author needs to correct P2-1 and add a regression test. The reviewer then repeats the review of PR #23.

### Traps and gotchas

- The scene pattern in `SceneTextRule` stops at a quote even when a backslash escapes it. A scene value with an escaped quote bypasses DL 9.
- Gitar approved the effective head but reported no rule coverage and no functional validation. The review checked those claims against the diff and local gates.
- `deck-test/` remains untracked and untouched.

### The questions that block progress

No owner question blocks progress. P2-1 needs the author's correction.

### The next concrete action

The author corrects P2-1 on PR #23. Codex reviews the correction against its new effective head.
