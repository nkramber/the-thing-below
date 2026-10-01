## Session 410: 2026-09-30, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round of the fight start and the first turn. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- The owner reported that a fight of the overworld starts while the lead walks, and that the lead walks on after the fight. `MapState.Advance` starts the next step of a held direction on the tick of the arrival, before `WorldRules` draws the fight of a zone or fires a trap. `MapState.StopStartedStep` now stops that step when a zone fight or a trap fight starts on the arrival (D-1374). `EncounterRulesTests.AFightOfAZoneStartsWithTheLeadOnItsTileAndNoNextStep` fails on the old rule.
- The owner reported that enemies always act first in a fight of the overworld. With no side behind, speed alone set the first turn: Marrek has 98, the lean wolf 115, and the snow crow 125. The owner chose a seeded random start (D-1375). `Battle.Start` takes the battle stream, and each combatant of a neutral fight starts at a random tick from 1 to its push. A sneak and an ambush keep D-770.
- Tests: `WithNoSideBehindEachSideOpensSomeFights` replaces the test of the old rule, and the defend test takes the first seed whose opening puts the grunt inside the defend. The identity story run covers the hero on each turn of the friend that the rules allow, because the turn order changed. The replay identity file changed (G-17).
- The guided runs reach the join of Dagvar on 37 of 40 seeds, three over the floor.

### The state of the build

- `make verify` passed on this machine, the smoke session included. `e309312` had a clean Gitar approval. This entry sits in the commit of the round, above `e309312`.

### What is in flight

- The CI legs of the push. The screen test of `d0156d4` changed 28 battle captures, because each fight now reaches its captured moment on another tick, and an enemy can open it. The frames `battle-menu-1x`, `battle-blow-1x`, and `battle-level-up-1x` were read, and each one shows its subject. The 28 baselines come from the artifact of run 36801576846 (D-733).
- The owner playtest of exit test 1 on Windows, then the review of the other provider (D-943).

### Traps and gotchas

- A test that reads the first turn of a neutral fight must loop over seeds or use a sneak or an ambush. `ReadyAt` has an internal setter, so a test cannot set it (D-1375).
- A story scene that starts on the arrival can meet the same held step. No report names it, so this round leaves it.

### The questions that block progress

- None for PR-17. OQ-216 and OQ-258 to OQ-260 block PR-90.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Read the `screen-test` job, and commit any changed baseline from its artifact. Then wait for the owner playtest from Ostby to the join of Dagvar.

## Session 409: 2026-09-30, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round of five playtest items and the replan of PR-90. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- The owner reported five items of the playtest. Each one has its decision:
  - The portrait of Orrin did not match his map sprite. Orrin takes the map sprite of the stranger, and Bergit takes a stand-in portrait, which is her map sprite at twice its size (D-1371).
  - A return from the overworld put the party in the house of Marrek. Each entrance now names the marker of its place in `arrive`, and the content set requires it. The village, the pasture, the town, and the two fixture places have one (D-1367).
  - The exit tile of the village showed a stair. The three road exits of the first playable draw `drawing.road_exit`, which is empty (D-1368).
  - The party arrived west of the village. `marker.overworld_village` sits on the road east of the entrance, at (61, 99) (D-1370).
  - The village was a plain rectangle. The new Ostby is 36 by 21, with a ragged thicket border, a stream and one bridge, a winding road, and four houses. The owner approved the draft preview (D-1369).
- The guided runs fell from 34 to 30 of 40. The old head gave exactly 34. Three teas in the first chest changed nothing. With no crows, the runs passed. The crow area of session 405 sat on the route, and the strip at (18, 11), 5 by 2, gives 34 again (F-158).
- The owner asked for a playthrough suite with three bot levels (D-1372). It overlaps PR-90, so the owner moved PR-90 whole to right after PR-17, with the suite (D-1373). The entry of PR-90 is now section 7.75 of `phase-2-first-playable.md`. OQ-258 to OQ-260 hold its open details.
- Tests: `AnEntranceArrivesBesideTheExitOfItsPlace` and `EachSpeakerShowsAPortraitOfTheLookOfItsMapSprite` fail on the old code. Three tests take the new rule: two error texts, and the arrival of the town.

