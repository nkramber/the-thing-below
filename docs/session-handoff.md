# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

## Session 37: 2026-09-16, Claude Code

Author: Claude Code
Session: the design-critic pass of PR #12 on the merged plan, the owner answers, and the fixes, on branch `docs/pr-12-critic`.

### What this session did, and why

- The owner merged PR #11 as `c4d39fe` on 2026-09-16. The session synced `main` and started PR #12, the design-critic pass of D-484 and D-490.
- The merged plan holds about 5,800 lines of roadmap files and 554 decisions, so the session ran the `design-critic` agent in five slices at once: the foundations, the frame and the effects, the play systems, the story and the release, and the registers.
- The five reports held 52 defects. The session checked each high-impact claim against the files before it acted, and F-57 records the pass.
- The owner answered 18 questions in five batches (D-555 to D-572). Four answers went against the recommendation, and two were answers that no option offered:
  - Nothing resets when the party leaves a dungeon, until a story event (D-555). That closes a loop that refilled every resource for one walk.
  - PR-80 holds the enemy record as a PR of its own (D-557).
  - The game has no fog of war (D-566), and the dungeon map screen shows the walked tiles (D-567).
  - The game draws one 16:9 frame of 1280 by 720, with black bars on the Deck, and 1920 by 1080 must look good (D-568). D-568 supersedes D-480.
  - Each use of "scene" names its kind: story scene, map scene, battle scene, or hub scene (D-572).
- The other answers took the recommendation. PR-68 and PR-50 move before PR-12 (D-556), the party window sets the starting row (D-558), and PR-61 shows the crash message and holds the input map (D-559, D-561). A workflow change takes the other review (D-560), PR-81 builds the sealed gallery (D-562), and a scene step joins a cast member (D-563). The first playable holds no boss (D-564), PR-39 lands after PR-78 (D-565), and the status window, the settings version, and the M-4 band have owners (D-569 to D-571).
- The session applied every answer and every forced fix across `docs/design.md`, the registers, the 17 roadmap files, the agent files, the PR template, three skills, the `playtest-bot` agent file, a runbook, and five world files. Twenty earlier decision rows gained their revision marks, and the note on D-543 gained its correction.
- Four fixes needed evidence, not a vote:
  - A read of the Valve Deck page on 2026-09-16 confirmed all four Verified rules. `docs/design.md` held two, so the session recorded the other two with the source and the date.
  - The note that the session wrote on D-543 while it answered the review of PR #11 was wrong: it put PR-68 before PR-14. D-556 now makes it true, and the row says so.
  - The rebuild of D-554 removed text that five area citations still pointed at. Each now cites a decision.
  - The critic claim that the reflection switch on Core reaches no program needs code to check. OQ-179 holds it for PR-5.
- The session placed two items with no question, because a stated rule forces each one. PR-23 draws the sprite frames of Ottild and Elio, under the rule of `area-art.md` that the frames land before each join. The night light setup of the mining town goes to the arc batch that writes the night pass, under D-442. The owner can move either.
- The handoff held ten entries before this one, so Session 27 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- The branch `docs/pr-12-critic` sits on `main`, and its remote head is the commit that holds this entry. A count of its commits goes stale with each fix to this entry, so the entry gives none.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The ids stay gap-free: D-1 to D-572, OQ-1 to OQ-179, and F-1 to F-57.
- A check proves that the phase lists of section 7, the sequence of section 8, and the five phase sequences give the same 79 PR items. Every active id from PR-1 to PR-81 has one place, and every numbered list in `docs/roadmaps/` counts from 1.

### In flight

PR #12 is open and waits for the cross-provider review.

- The automated pass is complete. The `Gitar` check run on the PR head `b9b0f24` completed with the conclusion `success`, and the review comment reported `Approved` with no issue (D-14, D-66).
- The pass left no inline comment, so no comment waits for an answer.
- The effective head is `84b4128`. The commits above it change `docs/session-handoff.md` alone, so each is a metadata commit and keeps that approval (the `pr-review` skill).
- PR #12 adds decision rows, so it takes the review of the other provider and never the `review-override` label (D-401).

### Traps and gotchas

- PR #12 changes `.github/pull_request_template.md` and the agent files, not `.github/workflows/`. D-560 moves workflow changes out of the override set from now on.
- D-568 supersedes D-480. A new citation of D-480 must name D-568. The external facts at 1280 by 800 stay, because they state the size of the Deck screen and the rule of Valve.
- A bare "scene" is not a term (D-572). The dated records, the resolved questions, the quoted source text, "scene light", "test scene", and "Godot scene file" keep their words.
- A renumber helper of this session once restarted a list at its anchor. The session repaired three lists and then checked every list of `docs/roadmaps/`. A later session that inserts a numbered item renumbers the whole list and runs that check.
- Phase 2 holds 46 PRs, and 43 of them land before Gate 2.
- The order of PR-27, the second hub, against the flight through the refuge is not checked. A later critic pass can read it.
- The next ids are D-573, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 38.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5. Every other detail question stays with the PR that the registers name (D-487).

