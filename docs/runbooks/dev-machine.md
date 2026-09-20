# Runbook: the development machine

Status: procedure, written 2026-09-12 for the owner's Mac, and revised the same day for D-99. Revised on 2026-09-14 for the new repository name (D-410), and on 2026-09-20 for the audit (D-696). Revised again on 2026-09-14 for the move to the external SSD (D-400), for the release block (D-449, D-456, D-458, D-465), and for the review of art batches (D-514). Written in ASD-STE100.

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

## Where a test runs

1. The Mac of the owner is the venue of each visual test (D-623).
2. A test runs on the Deck only when the answer needs the Deck.
3. Three answers need the Deck: the scale on the screen, the readability at 1x, and the frame time (D-621, M-6, M-7).
4. The owner reads each new effect on the Mac, as its PR lands (D-622).

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
