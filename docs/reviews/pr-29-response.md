# PR-29 response

Date: 2026-09-18

The answer of the author to `docs/reviews/pr-29.md`. The review gave one finding, and the
session read it as a claim (T-4, D-17).

## Identity

- PR: 29
- Branch: `feat/pr-6-tick-and-run-record`
- Base: `1960cf3f1faa34e30daa61df5fed16930d2c1070`
- Reviewed head: `ee1e6ea229e92409ba7d9016a5b1525eefd24e5c`
- New implementation head: the commit of this round, which the handoff entry names.

## P2-1: Snapshot validation accepts an even random-stream increment

Disposition: **full merit**.

### The reproduction

The session wrote the regression test first and ran it against the reviewed head. Three tests
failed, and each failure gives the trigger of the finding:

- `AStreamIncrementThatIsEvenIsAnErrorOfTheSnapshotLine`: `Assert.Throws() Failure: No
  exception was thrown`. The reader accepted the record.
- `AnEvenStreamIncrementNeverReachesAReplay`: `Actual: typeof(System.ArgumentException)`, with
  the message `The increment 240 is even, and every PCG32 increment is odd.` The error came
  out of `Pcg32.FromSnapshot` with no line of the record.
- `ASnapshotWithTooFewStreamsIsAnErrorOfTheSnapshotLine`: `Assert.Equal() Failure`. The error
  named the whole record, and not line 2.

The third failure shows the cause behind the finding. The reader made a snapshot on line 2
and checked it later, inside the constructor of `RunRecord`. `RunRecordText.Read` caught that
error and gave `RunRecordException.ForRecord`, which names no line. Thus every fault of a
snapshot lost its line, and the parity of an increment had no check at all.

### The correction

Two changes, and each one is the smallest that restores the contract:

- `TheThingBelow.Core/Runs/RunSnapshot.cs`: `Check` refuses an even increment, and the message
  names the stream and the value. `Pcg32.FromSnapshot` holds the same rule, so the snapshot
  now refuses what the generator refuses (T-2, G-18).
- `TheThingBelow.Core/Runs/RunRecordText.cs`: `ReadSnapshot` calls `Check` inside the read of
  line 2. `ReadLine` already turns an `ArgumentException` into a `RunRecordException` with the
  line, so every fault of a snapshot now names line 2.

### The regression checks

Three tests in `TheThingBelow.Tests/RunRecordTextTests.cs`, and each one fails on the reviewed
head `ee1e6ea` for the reason above (T-3):

1. `AStreamIncrementThatIsEvenIsAnErrorOfTheSnapshotLine` reads a record with an even
   increment, and it asserts `RunRecordException`, line 2, and the word `increment`.
2. `AnEvenStreamIncrementNeverReachesAReplay` proves that the fault stops at the read, so no
   replay starts on a stream that gives another sequence (T-7, G-5).
3. `ASnapshotWithTooFewStreamsIsAnErrorOfTheSnapshotLine` is the earlier test, and it now
   asserts line 2 too.

## The simulation version

`SimulationVersion.Current` stays at 3, and G-17 holds. The correction adds a refusal of a
malformed record, and it changes no rule that makes a state. The evidence:

- `replay-identity` matches the committed file for all 5 runs, the `replay` run included.
- The content hash stays `5ce12c64...f3c15f3`.
- The smoke session gives the state hash `0x82def31590ae6c3b`, which is the value of the
  reviewed head.

PR-5 took the same reading for the kind check of D-646, which also added a refusal alone.

## The state of the build

`make verify` passes: the build with 0 warnings, 477 tests, the format check, `det-lint`,
`ste-check`, the identity check, the content hash, and the headless Godot session.

## New ids

This round adds no decision, no question, and no finding of the design doc. The next ids stay
D-654, OQ-186, and F-80.