### Next concrete action

A Codex session reviews PR #12 at the effective head `84b4128` and writes `docs/reviews/pr-12.md` (T-4, D-17, D-401). The automated pass is complete, and no comment of it waits for an answer.

## Session 36: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #11 at effective head `6266d54`.

### What this session did, and why

- Read the current handoff first, then the review response, the review skill, the STE skill, the corrected diff, the roadmap contracts, and the PR comments.
- Verified that Claude Code made the substantive PR changes and that Codex remains the eligible opposite provider under T-4 and D-17.
- Recomputed the effective head. `6266d54` is the newest substantive commit. The later commit `7899972` changes only `docs/session-handoff.md`.
- Reproduced both original triggers and verified their corrections. PR-61 now tests the fit without the later screen-test job, names its fixture panel, and leaves screen captures to PR-41. D-543 now records D-544 as a partial revision and gives PR-68 sole ownership of the condition form.
- Verified the additional PR-68 correction. Its exit test uses a scripted intent list, while the later bot dependency stays in its review focus.
- The exit-test dependency scan, PR-id coverage check, order check, interim STE check, `git diff --check`, and guidance identity check pass.
- Updated `docs/reviews/pr-11.md`, preserved P1-1 and P2-1 under `## Earlier verdicts`, and set the verdict `Ready for owner merge` for `6266d54`.

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on `docs/pr-11-roadmaps`. Its remote tip is `7899972`, and its effective head is `6266d54`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 is ready for owner merge. PR #12 holds the next design-critic pass after the merge (D-484, D-490).

### Traps and gotchas

- The review verdict covers `6266d54`, not the metadata tip `7899972`.
- P1-1 had partial merit. The screen-capture part was fixed. The fixture-panel part remains owned by PR-61.
- P2-1 is fixed by the explicit revision note in D-543. A later decision must keep the same revision form.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 37.

### Open questions that block progress

None for PR #11. OQ-60 to OQ-178 remain assigned to later PRs.

### Next concrete action

The owner can merge PR #11. Then a fresh session starts PR #12 after the merge.

## Session 35: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the PR #11 review findings, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session read `docs/reviews/pr-11.md`, the `Changes required` verdict of Session 34 for head `848de1e`. It assessed each finding against the evidence before it changed a file (the `pr-review` skill).
- P1-1 has partial merit.
  - The part with merit: exit test 1 of PR-61 asked for a screen test, and PR-41 creates that job four PRs later. The order cannot move, because D-524 puts PR-61 right before PR-7, and PR-41 captures the map scene that PR-7 builds. So the test moves. PR-61 now computes both views and both steps of the fit in a headless test, and PR-41 takes the captures.
  - The part with no merit: the finding says that the panels of PR-61 wait for PR-62. Section 7.4 of `area-ui-input.md` gives the panel-sizing rule to PR-61, and PR-62 owns the window stack alone. The response quotes that section. The exit test now reads "the fixture panel" for precision, and the owner of the rule stands.
- A scan for the class of defect that P1-1 names found one more instance that the review did not list. Exit test 2 of PR-68 used the bots, and PR-15 lands three PRs later. The test now uses a scripted intent list, which needs no bot policy.
- P2-1 has full merit. D-544 moved the condition form from PR-18 to PR-68 and left no revision mark on D-543. The Effect column of D-543 now carries `Revised in part by D-544`, with the part that changed and the parts that stand. The stale fixture-condition line came out with it.
- The regression check of P2-1 asked that every roadmap citation name PR-68. A search found twelve citations of D-543 outside the register and the dated records. Each cites the one-form rule or names PR-68, so no roadmap text needed a change. The defect sat in the register row alone.
- The session wrote `docs/reviews/pr-11-response.md` with the disposition, the evidence, and the regression check of each finding.
- The session added no decision, no question, and no finding. Nothing in the answer needed an owner choice.
- The handoff held ten entries before this one, so Session 25 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on branch `docs/pr-11-roadmaps`. Its remote head is the commit that holds this entry.
- The corrections change `docs/decisions.md` and `docs/roadmaps/phase-2-first-playable.md`, which sit outside the metadata set. So the effective head moves off `848de1e` to this commit, and the repeat review reads the new head.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 waits for the repeat cross-provider review.

- The automated pass ran again on the corrected head `6266d54` and reported `Approved`, with no open finding and no new comment (D-14, D-66). The head landed at 14:08:46 UTC, the request went out at 14:08:57, and the result came at 14:09:36.
- The pass left two comments over the life of the PR. One had merit, and `1bca609` answered it. The other was the acknowledgement of a request.
- The two review findings need a Codex session to verify each original trigger and set each finding to `fixed`.

