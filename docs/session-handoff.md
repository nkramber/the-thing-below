# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

## Session 74: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `7732b1b`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the response file and the prior review record (D-588, D-589).
- Verified that Session 64 names Claude Code as the author. Codex is eligible under T-4 and D-17.
- Verified the earlier P2-1 correction against its original trigger. It stays fixed in `6f82d26`.
- Reviewed D-603 and the Gitar skill change. A Gitar pass on the effective head remains current across metadata commits.
- Ran `make verify`. The build, 8 tests, format, STE, and Godot smoke checks passed.
- Verified all 9 current CI checks pass on PR tip `784ebbf`.
- Verified the Gitar pass approves effective head `7732b1b`, its request and dashboard times satisfy the freshness rule, and there are zero review threads.
- Set the verdict to `Ready for owner merge` for effective head `7732b1b`.
- Moved Session 63 to the archive (D-18).

### State of the build

- `main` is `ee4305a`. The effective head is `7732b1b`, and the metadata tip is `784ebbf`.
- `make verify` passes. The nine live CI checks pass on the tip. The Gitar pass covers the effective head under D-603.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The review record and this entry are on the PR branch, and GitHub reports the current head. The verdict applies to effective head `7732b1b`.

### Traps and gotchas

- Metadata-only commits do not stale the Gitar pass under D-603. A commit outside the metadata set moves the effective head.
- The PR tip may move after the review record is pushed. Recheck the effective head and the published record.
- The next ids are D-604, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 75.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Wait for the owner merge of PR #19.

## Session 73: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the block of the Gitar freshness rule of PR #19, in the same session as Sessions 64, 66, 68, 69, and 71 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review round of Session 72 sets P2-1 to fixed, and it gives `Blocked` for one reason: the Gitar pass names `6f82d26`, and the tip was `b862cfb`.
- The block is correct under the text of the `gitar-review` skill, and that rule cannot pass. The repository requires a record of each Gitar pass in the handoff, which is a metadata commit, so each pass was stale at the moment of its record.
- The review gives the evidence itself. It pushed `a11f6d7` and `168e602` after it wrote the block, and `git diff --stat 6f82d26..168e602` gives three metadata paths alone. The tip has no Gitar check run.
- The owner answered the question. D-603 sets the rule: a Gitar pass covers the effective head, and a metadata commit does not make it stale.
- The `gitar-review` skill follows D-603. It gets the terms of the effective head and the metadata set, a new first condition, and the command that proves the effective head.

### State of the build

- `main` is `ee4305a`. The effective head before this round was `6f82d26`, and the ten checks passed on it.
- `make verify` passes on the Mac of the owner for this round.
- This round changes `docs/decisions.md` and a skill, so the effective head moved to `7732b1b`.
- The ten checks pass on `7732b1b`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes: the reply "On it" came at `14:44:17Z`, and the dashboard comment `5716314911` has the edit time `14:45:06Z`. Under D-603, this record does not make the pass stale.

### In flight

The repeat review of PR #19 at the effective head `7732b1b`, which gives the verdict.

### Traps and gotchas

- A rule that reads the branch tip fights a rule that reads the effective head. The record of a pass then makes the pass stale, and each side of the review moves the tip.
- D-603 does not weaken the pass. A commit outside the metadata set still needs a new pass.
- Automatic Gitar reviews are paused on this trial. Each new head needs a `Gitar review` comment after the push wait of three minutes.
- The next ids are D-604, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 74.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `7732b1b`, under D-602 and D-603.

## Session 72: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `6f82d26`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the response file and the prior review record (D-588, D-589).
- Verified that Session 64 names Claude Code as the author. Codex is the eligible reviewer (T-4, D-17).
- Re-read the original P2-1 trigger. The correction moves its exception into the merged-PR stop condition and names the bound PR.
- Set P2-1 to fixed in `6f82d26`. The instructions now allow the prompt for the bound PR and stop on another PR's merge.
- Ran `make verify`. The build, 8 tests, format, STE, and smoke checks passed.
- All 9 live CI checks pass on PR tip `b862cfb`.
- The Gitar dashboard approves `6f82d26`, but metadata commit `b862cfb` followed that review. The current PR tip has no Gitar run, so the freshness gate blocks approval.
- Updated `docs/reviews/pr-19.md` for effective head `6f82d26` and corrected the stale handoff fact in the PR description.

### State of the build

- `main` is `ee4305a`. The effective head is `6f82d26`, and PR tip `b862cfb` was the branch head at the start of this review.
- `make verify` passes. The 9 current CI checks pass on `b862cfb`.
- Gitar approves `6f82d26`; the approval is stale for the current PR tip.
- This round changes only the review and handoff metadata. The unrelated untracked `deck-test/` remains untouched.

### In flight

P2-1 is fixed. The review waits for a Gitar pass that matches the current PR tip.

### Traps and gotchas

- A Gitar pass after the code push becomes stale when a later metadata commit moves the PR tip.
- The dashboard update came before metadata commit `b862cfb`, and its check run names `6f82d26`.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 73.

### Open questions that block progress

No owner question blocks PR #19. The stale Gitar pass blocks approval.

### Next concrete action

Get a fresh Gitar pass on the branch tip, then repeat the review.

## Session 71: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the finding P2-1 of PR #19, in the same session as Sessions 64, 66, 68, and 69 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review round of Session 70 gives the verdict `Changes required` for the effective head `38aa19f`, with one finding. That session had network access, and it pushed its own record.
- P2-1 has full merit. Step 1 of the `one-pr-one-session` skill stops a session when the conversation holds a merged PR. The round of D-601 put the exception in a later paragraph, and the word "this rule" did not name the condition. A session that reads the list stops before it writes the prompt of D-601.
- The correction puts the exception in the condition itself, and it names the bound PR. The paragraph now says to write the prompt for the bound PR and its merge message alone.
- `docs/reviews/pr-19-response.md` holds the answer, the correction, and the regression check. It also notes two lines of the record that the verification of the same record refutes.
- The correction changes a skill, so the effective head moves again. The PR needs a new Gitar pass and a repeat review.

### State of the build

- `main` is `ee4305a`. The effective head before this round was `38aa19f`, and the ten checks passed on it.
- `make verify` passes on the Mac of the owner for this round: the build, 8 tests, the format check, the STE check, and the smoke session.
- The ten checks pass on `6f82d26`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes: the reply "Running the review now" came at `13:02:11Z`, and the new dashboard comment `5714796386` has the edit time `13:02:51Z`.

### In flight

The repeat review of PR #19 at the effective head `6f82d26`, which gives the verdict.

### Traps and gotchas

- An exception that sits under a list, and not in the condition, does not change the condition. Put the exception in the line that stops the work.
- Automatic Gitar reviews are paused on this trial. A new head needs a `Gitar review` comment after the push wait of three minutes.
- The review session of this round reached GitHub, and the earlier two did not. The environment of that harness is not stable, and D-602 covers the case with no network.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 72.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `6f82d26`.
