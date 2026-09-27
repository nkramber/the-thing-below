# The night and the night gate

Status: active runbook. Written in ASD-STE100. Decisions: D-507, D-509, D-510, D-513, D-1188 to D-1192.

This runbook gives the commands of the night job and of the night gate. `docs/roadmaps/area-ci.md` sections 7.14 and 7.15 hold the design, and G-22 holds the rule.

## What runs

- The `night` workflow starts at 04:17 UTC each day on the latest commit of `main` (D-1189). Each leg plays both policies from one first seed and uploads its night record (D-509, D-1190, D-1191).
- The `night-gate` workflow runs on each push of a PR, from the workflow file of `main` (F-37). It passes a docs-only PR and a PR with a green night on its head. It passes each other PR after a green night on `main` inside 48 hours (G-22, D-510, D-513, D-1188).
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

Run the gate again after a night by hand on the head of a PR, or after a failed night that a later night corrected.

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
