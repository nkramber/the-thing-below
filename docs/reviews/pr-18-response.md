# PR-18 review response

Date: 2026-09-16

This file answers the review in `docs/reviews/pr-18.md` of head `0c402dd`.

## P2-1: The local smoke target can accept a failed Godot build

Disposition: partial merit. The defect is real, and its stated trigger is not.

### The trigger of the finding does not reproduce

The finding says that the editor "build callback fails while `--build-solutions` returns exit code 0, as F-60 documents". A direct run refutes that claim. The check removed the `net10.0` pin of `TheThingBelow.Game.csproj`, which is the trigger of F-60, and then ran the editor with its output to a file:

```
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --editor \
  --path TheThingBelow.Game --build-solutions --quit > /tmp/d.log 2>&1
```

The editor gives an exit code of 1, and not 0. The log holds `An EditorPlugin build callback failed`. On the text of `0c402dd`, `make smoke` also failed with an exit code of 2 on this trigger.

F-60 carried the wrong claim, and this PR wrote it. The first measurement ran the editor through `| tail -30` and read `$?` after the pipe, so it read the exit code of `tail`. The row of F-60 in `docs/design.md` now marks that part refuted and keeps it, and `docs/roadmaps/area-ci.md` and `phase-1-foundations.md` follow.

### The defect is real, through a different failure

The verification found the failure that the finding aims at. A headless session whose managed assembly does not load never reaches `Quit`, and it runs without end. The check moved `TheThingBelow.Game.dll` away and ran the session. It wrote `Cannot instantiate C# script` and then waited until a kill.

With `--quit-after`, the same session ends with an exit code of **0** and writes no success line. So the exit code of the session hides the fault, and the exit code of the build does not. The `smoke` target of `0c402dd` read neither log, so it reported success for a session that never ran the boot code. This is F-64.

### Correction

`Makefile`, the `smoke` target (T-2, D-117, F-60, F-64):

- Each command writes its log to a file, and never through a pipe, because a pipe hides the exit code of the Godot process.
- The build step fails on a nonzero exit code and prints the last lines of its log.
- The session runs with `--quit-after $(SMOKE_FRAME_LIMIT)`, which is 600 frames.
- The target fails when the success line is absent, and when the log holds an error line.

`.github/workflows/ci.yml`, the `smoke` job: the session runs with `--quit-after 600` too, so a broken session fails the job in seconds and not at the time limit of 30 minutes. The comment that named an exit code of 0 for the editor build now names the true behavior, and the log check stays as a second guard.

### Regression check

| Case | `make smoke` on `0c402dd` | `make smoke` corrected |
|---|---|---|
| A healthy tree | 0 | 0 |
| The Godot build fails (F-60 trigger) | 2 | 2 |
| The scene cannot instantiate the boot class (F-64 trigger) | runs without end | 2, in 6.5 seconds |

Case three renamed the class of `TheThingBelow.Game/scripts/Boot.cs`, so the build passes and the scene cannot instantiate the script. The corrected target printed `smoke: the session wrote no success line. Read artifacts/smoke.log (T-2).` The tree was restored, and `make smoke` gives 0 again.

## P2-2: The review document line does not follow the required form

Disposition: full merit.

Evidence: the Documents section of the PR description held `` - `docs/reviews/`: This PR waits for the Codex review, which writes `docs/reviews/pr-<number>.md`. `` D-581 allows three forms: `Changed: <path>. <reason>`, `No change needed because <reason that names the path>`, and `Not applicable because <specific reason>`. The line matches none of them, and it names a placeholder path and not `docs/reviews/pr-18.md`.

Correction: the PR description now reads `` - `docs/reviews/`: Changed: `docs/reviews/pr-18.md` and `docs/reviews/pr-18-response.md`. The review record of this PR, and the answer to it. `` The record and this response are both on the branch, so `Changed` is the correct form.

Regression check: the Documents section was read again against D-581. Each of the twelve lines starts with its row name and uses one of the three forms.

## The review notes

- `make verify` did not complete in the review, because a local `dotnet build` produced no output for 60 seconds. It completes on the machine of the owner. The run after these corrections is recorded in the handoff entry.
- The review could not export the PR comments, because `gh api` did not connect. The Gitar finding on `.github/workflows/ci.yml` line 241 has a reply at `https://github.com/nkramber/the-thing-below/pull/18#discussion_r4032182291`, and the thread is resolved. A query of the review threads gives 0 unresolved threads.

## New ids

- F-64: a headless session whose managed assembly does not load runs without end, and its exit code hides the fault.
- F-60 keeps its id, and its exit-code part is marked refuted.
- No new D-# and no new OQ-#.

## The head

The corrections land on a commit above `0c402dd`. That commit changes `Makefile` and `.github/workflows/ci.yml`, so it becomes the new effective head, and the review of it is a repeat review.
