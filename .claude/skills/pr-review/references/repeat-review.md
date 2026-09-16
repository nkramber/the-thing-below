# PR review: repeat review

Part of the `pr-review` skill (D-588). Load this file when the session reviews a PR again after a correction. The scope rules of `SKILL.md` apply to each new finding.

## Repeat review procedure

Do these steps in order after the author revises the PR.

1. Read the response file when one exists.
2. Check the provider gate again. A reviewer fix changes eligibility.
3. Read the new head, the new base, the whole `git diff --stat`, and the diff since the reviewed head (D-589).
4. Verify each claimed fix against its original trigger and its regression check.
5. Set the `Status` line of each prior finding. Keep every id and every piece of evidence.
6. Inspect the new diff for new defects and affected consumers.
7. Add any new finding with the next index in its severity.
8. Update the Identity list to the new effective head.
9. Update the Verification section with the commands that ran on the new head.
10. Write the verdict against the new head. Keep one verdict name in the Verdict section.
11. Commit the review record and the handoff entry together, then run the session end gate.

Edit the existing `docs/reviews/pr-<number>.md`. Do not create a second file for the same PR.
Do not delete the prior verdict. Replace it, and keep each finding and its history.

Put the earlier verdict in a section above the Verdict section, under the heading `## Earlier verdicts`. The gate reads the section under the exact heading `## Verdict`, and it fails a section that names two verdicts. The count reads the prose too, so the reason after the verdict names no other verdict: write "the earlier findings are fixed" and not "the changes required are done".

Close a finding only when the evidence establishes the fix or an owner decision resolves it.
Record any required check that still waits for a result.

### When a finding closes

A finding closes when the correction makes its stated trigger pass and its regression check pass. Set the status to `fixed in <sha>` then.

A new trigger for the same class of defect is a new finding with a new id. Assess that new finding against the scope rules of `SKILL.md`. It is not a reason to hold the old id open.

Stop at the third assessment of one id. Write the pattern in the review record, and ask the owner whether this PR carries the whole surface, or a later PR does. A fourth correction of one finding is a scope question, and not a defect.