### Traps and gotchas

- The corrections move the effective head, so the repeat review must name the new head, and never `848de1e` (the `pr-review` skill).
- An exit test of a PR can only use a tool or a job that exists when that PR lands. A script now checks that rule over all five phase files, and the response records it.
- Two lines of that scan are false matches. PR-59 and PR-37 use "captures" as the verb of the screen-test job of PR-41, which lands before each of them.
- A decision that is revised in part stays citable, so the twelve citations of D-543 stand (`CLAUDE.md`).
- An on-demand pass from the comment `Gitar review` writes no check run. Its result is the review comment itself.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 36.

### Open questions that block progress

None. OQ-60 to OQ-178 stay with the later PRs that the phase files name (D-487).

### Next concrete action

A Codex session repeats the review of PR #11 at the new effective head. It reads `docs/reviews/pr-11-response.md`, verifies the trigger and the regression check of P1-1 and P2-1, sets the status of each finding, and writes the verdict for that head (T-4, D-17, the `pr-review` skill).

## Session 34: 2026-09-16, Codex

Author: Codex
Session: cross-provider review of PR #11 at effective head `848de1e`.

### What this session did, and why

- Read the current handoff first, then the review skill, the STE skill, the design, the decision register, the questions register, the roadmaps, the prior review records, and the PR comments.
- Verified that Claude Code made the substantive PR changes and that Codex is the eligible opposite provider under T-4 and D-17.
- Recomputed the effective head. `848de1e` is the newest substantive commit. The three later commits change only `docs/session-handoff.md`.
- Inspected the complete PR diff and found two blockers. PR-61 requires a screen test before PR-41 creates it and a panel test before PR-62 creates the panels. D-543 assigns the condition format to PR-18, while D-544 assigns it to PR-68 without an explicit revision note.
- Ran the interim STE check, `git diff --check`, and the guidance identity check. All passed. No executable checks exist yet.
- Added `docs/reviews/pr-11.md` with the verdict `Changes required` for `848de1e`.

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on `docs/pr-11-roadmaps`. Its remote tip is `e92e329`, and its effective head is `848de1e`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 needs the two roadmap corrections in `docs/reviews/pr-11.md`, then a repeat cross-provider review at the new effective head.

### Traps and gotchas

- A review record names the newest substantive commit, not a later handoff-only commit.
- PR-61 must not require the screen-test job of PR-41 or the panels of PR-62 unless the sequence changes with it.
- D-544 must explicitly revise D-543 or D-543 must name PR-68 as the condition-format owner.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 35.

### Open questions that block progress

None for this review. OQ-60 to OQ-178 remain assigned to later PRs.

### Next concrete action

The author corrects the PR-61 exit-test ownership and the D-543 decision note. Then a Codex session repeats the review and verifies the corrected triggers.

## Session 33: 2026-09-16, Claude Code

Author: Claude Code
Session: the five phase files of PR #11, the rebuild of sections 7 and 8, and the open of PR #11.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 32. The remote head was `32193bd`, and no other session pushed after it.
- The working tree held an uncommitted block from Session 32: the new `docs/roadmaps/readme.md`, plus edits to two skills, the agent files, `docs/questions.md`, and the runbook. The session checked each edit, ran the STE check, and committed the block as `544c7f2`.
- The session wrote the five phase files of D-485. Each PR entry gives a scope, an out-of-scope list, numbered exit tests, a review focus, its questions, and the area files that it cites (D-144, D-487).
  - `phase-1-foundations.md`: the Deck test, PR-1, PR-2, PR-3, PR-46, PR-4, PR-5, PR-6, PR-43, PR-44, PR-47, PR-34, M-1, M-2, and Gate 1.
  - `phase-2-first-playable.md`: 42 PRs, from PR-54 to PR-17, then M-3, M-4, M-6, Gate 2, and the store block of PR-74, PR-75, and PR-76.
  - `phase-3-story-systems.md`: PR-18, PR-19, PR-20, PR-21, the retired PR-22, and Gate 3.
  - `phase-4-region-one.md`: PR-23 to PR-27, PR-42, PR-73, PR-28, PR-29, PR-77, PR-30, M-5, and Gate 4.
  - `phase-5-first-release.md`: PR-31, PR-33, PR-39, PR-78, PR-79, PR-40, the two owner steps, the trailer, the retired PR-32, and Gate 5.
