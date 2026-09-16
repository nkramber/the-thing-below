# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

## Session 47: 2026-09-16, Codex

Author: Codex
Session: review of PR #14 at effective head `9837c1d`, on branch `docs/pr-14-one-pr-one-session`. Role: reviewer. Base: `26152c5`.

### What this session did, and why

- Read the handoff first, then the review, session, STE, and gitar skills. Read the complete diff, the affected contracts, the PR description, and every PR comment.
- Verified the provider gate under T-4 and D-17. Session 46 identifies Claude Code as the author of the substantive change.
- Recomputed the effective head. `9837c1d` changes the substantive paths. The later commit `6bafb15` changes the handoff alone.
- Applied the new skill to this PR and the standard PR template. Found P2-1: completion line 8 rejects any line that gives work to another PR.
- Verified the trigger against D-579 and G-16. PR #14 assigns the machine enforcement to PR-3, and the template names each PR that creates an absent check.
- Inspected the external GitHub claim. The official events page lists the `edited` type for `pull_request_target`.
- Verified the current gitar pass. The dashboard edit follows the second request, the check succeeded, and no review thread exists.
- Wrote `docs/reviews/pr-14.md` with the verdict `Changes required` for effective head `9837c1d`.
- The handoff held ten entries before this one, so Session 37 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. Its effective head is `9837c1d`.
- The interim STE check passes with 0 findings. Both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- D-1 through D-581 and OQ-1 through OQ-181 are gap-free.

### In flight

PR #14 needs the P2-1 correction. The review record applies to effective head `9837c1d` (T-4, D-17).

### Traps and gotchas

- D-577 forbids a later PR from carrying a document of the current PR. It does not forbid a roadmap from assigning independent work to its owner PR.
- D-579 assigns the machine enforcement to PR-3. G-16 requires each absent check to name its creator PR.
- The correction must keep the negative deferral case and add the valid future-owner case. A word search for `later PR` cannot decide the meaning alone.
- The next ids are D-582, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 48.

### Open questions that block progress

None for P2-1. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author limits completion line 8 to the documents and records of the current PR. The response adds both regression fixtures, and a new clean Codex session repeats the review.

## Session 46: 2026-09-16, Claude Code

Author: Claude Code
Session: PR #14, the rule of one PR in one session, on branch `docs/pr-14-one-pr-one-session`. Role: author. Base: `26152c5`.

### What this session did, and why

- The owner asked for one clean session for each PR, and for each PR as the complete unit of its work, to keep the context of each session small. The same instruction went to three other repositories of the owner. Each of those repositories gets its own session and its own PR.
- The harness of this session had no tool to start a top-level session in another project. So this session wrote this repository alone, and it gave the owner the prompts for the other three.
- The new skill `.claude/skills/one-pr-one-session/SKILL.md` holds the session binding, the start gate, the documents gate, the merge facts, the completion gate, and an enforcement table.
- `CLAUDE.md` and `AGENTS.md` require the skill before any PR work. The D-18 line "A documentation PR can follow the merge" is gone. The PR gate line on documents points at the skill.
- The owner answered three questions: the document rules go into PR-3 (D-579), a docs PR with its own concern stays allowed (D-580), and the Documents lines use three STE forms (D-581). D-576 to D-578 record the owner instruction. D-18 and D-15 carry `Revised in part` notes.
- `docs/design.md` gains the session pass line, F-58, and G-26. The PR-3 line of Phase 1 and the sequence position change.
- The PR-3 entry of `phase-1-foundations.md` gains scope lines, exit tests 8 to 11, and review focus lines. `area-tools.md` and `area-ci.md` follow, and `area-tools.md` gains a dated fact on the `edited` type of `pull_request_target`.
- `pr-review` loads the new skill, checks the Documents section, and ends the session after the end gate. The glossary gains "clean session", "Documents section", and "hand-over point". The PR template gains the rows and a no-deferral line.
- A forward test with a separate evaluator ran the skill on ten realistic requests. Each of the ten gave the result that the rule needs. Its notes added a refusal result for a PR that records an earlier PR, the gates of a reviewer, a meaning of substantive work, a rule for a second concern, and the form of each line. It also found a gap in the metadata set of `pr-review`, which OQ-181 holds.
- The handoff held ten entries before this one, so Session 36 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. Its effective head is `9837c1d`. The later commit that records the gitar pass changes this file alone, so it is a metadata commit.
- The gitar pass on `9837c1d` approved with no comment: 0 comments, 0 with merit, and no thread. The dashboard edit came after the push, and its summary names this change.
- The interim STE check passes with 0 findings, the skill validator passes, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- Size: `CLAUDE.md` grows from 14,735 to 14,976 bytes. The skill is 7,550 bytes, and a session loads it for PR work alone.

