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

## New ids and head

- No new D-#, F-#, or OQ-# id.
- The final PR head is the commit that holds this file. It changes `docs/runbooks/session-context.md`, so it is the new effective head.

## Note for the repeat review

The Verification section of `docs/reviews/pr-16.md` ends with the placeholder `<review metadata sha>` in its push line. The author does not edit the review record, so the reviewer can correct that line.