- A check of the files proves the coverage. Every active id from PR-1 to PR-79 has one entry in one phase file and one place in one phase sequence. PR-22 and PR-32 are retired, and each has a short entry that states the gap (G-10).
- The session then asked the one question that the rebuild could not settle: the shape of section 7 with 79 PR ids. The owner chose the high-level index, which the session recommended (D-554).
- The session rebuilt sections 7 and 8 of `docs/design.md` on that answer (D-488, D-554). Section 7 lost about 40 technical paragraphs, which now live in the phase files alone. Each phase in section 7 keeps its gate, its ordered item list with one line for each item, its plain-English paragraph, and a link to its phase file. Section 8 takes the order that the phase files hold.
- A check proves that the phase lists of section 7, the sequence of section 8, and the section 8 of each phase file give the same 77 ids in the same order.
- The rebuild also fixed each stale text that the traps of Session 32 named. G-22 now names PR-49. The read order of `CLAUDE.md` and `AGENTS.md` no longer says that a docs PR creates `docs/roadmaps/`, and its `det-lint` command line names PR-46. The PR gate of the agent files and the PR template gained the `screen-test` line of PR-41 and the bot line of PR-15, and both now name PR-46 and PR-49 (G-16, D-496).
- The status paragraph of `docs/roadmaps/readme.md` is gone, as D-488 asks.
- The session opened PR #11 at head `848de1e`, with the full PR template in its description. The PR adds decision rows, so it takes the review of the other provider and never the `review-override` label (D-401).
- The handoff held ten entries before this one, so Session 23 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on branch `docs/pr-11-roadmaps`. The branch holds twenty commits on `main`, and its remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The diff against `main` is 31 files: 18 new files in `docs/roadmaps/`, and 13 changed files.

### In flight

PR #11 passed the automated pass and waits for the cross-provider review.

- The effective head is `848de1e`. The two commits above it change `docs/session-handoff.md` alone, so each is a metadata commit and neither moves the effective head (the `pr-review` skill).
- The `Gitar` check run on `848de1e` completed with the conclusion `success`. An on-demand pass at 13:52 UTC read the corrected handoff and reported `Approved`, with one finding closed and none open (D-14, D-66).
- The pass left one inline comment, on a wrong citation in this entry. The author fixed it in `1bca609`, replied, and resolved the thread.
- Next comes the Codex review, which writes `docs/reviews/pr-11.md` with a verdict for the effective head `848de1e` (T-4, D-17). This PR adds decision rows, so no label exempts it (D-401).

### Traps and gotchas

- PR #11 adds decision rows, so the `review-override` label never applies to it (D-401). It needs the Codex review.
- Count a gitar pass only from a check run on the head SHA, never from a dashboard page. A push that changes a path outside the metadata set needs a new pass.
- A commit that changes `docs/reviews/`, `docs/session-handoff.md`, or `docs/session-handoff-archive.md` alone is a metadata commit, and it never moves the effective head (the `pr-review` skill). A handoff commit above an approved head keeps that approval.
- An on-demand pass from the comment `Gitar review` writes no check run. Its result is the review comment itself, and the check run stays on the last automatic pass.
- Section 7 of `docs/design.md` is now an index. A session that wants the scope or the exit tests of a PR reads its phase file, and it never adds a paragraph back to section 7 (D-554).
- The five phase files and the twelve area files each cite the other set. A change to a PR id, an order, or a gate must change the phase file, the area file, and sections 7 and 8 together.
- `docs/roadmaps/readme.md` is the index of the folder, and the read order of the agent files points at it.
- D-540 revises D-114 in part, and D-548 revises D-418 in part. A citation of either must name the revision.
- The evaluator of D-534 still has no measurement (F-53). PR-11 reports the cost of a turn before Gate 2.
- New PR ids stay at PR-43 to PR-79. The next id is PR-80.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 34.

### Open questions that block progress

None. The phase files carry every blocked question into a table in their section 9, and the register `docs/questions.md` holds each one. Phase 1 lists 28, Phase 2 lists 85, Phase 3 lists 6, Phase 4 lists 1, and Phase 5 lists 10. Each one blocks a later PR, and that PR asks it when it starts (D-487).

### Next concrete action

A Codex session reviews PR #11 at the effective head `848de1e` and writes `docs/reviews/pr-11.md` (T-4, D-17, D-401). The automated pass is complete, and no comment of it waits for an answer. After PR #11 merges, PR #12 holds the next design-critic pass on the merged plan (D-484, D-490).

## Session 32: 2026-09-16, Claude Code

Author: Claude Code
Session: the last three area files of PR #11, `area-story.md`, `area-audio.md`, and `area-release.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 31. The remote head was `95e3276`, and no other session pushed after it.
- The owner asked for the three files in one session: story, then audio, then release. All three belong to PR #11, so this entry covers the three (D-18).
- The session read the design, the story, audio, and release decision blocks, the questions register, the nine finished area files, the skills, and `docs/world/arc.md`.
- Five checks of the Godot source and docs at `4.7.2-stable` gave the audio facts. The story and release areas needed no new check. The release facts come from `docs/design.md`, read 2026-09-14.
- The story block: the owner took all six recommendations, in two batches.
  - Core runs each scene and holds its step index, and Game draws each step and sends a wait intent (D-540). That lets the bots and the night gate play every scene.
  - PR-36 splits, and PR-68 takes the scene format and the scene runner (D-541).
  - A story flag is a name that is on or off, and one condition form serves every reader (D-542, D-543).
  - PR-68 also takes the flag set, which closes the gap that F-55 records (D-544). PR-50 lands right after PR-68 (D-545).
