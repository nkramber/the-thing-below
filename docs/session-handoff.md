# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

## Session 57: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold, which is the first code of the repository.
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: PR-1. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read the top handoff entry, `CLAUDE.md`, and the `one-pr-one-session`, `ste-writing`, `design-doc-style`, and `csharp-conventions` skills.
- Read `deck-test/handover.md` and `deck-test/readme.md` on `spike/deck-test`. That step has no PR, so no document held its eight owner answers.
- Wrote D-592 to D-599 for those answers, and resolved OQ-75, OQ-76, OQ-77, OQ-78, OQ-83, OQ-92, and OQ-93.
- D-599 revises D-160 in part. The renderer pick comes after PR-1, and PR-1 sets Forward+ as a provisional renderer. PR-82 sets the picked renderer in one line.
- Added the cost model rows of the Deck test to section 4 of `docs/design.md`, with M-7 for the frame time of the test scene.
- Added the PR-82 entry to `phase-1-foundations.md`, section 7.3, and renumbered the later entries of section 7.
- Built the scaffold: `TheThingBelow.slnx`, `global.json`, `Directory.Build.props`, the Makefile, the pre-commit hook, the four projects of D-217, and the CI workflow.
- Wrote eight tests: the agent-file match test, three Core reference tests, and four tests of the tools command line.

### State of the build

