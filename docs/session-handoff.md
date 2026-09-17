# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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
- This round changes code, a test, a decision row, and one skill reference file, so the effective head moves to the commit of this round.
- Session 73 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The Gitar pass of the new head, then the repeat review of the other provider.

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

## Session 82: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #21, in the same session as Session 78 and Session 80 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The repeat review of `3a75767` marks P1-1, P1-2, and P2-1 as fixed. It adds one finding, P1-3, and it keeps the verdict `Changes required`.
- P1-3: RG 4 read the first bold verdict line and stopped. A section with the approved verdict and then `**Changes required.**` passed the gate. The claim reproduces, and it has full merit.
- The rule now reads each bold name of the `## Verdict` section. More than one verdict name gives a fault, on one line or on two lines.
- The correction text of the finding says that the line holds the approved verdict alone. A line with no prose after the name breaks the skeleton of the `pr-review` reference file, which each record of PR #19, PR #20, and PR #21 follows. The round applied the words of the regression check of the finding instead, and `docs/reviews/pr-21-response.md` gives the evidence.
- Added three tests. Two of them fail on the old rule. The third proves that an earlier verdict in its own section still passes, which is the shape of a repeat review record.
- The `pr-review` reference file names the new rule, and it says that an earlier verdict goes in a section of its own (D-579).

### The state of the build

- `main` is `04953e4`. The head before this round was `3a75767`, and the remote tip was `a8f91fe`.
- `make verify` passes: the build, 122 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, tests, and one skill reference file, so the effective head moves to `b466a64`.
- The nine CI checks pass on `b466a64`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `b466a64` with the verdict `Approved` and no finding. The pass needed a request again. Gitar replied `On it` at `19:07:27Z`, and its new dashboard comment `5719795670` has the edit time `19:08:51Z`. No thread is open.
- Session 72 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `b466a64`.

### Traps and gotchas

- The `## Verdict` section of a record now gives one verdict name in bold. A repeat review puts each earlier verdict in `## Earlier verdicts`, as the record of this PR does.
- The guard test `AnEarlierVerdictInAnotherSectionPasses` passes on both versions of the rule. It is a guard of the record shape, and not a regression test of P1-3.
- The head of this PR gets no automatic Gitar pass, because the trial paused them. Each round needs a `Gitar review` comment after the push wait.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 83.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Get a Gitar pass of the new head, then get the repeat review of the other provider.

## Session 81: 2026-09-17, Codex

Author: Codex
Session: repeat review PR #21 at effective head `3a75767`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Re-reviewed the three findings from Session 79 against the correction commit `3a75767`.
- Set P1-1, P1-2, and P2-1 to fixed in `3a75767` after their regression cases passed.
- Found P1-3: RG 4 accepts the first approved verdict and ignores a conflicting later verdict line.
- Corrected four stale facts in the PR description: the test count, the solution count, the verification count, and the session handoff row.
- Verified the author provider from Sessions 78 and 80. Codex remains the eligible reviewer under T-4 and D-17.

### The state of the build

- `main` and the merge base are `04953e4`. The effective code head is `3a75767`, and the remote tip before this review publication is `0a811b1`.
- `make verify` passes with 119 tests, clean format, 0 STE findings, and a successful smoke session.
- All nine CI checks pass on metadata tip `0a811b1`.
- The repeat review record and this handoff were published at `c9b17ab`.
- The Gitar pass approves effective head `3a75767` with no finding, and no inline review comment exists.
- The unrelated untracked `deck-test/` stays untouched.

### What is in flight

P1-3 remains open. The PR needs another correction and a third review round.

### Traps and gotchas

- RG 4 reads the first bold verdict line. It does not check for a second conflicting verdict line or for conflicting text after the approved line.
- PR #21 cannot run its live `review-gate` check because GitHub starts the workflow from `main` alone (F-37, D-500).
- The current Gitar approval covers effective head `3a75767`, even though the remote tip is a metadata commit.

### The questions that block progress

None for PR #21. OQ-3 remains for the owner after the first live check run.

### The next concrete action

Correct P1-3, then have Codex review the new effective head.

## Session 80: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #21, in the same session as Session 78 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The review of `docs/reviews/pr-21.md` gives `Changes required` for head `40ea275`, with three findings. Each one reproduces, and each one has full merit.
- P1-1: RG 4 read the whole Verdict section, so `**Not Ready for owner merge.**` passed. The rule now reads the verdict line, which starts with the name in bold.
- P1-2: RG 5 read every line of the record, so a head field of another section passed a record with no head field in its Identity list. The rule now reads the `## Identity` list alone.
- P2-1: RG 8 held `a separate pr` and not `a separate pull request`. The rule now reads `pull request` as `pr`, which covers every phrase of the set at one time.
- Added seven regression tests. Each one fails on the old code, and the round proved that with `git stash` (T-3).
- The `pr-review` reference file and the `one-pr-one-session` skill changed with the command, because the two hold the form that the command reads (D-579).
- `docs/reviews/pr-21-response.md` holds the disposition and the evidence of each finding.
- Put the title back at the top of `docs/session-handoff-archive.md`. The commit `1bdaa89` moved an entry above it, and the title left the file.

