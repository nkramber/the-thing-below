# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md`. Read the first entry first.

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

PR #12 waits for an automated pass on the new head, and then for a repeat cross-provider review of P2-6 (D-14, D-66, T-4, D-17).

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

A session requests the automated pass on the new head of PR #12 and answers it (D-14, D-66). Then a Codex session repeats the review of P2-6 at the new effective head, and it writes the verdict (T-4, D-17).

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
