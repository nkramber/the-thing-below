# Runbook: the development machine

Status: procedure, written 2026-09-12 for the owner's Mac, and revised the same day for D-99. Revised on 2026-09-14 for the new repository name (D-410), and on 2026-09-20 for the audit (D-696). Revised again on 2026-09-14 for the move to the external SSD (D-400), for the release block (D-449, D-456, D-458, D-465), and for the review of art batches (D-514). Revised on 2026-09-23 for the cost of an enemy turn (D-961). Written in ASD-STE100.

Facts checked on 2026-09-12:

- The machine is arm64 on macOS 26.5.
- The .NET 10 SDK is present: `dotnet --version` gives 10.0.400.
- Godot 4.7.2 .NET is present at `/Applications/Godot_mono.app`, because what-you-carry uses it. The name `Godot` is not on the command path.
- `gh` has a login as the owner, and `git` has the `origin` remote for `nkramber/the-thing-below`, checked 2026-09-14 (D-410).
- The checkout is at `/Volumes/SSD-1TB/the-thing-below`, on the external SSD, checked 2026-09-14 (D-400).
- Python 3.9.6 is present. No tool of this project needs it now, because PR-34 retired the interim atlas script (D-99, D-406).
- `gh` 2.100.0 is present, checked 2026-09-14. The `--attach` flag came in `gh` 2.99.0, and the review sheets of an art batch need it (D-514).
- The repository on GitHub is public until Phase 6 (D-4, D-456). Its license is GPL-3.0, because D-695 superseded D-54.

## Install the tools

1. Install the .NET SDK that `global.json` names. The current machine already has .NET 10.
2. Install Godot 4.7.2 .NET from https://godotengine.org/download/macos/ when the machine lacks it. Put it at `/Applications/Godot_mono.app`.
3. Run `/Applications/Godot_mono.app/Contents/MacOS/Godot --version` and check the version against the design header.
4. For an export by hand, install the .NET export templates from the Godot editor with "Manage Export Templates".
5. Keep `gh` at 2.99.0 or later, because the review sheets of D-514 need its `--attach` flag.

## Prepare a checkout

1. Clone the repository: `git clone git@github.com:nkramber/the-thing-below.git`.
2. Run `make hooks` once in a fresh checkout. The pre-commit hook then refuses a commit on `main` (D-8).
3. Run `make verify` before every PR. It runs each check of the Makefile, from the build to the smoke session.
4. The STE check reads the `.md` files that git tracks, and no other file (D-702). An untracked scratch note fails no check and no commit.
5. A `.DS_Store` file under `content/` fails the content tests, because no record reads it. Remove the file.

## The debug console

1. Run `make run` for a play session on this machine (D-3).
2. Press the backquote key to open the debug console, and press it again to close it (D-725).
3. Type `help` for the list of the commands. The console takes every key while it is open, so the game reads none of them.
4. A command that changes the run enters the run record with its debug mark, so the run still replays (D-171).
5. A release export holds no console, because it references no debug assembly (D-260, D-492).

## A battle before the battle screen

The battle screen lands in PR-10. Until then, the console takes the turn of a character, and the log shows each event (D-767).

1. Walk into an enemy, or let an enemy see the party. The map stops, and a battle starts (D-531).
2. Open the console, and type `battle`. The answer gives each combatant with its slot, and the next six turns (D-756).
3. Type `attack 0` to attack the enemy of slot 0. Melee reaches the front row while anyone stands in it (D-377).
4. Type `defend`, `step`, `flee`, or `item 0` for the other actions of the turn (D-755, D-767).
5. A command on the turn of no character changes nothing, and the log file names the reason (T-2).
6. After a win or a flee, the map runs again when the log shows every event (D-522).
7. After a wipe, the run starts again from its start, because PR-16 adds the reload of a wipe (D-231, D-776).

## A hub before the region map

The region map lands in PR-35. Until then, the console moves the party to a hub (D-1133).

1. Open the console, and type `goto map.fixture_hub`. The party stands on the spawn point of the hub, and the autosave writes (D-224, D-1132).
2. Face an NPC or the waystone, and press confirm. The window of its service opens (D-1131).
3. Pick Rest to restore the party, or Save to write the slot save (D-1132).
4. Type `goto map.fixture_dungeon` to return to the dungeon. The dungeon forgets its walked tiles and its dead enemies until PR-35.

