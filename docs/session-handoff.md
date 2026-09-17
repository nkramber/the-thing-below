# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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
- The updated review record and this entry await publication. The review gate must pass on the published record.

### What is in flight

Publish the repeat review and verify that `review-gate` passes for effective head `70df5ef`.

### Traps and gotchas

- The correction changes the effective head from `f5eba68` to `70df5ef`. Later handoff commits do not change it.
- The current Gitar summary repeats the old count of 186 tests. The local suite and the author response verify 187.
- `deck-test/` remains untracked and untouched.

### The questions that block progress

None for PR #23. The finding is fixed, and the review gate waits for the published verdict.

### The next concrete action

Push the updated review record and handoff. Verify the new PR head and all checks, then hand the PR to the owner.

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

## Session 89: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-46, the `det-lint` command and its job. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. PR: #23. Role: author. Base: `0fbecab`.

### What this session did, and why

- Asked the owner OQ-70 and OQ-71, which blocked PR-46. The owner took the recommended option of each one, and D-614 and D-615 record the answers.
- Wrote the `det-lint` command in Tools on the Roslyn compiler library, version 5.9.0 of `Microsoft.CodeAnalysis.CSharp` (D-498). The command reads the type of each expression, so a real literal with no suffix fails too (F-38).
- Ten rules: DL 0 for a compilation error, DL 1 to DL 7 for Core, and DL 8 and DL 9 for Game. Section 7.4 of `docs/roadmaps/area-tools.md` holds the table.
- Added the `det-lint` job to the Linux leg of CI and the `lint` target of `make verify`.
- The first live run found a real fault: `CoreAssembly.Self` gave a `System.Reflection.Assembly` from Core. No code read it, because the reference test of G-1 reads the built file (F-61). The member now gives the name of the assembly, and that test reads the name.
- F-65 joins the finding register: Godot.NET.Sdk writes the build output of a Godot project to `.godot/mono/temp/bin/<configuration>/`, and that folder holds `GodotSharp.dll` from the NuGet restore.

### The state of the build

- `main` is `0fbecab`. The head of PR #23 is `f5eba68` before this entry.
- `make verify` passes: the build with 0 warnings, 186 tests, the format check, `det-lint` with 0 findings, `ste-check` with 0 findings, and the smoke session.
- Every CI check passes on `88215f8`: three build legs, three smoke legs, the changed paths job, the coverage report, `ste-check`, and the new `det-lint` job. The `det-lint` job takes 26 seconds.
- `review-gate` fails on RG 3 alone, because no record exists at `docs/reviews/pr-23.md` yet. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.

### What is in flight

The Codex review of the effective head `f5eba68`. The Gitar pass gives the verdict `Approved` with no finding and no open thread. This PR changes code and `.github/workflows/`, so no label exempts it (D-560).

### Traps and gotchas

- The lint needs a build first. `ReferenceSet` reads the Godot assembly from the Game build output, and the error of an absent folder names the build command (T-2).
- The `bin` folder of the Game project stays empty. F-65 names the real output folder.
- DL 4 passes `typeof(X)` alone, because the content reader of PR-5 needs it in an attribute (F-36). It fails each member of `Type` that reflects.
- `GodotTextRule.TextHelperType` holds the name of the text helper of PR-61. That PR confirms the name or changes the constant (G-16).
- `deck-test/` stays untracked, as it was before this session.
- The commit `88215f8` changes the two handoff files alone, so the effective head stays `f5eba68` (D-610).
- Gitar paused automatic reviews for the trial, and the pass of this PR ran on the first push.
- The next ids are D-616, OQ-183, F-66, L-16, G-27, PR-85, M-8, and Session 90.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

The Codex review of PR #23 at the effective head `f5eba68`. The review record goes to `docs/reviews/pr-23.md`, and RG 3 passes with it.

## Session 88: 2026-09-17, Codex