- The audio block: the owner took all four recommendations.
  - Three PRs take the audio work that PR-38 leaves: PR-69 the player, PR-70 the rules, and PR-71 the sound room (D-546).
  - The build renders each track into the Game assembly, beside the content files (D-547). That answers the open note of D-508.
  - An audio file names the content ids that it serves, and a rule file names no track (D-548). That revises D-418 in part.
  - PR-72 and PR-73 hold the music and the sounds of region one (D-549).
- The release block: the owner took all four recommendations.
  - The store page work splits into PR-75, the text and the owner steps, and PR-76, the art and the screenshots (D-550).
  - PR-74 adds the capture, right before PR-75, so one path takes each screenshot and each trailer shot (D-551).
  - PR-77 holds the credits roll, right after PR-29, so the owner sees it at Gate 4 (D-552).
  - PR-40 splits into PR-78, the Steamworks code, PR-79, the macOS signature in CI, and PR-40, the Steam publish (D-553).
- F-55 and F-56 record the two findings. F-55 is the flag set that sat one phase after its first reader. F-56 is a Godot audio call that returns an empty reference with a message in the log alone, and a playback position that Godot does not report exactly.
- The session filed 35 detail questions, OQ-144 to OQ-178, as D-487 asks. None blocks PR #11.
- The session wrote three area files. It updated `docs/design.md` (three dated lines, four system map rows, the cost model, F-55, and F-56), `area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, `area-effects.md`, `area-ui-input.md`, `area-exploration.md`, `area-progression.md`, the `ste-writing` skill, and fifteen earlier decision rows with notes.
- Two rules entered the files with no question, because an earlier decision forces each one. A scene names no art, as D-519 asks. Game moves a scene sprite from the tick of Core, never from a timer of Godot, as it moves the camera (F-52, G-23). The owner can ask for another rule.
- The handoff held ten entries before this one, so Session 22 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds twelve commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: the twelve area files of D-485 are done. Next come the five phase files, then the rebuild of sections 7 and 8 of `docs/design.md` (D-488). Then PR #11 opens.

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan: G-22 in `docs/design.md`, the PR gate of `CLAUDE.md` and `AGENTS.md` at lines 130 and 145 to 147, and lines 12, 13, and 15 of `.github/pull_request_template.md`. Section 7 still puts the export job in PR-7, the night gate in PR-15, and det-lint in PR-4, and M-3 reads one night count.
- Section 7 also needs every new PR in order. The story, audio, and release adds are: PR-68 right before PR-36, PR-50 right after PR-68, PR-69 and PR-70 right after PR-38, PR-71 after PR-70, PR-72 right before PR-17, PR-74 right after Gate 2, then PR-75 and PR-76, PR-77 right after PR-29, PR-73 in Phase 4, and PR-78 and PR-79 before PR-40 in Phase 5.
- Nine PRs lost work to a new id. The five of Session 31 stand, and four more join: the scene format and the runner left PR-36 for PR-68, the store page work left PR-40 for PR-75 and PR-76, the macOS signature left PR-40 for PR-79, and the Steamworks code left PR-40 for PR-78 (D-541, D-550, D-553). Section 7 still gives each one to the old PR.
- Section 7 also holds four texts that a later decision contradicts. The PR-36 entry says "Implement the runner in Game and the choice result in Core", which D-540 revises. The PR-18 entry gives PR-18 the flag set, which D-544 moves to PR-68. The PR-38 entry and the store page entry still say that the roadmaps PR gives the ids, which D-546, D-549, and D-550 answer. The PR-33 entry still leaves the credits roll unplaced, which D-552 answers.
- The PR-7 entry of section 7 still says that the roadmaps PR gives the export job its id, which D-503 answered in Session 29.
- After PR #11 merges, line 17 of `CLAUDE.md` and `AGENTS.md` is stale: `docs/roadmaps/` exists, and the roadmaps docs PR no longer creates it.
- `docs/roadmaps/readme.md` is the index of the folder. Its last paragraph, "Status, 2026-09-16", describes the phase files as absent, and the rebuild session removes that paragraph. The rebuild also adds a link from section 7 of `docs/design.md` to that index, which the `design-doc-style` skill asks for and section 7 does not have.
- Section 8 needs the owner step for the Apple Developer Program before PR-79, not before PR-40 (D-455, D-553).
- D-540 revises D-114 in part, and D-548 revises D-418 in part. A citation of either must name the revision.
- The evaluator of D-534 still has no measurement (F-53). PR-11 reports the cost of a turn before Gate 2.
- New PR ids so far: PR-43 to PR-79, 37 of the about 20 that D-486 named. The next id is PR-80.
- The next ids are D-554, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 33.

### Open questions that block progress

None for PR #11. OQ-168 to OQ-178 block PR-6, PR-31, PR-33, PR-74, PR-76, PR-78, PR-79, and PR-40. OQ-156 to OQ-167 block PR-38, PR-69, PR-70, PR-71, and PR-72. OQ-144 to OQ-155 block PR-68, PR-36, PR-19, and PR-20. OQ-134 to OQ-143 block PR-67, PR-12, and PR-13. OQ-124 to OQ-133 block PR-9, PR-10, PR-11, and PR-20. OQ-114 to OQ-123 block PR-7, PR-8, PR-21, PR-35, PR-64, and PR-65. OQ-104 to OQ-113 block PR-61, PR-62, PR-63, and PR-36. OQ-92 to OQ-103 block the Deck test, PR-56 to PR-59, PR-37, and PR-10. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-72 and OQ-74 block PR-2, PR-3, PR-46, PR-47, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 blocks PR-75, PR-33, and PR-44, OQ-58 blocks PR-78, OQ-59 blocks PR-75, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads the twelve area files and D-483 to D-490, then it writes the five phase files of D-485: `phase-1-foundations.md`, `phase-2-first-playable.md`, `phase-3-story-systems.md`, `phase-4-region-one.md`, and `phase-5-first-release.md`. Each phase entry gives its PR a scope, exit tests, a review focus, its questions, and the area file that it cites (D-144, D-487).

## Session 31: 2026-09-16, Claude Code

Author: Claude Code
Session: the fifth to the ninth area files of PR #11, `area-effects.md`, `area-ui-input.md`, `area-exploration.md`, `area-battle.md`, and `area-progression.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 30. The remote head was `c2c169b`, and no other session pushed after it.
- The owner asked for one area after another in one session: effects, then UI and input, then exploration, then battle, then progression. All five files belong to PR #11, so this entry covers the five (D-18).
- The session read the design, the whole decision register, the questions register, the four finished area files, the skills, the runbook, the PR template, the places file of the world, and the `game-text-style` skill.
- Seven read-only research agents read the Godot source and docs at `4.7.2-stable`, the Steam and SteamOS pages, and WCAG 2.2. The session checked each key fact again before it entered a document. The battle and progression areas needed no external fact, because Core holds every rule of a fight and of a build.
- The effects block: the owner took all four contract recommendations.
  - Every effect PR lands before PR-17, each right after the first scene that it needs (D-520). PR-56 is light and shadows, PR-57 the effect files with particles and the battle effects, PR-58 the ambient effects, PR-59 glow, and PR-60 the transitions.
  - OQ-73 closed with a review sheet of eight fixed light directions, so PR-48 lands right before PR-56 (D-521).
  - No rule waits for an effect. Where the world waits for one, Game sends a wait intent, and the run record holds it (D-522).
  - The Deck test also measures an effect budget, and a test fails content that passes it (D-523).
