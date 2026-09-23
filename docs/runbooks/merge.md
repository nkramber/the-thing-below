# The review and the merge

Status: active runbook. Written in ASD-STE100. Decisions: D-926 to D-933, and D-942 to D-946.

This runbook gives the loop of the author from a push to the merge. It also gives the review command, the three-strike stop, the auto-merge, and the merge settings of the repository. The `one-pr-one-session`, `gitar-review`, and `pr-review` skills hold the rules, and this runbook holds the commands.

## The author loop

1. Push the round, with its handoff entry (D-18).
2. Get a complete Gitar pass of the head with the `gitar-review` skill, and answer each comment (D-14).
3. Start `make codex-review PR=<n>` in the background, and wait for the notice at its end.
4. Read the last line of the output: `codex-review: outcome <name> (exit <code>)`.
5. On `approve`, go to "The confirmation and the auto-merge".
6. On `changes-required`, answer each finding with the `pr-review` skill, then go to step 1.
7. On `three-strike-stop`, go to "The three-strike stop".
8. On `fault` or `refused`, read the reason lines, correct the cause, then go to step 3.

**Gitar pause (D-945).** Step 2 is one read of the Gitar output with commands B and C of the `gitar-review` skill, with no wait for a pass. Step 3 runs `make codex-review PR=<n> -- --skip-gitar-review` (D-946). A Gitar review thread or a finding of the dashboard is feedback. On feedback, stop at once, and tell the owner before any other step. A claim of the CI analysis alone gets its answer with no stop.

## A commit of documents alone

After an approval, a commit that changes paths of the skip set alone keeps the approval (D-943). The skip set holds `docs/`, all of `.claude/`, `README.md`, `CLAUDE.md`, `AGENTS.md`, `LICENSE`, and the PR template (D-857). The `review-gate` check stays green, and the PR needs no new run of `make codex-review`. A change of a decision row keeps the approval too.

Such a commit still gets its Gitar pass, and the author answers each comment and each claim of the pass (D-944). Do these steps for a commit of documents alone after the approval:

1. Turn off the auto-merge with `gh pr merge <n> --disable-auto` when it is on.
2. Push the commit, with its handoff entry.
3. Get a complete Gitar pass of the new head with the `gitar-review` skill, and answer each comment.
4. Go to "The confirmation and the auto-merge".

**Gitar pause (D-945).** Step 3 is the read of the author loop, with no wait for a pass.

A commit that changes a path outside the skip set needs a new review. Go to step 1 of the author loop.

## The review command

The `codex-review` command of Tools holds the logic, and the Makefile target runs it (D-926). A run does these steps:

1. Install the newest `@openai/codex` with npm, and refuse a version older than the minimum (D-927).
2. Refuse a CLI with no ChatGPT login (D-932).
3. Probe the model `gpt-6-luna` at the effort `medium`.
4. Refuse a PR that is not open, a checkout that differs from origin, and a working tree with changes.
5. Refuse a Gitar pass that is not complete for the effective head.
6. Make a worktree at the head of the PR in the temporary folder, on the local branch `review/pr-<n>`.
7. Run the review with the prompt `Review PR #<n>.` and the instructions of the skill and the push.
8. Fetch, read the record on origin, and compare its head field with the effective head (D-610).
9. Print the verdict, the open finding ids, the three-strike ids, and the path of the transcript.

The flag `--skip-gitar-review` skips step 5, and the command then reads no Gitar fact (D-946). The prompt of the reviewer then says that no complete Gitar pass is a condition of the review. Each review prompt also says that each Gitar item still needs its answer, and that the reviewer ignores a Gitar comment with no item (D-964). Make reads a word after `--` as a goal, so the form of the target is `make codex-review PR=<n> -- --skip-gitar-review`. The target refuses each other extra goal, and the goal of the flag fails without the `codex-review` goal.

The command removes `OPENAI_API_KEY` and `CODEX_API_KEY` from each Codex process that it starts, so no review runs at API prices (D-932). The removal changes the environment of that process alone. The shell and each other process keep their keys.

CAUTION: Do not pass `forced_login_method` to the CLI. A forced method that differs from the login makes the CLI log out. Each Codex process of the machine then fails until the owner logs in again.

The transcript, the error log, and the last message of the reviewer go to the folder artifacts/codex-review, which git ignores. The command removes the worktree after each outcome except a fault. The next run removes a worktree that a fault kept.

| Outcome | Exit code of the command | What the author does |
|---|---|---|
| `approve` | 0 | Go to "The confirmation and the auto-merge". |
| `changes-required` | 2 | Answer each finding, then push the next round. |
| `three-strike-stop` | 3 | Go to "The three-strike stop". |
| `fault` or `refused` | 1 | Read the reason, correct the cause, and run the command again. |

Make gives the exit code 2 for each failed target. Thus read the last line of the output, and not the code of Make. The direct command gives the code of the table:

```bash
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- codex-review --root . --pull-request <n>
```

A review takes longer than the limit of 10 minutes of a tool call. In Claude Code, start the command in the background, and the harness calls the session again when the command ends. Do not poll the command.

The check of the Gitar pass reads the machine part alone. The dashboard has an edit after the push of the effective head, no Gitar check runs, and no review thread of Gitar is open. The author answers each claim of the dashboard text before the run, with the `gitar-review` skill.

