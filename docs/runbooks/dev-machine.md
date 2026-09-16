# Runbook: the development machine

Status: procedure, written 2026-09-12 for the owner's Mac, revised the same day for D-99, and on 2026-09-14 for the new repository name (D-410). Revised again on 2026-09-14 for the move to the external SSD (D-400), for the release block (D-449, D-456, D-458, D-465), and for the review of art batches (D-514). Written in ASD-STE100.

Facts checked on 2026-09-12:

- The machine is arm64 on macOS 26.5.
- The .NET 10 SDK is present: `dotnet --version` gives 10.0.400.
- Godot 4.7.2 .NET is present at `/Applications/Godot_mono.app`, because what-you-carry uses it. The name `Godot` is not on the command path.
- `gh` has a login as the owner, and `git` has the `origin` remote for `nkramber/the-thing-below`, checked 2026-09-14 (D-410).
- The checkout is at `/Volumes/SSD-1TB/the-thing-below`, on the external SSD, checked 2026-09-14 (D-400).
- Python 3.9.6 is present. The interim STE checker needs it until PR-2 (D-10, D-101).
- `gh` 2.100.0 is present, checked 2026-09-14. The `--attach` flag came in `gh` 2.99.0, and the review sheets of an art batch need it (D-514).
- The repository on GitHub is public until Phase 6 (D-4, D-54, D-456).

## Install the tools

1. Install the .NET SDK that `global.json` names, once PR-1 creates it. The current machine already has .NET 10.
2. Install Godot 4.7.2 .NET from https://godotengine.org/download/macos/ when the machine lacks it. Put it at `/Applications/Godot_mono.app`.
3. Run `/Applications/Godot_mono.app/Contents/MacOS/Godot --version` and check the version against the design header.
4. For an export by hand, install the .NET export templates from the Godot editor with "Manage Export Templates".
5. Keep `gh` at 2.99.0 or later, because the review sheets of D-514 need its `--attach` flag.

## Prepare a checkout

1. Clone the repository: `git clone git@github.com:nkramber/the-thing-below.git`.
2. After PR-1 merges, run `make hooks` once. The pre-commit hook then refuses a commit on `main` (D-8).
3. Run `make verify` before every PR. Until PR-1, run the interim STE check from `CLAUDE.md` by hand.

## The Steam Deck

1. Put the Deck in desktop mode and enable SSH, or copy the Linux export by USB.
2. Before PR-7, export the Game project for Linux x86_64 by hand. From PR-7 on, download the Linux x86_64 build artifact of the last merge from CI (D-449).
3. Copy the build to the Deck. Run it from a shell until the Steam build exists (D-85, D-92, D-458).
4. Find the save folder on the Deck at `~/.local/share/the-thing-below` (D-465).
5. Record the readability and the frame time under M-6 in `docs/design.md` (D-161).
6. Before PR-1, run the Deck test of D-160, and record the model, the refresh rate, and the effect budget (D-523). OQ-92 and OQ-93 hold where the test scene lives and how to read the frame time.

## Owner actions on GitHub

1. Before PR-1, enable "Require actions to be pinned to a full-length commit SHA" in the Actions settings of the repository (D-511). The owner did this on 2026-09-14.
2. After PR-3 merges, require the `ci`, `smoke`, `ste-check`, and `review-gate` checks on `main` (OQ-3, D-4). GitHub lists a check as a choice only after it ran once.
3. Require each later check on `main` after its first run, such as the bot runs and `night-gate` (D-505, G-22).
4. When GitHub disables the night schedule after 60 days with no activity, enable the workflow again (F-41).
5. Then have a session run a night by hand on `main` (D-509).
6. Turn off "Allow merge commits" and "Allow rebase merging", and keep "Allow squash merging" (D-8). The owner did this on 2026-09-14.
7. Before any paid content lands, have a session check that every tool works on a private repository (D-456).
8. Upgrade the account to GitHub Pro, so the required checks stay on `main` (D-456).
9. Make the repository private.

## Session start

1. Run `git fetch origin` and `git status --short --branch`.
2. Read `docs/session-handoff.md`, then `CLAUDE.md`.
3. Start a branch from `main` for the PR of this session (D-18).