### The state of the build

- `make verify` passed on this machine before the last text edits, and it runs again before the push. `f8ea16c` had a clean Gitar approval. This entry sits in the commit of the round, above `f8ea16c`.

### What is in flight

- The owner playtest of exit test 1 on Windows, then the review of the other provider (D-943).

### Traps and gotchas

- The guided runs sit exactly on the floor of 34 of 40. A small change of a route or a patrol can fail the test. PR-90 replaces the test with the playthrough suite.
- A map file of a place needs a marker for each entrance that leads to it (D-1367). A new place of a later PR adds one.
- The overworld command moves only the things that its settings place. The arrival markers stay where `overworld.json` puts them.
- A `git checkout` of a map file drops the markers of this round. Edit the file, never restore it.

### The questions that block progress

- None for PR-17. OQ-216 and OQ-258 to OQ-260 block PR-90.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Then wait for the owner playtest from Ostby to the join of Dagvar.

## Session 408: 2026-09-30, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round of two end marks during the owner playtest. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- Ran `58d2148` on the Steam Deck over SSH. The Deck logged its pad as known and not ignored, so D-1365 does not block the Deck. Session 407 records the steps.
- The owner read "Why." in the opening and said: "This needs a question mark, not a period." The line `line.village_marrek_why` now reads "Why?". The owner also chose "Who are you?" for `line.pasture_marrek_who`, the one other question of Marrek with a period (D-1366). No test and no capture reads either line.

### The state of the build

- `58d2148` had a clean Gitar approval, and a PR comment answered its CI analysis of RG 4 and RG 5. This entry sits in the commit of the round, above `df623c7`.

### What is in flight

- The owner playtest of exit test 1 on Windows, then the review of the other provider (D-943).

### Traps and gotchas

- A question in player text takes a question mark. The voice of Marrek asks short questions, and the period made them read as a fault (D-1346, D-1366).

### The questions that block progress

- None. The owner playtest of exit test 1 blocks the review verdict.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Then wait for the owner playtest from Ostby to the join of Dagvar.

## Session 407: 2026-09-29, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round of the racing wheel during the owner playtest. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- The owner reported that the lead walked south again, about 30 seconds after the first house, with the Xbox pad unplugged. After that, the S key did not move the menu cursor. The log named the device: `Logitech G29 Driving Force Racing Wheel`, pad 0, `"known":"no"`. A raw axis of the wheel rested, then went past the dead zone. It held `step_south` and `ui_down`, so the press gate stopped each S press as a second press (F-107).
- The owner chose to ignore each button and each axis of a device with no controller mapping, and to remove the rest rule of session 405 (D-1365). `PressGate` holds the ignored pads: `IgnorePad`, `Ignores`, and `ForgetPad`, which stops ignoring a pad that disconnects. `Boot.OnPadConnectionChanged` ignores a pad that connects with no mapping, and its log line gains the field `ignored`.
- `Boot.cs`, `PressGate.cs`, and `PressGateTests.cs` came back from `a1ed5f2` first, so no code of the rest rule stays.
- Tests: three new `PressGateTests`, and the pads check of the smoke session. The smoke check fails on the old gate.

### The state of the build

- On this machine, the `PressGateTests`, the smoke session, and the format check pass. `2e9eecf` had a clean Gitar approval.
- This entry sits in the commit of the round, above `2e9eecf`.

### What is in flight

- The owner playtest of exit test 1 on Windows, with the wheel plugged in, then the review of the other provider (D-943).

### Traps and gotchas

- The Steam Deck shows its pad as known. A run of `58d2148` on the Deck in Desktop Mode, outside Steam, logged `"name":"Steam Deck Controller","known":"yes","ignored":"no"`. Game Mode gives the virtual pad of Steam Input, which SDL also maps, and no run checked it yet.
- A run on the Deck over SSH needs `DISPLAY=:0`, the `XAUTHORITY` of the desktop session (read it from the environment of `plasmashell`), `DOTNET_ROOT=$HOME/.dotnet`, and `$HOME/.dotnet` on the PATH. Without the last two, Godot cannot load .NET and crashes.
- A real pad with no SDL mapping now does nothing. A later setting or a mapping file can cover it (D-1365).

