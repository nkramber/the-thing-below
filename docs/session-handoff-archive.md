# Session handoff archive

Sessions older than the 10 in `docs/session-handoff.md`, newest first (D-18). Move an entry here word for word.

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

## Session 29: 2026-09-14, Claude Code

Author: Claude Code
Session: the third area file of PR #11, `docs/roadmaps/area-ci.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 28. The remote head was `bad1797`, and no other session pushed after it.
- The session read the design, the whole decision register, the questions register, `area-core.md`, `area-tools.md`, the skills, the agent file of the playtest bot, the PR template, and the runbook of the machine. It read the decision register of what-you-carry for its night record and night gate rules, for the process alone (D-277).
- Three read-only research agents read GitHub, Godot, Microsoft, xUnit, Coverlet, and ReportGenerator pages. The session fetched eight key pages again before a fact entered a document. It also read the NuGet API, the Godot release files and their SHA-512 list, the licenses of five actions, and the settings of the repository through `gh api`.
- The session asked twelve contract questions in three batches (D-487), and the owner took nine recommendations:
  - The export job is PR-54, right before PR-7 (D-503). It also runs on a PR that changes the export, and each export runs the smoke session (D-512).
  - The replay-identity job compares each leg with a committed identity file (D-504).
  - PR-1 publishes the coverage report (D-506).
  - The Game assembly embeds `content/`, and Tools holds the one reader of the folder (D-508).
  - The night record is an artifact of the run of the night job (D-509). A night on the head commit of a PR passes that PR (D-510), and a docs-only PR passes the night gate (D-513).
  - Workflows use five actions of the `actions` organization of GitHub, each pinned to a full commit SHA, and D-511 is their G-13 entry.
- The owner chose against three recommendations:
  - The bot runs play on all three CI legs, not on Linux alone (D-505).
  - A night plays ten thousand runs on Linux and two thousand each on Windows and macOS (D-507). The session recommended ten thousand on each leg, and then one thousand on the other two. D-507 revises D-64 in part, and the session confirmed the final count before the row.
- F-40 records that the test command of `CLAUDE.md` works in VSTest mode alone. F-41 records four rules of GitHub Actions, F-42 two Godot export facts, and F-43 a night gate that blocked its own fix.
- The session filed ten detail questions for PR-1, PR-15, PR-41, PR-49, and PR-54 (OQ-75 to OQ-84), as D-487 asks. None blocks PR #11.
- The session updated `docs/design.md`: a dated line, the system map, the cost model, and F-40 to F-43. It updated `area-core.md`, `area-tools.md`, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, the runbook of the machine, and six earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 19 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds three commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: three of twelve area files are done. Next comes `area-art.md`, then the other eight area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan:
  - G-22 in `docs/design.md`: PR-49 (D-496), the night counts (D-507), a night on the head of a PR (D-510), and docs-only PRs (D-513).
  - The PR gate of `CLAUDE.md` and `AGENTS.md`, lines 145 to 147: det-lint in PR-46, the identity file (D-504), the night gate in PR-49, and a new line for the bot runs of PR-15 (D-505). Line 130 still says det-lint comes "after PR-4".
  - Lines 12, 13, and 15 of `.github/pull_request_template.md`, with the same changes and the bot line.
  - Section 7 still puts the export job in PR-7 and the night gate in PR-15, and M-3 reads one night count. Section 8 needs the owner step of D-511 before PR-1.
- The owner must enable "Require actions to be pinned to a full-length commit SHA" before PR-1 (D-511). The setting was off on 2026-09-14, and step 1 of the owner actions in the runbook names it.
- `gh api` showed "Allow rebase merging" on, on 2026-09-14. The runbook asks the owner to turn it off (D-8).
- D-508 changes three later area files. `area-art.md` plans the atlas as bytes loaded at run time (`Image.LoadPngFromBuffer`), not as an imported texture. `area-audio.md` says where rendered audio lives, because a large render does not suit the assembly. The phase file gives PR-5 work in Game and Tools, not in Core alone.
- The embedded resource names must match on every leg. Check the path separator on Windows in PR-5.
- F-40: if OQ-75 picks MTP, PR-1 changes the test commands of `CLAUDE.md`, `AGENTS.md`, and the `csharp-conventions` skill.
- GitHub disables a schedule after 60 days with no activity in a public repository, and the night gate then fails every PR (F-41).
- New PR ids so far: PR-43 to PR-54, twelve of about 20 (D-486). The next id is PR-55. The owner chose the fuller option three times in this block. Show running counts of runs, legs, and CI time in each batch.
- Contract questions for `area-art.md` from earlier sessions: how the owner sees an image batch in a PR (G-25), and whether `gh` can put an image in a PR description. Check it before a question assumes it.
- The next ids are D-514, OQ-85, F-44, L-16, G-26, PR-55, M-7, and Session 30.

### Open questions that block progress

None for PR #11. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `area-tools.md`, `area-ci.md`, D-491 to D-513, and the graphics entries of `docs/design.md`. Then it writes `docs/roadmaps/area-art.md`, asks the contract questions, and files detail questions with their PRs (D-487, D-488).

## Session 28: 2026-09-14, Claude Code

Author: Claude Code
Session: the second area file of PR #11, `docs/roadmaps/area-tools.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 27. The remote head was `4e16d05`, and no other session pushed after it.
- The session read the design, the whole decision register, the questions register, `area-core.md`, the skills, the agent files, the interim tools, the PR template, and the runbook of the machine.
- The session asked seven contract questions in two batches (D-487), and the owner took each recommendation:
  - Four tools leave the PRs that they share: det-lint becomes PR-46, the PNG code PR-47, the normal maps PR-48, and the night gate PR-49 (D-496).
  - Four tools with no PR take ids and land right before their first user: the screenplay tool PR-50, the PNG import PR-51, the map preview PR-52, and the tile-edge tool PR-53 (D-497).
  - det-lint reads code through the Roslyn compiler library, and the row of D-498 is the decision entry of the package (G-13).
  - Game shows player text through one text helper, and det-lint fails a Godot text property outside it (D-499).
  - GitHub starts `pull_request_target` and `schedule` only from `main`, so PR-3 and PR-49 prove their commands in Tests, and G-16 gains a note (D-500, F-37).
  - The tile-edge tool writes one edge file per map outside the rule files (D-501).
  - A tool whose output a test compares on every CI leg uses integer math (D-502).
- The session read thirteen sources before a fact entered a document: the GitHub trigger rules, two NuGet pages and the NuGet API, eight .NET and C# pages, and the PNG standard. A page summary of NuGet named xunit.v3 4.0.0 as the latest version, and the NuGet API lists 4.0.1, so no xUnit version entered a document.
- F-38 records two float facts of .NET. F-39 records that the default string order of .NET follows the culture and the ICU version of the machine.
- F-39 corrects `area-core.md`: its section 7.5 named `SortedDictionary` for a fixed order and named no comparer. G-4 already asks for a fixed order, and among the built-in comparers only an ordinal comparison stays fixed across machines. So the session added the rule with no question. The owner can still ask for another rule.
- The session filed eight detail questions for PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15 (OQ-67 to OQ-74), as D-487 asks. None blocks PR #11.
- The session updated `docs/design.md`: a dated line, the system map, F-35 to F-39, and a note on G-16. It updated `area-core.md`, the `csharp-conventions`, `pr-review`, and `ste-writing` skills, and 14 earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 18 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds two commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: two of twelve area files are done. Next comes `area-ci.md`, then the other nine area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8, because D-496 moves det-lint to PR-46 and the night gate to PR-49. These texts still name the old PRs: G-22 in `docs/design.md`, lines 130, 145, and 147 of `CLAUDE.md` and `AGENTS.md`, and lines 12 and 15 of the PR template. Change them in the rebuild, not before.
- Sections 7 and 8 of `docs/design.md` still show PR-6 whole, PR-4 with det-lint, PR-15 with the night gate, and PR-34 with the PNG code and the normal maps.
- New PR ids so far: PR-43 to PR-53, eleven of about 20 (D-486). The next id is PR-54. The owner took every split in this block, so the count can pass 20. Show running counts in each batch.
- Contract questions for `area-ci.md`, from Session 27 and this session:
  - Where the exported Game reads its content files, and where the host code that reads content files lives.
  - The PR id of the export job of D-449, which PR-45 follows (D-492).
  - The PR and the package of the coverage report of D-174 (G-13).
  - How the `replay-identity` job compares the CI legs: committed hashes, or a compare across legs.
  - Where the night record lives, and how the `night-gate` check finds it (D-500).
  - The job for the few hundred bot runs on each PR (D-64).
- A detail question for PR-1 belongs in `area-ci.md`: `CLAUDE.md` names xUnit, and no decision row justifies a test package (G-13). On 2026-09-14 the NuGet API listed xunit.v3 4.0.1 under Apache-2.0.
- `area-art.md` must settle how the owner sees an image batch in a PR (G-25). The swatch sheet, the atlas, the map preview, and the normal-map preview are PNG files. Check whether `gh` can put an image in a PR description before a question assumes it.
- `area-audio.md` must settle how the listen command of D-439 plays sound from a console program.
- OQ-70: det-lint needs the Godot assembly to read Game types, so PR-46 needs a Game build or the Godot package.
- A page summary can misstate a version. Check each package fact against the NuGet API at `api.nuget.org/v3-flatcontainer/<id>/index.json`.
- The next ids are D-503, OQ-75, F-40, L-16, G-26, PR-54, M-7, and Session 29.

### Open questions that block progress

None for PR #11. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `docs/roadmaps/area-tools.md`, D-491 to D-502, and the Phase 1 entries of `docs/design.md`. Then it writes `docs/roadmaps/area-ci.md`, asks the contract questions above, and files detail questions with their PRs (D-487, D-488).

## Session 27: 2026-09-14, Claude Code

Author: Claude Code
Session: the first area file of PR #11, `docs/roadmaps/area-core.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- Session 26 (Codex) gave PR #10 the verdict `Ready for owner merge` at `7eb2abc`. The owner merged PR #10 as `63803d9`.
- The session started PR #11 on the new branch `docs/pr-11-roadmaps` from `63803d9`, and it wrote an area file first (D-488).
- The session read the design, the whole decision register, the questions register, the skills, and one focused roadmap of what-you-carry, for its document shape alone (D-277).
- The session asked four contract questions (D-487), and the owner took each recommendation:
  - PR-6 splits into three PRs. PR-6 keeps the loop, the intents, the run record, and replay. PR-43 takes the save files, and PR-44 takes crash files and log files (D-491).
  - PR-6 adds the debug seam, and PR-45 creates the debug assembly right after PR-7 (D-492).
  - The run record holds intents, with no device kind (D-493).
  - A sixth project, `TheThingBelow.Storage`, holds the file code (D-494).