Author: Codex
Session: review PR #22 at effective head `1f07b6b`. Repository: the-thing-below. Branch: `feat/pr-84-context-budget`. Role: reviewer. Base: `9787b2d`.

### What this session did, and why

- Reviewed PR #22, the context budget checks in `ste-check`, against the five exit tests of section 7.6 of `docs/roadmaps/phase-1-foundations.md`.
- Verified the Gitar heading-boundary finding and its regression test. The shared detector fixes the trigger.
- Updated the PR test count to 136 after local verification.
- Wrote `docs/reviews/pr-22.md` with the verdict for effective head `1f07b6b`.

### The state of the build

- `main` is `9787b2d`. The PR branch is `feat/pr-84-context-budget`, with effective head `1f07b6b` and metadata tip `b55f8e6` before this review commit.
- Local build, 136 tests, format check, and `ste-check` pass. CI build, test, format, smoke, changed paths, coverage, and `ste-check` pass on `1f07b6b`.
- The initial `review-gate` run failed RG 3 because the review record was not on the head. After the metadata push, all ten checks pass, including `review-gate`.

### What is in flight

The review record and this entry are pushed as `d7f6fb3`. GitHub reports this commit as the head, the tree has no staged or tracked changes, and all ten checks pass, including `review-gate`.

### Traps and gotchas

- `deck-test/` was untracked before review work and remains untouched.
- A commit that changes only this PR's review record and handoff files does not move the effective head (D-610).
- The PR description now records the verified test count of 136.

### The questions that block progress

None for this PR. Future checks remain named with their creating PR in the PR gate (G-16).

### The next concrete action

The review is published at `d7f6fb3`, and all checks pass. The owner can merge PR #22.

## Session 87: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the Gitar pass of PR #22, in the same session as Session 86 (D-582).
Repository: the-thing-below. Branch: `feat/pr-84-context-budget`. PR: #22. Role: author. Base: `9787b2d`.

### What this session did, and why

- The Gitar pass on `bdf9e06` gives the verdict `Approved with suggestions` and one finding.
- The finding: `SizeRules` read a session heading as the prefix `## Session `, and `SessionNumberRules` reads it as the form `## Session N:`. The two rules of one command disagreed on where an entry starts.
- The claim reproduces. A line of the top entry such as `## Session numbering rules` ended the measured region, so SIZE 2 undercounted the entry and could miss a real fault.
- The finding has full merit. `SessionNumberRules.IsSessionHeading` is now the one detector, and `SizeRules` calls it. The fault message of an absent entry names the form of a heading.
- One regression test, `ASubHeadingInsideTheTopEntryDoesNotEndIt`. It fails on the old code with 135 tests and passes on the new code with 136.
- The Gitar pass approves the new head `1f07b6b` with the verdict `Approved` and `1 closed / 1 findings`. Its one thread is resolved, and no thread is open.
- The first live run of the `review-gate` check found a real fault (F-37, D-500). The `docs/reviews/` line of the Documents section was not one of the three forms of D-581, and RG 7 failed. The line is now the `Changed:` form, and RG 7 passes.

### The state of the build

- `main` is `9787b2d`. The head before this round was `bdf9e06`, and the PR is #22.
- The nine CI checks pass on `1f07b6b`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- `review-gate` fails on RG 3 alone, because no record exists at `docs/reviews/pr-22.md` yet. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.
- `make verify` passes: the build, 136 tests, the format check, the STE check with 0 findings, and the smoke session.

### What is in flight

The Codex review of the effective head `1f07b6b`.

### Traps and gotchas

- The Gitar trial paused automatic reviews. The first round got an automatic review at 20:12:31Z, and this round got none. The round needed a `Gitar review` comment at 20:23:50Z, and the reply came at 20:24:11Z.
- Gitar deleted the dashboard comment and posted a new one with the id 5720742291. The older id gives HTTP 404. Read the newest id in each check.
- RG 3 fails until the review record lands. That is the gate of T-4, not a fault of this PR.
- `CLAUDE.md` holds 152 free bytes under the 16 KB limit (D-613).
- The next ids are D-614, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 88.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Codex reviews PR #22 at the effective head `1f07b6b`, and writes `docs/reviews/pr-22.md`.