- `main` is `9f27f12` (PR #17). The branch `feat/pr-1-scaffold` holds the scaffold.
- `make verify` passes on the Mac of the owner: the build, the 8 tests, `dotnet format`, the STE check with 0 findings, and the Godot smoke session.
- The smoke session prints the renderer as `forward_plus` and the frame as 1280 by 720, and it ends with no error.
- The CI workflow holds five jobs: `changed-paths`, `build-test-format`, `coverage`, `smoke`, and `ste-check`. No leg ran them yet.

### In flight

The push of this branch, the gitar pass, and then the Codex review (T-4, D-17). The PR adds decision rows, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- The Godot editor writes `net8.0` into a `.csproj` that holds no target framework, over `Directory.Build.props` (F-60). The Game project pins `net10.0` in its own file, and `.gitignore` holds `*.csproj.old`.
- The Godot editor build gives an exit code of 0 when its build callback fails. The smoke job reads the log for `build callback failed` (F-60, T-2).
- A coverage run instruments the Core copy in the test output folder and adds `System.Threading` to it (F-61). The reference test reads the file that the Core project built.
- The compiler writes no metadata entry for a project reference that no code uses. A second test reads the Core project file, so an added reference fails (F-61).
- The generated entry point of `xunit.v3` runs the console runner unless `UseMicrosoftTestingPlatformRunner` is on. The console runner reads no Coverlet option.
- Coverlet 10 takes `--coverlet`, and not `--coverage`. It writes its file to the results directory, and it takes no output path.
- The owner ran no Deck test yet. PR-82 waits for that run.
- The next ids are D-600, OQ-183, F-62, L-16, G-27, PR-83, M-8, and Session 58.

### Open questions that block progress

None for PR-1. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push the branch, open PR-1, then answer the gitar pass and hand the PR to Codex.

## Session 56: 2026-09-16, Claude Code

Author: Claude Code
Session: the Deck test of step 7.1 of `docs/roadmaps/phase-1-foundations.md`. That step has no PR and no review.
Repository: the-thing-below. Branch: `spike/deck-test`. PR: none. Role: author of a spike.

### What this session did, and why

- Built the throwaway test scene of D-160 and D-523 on the branch `spike/deck-test`, which never merges (D-597).
- The scene draws the load of D-160 at the frame of 1280 by 720: normal maps, point lights with shadows, glow, the four ambient kinds, fog, the CRT pass, and a wipe transition.
- The sweep runs 20 stages. Each stage holds 60 warm-up frames and 300 measured frames.
- `scripts/FrameMeter.cs` reads the time of each frame and counts each frame over 16.667 milliseconds (D-598).
- Ran the fetch script for the export templates, and the SHA-512 matched (D-596).
- The owner answered eight questions. `deck-test/handover.md` holds each one, and PR-1 records them as D-592 to D-599.

### State of the build

- The scene, the shaders, the meter, and the report all work. A run on the Mac proved them.
- The native Linux export `build/DeckTest.x86_64` exists. No machine ran it.
- `main` held no code during this session.

### In flight

The run on the Deck, which the owner does. That run answers D-160 and gives the first effect budget of D-523.

### Traps and gotchas

- The Godot export needs a solution file beside `project.godot`. With none, the export writes an ELF file, exits 0, and packs no managed assembly.
- An exit code of 0 hides an export fault. The export gives `completed with warnings` and exits 0. PR-54 must read the log.
- macOS caps the frame rate whatever the vsync setting says. A Mac run reports 16.67 milliseconds in every stage, and the report refuses to give a budget (T-2).
- Only the Deck run answers D-160. A Mac run tests the scene and the report, and nothing else.
- Godot drops each light past 15 on one canvas item with no message (F-46). The sweep stops the light row at 15.

### Open questions that block progress

None. OQ-92 and OQ-93 closed with D-597 and D-598.

### Next concrete action

The owner copies `build/DeckTest.x86_64` and `run-deck-test.sh` to the Deck and runs `./run-deck-test.sh`. PR-82 then sets the renderer.

## Session 55: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #16 at effective head `e3e6611`, after the owner confirmed Gitar approval.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the current handoff and the repeat-review, review-record, commit, and STE instructions.
- Verified that the local branch and fetched remote branch both point to `e3e6611`. This commit changes the runbook, so it is the effective head.
- Verified the provider gate. Claude Code authored the PR, and Codex reviews it (T-4, D-17).
- Reproduced P2-2 in a scratch repository under `set -e`. The corrected command committed review and handoff records with no eligible STE file.
- Set P2-2 to fixed in `e3e6611`. The full interim STE check, diff check, and identity check pass.
- The owner confirmed Gitar approval of the current changes. GitHub API access failed, so the dashboard comment and check query could not be read independently.
- Updated `docs/reviews/pr-16.md`, preserved the earlier verdicts, and set the current verdict to `Ready for owner merge` for `e3e6611`.

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`. The review and handoff commit `86b76b6` was pushed and verified as the remote head. The effective head remains `e3e6611`.
- The current verdict is `Ready for owner merge`, based on the fixed findings and the owner's Gitar confirmation.

### In flight

The review record and this entry are committed and pushed to PR #16. This entry records the verified push.

### Traps and gotchas

- GitHub API access failed during this session. The owner confirmed the current Gitar approval.
- The remote fetch succeeded. The PR page and comment API did not respond.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 56.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push this metadata update, fetch the remote, and verify the clean branch status and PR head.

## Session 54: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #16, in the same conversation as Sessions 50 and 52 (D-582).
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56`.

### What this session did, and why

- The owner asked the session to address the review feedback again. The repeat review of Session 53 set P2-1 to fixed and gave `Blocked` for head `d2479ce`, with P2-2 open.
- Before that review, a manual Gitar review approved `d2479ce` with 0 findings. The first wait stopped at the "On it" reply, and the second at the new dashboard comment, which had a new id.
- P2-2 has partial merit. The runbook block commits in a plain run with dated records alone. It made no commit under `set -e`, or with the `files=` line joined by `&&`, in bash and in zsh. Both providers ran it in a joined form.
- The filter now treats a `grep` status of 1 as an empty list, and a status of 2 still fails (D-585, T-2). The regression check fails on the old text under `set -e` and passes on the new text in each of the eight runs. A checker finding still stops the commit.
- The stale Gitar review has full merit, and a new request follows this push. The absent check runs have no merit as a blocker, because PR-1 and later PRs create the checks (G-16).
- `docs/reviews/pr-16-response.md` records both answers.
- The handoff held ten entries before this one, so Session 44 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry changes the runbook, so it is the new effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #16 waits for a current Gitar review of the new head, then the repeat Codex review of P2-2 (T-4, D-17).

### Traps and gotchas

- A review commit makes the Gitar review of the effective head stale for the branch head. The author requests a new Gitar review after its next push.
- A shell run with `set -e` stops at a command substitution that returns nonzero. Test runbook commands in the plain form and under `set -e`.
- Gitar can replace the dashboard comment with a new id. Read the newest dashboard comment in each check.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 55.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the new head and answers each finding. Then the Codex reviewer repeats the review of P2-2.

## Session 53: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #16 at effective head `d2479ce`.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the top handoff entry and the one-PR, repeat-review, review-record, and STE skills.
- Verified Claude Code authored PR #16 and its correction. Codex remains eligible under T-4 and D-17.
- Verified the base and head with GitHub PR metadata. The effective head is `d2479ce` because it changes the runbook.
- Reproduced P2-1 against the old and corrected runbook commands in bash and zsh. The old command returned 0 and left a partial file. The corrected command returned 1 and left no file.
- Set P2-1 to fixed in `d2479ce` and updated the existing review record.
- Found P2-2 in the staged-file commit command. With only review and handoff records staged, `grep -v` returns 1 and stops the `&&` chain before the commit.
- The current comment export confirms Gitar approved `d2479ce` with 0 findings and 0 threads. That pass predates the review metadata push to `60260ef`, so it is stale for the current branch head. GitHub reports no check runs.
- The handoff held ten entries. Session 43 moved word for word to the archive (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`, with metadata head `60260ef` and base `2bc7d56` per `gh pr view`. The effective implementation head remains `d2479ce`.
- The interim STE check, diff check, identity check, and P2-1 regression check pass.
- The P2-2 reproduction fails the documented commit chain for this metadata-only commit. A local safe empty-list handling lets the review records commit without skipping any eligible STE file.
- The review verdict is Blocked for open P2-2, the stale Gitar review, and absent check runs.

### In flight

PR #16 needs the author to fix P2-2 and push the correction. The author then gets a current Gitar review and required check results. The current review record says Blocked for effective head `d2479ce`.

### Traps and gotchas

- `git fetch` failed under the default sandbox, then succeeded with elevated access.
- The current Gitar dashboard approval is for `d2479ce`; the later review metadata push makes it stale for the branch head.
- GitHub reports no check runs for the branch.
- The documented commit pipeline fails when its path filter finds no STE-eligible staged Markdown files.
- The regression harness is `/tmp/pr16_export_regression.sh`.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 54.

### Open questions that block progress

No owner question blocks PR #16. Fresh Gitar and CI evidence remains unavailable.

### Next concrete action

The author fixes P2-2 so an empty eligible-file list does not stop the commit command. Then the author pushes the correction, requests a current Gitar review, and checks why no CI jobs report. Codex repeats the review at the new effective head.

## Session 52: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #16, in the same conversation as Session 50 (D-582).
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56`.

### What this session did, and why

- The owner asked the session to address the review feedback. The Codex review of Session 51 gave `Changes required` for head `45e960d`, with one finding, P2-1.
- Before the review, the session requested a manual Gitar review of `45e960d`. It waited with the one wait command of D-586 two times: the first wait stopped at the placeholder comment, and the second at the review. Gitar approved with 0 findings and 0 threads.
- P2-1 has full merit. The comment export of `docs/runbooks/session-context.md` returned 0 and left a comments file after a failed GitHub call. A fake `gh` reproduced it in bash and in zsh.
- The export now runs in one `&&` chain into a part file, renames the file only after every call passes, and fails with a message otherwise (T-2, D-589). The regression check fails on the old runbook text and passes on the new text in both shells. The real `gh` run saved three comments.
- `docs/reviews/pr-16-response.md` records the answer.
- The handoff held ten entries before this one, so Session 42 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry changes the runbook, so it is the new effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #16 waits for a current Gitar review of the new head, then the repeat Codex review of P2-1 (T-4, D-17).

### Traps and gotchas

- The first Gitar comment after a request can be a placeholder with the pause note and a spinner. Wait again with `since` at its time (D-586).
- The review commit of Session 51 came from the same checkout. `git fetch` alone did not show it, because the local branch already held it.
- The push line of `docs/reviews/pr-16.md` holds the placeholder `<review metadata sha>`. The response file asks the reviewer to correct it.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 53.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the new head and answers each finding. Then the Codex reviewer repeats the review of P2-1.

## Session 51: 2026-09-16, Codex

Author: Codex
Session: review of PR #16 at effective head `45e960d`.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the start set, the one-PR skill, the PR review skill, the STE skill, and the review record and commit references.
- Verified that Claude Code authored the PR from Session 50. Codex is the eligible reviewer under T-4 and D-17.
- Recomputed the effective head as `45e960d`. It is the only commit after the base and changes substantive paths.
- Inspected all 18 paths in the diff, the decisions D-583 to D-591, F-59, and OQ-182.
- Found P2-1: the PR comment export can return success after an earlier GitHub retrieval fails. A shell reproduction returned 0 after a failed command and a successful command.
- Verified the Gitar dashboard approval, its check, and the absence of review threads. Corrected the stale dashboard timestamp in the PR description.
- The full interim STE check passes with 0 findings. `git diff --check` passes, and `AGENTS.md` and `CLAUDE.md` are identical.
- The handoff held ten entries before this one, so Session 41 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`. Its remote head and effective head are `45e960d` before this review commit.
- Gitar approved this head. The cross-provider review found P2-1, recorded in `docs/reviews/pr-16.md`.
- `make verify` is unavailable because the Makefile does not exist. The direct STE and diff checks pass.

### In flight

PR #16 needs the author to fix P2-1 and run the regression check. The review record and this handoff entry are ready to commit and push.

### Traps and gotchas

- The comments export must fail if any API request fails. Otherwise a partial file can appear complete.
- The PR description timestamp now matches the Gitar dashboard update at 21:23:53Z.
- The review applies to effective head `45e960d`, not the metadata tip that will publish this record.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 52.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author makes each failed GitHub retrieval fail the comment-export command, tests that a partial export returns nonzero, and requests a repeat Codex review after the fix.

## Session 50: 2026-09-16, Claude Code

Author: Claude Code
Session: the token audit of the repository, then PR #16, the session context budget.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56` (PR #15).

### What this session did, and why

- The owner asked for a read-only audit of token use. The audit read the usage records of 10 Claude Code sessions and 10 Codex sessions of this repository, and the size of every instruction file.
- Each harness call sends the whole context again. The Claude Code sessions sent a median of 350k tokens in each call, and a maximum of 886k, with no context compaction. Whole reads of the read order held about 29% of the carried context. A call for each STE check cost 19% of the input tokens, and a call for each GitHub poll cost 14%. `CLAUDE.md` cost about 1.5%.
- The owner said "Implement all fixes as recommended." D-583 to D-591 record the answers. OQ-182 asks where a size check goes. F-59 records the finding.
- The start set replaces the whole read order at the start (D-583, D-584). The STE check runs in the commit command (D-585). One command waits for Gitar (D-586). The session tells the owner when it is ready for a context compaction (D-587).
- `pr-review` split into a core of 17,922 bytes and five reference files (D-588, D-589). The glossary of the project areas moved word for word to `ste-writing/references/glossary.md` (D-590). D-591 covers scripts and edits.
- `docs/runbooks/session-context.md` holds the evidence and the commands. Each command ran on this machine, in zsh.
- The auto mode classifier of the harness refused the edits of the session skill and of `CLAUDE.md` two times. The owner then approved the edits in the conversation.
- A separate evaluator ran the changed rules on four requests and found five defects. All five had merit, and this PR fixes them: a handoff push during the Gitar wait, the lost full STE check, unset variables in the comments command, the provider gate against D-584, and the wait after "On it".
- The shared `gitar-review` skill did not change, because it is the same file in each repo.
- The handoff held ten entries before this one, so Session 40 moved word for word to the top of `docs/session-handoff-archive.md` (D-18). A repeated rule line between two entries left this file.

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry is its effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The start set fell from 55,786 bytes to 28,491 bytes, about 20.7k to 10.6k tokens at 2.7 bytes for each token. `CLAUDE.md` grew from 15,001 to 16,052 bytes.

### In flight

PR #16 waits for a Gitar pass and for the Codex review, because it adds decision rows (T-4, D-17, D-401). For this work the owner told the session to set aside the Gitar procedure, so no Gitar request ran.

### Traps and gotchas

- The shell of this machine is zsh. zsh does not split `$files` into words, so the commit command pipes the file list to `xargs`.
- The harness gives the compaction command to the owner alone. A session cannot compact itself, so D-587 tells the owner.
- A push while Gitar reviews makes the review stale. Commit the handoff entry of a round before the push of that round.
- The checker does not read the glossary. A session that writes about a project area loads `references/glossary.md` (D-590).
- The audit scripts lived in the scratch folder of the session. No tool of this repository reads the usage records (D-99).
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 51.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the PR #16 head and answers each finding. Then the Codex review of PR #16 runs.

## Session 49: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #14 at effective head 7e65c7e.
Repository: the-thing-below. Branch: docs/pr-14-one-pr-one-session. PR: #14. Role: reviewer. Base: 26152c5.

### What this session did, and why

- Read the handoff first, then the one-PR, review, STE, and gitar skills. Read the prior review, its response, the complete correction diff, the affected contracts, the PR description, and all PR comments.
- Verified the provider gate under T-4 and D-17. Session 46 identifies Claude Code as the author. Session 48 identifies Claude Code as the correction author. Codex is the eligible reviewer.
- Recomputed the effective head. 7e65c7e changes substantive paths. The review and handoff commits change only metadata paths.
- Reproduced P2-1 and checked its correction. The gate rejects a deferral of this PR's own documents or records, and it permits a line that names PR-3 as the owner of independent roadmap work. D-579, the PR template, and PR-3 exit tests 10 and 11 agree.
- Checked the D-582 session-end changes and the author and reviewer instructions. They agree with the revised decision.
- Verified the latest manual Gitar review after the correction push. Its dashboard reports approval, its check passes, and no review thread or formal PR review remains.
- Updated docs/reviews/pr-14.md, kept the earlier Changes required verdict under Earlier verdicts, and set the current verdict to Ready for owner merge for 7e65c7e.
- Corrected the PR description's other-provider checkbox after recording the verified verdict.
- The handoff held ten entries before this one, so Session 39 moved word for word to the top of docs/session-handoff-archive.md (D-18).

### State of the build

- No code, solution, or Makefile exists. main is 26152c5 (PR #13).
- PR #14 is open on docs/pr-14-one-pr-one-session. Its remote tip before this review is 7e65c7e, and its effective head is 7e65c7e.
- The interim STE check passes with 0 findings, git diff --check passes, and CLAUDE.md and AGENTS.md stay identical.
- Build and test commands did not run because the repository has no code, solution, or Makefile.

### In flight

The cross-provider review is ready for owner merge at effective head 7e65c7e (T-4, D-17). The PR still needs a current Gitar review of its latest metadata tip before merge. The last Gitar dashboard edit predates the review publication, and GitHub reports no check on the new tip.

### Traps and gotchas

- Completion line 8 now applies only to documents and records of the current PR. A line that assigns independent roadmap work to its owner PR passes under D-579 and G-16.
- P2-1 stays in the review history as fixed. The earlier verdict remains under Earlier verdicts.
- The next ids are D-583, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 50.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author requests a current Gitar review of the latest PR tip and answers each finding. P2-1 is fixed; the Gitar review is the remaining merge gate.


## Session 48: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #14, on branch `docs/pr-14-one-pr-one-session`. Role: author, in the same conversation as Session 46 (D-582). Base: `26152c5`.

### What this session did, and why

- The owner asked the session to answer the review of Session 47. The skill of this PR blocked the answer, because the author session had reached its hand-over point. The owner said: "Addressing Codex/gitar review feedback does NOT qualify for a new session."
- D-582 records that rule and revises D-576 in part. The author session now answers each gitar comment and each review, and it ends at `Ready for owner merge` or the label.
- P2-1 has full merit. Completion line 8 rejected any line that names another PR, so it rejected PR #14 and the PR template. The gate now rejects only a document or a record of this PR that waits for another PR.
- The same boundary reaches `CLAUDE.md`, `AGENTS.md`, the PR template, D-579, and the PR-3 scope. PR-3 gains exit test 11, which passes a PR that names the PR of an absent check.
- `pr-review`, the glossary, and `docs/design.md` follow D-582.
- `docs/reviews/pr-14-response.md` records both answers.
- A separate evaluator ran the corrected skill on seven requests, the two fixtures of the review included. The deferral failed, and the PR that names PR-3 passed. Its notes found a clash that the first correction made: the reviewer could repeat a review, yet it ended at its first record. The reviewer now ends at the same verdict as the author. A correction author comes from the provider of the author (T-4).
- The handoff held ten entries before this one, so Session 38 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. The commit that holds this entry changes paths outside the metadata set, so it is the new effective head.
- The interim STE check passes with 0 findings, the skill validator passes, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- Size: `CLAUDE.md` is 15,004 bytes, and the skill is 8588 bytes.

### In flight

PR #14 waits for a current gitar review of the new head, then the repeat Codex review of P2-1 (T-4, D-17). This author session stays bound to PR #14 and answers each finding (D-582).

### Traps and gotchas

- A line that names the PR of independent roadmap work is not a deferral (G-16). Only a document or a record of the current PR can defer.
- D-579 changed its text inside this PR, before any merge. The response file says why.
- The author session and the reviewer session each end at `Ready for owner merge` for the effective head, or at the label, not at the request for a review (D-582). Each round of a session adds a new handoff entry.
- The next ids are D-583, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 49.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get a current gitar review of the new head and answer each finding. Then a Codex session repeats the review of P2-1 from `docs/reviews/pr-14-response.md`.
