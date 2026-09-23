# Session context

Status: active runbook. Written in ASD-STE100. Decisions: D-583 to D-591.

Each call of a harness sends the whole context of the session again. Thus a large context costs tokens on every later call, and a long session multiplies that cost. This runbook keeps the context small and keeps every gate.

## The evidence

An audit on 2026-09-16 read the usage records of 10 Claude Code sessions and 10 Codex sessions of this repository.

- The Claude Code sessions sent a median of 350k tokens of context in each call, and a maximum of 886k.
- Whole reads of the documents of the read order held about 29% of the context that the sessions carried.
- A call that ran the STE checker alone cost 19% of the input tokens. A call that polled GitHub alone cost 14%.
- `CLAUDE.md` cost about 1.5% of the tokens. Handoff entries and review records cost about 1%. These stay as they are.

## Targeted reads

A targeted read gets one row, one section, or one PR block by its id or its heading. Put all the ids of one question in one command. Limit the output with `cut` and `head`. Do not read the same section two times in a session.

```bash
# The top handoff entry, and the highest session number
awk '/^## Session /{n++} n==1' docs/session-handoff.md
grep -m1 '^## Session' docs/session-handoff.md

# Each handoff entry of one PR, for the provider gate (the branch slug of the PR)
awk -v b='pr-14-' '/^## Session /{if(e~b)printf "%s",e; e=""} /^## Session /||e!=""{e=e $0 "\n"} END{if(e~b)printf "%s",e}' docs/session-handoff.md

# Decision rows by id. The Effect column of a row names each later revision.
grep -n -E '^\| D-(576|582) \|' docs/decisions.md

# Questions, guardrails, lessons, and findings by id
grep -n -E '\*\*OQ-(179|181)\.' docs/questions.md
grep -n -E '\*\*(G-8|G-16|L-15)\.' docs/design.md
grep -n -E '^\| F-(12|58) \|' docs/design.md

# A design section by its number
awk '/^## 6\./{p=1} /^## 7\./{p=0} p' docs/design.md

# The roadmap file of a PR, then its block
grep -rn -E '^#+ .*PR-5:' docs/roadmaps/ | head -5
awk '/^### 7\.12 PR-5:/{p=1;print;next} /^#{2,3} /{p=0} p' docs/roadmaps/phase-1-foundations.md

# A topic, with a limit on the output
grep -n -i -E 'handoff|session' docs/decisions.md | cut -c1-200 | head -20
```

A row that says "Superseded by D-N" or "Revised in part by D-N" needs a targeted read of D-N too. Cite each id that the work touches, in the PR and in the handoff entry.

## A critic pass

A critic pass reads whole files. Run it in the `design-critic` agent. The agent reads the files in its own context and returns the findings alone. Then the session reads the evidence of each finding with a targeted read.

## Checks in the commit command

Run the STE checker in the same command as `git diff --check` and the commit (D-585). The checker reads every live document that git tracks, and it takes no file list (D-608, D-702). A failed check prints the last 20 lines, and the commit does not run. The command works in bash and in zsh, with `set -e` too.

```bash
git add <paths>
{ out=$(dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj \
  -- ste-check --root . 2>&1) || { printf '%s\n' "$out" | tail -20; false; }; } \
  && git diff --cached --check && git commit -q -m "<subject>" && git log --oneline -1
```

The checker reads each tracked file from the working tree, so it can name a fault that this commit does not stage. Correct that file too, or put it back. An untracked file takes no rule (D-702). Run the same command one time before the first push of a PR.

## The Gitar wait

Wait for Gitar with one command (D-586). The command stops when a Gitar comment has an edit time after the recorded time, or when five minutes pass. It reads each Gitar comment, and the reply to a request is a Gitar comment. Then run command B of the `gitar-review` skill one time. When command B shows the "On it" reply and no new dashboard edit, run the wait again with `since` set to the reply time.

In Claude Code, run the command in the background, and the harness calls the session again when the command ends. In Codex, run it as one command.

```bash
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
n=<number>
since=<the push time or the request time from command A, in UTC>
end=$(( $(date +%s) + 300 )); found=""
while [ -z "$found" ] && [ "$(date +%s)" -lt "$end" ]; do
  last=$(gh api --paginate "repos/$repo/issues/$n/comments" \
    --jq '.[] | select(.user.login == "gitar-bot[bot]") | .updated_at' | sort | tail -1)
  if [ -n "$last" ] && [[ "$last" > "$since" ]]; then found=$last; else sleep 20; fi
done
echo "gitar comment after $since: ${found:-none in 300 s}"
```