## Session 86: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-84, the context budget check in the `ste-check` command.
Repository: the-thing-below. Branch: `feat/pr-84-context-budget`. PR: #22. Role: author. Base: `9787b2d`.

### What this session did, and why

- Added `TheThingBelow.Tools/SteCheck/SizeRules.cs`, the size rules of the context budget (D-583, D-611). It holds the three limits in one place, and the `ste-check` command runs it after the session number check.
- SIZE 1 reads `CLAUDE.md` and `AGENTS.md` at 16 KB. D-20 keeps the two files identical, and D-583 names each one in the start set.
- SIZE 2 reads the top handoff entry at 5 KB, from its heading to the heading of the next entry. An older entry takes no limit (D-584).
- SIZE 3 reads every `.md` file of `.claude/skills/` at 36 KB.
- The count reads the lines of a file, and each line ending counts as one byte. Thus every CI leg reads the same number, whatever the line ending of the checkout.
- `CLAUDE.md` held 17015 bytes, 631 above the limit. The session trimmed 783 bytes from each instructions file: stale sentences that name PR-1, PR-2, and PR-3 as the creator of a check that exists now, and prose that the `ste-writing` and `gitar-review` skills already hold. No rule left the file.
- The owner answered the headroom question. D-613 keeps the file at 16232 bytes, refuses a move of the command list to a runbook, and asks each later PR to find its own bytes.
- Added 12 tests. They cover the five exit tests of section 7.6 of `docs/roadmaps/phase-1-foundations.md`.

### The state of the build

- `main` is `9787b2d`, which merged PR #21. The branch is `feat/pr-84-context-budget`, and the PR is #22. The first head is `5a8bae7`.
- `make verify` passes: the build, 135 tests, the format check, the STE check with 0 findings, and the smoke session.
- `CLAUDE.md` and `AGENTS.md` hold 16232 bytes each. The largest skill file is `.claude/skills/pr-review/SKILL.md` at 19216 bytes.

### What is in flight

The Gitar pass on `5a8bae7`, the CI checks, and the Codex review.

### Traps and gotchas

- This PR is the first live run of the `review-gate` check (F-37, D-500). PR-3 could not run it. A failure of that check needs a read of the workflow, not a change to the command.
- `CLAUDE.md` holds 152 free bytes. A later PR that adds a rule first removes the bytes that it needs (D-613).
- The size rules need `CLAUDE.md`, `AGENTS.md`, the handoff, and one skill file. `SteCheckCheckout.Build` writes each one, so a new fixture checkout gets them.
- This PR adds a decision row, so the `review-override` label does not apply (D-401, D-609). It goes to Codex.
- The next ids are D-614, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 87.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Commit the work, push, then get the Gitar pass on the head.

## Session 85: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #21 at effective head `89d125e`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Read the author response and verified the P1-4 correction against its trigger.
- Verified that D-612 requires one bold span in the Verdict section, and that the new rule enforces it.
- Set P1-1, P1-2, P1-3, P1-4, and P2-1 to fixed in `89d125e`.
- Ran the 14 focused review-gate tests and `make verify`. All checks passed, including 123 tests and Godot smoke.
- Verified all nine CI checks pass on metadata tip `1d3398b`. The Gitar pass approves effective head `89d125e`; its report says no rules were evaluated and functional validation was not enabled.
- Updated the review record with `Ready for owner merge` for effective head `89d125e`.

### The state of the build

- `main` and the merge base are `04953e4`. The effective head is `89d125e`, and the remote tip is metadata commit `1d3398b`.
- `make verify` passes: build, 123 tests, format, STE with 0 findings, and Godot smoke.
- All nine CI checks pass on `1d3398b`. The unrelated untracked `deck-test/` remains untouched.

### What is in flight

The review record and this entry need a commit and push to the PR branch.

