# PR-39 review response

Date: 2026-09-19

Reviewed head: `671d712`. Head of this response: the commit that carries this file.

## P1-1: The export smoke command discards a failed process status

**Disposition: full merit.**

The claim reproduces. The step ran the exported game with `|| true`, so it read the log alone
and never read the exit code. A build that writes the success line and then fails gave a green
leg, and the job packed and uploaded that build.

A probe ran the old step text and the new step text against a stub game. The stub writes the
success line of the session and then ends with the exit code 3:

```
old step, game exit 3: step exit 0
new step, game exit 3: step exit 1
new step, game exit 0: step exit 0
```

The first line is the fault. The second line is the correction. The third line shows that a
session with no fault still passes.

**The correction.** `.github/workflows/export.yml` keeps the exit code in `status` and reads it
after the two log checks. The log checks run first, so the message of a session with no success
line stays the first message. The success line stays a condition, because a session whose
managed assembly does not load ends with the exit code 0 and writes no line (F-64).

**The regression checks.**

- `TheSmokeStepKeepsTheExitCodeOfTheExportedGame` of `TheThingBelow.Tests/ExportWorkflowTests.cs`.
- `NoCommandThatStartsTheSmokeSessionDropsItsExitCode` of `TheThingBelow.Tests/SmokeExitCodeTests.cs`.

Both tests fail on the old workflow text. The run of the old text gave 2 failures of 834 tests,
with `Assert.DoesNotContain() Failure: Sub-string found` on the line with `|| true`.

## The same fault in the `Makefile`

The review named `.github/workflows/export.yml` alone, and the finding is correct there. The
`smoke` target of the `Makefile` held the same fault at line 79, and the smoke job of
`.github/workflows/ci.yml` did not, because its pipeline runs under `pipefail`. Thus
`make verify` on the machine of the owner passed a session that wrote the success line and then
failed.

The repair of a second file is a second concern, which G-8 refuses. The session asked the owner,
and the owner chose one PR. D-694 records that answer and names this exception.

- The `smoke` target keeps the exit code in `status` and reads it after the log checks.
- `TheThingBelow.Tests/SmokeExitCodeTests.cs` holds the rule for the three callers of the
  session: the target of the `Makefile`, the smoke job of CI, and the export job.
- `TheMakefileSmokeTargetFailsOnANonzeroExitCode` reads the recipe of the target.
- Both tests of the `Makefile` fail on the old recipe: 2 failures of 837 tests.

## New ids

- D-694: the `make smoke` exit code in PR-54.

## Checks

- `make verify`: passes with 837 tests, 0 warnings, and clean format, det-lint, STE, replay
  identity, content hash, and smoke.
- `make smoke`: passes with the new recipe, and it prints the success line of the session.
- The probe above: the old text passes a failed session, and the new text fails it.

## Head

The final head of this round is the commit that carries this file. The review of that head reads
`.github/workflows/export.yml`, the `Makefile`, `TheThingBelow.Tests/ExportWorkflowTests.cs`,
`TheThingBelow.Tests/SmokeExitCodeTests.cs`, and the four documents of this round.
