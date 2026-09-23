# PR-60 review response

Date: 2026-09-23

This file answers the review of `docs/reviews/pr-60.md`, which gave the verdict `Changes required` for head `aa1a10f`.

## P2-1: PR deletes and rewrites dated review history

Disposition: full merit.

Evidence: the finding cites D-10, and the text of D-10 holds no rule about the rewrite of a dated record. The rule is in the `ste-writing` skill: "A dated record is history, and a rewrite falsifies it." `docs/runbooks/rename-and-move.md` applies the same rule with D-10. D-17 requires one review file for each PR, and `aa1a10f` deleted `docs/reviews/pr-58.md`. The finding thus holds, with this correction of its citation.

The owner chose the deletion in the author session, after the session named the conflict with the dated-record rule. The finding and that answer conflicted, so the session asked the owner again (D-19, D-24). The owner chose to keep the dated records.

Correction:

- `docs/reviews/pr-58.md` and `docs/reviews/pr-59.md` are the same as on `main` at `a20d32f`.
- Sessions 209 and 210 are in `docs/session-handoff-archive.md`, with the text of `main`. Each entry of PR #59 keeps its text of `main`.
- `docs/decisions.md` keeps D-895 and each `Revised in part by D-895` marker. D-909 ends the pause, and D-895 reads `Superseded by D-909`. Each of the six revised rows names D-909.
- The rule files stay reverted: the four skills, `CLAUDE.md`, `AGENTS.md`, the PR template, and `docs/runbooks/session-context.md` are the same as at `8d98c46~1`.

Regression check:

- `git diff origin/main -- docs/reviews/pr-58.md docs/reviews/pr-59.md` shows no line.
- `git diff origin/main -- docs/session-handoff-archive.md` shows added lines alone: sessions 213 to 210 moved from the handoff.
- `git grep -n "D-895"` outside `docs/decisions.md`, the handoff files, and `docs/reviews/` shows no line.
- `make ste-check` gives 0 findings.

## New ids

- D-909.

## The final head

The correction commit is the effective head for the repeat review.