- The UI and input block: the owner took all four contract recommendations.
  - PR-61 builds the UI base right before PR-7, and PR-62 the menu windows right before PR-12 (D-524, D-525).
  - PR-63 builds the settings and the four accessibility settings in Phase 2, right before PR-57 (D-526).
  - One JSON file holds the UI style, and Game builds the Godot `Theme` from it (D-527).
- The exploration block: the owner took three recommendations and chose one split against the recommendation.
  - One rule file holds each map, with its terrain rows and every thing that a rule reads (D-528).
  - PR-16 splits, and PR-64 takes the traps, the hazards, and the statuses on the map (D-529).
  - The shop leaves PR-14 for PR-65, against the recommendation of one PR for the whole hub (D-530).
  - One run holds the map state and the battle state, and no map system ticks during a battle (D-531).
- The battle block: the owner took three recommendations and chose the deeper evaluator against the recommendation.
  - Core resolves each action at once and emits events. Game plays the queue and takes the next command when it drains, so no wait intent enters a fight (D-532).
  - PR-9 splits, and PR-66 takes the eight elements and the ten statuses (D-533).
  - The evaluator simulates each legal action and the strongest reply of the other side (D-534). The session recommended one action ahead alone.
  - A group file for each region holds each enemy group, and a map names a group by its id (D-535).
- The progression block: the owner took all four contract recommendations.
  - PR-12 splits, and PR-67 takes the character level, the experience, and MP, right before PR-12 (D-536).
  - Each character carries its own stat curve in content (D-537). That closes a gap that this session found, which F-54 records.
  - The quest state of PR-19 holds every personal task, and a side aptitude reads a story flag (D-538).
  - Each lesson lists its named forms, and each form names the point total that opens it (D-539).
