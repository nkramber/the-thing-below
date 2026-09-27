# The night and the night gate

Status: active runbook. Written in ASD-STE100. Decisions: D-507, D-509, D-510, D-513, D-1188 to D-1192, D-1201 to D-1204.

This runbook gives the commands of the night job and of the night gate. `docs/roadmaps/area-ci.md` sections 7.14 and 7.15 hold the design, and G-22 holds the rule.

## What runs

- The `night` workflow starts at 04:17 UTC each day on the latest commit of `main` (D-1189). Each leg plays both policies from one first seed and uploads its night record (D-509, D-1190, D-1191).
- The `night-gate` workflow runs on each push of a PR, from the workflow file of `main` (F-37). It passes a docs-only PR and a PR with a green night on its head. It passes each other PR after a green night on `main` inside 48 hours (G-22, D-510, D-513, D-1188).
- A night on an earlier commit of a PR passes the head when each later commit changes documents alone (D-1204). A review record and a handoff entry thus keep the night.
- The `night-promote` workflow runs on each push to `main`. It keeps the green night of the merged PR as the newest evidence of `main` over a failed night (D-1202, D-1203).
- A failed leg sends one Pushover message with each failed leg, the first seed, and the link (D-1201).
- The result of the last push stands until the merge (D-1188). No workflow runs the gate again after a night.

## Start a night by hand

A night by hand needs the workflow file on `main`, so it works after the merge of PR-49 (F-37).

```bash
# A night on `main`, with the range of its run number (D-1190)
gh workflow run night.yml --ref main

# A night on the branch of a PR, with the range of the failed night (D-510)
gh workflow run night.yml --ref <branch> -f first-seed=<first seed of the failed night>

# The newest night runs, and the watch of one run
gh run list --workflow night.yml --limit 5
gh run watch <run id> --exit-status
```

The first seed of a failed night is on the page of its run, and in each of its night records.

## Run the gate again

Run the gate again after a night by hand on a PR, or after a promotion. Run it again after a failed night that a later night corrected, too.

```bash
link=$(gh pr checks <number> --json name,link --jq '.[] | select(.name == "night-gate") | .link')
run=$(printf '%s\n' "$link" | sed -E 's#.*/actions/runs/([0-9]+).*#\1#')
gh run rerun "$run"
```

## Replay a failed run

The night uploads the record of each failed run in the artifact `night-failed-runs-<leg>`. The `bots` command replays one record (T-7):

```bash
gh run download <run id> --name night-failed-runs-<leg> --dir /tmp/night-failed
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- bots --root . --replay /tmp/night-failed/<policy>-<seed>.record
```

## The first night and the required check

D-1192 gives these steps to the author session of PR-49, after its merge and before the transitional prompt:

1. Start a night on `main` with the command above.
2. Watch the run until it ends. Stop and tell the owner when it fails.
3. Add the check to the protection of `main`.
4. Compare the live setting with `docs/runbooks/branch-protection.json` with the command of `docs/runbooks/merge.md`.

```bash
gh api -X POST repos/nkramber/the-thing-below/branches/main/protection/required_status_checks/contexts \
  -f 'contexts[]=night-gate'
```

## When the schedule stops

GitHub disables the schedule of a public repository after 60 days with no activity (F-41). The gate then fails each PR as stale.

```bash
gh workflow enable night.yml
gh workflow run night.yml --ref main
```

## The count of runs

The counts of D-1191 live in `.github/workflows/night.yml`, with the run that measured them. Measure again when the bot job of a PR shows a slower rate than that run. Keep each policy inside 30 minutes on the slowest leg (G-14).

## The promotion

The night of a fix plays the range of the failed night with the `first-seed` input, as the command above shows. At the merge of that PR, the `night-promote` workflow checks four conditions (D-1202, D-1203):

- The newest evidence of `main` is a failed night.
- The branch night succeeded on each leg, and it started inside 48 hours.
- The branch night played the same first seed and the same count of runs of each policy as the failed night.
- The tree of the merge commit differs from the tree of the night commit in paths of a docs-only PR alone.

Each push to `main` gets its own run, and each run waits for each earlier run to end. The checks thus follow the order of the pushes, and no push loses its run. The job summary of the run names each condition that did not hold. Read the newest promotion with these commands:

```bash
gh run list --workflow night-promote.yml --limit 5
gh api 'repos/nkramber/the-thing-below/actions/artifacts?name=night-promotion&per_page=5' \
  --jq '.artifacts[] | {id, run: .workflow_run.id, commit: .workflow_run.head_sha, created_at}'
```

After a promotion, the gate of each open PR keeps the result of its last push (D-1188). Run the gate of each open PR again with the command of the section above.

## The alert and the notify workflow

The alert job of the night workflow reads the repository secrets `PUSHOVER_USER_KEY` and `PUSHOVER_API_TOKEN` (D-1201). No file, log, or record holds their values. A failed send fails the alert job with the HTTP status and the errors of Pushover.

Send a test message through the `notify` workflow:

```bash
gh workflow run notify.yml --ref main -f title="The Thing Below: test" -f message="A test of the notify workflow."
gh run list --workflow notify.yml --limit 1
```
