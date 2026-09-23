# PR review: the author answer

Part of the `pr-review` skill (D-588). Load this file when the session is the author and answers gitar or a review.

## The automated pass

An automated reviewer, gitar, comments on every PR after a push (D-14). This pass comes before the cross-provider review and never replaces it (T-4). A documentation PR answers the pass too (D-66).

After each push, the author loads `.claude/skills/gitar-review/SKILL.md` and follows it. That skill holds the procedure: get a current review of the head, read each finding as a claim, fix or refute it, reply, and resolve. This section does not repeat it.

These rules of this repo add to the `gitar-review` skill, and they win over it:

- The author answers every comment before the hand-over to the other provider, or before the session applies the `review-override` label (D-67).
- When the pass is complete, run `make codex-review PR=<n>` in the background (D-926). On a documentation PR that changes no row of `docs/decisions.md`, apply the `review-override` label yourself at this point instead (D-67, D-401).
- A reply names no provider, harness, or model as the source of the work (T-6, D-22).
- Record the pass in the handoff entry: the count of comments, the count with merit, and the commit that answered each one.

## Address review findings

Use this section when you answer a review. The author does this work, not the reviewer.

**A finding is a claim, not a fact.** A review can be wrong. Assess each finding against the evidence before you change anything. A finding carries no authority that the evidence does not give it.

1. Run `git fetch` and `git status --short --branch`. If the checkout is ahead of the remote with the reviewer's commit, push it first. Record that in the response file.
2. Read the finding, then read the file and the lines it names.
3. Reproduce the trigger. A finding that does not reproduce has no merit.
4. Read the contract the finding cites. Check the `Effect` column of `docs/decisions.md` for a later revision.
5. Decide the disposition: full merit, partial merit, or no merit.
6. Correct every finding that has merit. Use the smallest change that restores the contract.
7. Record each disposition in `docs/reviews/pr-<number>-response.md`.
8. Commit the response, the corrections, and the handoff entry, then run the session end gate.

Push back when the evidence supports it. State the reason and show the proof:

| Reason to push back | What to show |
|---|---|
| The finding reads a rule too broadly. | Quote the rule. Name the other files that the broad reading also condemns. |
| The finding cites a superseded decision. | Quote the `Effect` column and name the current decision. |
| The finding calls a partial revision a supersession. | Quote the `Revised in part by` marker and the part that still stands. |
| The trigger does not reproduce. | Give the command, the revision, and the result. |
| The correction breaks another contract. | Name the contract and the caller that it breaks. |
| The finding states a style preference. | Name the contract that the code does not break. |
| The finding repeats a risk that a decision already accepted. | Quote the D-# id and its accepted risk. |
| The finding asks for work outside the PR scope. | Quote the roadmap entry and the exit tests. Name the PR that holds the work. |
| The `codex-review` command gives the three-strike stop for one id. | Follow the three-strike stop of `docs/runbooks/merge.md`. Name the three rounds, and ask the owner (D-19, D-929). |

A disagreement belongs in the response file, with the evidence. Never delete a finding from the review record.
The reviewer sets a refuted finding to `withdrawn` and keeps the evidence that refuted it.

Never accept a finding only to close the review faster. A wrong correction costs more than a written disagreement.
Never widen a correction past the contract that the finding names.
Ask the owner when a finding and an owner decision conflict. Quote both (D-19, D-24).

Partial merit is common. Correct the part that has merit, and refute the rest in the same entry.

## The response file

The author answers a review in `docs/reviews/pr-<number>-response.md`.
This file is a convention, not a gate. `review-gate` does not read it.
Write one when the verdict is `Changes required` or `Blocked`. A clean first pass needs none.

The response file states, for each finding:

- The disposition: full merit, partial merit, or no merit.
- The evidence, when the disposition is partial merit or no merit.
- The correction that landed, with the file and the decision id.
- The regression check that ran, and its result.

The response also lists each new D-# and F-# id, and the final PR head.
A disagreement with a finding belongs here, with the evidence. Do not remove the finding from the review file.
