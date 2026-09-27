---
name: night-fix
description: The loop of a fix session that the night watcher starts for a failed night of main. Replay the failed seed, fix it on a new PR, prove the fix with a branch night, pass the Codex review, and send a Pushover. Load when the prompt of the night watcher names this skill.
---

# Night fix skill

The night watcher starts this session with `claude -p` in a new worktree of `main` (D-1205). The session has no person at the prompt, and it runs with the permission mode `bypassPermissions` (D-1208). The session is the author of one fix PR. It never merges, and it never turns on the auto-merge (T-4, D-933).

Every rule of `CLAUDE.md` applies, with one change: D-1206 revises D-19 in part for this session. Load `one-pr-one-session`, `ste-writing`, `csharp-conventions`, `gitar-review`, and `pr-review` before the first change.

## The binding

- Repository: `the-thing-below`.
- Roadmap id: the highest `PR-` id of section 8 of `docs/design.md` and of the phase files, plus one.
- Branch: `fix/pr-<roadmap id>-night-<run id>`, from `origin/main` (D-8).
- Role: author. The handoff author field is `Claude Code`.

The worktree starts on a detached `origin/main`. Make the branch in the worktree. Never change the main checkout of the owner, because the owner can work there.

## The loop

1. Read the top entry of `docs/session-handoff.md`.
2. Read the failed night with `gh run view <run id>`. Write down the first seed and each failed leg.
3. Download the artifact `night-failed-runs-<leg>` of each failed leg. `docs/runbooks/night.md` gives the command.
4. Replay each failed record with the `bots` command, and find the cause (T-7).
5. Make the branch. Fix the cause, and add a regression test that fails on the old code (T-3).
6. Raise the simulation version when the fix changes a behavior of Core (G-17).
7. Add the roadmap entry of the fix PR to the phase file and to section 8 of `docs/design.md`.
8. Run `make verify`. Commit with the STE check of `docs/runbooks/session-context.md`.
9. Add the handoff entry of the round, push, and open the PR with the template.
10. Run the Gitar poll of the `gitar-review` skill in the background. Answer each Gitar item.
11. Start a branch night on the failed range, and watch it in the background:

```bash
gh workflow run night.yml --ref <branch> -f first-seed=<first seed of the failed night>
gh run list --workflow night.yml --branch <branch> --limit 1 --json databaseId --jq '.[0].databaseId'
gh run watch <run id> --exit-status
```

12. On a red branch night, count it. Go back to step 4 with the new records.
13. On a green branch night, run the `night-gate` check of the PR again. `docs/runbooks/night.md` gives the command.
14. Run `make codex-review PR=<n>` in the background, and read its outcome line (D-926).
15. On `changes-required`, answer each finding with the `pr-review` skill, then push, and do the Gitar pass.
16. After a push that changes a path outside the paths of a docs-only PR, go back to step 11.
17. On `approve`, send the Pushover of the ready PR. Then add the last handoff entry, push it, and end.

The gate accepts a branch night on the head, or on a commit with documents alone after it (D-1204). Thus a commit of a review record or a handoff entry needs no new night. A commit of code needs one.

## The Pushover

The `notify` workflow sends each message, and it reads the secrets of the repository (D-1207). The Mac holds no copy of a secret.

```bash
gh workflow run notify.yml --ref main \
  -f title="The Thing Below: PR ready to merge" \
  -f message="PR #<n> fixes night run <run id>. Codex approved head <sha>. Session <session id>." \
  -f link="https://github.com/nkramber/the-thing-below/pull/<n>"
```

## The three stop events

Stop at these three events alone (D-1205):

- A third red branch night of this PR.
- A three-strike stop of the `codex-review` command: the outcome `three-strike-stop`, with exit code 3 (D-929).
- An absolutely critical owner-only question of D-1206.

At a stop, do these steps, and then end the session:

1. Commit and push the work of the round, with a handoff entry that names the stop.
2. Send a Pushover with the stop event, the PR link, and the session id.
3. Give the owner the resume command in the message: `claude --resume <session id>` in the worktree.

## The questions

D-1206 holds the owner definition, verbatim: "'Absolutely critical owner-only question' means the following: anything that changes game decisions, rules, content, characters, or anything an end-user player might see. It does NOT cover: infrastructure, testing, or anything that an end-user player would NOT see."

- A question of that kind is a stop event. File it in `docs/questions.md` first.
- For each other question, choose the best answer from the code and the documents. Continue the loop.
- Record each such answer as a row of `docs/decisions.md` with the next D-# id. Its Effect column starts with "session best-effort answer".
- Name each best-effort answer in the PR description and in the handoff entry, so the owner reads it before the merge.

## Traps

- The failed night can fail on a new seed with no new code (D-1190). The cause can be old code, and the fix stays a code fix.
- A night takes about 55 minutes. Run each watch in the background, and never poll in the foreground.
- The branch night needs the first seed of the failed night, or no promotion follows the merge (D-1203).
- The promotion turns `main` green at the merge (D-1202). An open PR then needs a new run of its gate.
- The session can run for many hours. The watcher stops it after 24 hours, and it tries to send a Pushover. The log of the watcher holds each stop.
