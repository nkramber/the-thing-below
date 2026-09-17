# The handover of the Deck test

Status: the record of a spike session. Owner: Nate. Written in ASD-STE100 (D-10).

This file keeps the owner answers and the test state that the Deck test session collected.
The session was the step 7.1 of `docs/roadmaps/phase-1-foundations.md`, which has no PR and
no review. Thus no document in `docs/` holds these answers yet. The PR-1 session reads this
file and records each row.

## The owner answers of this session

The owner gave these answers on 2026-09-16. Each one needs a decision row, from D-592.

| Question | The answer | Note |
|---|---|---|
| OQ-75 | MTP mode, with `xunit.v3` as the one test package | The test commands of `CLAUDE.md`, `AGENTS.md`, and `csharp-conventions` change |
| OQ-76 | Coverlet and ReportGenerator | Two packages need rows (G-13). The job shows a Markdown summary and uploads the file |
| OQ-77 | The labels with a version: `ubuntu-24.04`, `windows-2025`, `macos-26` | Each move to a newer image takes a PR |
| OQ-78 | A first job reads the paths, and each build and test job skips on a docs PR | Not the recommendation. Read the trap below |
| OQ-83 | The cache action of D-511, with a SHA-512 check on every run | This session ran it, and the SHA-512 matched |
| OQ-92 | A branch of its own, `spike/deck-test` | This branch. It never merges |
| OQ-93 | The test scene measures itself | Done. `scripts/FrameMeter.cs` holds the measurement |
| The renderer of PR-1 | Forward+, as a provisional setting | Revises D-160 in part. Read the section below |

## The trap in the answer to OQ-78

The owner chose the path filter. A path filter at the level of the workflow leaves each check
`Pending`, and a required check that stays `Pending` stops the merge.

Thus PR-1 puts the condition on each job, and never on the workflow. A job that a condition
skips reports `Success`, and the required check passes.

## The deferral of the renderer pick

The owner deferred the Deck test on 2026-09-16, and PR-1 starts before it. This answer
revises D-160 in part: the pick can come after PR-1. Each other part of D-160 stands.

PR-1 sets Forward+ in the Game project as a provisional setting. Forward+ is the default of
Godot, and it gives the HDR 2D that the glow of D-188 needs. Mobile also gives HDR 2D.

After the owner runs the Deck test, a PR of its own sets the renderer that the test picked.
That PR changes one line of `project.godot`, and it cites the reports of the Deck.

No exit test of PR-1 reads the renderer. Four of the seven cover `make verify`, the three CI
legs, the headless smoke session, and the STE check. The other three cover the agent-file
match test, the Core reference test, and the coverage report.

## The state of the Deck test

The test scene is complete, and the native Linux export is ready. The run on the Deck is the
next step, and the owner does it.

- The scene, the shaders, the meter, and the report all work. A run on the Mac proved them.
- The Mac gives no numbers, because macOS caps the frame rate. `readme.md` holds this trap.
- No machine ran the export `build/DeckTest.x86_64`. Its first run is the run on the Deck.
- The renderer pick of D-160 and the effect budget of D-523 wait for that run.

## What the PR-1 session does

1. Fetch this branch, and read this file and `readme.md`.
2. Record a decision row for each answer above, from D-592, with the date 2026-09-16.
3. Record the deferral of the renderer pick, which revises D-160 in part.
4. Add the cost model rows of the Deck test to `docs/design.md`, section 4.
5. Mark OQ-75, OQ-76, OQ-77, OQ-78, OQ-83, OQ-92, and OQ-93 as resolved in `docs/questions.md`.
6. Add the handoff entry of the Deck test session, and then its own entry.
7. Build the scaffold of PR-1, with Forward+ as the provisional renderer.
8. Add a line to the roadmap entry 7.2, because the renderer bullet now waits for a later PR.

The traps of `readme.md` apply to PR-1 and to PR-54. The trap of the solution file and the
trap of the exit code both hide a broken export.
