# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

The review record and this entry need one metadata commit and push. The verdict applies to effective head `7732b1b`.

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

## Session 70: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `38aa19f`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the PR review instructions, earlier review record, and response file.
- Verified that Session 64 names Claude Code as the author. Codex is the eligible reviewer (T-4, D-17).
- Reviewed all 11 changed paths, including the new D-602 review procedure.
- Found P2-1: the merged-PR stop rule conflicts with the transitional prompt that D-601 requires.
- Ran `make verify`. The build, 8 tests, format, STE, and smoke checks passed.
- Verified that all 9 live CI checks pass on PR head `cd5ee36`.
- The Gitar dashboard approves effective head `38aa19f`, but no Gitar run exists on current PR head `cd5ee36`. Its pass is stale.
- Updated `docs/reviews/pr-19.md` with the finding and the verdict `Changes required` for effective head `38aa19f`.

### State of the build

- `main` is `ee4305a`. The effective head is `38aa19f`, and the remote PR tip is `cd5ee36`.
- `make verify` passes. All 9 live CI checks pass on `cd5ee36`.
- The latest Gitar approval is for `38aa19f`, so the automated review is stale for the branch tip.
- The review and handoff commit `cd5ee36` changes metadata only, so it does not move effective head `38aa19f`. The unrelated untracked `deck-test/` remains untouched.

### In flight

P2-1 needs a correction. A current Gitar review also needs to cover the branch tip before merge.

### Traps and gotchas

- Step 1 of the one-PR skill blocks work when the bound PR is merged. Step 6 requires a prompt after that merge (D-601).
- The PR tip can move through a metadata commit while its effective head stays `38aa19f`.
- D-602 does not apply because this review session has network and writable Git metadata.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 71.

### Open questions that block progress

No owner question blocks PR #19. P2-1 and the stale Gitar review block the merge.

### Next concrete action

Correct P2-1, push the correction, complete the Gitar pass, then repeat the Codex review (D-582).

## Session 69: 2026-09-17, Claude Code

Author: Claude Code
Session: the owner answer for the review gate of PR #19, in the same session as Sessions 64, 66, and 68 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- Two review rounds of PR #19 gave no finding and no verdict, because the review session has no GitHub access and a read-only `.git` directory.
- The session asked the owner and gave four options. The owner chose the verdict from the local evidence, and asked for the rule in the `pr-review` skill.
- D-602 records the rule. A review session with no network can give `Ready for owner merge` when the local branch holds the effective head, the review reads the whole diff, and each local check runs. The record marks the evidence of the author and lists each item that the session cannot verify.
- The `pr-review` skill gets the section "A review with no network", and the Verification and Verdicts sections point to it.
- The change moves the effective head, because it changes `docs/decisions.md` and `.claude/skills/`. The PR needs a new Gitar pass and a new review round.

### State of the build

- `main` is `ee4305a`. The new effective head is the head of this round.
- The ten checks pass on `38aa19f`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes for the new head: the reply "Running the review now" came at `12:29:51Z`, and the new dashboard comment `5714387972` has the edit time `12:31:10Z`.
- `make verify` passes on the Mac of the owner.

### In flight

The review round of PR #19 at the effective head `38aa19f`, which gives the verdict under D-602.

### Traps and gotchas

- A decision that comes from a review can move the effective head. This round does, so the earlier Gitar pass and the earlier review rounds do not cover it.
- Automatic Gitar reviews are paused on this trial, so the new head needs a `Gitar review` comment after the push wait.
- D-602 gives no permission to skip a local check. A session that cannot run a check names it in the record.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 70.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the review round of PR #19 from Codex at the effective head `38aa19f`, under D-602.

## Session 68: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #19, in the same session as Sessions 64 and 66 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The repeat review of Session 67 gives no finding and keeps the `Blocked` verdict. The reason is the environment of the review session: no GitHub access, and a read-only `.git` directory.
- This round commits the updated `docs/reviews/pr-19.md` and the Session 67 entry with no change to their text, because that session cannot commit or push.
- `docs/reviews/pr-19-response.md` gets a section for the repeat review. It corrects two points of the record: the response file is the work of the author and not of the owner, and the record names the repository `natekramber/the-thing-below`, which does not exist.
- The round moves the Session 57 and Session 58 entries to `docs/session-handoff-archive.md`, because the file keeps the 10 newest entries (D-18).
- Two review rounds now give no finding and no verdict. The session asked the owner how to unblock the gate of T-4.

### State of the build