- F-46 to F-54 record the findings of the five blocks: three silent failures of 2D light, a glow that can reach a lit sprite, a fit that Godot cannot make, three font defaults, five input facts, four tile map defaults, a camera that centers a small map with no doc behind it, an evaluator with no measurement, and stats with no source after the end of the job system.
- The session filed 52 detail questions, OQ-92 to OQ-143, as D-487 asks. None blocks PR #11.
- The session wrote five area files. It updated `docs/design.md` (five dated lines, the system map, the cost model, and F-46 to F-54), `area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, the runbook of the machine, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, and ten earlier decision rows with notes.
- Three rules entered the files with no question, because an earlier decision or a verified fact forces each one. An effect file names the content ids that it serves, and a rule file never names an effect, as D-519 asks for art (D-495). Game makes each intent from an input event, never from a poll of `Input`, because a poll sees input that a menu already took (F-50, D-493). Game moves the camera from the tick, because the smoothing of Godot can run more than once in a frame (F-52). The owner can ask for another rule.
- The handoff held ten entries before this one, so Session 21 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds nine commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: nine of twelve area files are done. Next comes `area-story.md`, then `area-audio.md` and `area-release.md` (D-485). Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan: G-22 in `docs/design.md`, the PR gate of `CLAUDE.md` and `AGENTS.md` at lines 130 and 145 to 147, and lines 12, 13, and 15 of `.github/pull_request_template.md`. Section 7 still puts the export job in PR-7, the night gate in PR-15, and det-lint in PR-4, and M-3 reads one night count.
- Section 7 also needs the new PRs in order: PR-61 before PR-7, PR-55 before PR-10, PR-48 and PR-56 between PR-10 and PR-11, PR-63 before PR-57, PR-57 to PR-60 after it, PR-66 right after PR-9, PR-67 right before PR-12, PR-62 before PR-12, PR-65 after PR-13, and PR-64 after PR-16. Section 8 needs the Deck test with its effect budget before PR-1.
- Five PRs lost work to a new id: the settings left PR-33 for PR-63, the shop left PR-14 for PR-65, the traps left PR-16 for PR-64, the elements and statuses left PR-9 for PR-66, and the levels and MP left PR-12 for PR-67 (D-526, D-529, D-530, D-533, D-536). Section 7 still gives each one to the old PR.
- The evaluator of D-534 has no measurement, and the same code runs on the Deck and through a night of fourteen thousand runs (F-53). PR-11 reports the cost of a turn before Gate 2.
- Godot drops a light past 15 on one canvas item with no message (F-46). Godot has no fit like the fit of D-232 (F-48), and it centers a map smaller than the view with only the source behind it (F-52).
- `area-story.md` must hold the flags, the branches, the quests, the personal tasks of D-538, the scene format that the screenplay tool of PR-50 prints, and the scenes that play on a map. `area-audio.md` holds the tracks, the stings, and the sound room. `area-release.md` holds the store work at Gate 2, the trailer capture with fixed particle seeds, and the studio mark of OQ-90.
- New PR ids so far: PR-43 to PR-67, 25 of the about 20 that D-486 named. The next id is PR-68.
- The next ids are D-540, OQ-144, F-55, L-16, G-26, PR-68, M-7, and Session 32.

### Open questions that block progress

None for PR #11. OQ-134 to OQ-143 block PR-67, PR-12, and PR-13. OQ-124 to OQ-133 block PR-9, PR-10, PR-11, and PR-20. OQ-114 to OQ-123 block PR-7, PR-8, PR-21, PR-35, PR-64, and PR-65. OQ-104 to OQ-113 block PR-61, PR-62, PR-63, and PR-36. OQ-92 to OQ-103 block the Deck test, PR-56 to PR-59, PR-37, and PR-10. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-72 and OQ-74 block PR-2, PR-3, PR-46, PR-47, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads the nine finished area files, D-520 to D-539, and the story entries of `docs/design.md`, such as D-40, D-114, D-173, D-282, D-329, D-350, and D-352 to D-355. Then it writes `docs/roadmaps/area-story.md`, asks the contract questions, and files detail questions with their PRs (D-487, D-488).

## Session 30: 2026-09-15, Claude Code

Author: Claude Code
Session: the fourth area file of PR #11, `docs/roadmaps/area-art.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 29. The remote head was `4d68a01`, and no other session pushed after it.
- The owner reported two changes to the repository settings: "Allow rebase merging" is off, and "Require actions to be pinned to a full-length commit SHA" is on. `gh api` confirmed both, and squash merging is the only merge method. The runbook, `area-ci.md`, D-8, and D-511 record it.
- The session read the design, the whole decision register, the questions register, the three finished area files, the skills, the runbook, the PR template, the palette, and the old atlas script.
- Two read-only research agents read GitHub pages and the Godot docs and source at `4.7.2-stable`. The session checked each key fact again before it entered a document: the `gh` help and release notes, the GitHub docs source, the README of `upload-artifact`, and the Godot XML, shader, and C++ files.
- A new fact settled the trap of Session 29: `gh` 2.99.0 of 2026-09-01 uploads an image into a PR description with `--attach`, and the Mac has 2.100.0.
- The owner took all six contract recommendations in two batches:
  - The owner sees each art batch as review sheets that `gh --attach` puts in the PR description, and no review image enters git (D-514).
  - A drawing is one JSON file with rows as strings (D-515). This settles G-6 and D-116 against the `.grid` files of D-119 and D-404.
  - A large picture places drawn pieces (D-516), because one full-screen grid holds about 1.1 million palette keys (F-44). PR-55 builds the format right before PR-10 (D-518).
  - Core holds the record of every content file, a file that no rule reads included (D-517).
  - An art file names the content ids that it draws, and a rule file never names art (D-519).
