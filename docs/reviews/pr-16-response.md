# PR-16 review response

Date: 2026-09-16

This file answers the review in `docs/reviews/pr-16.md` of head `45e960d`.

## P2-1: The PR comment export can succeed with missing comments

Disposition: full merit.

Evidence: a fake `gh` on the `PATH` failed the issue comments call with exit 1 and passed every other call. The block of `docs/runbooks/session-context.md` at `45e960d` exited 0 and left `comments.md` in bash and in zsh. The exit status of a brace group is the status of its last command, so the earlier failure was lost.

Correction: `docs/runbooks/session-context.md`, section "Evidence for a review" (D-589, T-2). The calls now run in one `&&` chain into `part.md`. Only a full chain renames `part.md` to `comments.md`. A failed call removes `part.md`, prints `comment export failed, no comments file`, and returns nonzero. The command also removes an old `comments.md` first, so a file from an earlier run cannot look current. The prose names the rule.

Regression check: the check extracts the block from the runbook, sets `n=16`, and runs it with the fake `gh` in bash and in zsh.

| Runbook text | Shell | Exit | `comments.md` |
|---|---|---|---|
| `45e960d` | bash | 0 | present |
| `45e960d` | zsh | 0 | present |
| corrected | bash | 1 | absent |
| corrected | zsh | 1 | absent |

The corrected block ran with the real `gh` on PR #16 too. It saved `comments.md` with three comment headings. `bash -n` and `zsh -n` pass.

## P2-2: The commit command stops when no STE files are staged

This section answers the repeat review of head `d2479ce`.

Disposition: partial merit.

Evidence against the stated trigger: in the runbook text at `d2479ce`, the `files=` assignment ends its line. No `&&` follows it. A scratch repository staged `docs/reviews/r.md` and `docs/session-handoff.md` alone and ran the extracted block. The block committed in bash and in zsh. The `grep -v` status of 1 does not stop a plain run.

Evidence for the part with merit: the same block made no commit under `set -e`, in bash and in zsh. It made no commit when a session joined the `git add` line and the `files=` line with `&&`. The reviewer and the author both ran the block in a joined form. Thus the block fails on dated records alone in a form that sessions use.

Correction: `docs/runbooks/session-context.md`, section "Checks in the commit command" (D-585, T-2). The filter now reads `{ grep -v ... || [ $? -eq 1 ]; }`. A status of 1 means an empty list and passes. A status of 2 is a `grep` error and still fails. The prose says that a list of dated records alone skips the checker.

Regression check: the check extracts the block from the runbook, stages the two dated records in a new repository, and counts the commits.

| Runbook text | Shell | Mode | Commits |
|---|---|---|---|
| `d2479ce` | bash | plain | 1 |
| `d2479ce` | bash | `set -e` | 0 |
| `d2479ce` | zsh | plain | 1 |
| `d2479ce` | zsh | `set -e` | 0 |
| corrected | bash | plain | 1 |
| corrected | bash | `set -e` | 1 |
| corrected | zsh | plain | 1 |
| corrected | zsh | `set -e` | 1 |

A staged file with a semicolon still stopped the corrected block before the commit, in bash and in zsh. `bash -n` and `zsh -n` pass.

## The other parts of the Blocked verdict

- The stale Gitar review: full merit. The review commits `60260ef` and `b348395` came after the Gitar review of `d2479ce`. The author requests a Gitar review of the new head after this push, and the PR description records it.
- The absent check runs: no merit as a blocker. No workflow exists yet. The PR gate names PR-1, PR-4, PR-15, PR-41, PR-46, and PR-49 as the creators of each check (G-16). The `gitar-review` skill states that a manual review can attach no check run to the head. The skill rule "Prove that a review is current" uses the dashboard edit time instead.

## New ids and head

- No new D-#, F-#, or OQ-# id.
- The final PR head is the commit that holds this file. It changes `docs/runbooks/session-context.md`, so it is the new effective head.

## Note for the repeat review

The Verification section of `docs/reviews/pr-16.md` ends with the placeholder `<review metadata sha>` in its push line. The author does not edit the review record, so the reviewer can correct that line.