- A fifth question came up while the file took shape, and the owner took the recommendation: the content hash covers the rule files alone (D-495).
- The session read eight sources on .NET, Git for Windows, the GitHub runner image, and git before a fact entered a document. F-35 records that the .NET hash classes call OS libraries and that a string hash code can change between runs. F-36 records that System.Text.Json uses reflection by default.
- A check found that `.gitattributes` already sets `eol=lf` from PR #1, so the Windows CI leg needs no finding for line ends. The area file cites the rule beside the default of Git for Windows.
- The session filed seven detail questions for PR-4, PR-5, PR-6, and PR-43 (OQ-60 to OQ-66), as D-487 asks. None blocks PR #11.
- The session wrote `docs/roadmaps/area-core.md` and added 18 revision notes to earlier rows. It updated the system map, the finding register, and a dated line in `docs/design.md`, and the `csharp-conventions` and `pr-review` skills.
- The handoff held ten entries before this one, so Session 17 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds one commit on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: one of twelve area files is done. Next come `area-tools.md` and `area-ci.md`, then the other nine area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- Sections 7 and 8 of `docs/design.md` still show PR-6 whole, with no PR-43, PR-44, or PR-45. The rebuild at the end of PR #11 changes them (D-488, D-491, D-492). Do not edit them before the phase files exist.
- New PR ids so far: PR-43, PR-44, and PR-45, three of about 20 (D-486). The next id is PR-46.
- The export job of D-449 still needs its PR id in `area-ci.md`, and PR-45 comes after that job (D-492).
- Where the exported Game reads its content files is open. Godot exports a JSON file only through the filter for non-resource files, and `content/` sits outside the Game project folder (D-118). The session did not check whether an export can take a file from outside that folder. Ask it as a contract question in `area-ci.md` (D-487).
- Storage holds the file code for saves, records, crash files, and logs (D-494). The reader of content files for Tools and Tests has no home yet. Settle it with the content question above.
- D-495 keeps the string text out of the content hash. A rule file that names a string id still needs the id test of section 7.7 of the area file.
- The system map now splits the snapshot bytes of Core from the files of Storage. A later area file keeps that split.
- Every later Core PR carries the seven steps of section 7.14 of the area file. The phase files turn them into exit tests.
- The owner took every recommendation in this block. Still show running counts of PR ids and files in each batch (Session 23).
- The next ids are D-496, OQ-67, F-37, L-16, G-26, PR-46, M-7, and Session 28.

### Open questions that block progress

None for PR #11. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, D-491 to D-495, and the Phase 1 entries of `docs/design.md`. Then it writes `docs/roadmaps/area-tools.md`, asks contract questions alone, and files detail questions with their PRs (D-487, D-488).

## Session 26: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #10 at effective head `7eb2abc`.

### What this session did, and why

- Verified the new base, effective head, changed paths, provider gate, response file, and existing PR comments.
- Read the correction diff, the complete PR diff, the design and decision contracts, the question register, the skills, the agent guidance, the handoff archive, and the PR description.
- Reproduced P2-1. D-488 now uses `Revised in part by D-490`, names PR #11 as the changed part, and keeps the writing order.
- Confirmed D-490 names D-488 among the revised decisions and that the design and handoff identify PR #11.
- Confirmed the repeat Gitar check passed on `7eb2abc` with no new comment.
- Updated `docs/reviews/pr-10.md`, preserved P2-1 with its earlier evidence, and set the verdict to `Ready for owner merge` for `7eb2abc`.
- Ran the interim STE check with 0 findings, `git diff --check`, and the guidance identity check.

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. Its effective head is `7eb2abc`.
- The interim STE check passes with 0 findings. The repeat review metadata and handoff are pushed in `e8cc765`.

### In flight

PR #10 is ready for owner merge. Then PR #11 starts the roadmaps on a new branch.

### Traps and gotchas

- D-488 and D-489 keep `PR #10` in their topic columns as dated text. Their revision notes carry the current PR number.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 27.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

The owner can merge PR #10. Then a fresh session starts PR #11 with `docs/roadmaps/area-core.md`.

## Session 25: 2026-09-14, Claude Code

Author: Claude Code
Session: the answer to the review of PR #10, on branch `docs/pr-10-roadmaps`.

### What this session did, and why

- Session 24 (Codex) reviewed PR #10 at `9355d62` and gave `Changes required` with P2-1: D-488 kept the PR number that D-490 changed, and D-490 did not mark D-488 as revised in part.
- The owner asked the session to address the feedback. The session fetched the branch at `571e39e`, read the review record, and found no other open comment or thread.
- P2-1, full merit: the Effect column of D-490 now revises D-484, D-488, and D-489 in part. The note on D-488 now uses the marker `Revised in part by D-490`, names the PR number as the changed part, and keeps the order of the work.
- A search of the live documents found PR #11 as the PR of the roadmaps and the rebuild in D-484, D-488, D-489, D-490, the dated line and step 2 of section 8 in `docs/design.md`, and the handoff.
- `docs/reviews/pr-10-response.md` records the disposition.
- The handoff held ten entries before this one, because Session 24 moved Session 14 to the archive. Session 15 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. The commit that holds this entry is the new effective head, because it changes `docs/decisions.md`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #10 answers a new gitar pass on the new head. Then a Codex session runs the repeat review of PR #10 and updates `docs/reviews/pr-10.md`. The owner merges. Then PR #11 starts the roadmaps on a new branch (D-488, D-490).

### Traps and gotchas

- After a push, the first `Gitar review` request re-runs the previous head, and it can complete an existing check run again rather than start a new one. Send the second request when the dashboard updates or an old run completes again with no run on the new head.
- A reviewer session can move an old entry to the archive. Count the handoff entries before a rotation, and never assume the count.
- D-488 and D-489 keep "PR #10" in their topic column as dated text. Their revision notes carry the current PR number.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 26.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on the new head and records it in the PR description. Then a Codex session runs the repeat review of PR #10 under the `pr-review` skill. The owner merges. Then a fresh session starts PR #11 with `docs/roadmaps/area-core.md`.

## Session 24: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #10 at effective head `9355d62`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and existing PR comments.
- Read the complete diff, the design and decision contracts, the questions register, the skills, the agent guidance, the handoff archive, and the PR description.
- Confirmed the final Gitar check passed on `9355d62` and that its one suggestion was fixed in that commit.
- Found P2-1: D-488 still names PR #10, while D-490 says D-488 binds PR #11 without a revision note for D-488.
- Ran the interim STE check with 0 findings, `git diff --check`, and the guidance identity check.
- Added `docs/reviews/pr-10.md` with the verdict `Changes required` for `9355d62`.

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. Its effective head is `9355d62`.
- The interim STE check passes with 0 findings. The review record and handoff are pushed in `95fd404`.

### In flight

PR #10 needs the D-488 revision note and a repeat review. The owner merges after the verdict covers the new effective head.

### Traps and gotchas

- D-490 must revise D-488 in part, not only D-484 and D-489. The writing order stays unchanged, and only the PR number changes to PR #11.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 25.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

The author adds the D-488 revision note and runs a repeat Gitar pass. Then a Codex session updates `docs/reviews/pr-10.md` for the new effective head.

## Session 23: 2026-09-14, Claude Code

Author: Claude Code
Session: the shape of the roadmaps after PR #9 merged, as docs PR #10 on branch `docs/pr-10-roadmaps`.

### What this session did, and why

- Session 22 (Codex) reviewed PR #9 at `5097e8a` with no finding. The owner merged PR #9 as `4f37c99`.
- The same harness run as Session 21 started the roadmaps docs PR (D-399). It asked OQ-56 first, and the owner chose no change to the sequence (D-483).
- The session read every text that sends work to the roadmaps. The list holds PR ids for the export job, the store page work, the credits roll, the trailer capture, the debug assembly, the sound room, and the mood cues. It also holds the items of the technical, graphics, UI, and systems roadmaps.
- The owner set the shape of the work:
  - Two docs PRs for the roadmaps work: one for the roadmaps and the rebuild of sections 7 and 8, and a later one for the design-critic pass (D-484).
  - Twelve area files and five phase files in `docs/roadmaps/`, with the names in D-485.
  - One new PR id per system or tool, about 20, from PR-43 (D-486).
  - The roadmaps ask contract questions, and they file detail questions with the PR they block (D-487).
  - Areas first, then phases, then the rebuild (D-488). The roadmaps PR opens when its work is complete (D-489).
- The owner asked whether every document was current, and why no PR was open. The session quoted D-489, and the owner chose to merge the shape now as PR #10 (D-490). The roadmaps move to PR #11, and the critic pass to PR #12.
- The session brought the design current: a dated line, the file set in section 7, and step 2 of section 8. Notes on D-399, D-484, D-488, and D-489 record the later answers. No roadmap file exists yet, and the area files wait for a fresh context.
- The gitar pass on `05dd81a` approved with one suggestion, which had merit: the D-485 row sat after D-487. The commit that holds this revision of the entry moves the row between D-484 and D-486, and the reply on the thread names it.
- The handoff held eleven entries before this one, because Session 22 added its entry and moved none. Sessions 13 and 12 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps` with D-483 to D-490. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings.

### In flight

PR #10 answers the gitar pass. It adds decision rows, so a Codex session reviews it, and no `review-override` label applies (D-401). The owner merges. Then PR #11 starts the roadmaps on a new branch (D-488, D-490).

### Traps and gotchas

- D-484 and D-490 put the design-critic pass in PR #12. Do not run it in PR #10 or PR #11.
- D-487: in PR #11, ask a question only when it changes the order, a gate, or a contract between PRs. File each detail question in `docs/questions.md` with the PR it blocks.
- New PR ids start at PR-43 (D-486). PR-22 and PR-32 stay retired (G-10).
- Each file follows the focused roadmap template of the `design-doc-style` skill: the status header and sections 1, 5, 7, 8, and 9. Each phase entry lists its scope, exit tests, review focus, filed questions, and area file (D-144).
- The owner chose finer shapes than the session recommended twice in this block: two PRs, and twelve area files. Show running counts of files and PR ids in each batch.
- D-489 now binds PR #11: push its branch at each session end, and open it when the roadmaps and the rebuild are complete (D-490). PR #11 needs a branch of its own, such as `docs/pr-11-roadmaps`.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 24.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #10. Then a Codex session reviews PR #10 under the `pr-review` skill and writes `docs/reviews/pr-10.md` (D-401). The owner merges. Then a session starts PR #11 on a new branch. It reads D-144, D-145, D-484 to D-490, the `design-doc-style` skill, and section 7 of `docs/design.md`, and it writes `docs/roadmaps/area-core.md` first (D-488).

## Session 22: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #9 at effective head `5097e8a`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and all existing PR comments.
- Read the complete diff, the release entries of the design and decision documents, the questions register, the changed skills and guidance, the runbook, the world note, the handoff archive, the PR description, and prior review records.
- Checked D-480, D-481, and D-482 against their revision notes. The release facts, the aspect ratios, the supported targets, the macOS export form, and the sequence agree.
- Found no in-scope finding. The review record is `docs/reviews/pr-9.md` with the verdict `Ready for owner merge` for `5097e8a`.

### State of the build

- No code exists. `main` is `f4a1c6b` (PR #8).
- PR #9 is open on `docs/pr-9-release-block`. The effective head is `5097e8a`. This entry and the review record are metadata.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #9 waits for the owner to merge. The roadmaps docs PR follows (D-399).

### Traps and gotchas

- D-184 excludes only `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. The effective head is `5097e8a`, not this metadata commit.
- D-481 supersedes the arm64 Windows and Linux exports and their five CI legs. D-482 keeps a universal macOS export while supporting Apple silicon alone.
- GitHub API calls and `git fetch origin` hit environment errors during this review. The PR metadata and existing remote-tracking refs still identified the reviewed commits and Gitar result.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-483, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 23.

### Open questions that block progress

None for PR #9. OQ-56 waits for the roadmaps PR. OQ-57 and OQ-59 block the store page at Gate 2. OQ-58 blocks PR-40. OQ-3 waits for PR-3.

### Next concrete action

Push this review record and handoff entry. Then the owner can merge PR #9. The next session starts the roadmaps docs PR and asks OQ-56 first.

## Session 21: 2026-09-14, Claude Code

Author: Claude Code
Session: the release block of the full plan, and two aspect ratios, on branch `docs/pr-9-release-block`.

### What this session did, and why

