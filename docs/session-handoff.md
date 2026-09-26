## Session 342: 2026-09-26, Codex

Author: Codex
Session: repeat review PR #87 after the P2-1 correction. Repository: the-thing-below. Branch: `review/pr-87`. PR: #87. Role: reviewer. Base: `f383aa9`.

### What this session did, and why

- Verified that the runner checks each state and that the tick 7 regression test records and replays the softlock (D-1179, T-3).
- Marked P2-1 fixed in `docs/reviews/pr-87.md`. Verified the current Gitar CI-analysis claim against run `36279907132`; the log names RG 4 and RG 5 from the prior review record (D-964).

### The state of the build

- The local build, 17 focused tests, format, and STE check pass. CI run `36279906549` passes substantive jobs on every leg. The remote head before this metadata commit is `f812808`.

### What is in flight

- This metadata commit and its push. The PR then waits for the owner merge.

### Traps and gotchas

- Review-gate run `36279907132` reads the prior review record. RG 4 and RG 5 wait for this update.

### The questions that block progress

None.

### The next concrete action

The owner reads the updated record and confirms the merge.

## Session 341: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-15, round 3, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-15-bots`. PR: #87. Role: author. Base: `f383aa9`.

### What this session did, and why

- Gitar approved `66dcf4d` with no finding. The author answered its CI-analysis claim on the PR: the one fault of `review-gate` was RG 3, the absent record (D-964). Two claims in all, one on each head, and neither had merit.
- The cross-provider review of `66dcf4d` gave `Changes required` for P2-1: the check each 60 played ticks could miss a softlock that clears on a later tick. The finding has full merit. `docs/reviews/pr-87-response.md` holds the answer.
- The runner now checks each state. The check skips the trial when the state accepts a toggle, which always changes the hash, and a seed loop proves that the two paths agree. The check on each tick ran faster than the old sampling.
- Moved the untracked `TheThingBelow.Game/scripts/Ui/DialogueChange.cs.uid` to `/tmp/pr15-aside/`, because the review refuses a tree with an untracked file. PR-36 added `DialogueChange.cs` with no `.uid`, and the repo tracks 91 such files. The owner decides where that file goes.

### The state of the build

- `make verify` passes on this machine. The remote head before this commit is `3e8f772`, the review record.

### What is in flight

- The Gitar pass of this push, then the repeat review of `make codex-review PR=87`.

### Traps and gotchas

- The review command refuses a tree with an untracked file. Godot writes a `.uid` for a new script when the editor opens the project.
- The owner adds `bots` to the required checks of `main` after its first run (section 7.16 of `docs/roadmaps/area-ci.md`).

### The questions that block progress

None.

### The next concrete action

Wait for the Gitar pass and the CI of this push. Then run `make codex-review PR=87` in the background.

## Session 340: 2026-09-26, Codex

Author: Codex
Session: review PR #87. Repository: the-thing-below. Branch: `review/pr-87`. PR: #87. Role: reviewer. Base: `f383aa9`.

### What this session did, and why

- Reviewed effective head `66dcf4d`. P2-1 finds that 60-tick softlock sampling can miss a softlocked state between samples (D-1179).
- Verified the Gitar CI-analysis item against the review-gate log. RG 3 waits for the review record, and the author answered the claim about unchecked boxes (D-964).
- Added `docs/reviews/pr-87.md` and corrected the PR Documents row.

### The state of the build

- `make verify` passes with 4,003 tests. CI run `36278779725` passed all substantive jobs on every leg. Review-gate run `36278780411` waits for the review record. The remote head before this metadata commit is `66dcf4d`.

### What is in flight

- The PR needs a correction for P2-1 and a repeat review.

### Traps and gotchas

- The runner checks softlocks every 60 played ticks. The exit condition of D-1179 applies to each state.
- A clean Gitar approval has no item and does not block the verdict (D-964).

### The questions that block progress

None.

### The next concrete action

Correct P2-1, add its regression test, and request a repeat review of PR #87.

