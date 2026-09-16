# Rename and move the repository

Status: runbook, complete. Owner: Nate. Written 2026-09-12 (D-215 to D-217). Steps 1 to 3 ran on 2026-09-14 (D-410), and steps 4 to 8 ran the same day in the rename PR #5. Steps 9 to 12 ran after the merge. Written in ASD-STE100 (D-10).

This runbook renames the repository from the working title to the tentative name, then moves the checkout to the external SSD. The owner and a session do it once, after PR #2 merges, before the rest of the plan and before PR-1 (D-216, D-400). A docs audit PR merges between step 3 and step 4 (D-411).

## Names

| Item | Before | After |
|---|---|---|
| GitHub repository | `nkramber/terminal-rpg` | `nkramber/the-thing-below` |
| Local checkout | `~/Repos/terminal-rpg` | `/Volumes/SSD-1TB/the-thing-below` |
| Title in documents | terminal-rpg | The Thing Below |
| Solution and projects in commands | `TerminalRpg.*` | `TheThingBelow.*` |

## Before you start

- PR #2 is merged, and no other PR is open (D-400).
- `git fetch` and `git status --short --branch` show no `[ahead N]` on `main`.
- The Mac shows the external SSD at `/Volumes/SSD-1TB`.
- The name is tentative (D-215). Search the Steam store and the trademark registers for "The Thing Below" first. If a conflict shows, file it in `docs/questions.md` and stop. The search of 2026-09-14 found no conflict that stops the name, and the owner approved the rename (D-408). The external facts in the status header of `docs/design.md` give each query, its source, and its result. EUIPO, TMview, and WIPO did not answer a query from a script, so PR-75 checks them before the store page goes public (D-550).
- The docs audit PR merges before step 4 starts (D-411).

## Procedure

1. Rename the GitHub repository: `gh repo rename the-thing-below --repo nkramber/terminal-rpg --yes`. Done 2026-09-14 (D-410).
2. Set the local remote: `git remote set-url origin git@github.com:nkramber/the-thing-below.git`. Done 2026-09-14.
3. Run `git fetch`, and confirm that the remote answers. Done 2026-09-14.
4. After the docs audit PR merges, start a short branch from `main` for the rename PR (D-8, D-411). Done 2026-09-14.
5. Replace the working title with the new names in `README.md`, the documents, the agent files, and the skills (D-217). Done 2026-09-14.
6. Keep the dated records as they are: `docs/reviews/`, the handoff entries, `docs/archive/`, and the rows of the two registers. Done 2026-09-14.
7. Run the STE check. Open the rename PR, and answer the automated pass (D-66). When the PR changes no decision row, the session applies the `review-override` label after the pass approves (D-67, D-401). Done 2026-09-14 in PR #5.
8. The owner merges the rename PR. Done 2026-09-14. PR #5 merged as `aee6f35`.
9. Clone the repository to the SSD: `git clone git@github.com:nkramber/the-thing-below.git /Volumes/SSD-1TB/the-thing-below`. Done 2026-09-14.
10. Copy the local session notes that key on the old checkout path to the key of the new path. Done 2026-09-14. The new key is `-Volumes-SSD-1TB-the-thing-below`.
11. Open a session in the new checkout, and run the STE check there. Done 2026-09-14, with 0 findings.
12. The owner deletes the old checkout, only after the new checkout passes. Done 2026-09-14.

## Notes

- Claude Code keeps its local project notes under `~/.claude/projects/`, in a folder named from the checkout path. Step 10 copies the `memory/` folder of the old name to the folder of the new name.
- The dated records keep the old name, because a rewrite of a dated record falsifies it (D-10).