### In flight

PR #14 waits for the Codex review at effective head `9837c1d`. It changes rows of `docs/decisions.md`, so it takes the review and not the label (D-401). A manual gitar review of the metadata commit confirms that the pass is current. This session ends at that hand-over point (D-576).

### Traps and gotchas

- No check can see the conversation of a session (F-58). The binding and the clean start stay agent-enforced and owner-enforced. PR-3 enforces the document rules from its merge on (D-579).
- The handoff says "open" for a PR that the owner merges later. Git holds the merge. Do not open a PR to correct it (D-578).
- A PR-3 session must keep the rows of the skill table and the rows of the command in step. Exit tests 8 to 11 of PR-3 name them.
- The next ids are D-582, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 47.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

A new clean Codex session reviews PR #14 at its effective head under the `pr-review` skill (T-4, D-17). Then the owner merges.

## Session 45: 2026-09-16, Claude Code

Author: Claude Code
Session: PR #13, the link from the guidance to the new `gitar-review` skill, on branch `docs/pr-13-gitar-review`.

### What this session did, and why

- The owner added the shared skill `.claude/skills/gitar-review/SKILL.md` and asked that `pr-review` and the other guidance link to it, with no copy of its procedure and no wrong text.
- The skill adds the proof that a review is current. The older text in `CLAUDE.md` and `pr-review` asked for `Gitar review` only on a pause, and it did not check that the review covers the head.
- `CLAUDE.md` and `AGENTS.md`: the skill list names `gitar-review`. The section "Automated review pass" points to the skill and keeps only the rules of this repo. The PR gate line asks for a current review.
- `pr-review`: the section "The automated pass" points to the skill and keeps the rules of this repo. The reviewer checks with the read commands of the skill that the pass is current.
- `.github/pull_request_template.md`: the gitar line asks for a current review.
- The commit adds the skill file as the owner wrote it. It passes the STE check with 0 findings.
- The owner answered two questions. No decision row records the skill, so `docs/decisions.md` stays as it is. The repo keeps the spelling `gitar`, and the shared skill keeps `Gitar`.
- The handoff held ten entries before this one, so Session 35 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `b292624` (PR #12).
- PR #13 is open on branch `docs/pr-13-gitar-review`. Its head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #13 waits for a current gitar review under the `gitar-review` skill. It changes paths in the override set alone and no decision row, so the session applies the `review-override` label after the pass approves the head (D-16, D-67, D-401).

### Traps and gotchas

- The `gitar-review` skill is the same file in each repo. Do not edit it here. Put a rule of this repo in `CLAUDE.md` or `pr-review`.
- A rule of this repo wins over the skill. Step 20 of the skill tells the owner that the PR is ready to merge. Here the PR goes to the other provider, or it takes the label.
- The D-14 row still names the pause as the trigger for `Gitar review`. The owner chose no decision row, so the skill carries the wider trigger.
- The dated records keep the old procedure text. Do not correct them.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 46.

### Open questions that block progress

None for PR #13. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

Get a current gitar review of PR #13 and answer each finding under the `gitar-review` skill. Apply the `review-override` label after the pass approves the head. Then the owner merges.

## Session 44: 2026-09-16, Codex

Author: Codex
Session: third repeat cross-provider review of PR #12 at effective head `ff04f87`.

### What this session did, and why

- Read the current handoff first, then the review response, the review and STE skills, the correction diff, the affected contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 43 identifies Claude Code as the author of the substantive P2-7 correction.
- Recomputed the effective head. `ff04f87` is the newest substantive commit. The later commits `54edbe5` and `ef5a8b0` change only review and handoff metadata.
- Reproduced P2-7 and its regression check. The art and effects ownership rows now name PR-81 and D-575, and the PR-81 budget test now covers each map and each battle place under D-523.
- Checked the adjacent group scopes. The PR-35 and PR-17 exclusions now include PR-81. The remaining narrower references either record historical text or give PR-81 its own row.
- Verified the automated finding and its correction. The final pass on `ef5a8b0` confirms the four-file count, and every review thread is resolved.
- Updated `docs/reviews/pr-12.md`, preserved the three earlier verdicts, and set the current verdict to `Ready for owner merge` for `ff04f87`.
- Corrected the stale other-provider checkbox in the PR description after the verdict became current.
- The handoff held ten entries before this one, so Session 34 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on branch `docs/pr-12-critic`. Before this review commit, its remote tip is `ef5a8b0`, and its effective head is `ff04f87`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run. The automated pass reports approval with three closed findings and no open finding.

### In flight

PR #12 is ready for owner merge. The review record applies to effective head `ff04f87` (T-4, D-17).

### Traps and gotchas

- The verdict covers the effective head `ff04f87`, not the later metadata tip.
- D-523 applies the effect budget to each map and each battle place. The boss of a one-map dungeon still creates a battle place.
- A new PR that joins a named group must join its ownership tables and its scope limits.
- OQ-180 blocks PR-81, not PR #12.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 45.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

The owner can merge PR #12.

## Session 43: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the second repeat review of PR #12, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read the second repeat review of Session 42. It set P2-6 to `fixed in 0fbfa82`, and it added P2-7 with the verdict `Changes required` for head `0fbfa82`.
- P2-7 has full merit. The art and effects tables gave the later places to PR-23 to PR-27 alone, and the budget exit test of PR-81 left out its battle place, which the boss of D-575 needs.
- The art row and the effects row now name PR-81 and cite D-575. The budget test of PR-81 now uses the boundary of the other dungeon builds: each map and each battle place.
- A scan for the same class found two more lines in Phase 2, the scope limits of PR-35 and PR-17. The sealed door is a story gate on the region map, so both lines now name PR-81.
- `docs/reviews/pr-12-response.md` gained the answer of this round.
- The handoff held ten entries before this one, so Session 33 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The correction changes four roadmap files outside the metadata set, so the effective head moves off `0fbfa82` to the commit of this answer.
- The automated pass on `ff04f87` reported `Approved with suggestions`, with one finding. It had merit: the response and this entry said three roadmap files, and the commit changed four. The same count was wrong for the first round too, and a follow-up commit corrected all three lines.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #12 waits for the repeat cross-provider review of P2-7 at the effective head `ff04f87` (T-4, D-17). The automated pass is complete, and no comment of it waits for an answer (D-14, D-66).

### Traps and gotchas

- A new PR that joins a group of PRs must join every table and every scope limit that names the group. PR-81 joined the sequence first and the ownership tables later.
- The budget test of a dungeon covers each map and each battle place (D-523).
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 44.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `ff04f87`. It reads the P2-7 section of `docs/reviews/pr-12-response.md`, checks the trigger and the regression check, and writes the verdict (T-4, D-17).

## Session 42: 2026-09-16, Codex

Author: Codex
Session: second repeat cross-provider review of PR #12 at effective head `0fbfa82`.

### What this session did, and why

- Read the current handoff first, then the review response, the review and STE skills, the correction diff, the changed contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 41 identifies Claude Code as the author of the D-575 correction.
- Recomputed the effective head. `0fbfa82` is the newest substantive commit, and `74f9477` changes only the handoff metadata.
- Reproduced P2-6 and its regression check. D-575 fixes the dungeon count and classification in the decisions, design, roadmaps, and world files.
- Found one new adjacent contract defect, P2-7. The PR-81 budget test omits its boss battle place, and the art and effects tables omit PR-81.
- Updated `docs/reviews/pr-12.md`, preserved both earlier verdicts, and set the current verdict to `Changes required` for `0fbfa82`.
- Corrected the stale PR title, the automated-pass checkbox, and the D-# and OQ-# ranges in the PR description.
- The handoff held ten entries before this one, so Session 32 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on branch `docs/pr-12-critic`. Before this review commit, its remote tip is `74f9477`, and its effective head is `0fbfa82`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run. The automated pass reports approval with two closed findings and no open finding.

### In flight

PR #12 needs the P2-7 roadmap correction, then another repeat cross-provider review at the new effective head.

### Traps and gotchas

- A full dungeon contract reaches the phase file and each affected area ownership table.
- D-523 requires the effect-budget test for every map and battle place. A boss adds a battle place even when the place has one map.
- P2-6 stays fixed. The next correction must not reopen the five-dungeon count or the order of play.
- OQ-180 blocks PR-81, not PR #12. D-487 permits a future PR question in the roadmap.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 43.

### Open questions that block progress

None. P2-7 needs no owner decision. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

The author adds PR-81 to the later-place rows of `area-art.md` and `area-effects.md`. The author also adds its battle place to the PR-81 budget exit test, updates the response file, and requests another review.

## Session 41: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #12, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read the repeat review of Session 40. It set P1-1 and P2-1 to P2-5 to `fixed in d31ae6b`, and it added P2-6 with the verdict `Changes required` for head `d31ae6b`.
- The automated pass also closed its one finding and reported `Approved`.
- P2-6 has full merit. D-56, D-244, and D-313 counted four dungeons, and D-562 called the sealed gallery "a dungeon map" with no revision of those rows.
- The owner classified the gallery as the fifth dungeon, with the full dungeon contract (D-575). The session recommended a passage that only uses the dungeon-map format, and the owner chose a dungeon.
- The session applied D-575: marks on D-56, D-244, D-313, D-562, and D-564, the dungeon contract in the PR-81 entry, and the count of five in the design doc, two area files, Phase 3, `docs/world/places.md`, and `docs/world/arc.md`.
- A scan for the old count found three more rows, D-327, D-346, and D-369, and each now names D-575.
- A fifth dungeon needs a boss, and no document named one, so OQ-180 holds that question for PR-81 (D-487).
- `docs/reviews/pr-12-response.md` gained the answer of this round.
- The handoff held ten entries before this one, so Session 31 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The correction changes files outside the metadata set, so the effective head moves off `d31ae6b` to the commit of this answer.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The ids stay gap-free: D-1 to D-575, and OQ-1 to OQ-180.

### In flight

PR #12 waits for the repeat cross-provider review of P2-6 (T-4, D-17).

- The automated pass on `6a43f1a` reported `Approved with suggestions`, with one new finding. It had merit: the plain-English paragraph of Phase 4 in `docs/design.md` still said "Four more dungeons". The session corrected it to five dungeon builds, and a scan for other forms of the count found no other current contract (D-14, D-66).
- The correction is `0fbfa82`, the effective head. The pass on it reported `Approved`, with both of its findings closed and none open, so no comment waits for an answer.

### Traps and gotchas

- Region one has two hubs and five dungeons (D-575). The hanging cells count once, although the party visits them twice (D-327).
- The order of play is the hanging cells, the deep mine, the second visit to the cells, the sealed gallery, the refuge, the mining town by night, the border fort, and the ice crossing (D-313, D-574, D-575).
- The target of six to eight hours of D-56 stands with five dungeons. M-5 measures it, and a miss changes content in a PR of its own.
- A revised count reaches more rows than the rows that set it. A scan for the old words reads the Effect column of every decision too.
- A reviewer session can archive a handoff entry too. Before a session archives the oldest entry, it reads the handoff and takes the oldest session that the handoff still holds.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 42.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `0fbfa82`. It reads the P2-6 section of `docs/reviews/pr-12-response.md`, checks the trigger and the regression check, and writes the verdict (T-4, D-17). The automated pass is complete.