- Session 20 (Codex) reviewed PR #8 at `511203c` with no finding. The owner merged PR #8 as `f4a1c6b` and asked what comes next.
- The handoff and D-399 put the release block docs PR next. The first lever of OQ-56 moves that PR after the first playable, so the session asked first. The owner kept the order (D-447).
- Three read-only research agents read Steamworks, Apple, Microsoft, GitHub, and Godot pages. The session fetched each key page again and checked the quotes before a fact entered a document.
- The release block ran in twelve batches, D-447 to D-480:
  - Versions and builds: 0.MINOR.PATCH until 1.0.0, and exports on every merge from PR-7 (D-448, D-449). The owner first added arm64 builds and arm64 CI legs (D-464, D-474).
  - Signing: macOS notarized on Steam from PR-40, and Windows unsigned (D-455, D-463). F-32 records the Apple fee that the cost model lacked.
  - GitHub: prologue tags alone on GitHub Releases, until the Steam demo (D-457, D-470). The repository goes private before paid content (D-456).
  - Steam: the native Linux build on the Deck, the rating Verified, engine input with one Steamworks call for glyphs, and Auto-Cloud on the folder `the-thing-below` (D-458 to D-461, D-465). PR-40 picks the binding (D-462, OQ-58).
  - Store: the store page at Gate 2, store text and capsule grids by sessions, a trailer from replays, one Next Fest, and the demo name "The Thing Below: Prologue" (D-452, D-471, D-472, D-475, D-476, D-478).
  - Studio and players: a studio name picked before the store page (OQ-57), a studio mark on the splash, credits in three places, crash files to a studio email, and trusted players after Gate 4 (D-450, D-451, D-467 to D-469, D-473). Achievements come with the full game alone (D-466).
  - The AI disclosure of the Steam content survey waits for OQ-59, before the store page review at Gate 2 (D-477).