## The three-strike stop

The command exits with code 3 when an open finding lists three effective heads or more on its `Open at:` line (D-929). Then do these steps:

1. Turn off the auto-merge with `gh pr merge <n> --disable-auto`. The command gives an error when the auto-merge is off, and that error is no fault.
2. Stop the fix loop. Push no correction of the finding.
3. Ask the owner with `AskUserQuestion`. Give the finding, the evidence of the reviewer, the answers of the author, and the options.
4. Record the answer of the owner in `docs/reviews/pr-<n>-response.md`.
5. Record the answer as a decision too when it sets a rule.

## The confirmation and the auto-merge

Turn on the auto-merge only when each of these conditions holds (D-930, D-933, D-942):

- The last metadata commit is on origin. It holds the review record and the handoff entry of the author.
- The Gitar pass is complete for the effective head under the `gitar-review` skill.
- The record gives `Ready for owner merge` for the effective head, or for an earlier head that D-943 keeps approved.
- The owner confirmed the merge after the summary in four sections.

**Gitar pause (D-945).** The Gitar condition above holds with no pass when the read of the author loop found no open Gitar thread or finding. The `review-override` label of a docs-only PR needs the same read, and no Gitar approval.

The summary goes inside the question block of `AskUserQuestion`, so the owner reads it with the question. Text above a question card does not show. Each section has a few sentences:

- What: the concern of the PR, and what it changes.
- How: the approach, and the main files.
- CI: whether each check is green, with the run id.
- Codex review: the verdict of the record, `Ready for owner merge`, `Blocked`, or `Changes required`, with its head. For a PR under the `review-override` label, write "Not applicable: the `review-override` label covers this PR (D-401)."

Then run these commands. Run the wait in the background.

```bash
gh pr merge <n> --auto --squash
gh pr checks <n> --watch --required --interval 60
gh pr view <n> --json state,mergedAt,mergeCommit
```

When the state is `MERGED`, write the transitional prompt of step 6 of the `one-pr-one-session` skill at once. When a check fails, the PR stays open and the auto-merge stays on. Correct the cause, and start the loop again at step 1. A push outside the skip set moves the effective head, so the `review-gate` check fails until a new review approves the new head. A push of documents alone keeps the approval (D-943).

The owner merges PR #63 by hand, and the first auto-merge comes on the next PR (D-931). The session posts the summary of PR #63 before the hand-over (D-933).

## The merge settings

`docs/runbooks/branch-protection.json` records the settings of the repository and the protection of `main` (D-931). Compare the record with the live settings before a merge, and after each change of a setting:

```bash
repo=nkramber/the-thing-below
live=$(jq -n -S \
  --argjson repository "$(gh api "repos/$repo" --jq '{allow_auto_merge, allow_merge_commit, allow_rebase_merge, allow_squash_merge, delete_branch_on_merge}')" \
  --argjson main "$(gh api "repos/$repo/branches/main/protection" --jq '{allow_deletions: .allow_deletions.enabled, allow_force_pushes: .allow_force_pushes.enabled, enforce_admins: .enforce_admins.enabled, required_approving_review_count: .required_pull_request_reviews.required_approving_review_count, required_conversation_resolution: .required_conversation_resolution.enabled, required_linear_history: .required_linear_history.enabled, required_status_checks: {contexts: (.required_status_checks.contexts | sort), strict: .required_status_checks.strict}}')" \
  '{main: $main, repository: $repository}')
diff <(jq -S . docs/runbooks/branch-protection.json) <(printf '%s\n' "$live") && echo "protection: matches the record"
```

A setting of the repository is outward-facing. A session changes one only after the approval of the owner, in the PR that changes the record.

- `enforce_admins` is on, so no account skips a rule. The owner merge takes the same checks as the auto-merge.
- `required_conversation_resolution` covers each review thread of Gitar. A top-level comment of Gitar is no thread, and the author answers it under the `gitar-review` skill.
- A required context matches by name. Each context reports on a docs-only head and on a code head, because each gate job of `ci.yml` runs with `if: always()`. The `review-gate` workflow runs on each event of the PR.
- The `export` workflow runs on a change of its paths alone, so it is not a required context (D-512, D-692). A required context that never reports blocks each merge.

## The end of the Gitar pause

The pause of D-945 holds until the owner tells a session to end it in a PR. Each pause text of a rule file is one whole line, and that line holds the marker that the command below reads. Thus that PR removes the pause with these steps:

1. Make a branch from `main` for the PR.
2. Run the command below. It lists each line of the pause.
3. Delete each line that it lists.
4. Delete this section.
5. Add a decision row that ends the pause, and write `Superseded by` that row in the Effect column of D-945.
6. Run the command again. It must show no line.

```bash
git grep -n -E "Gitar pause \(D-945\)" -- . ":!docs/decisions.md" ":!docs/session-handoff.md" ":!docs/session-handoff-archive.md" ":!docs/reviews/"
```

The flag `--skip-gitar-review` stays, with each text of D-946 (D-946). The dated records and the register keep the pause as history. PR #58 and PR #60 did the same for the pause of D-895, which D-909 ended.