### The state of the build

- `main` is `04953e4`. The head before this round was `40ea275`, and the remote tip was `755cd67`, which holds the review record.
- `make verify` passes: the build, 119 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, tests, and two skill files, so the effective head moves to `3a75767`.
- The nine CI checks pass on `3a75767`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `3a75767` with the verdict `Approved` and no finding. The pass needed a request, because the head got no automatic pass in the wait of five minutes. Gitar replied `On it` at `18:32:04Z`, and it posted a new dashboard comment `5719382142` with the edit time `18:34:50Z`. No thread is open.
- Session 70 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `3a75767`.

### Traps and gotchas

- A review record must now hold the verdict name in bold on its own line, and the head field in the `## Identity` list. An older record in another form fails RG 4 or RG 5.
- The proof that a regression test fails on the old code needs the old source of the tool alone. The tests build against both, because they use the public members of the rules.
- The first test of P1-2 used a stale head in the Identity list, and the old rule failed that record for another reason. The test now uses an Identity list with no head field, which is the true trigger.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 81.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Answer the Gitar pass of the new head, then get the repeat review of the other provider.

## Session 79: 2026-09-17, Codex

Author: Codex
Session: review PR #21 at effective head `40ea275`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Reviewed PR #21, the review gate, from its merge base to effective head `40ea275`.
- Added three findings to `docs/reviews/pr-21.md`: a negated approval can pass RG 4, an out-of-section head can pass RG 5, and RG 8 misses spelled-out deferrals.
- Corrected the `docs/reviews/` line in the PR description after the review record landed.
- Verified the author provider from Session 78 and the opposite-provider rule of T-4 and D-17.
- Checked all 28 changed paths, the workflow trust boundary, the command rules, the test fixtures, and the Documents section.

### The state of the build

- `main` is `04953e4`, and the implementation head is `40ea275`.
- `make verify` passes locally with 113 tests, clean format, 0 STE findings, and a successful smoke session.
- All nine CI checks pass on PR tip `d5d9332`. The live review-gate check is absent on PR-3 by design (F-37, D-500).
- All nine CI checks also pass on metadata tip `75b5f63`, after the review record and corrected Documents row were pushed.
- The Gitar pass is current on `40ea275`, and it reports approval with no finding.
- The remote PR tip before this review publication was `d5d9332`. The untracked `deck-test/` stays untouched.
- The review record and this handoff are published at `1bdaa89`. The record gives the findings for the author to correct.

### What is in flight

The PR waits for the author to correct the findings and for a repeat review.

### Traps and gotchas

- A review record and handoff commit do not move the effective head (D-610).
- `make verify` cannot run the live review-gate workflow on this PR. The first live run is on the next PR (D-500).
- The absent checks are det-lint (PR-46), replay identity (PR-4), screen test (PR-41), bot (PR-15), and night gate (PR-49).

### The questions that block progress

None for the findings. OQ-3 remains for the owner after the first live check run.

### The next concrete action

The author corrects the findings, then Codex reviews the new effective head.

## Session 78: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-3, the review gate. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- Asked the owner OQ-69, OQ-181, and OQ-182 before any change (D-19). The answers are D-609, D-610, and D-611.
- Wrote the `review-gate` command in Tools, with eight rules from RG 1 to RG 8 (D-15, D-579).
- RG 1 and RG 2 read the override label. RG 3 to RG 5 read the review record. RG 6 to RG 8 read the documents.
- The command takes one JSON file with the facts of the PR, and one folder with the files of the head.
- Added `.github/workflows/review-gate.yml` on `pull_request_target`. The job runs from `main`, and it never builds the head.
- Added 60 tests. They hold each exit test of the roadmap entry, and the report of each fixture is in the PR description (D-500).
- D-610 made the metadata set the four paths of the PR. The `pr-review` reference, the `gitar-review` skill, and the `one-pr-one-session` skill hold the new set.
- D-611 made PR-84, the context budget check in the `ste-check` command, right after PR-3. The roadmaps and the sequence hold it.

### The state of the build

- `main` is `04953e4`. This branch holds the commit of this entry.
- `make verify` passes: the build, 113 tests, the format check, the STE check with 0 findings, and the smoke session.
- The nine CI checks pass on the head `40ea275`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `40ea275` with the verdict `Approved` and no finding. The dashboard comment `5718999630` has the edit time `18:03:21Z`, which is after the push. No thread is open.
- The `review-gate` check is absent from the head, because GitHub starts its trigger from `main` alone (F-37, D-500).
- The unrelated untracked `deck-test/` stays untouched.

### What is in flight

The review of the other provider at the effective head `40ea275`.

### Traps and gotchas

- GitHub starts `pull_request_target` from the default branch alone, so the live check cannot run on this PR (F-37). The first live run is the next PR (D-500).
- The command reads the description of the PR. An edit of the description changes the result, so the workflow also runs on the `edited` type.
- The deferral rule reads the Documents lines alone, and it reads a set of phrases. `DocumentRules.DeferralPhrases` holds each one.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 79.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Open PR #21, answer the Gitar pass, then hand the PR to the other provider.