- The question of D-516 named the file a layout. A layout already names a map file (D-39, D-386), so the rows and the documents use the term large picture (D-12). The Effect column of D-516 says so, and the owner can ask for another term.
- The Godot checks removed one question: both renderer families correct the normal map of a sprite that `flip_h` mirrors, so the atlas holds no mirrored frame. F-45 records three Godot defaults that meet the pixel art.
- The session filed seven detail questions for PR-34, PR-7, PR-33, and PR-55 (OQ-85 to OQ-91), as D-487 asks. None blocks PR #11.
- The session wrote `docs/roadmaps/area-art.md`. It updated `docs/design.md` (a dated line, the system map, the cost model, F-44, F-45, G-24, and G-25), `area-tools.md`, `area-core.md`, `area-ci.md`, the runbook of the machine, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, and eleven earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 20 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds four commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- On 2026-09-14, `gh api` showed `allow_rebase_merge` false, `allow_merge_commit` false, and `sha_pinning_required` true.

### In flight

PR #11: four of twelve area files are done. Next comes `area-effects.md`, then the other seven area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan:
  - G-22 in `docs/design.md`: PR-49 (D-496), the night counts (D-507), a night on the head of a PR (D-510), and docs-only PRs (D-513).
  - The PR gate of `CLAUDE.md` and `AGENTS.md`, lines 145 to 147: det-lint in PR-46, the identity file (D-504), the night gate in PR-49, and a new line for the bot runs of PR-15 (D-505). Line 130 still says det-lint comes "after PR-4".
  - Lines 12, 13, and 15 of `.github/pull_request_template.md`, with the same changes and the bot line.
  - Section 7 still puts the export job in PR-7 and the night gate in PR-15, and M-3 reads one night count. The owner step of D-511 is done, so section 8 needs no step for it.
  - Section 7 still gives PR-34 the normal maps and "the grid schema", and PR-10 "A backdrop per place" as one grid. PR-17 names the sprite set of Marrek alone, and section 7.9 of `area-art.md` adds the frames of Bergit and Dagvar (D-200, D-362). PR-55 needs an entry right before PR-10 (D-518).
- Keep the word layout for map files alone. The glossary of the `ste-writing` skill has the new art terms: drawing file, piece, large picture, atlas index, and review sheet.
- The dated row D-204 says "edge pieces" for edge tiles. `area-tools.md` now says "border tile", because piece is a glossary term.
- `area-effects.md` meets three facts: a custom shader that writes `NORMAL_MAP` gets no flip correction (issue 101277), 2D light draws at the pixel size of the Viewport whatever the texture filter (the 2D lights tutorial, read 2026-09-14), and each piece of a large picture has a normal map (D-516).
- `area-ui-input.md` must set the stretch, because the 4.7 editor writes `canvas_items` and `expand` into a new project (F-45). The fonts load from the Game assembly (D-508). A research agent read in the source that `FontFile.data` takes the bytes of a dynamic font, and no doc or test confirms it.
- OQ-86 meets `area-exploration.md`: a `TileMapLayer` needs its tiles on a regular grid in the atlas.
- The boot splash image is a PNG path that the engine reads at the start, so the studio mark of D-468 cannot come from the atlas (OQ-90).
- The samples readme still describes the `.grid` files, and the note on D-404 carries D-515. Sessions skip `docs/samples/` (D-403).
- New PR ids so far: PR-43 to PR-55, thirteen of about 20 (D-486). The next id is PR-56.
- The next ids are D-520, OQ-92, F-46, L-16, G-26, PR-56, M-7, and Session 31.

### Open questions that block progress

None for PR #11. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, D-491 to D-519, and the effects entries of `docs/design.md`, such as D-139, D-160, D-180 to D-196, and D-240. Then it writes `docs/roadmaps/area-effects.md`, places PR-48 and the first PR that draws light, asks the contract questions, and files detail questions with their PRs (D-487, D-488).
