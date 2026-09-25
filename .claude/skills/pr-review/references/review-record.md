# PR review: the review record

Part of the `pr-review` skill (D-588). Load this file before you write a review record or correct the PR description. Load it too before you read the head and the verdict for `review-gate`.

## Review record

Use one file per PR in `docs/reviews/` (D-17). Reuse its existing name and finding ids on repeat reviews.
For a new record, use `docs/reviews/pr-<number>.md` with the actual PR number, not the roadmap id.
Record provider names only in the permitted review record and handoff author fields (D-22).
Omit those names from any PR description or GitHub comment.

The `review-gate` job reads this file (D-15), and so does the `codex-review` command (D-926). Four parts of it are machine-read. Keep their format exact:

| Part | Exact form | Rule |
|---|---|---|
| The file name | `docs/reviews/pr-<number>.md` | The number is the GitHub PR number, not the roadmap id. |
| The head field | `- Head: ` and the hash in backticks, in the `## Identity` list | The hash is the effective head. A short hash is permitted. The rule reads that list alone. |
| The verdict | The verdict line of the `## Verdict` section, such as `**Ready for owner merge.**` | The line starts with one of the three names in bold. Write the name exactly, and add no word to it. The section gives one bold span, and that span is the name (D-612). |
| The rounds of a finding | The line `Open at: ` in each finding, with each effective head in backticks | The `codex-review` command reads it for the three-strike stop (D-929). The `review-gate` check does not read it. |

The effective head is the newest commit that changes a path outside the metadata set.
The metadata set holds four paths of this pull request (D-610):

- `docs/reviews/pr-<number>.md`
- `docs/reviews/pr-<number>-response.md`
- `docs/session-handoff.md`
- `docs/session-handoff-archive.md`

A commit that changes only those paths is a metadata commit, and it does not change the effective head.
A commit that changes the record of another pull request moves the effective head.
The required review commit holds the review record and the handoff entry, so it is always a metadata commit.
Without that rule the review commit invalidates the review that it publishes.
Record the effective head, not the tip, when the review commit is the last commit.

A repeat review replaces the verdict of the `## Verdict` section. Put each earlier verdict in a
section of its own, such as `## Earlier verdicts`. A second bold span in the `## Verdict` section
gives a fault, because the gate cannot know which span is the current verdict (D-612). Give the
reason of the verdict in prose after the name, and use no bold in that prose.

Use this skeleton. Keep the heading text and the order.

```markdown
# PR-<number> review

Date: <YYYY-MM-DD>

## Identity

- PR: <number>
- Target: `main`
- Base: `<sha>`
- Merge base: `<sha>`
- Head: `<effective head sha>`
- Branch: `<branch>`

## Provider gate

State the author provider, the source of that fact, and the reviewer provider.
State the gate result against T-4 and D-17.

## Intended behavior and scope

State the intent, what the review inspected, and every affected contract.
List each path of `git diff --stat` as inspected, or name it as uninspected (D-589).
Name any area that remains uninspected.

## Findings

One subsection per finding, in severity order. Use the finding format below.
Write "No finding." when the review found none.

## Out of scope

One line per concern that a later PR holds. Name that PR or roadmap item.
Give no severity here. Write "None." when the review found none.

## PR comments

One line per existing comment thread on the PR: the claim, the author's answer, and what the review verified (D-14).
Write "None." when the PR holds no comment.

## Description edits

One line per correction that this review made to the PR description.
Give the old value and the new one. Write "None." when the review changed nothing.

## Verification

One line per command or check, with its result.
Name each check that did not run and the reason.
End with the push line: `- Push: <sha> is the head of origin/<branch>, verified with gh pr view.`

## Open questions and accepted risks

Name each open OQ-# and each accepted risk with its D-# id.

## Verdict

**<Blocked | Changes required | Ready for owner merge>.** This verdict applies to head `<sha>`.
Give the reason in one or two sentences.
```