## Session 77: 2026-09-17, Codex

Author: Codex
Session: review PR #20 at effective head `e800f4c`. Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. Role: reviewer. Base: `9b84158`.

### What this session did, and why

- Verified Claude Code authored PR #20, so Codex meets the cross-provider gate (T-4, D-17).
- Read the full diff, the PR description and comments, the PR-2 roadmap entry, and the affected decisions and questions (D-589).
- Ran `make verify`: build, 53 tests, format, STE check, and Godot smoke all passed.
- Verified all nine CI checks pass on PR tip `2e316e0`. The effective head remains `e800f4c` because later commits change metadata paths alone.
- Gitar's finding on three missed contractions is fixed and resolved in `e800f4c`.
- Wrote `docs/reviews/pr-20.md` with `Ready for owner merge` for `e800f4c`.

### State of the build

- `main` is `9b84158`. The effective head is `e800f4c`, and the remote tip before this review commit is `2e316e0`.
- Local `make verify` passes. All nine CI checks pass on the remote tip.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The review record and this entry are on the PR branch, and GitHub reports the current head.

### Traps and gotchas

- The review covers effective head `e800f4c`; later metadata commits do not change it.
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 78.

### Open questions that block progress

None for PR #20. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The review record and handoff are on the remote. The owner can merge PR #20.

## Session 76: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the Gitar pass of PR #20, in the same session as Session 75 (D-582).
Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. PR: #20. Role: author. Base: `9b84158`.

### What this session did, and why

- The Gitar pass on head `a155ebc` gives `Approved with suggestions` with one finding.
- The finding says that the contraction rule misses `he's`, `she's`, and `who's`. The claim holds. A run of the pattern on each form gives no match, and the table of the skill promises a pronoun with `'s`.
- Fixed the pattern. The possessive of each of these pronouns has no apostrophe, so each form is always a contraction: its, his, hers, and whose.
- Added seven tests: four forms that fail the rule, and three possessives that pass it. Three of the four fail on the old pattern.
- Hoisted two patterns that a method built on each call. The result does not change, and the tool no longer compiles a pattern in a loop (T-1).

### State of the build

- `main` is `9b84158`. The head before this round was `a155ebc`, and the ten checks passed on it.
- `make verify` passes on this round: the build, 53 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code and a test, so the effective head moved to `e800f4c`.
- Session 66 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).
- The nine CI checks pass on `e800f4c`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `e800f4c` with the verdict `Approved`, and it names the fix. The dashboard comment `5718363510` has the edit time `17:11:42Z`, which is after the push time `17:10:29Z`. The one thread is resolved, and no thread is open.

### In flight

The Codex review of PR #20 at the effective head `e800f4c`.

### Traps and gotchas

- Automatic Gitar reviews are paused on this trial, and the pass on `a155ebc` still ran. Read the Gitar check on the head before a `Gitar review` comment.
- The reply to the thread names the commit that fixes the finding.
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 77.

### Open questions that block progress

None for PR #20. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the Gitar pass on the new head, then hand PR #20 to Codex for the review.

## Session 75: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-2, the STE checker in C#. Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. Role: author. Base: `9b84158`.

### What this session did, and why

- Asked the owner OQ-67 and OQ-68 before any change (D-19). The answers are D-604 and D-605.
- Asked three more questions that the work raised, and the answers are D-606, D-607, and D-608.
- Wrote the `ste-check` command of Tools as new code (D-101, D-277). It holds the eight writing rules of the `ste-writing` skill, the comment rule of F-11, the reference check, and the session number check.
- The writing rules give the same result as the interim Python script on every live document. The run gives 0 findings.
- Retired `docs/tools/ste-check.py`. The Makefile target, the `ste-check` CI job, `CLAUDE.md`, `AGENTS.md`, the skill, and the two runbooks now name the command.
- Corrected six citations that the new reference check found: two dead paths in decision rows, one bare `SKILL.md`, one external path under `.github/`, and two range markers that read as a citation of D-1.
- Moved Session 65 and Session 64 to the archive. The handoff held 11 entries, and D-18 keeps 10.

### State of the build

- `main` is `9b84158`. The branch is `feat/pr-2-ste-checker`.
- `make verify` passes: the build, 46 tests, the format check, the STE check with 0 findings, and the Godot smoke session.
- The new command runs the whole checkout in about one second.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The first push of PR-2, then the Gitar pass, then the Codex review.

### Traps and gotchas

- The reference check reads a path in backticks. Write a branch name, an external path, and a refused file name without backticks, or name the PR that creates the file (G-16).
- A line that names a `PR-#` marks every path on that line. The mark is broad by design.
- The checker reads the working tree, and not the staged files (D-608). A fault in an unstaged file stops the commit.
- The rule MD 1 fails an HTML comment across lines. The removal of such a comment hides prose from every rule (F-11).
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 76.

### Open questions that block progress

None for PR-2. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push the branch, open PR-2, and get the Gitar pass on the head.