## The Steam Deck

1. Put the Deck in desktop mode and enable SSH, or copy the Linux export by USB.
2. Download the Linux x86_64 build artifact of the last merge from CI, which the export job of PR-54 writes (D-449).
3. Copy the build to the Deck. Run it from a shell until the Steam build exists (D-85, D-92, D-458).
4. Find the save folder on the Deck at `~/.local/share/the-thing-below` (D-465).
5. Record the readability and the frame time under M-6 in `docs/design.md` (D-161).
6. The Deck test of D-160 ran on 2026-09-17, on an OLED Deck at 89.9 Hz (D-616, D-617).
7. To run it again, fetch the branch spike/deck-test, and read `deck-test/readme.md` on it (D-597).
8. Copy `DeckTest.x86_64` and `run-deck-test.sh` to one folder on the Deck, and run the script in desktop mode.
9. The script runs both renderers and puts each report in a `reports` folder beside it (D-598).
10. A report with the warning of a capped frame rate gives no budget, and the run needs a repeat (T-2).
11. Run the screen scale probe before PR-7 and PR-34, on the four screens of M-8 (D-621, D-638).

### The cost of an enemy turn

PR-11 added the `evaluator-cost` command of Tools. It times one enemy turn of the worst fight: six enemies against three characters (D-961, F-53).

1. Open a shell on the Deck: `ssh deck@10.0.0.46` from the Mac, with the key of the Mac. The Deck takes no password.
2. In `~/the-thing-below`, fetch and check out the branch of the PR.
3. The Deck has no `make`. Run `dotnet run -c Release --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- evaluator-cost --root .` with `~/.dotnet` on the path.
4. Read the line of the 95th percentile. The limit is 1000 us, and the command fails a miss (D-961).
5. Give the owner the four lines of the report. The PR description records them.
6. A miss changes the depth or the profiles in the same PR (G-14).

## Where a test runs

1. The Mac of the owner is the venue of each visual test (D-623).
2. A test runs on the Deck only when the answer needs the Deck.
3. Three answers need the Deck: the scale on the screen, the readability at 1x, and the frame time (D-621, M-6, M-7).
4. The owner reads each new effect on the Mac, as its PR lands (D-622).

## The screenplay of a story batch

Run these steps in each PR that changes a story scene file, or a line that a story scene speaks (D-57, G-25). The `make screenplay` target writes the Screenplay section of the PR description (D-1015, D-1016, D-1017).

1. Write the PR description to a file: `gh pr view <n> --json body -q .body > artifacts/pr-body.md`.
2. Run `make screenplay BODY=artifacts/pr-body.md`. The target reads the merge base with origin/main.
3. Set `BASE=<commit>` when the base of the PR is not the merge base with origin/main.
4. Read the Screenplay section of the file before you send it.
5. Send the file: `gh pr edit <n> --body-file artifacts/pr-body.md`.
6. Run the steps again after each change of the batch. A second run replaces the section.

The command fails on a body above 65,536 characters, the limit of GitHub. Split the batch over more PRs then.

## Owner actions on GitHub

1. Before PR-1, enable "Require actions to be pinned to a full-length commit SHA" in the Actions settings of the repository (D-511). The owner did this on 2026-09-14.
2. Require the eight checks of D-685 on `main` (OQ-3, D-4). GitHub lists a check as a choice only after it ran once.
3. Turn on the setting that applies the protection to the administrators too, so no direct commit reaches `main` (D-25). The owner did this on 2026-09-20 (D-706).
4. Require each later check on `main` after its first run, such as the bot runs and `night-gate` (D-505, G-22).
5. When GitHub disables the night schedule after 60 days with no activity, enable the workflow again (F-41).
6. Then have a session run a night by hand on `main` (D-509).
7. Turn off "Allow merge commits" and "Allow rebase merging", and keep "Allow squash merging" (D-8). The owner did this on 2026-09-14.
8. Before any paid content lands, have a session check that every tool works on a private repository (D-456).
9. Upgrade the account to GitHub Pro, so the required checks stay on `main` (D-456).
10. Make the repository private.

## Session start

1. Run `git fetch origin` and `git status --short --branch`.
2. Read `docs/session-handoff.md`, then `CLAUDE.md`.
3. Start a branch from `main` for the PR of this session (D-18).