### The questions that block progress

- None. The owner playtest of exit test 1 blocks the review verdict.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Then wait for the owner playtest from Ostby to the join of Dagvar.

## Session 406: 2026-09-29, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round of the gear help line during the owner playtest. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- The owner asked to move the playtest after the next two PRs, then kept it in PR-17: "Keep it right here. Wait for me to do it." Exit test 1 stays, and no decision changes.
- The owner asked what "Left and right show the next one." means. The line is `menu.gear_help` of the gear window. Left and Right show the next character, and with Marrek alone they seem to do nothing. The owner chose the new text "Left and right change the character." in this PR (D-1364). No capture shows the line, so no baseline changes.
- The owner asked why Escape ends the game. D-813 ends a development session on Escape when no menu and no console are open, so it is not a fault.
- Gave the owner a PowerShell profile function outside the repository: `the-thing-below [branch]` fetches, switches to the branch (default `main`), pulls with fast-forward alone, builds, and starts the console exe of Godot. It stops on an uncommitted change.

### The state of the build

- `b572b62` had a clean Gitar approval. On this machine, the tests pass (4730), and the content hash matches after the string change.
- This entry sits in the commit of the round, above `b572b62`.

### What is in flight

- The owner playtest of exit test 1 on Windows, then the review of the other provider (D-943).

### Traps and gotchas

- The owner starts the game with the profile function, and the Godot exe is `C:\Godot\Godot_v4.7.2-stable_mono_win64_console.exe`. The function switches the shared checkout of that machine to the branch that it names.
- In a development build, Escape on the map ends the session (D-813). The owner uses Backspace or the B button to cancel.

### The questions that block progress

- None. The owner playtest of exit test 1 blocks the review verdict.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Then wait for the owner playtest and the log lines of the pad (F-156).

## Session 405: 2026-09-28, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round that fixes the two faults of the owner playtest on Windows. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- Asked the owner the two questions of session 404. The forest rule took three answers. The owner first chose "forest blocks everywhere". A trial of that rule failed 346 of 4719 tests, because D-1262 refuses a zone on a blocked tile and the overworld holds two forest zones (D-1283). The owner then chose "forest blocks on a hub and a dungeon", and then a new tile kind.
- Added the thicket (D-1362, D-1363, F-157). The tile kind `Thicket` writes `T`, blocks the lead, and stops sight. The drawing `drawing.tile_thicket` is a denser and darker forest. Ostby and Ostby Pasture write each tree tile as a thicket. The crows of the pasture move their area from (18, 11) to (22, 11), because their old area held the middle clump, and D-209 needs a walkable area.
- Added the stick at rest (F-156). The owner had an Xbox pad plugged in and off, with no stick movement. `PressGate` passes a push of a stick axis only after that axis came to rest inside the dead zone one time. `ForgetPad` forgets the rest, so a pad that connects again rests again. `Boot.LogRefusedSticks` writes a warning that names each refused axis one time.
- Tests: `TileKindsTests`, `FirstPlayableContentTests.EachPlaceWritesThicketForItsTreesAndNoForest`, seven `PressGateTests`, and the pads check of the smoke session. The content hash and the atlas changed. The simulation version stays 40, because this PR raised it, and the PR-17 note of `SimulationVersion` names the thicket (G-17).
- Read the map previews of the village and the pasture. No capture of the screen test shows a hub of PR-17, so no baseline changes.

### The state of the build

- `make verify` passed on this machine: 4730 tests, format, det-lint, STE, identity, bots, and the content hash. Then the new drawing changed, and the atlas check and the smoke session passed again after `atlas --root .`.
- This entry sits in the commit of the round, above `a1ed5f2`.

### What is in flight

- The Gitar pass of the round, then the owner playtest of exit test 1 from Ostby to the join of Dagvar. Core, content, and Game changed after the review of session 403, so the PR needs a new review of the other provider (D-943).

### Traps and gotchas

