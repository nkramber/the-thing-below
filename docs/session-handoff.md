# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 40: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #12 at effective head `d31ae6b`.

### What this session did, and why

- Read the current handoff first, then the review response, the review skill, the STE skill, the correction diff, the original triggers, the adjacent contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 39 identifies the other provider as the author of the substantive corrections.
- Recomputed the effective head. `d31ae6b` is the newest substantive commit. The later commits `aa3c5e2` and `c8e7e48` change only the review and handoff metadata paths.
- Verified P1-1 and P2-1 through P2-5 against their original triggers and regression checks. Each earlier finding is fixed in `d31ae6b`. The archive response correction in `aa3c5e2` also matches the committed archive.
- Checked the added PR-27 move. D-574, the design sequences, the Phase 4 sequence, the exploration roadmap, the places file, and the arc agree that PR-27 follows PR-81 and precedes PR-25.
- Found one new adjacent contract defect, P2-6. D-56, D-244, and D-313 define four dungeons. D-562 and the places file classify the sealed gallery as another dungeon without a revision mark.
- Updated `docs/reviews/pr-12.md`, preserved the earlier verdict, and set the current verdict to `Changes required` for `d31ae6b`.
- Corrected two stale PR facts. The title and the description now end the decision range at D-574.
- The handoff held ten entries before this one, so Session 30 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The current target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on `docs/pr-12-critic`. Before this review commit, its remote tip is `c8e7e48`, and its effective head is `d31ae6b`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run on the branch. The repeat automated pass has one fixed and resolved finding and no open finding.

### In flight

PR #12 needs the owner classification and correction in P2-6, then another repeat cross-provider review at the new effective head.

### Traps and gotchas

- A dungeon count and a map implementation kind are separate only when a current decision says so. D-562 now calls the gallery a dungeon map, and the places table calls it a dungeon.
- If the sealed gallery is a fifth dungeon, the correction must revise D-56, D-244, and D-313. If it is a route, D-562 and its consumers must state that it only uses the dungeon-map format.
- The six earlier findings stay fixed. A response to P2-6 must not reopen their corrected contracts.
- The review verdict covers `d31ae6b`, not the metadata tip.
- The next ids are D-575, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 41.

### Open questions that block progress

The owner must classify the sealed gallery as a fifth dungeon or as a route that uses the dungeon-map format. OQ-179 continues to block PR-5.

### Next concrete action

The author asks the owner to classify the sealed gallery, records the answer, corrects P2-6 and its adjacent consumers, and updates `docs/reviews/pr-12-response.md`. Then a Codex session repeats the review at the new effective head.

## Session 39: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #12, and the place of PR-27, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read `docs/reviews/pr-12.md`, the `Changes required` verdict of Session 38 for head `84b4128`. The owner also asked the session to examine the order of PR-27. The session assessed each finding against the evidence before it changed a file (the `pr-review` skill).
- Five findings have full merit, and one has partial merit. `docs/reviews/pr-12-response.md` holds each disposition, its evidence, and its regression check.
- P1-1: the rule of D-568, "no blur and no uneven pixels", cannot hold at a scale of 1.5. The session had written that absolute, and the owner had said "It won't be perfect pixel scaling, but it MUST look good." The owner chose even pixel sizes with a slight softness (D-573), which resolves OQ-105.
- P2-1: the scan of Session 37 left out the decision register and skipped each line that already named D-568, so it passed stale frame contracts. A corrected scan found the four that the review listed and five more, and each now names D-568.
- P2-2 and P2-3: OQ-114, OQ-135, the PR-30 exit test, Gate 4, and OQ-93 now match D-566, D-555, D-571, and D-572. D-42 gained its mark for D-555.
- P2-4 has partial merit. The PR description and a new dated line named the design-critic agent as the source of work, and both now name the pass of D-484. The name of the pass stays, because D-484 names it as a plan item.
- P2-5: the archive step of Sessions 35 and 37 inserted a session above a named older session, not at the top. The archive now runs from Session 29 down to Session 1, with no text changed, and this session inserted Session 29 at the top.
- The place of PR-27: the second hub is the refuge of the old faith (D-243, `docs/world/arc.md`). The party reaches it right after the sealed gallery (D-331), and it serves the fort and the ice (D-356). The owner moved PR-27 right after PR-81 (D-574).
- The handoff held ten entries before this one, so Session 29 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The corrections change files outside the metadata set, so the effective head moves off `84b4128` to the commit of this answer. The repeat review reads that head.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The phase lists of section 7, the sequence of section 8, and the five phase sequences give the same 79 PR items.