## Finding format

Give each finding a stable id: the letter `P`, the severity number, a hyphen, and an index. `P1-1` is the first P1 finding.
Keep the id for the life of the PR. Never renumber a finding on a repeat review.

```markdown
### P<severity>-<n>: <short title that states the defect>

Status: <open | fixed in `<sha>` | accepted risk, D-# | withdrawn>.

Open at: `<sha>`, `<sha>`.

File: `<path>:<line range>`, or Commit: `<sha>`.

Trigger: the input or state that produces the defect.

Expected: the required behavior, with the contract, tenet, guardrail, or D-# id.

Actual: the observed behavior.

Consequence: the effect on the player, the data, the build, or the maintainer.

Correction: the smallest change that restores the contract.

Regression check: the command or test that establishes the fix, and the result that must appear.
```

A withdrawn finding stays in the file with the evidence that refuted it. Never delete a finding.

The `Open at:` line lists the effective head of each round in which the finding is open, oldest first (D-929). In each round, add the head of the round to each finding that is open in that round. Never remove a head. A finding that is not open in a round does not get the head of that round. The `codex-review` command gives a fault when an open finding does not list the effective head, or when a closed finding lists it.

## Correct the PR description

A PR description is part of the documentation set. A description that names a stale head, an old count, or a superseded correction misleads the owner at the merge.

The reviewer corrects such a description directly. It needs no finding, and the author needs no extra pass for it.

The reviewer changes only a fact that the review verified:

- the effective head, the base, or the merge base.
- a count that the review ran, such as the test total or the finding total.
- a check result that the review read.
- a sentence that names a correction that a later commit replaced.

The reviewer never changes:

- what the author says the PR does, or why.
- a decision, a tradeoff, or a recommendation.
- a gate line that the owner ticks.

Name no provider, agent, harness, or model in the description (T-6, D-22).

Write one line for each edit in the review record, under `## Description edits`. Give the old value and the new one. The owner then reads every change in one place.

A claim that is wrong in substance stays a finding. The reviewer corrects a stale fact, and the author corrects a wrong statement.

## The review gate check

The `review-gate` check applies eight rules, RG 1 to RG 8 (D-15, D-579). RG 1 and RG 2 read the `review-override` label. RG 6 to RG 8 read the handoff and the Documents section of the description (D-581). Three rules read this file:

1. RG 3: `docs/reviews/pr-<number>.md` exists for the PR number.
2. RG 4: the verdict is `Ready for owner merge`.
3. RG 5: the head in the Identity list is the effective head, or an earlier head that D-943 keeps approved.

The check also passes a PR in the override set that has the `review-override` label and changes no decision row (D-16, D-401). A PR that changes `.github/workflows/` never passes on the label, because each gate lives in a workflow file (D-560). A PR that changes `.claude/settings.json` or .claude/settings.local.json never passes on it too, because each file can hold a hook that runs a command (D-700, D-1086).

The check has three states. Read the color before you start:

| Color | Meaning | What to do |
|---|---|---|
| Grey | No review record exists for this PR. The job line reads red. | Write one. This is the normal state before a review. |
| Red | A review record exists, and it does not approve this head. | Read the findings. The author corrects them. |
| Green | An approved review covers the effective head. | The author may turn on the auto-merge, and the owner may merge (D-8, D-930). |

Rule 3 fails when the author pushes code after the approval. That result is correct.
Reassess the new diff, then update the head field and the verdict together.
Rule 3 does not fail when the last commit changes only the metadata paths.
Rule 3 does not fail when each commit after the approved head changes only paths of the skip set (D-857, D-943).
A new review still names the effective head, because it reads that head.

The check cannot run on the PR that creates it or changes it, because GitHub starts `pull_request_target` only from `main` (F-37). Such a PR proves the command in Tests, and the live check reads the new rules on the next PR (D-500).
