# The night watcher

Status: active runbook. Written in ASD-STE100. Decisions: D-1205 to D-1208.

The night watcher is a launchd job on the Mac mini of the owner. It reads the newest night of `main` at minute 0 and minute 30 of each hour. For a failed night, it starts one session of Claude Code in a new worktree. That session follows the `night-fix` skill, and it sends a Pushover when the PR is ready to merge. The owner merges, and the watcher never merges (D-933).

## What the Mac needs

- Claude Code at `~/.local/bin/claude`, version 2.1.283 or later, signed in.
- `gh` signed in, `git`, `dotnet`, `make`, and the Codex CLI of `make codex-review`, each on the PATH of the shell.
- The system sleep off, and a desktop session of the owner. The launchd agents start in a desktop session alone.
- The checkout of the repository on its disk, because the worktrees use its git data.

## Install the job

Run the target from the checkout, in a shell with the full PATH. The job keeps that PATH.

```bash
make night-watch-install
launchctl print gui/$(id -u)/com.thethingbelow.night-watch | head -20
```

The target publishes Tools into `~/.the-thing-below/night-watch/bin`, and it writes `~/Library/LaunchAgents/com.thethingbelow.night-watch.plist`. It fails when a program of the loop is not on the PATH. Run the target again after a change of the watcher reaches `main`.

## The folder of the watcher

The folder `~/.the-thing-below/night-watch` holds these files:

| Path | Content |
|---|---|
| `night-watch.log` | One line for each check, with the UTC time and the reason |
| `handled/<run id>` | The mark of a night that got a session, with the session id |
| `worktrees/night-<run id>` | The worktree of the session of that night |
| `logs/night-<run id>.log` | The output of the session |
| `launchd-out.log`, `launchd-error.log` | The output of the job |

## One check by hand

```bash
make night-watch
tail -5 ~/.the-thing-below/night-watch/night-watch.log
```

## Resume a stopped session

A session stops at three events alone: a third red branch night, a three-strike stop of Codex, or an owner-only question (D-1205, D-1206). The Pushover gives the session id. Resume the session in its worktree:

```bash
cd ~/.the-thing-below/night-watch/worktrees/night-<run id>
claude --resume <session id>
```

## Start a night again

A mark stops a second session for the same night. To start a new session for that night, remove its mark, then run one check:

```bash
rm ~/.the-thing-below/night-watch/handled/<run id>
make night-watch
```

## Remove the job

```bash
launchctl bootout gui/$(id -u)/com.thethingbelow.night-watch
rm ~/Library/LaunchAgents/com.thethingbelow.night-watch.plist
```

After a merge of a fix, remove its worktree with `git worktree remove <path>` in the checkout.