## Evidence for a review

Save the PR comments to one file with one command, then read that file (D-589). When any GitHub call fails, the command fails and leaves no comments file (T-2). Read the diff in stages, and list each path of the stat in the review record.

```bash
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
n=<number>
dir="${TMPDIR:-/tmp}/the-thing-below-pr-$n"; mkdir -p "$dir"; rm -f "$dir/comments.md" "$dir/part.md"
gh pr view "$n" --json title,body,headRefOid,baseRefOid > "$dir/part.md" \
  && gh api --paginate "repos/$repo/issues/$n/comments" \
    --jq '.[] | "## \(.user.login) \(.created_at) \(.updated_at)\n\(.body)\n"' >> "$dir/part.md" \
  && gh api --paginate "repos/$repo/pulls/$n/reviews" \
    --jq '.[] | "## review \(.user.login) \(.state) \(.submitted_at)\n\(.body)\n"' >> "$dir/part.md" \
  && gh api --paginate "repos/$repo/pulls/$n/comments" \
    --jq '.[] | "## \(.user.login) \(.path):\(.line) \(.created_at)\n\(.body)\n"' >> "$dir/part.md" \
  && mv "$dir/part.md" "$dir/comments.md" && echo "saved: $dir/comments.md" \
  || { rm -f "$dir/part.md"; echo "comment export failed, no comments file" >&2; false; }

git diff --stat <merge base>...<head>
git diff <merge base>...<head> -- <path>
git diff <reviewed head>..<head> -- <path>
```

## Scripts, edits, and output

- Save a script of more than 10 lines to a file one time, and run the file again (D-591).
- Edit the section that changes. Write a whole file only when the file is new.
- Save a large output to a file, and read the lines that the work needs.

## Measures

The audit of 2026-09-16 set these values. An audit reads the usage records of the harness. No tool of this repository reads them (D-99).

| Measure | Value on 2026-09-16 | Target |
|---|---|---|
| Context at call 10, outside a critic pass | 312k and 353k in two docs sessions | 160k or less |
| STE checker calls | 155 calls in 10 sessions | One call for each commit |
| GitHub poll calls for each Gitar round | 107 calls in 10 sessions | 3 or less |
| Files that a session reads in full at the start | about 20.7k tokens | about 11k tokens |

### The M-1 numbers

M-1 reads the local record of each harness. No tool of this repository reads them (D-99), so an audit
writes a script of its own, runs it, and keeps it outside the checkout.

Claude Code writes one file for each session in `~/.claude/projects/<the checkout path>/`:

- Each line is one JSON record, and a record of the type `assistant` holds `message.usage`.
- That object holds `input_tokens`, `cache_creation_input_tokens`, `cache_read_input_tokens`, and `output_tokens`.
- A record holds `gitBranch`, and the branch of the last record before an assistant record gives the PR.
- One file can hold more than one branch, so read the branch for each record and not for each file.

Codex writes one file for each session under `~/.codex/sessions/` and `~/.codex/archived_sessions/`:

- A record of the type `token_count` holds `info.total_token_usage`, and the last one gives the session total.
- The field `cached_input_tokens` sits inside `input_tokens`, so `total_tokens` matches the M-1 number of D-672.
- The first prompt of the session gives the PR. It holds the binding block, or a line such as `Review PR #30`.
- Read `cwd` from the first record, and skip a session of another checkout.

### The M-2 numbers

M-2 reads GitHub. The `gh` command gives both parts:

```
gh run list --branch <branch> --workflow ci.yml --limit 40 --json databaseId,conclusion,createdAt,updatedAt
gh api repos/:owner/:repo/actions/runs/<id>/jobs --jq '[.jobs[] | {name, conclusion, started_at, completed_at}]'
```

- The table of section 4 reads the newest green `ci` run of each PR, and not the sum of every run.
- The wall time of a job is `completed_at` less `started_at`. The clock of the run is `updatedAt` less `createdAt`.
- The runner name in the job name gives the leg. A job with no runner name is a shared job.
