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

Each round put the corrected files back with `git stash`, and it ran the tests of the review gate
against the old code.

- The first round: seven tests failed. They are the five of P1-1 and P1-2, and the two new rows of
  the deferral theory of P2-1.
- The second round: the two new fault tests of P1-3 failed.
- The third round: the new fault test of P1-4 failed. The two tests of P1-3 failed too, because
  they read the new message of the rule.

Each of these tests passes on the corrected code. The solution holds 123 tests.

## P1-3: the verdict check ignores a later conflicting verdict

Disposition: full merit.

The trigger reproduces. The rule of the first round read the first bold verdict line of the section
and stopped there. A section with `**Ready for owner merge.**` and then `**Changes required.**`
passed the gate.

Correction: the rule now reads each bold name of the `## Verdict` section. It gives a fault when
the section holds more than one verdict name, and it needs the one name to be the approved verdict.

One word of the finding needs a limit. The correction text says that the verdict line holds "only
the approved verdict". A line with no text after the name breaks the skeleton of the `pr-review`
reference file, which is `**Ready for owner merge.** This verdict applies to head <hash>.` The
records of PR #19, PR #20, and PR #21 all hold that text. The correction reads the words of the
regression check instead: the section holds one verdict name in bold, and a second name gives a
fault, on the same line or on a later line. The prose after the name stays legal.

The reference file names the new rule, and it says where an earlier verdict goes (D-579). The
record of this PR already keeps its earlier verdict in `## Earlier verdicts`, so the rule passes it.

Regression checks:

- `ASecondVerdictLineAfterTheApprovedVerdictFails`: a second verdict line gives a fault.
- `ASecondVerdictOnTheLineOfTheApprovedVerdictFails`: a second name on the same line gives a fault.
- `AnEarlierVerdictInAnotherSectionPasses`: a verdict in `## Earlier verdicts` passes. This test
  guards the record shape of a repeat review, and it passes on both versions of the rule.

## P1-4: the verdict check ignores an invalid bold contradiction

Disposition: full merit.

The trigger reproduces. The rule of the second round counted the bold spans that match a verdict
name. The span `**Not Ready for owner merge.**` matches no name, so a section with the approved
name beside it gave one verdict name, and it passed.

Correction: the `## Verdict` section now holds one bold span, and that span is the verdict name.
A second bold span gives a fault, whatever its text. The rule then covers every bold contradiction,
and not the one form of this finding.

This is the third round of findings on RG 4: a match of a part of the text (P1-1), a read of the
first bold line alone (P1-3), and a bold negation beside the approved name (P1-4). Each finding
reproduces, and each correction was narrow. The session asked the owner to settle the scope of the
rule (D-19), and the answer is D-612: one bold span, which is the verdict name, and free prose after
it. A check of the prose stays out of scope, because the reference file asks the reviewer to give
the reason of the verdict in that prose.

The new rule passes each of the 16 records of `docs/reviews/`. A count of the bold spans of the
`## Verdict` section of each record gives one span in each file.

Regression check: `ABoldNegationAfterTheApprovedVerdictFails` gives a fault, and it passes on no
older version of the rule. The two tests of P1-3 now read the new message.

## One correction outside the findings

The commit `1bdaa89` of the review put the Session 69 entry above the title of
`docs/session-handoff-archive.md`, and the title left the file. This round puts
`# Session handoff archive` back at the top. The STE checker reads no writing rule in that file,
because it is a dated record (D-10), so no check saw the loss.

## Ids and the head

- D-612 gives the scope of RG 4, and the owner answered it on 2026-09-17. The other corrections
  apply D-15, D-17, D-577, D-578, and D-579. No new question.
- No new finding id. The three review findings keep their ids in `docs/reviews/pr-21.md`.
- The effective head of the first round is `3a75767`, and of the second round `b466a64`. The
  effective head of the third round is the commit that holds the correction of P1-4.