- `main` is `ee4305a`. The effective head is `c065a11`, and this round changes `docs/reviews/` and `docs/session-handoff.md` alone.
- The nine CI checks pass, and the Gitar pass approves `c065a11` with no finding.
- `make verify` passes on the Mac of the owner.

### In flight

The answer of the owner about the review gate of PR #19.

### Traps and gotchas

- The review session of this machine has no network and a read-only `.git` directory. A review in that environment can read the diff, and it cannot verify the PR, the comments, or the checks.
- An author cannot clear a `Blocked` verdict, and evidence from the author is not independent evidence for the reviewer.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 69.

### Open questions that block progress

The review gate of PR #19 blocks the merge. The session asked the owner, and the answer becomes D-602 or OQ-183.

### Next concrete action

Get the answer of the owner about the review gate, then apply it to PR #19.

## Session 67: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `c065a11`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Re-read the review instructions and the existing PR #19 review record and response file.
- The local branch advanced to `d6d1529`. Commits after `c065a11` only change review and session handoff records, so the effective head remains `c065a11`.
- The response file reports the PR head, Gitar result, comments, checks, and build result. GitHub access failed again, so this session could not verify those claims independently.
- The review record still gives `Blocked`. It also records that the Git index is read-only, so this session cannot commit or push its update.

### State of the build

- `main` points to `ee4305a`. The local branch and its tracking ref point to `d6d15294f7fe4772e693ccf949a0154056102a1d`.
- The effective head remains `c065a11` because later commits change metadata only.
- `git diff --check` passes for the PR changes. The previous session's STE and identity checks pass; its build result remains unknown from this environment.
- GitHub checks, comments, PR state, and live remote head remain unverified.

### In flight

The review remains blocked until GitHub evidence can be verified and the updated record can be committed and pushed.

### Traps and gotchas

- `gh` cannot connect to `api.github.com`.
- `git fetch` cannot write `.git/FETCH_HEAD`, and Git cannot create `.git/index.lock`.
- The response file is author-provided evidence, not independent confirmation by this review.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 68.

### Open questions that block progress

No owner question blocks PR #19. GitHub access and Git metadata write access block the final review publication.

### Next concrete action

Restore GitHub API and Git write access. Verify the PR comments and checks, then commit and push the existing review record and this entry.

## Session 66: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #19, in the same session as Session 64 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review of Session 65 gives no finding on the change. Its verdict is `Blocked`, because that session had no GitHub access, no writable `.git/FETCH_HEAD`, and no build result.
- `docs/reviews/pr-19-response.md` gives each piece of evidence that the review session could not get: the head, the state, the comment export, the thread count, the nine checks, and the result of `make verify`.
- The review session could not push. This round commits `docs/reviews/pr-19.md` and the Session 65 entry with no change to their text, so the PR holds each record of its review (D-577).
- The round moves the Session 55 and Session 56 entries to `docs/session-handoff-archive.md`, because the file keeps the 10 newest entries (D-18).

### State of the build

- `main` is `ee4305a`. The PR head on GitHub is `0bcd246`, the state is `OPEN`, and the merge state is `CLEAN`.
- The effective head is `c065a11`. This round changes `docs/reviews/` and `docs/session-handoff.md` alone, which is the metadata set, so the effective head does not move.
- The nine CI checks pass, and the Gitar pass approves `c065a11` with no finding. The comment export gives one comment, which is the Gitar dashboard, and zero review threads.
- `make verify` passes on the Mac of the owner in 9 seconds with a warm build.

### In flight

The repeat review of PR #19 at the effective head `c065a11` (T-4, D-17). Only the reviewer can set the verdict.

### Traps and gotchas

- A `Blocked` verdict can name no defect. This one names missing evidence of the review session, and the author cannot clear it.
- The review session read 30 seconds of `dotnet build` as an unknown result. A cold build takes longer than that, and a restore with no network fails.
- A metadata commit does not move the effective head, and it needs no new Gitar request. Automatic Gitar reviews are paused on this trial, so a request costs one of a limited set.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 67.

### Open questions that block progress

No owner question blocks PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3. OQ-182 blocks nothing.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `c065a11`.

## Session 65: 2026-09-16, Codex

Author: Codex
Session: review of PR #19 at effective head `c065a11`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the handoff, the one-PR, PR-review, project-contract, review-record, commit, and STE instructions.
- Verified the provider gate. Session 64 names Claude Code as the PR author, so Codex is eligible under T-4 and D-17.
- Inspected all eight changed paths and checked the D-600 path set against the workflow and the equal agent files against D-20.
- The STE check and whitespace check pass. The build did not finish within 30 seconds.
- GitHub API access failed. The PR description, current comments, live checks, and remote head could not be verified.
- The review record gives `Blocked` for effective head `c065a11`, because required current PR evidence is unavailable.