- Mid-block, the owner asked for a variety of aspect ratios and a revision of D-229. After four answers in a few minutes, the game supports 16:10 and 16:9 alone, with black bars on every other shape (D-480). The answer lands in this PR, and `CLAUDE.md` and `AGENTS.md` no longer list exceptions to G-8 (D-479).
- After gitar approved `980e96c` with 0 comments, the owner cut the scope to four targets: Windows and Linux on x86_64, macOS on Apple silicon, and the Steam Deck (D-481). D-481 supersedes D-464 and D-474. The macOS build stays the official universal build, and the game supports Apple silicon alone (D-482).
- F-33 records five gaps that the block closed, and F-34 records the screenshot format of Steam.
- The session updated `docs/design.md`, `docs/questions.md`, `CLAUDE.md`, `AGENTS.md`, the PR template, three skills, the dev-machine runbook, and `docs/world/setting.md`.
- The handoff held eleven entries before this one, because Session 20 added its entry and moved none. Sessions 11 and 10 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `f4a1c6b` (PR #8).
- PR #9 is open on `docs/pr-9-release-block`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #9 answers the gitar pass. It adds decision rows, so a Codex session reviews it, and no `review-override` label applies (D-401). The owner merges. Then the roadmaps docs PR starts (D-399).

### Traps and gotchas

- The PR holds two concerns on owner instruction (D-479). The description names the second concern, so a reviewer does not read it as a break of G-8.
- D-480 took several answers: a wider view, a limit at 16:9, a crop of narrow screens, then 16:9 alone, then 16:9 and 16:10. Only the last answer is a row. Any text that names 21:9, 4:3, or a crop is stale.
- D-471 moves the Steam Direct fee, the EU and WIPO name checks, the store text, and the capsule art to Gate 2. OQ-57 and OQ-59 now block the store page at Gate 2, and OQ-57 also blocks the crash address of D-473.
- The roadmaps PR gives PR ids to the export job after PR-7, the store page work after Gate 2, the credits roll, and the trailer capture.
- The Steamworks pages do not say how Auto-Cloud settles a conflict or whether a demo app needs a fee. PR-40 checks both.
- D-464 and D-474 are superseded inside this PR. Any text that names arm64 builds for Windows or Linux, five exports, or five CI legs is stale.
- The owner often gives a custom answer that widens the scope. Ask the limits in the next batch, and confirm the final state before the rows.
- Gitar runs one pass by itself on a new PR. After a later push, post `Gitar review`, and count a pass only from a `Gitar` check run on the head.
- The next ids are D-483, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 22.

### Open questions that block progress

None for PR #9. OQ-57 and OQ-59 block the store page at Gate 2, and OQ-58 blocks PR-40. OQ-56 waits for the roadmaps PR, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #9. Then a Codex session reviews PR #9 under the `pr-review` skill and writes `docs/reviews/pr-9.md` (D-401). The owner merges. Then a session starts the roadmaps docs PR, asks OQ-56 first, and rebuilds sections 7 and 8 of `docs/design.md` (D-399).

## Session 20: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #8 at effective head `511203c`, on branch `docs/pr-8-docs-current`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, author provider, and the automated pass.
- Confirmed the provider gate. Session 19 identifies Claude Code as the author of the substantive changes, and Codex is the eligible reviewer.
- Read the complete diff, the design sequence, the decision and question registers, the project guidance, the handoff archive, and the PR description.
- Checked D-446 against D-442 and D-193. The partial revision leaves the other time rules of D-442 current.
- Confirmed that `AGENTS.md` and `CLAUDE.md` stay identical, the handoff has ten current sessions, and Sessions 9 and 8 moved word for word to the archive.
- Wrote `docs/reviews/pr-8.md` with no finding and the verdict `Ready for owner merge` for `511203c`.

### State of the build

- No code exists. `main` is `cf2b197` (PR #7).
- PR #8 is open on `docs/pr-8-docs-current`. The remote head is the review commit that holds this entry and `docs/reviews/pr-8.md`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` are identical.

### In flight

PR #8 is ready for the owner to merge. OQ-56 blocks only the roadmaps PR's rebuild of section 8. The release block docs PR follows this PR (D-399).

### Traps and gotchas

- The effective head is `511203c`, not the later metadata commit that publishes the review record (D-184).
- D-446 revises the rule for wrong things only. D-442 still sets the time of day for maps, and D-443 still governs night versions of place music.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist yet. The PRs named in `AGENTS.md` create them.
- OQ-3 remains open for branch protection and does not block this documentation PR.

### Open questions that block progress

OQ-56 waits for the roadmaps PR. OQ-3 waits for PR-3.

### Next concrete action

The owner merges PR #8. Then a session starts the release block docs PR and reads D-53, D-85, D-93, D-143, and the Phase 5 entries of `docs/design.md` before it asks the release questions.

## Session 19: 2026-09-14, Claude Code

Author: Claude Code
Session: the audit of every document before a context reset, after PR #7 merged, on branch `docs/pr-8-docs-current`.

### What this session did, and why

- Session 18 (Codex) reviewed PR #7 at `138e5cf` with no finding and the verdict `Ready for owner merge`. The owner merged PR #7 as `cf2b197`.
- The owner asked: "Ensure ALL docs are up to date in preparation for context reset."
- A search of the live documents for the old clock, the old audio rules, and the state of PR #7 found two stale texts:
  - `CLAUDE.md` and `AGENTS.md` named PR #2 as the one exception to G-8, but D-437 made PR #7 a second exception.
  - Step 2 of section 8 in `docs/design.md` did not show PR #6 and PR #7 as merged.
- Two owner items lived only in the conversation, and a reset would lose them. The session asked both:
  - The reading of D-442 on the wrong things. The owner chose placement by the story, at any time of day (D-446). D-442, D-193, and the effect of D-414 gained notes.
  - The levers for an earlier playable build. The owner filed them as OQ-56 for the roadmaps PR.
- The skills, the agents, the runbooks, the README, the PR template, and the world files hold no stale text. There, "clock" means the wall clock, and "phase" means a roadmap phase or a boss phase.
- `docs/design.md` gained a dated line for this pass.
- The handoff held eleven entries before this one, because Session 18 added its entry and moved none. Sessions 9 and 8 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `cf2b197` (PR #7).
- PR #8 is open on `docs/pr-8-docs-current`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #8 answers the gitar pass. It adds D-446, so it takes a Codex review, and no `review-override` label applies (D-401). The owner merges. Then the release block docs PR starts (D-399).

### Traps and gotchas

- The push line of `docs/reviews/pr-7.md` keeps the placeholder `<review metadata sha>`. The review commit is `a0cf252`. The record belongs to the reviewer, so this session left it as it is.
- The wrong things have no time rule now (D-446). Outside the dated records and the superseded rows, a text that says they walk after dusk or at night is stale.
- OQ-56 blocks only the rebuild of section 8 in the roadmaps PR. The release block docs PR comes first, unless the owner answers OQ-56 sooner.
- Gitar runs one pass by itself on a new PR. After a later push, post `Gitar review`. When a new dashboard appears with no `Gitar` check run on the head, post the second request within a minute.
- In the audio block, the owner often picked the fullest option, then cut scope for cost. Show the running count of tracks, light setups, or tests in each batch.
- The next ids are D-447, OQ-57, F-32, L-16, G-26, PR-43, M-7, and Session 20.

### Open questions that block progress

None for PR #8. OQ-3 waits for PR-3. OQ-56 waits for the roadmaps PR.

### Next concrete action

This session answers the gitar pass on PR #8. Then a Codex session reviews PR #8 under the `pr-review` skill and writes `docs/reviews/pr-8.md` (D-401). The owner merges. Then a session starts the release block as its own docs PR (D-262, D-399). It reads D-53, D-85, D-93, D-143, and the Phase 5 entries of `docs/design.md`, then asks the owner the release questions in batches.

## Session 18: 2026-09-14, Codex

Author: Codex
Session: review of PR #7 at effective head `138e5cf`, on branch `docs/pr-7-audio-block`.

### What this session did, and why

- Read the handoff, the project instructions, the `pr-review` and `ste-writing` skills, the design, decisions, questions, world, review, and PR documents.
- Verified the provider gate: Claude Code authored the substantive PR changes, and Codex reviewed them.
- Reviewed the complete PR diff. The audio decisions, the time-of-day supersession chain, the roadmap entries, the glossary, and the document dispositions agree at `138e5cf`.
- Verified the Gitar comment. The added D-190 note in `138e5cf` completes the back-reference to D-436.
- The effective head is `138e5cf`, not `f8ca3c3`, because `138e5cf` changes `docs/decisions.md`, which is outside the metadata set.
- The review record is `docs/reviews/pr-7.md`. It has no finding and gives the verdict `Ready for owner merge` for `138e5cf`.

### State of the build

- No code exists. `main` is `11498f1` (PR #6).
- PR #7 is open on `docs/pr-7-audio-block`. The remote head is the review commit that holds this entry and `docs/reviews/pr-7.md`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #7 waits for the owner to merge. Then the release block docs PR starts, followed by the roadmaps docs PR (D-399).

### Traps and gotchas

- A commit that changes `docs/decisions.md` is substantive. The effective head rule excludes only `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`.
- The Gitar dashboard comment is not the only evidence of a pass. The Gitar check passed, but `gh pr checks` also reported a GitHub API connection error.
- The build, test, format, Godot, and later gate checks do not exist until the PRs named in `docs/design.md` create them.

### Open questions that block progress

None for PR #7. OQ-3 waits for PR-3. The EUIPO, TMview, and WIPO checks wait for PR-40.

### Next concrete action

The owner merges PR #7. Then a session starts the release block docs PR.

## Session 17: 2026-09-14, Claude Code

Author: Claude Code
Session: the audio block of the full plan, and the change to a time of day that the story sets, on branch `docs/pr-7-audio-block`.

### What this session did, and why

- PR #6 merged as `11498f1`. On the owner instruction "continue work", the same harness run started the audio block, the next docs PR of D-399.
- The owner asked when a build to play and feel comes. From section 7 of `docs/design.md`: PR-7 is the first build to walk in, PR-10 and PR-11 give fights, and PR-17 is the first playable.
- PR-17 comes after three docs PRs and 23 code PRs. At 2 to 4 sessions per reviewed PR, that is about 50 to 100 sessions.
- The session named three levers for the roadmaps PR: the release docs PR later, PR-2 and PR-3 after PR-7, or a throwaway feel prototype. The owner chose none yet.
- The audio block ran in eight batches:
  - Style: 16-bit synthesized instrument voices for the music and the sound effects (D-412, D-423). D-87 keeps our own tool and no licensed sound.
  - Music everywhere: place tracks, three battle tracks per region, mood cues and key cues for scenes, and ten themes in region one (D-413, D-415, D-418, D-419).
  - Stings for a wipe, a level up, a victory, and a key find. Ambience under the music, map sounds, a sound family per kind, and soft menu sounds (D-422, D-424 to D-426, D-431).
  - The main theme on the title screen and at the end of region one (D-427). The place music plays on under menus, and after a battle the ambience plays alone before the track resumes (D-421, D-429).
  - The build renders the audio, and the repository commits a hash list, not WAV files (D-432, F-31). Sessions write the music as tracker rows, and the owner hears each batch with a listen command, and later in a sound room (D-433, D-438, D-439).
  - Vibration at heavy moments alone, and four more audio settings (D-434, D-435).
- Mid-block, the owner stopped time in dungeons, and set rest and travel rules (D-436, D-440, D-441). Minutes later, the owner removed the day clock: the story sets the time of day of each map (D-442).
- D-442 supersedes D-190, D-192, D-197, D-198, D-436, D-440, and D-441. D-443 and D-444 cut the night versions of the music, and D-445 removes the sun or moon mark from the HUD.
- The owner put the clock answer in this PR, so the PR holds two concerns (D-437, G-8).
- The session stated three readings. The owner kept two: ambush and elite fights play the common battle track (D-415), and the refuge counts as a cave and the sealed gallery as a mine (D-417). The third has no answer yet: the wrong things walk only on a map set to dusk or night (D-442).
- The session updated `docs/design.md`: a dated line, the system map, F-31, the cost model, PR-7, PR-8, PR-18, PR-33, PR-36, PR-38, and step 2 of section 8.
- It also updated the glossary of the `ste-writing` skill, the open items of `docs/world/setting.md`, and OQ-10 in `docs/questions.md`, and it added revision notes to 14 earlier rows.
- The handoff held ten entries before this one, so Session 7 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).
- The gitar pass on `f8ca3c3` approved with one suggestion: 1 comment, with merit. D-190 lacked its note for D-436, and the commit that holds this revision of the entry adds it. A script check found no other target row of D-412 to D-445 without its note.

### State of the build

- No code exists. `main` is `11498f1` (PR #6).
- PR #7 is open on `docs/pr-7-audio-block`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #7 answers the gitar pass. It adds decision rows, so no `review-override` label applies, and a Codex session reviews it (D-401). The owner merges. Then the release block docs PR, then the roadmaps docs PR (D-399).

### Traps and gotchas

- The PR holds two concerns on owner instruction (D-437). The PR description cites D-437, so a reviewer does not read the second concern as a break of G-8.
- D-442 ends the day clock. Outside the dated records, a text that names a phase of the day, the day clock, or a wait at a save point is now stale. Roadmap phases and boss phases stay. The glossary sets "time of day".
- Five rows of this PR are superseded inside the same PR: D-416, D-420, D-436, D-440, and D-441. They stay as a dated record, as D-395 and D-396 did.
- Region one needs about 20 tracks. Each night beat that the arc adds can need a night version of a place track (D-443).
- The synthesizer must render the same bytes on all three platforms, so it needs integer math (D-432). The PR-38 text carries that.
- The roadmaps PR gives PR ids to the rest of the audio player, the first music, and the sound room (PR-38, D-399).
- In this block, the owner often picked the fullest option, then cut scope for cost. Show the running count of tracks, light setups, or tests in each batch.
- The next ids are D-446, OQ-56, F-32, L-16, G-26, PR-43, M-7, and Session 18.

### Open questions that block progress

None for PR #7. OQ-3 waits for PR-3. The levers for an earlier playable build wait for the owner, and the roadmaps PR is their place.

### Next concrete action

This session answers the gitar pass on PR #7. Then a Codex session reviews PR #7 under the `pr-review` skill and writes `docs/reviews/pr-7.md` (D-401). The owner merges. Then a session starts the release block as its own docs PR (D-399).

## Session 16: 2026-09-14, Claude Code

Author: Claude Code
Session: the first session in the new checkout `/Volumes/SSD-1TB/the-thing-below`, and a docs PR that closes `docs/runbooks/rename-and-move.md`, on branch `docs/pr-6-close-move`.

### What this session did, and why

- PR #5 merged as `aee6f35` at 17:09:52Z, with the `review-override` label. The Session 15 entry went in before the merge, so it does not record what came after.
- After the merge, Session 15 ran two steps of the runbook:
  - Step 9: the clone to `/Volumes/SSD-1TB/the-thing-below`, clean at `aee6f35`.
  - Step 10: the copy of the local session notes from `~/.claude/projects/-Users-nate-Repos-terminal-rpg/memory/` to `~/.claude/projects/-Volumes-SSD-1TB-the-thing-below/memory/`.
- Step 11: this session opened in the new checkout. The interim STE check passed with 0 findings. `diff -r` of the two notes folders found no difference, and the session read its notes from the new folder.
- Before step 12, the session checked that the old checkout `~/Repos/terminal-rpg` held nothing that GitHub lacks:
  - The tree of its last branch tip `152fa62` is the tree of `aee6f35`.
  - `git ls-remote` shows each of its five local branch tips on GitHub, as `refs/pull/1/head` to `refs/pull/5/head`.
  - It had no stash, no untracked or ignored file, no `.claude/settings.local.json`, and no hook.
- Step 12: the owner deleted the old checkout. A check at 17:24:12Z found no folder at that path.
- The docs PR makes the documents show the move as complete:
  - `docs/runbooks/rename-and-move.md`: the status is complete, and steps 7 to 12 are marked done.
  - `docs/design.md`: a dated line for the move pass, and step 2 of section 8 shows PR #5 merged and the checkout on the SSD.
  - `docs/runbooks/dev-machine.md`: a dated fact for the checkout path.
- The PR changes no decision row. The owner answer on step 12 carries out a step that D-216 and D-400 already set, so it adds no row (D-68).
- The handoff held ten entries before this one, so Session 6 moved word for word to the top of `docs/session-handoff.md` (D-18).

### State of the build

- No code exists. `main` is `aee6f35` (PR #5).
- The checkout is `/Volumes/SSD-1TB/the-thing-below`. The old checkout no longer exists.
- PR #6 is open on `docs/pr-6-close-move`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #6 answers the gitar pass. It changes no decision row, and every path is in the override set, so the session applies the `review-override` label after the pass approves the head (D-67, D-401). The owner merges. Then the audio block docs PR starts (D-399).

### Traps and gotchas

- The checkout is on the external SSD. A session cannot open it when the Mac does not show `/Volumes/SSD-1TB`.
- The local session notes key on the checkout path, now `~/.claude/projects/-Volumes-SSD-1TB-the-thing-below/memory/`. The old folder `-Users-nate-Repos-terminal-rpg` still exists, and no session reads it now.
- Gitar runs one pass by itself when a new PR opens, even while the automatic passes are paused (PR #5).
- After a later push, post `Gitar review` two times. The first request runs the pass on the older head, and the second runs it on the new head (Session 13).
- Count a pass only when a `Gitar` check run on the head commit ends. The dashboard comment is not proof.
- The dated records keep `terminal-rpg` and `~/Repos/terminal-rpg` on purpose.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 17.

### Open questions that block progress

None for PR #6. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #6, and applies the `review-override` label when the pass approves the head. The owner merges. Then a session starts the audio block, the next docs PR (D-262, D-399). It reads the audio rows first: D-87, D-115, D-223, D-226, and PR-38 in `docs/design.md`. Then it asks the owner the audio questions in batches and records each answer. That PR adds decision rows, so the other provider reviews it (D-401).

## Session 15: 2026-09-14, Claude Code

Author: Claude Code
Session: the rename PR, steps 4 to 7 of `docs/runbooks/rename-and-move.md`, on branch `docs/pr-5-rename`.

### What this session did, and why

- PR #4 merged as `a16a83e` after the repeat review of Session 14. No other PR was open, so step 4 of the runbook started (D-411).
- The session started `docs/pr-5-rename` from `main`. The next GitHub number was 5.
- A search of every tracked file found 41 mentions of the working title. The dated records keep theirs: `docs/archive/`, the handoff, and the rows of D-9, D-72, and D-102 (step 6).
- The live documents now use the tentative name The Thing Below, and the commands use the names of D-217:
  - `CLAUDE.md` and `AGENTS.md`: the title, the sentence on the project names, and 10 command names each. The two files stay identical.
  - `README.md`: the title, and "The name is tentative."
  - `docs/design.md`: the title, the thesis, step 2 of section 8, and a dated line for the rename pass.
  - The `csharp-conventions` and `ste-writing` skills: the command names, and the technical name of the game.
  - `docs/runbooks/rename-and-move.md`: the status, and steps 4 to 6 marked done.
- The runbook keeps the old names in its table of names and in step 1, because those lines record the change.
- OQ-7 already names D-215, D-217, and D-410, so the questions register needs no note. No owner question came up for this PR (D-68), and the PR changes no decision row.
- The handoff held eleven entries before this one, because Session 14 added its entry and moved none. Sessions 5 and 4 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `a16a83e` (PR #4).
- PR #5 is open on `docs/pr-5-rename`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The local checkout is still `~/Repos/terminal-rpg`.

### In flight

PR #5 answers the gitar pass. It changes no decision row, and every path is in the override set, so the session applies the `review-override` label after the pass approves the head (D-67, D-401). The owner merges. Then steps 9 to 12 of the runbook follow: the clone to the SSD, the copy of the session notes, a new session in the new checkout, and the removal of the old checkout by the owner.

### Traps and gotchas

- Post `Gitar review` after each push, and count the pass only from a `Gitar` check run on the head (Session 13). A request runs the pass on the PR head at the previous request, so a second request can be necessary.
- The label needs a new approval after each push (D-67).
- Step 10 copies `~/.claude/projects/-Users-nate-Repos-terminal-rpg/memory/` to the folder of the new path, probably `-Volumes-SSD-1TB-the-thing-below`. Check the folder name after the first session in the new checkout.
- The dated records and the table of names in the runbook keep `terminal-rpg` on purpose. A search for the old name finds them.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 16.

### Open questions that block progress

None for PR #5. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #5 and applies the `review-override` label when the pass approves the head. The owner merges. Then a session runs steps 9 to 12 of `docs/runbooks/rename-and-move.md`.

## Session 14: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #4 at effective head `919a865`.

### What this session did, and why

- Read the current handoff, the prior review, the response file, the new substantive diff, and the current PR metadata.
- Verified the provider gate remains eligible. Claude Code authored the changes, and Codex reviewed them.
- Reproduced both prior corrections. The design-doc skill now matches the numbered sections of `docs/design.md`, and the name-search facts now include sources, queries, results, and dates.
- Confirmed the automated pass completed successfully on `919a865` with no issue comments.
- Updated `docs/reviews/pr-4.md`, preserved P2-1 and P2-2 with their earlier evidence, and set the verdict to `Ready for owner merge` for `919a865`.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. Its effective head is `919a865`; later commits contain metadata only.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` remain identical.

### In flight

PR #4 is ready for owner merge. After merge, step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- The live `gh pr checks` call returned a GitHub API connection error during this review. The handoff records the successful Gitar check run on `919a865`.
- The review applies to `919a865`, not the later handoff-only tip `09ace6c`.
- OQ-3 remains open for branch protection.

### Open questions that block progress

None for PR #4.

### Next concrete action

The owner can merge PR #4. Then run step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 13: 2026-09-14, Claude Code

Author: Claude Code
Session: the gitar pass on the answer to the review of PR #4, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- Session 12 pushed `919a865` at 15:02:16Z. The pass on that head had no result when that session stopped, so this session read the result on GitHub.
- The repository activity API shows no push after `919a865`. Three `Gitar review` requests came after that push, the last at 15:36:57Z.
- Gitar edited its dashboard comment at 15:37:15Z. The comment says approved, with no issue found. The `Gitar` check run on `919a865` ended with `success` at 15:37:22Z.
- The PR holds 0 review threads, 0 line comments, and 0 reviews. The pass on `919a865` has 0 comments, 0 with merit, and no fix commit.
- The pass is complete (the `pr-review` skill, "The automated pass"). The PR adds decisions, so no `review-override` label applies (D-401).
- The handoff held ten entries, so Session 3 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).
- After the first push of this entry as `92926a6`, the session requested a pass with `Gitar review` at 15:46:41Z, 41 seconds after the push.
- Gitar ran the pass on `919a865` again, not on `92926a6`. The check run on `919a865` started at 15:46:46Z and ended with `success` at 15:47:22Z.
- Gitar deleted its dashboard comment and posted a new one at 15:47:20Z. The new comment says approved and repeats the old summary word for word.
- The check suite of gitar on `92926a6` stayed `queued`, with 0 check runs. The poll of the session waited for a check run on `92926a6`, and it failed at its time limit.
- The owner saw the new dashboard comment first. The session at first read it as a pass on `92926a6`, then the check runs showed the old head.
- This revision of the entry corrects the traps on the result of a pass. The PR description records the result of each pass on the tip.
- The session pushed that revision as `4ce3138` at 16:08:16Z. It waited 5 minutes, then requested a pass at 16:13:17Z.
- Gitar ran the pass on `92926a6`, not on `4ce3138`. The check run on `92926a6` started at 16:13:22Z and ended with `success` at 16:13:58Z.
- The poll saw the new dashboard comment with no check run on `4ce3138`, and it reported that at once.
- The check runs of every request fit one rule: a request runs the pass on the commit that was the PR head at the request before it.
- This third revision of the entry records that rule in the traps.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. The remote head is the commit that holds this entry, above `919a865`.
- The effective head stays `919a865`, because the commit that holds this entry changes the handoff files alone (the `pr-review` skill).
- The gitar pass on `919a865` is complete. The PR description records the pass on the tip that holds this entry.
- The interim STE check passes with 0 findings.

### In flight

PR #4 waits for a gitar pass on the tip that holds this entry. Then a Codex session runs the repeat review of `919a865` (the `pr-review` skill, "Repeat review procedure"). The owner merges. Then step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- Count a gitar pass only when a `Gitar` check run on the head commit starts after the request and ends. `gh api repos/{owner}/{repo}/commits/<sha>/check-runs` reads it.
- The dashboard comment is not proof. Requests 3 seconds, 41 seconds, and 5 minutes after a push ran the pass on the older commits `cb6e96e`, `919a865`, and `92926a6`. Each dashboard said approved.
- A request runs the pass on the PR head at the previous request, so a wait after a push does not help. After a push, post `Gitar review` and wait for its check run.
- Then post `Gitar review` again. That second request runs the pass on the new head.
- A check suite of gitar in the state `queued`, with 0 check runs, means that no pass ran on that commit. `2072219`, `c73c19f`, and `92926a6` show that state.
- Gitar can edit its dashboard comment or replace it with a new one. Read the newest gitar comment that contains "Code Review".
- The REST API names the bot `gitar-bot[bot]`, and `gh pr view` names it `gitar-bot`. A filter on one exact login finds nothing in the other form.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push, and wait for the check run on the head.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 14.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session gets a `Gitar` check run on the tip that holds this entry, and the PR description records it. Then a Codex session runs the repeat review of PR #4 at `919a865` and updates `docs/reviews/pr-4.md`. The owner merges. Then a session runs step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 12: 2026-09-14, Claude Code

Author: Claude Code
Session: the answer to the review of PR #4, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- The review in `docs/reviews/pr-4.md` gave `Changes required` at `cb6e96e`, with P2-1 and P2-2. The session pulled the two review commits first.
- P2-1, full merit: the template of the `design-doc-style` skill numbered the status header as item 1, so every section number sat one above the headings of `docs/design.md`. The status header is now unnumbered, and the list numbers 1 to 9 match the headings.
- P2-2, full merit: the name search had no dated source in the repository. The session ran each check again and wrote the URLs, the queries, the results, and the controls into the external facts of `docs/design.md`. D-408 and the rename runbook point there.
- `docs/reviews/pr-4-response.md` records both dispositions.
- The handoff held eleven entries before this one, so Sessions 1 and 2 moved to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. The commit that holds this entry is the new effective head, above the review commits `2072219` and `c73c19f`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #4 answers a new gitar pass, then takes a repeat review on the new effective head (the `pr-review` skill). The owner merges. Then step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- The USPTO search service has no public documentation. The POST body in the external facts worked on 2026-09-14, and its controls prove the `WM` field. A later change of the service can break the query.
- The EUIPO, TMview, and WIPO checks stay open for PR-40 (D-408).
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 13.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on the new head. Then a Codex session runs the repeat review of PR #4 and updates `docs/reviews/pr-4.md`. The owner merges.

## Session 11: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #4 at effective head `cb6e96e`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and existing PR comments.
- Read the complete diff, the design roadmap, the decision and question registers, the changed skills and agent files, the runbooks, the world files, and the PR description.
- Confirmed the automated pass approved the head with no issue comments. The local interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` remain identical.
- Found P2-1: the changed `design-doc-style` skill gives section numbers that do not match `docs/design.md`.
- Found P2-2: the material name-search record has no dated primary-source links or repeatable query record in the repository documents.
- Wrote `docs/reviews/pr-4.md` with the verdict `Changes required` for `cb6e96e`.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open at `cb6e96e` on `docs/pr-4-docs-audit`.
- The interim STE check passes with 0 findings. No solution, Makefile, CI, review-gate, or Godot project exists yet.

### In flight

PR #4 waits for the author to correct P2-1 and P2-2, push the changes, request the automated pass, and take a repeat review at the new effective head.

### Traps and gotchas

- `docs/design.md` uses section 5 for the defect register, section 6 for guardrails, and section 7 for the roadmap. The changed design-doc skill says 6, 7, and 8.
- The repository name search remains tentative because EUIPO, TMview, and WIPO did not answer. PR-40 owns the later verification.
- The review record is metadata. A later metadata commit does not change the effective implementation head, but a substantive correction does.

### Open questions that block progress

None. OQ-3 remains open for branch protection and does not block this review.

### Next concrete action

Correct P2-1 and P2-2, run the focused checks and the interim STE check, then repeat the review on the new effective head.

## Session 10: 2026-09-14, Claude Code

Author: Claude Code
Session: the start of the rename runbook after PR #3 merged, then a docs PR that makes every document current before the rename PR, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- PR #3 merged as `d29921d`. The preconditions of `docs/runbooks/rename-and-move.md` held: no open PR, the SSD mounted, and the target path and the GitHub name free.
- A search for "The Thing Below" found no game on Steam and no United States mark. A free jam game on itch.io is called "The Thing Beneath", and its devlog once says "The Thing Below". The EU and WIPO registers did not answer. The owner chose to go ahead and to start now (D-408, D-409).
- Steps 1 to 3 of the runbook ran: the GitHub repository is `nkramber/the-thing-below`, and the local `origin` points at it. The owner's instruction "Ensure ALL docs are up to date before you rename/move repo" arrived after those steps. The owner kept the new name (D-410) and chose a current-state audit in its own docs PR before the rename PR (D-411).
- Three read-only audit agents read the design, the world files with the questions register, and the process files. Two scripts checked the revision notes of the decision register and the file paths in the documents. The session verified each finding against its source before a change.
- The fixes cover these files:
  - `docs/design.md`: the status header, the system map, F-2, F-3, F-9, T-4, PR-1, PR-6, PR-10, PR-14, PR-17, PR-34, PR-37, PR-40, the Phase 2 gate, and section 8.
  - `docs/questions.md`: nine notes.
  - `docs/world/`: three items.
  - `CLAUDE.md` and `AGENTS.md`: the override set and the Python exceptions.
  - The PR template, four skills, and one agent file.
  - Both runbooks, the docstring of `docs/tools/ste-check.py`, and the Rust block of `.gitignore`.
- D-78 gained its note for D-98. F-30 records the audit.
- Findings the session did not change: the open item on the two months after region one in `places.md` already defers to region two (D-353). PR-35 keeps "one hub and one dungeon" as the first nodes, because no decision says whether the village is a node.

### State of the build

- No code exists. `main` is `d29921d` (PR #3) on `nkramber/the-thing-below`.
- Branch `docs/pr-4-docs-audit` holds one commit above `main`, the commit that holds this entry.
- The interim STE check passes with 0 findings.
- The local checkout is still `~/Repos/terminal-rpg`. The documents keep the working title until the rename PR.

### In flight

PR #4, the docs audit, answers the gitar pass, then takes a Codex review, because it adds D-408 to D-411 (D-401). The owner merges. Then step 4 of the runbook starts the rename PR, and the clone to the SSD and the copy of the session notes follow (D-400, D-411).

### Traps and gotchas

- The GitHub repository has a new name. The old URL redirects, but set `origin` to `git@github.com:nkramber/the-thing-below.git` in any other checkout.
- The rename PR swaps the title and the project names alone. This PR already changed the facts about the GitHub repository in `docs/design.md` and `docs/runbooks/dev-machine.md`.
- The session notes of Claude Code key on the checkout path. Step 10 of the runbook copies the memory folder after the clone.
- Do not renumber the steps of `docs/runbooks/rename-and-move.md`: D-400 cites step 10 by number.
- `grep` on this machine is `ugrep`, which rejects a long bounded repeat such as `.{0,120}`. Use Python for a context search.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 11.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #4. Then a Codex session reviews PR #4 under the `pr-review` skill and writes `docs/reviews/pr-4.md` (D-401). The owner merges. Then a session runs step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 9: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #3 at effective head `f684ed5`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, and all three substantive commits.
- Confirmed the provider gate. The handoff identifies Claude Code as the author, and Codex is the reviewer.
- Read the complete diff, the design roadmap, the decision and question registers, the cast file, the project guidance, the atlas script, the sample readme, the five grids, and both review sheets.
- Confirmed that the sample grids have 32 rows of 32 characters, all keys exist in the 48-color palette, and the visual sheets match the stated sample.
- Confirmed that the deleted 16 by 16 files have no broken current consumer. The retained atlas script fails with the documented contextual error until PR-34 ports it.
- Wrote `docs/reviews/pr-3.md` with the verdict `Ready for owner merge`.

### State of the build

- No code exists. `main` is `7375310` (PR #2).
- The effective head is `f684ed5`. The review commit and this handoff entry are metadata commits and do not change that head.
- The interim STE check passes with 0 findings. `git diff --check origin/main...HEAD` is clean.
- PR #3 is open. The automated pass approved the final head with zero issues. No CI or review-gate checks exist yet.

### In flight

PR #3 is ready for owner merge. After merge, the next work is the rename and move in `docs/runbooks/rename-and-move.md` (D-400).

### Traps and gotchas

- Skip `docs/samples/` during automatic exploration (D-403), except when the owner or the handoff points to it.
- PR #3 is the GitHub PR number for the sprite sample. Roadmap PR-3 is the later review-gate item.
- The interim atlas tool now fails with `no .grid file` because D-405 removed the old content. PR-34 ports the tool to 32 by 32 grids.
- Automatic passes are paused for the period. The owner posted `Gitar review` after each push.

### Open questions that block progress

OQ-3 remains open for branch protection after PR-3 merges. It does not block the owner merge of this documentation PR.

### Next concrete action

Commit and push this review record and handoff. Then the owner can merge PR #3. The next session runs `docs/runbooks/rename-and-move.md` after the merge.

## Session 8: 2026-09-14, Claude Code

Author: Claude Code
Session: draft 32 by 32 cast sprites for owner review, then a small docs PR that saves the approved look as a sample and removes the 16 by 16 test sprites, on branch `docs/pr-3-sprite-sample`.

### What this session did, and why

- While PR #2 waited for its repeat review, the owner asked for new sprite sheets to review. The session drew front sprites of Marrek, Bergit, Dagvar, Ottild, and Elio at 32 by 32 in the test style (D-201, D-233, D-237, D-289), on the 48-color palette, as material maps that a scratchpad script shaded and rendered. A second draft fixed banded faces, the pick of Marrek, and the cloak of Ottild.
- The owner said that the look works and asked to save it as a sample in a small PR (D-402). The owner chose `docs/samples/`, with a rule that sessions skip the folder during automatic exploration (D-403), and the sheets and grids without the script (D-404).
- Added `docs/samples/readme.md` and `docs/samples/2026-09-14-cast-sprites/` (two sheets and five grids), the skip rule in `CLAUDE.md` and `AGENTS.md`, revision notes on D-20 and D-233, and pointers in PR-34 and `docs/world/cast.md`.
- PR #2 merged before this branch started, so the branch starts from `main` at `7375310`.
- The owner asked whether the rest of `content/sprites/` was out of date. The four 16 by 16 grids and `atlas.png` were, and the palette was not: its 48 colors stay the first 48 of the palette, and the sample uses them. The owner chose to remove the grids and the atlas in PR #3 (D-405, D-407) and to keep `docs/tools/make-atlas.py` as a reference with an out-of-date notice (D-406). The session had recommended the removal of the tool. The change adds revision notes on D-119, D-233, and D-402, and updates PR-34, `docs/samples/readme.md`, and `docs/world/cast.md`.

### State of the build

- No code exists. `main` is `7375310` (PR #2).
- Branch `docs/pr-3-sprite-sample` holds three commits above `main`: `e4a937e`, which opened PR #3, `6d8b5a7`, which splits one long sentence in `docs/samples/readme.md` that the STE check flagged, and the commit that holds this revision of the entry (D-405 to D-407).
- The interim STE check passes with 0 findings. The interim atlas tool finds no grid to read, and it carries an out-of-date notice until PR-34 ports it (D-406).

### In flight

PR #3 answers the gitar pass, then takes a Codex review, because it adds decisions (D-401). Then the owner merges. After that, the plan of Session 4 stands: the rename and the move (D-400), then the audio, release, and roadmaps docs PRs (D-399).

### Traps and gotchas

- Skip `docs/samples/` during automatic exploration (D-403).
- The branch name carries the GitHub number 3. Roadmap PR-3, the review gate, is a different item (D-13).
- Two untracked concept images sat in `content/sprites/`: `party-characters-32.png` and `party-sample-sheet-concept.png`. This session did not make them, they never entered a commit, and they are not part of D-402. The owner asked to delete them. After D-405, `content/sprites/` holds `palette.json` alone.
- The sample grids use the 48-color palette. PR-34 grows the palette to 64 (D-181, D-185), so the sample can change there.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- A multi-line guard with `set -e` did not stop at the failed STE check in this shell, so `e4a937e` went out with one STE finding. Test the exit code of each check on its own before a commit.
- `python3 docs/tools/make-atlas.py` now exits with code 1 and the message "no .grid file". That result is expected (D-405, D-406). Do not restore the 16 by 16 grids to make the tool pass.
- The next ids are D-408, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 9.

### Open questions that block progress

None for PR #3. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #3. Then a Codex session reviews PR #3 under the `pr-review` skill and writes `docs/reviews/pr-3.md` (D-401). The owner merges. The next Claude Code session runs `docs/runbooks/rename-and-move.md` (D-400).

## Session 7: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #2 at effective head `6586c7c`.

### What this session did, and why

- Verified the author response and the new effective head after the two prior findings.
- Reproduced P1-1 and P3-1 from the earlier review. Both corrections pass.
- Confirmed that the PR-9 gate now separates persistent Poison, Blind, and Silence from statuses that end with battle (D-390).
- Confirmed that `git diff --check origin/main` reports no whitespace error.
- Confirmed that the refreshed automated pass approved the corrected head with no new comment.
- Updated `docs/reviews/pr-2.md` with the prior finding history and the verdict `Ready for owner merge`.

### State of the build

- No code exists. `main` is `9dd80da`.
- The effective head is `6586c7c`. The pushed review commit is `3b73229`. Later metadata commits do not change the effective head.
- The interim STE check passes with 0 findings.
- No GitHub checks are reported. PR-1 and PR-3 create the build and review-gate checks.

### In flight

PR #2 is ready for owner merge after the repeat review. The next work is the rename and move in `docs/runbooks/rename-and-move.md` (D-400).

### Traps and gotchas

- Keep both finding ids and the earlier verdict in `docs/reviews/pr-2.md`.
- A new substantive head needs another repeat review. Metadata commits do not change the effective head.
- The automated pass is paused for the period. Post `Gitar review` after each substantive push, as D-66 requires.

### Open questions that block progress

None for PR #2. OQ-3 waits for PR-3.

### Next concrete action

Commit and push this review record and handoff. Then the owner can merge PR #2. A later session runs the rename and move procedure.

## Session 6: 2026-09-14, Claude Code

Author: Claude Code
Session: the author's answer to the review of PR #2 (`docs/reviews/pr-2.md`, verdict `Changes required` at `4b3d04e`), on branch `docs/pr-2-world-building`.

### What this session did, and why

- Read the review record and reproduced both findings on `d42a1a1`, the tip after the review commits.
- P1-1, full merit: the PR-9 gate asserted that every status ends, against D-390. The gate now asserts two classes: every status but poison, blind, and silence ends with its battle, and those three remain after it. The map and menu rules of the three stay in PR-16.
- P3-1, full merit: removed the trailing space from three lines of `docs/design.md` (the thesis, PR-4, and PR-7). `git diff --check origin/main` is clean.
- Wrote `docs/reviews/pr-2-response.md` with each disposition, correction, and regression check. No new D-#, OQ-#, or F-# id.
- Checked the PR for other feedback: no new automated comment, no line comment, and no review on GitHub.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- PR #2 is open. The commit that holds this entry changes `docs/design.md`, so it is the new effective head, and the verdict on `4b3d04e` no longer covers it.
- The interim STE check passes with 0 findings, and `git diff --check origin/main` is clean.
- The session requested an automated pass on the new head with the comment `Gitar review`, and the PR description records the result. CI and the review gate do not exist yet (PR-1, PR-3).

### In flight

PR #2 waits for a repeat review of the new effective head (the `pr-review` skill, "Repeat review procedure"). When the review record reads `Ready for owner merge` for that head, the owner merges. After the merge, the plan of Session 4 stands: the rename and the move (D-400), then the audio, release, and roadmaps docs PRs (D-399).

### Traps and gotchas

- The reviewer updates the same `docs/reviews/pr-2.md`: keep the finding ids, set each status line, and put the earlier verdict under `## Earlier verdicts`.
- The response file is a convention, and the review gate does not read it.
- Automatic passes are paused for the trial period. Post `Gitar review` after each push, and read the newest dashboard comment by its time.
- Session 5 cites D-184 for the metadata rule. D-184 is the normal-map tool, and the rule lives in the `pr-review` skill with no D-# id.
- The next ids are D-402, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 7.

### Open questions that block progress

None for PR #2. OQ-3 waits for PR-3.

### Next concrete action

A Codex session runs the repeat review of PR #2 at the new effective head, verifies P1-1 and P3-1 against their regression checks, and updates `docs/reviews/pr-2.md` with its own handoff entry. If the verdict is `Ready for owner merge`, the owner merges. Then a Claude Code session runs `docs/runbooks/rename-and-move.md` (D-400).

## Session 5: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #2 at effective head `4b3d04e`.

### What this session did, and why

- Verified the PR target, base, branch, tip, changed files, and automated pass on GitHub.
- Confirmed that the tip `f748ee3` changes only the handoff, so the effective implementation head stays `4b3d04e` (D-184 rule in the `pr-review` skill).
- Read the design, decisions, questions, world files, runbook, changed skills, PR description, and full PR diff.
- Found P1-1: the PR-9 exit test says every status ends, but D-390 makes Poison, Blind, and Silence persist past battle.
- Found P3-1: `git diff --check` reports trailing whitespace on three added lines in `docs/design.md`.
- Wrote `docs/reviews/pr-2.md` with the verdict `Changes required`.

### State of the build

- No code exists. `main` is `9dd80da`.
- The PR tip is `af7861b`. The effective head under the metadata rule is `4b3d04e`.
- The interim STE check passes with 0 findings.
- No GitHub checks are reported. PR-1 and PR-3 create the build and review-gate checks.

### In flight

PR #2 waits for the author to correct P1-1 and P3-1, push the changes, and request another review pass. A new effective head needs a repeat review. The automated pass has no open comment.

### Traps and gotchas

- Keep the current verdict under the exact `## Verdict` heading. Keep this finding id on a repeat review.
- Review the new effective head after the correction. Do not retain approval across a substantive push.
- The automated pass is paused for the period. Post `Gitar review` after each push, as D-66 requires.

### Open questions that block progress

OQ-3 waits for PR-3. It does not block the correction of this review.

### Next concrete action

The author corrects the PR-9 gate and the three whitespace errors, then pushes. The next Codex session re-reviews the new effective head and updates the same review record.

## Session 4: 2026-09-13, Claude Code

Author: Claude Code
Session: the four critic questions left from Session 3, the arc block of the world-building interview (OQ-18), and then the systems block of the full plan, on branch `docs/pr-2-world-building`, posted for review as PR #2 at the end (D-398).

### What this session did, and why

- Resumed from the Session 3 entry and asked OQ-52 to OQ-55 as one batch (D-305 to D-308). Every cast member is an adult, the lead always walks the map, the banned list gains one FF7 device (a gem or orb that stores power), and Elio stamps rites at the license office alone. Before the ask, the session corrected an overstated con in OQ-55 and added a third row to OQ-54. The owner chose the gem or orb alone.
- Ran the arc block of region one in eleven batches (D-309 to D-355). `docs/world/arc.md` holds the story in order.
- The spine: Marrek fights alone near his village, and Bergit joins because she needs a witness. The party frees Dagvar from the hanging cells, the church sends Elio to spy, and Ottild joins with the way into the deep mine. In the mine the party finds the crew that the guild sealed in alive, a wrong thing made by the blood of the war, and the mark of Marrek's parent, who got out alive. Elio turns. Church wardens capture the party, which breaks out of the cells, kills the bishop, flees through the gallery to the refuge, and passes the town by night. The wardens raid the refuge. The bandits of the fort sell the party, and the last fight is the captain of the wardens on the ice. The one set choice so far: spare or kill the captain.
- Owner reframings: the relationship value per character and faction reputation left the game, and a choice is a fixed story flag (D-328, D-329). Elio is the one death in the cast, after region one, and he must be innocent and lovable (D-321, D-322). Harm to a child is never shown directly, but text can imply or state it, and scenes can show aftermaths (D-335). Marrek fights alone first, and the others join one at a time (D-336). The guild is neither evil nor good (D-324).
- Two clashes surfaced, and the owner settled both: D-290 against the set turn of Elio (D-328), and one picked choice against "two or three" (D-355).
- Swept the registers, `cast.md`, `places.md`, `setting.md`, `banned-devices.md`, rule 13 of the `game-text-style` skill, and `docs/design.md`. PR-9 plans one to three fighters, PR-17 builds the village, the town, and the cells, and PR-19 lost reputation and relationships. The Phase 3 gate changed, and PR-23 to PR-26 each name one dungeon build (F-29). OQ-42, OQ-45, and OQ-46 lost options that the new decisions void, and OQ-41 now names Elio.
- The arc block landed as `4e76c4f` and was pushed. On owner instruction, the session then ran the systems block in the same session (D-356 to D-397).
- Lessons: slots on the character that swap at hubs and save points, growth per character and per lesson, and an aptitude bonus, half for a side aptitude (D-356 to D-361). Every character has a basic attack (D-359). The first playable holds Marrek, Bergit, and Dagvar (D-362), and a newcomer joins at a set level (D-363).
- Battle: action delay on the timeline, a front row and a back row per side, a step between rows that costs time, and a flee with a chance and a grace time on the map (D-376 to D-381). Items restore less in battle, stacks are small, a find over the limit stays where it lies, and a small set of items gets used up (D-382 to D-385). A steal takes from a list per enemy, and a Theft drill opens marked locks and disarms traps (D-383, D-386).
- The law and lessons: the party never gets a license or a stamp, and the story alone carries the risk (D-366, D-367). Lessons come from treasure, shops, and people (D-365), and the lessons and gear of Elio die with him (D-364).
- Levels and statuses: a downed character earns half experience, a soft cap per region holds the range, and a save point restores MP alone (D-387 to D-389). Poison, blind, and silence last past a battle, poison can down on the map, and a map wipe reloads even with a healthy reserve (D-390, D-392, D-393, D-397). Mend rites and cures work from the menu, and cures belong to Mend (D-391, D-394).
- Mid-block, the owner moved the start of the game to a small village on the road below the mining town, where Marrek grew up (D-368 to D-373). Winter beasts are his first foes, and Bergit finds him while she guards the road for coin. D-346 is superseded, and D-284 and D-250 are revised in part.
- The owner stopped the first systems batch to ask what a lesson is. The session explained it and now glosses the terms in every question.
- On owner instruction, the session posted the plan through the systems block as PR #2 for review (D-398). The owner set what follows the merge: the rename and the move first, then the audio block, the release block, and the roadmaps as one docs PR each (D-399, D-400). A docs PR that adds or revises a decision takes a Codex review, and the `review-override` label stays for the other docs PRs (D-401). The session updated `CLAUDE.md`, `AGENTS.md`, the PR template, the `pr-review` skill, the runbook, the label description, and section 8 of the design doc to match.
- Opened PR #2 at `4b3d04e`. The automated pass approved that head with no comment: zero comments, zero with merit, and no fix commit. Its note says that automatic passes are paused for the trial period, so each later push needs the comment `Gitar review`.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds eleven commits above `main`: the five of Session 2, the two of Session 3, `4e76c4f` (the arc block), `02051a2` (the systems block), `4b3d04e` (D-398 to D-401, the head that opened PR #2), and the commit that holds this revision of the entry. That last commit changes the handoff alone, so the effective head for the review stays `4b3d04e` (`pr-review` skill).
- The interim STE check passes on every non-exempt `.md` file, `docs/world/arc.md` included.
- PR #2 is open against `main`. The automated pass approved `4b3d04e` with no comment. This entry is a metadata commit above that head, so the session requested a new pass on the new head with the comment `Gitar review`, and the PR description records the result. CI and the review gate do not exist yet (PR-1, PR-3).

### In flight

PR #2, the plan through the systems block (D-398). Blocks done: setting, technical, graphics, UI, places, cast, arc, and systems. PR #2 cleared the gitar pass with no comment, and it now waits for a Codex review, because it adds decisions (D-401). Then the owner merges. After the merge, in order (D-399, D-400):

- The rename to the-thing-below and the move to the external SSD, by `docs/runbooks/rename-and-move.md` (D-216, D-400).
- The audio block as its own docs PR, then the release block as its own docs PR (D-262, D-399).
- The roadmaps as their own docs PR: the five phase roadmaps and the area roadmaps (D-144, D-145), a PR-# id for every new system (C-10 of the first critic pass), sections 7 and 8 of `docs/design.md` rebuilt from them, and another design-critic pass. The village and the land near it ride in PR-17 (D-369, D-370), and the second visit to the cells is PR-24 (F-29).
- Each of the three plan PRs adds decisions, so each takes the gitar pass and a Codex review (D-401). Then the Deck test (D-160) and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- PR #2 is open. Answer the gitar pass after each push (D-66), and do not apply the `review-override` label, because the PR adds decisions (D-401). The PR description and comments name no provider (D-22). Automatic passes are paused for the trial period, so post `Gitar review` after each push, and read the newest dashboard comment by its time.
- Relationships and reputation are gone (D-328, D-329), and D-40, D-242, and D-290 are revised in part. A systems option that uses standing, reputation, or a relationship value is void. OQ-42 and OQ-45 mark their void options.
- The owner often answers with long free text that sets several beats at once. Split it into rows, confirm a typo as a reading inside the next question, and ask at once about any clash with an earlier decision, quoting both. D-318 records the reading "imprisoned", which the owner kept.
- The banned list holds one FF7 device alone (D-307). The owner declined bans on a pumped power and on the death of a healer at the hand of the villain, so do not add them back.
- A battle holds one, two, or three characters (D-336). The first playable holds Marrek, Bergit, and Dagvar (D-362), so PR-14 and PR-16 test the swaps with a fixture party of four.
- Lesson growth belongs to the character, not to the item: a lesson passed back resumes at the level of its earlier owner (D-361).
- D-346 is superseded: the first fights happen near the village, and the cellars under the town have no role (D-370). D-45 lost its line on consumables (D-384). The owner switched the map-wipe rule twice: D-395 and D-396 are superseded, and D-397 keeps a wipe with no reserve, on the map and in battle.
- Gloss lesson, rite, drill, kind, and aptitude in every question batch. The owner does not answer a batch until each term is plain.
- Several decisions carry a known cost from their option: the gallery needs a second passage (D-343), the old galleries reach toward the pass (D-344), the fort repeats the beat of the cells (D-341), and a spared captain must return (D-354). The roadmaps and the content PRs must meet them.
- The arc keeps open items for the content PRs: the names of the bishop, the priest, the captain, and the survivor, the place of the confrontation, the personal tasks (D-352), and one or two more set choices (D-355).
- The STE checker flags "standing" after a preposition as an -ing form.
- On the picks of this block, the owner chose against the recommendation or wrote a custom answer about half the time. Keep options that differ in kind, with honest cons.
- The next ids are D-402, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 5.

### Open questions that block progress

No systems question remains open, and the audio and release blocks have no filed questions yet. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The gitar pass on PR #2 is complete, with no comment. The next action belongs to a Codex session: review PR #2 under the `pr-review` skill against the effective head `4b3d04e`, write `docs/reviews/pr-2.md`, and push it with its own handoff entry (D-17, D-401). If that review finds defects, a Claude Code session answers them in `docs/reviews/pr-2-response.md`. The owner merges. The next Claude Code session runs the rename and the move by `docs/runbooks/rename-and-move.md` (D-400), then starts the audio block on a new branch as its own docs PR (D-399). Audio already holds D-87, D-115, and D-223. The first audio topics: the style of the music after the move to sprites (D-98), music per place and per phase of the day (D-192), battle and boss music, sounds for the battle effects of D-186, and the mix settings of D-226. That session records each answer from D-402 on.

## Session 3: 2026-09-13, Claude Code

Author: Claude Code
Session: the cast block of the world-building interview (OQ-18), on branch `docs/pr-2-world-building`, with no PR yet (D-147). The owner replaced the job system during the block, and a second design-critic pass followed.

### What this session did, and why

- Resumed from the Session 2 entry and asked the cast block in batches (D-24). D-267 to D-300 record the answers, and `docs/world/cast.md` holds the cast.
- The owner replaced the FF5 job system in two steps: first a fixed role and a side role per character (D-268), then no classes at all and a system in the shape of FF7 materia (D-272). Abilities come from lessons: rites for spells and drills for physical abilities (D-275, D-278). Each character has a main aptitude and a hidden side aptitude that a missable personal task unlocks (D-274, D-282, D-283). Eight kinds: Mend, Harm, Blight, Boon, Blade, Guard, Shot, and Theft (D-281).
- Cast rules: one lead, and the map follows the lead in the party or in reserve (D-267, D-292). No two characters share a main aptitude or a side aptitude. The one exception: the replacement can share the main aptitude of the one dead character (D-274, D-279, D-303). The cast holds eight: five in region one, then two new characters and the replacement after it (D-280, D-299).
- The party of region one (D-284 to D-298): Marrek, the lead, 19, Blade and Guard. Bergit, a warden who lost her guild mark, 31, Guard and Shot. Dagvar, a hexer who tends the waystones, 35, Harm and Blight. Ottild, a cutpurse who knows the old tunnels, 21, Theft and Blade. Elio, a foreign clerk of the license office sent to watch the party, 24, Mend and Boon. A foreign name can take three syllables (D-300).
- The owner stopped a check of what-you-carry for a system to port. Other repositories guide the documents alone, and every tool is new code (D-277). The "Port" lines of PR-2, PR-3, PR-4, PR-5, PR-15, and PR-38 now say so.
- The session added revision notes to more than 30 older decisions, closed OQ-34, filed OQ-38 to OQ-47 for the systems block, retired PR-22, and swept the design doc, the world files, the glossary, the skills, and the README. Two FFT devices joined the banned list: a noble who hides the family name, and two friends of noble and common birth split by betrayal.
- The owner chose a critic pass before the handoff. The design-critic agent found 14 defects (F-28). The session verified each one against the files, fixed the document defects, and rejected one claim (D-282 refines D-268 and D-274). The owner answered four questions (D-301 to D-304): no choice removes a cast member, a legal use of a rite needs a license and a stamp, only the replacement shares a main aptitude, and PR-42 moves to Phase 4.
- The owner stopped the session for a context reset before the second batch of critic questions. OQ-52 to OQ-55 hold them, unasked.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds seven commits above `main`: the five of Session 2, `e82c3ad` (the cast block), and the commit that holds this entry (the critic answers and this handoff). The session pushed the branch at the end (D-147), and the remote head is the commit that holds this entry.
- The interim STE check passes on every non-exempt `.md` file, `docs/world/cast.md` included.
- No PR exists, so gitar has not run. CI and the review gate do not exist yet.

### In flight

The full-plan docs PR (D-142). Blocks done: setting, technical, graphics, UI, places, and cast. Blocks left, in order (D-262): the arc, then systems, audio, and release. After the interview, the plan still needs:

- `docs/world/arc.md`.
- The five phase roadmaps and the area roadmaps in `docs/roadmaps/` (D-144, D-145).
- A PR-# id for every new system (C-10 of the first critic pass): particles, light, the day clock, transitions, crash files, the UI screens, and the lesson and aptitude screens.
- Sections 7 and 8 of `docs/design.md` rebuilt from the roadmaps, then another design-critic pass.
- The PR, the gitar pass, the label (D-67), and the owner merge. Then the rename and the move (D-216), the Deck test (D-160), and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- No PR exists for this branch until the plan is complete (D-147). Push at each session end, and open no draft.
- The job system is gone (D-268, D-272). D-32, D-55, D-77, D-150 to D-152, and D-256 are superseded, and many more are revised in part. Read the Effect column before you cite any decision under D-267. Warden, hexer, mender, and cutpurse now name people, not jobs (D-276).
- Terms (D-278): lesson, rite, drill, kind, main aptitude, side aptitude, stamp, and lead. Write a kind with a capital letter (Mend, Blade), because the kind names are common words. Documents say "a strip of soft metal", because "lead" is the term for the lead character. "Cast" as a noun means the story characters, never a use of a spell.
- Other repositories guide documents alone (D-277). Do not read them for a system, a tool, or code.
- The aptitude arithmetic is tight. The three later side aptitudes must be Harm, Mend, and Theft, and the arc must set who dies before it assigns them, or the replacement can have no legal side aptitude (D-299).
- A session reading is not an owner decision. D-274, D-292, and D-298 each recorded one. D-303 confirmed the first, and OQ-52 and OQ-53 ask the other two.
- On the creative picks of this block, the owner chose against the recommendation most of the time: the Blade lead, the church clerk, the two-syllable names, the young party, and three syllables for a foreign name. Give options that differ in kind, with honest cons.
- The STE checker counts a bold title with its paragraph and fails a numbered list sentence over 20 words. It does not check tables. It does not catch a clash of terms, so the critic found "lead strip" and "physical skill".
- An interrupted step can land part of its edits. Check the files with a grep before you trust an earlier plan.
- The next ids are D-305, OQ-56, F-29, L-16, G-26, PR-43, M-7, and Session 4.

### Open questions that block progress

OQ-18 continues with the arc block. OQ-48 (where the death falls, and whether the lead can die), OQ-49 (scenes that set the fighters), and OQ-54 (the distance rule and FF7) belong to the arc. OQ-52 waits for the cast of later regions. OQ-35, OQ-38 to OQ-47, OQ-50, OQ-51, OQ-53, and OQ-55 wait for the systems block. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The next session reads this entry, then asks the four unasked critic questions as one batch: OQ-52, OQ-53, OQ-54, and OQ-55. Then it starts the arc block of OQ-18 in batches (D-24), with OQ-48 first. The first arc topics: who dies and where, what happened to the parent of Marrek (D-291), which power sends Elio (D-290), what order Bergit refused (D-294), and the road from the mining town to the ice crossing. It checks each option against `docs/world/banned-devices.md` first, records each answer from D-305 on, and writes `docs/world/arc.md` when the block closes.

## Session 2: 2026-09-13, Claude Code

Author: Claude Code
Session: PR #1 merged, then the world-building interview (OQ-18) grew into the full-plan interview of D-142. Branch `docs/pr-2-world-building`, with no PR yet (D-147). The session ran from 2026-09-12 into 2026-09-13.

### What this session did, and why

- Confirmed the merge of PR #1. `main` is `9dd80da`, and its tree matches the approved head `cc34247`. The last gitar pass approved `cc34247` before the label went on (D-67).
- Ran the world-building interview. On owner instruction it grew into a full roadmap before PR-1, in one docs PR (D-142, D-144 to D-147). D-123 to D-266 record the answers.
- Setting block (D-123 to D-159): a region ceded by treaty, a thing below that answers spilled blood, two churches, the license law and hidden jobs, waystones, mountain passes, and a ban six years old. `docs/world/setting.md` and `docs/world/banned-devices.md` hold it. The owner asked for distance from FFT (D-136, D-140).
- Owner redirections: every plotline converges, and no faction falls per region (D-131, F-21). Region one is a free prologue, a Steam demo of the full game (D-133, D-143). 2D effects plan from the start (D-139). The tentative name is The Thing Below, with a rename and a move to the external SSD after this PR merges (D-215 to D-217, `docs/runbooks/rename-and-move.md`).
- Technical area (D-160 to D-179): a Deck test picks the renderer, 60 frames locked, a real-time map at 60 ticks, JSON grid maps, stable ids with migrations, plain state and systems, basis points, crash files, debug intents, a CI software render with desktop contact sheets, JSON scene steps, a coverage report, an in-house PNG codec, strict C# schema types, atomic saves, and JSON log lines.
- Graphics area (D-180 to D-210): full light with generated normal maps, free light on a 64-color palette, glow, heavy short battle effects, four ambient kinds, a 48-minute day cycle, ten transitions, three sprite views, battle poses, the test-sprite art style, layered backdrops, true-size large enemies that hold an area, and an unlit UI.
- UI area and the frame (D-211 to D-241): nested windows, a minimal HUD, numbers with a message line, four accessibility settings, dark iron windows, and silent typed dialogue. The owner replaced 640 by 360 with 1280 by 800, 32-pixel tiles, a 16-pixel font, and a 32-pixel title font (D-227, D-228, D-235). The default fits the screen height, with whole-number scale as a setting (D-232). A sweep changed every document that named the old sizes (F-24). Fonts: Terminus and Terminus Bold 32 (D-263, D-264).
- Places block (D-242 to D-255): four factions, a mining town and a cave community across a gorge, the deep mine, the hanging cells, and the border fort and the ice crossing at the high pass. `docs/world/places.md` holds it.
- The design-critic agent read the plan after the frame change and found 13 defects (F-25 to F-27). The session fixed the stale text and 24 missing revision notes, and added PR-41 for the screen-test job. The owner answered the rest (D-256 to D-262, D-265, D-266).
- External facts from Steamworks and Godot pages were checked by the session against the live pages, and `docs/design.md` records them with dates.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds five commits above `main`: `745c6e2`, `8d5ad99`, `5f1e5f7`, `1ddd3ac`, and the commit that holds this entry. The session pushed the branch at the end (D-147), and the remote head is the commit that holds this entry.
- The interim STE check passes on every non-exempt `.md` file, `docs/world/` and the new runbook included.
- No PR exists, so gitar has not run. CI and the review gate do not exist yet.

### In flight

The full-plan docs PR (D-142). Blocks done: setting, technical, graphics, UI, and places. Blocks left, in order (D-146, D-262): the cast, the arc, then systems, audio, and release. After the interview, the plan still needs:

- `docs/world/cast.md` and `docs/world/arc.md`.
- The five phase roadmaps and the area roadmaps in `docs/roadmaps/` (D-144, D-145).
- A PR-# id for every new system (critic C-10): particles, light, the day clock, transitions, the job law, crash files, and the UI screens.
- Sections 7 and 8 of `docs/design.md`, then a second design-critic pass.
- The PR, the gitar pass, the label (D-67), and the owner merge. Then the rename and the move (D-216), the Deck test (D-160), and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- No PR exists for this branch until the plan is complete (D-147). Push at each session end, and open no draft.
- Decision rows carry two dates: D-123 to D-249 on 2026-09-12, and D-250 onward on 2026-09-13.
- The STE checker counts a bold PR title with its paragraph, so a PR entry of six sentences fails rule 6.6. It also flags "is mounted", "should", "stops being", and an -ing word at the start of a sentence.
- `README.md` is in the override set now (D-239). `content/` is not, so the palette growth to 64 and the redraw of the four sprites wait for PR-34 (D-185, D-233).
- The font samples and the render scripts lived in the session scratchpad and are gone. The 8-pixel candidates are void (D-230). ChillBitmap names both OFL and GPL terms for its 16-pixel build, with no "either".
- The move to the SSD changes the folder that keys the local session notes of the harness. Step 10 of the runbook copies them.
- Many Edit calls on one file in one step all landed in this session. Verify with a grep before each commit.
- The next ids are D-267, OQ-38, F-28, L-16, G-26, PR-42, M-7, and Session 3.

### Open questions that block progress

OQ-18 continues with the cast and the arc. OQ-34 (papers for a licensed job) and OQ-35 (the feeding as a battle rule) wait for the systems block. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The next session reads this entry, then asks the cast block of OQ-18 in batches (D-24). The first topics: the lead structure, why the five travel together, the first three cast members and their starting jobs (the Warden and the Mender at Gate 2, D-256), the last two, and names in the sound palettes of D-159. It checks each option against `docs/world/banned-devices.md` first, and records each answer from D-267 on.

## Session 1: 2026-09-12, Claude Code

Author: Claude Code
Session: establish the documents, the skills, the agents, the registers, and the design, through two pivots. Branch `docs/foundation`, PR #1.

### What this session did, and why

- Read both reference repositories in full: the agent files, the skills, the review workflow, the handoff, the registers, and a review pair (D-23).
- Ran the repository interview, D-1 to D-25, and the roadmap interview, D-26 to D-65. Found one conflict, D-39 against D-46, and the owner settled it as D-47 (F-6, L-13).
- Wrote `CLAUDE.md` and `AGENTS.md` as identical files (D-20), five skills, two agents, the PR template, the runbook, the `LICENSE` file (D-54), and the registers.
- The gitar pass on PR #1 left two comments, both with merit. The checker now removes a one-line HTML comment (F-11), and the PR description count reads 15 files. One commit answered both, `0b2539a`, and the reply on each thread names it. The second pass approved that head. The session had claimed gitar was absent without a check (F-12), and D-66 records the owner's instruction that every PR answers the pass.
- The owner set two process rules: the session applies the `review-override` label itself after the pass approves (D-67), and asks every open question before a docs PR (D-68). The session created the label.
- The first pivot, D-78: a terminal look in a window, not a terminal. The second pivot, D-98: a sprite-based game with no terminal look at all, and the language reopened. The engine interview chose Godot 4 with C#, an engine-free Core, and the what-you-carry tool ports (D-99 to D-118). L-14 records the lesson: ask the medium question first.
- Made the sprite feasibility test (D-94): a palette and four 16 by 16 sprites as text grids. The owner said sprites are in (D-97). The grids, the 48-color palette (D-121), and the atlas landed under `content/sprites/` with the interim atlas tool at `docs/tools/make-atlas.py` (D-119).
- Archived the terminal design as `docs/archive/design-v1-terminal-2026-09-12.md` and wrote `docs/design.md` v2: the Godot shape, findings F-1 to F-18, guardrails G-1 to G-25, five phases with PR-1 to PR-40, and the sequence. PR-32 is retired.
- Replaced the `rust-conventions` skill with `csharp-conventions`, and rewrote the code rules, the build commands, the runbook, the PR template, and the glossary for Godot and C#.
- The third gitar pass, on the pivot head, left two comments on the atlas tool, both with merit (F-19, F-20). The tool gained a pixel `--check` mode and fails on a repeated palette key. Commit `a332a02` answered both, and the reply on each thread names it. The fourth pass approved `a332a02` at 00:39 UTC on 2026-09-13 with four findings resolved over the four passes and no new issue. The session applied the `review-override` label (D-67) and ticked the pass and the override lines in the PR body.
- This entry is a metadata commit above `a332a02`. Its push voids the approval under D-67, so the session removed the label, requested a new pass, and puts the label back when the pass approves this head.

### State of the build

- No code exists. The machine has .NET 10.0.400 and Godot 4.7.2 .NET at `/Applications/Godot_mono.app`.
- `main` holds the owner's root commit `6b899dd` alone, an empty `CLAUDE.md` (D-25).
- Branch `docs/foundation` holds everything else, as PR #1 (D-26, D-79). The effective head is `a332a02`. The remote head is the commit that holds this entry, checked with the session end gate before the session ended.
- The interim STE check passes on every non-exempt `.md` file. `python3 docs/tools/make-atlas.py --check` proves that the committed atlas matches the grids by pixel.
- No CI exists. PR-1 creates it. The review gate does not exist. PR-3 creates it. Every review thread on PR #1 is resolved, and each has a reply that names its commit.

### In flight

PR #1, at the pass on the handoff commit. When it approves, the session applies the `review-override` label, and the owner merges. When it finds something, the session answers it under the `pr-review` skill and repeats.

### Traps and gotchas

- The harness reminder asks for a co-author trailer in every session. D-22 forbids it, and `.claude/settings.json` sets empty strings.
- The Python checker applies the 20-word limit to every numbered list item (F-5). Keep numbered items short, or use bullets.
- Every command in `CLAUDE.md` except the interim STE check and the atlas tool waits on PR-1. Do not run `make` before it exists.
- gitar's trial quota pauses the automatic pass, so post `Gitar review` on the PR after each push and wait for the result. A push after the label removes the approval, so wait for the next pass before the label goes back on (D-67).
- A pass can finish inside three minutes. It posts a new dashboard comment, and it can land before a poll starts. Read the newest gitar comment by its `created_at`, and never filter on a time after the request.
- A metadata commit on the handoff alone still voids the gitar approval, because the approval is on the head. Write the handoff entry before the last pass, not after it.
- The `playtest-bot` agent has no runner until PR-15. It stops and says so.
- The atlas holds the grids in file name order: cutpurse, hexer, mender, warden. The test sheet of D-94 held them in another order, and the pixels are the same.
- Thirty decisions changed in one day through D-78 and D-98. Read the `Effect` column before you cite any decision under D-99.
- The next ids are D-123, OQ-26, F-19, L-16, G-26, PR-41, M-7, and Session 2.

### Open questions that block progress

OQ-18, the world-building interview, blocks the rename (D-102) and Phase 4. OQ-3 blocks the enforced gate after PR-3. OQ-14 folds into OQ-18. Nothing blocks PR-1.

### Next concrete action

The session waits for the pass on this head and applies the label. The owner merges PR #1. The next session runs the world-building interview (OQ-18) as a docs PR under D-68, then the rename PR (D-102). Then a session starts PR-1 from `main` per the Phase 1 roadmap, and writes `docs/roadmaps/phase-1-foundations.md` first with the exit tests of PR-1 to PR-6 and PR-34 and the three font candidates (D-122), under the `design-doc-style` skill.