- The stick rule drops a fast first push of an axis that sent no motion inside the dead zone before it. The next push walks. F-156 records it.
- The cause of F-156 is not confirmed. On the next play, the owner can read the lines `a pad connected` and `a stick axis pushed before it came to rest` in `%APPDATA%\the-thing-below\logs`. A drift that starts after a rest passes the new rule.
- The forest stays walkable on the overworld (D-1256). A new hub or dungeon writes `T` for its trees, and `FirstPlayableContentTests` reads the five places of PR-17 alone.

### The questions that block progress

- None. The owner playtest of exit test 1 blocks the review verdict.

### The next concrete action

Run the Gitar poll of this push, and answer each item. Then ask the owner to play from Ostby to the join of Dagvar on Windows, with the pad plugged in, and to send the log lines of the pad.

## Session 404: 2026-09-28, Claude Code

Author: Claude Code
Session: author of PR #100 (PR-17), the round after the review of session 403. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- Read the review of session 403: no finding, and the verdict `Blocked` because the owner has not played the route (exit test 1). Removed the blank line at the end of this file that the review named.
- Gave the owner the steps to play on Windows: the .NET SDK 10.0.400, Godot 4.7.2 mono win64, the `--build-solutions` build, then `--path TheThingBelow.Game`. In PowerShell, a quoted path needs the call operator `&`.

### The state of the build

- The remote head carries the review record of session 403 at `e2a94da`, and this entry sits above it. Every CI check of `9bca655` passed except RG 3 before the record.

### What is in flight

- The owner playtest on Windows. The owner reported two faults in the first minutes:
  1. The lead moves toward the bottom of the screen with no input. The cause is not known. A gamepad or another stick device with drift on the left Y axis is the first suspect: `GameInputMap` binds `StepSouth` to `JoyAxis.LeftY`, and the dead zone is 0.5 (D-861).
  2. Forest tiles of an inner map let the lead pass. The owner said: "woods in an 'inner map' shouldn't be passable, especially since we're using them as map borders." Ostby and Ostby Pasture use forest as a border.

### Traps and gotchas

- The PR is not merged, so the next session continues this PR as its author. It is not the next PR.
- The owner plays on Windows. The checkout there is `C:\Users\natek\documents\vscode\repos\the-thing-below`.
- A fix to Core, content, or Game after the review needs a new review. The skip set alone keeps an approval (D-943).

### The questions that block progress

- The form of the forest rule: forest blocks the lead on a hub and a dungeon, a new tile kind for trees that block, or a content change to another border tile. Ask the owner, and ask whether forest on an inner map also stops sight.
- The cause of the pull toward the bottom of the screen. Ask the owner whether a controller or another stick device is plugged in, and whether the pull stops without it.

### The next concrete action

Ask the owner the two questions above, fix both faults with tests, push, run the Gitar poll, then ask the owner to play the route to the join of Dagvar again.

## Session 403: 2026-09-28, Codex

Author: Codex
Session: review PR #100 (PR-17). Repository: the-thing-below. Branch: `review/pr-100`, tracking `origin/feat/pr-17-first-playable-content`. Role: reviewer. Base: `9a567d6f44dc386187a2798a9c01c3e8de46e8fb`.

### What this session did, and why

- Reviewed effective head `9bca655690c879e4968392886d5cd1647a5b2baa`, all 238 changed paths, the PR description, the roadmap exit tests, the decisions, and the existing PR comments.
- Verified the Gitar CI claim against the raw log. RG 3 alone failed because the review record did not yet exist.
- Found no code defect. The review is blocked because the required owner playtest of the full route has no evidence.

### The state of the build

- Local `make verify` passed with 4,719 tests. The overworld and edge checks passed.
- Every GitHub check passed except the pre-record review-gate RG 3 fault. Gitar passed on the effective head.
- The remote PR head before this metadata commit is `9bca655690c879e4968392886d5cd1647a5b2baa`.

### What is in flight

- The review record and this entry form one metadata commit (D-610).

### Traps and gotchas

- The author answered the Gitar CI claim. The log confirms that RG 3 alone faulted before this review record existed.
- The owner playtest is exit test 1 of PR-17. The guided-run test does not establish an owner playtest.
- Push with `git push origin HEAD:feat/pr-17-first-playable-content`.

### The questions that block progress

The owner playtest evidence for exit test 1 is unresolved.

### The next concrete action