### In flight

PR #12 waits for the repeat cross-provider review (T-4, D-17).

- The effective head is `d31ae6b`. The commits above it change `docs/reviews/` and `docs/session-handoff.md` alone.
- The automated pass on `d31ae6b` reported `Approved with suggestions`, with one comment. That comment had merit: the response gave the archive range as Session 28 down to Session 1, and the same commit had archived Session 29 at the top. `aa3c5e2` corrected both lines, and the thread is resolved (D-14, D-66).
- An on-demand pass writes no check run, so the evidence of the pass is its review comment.

### Traps and gotchas

- A scan for a superseded rule must read `docs/decisions.md` too, and it must show each hit with its context. A filter that skips a line because it names the new decision passes a line that names both.
- The size 1280 by 800 stays true in three places: the Deck screen, the rule of Valve for text at 1280 by 800, and the display list of the fit test of PR-61. Each is a fact about the Deck, not about the frame.
- The archive step inserts a moved session at the top of the archive, above the newest archived session.
- An owner statement that accepts a tradeoff stays a tradeoff in the decision row. Never turn "must look good" into an absolute rule.
- The next ids are D-575, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 40.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `d31ae6b`. It reads `docs/reviews/pr-12-response.md`, checks each trigger and regression check, and writes the verdict (T-4, D-17). The automated pass is complete, and no comment of it waits for an answer.

## Session 38: 2026-09-16, Codex

Author: Codex
Session: cross-provider review of PR #12 at effective head `84b4128`.

### What this session did, and why

- Read the current handoff first, then the review skill, the STE skill, the complete PR diff, the changed contracts, the PR description, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 37 identifies the other provider as the author of the substantive changes.
- Recomputed the effective head. `84b4128` is the newest substantive commit. The four commits above it change only `docs/session-handoff.md` and `docs/session-handoff-archive.md`.
- Found six blockers. D-568 requires a 1.5 scale with neither blur nor uneven pixels, which no raster filter can supply. Current consumers retain the old frame, fog, save-point, and M-4 band rules. OQ-93 assigns Deck test instrumentation to a story scene. The PR description attributes the critic work to an agent. Session 27 is out of order in the handoff archive.
- Added `docs/reviews/pr-12.md` with the verdict `Changes required` for `84b4128`.
- Verified the PR order and id coverage, the roadmap list numbers, the guidance identity, the interim STE check, and `git diff --check`. The automated pass has one approval comment and no inline thread.
- The handoff held ten entries before this one, so Session 28 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The current target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on `docs/pr-12-critic`. Before the review commit, its remote tip is `d4330d1`, and its effective head is `84b4128`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run on the branch. The automated-pass issue comment reports `Approved` and no issue.

### In flight

PR #12 needs the six corrections in `docs/reviews/pr-12.md`, then a repeat cross-provider review at the new effective head.

### Traps and gotchas

- A 1280 by 720 image cannot scale to 1920 by 1080 with both equal source-pixel widths and no sample mixing. OQ-105 needs an owner choice that relaxes one property.
- D-568 supersedes D-480. Current implementation contracts must use the one 1280 by 720 frame, not only add a citation.
- D-555, D-566, and D-571 also need propagation into the questions and exit tests that implement them.
- The term correction in OQ-93 is `test scene`, not `story scene`.
- A PR description cannot credit an agent as the source of work, even when the named agent is a configured repository tool (T-6, D-22).
- D-18 requires every archive block to stay newest first. Session 27 is below Sessions 26 and 24 and needs a block-only move.
- The review verdict covers `84b4128`, not the metadata tip.
- The next ids are D-573, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 39.

### Open questions that block progress

OQ-105 blocks PR-61 and needs an owner answer that resolves P1-1. OQ-179 continues to block PR-5.

### Next concrete action

The author assesses each finding, asks the owner to settle the 1080p tradeoff in OQ-105, corrects the findings with their adjacent consumers, and writes `docs/reviews/pr-12-response.md`. Then a Codex session repeats the review at the new effective head.
