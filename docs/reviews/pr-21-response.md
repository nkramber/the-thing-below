# PR-21 review response

Date: 2026-09-17

The review of `docs/reviews/pr-21.md` gives `Changes required` for head `40ea275`. It holds three
findings. Each one reproduces, and each one has full merit. This round corrects all three, and it
adds a regression test for each one that fails on the old code (T-3).

## P1-1: the verdict check accepts a negated approval

Disposition: full merit.

The trigger reproduces. `CheckVerdict` read the whole `## Verdict` section as one text, and it
searched for each verdict name in that text. The text `**Not Ready for owner merge.**` holds the
approved name inside a longer word group, so the rule found one name and passed. A record that
refuses the merge could pass the gate, against T-2 and D-15.

Correction: `TheThingBelow.Tools/ReviewGate/ReviewRecordRules.cs` now reads the verdict line of the
section. The line starts with the verdict name in bold. The rule takes that name, and it fails a
name outside the three names of the `pr-review` skill. It also fails a section with no such line.

The `pr-review` reference file changed with the command. The row of the machine-read table now
gives the form of the verdict line (D-579).

Regression checks, in `TheThingBelow.Tests/ReviewGateRuleTests.cs`:

- `ANegatedApprovedVerdictFails`: `**Not Ready for owner merge.**` gives a fault.
- `AVerdictSectionWithNoBoldVerdictLineFails`: a section with no bold line gives a fault.
- `AVerdictLineOfBlockedFailsWhenTheProseNamesTheApprovedVerdict`: the bold name decides the result.

## P1-2: the head check accepts a field outside Identity

Disposition: full merit.

The trigger reproduces. `CheckHead` read every line of the record, and it took the first head field
of the whole file. A record with no head field in its Identity list passed the rule when another
section named the effective head. The record of a stale identity could then pass RG 5.

Correction: the rule now reads the `## Identity` section, and it takes the head field of that
section alone. A record with no `## Identity` section gives a fault too.

The row of the machine-read table in the `pr-review` reference file names the `## Identity` list.

Regression checks:

- `AHeadFieldOutsideTheIdentityListFails`: an Identity list with no head field, and a head field
  under `## Verification`, gives a fault. The old rule passed this record.
- `ARecordWithNoIdentitySectionFails`: a record with no Identity section gives a fault.

## P2-1: the deferral check misses spelled-out pull requests

Disposition: full merit.

The trigger reproduces. The phrase set holds `a separate pr`, and a line with the words
`a separate pull request` passed RG 8. Each other phrase of the set had the same hole.

Correction: `DeferralPhrase` now reads `pull request` as `pr` before it looks for a phrase. The
correction covers every phrase of the set at one time, and not the one phrase of the finding. A
list of both forms of each phrase grows with each new phrase, and one form can drop out of it.

The `one-pr-one-session` skill names the rule, so an author knows that both forms fail (D-579).

Regression checks, two new rows of `EachDeferralOfADocumentOfThisPullRequestFails`:

- `No change needed because a separate pull request holds ...` gives a fault.
- `Changed: ... A later pull request adds the section.` gives a fault.

## Proof that each test fails on the old code

The round put the two corrected files back with `git stash`, and it ran the tests of the review gate
against the old code. Seven tests failed: the five above, and the two new rows of the deferral
theory. The same tests pass on the corrected code. The solution holds 119 tests.

## One correction outside the findings

The commit `1bdaa89` of the review put the Session 69 entry above the title of
`docs/session-handoff-archive.md`, and the title left the file. This round puts
`# Session handoff archive` back at the top. The STE checker reads no writing rule in that file,
because it is a dated record (D-10), so no check saw the loss.

## Ids and the head

- No new decision and no new question. The corrections apply D-15, D-17, D-577, D-578, and D-579.
- No new finding id. The three review findings keep their ids in `docs/reviews/pr-21.md`.
- The effective head of this round is the commit that holds these corrections.