Commit the review record and this entry, push the metadata commit, then verify the remote PR head.
## Session 402: 2026-09-28, Claude Code

Author: Claude Code
Session: author of PR-17. Repository: the-thing-below. Branch: `feat/pr-17-first-playable-content`. Role: author. Base: `9a567d6`.

### What this session did, and why

- Asked the owner every question of PR-17. D-1328 to D-1361 hold the answers. PR-17 split: the places as rule content and text with stand-in drawings, then PR-112 to PR-115 for the art and the effects (D-1328, D-1329).
- OQ-249 now blocks PR-23, because no character of the first playable waits in reserve (D-1330). OQ-245 is resolved as the cover (D-1352). New questions: OQ-255 and OQ-256.
- Built six Core changes by owner choice: the pay step (D-1335), a gate on every map (D-1347), the time of a map set by a flag with save format 21 and record format 6 (D-1349), the kit of a newcomer (D-1350), the cover (D-1352), and the cap of 9,999 (D-1357). Also the battle line of two lines that scrolls in a box of one line (D-1356, D-1359), the capital of a common name (D-1358), and the wait fix of D-1360.
- Wrote the content: Ostby, Ostby Pasture, Gruvhald, and the Hanging Cells on two floors, the enemies, the cast kits, the lessons, the gear, the items, the shop, the inn, the scenes, and the night after the end. The owner approved three text batches (D-1354, D-1355, D-1361).

### The state of the build

- An agent of this session read each of the 156 frames of `make sheet`. The one new fault, the long line of the ui fixture, is fixed.
- Local head `eb280ee` and later commits. Local checks pass: 4719 tests, format, det-lint, STE, content hash, atlas, identity, overworld, edges, and `make smoke`.
- The remote PR branch holds nothing yet. `origin/main` is `9a567d6`.

### What is in flight

- The first push, the PR, the map previews, and the Gitar pass. The screen baselines change with the battle line and the new start, and the CI artifact gives the new baselines.

### Traps and gotchas

- The first run starts in `map.village` now. `GameRun.StartFixture` keeps the fixture dungeon and the fixture kit for the smoke session and the screen fixtures.
- An exit to a map that is not the overworld arrives on the spawn point of that map. The town spawn sits by the chapel stair.
- The whole-run test needs 34 clean runs of 40, and it gets 34. A weaker party can tip it.
- The generator of the overworld rewrites the terrain, the zone grid, and the places of its things alone. The pasture entrance and the arrival markers are hand-written.

### The questions that block progress

None. OQ-255, OQ-256, and OQ-257 block no step of PR-17. OQ-257 holds the fault of the shop windows at 1x, which came before PR-17.

### The next concrete action

Push, open the PR with the Documents section, the three text batches, and the map previews, then run the Gitar poll.

## Session 401: 2026-09-28, Codex

Author: Codex
Session: review PR #99 (PR-53). Repository: the-thing-below. Branch: `review/pr-99`, tracking `origin/feat/pr-53-tile-edges`. Role: reviewer. Base: `f0db11584f575a90ac628f0d9aaa99115ac969c9`.

### What this session did, and why

- Reviewed effective head `eee30909a50681222d09553b82db4a1361bb827b`, the complete 62-path diff, the PR comments, the decisions, and the PR-53 exit tests.
- Verified both Gitar items. The repeated-piece finding is fixed and confirmed by Gitar. RG 3 alone failed because the review record was absent (D-964).
- Found no defect. All 4,583 tests passed in `make verify`. The screen-test artifact shows the overworld shore at each changed size.

### The state of the build

- CI on `eee30909a50681222d09553b82db4a1361bb827b` passed every check except review-gate RG 3. Required checks passed on macOS, Ubuntu, and Windows. Local `make verify` passed.
- The remote PR head before this metadata commit is `eee30909a50681222d09553b82db4a1361bb827b`.

### What is in flight

- The review record and this entry form one metadata commit (D-610).

### Traps and gotchas

- RG 3 failed before this review record existed. The other review-gate rules passed.
- Push with `git push origin HEAD:feat/pr-53-tile-edges`.
- Session 391 moves to `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Push the metadata commit, fetch, check the branch status and PR head, then confirm that review-gate passes.