### State of the build

- `main` points to `ee4305a`. The checked-out PR branch and its tracking ref point to `0bcd246`.
- The effective head is `c065a11`; later commits only change metadata.
- The STE check gives zero findings, the diff check passes, and `CLAUDE.md` equals `AGENTS.md`.
- The build result is unknown. GitHub checks and the current remote head were not available.
- The unrelated untracked `deck-test/` directory remains untouched.

### In flight

The PR review is blocked on GitHub evidence and cannot reach its hand-over point.

### Traps and gotchas

- `git fetch` failed because `.git/FETCH_HEAD` is not writable in this environment.
- `gh` could not connect to `api.github.com`, so the required comment export produced no file.
- Do not treat handoff reports of a green Gitar pass and CI as independent verification.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 66.

### Open questions that block progress

No owner question blocks PR #19. The missing network evidence blocks a final review verdict.

### Next concrete action

Restore GitHub API access, export the PR description and all comments in one command, verify the live checks and remote head, then update this review record.

## Session 64: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-83, the decision row of the docs-only skip set and the transitional prompt rule.
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19, which is PR-83 of the roadmap. Role: author. Base: `ee4305a`.

### What this session did, and why

- PR #18 merged as `ee4305a`. Its `changed-paths` job holds a skip set, and no decision recorded the paths. The reasoning was a comment of `.github/workflows/ci.yml` alone, and no file in `docs/` named it.
- D-600 records the skip set: `docs/`, `.claude/`, `README.md`, `LICENSE`, and `.github/pull_request_template.md`. `CLAUDE.md` and `AGENTS.md` stay out of it, because `AgentFileTests` reads both files and fails when they differ (D-20). A skipped job reports `Success`, so a skip of the agent files would pass the PR shape that breaks D-20 most often.
- The skip set and the override set of D-16 are not the same set. The override set holds both agent files, and `LICENSE` is in the skip set alone.
- The comment of the `changed-paths` job and a bullet of `docs/roadmaps/area-ci.md` now cite D-600.
- D-601 records the transitional prompt. After the owner says `Merged PR #x` for the PR of the session, the session writes one fenced block for the next clean session, and then it ends. `CLAUDE.md` and `AGENTS.md` hold the rule, and step 6 of the `one-pr-one-session` skill holds the template.
- The owner approved the two concerns of this PR before the work started. G-8 refuses a second concern without that answer.

### State of the build

- `main` is `ee4305a`. The branch `docs/pr-83-skip-set-decision` sits on that base.
- `make verify` passes on the Mac of the owner: the build, 8 tests, the format check, the STE check, and the smoke session.
- The PR adds two decision rows and changes `.github/workflows/`, so the `review-override` label does not apply (D-401, D-560). The PR needs the Codex review.
- The nine CI checks pass on `c065a11`, and the Gitar pass approves that head with no finding. The head of command B is `c065a11`, and the dashboard edit at `2026-09-17T03:55:13Z` is later than the push at `2026-09-17T03:54:29Z`. The PR has zero review threads.
- The build and test legs ran on this PR and did not skip, because the PR changes the agent files. This is the rule of D-600 at work.
- The change to `CLAUDE.md` and `AGENTS.md` keeps the two files identical, and it takes both files out of the skip set of D-600. The build and test job runs on this PR.

### In flight

The Codex review of PR #19 at the effective head `c065a11` (T-4, D-17). The commit of this record changes `docs/session-handoff.md` alone, which is in the metadata set, so it does not move the effective head.

### Traps and gotchas

- The session verified each claim of D-600 against the code: the `case` pattern of the job, the paths that `AgentFileTests` reads, and the run time of the three legs on PR #18, which was 37 to 79 seconds. The `ste-check` job holds no skip condition, so a docs PR still gets the STE check.
- `CLAUDE.md` is 16823 bytes, and the size check of OQ-182 proposes a limit of 16 KB. The file passed that limit on `main` at `ee4305a`, before this PR. OQ-182 has no answer, and no check exists.
- The uncommitted work of the merged branch `feat/pr-1-scaffold` is in a git stash of this machine. The patch of the owner replaced it. Drop the stash after the merge.
- A skipped job reports `Success`. A path rule that is too wide passes a PR that ran no check.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 65.

### Open questions that block progress

None for PR-83. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3. OQ-182 blocks nothing. The owner has not run the Deck test, so PR-82 waits and the renderer stays provisional (D-599).

### Next concrete action

Hand PR #19 to Codex for the review, and write `docs/reviews/pr-19.md`.