## Session 339: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-15, round 2. Repository: the-thing-below. Branch: `feat/pr-15-bots`. PR: #87. Role: author. Base: `f383aa9`.

### What this session did, and why

- Read the first CI run of the `bots` job, run 36278255713. Each leg played 1,000 runs of each policy with no crash and no softlock, and the three legs gave the same ends.
- Set the counts of D-1180 from the times of the slowest leg, macos-26: 20 seconds for 1,000 greedy runs and 22 for 1,000 random runs. Each policy takes half of 5 minutes, less a margin: 7,000 greedy runs and 6,500 random runs, about 283 seconds on macos-26.
- Answered the Gitar CI-analysis claim on the PR. The one fault of `review-gate` is RG 3, the absent review record, and not the checkboxes (D-964). The Gitar code review approved `a60e1b5` with no finding.

### The state of the build

- Every check of `a60e1b5` passed except `review-gate`, which waits for `docs/reviews/pr-87.md`. The remote head before this commit is `a60e1b5`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=87`.

### Traps and gotchas

- The local `bots` target runs a Debug build, so it plays about 5 times slower than a CI leg. Time the counts on CI alone.
- The owner adds `bots` to the required checks of `main` after its first run (section 7.16 of `docs/roadmaps/area-ci.md`).

### The questions that block progress

None.

### The next concrete action

Wait for the Gitar pass and the CI of this push. Then run `make codex-review PR=87` in the background.

## Session 338: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-15. Repository: the-thing-below. Branch: `feat/pr-15-bots`. PR: #87. Role: author. Base: `f383aa9`.

### What this session did, and why

- Asked OQ-74 and OQ-80 and four more questions, and recorded D-1179 to D-1185. The softlock is a state where no accepted intent changes the state other than the tick. The count of runs fits 5 minutes of bot play on the slowest leg. The greedy policy, the goal flag, the tick budget, the battle numbers, and the start maps follow from the answers.
- Built the query of the accepted intents in Core (`AcceptedIntents`, `Simulation.Accepted()`), the bot rules file and its reader, the record store of Storage, and the `bots` command of Tools with both policies, the softlock check, the saves, the reload after a wipe, and `--replay`.
- Added the `bots` family and its gate job to CI, `make bots` to `make verify`, and `bots` to the skipped checks of D-858.
- Found F-154: Game starts in the fixture dungeon, which holds no story scene, so no run could reach a goal. D-1185 starts the runs in the dungeon and on the hub in turn.
- Measured the budget: the longest greedy run to the goal over seeds 1 to 2,000 took 783 ticks, so the budget is 2,349 (D-1184). A softlock check on each tick cost about 99% of a run, so the runner checks each 60 played ticks.

### The state of the build

- `make verify` passes on this machine. The remote head before this commit is `fe4d4b4`, and this push opens PR #87. CI of the first push measures the counts of D-1180.

### What is in flight

- The counts of `GREEDY_RUNS` and `RANDOM_RUNS` in the `bots` job hold 1,000 each until the first CI run gives the time of each leg. Then the session sets the counts of D-1180 and records the numbers in the PR.
- The Gitar pass, then `make codex-review`.

### Traps and gotchas

- `CLAUDE.md` and `AGENTS.md` sit 34 bytes under the size limit of D-583. A later line there needs a cut first.
- A collection expression such as `[null]` on a `List` in Core compiles to `CollectionsMarshal` and adds `System.Runtime.InteropServices` to the references of Core, which the G-1 test refuses.
- Many test content sets hold no map, so the load checks the goal flag alone. The runner checks the start maps (`BotRules.RequireStartsOf`).
- 12 of 1,000 greedy hub seeds wipe in the fight of the rats before any save and loop to the budget. That is a budget end under D-1179, not a failure.
- The owner adds `bots` to the required checks of `main` after its first run (section 7.16 of `docs/roadmaps/area-ci.md`).

### The questions that block progress

None.

### The next concrete action

Read the time of each leg in the first CI run of the `bots` job, set the counts, and push. Then run the Gitar pass.

## Session 337: 2026-09-26, Codex

Author: Codex
Session: repeat review PR #86 (PR-36). Repository: the-thing-below. Local branch: `review/pr-86`; PR branch: `feat/pr-36-dialogue`. PR: #86. Role: reviewer. Base: `6f02d3d`.

### What this session did, and why

- Re-reviewed effective head `6b1b7d5`. The same line now redraws for each say step, and the portrait and name redraw when the speaker changes (D-223, D-997).
- The regression test reaches two speakers of the same line id. It passes with the correction. P2-1 is fixed in `docs/reviews/pr-86.md`.
- The CI-analysis claim of Gitar names RG 4 and RG 5. The workflow log confirms that the prior review record still held the old verdict and head (D-964).
- The record keeps the earlier `Changes required` verdict and gives `Ready for owner merge` for the current effective head (T-4, D-17).

### The state of the build

- `make verify` passes with 3,938 tests, format, det-lint, STE, replay identity, content hash, atlas, and smoke. CI run `36273874604` passes all checks except `review-gate`, which waits for this review record. The remote head before the metadata commit is `6b1b7d5`.

### What is in flight

- This session commits the review record and handoff as one metadata commit, then verifies the push.
- The PR waits for the owner merge (D-930).

### Traps and gotchas

- The correction changes no capture content. The CI artifact of run `36271984962` remains the visual evidence for the earlier screen changes (D-733).
- The clean Gitar approval has no item. The actionable CI analysis names RG 4 and RG 5 (D-964).

### The questions that block progress

None. OQ-250 blocks no PR yet.

### The next concrete action

The owner reads the review record and confirms the merge.

## Session 336: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #86 (PR-36), round 2. Repository: the-thing-below. Branch: `feat/pr-36-dialogue`. PR: #86. Role: author. Base: `6f02d3d`.

### What this session did, and why

- The review of `cd1d452` gave `Changes required` for P2-1: the box drew the speaker again on a new line id alone, so a second speaker of the same line kept the first portrait (D-223, D-997).
- Full merit. `DialogueChange` now decides each redraw with no engine type: the line on each new say step, and the speaker on each change. `ScenePlay.LineStep` names the say step of the line.
- `ScenePlayTests.ASecondSpeakerOfTheSameLineDrawsItsPortraitAndItsNameAgain` failed on the old rule and passes now. `docs/reviews/pr-86-response.md` records the answer.
- Gitar approved `e9ff621` and `cd1d452` with no thread. Its CI analysis made four claims, and two PR comments answer them: the baselines, RG 3 two times, and the coverage job.

### The state of the build

- The full suite passes: 3,938 tests. `dotnet format` and det-lint report no finding. CI run `36271984962` passed every job of `cd1d452` but `review-gate`. The remote head is the push of this round.

### What is in flight

- The Gitar pass and CI of this round, then `make codex-review PR=86`.

### Traps and gotchas

- The correction draws no capture differently, so each baseline stands.
- `DialogueChange` resets when the box hides, so the next line draws in full.

### The questions that block progress

None. OQ-250 blocks no PR yet.

### The next concrete action

When CI is green but `review-gate` and Gitar completes, run `make codex-review PR=86` in the background.

## Session 335: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #86. Repository: the-thing-below. Branch: `review/pr-86`. PR: #86. Role: reviewer. Base: `6f02d3d`.

### What this session did, and why

- The review found P2-1: a repeated line id can leave the previous speaker's portrait and name on screen (D-223, D-997).
- The review records `Changes required` for effective head `cd1d452` (T-4, D-17).
- The `docs/reviews/` Documents row now names `docs/reviews/pr-86.md` (D-581).

### The state of the build

- `make verify` passes on macOS with 3,937 tests. CI run `36271984962` passes every job except `review-gate`, which awaited this review record. The remote head before the metadata commit is `cd1d452`.

### What is in flight

- The author must correct P2-1, add its regression test, and request a repeat review.

### Traps and gotchas

- The CI capture artifact is the visual source for screen review (D-733). The local renderer does not reproduce its baselines.
- The clean Gitar approval has no item. The CI analysis claim is RG 3 alone (D-964).

### The questions that block progress

None. OQ-250 concerns the pause of a story-scene fight and blocks no PR yet.

### The next concrete action

The author corrects P2-1 and requests a repeat review of PR #86.

## Session 334: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-36, round 1. Repository: the-thing-below. Branch: `feat/pr-36-dialogue`. PR: #86. Role: author. Base: `6f02d3d`.

### What this session did, and why

- The owner answered OQ-150 and OQ-151, folded the playtest fixes of PR-65 into PR-36, and set the text batch and the scope (D-1172 to D-1178). OQ-250 is new.
- Core: a choice holds two to four options, and the simulation version rises to 33 (D-1175, G-17).
- Game: `ScenePlay` follows each step from the ticks and sends one wait intent at its end. `DialogueBox` draws the line, the portrait, the name plate, and the choices. The map walks each actor, and the story pause dims the frame (D-1009, D-1013).
- A set fight takes its transition with no encounter: the boss flag, then the largest body of the group (D-788, D-937).
- Fixes: WASD and Backspace in menus, the "Quantity" label, sharp titles (F-153), the empty line of a service window, and the blur band on the lead or the camera marker (D-1173, D-1177).
- Content: two fixture story scenes on tiles of the fixture hub, three fixture portraits, and the new portraits atlas page.

### The state of the build

- `make verify` passes locally. CI run `36271514085` failed on the baselines alone. This round commits 108 changed baselines and 5 new `scene-*` baselines from its `screen-captures` artifact (D-733). The remote head is the push of this round.

### What is in flight

- Gitar approved `e9ff621` with no finding. Its CI analysis named the baselines, RG 3, and the coverage job, and a PR comment answers each claim. The baseline push needs a new Gitar pass and green CI.

### Traps and gotchas

- Core runs a whole move at once. `ScenePlay.TryWalk` walks the actor back from its end tile along the path.
- Core refuses an intent that no step waits for. `ScenePlay` checks the queue and the queued pause before each intent.
- The stranger trigger stands at (4, 7), off every walk to a service. A test walks each capture route and fails if a route crosses a trigger.

### The questions that block progress

None. OQ-250, the pause of the fight of a story scene, blocks no PR yet.

### The next concrete action

When CI is green except `review-gate` and Gitar completes, run `make codex-review PR=86` in the background.

## Session 333: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #85 (PR-65). Repository: the-thing-below. Branch: `review/pr-85`, pushed to `feat/pr-65-shop`. Role: reviewer. Base: `8e81487`.

### What this session did, and why

- The review found no defect in the shop, gold, save, replay, or screen changes. The record gives `Ready for owner merge` for effective head `4edc5e5` (T-4, D-17).
- The latest Gitar CI analysis named ten missing shop baselines. The head contains all ten, and the current screen-test passes. The record answers this claim and the earlier RG 3 claim (D-964).
- The review inspected all 130 changed paths and the CI screen artifact. No visual fault appeared (D-784).

### The state of the build

- `make verify` passes with 3,905 tests. CI run `36266126601` passes all build, test, format, replay, screen, smoke, and STE checks at effective head `4edc5e5d3ee4f1fbe93bed28fb19e0b5ae98d42a`. The metadata commit is the remote head after push.

### What is in flight

- The PR waits for the owner merge (D-930).

### Traps and gotchas

- `review-gate` failed RG 3 before this record existed. Check the fresh result after the metadata push.
- Gitar's missing-baseline claim was true before the latest head. The ten CI baselines and the screen-test now pass.

### The questions that block progress

None. OQ-121 is resolved by D-1149 to D-1155.

### The next concrete action

The owner reads the review record and confirms the merge.