### Traps and gotchas

- D-612 closes the scope of RG 4. A review that requires a check of Verdict prose needs a new owner answer.
- PR-3 has no live `review-gate` check. D-500 accepts the command tests as evidence, and the first live run is on the next PR.
- The next ids are D-613, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 86.

### The questions that block progress

None for PR #21. OQ-3 applies after the first live check run. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Commit this review and handoff, push, then verify the published head.

## Session 84: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the third review round of PR #21, in the same session as Session 78, Session 80, and Session 82 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The review of `b466a64` marks P1-1, P1-2, P1-3, and P2-1 as fixed. It adds P1-4, and it keeps the verdict `Changes required`.
- P1-4: the rule counted the bold spans that match a verdict name, so `**Not Ready for owner merge.**` beside the approved name passed. The claim reproduces, and it has full merit.
- The `## Verdict` section now holds one bold span, and that span is the verdict name. A second bold span gives a fault, whatever its text.
- This was the third round of findings on RG 4, so the session asked the owner to settle the scope of the rule (D-19). The answer is D-612, and it is the rule above. A check of the prose of the section stays out of scope.
- A count of the bold spans of the `## Verdict` section of each of the 16 records of `docs/reviews/` gives one span, so the new rule needs no change to any record.
- Added one test for the new trigger. It fails on each older version of the rule.

### The state of the build

- `main` is `04953e4`. The head before this round was `b466a64`, and the remote tip was `1126bc2`.
- `make verify` passes: the build, 123 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, a test, a decision row, and one skill reference file, so the effective head moves to `89d125e`.
- The nine CI checks pass on `89d125e`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `89d125e` with the verdict `Approved` and no finding. The pass needed a request again. Gitar replied `On it` at `19:33:14Z`, and its new dashboard comment `5720109237` has the edit time `19:35:01Z`. No thread is open.
- Session 74 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `89d125e`.

### Traps and gotchas

- D-612 closes the scope of RG 4. A finding that asks the rule to read the prose of the section needs a new owner answer first.
- The `## Verdict` section of a record takes no bold word in its prose. The reference file says so.
- The head of this PR gets no automatic Gitar pass, because the trial paused them. Each round needs a `Gitar review` comment after the push wait of three minutes.
- The next ids are D-613, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 85.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Get a Gitar pass of the new head, then get the repeat review of the other provider.

## Session 83: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #21 at effective head `b466a64`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Re-read the response file and verified the correction of P1-3 against its trigger.
- Set P1-3 to fixed in `b466a64`. The two regression tests for a second recognized verdict fault, and the earlier-verdict guard passes.
- Found P1-4: RG 4 passes an approved verdict followed by a bold negation. A probe against the current code returns `Pass`.
- Verified author-provider evidence in Sessions 78, 80, and 82. Codex is the eligible reviewer (T-4, D-17).
- The review record keeps one current verdict and the earlier verdict history.

### The state of the build

- `main` and the merge base are `04953e4`. The prior effective head was `3a75767`, and the new effective head is `b466a64`.
- The PR tip before this review was `c313e61`, a metadata commit. All nine CI checks pass on that tip.
- `make verify` passes at `b466a64`: build, 122 tests, format, 0 STE findings, and Godot smoke.
- Gitar approved `b466a64` at `19:08:59Z` with no finding. Its report says no rules were evaluated and functional validation was not enabled. No inline review comment exists.
- The unrelated untracked `deck-test/` remains untouched.

### What is in flight

P1-4 remains open. The PR needs a correction and another Codex review.

### Traps and gotchas

- RG 4 counts recognized verdict names and ignores other bold spans. A negated verdict can follow an approved verdict without a fault.
- PR-3 has no live `review-gate` check because GitHub starts the workflow from `main` alone (F-37, D-500).

### The questions that block progress

None for PR #21. OQ-3 remains for the owner after the first live check run.

### The next concrete action

Correct P1-4, then have Codex review the new effective head.
