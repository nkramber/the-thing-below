# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

## Session 64: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-83, the decision row of the docs-only skip set and the transitional prompt rule.
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19, which is PR-83 of the roadmap. Role: author. Base: `ee4305a`.

### What this session did, and why

- PR #18 merged as `ee4305a`. Its `changed-paths` job holds a skip set, and no decision recorded the paths. The reasoning was a comment of `.github/workflows/ci.yml` alone, and no file in `docs/` named it.
- D-600 records the skip set: `docs/`, `.claude/`, `README.md`, `LICENSE`, and `.github/pull_request_template.md`. `CLAUDE.md` and `AGENTS.md` stay out of it, because `AgentFileTests` reads both files and fails when they differ (D-20). A skipped job reports `Success`, so a skip of the agent files would pass the PR shape that breaks D-20 most often.
- The skip set and the override set of D-16 are not the same set. The override set holds both agent files, and `LICENSE` is in the skip set alone.
- The comment of the `changed-paths` job and a bullet of `docs/roadmaps/area-ci.md` now cite D-600.
- D-601 records the transitional prompt. After the owner says `Merged PR #x` for the PR of the session, the session writes one fenced block for the next clean session, and then it ends. `CLAUDE.md` and `AGENTS.md` hold the rule, and step 6 of the `one-pr-one-session` skill holds the template.
- The owner approved the two concerns of this PR before the work started. G-8 refuses a second concern without that answer.

### State of the build

- `main` is `ee4305a`. The branch `docs/pr-83-skip-set-decision` sits on that base.
- `make verify` passes on the Mac of the owner: the build, 8 tests, the format check, the STE check, and the smoke session.
- The PR adds two decision rows and changes `.github/workflows/`, so the `review-override` label does not apply (D-401, D-560). The PR needs the Codex review.
- The change to `CLAUDE.md` and `AGENTS.md` keeps the two files identical, and it takes both files out of the skip set of D-600. The build and test job runs on this PR.

### In flight

The push of the branch, then the PR, then the Gitar pass, then the Codex review (T-4, D-17).

### Traps and gotchas

- The session verified each claim of D-600 against the code: the `case` pattern of the job, the paths that `AgentFileTests` reads, and the run time of the three legs on PR #18, which was 37 to 79 seconds. The `ste-check` job holds no skip condition, so a docs PR still gets the STE check.
- `CLAUDE.md` is 16823 bytes, and the size check of OQ-182 proposes a limit of 16 KB. The file passed that limit on `main` at `ee4305a`, before this PR. OQ-182 has no answer, and no check exists.
- The uncommitted work of the merged branch `feat/pr-1-scaffold` is in a git stash of this machine. The patch of the owner replaced it. Drop the stash after the merge.
- A skipped job reports `Success`. A path rule that is too wide passes a PR that ran no check.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 65.

### Open questions that block progress

None for PR-83. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3. OQ-182 blocks nothing. The owner has not run the Deck test, so PR-82 waits and the renderer stays provisional (D-599).

### Next concrete action

Push the branch, open the PR with the Documents section, then get a Gitar review of the head.

## Session 63: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #18 at effective head `d8c31b8`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the start set and the repeat-review, review-record, commit, and STE instructions.
- Verified that Claude Code authored PR #18. Codex remains the eligible reviewer under T-4 and D-17.
- Verified that the current effective head is `d8c31b8`; the later commits only change metadata.
- Read the P2-4 answer with the regression table. Each says the old target runs without end, and a frame limit alone would leave a false pass.
- Checked that the Makefile and CI set a frame limit and require the success line.
- The latest Gitar dashboard approves `d8c31b8`. All nine CI checks pass on the current branch tip.
- The saved GraphQL query reports zero unresolved threads. A fresh export failed to connect.
- The owner confirms that zero inline threads remain unresolved.
- Set P2-4 to fixed in `d8c31b8`. The verdict is `Ready for owner merge` for that head.
- The handoff held ten entries. Session 53 moved to the archive (D-18).

### State of the build

- `main` is `9f27f12`. The PR branch tip before this review commit is `47a7e1639104b69d726c993c4404e4bcbbe552fd`.
- The effective head is `d8c31b8`. The response, design row, and roadmap row now state the same two-fault behavior.
- All nine checks pass on the branch tip: changed paths, STE check, build/test/format on three platforms, coverage, and smoke on three platforms.
- The Gitar dashboard approves the effective head with one closed finding and no open issues.
- No uncommitted source or project changes exist. The unrelated untracked `deck-test/` directory remains unchanged.

### In flight

The review record and this entry need one metadata commit and push. The verdict applies to effective head `d8c31b8`.

### Traps and gotchas

- A frame limit and a success-line check are both needed for the smoke contract (F-64).
- The API did not return inline threads in this session. The response file records the last successful query and result.
- Do not add unrelated paths to the review commit. Keep `deck-test/` untouched.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 64.

### Open questions that block progress

None for PR #18. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Commit this review record and handoff entry, push them, then verify the remote head and status.

## Session 62: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat Codex review of PR #18, in the session that authored it (D-582).
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: #18. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read `docs/reviews/pr-18.md` at head `6e0622a`. The repeat review withdrew P2-1, set P2-2 and P2-3 to fixed, and opened P2-4. The verdict is `Blocked`.
- P2-4 has full merit. The response file said that the `smoke` target of `0c402dd` reported success for a broken session, and its table said that the case runs without end. Both cannot hold.
- The table holds. With no frame limit the session waits until a kill, so the target reports no result.
- The sentence conflated the two faults of the old target. The target has no frame limit, and it reads no log. A frame limit alone turns the first fault into the second, which is a false pass. The correction needs both parts.
- `docs/reviews/pr-18-response.md` now names the two faults, agrees with its table, and holds a P2-4 section.
- The same wrong sentence was in the F-64 row of `docs/design.md`, which this PR wrote. That row and the F-64 row of `phase-1-foundations.md` now name the two faults.
- The review could not enumerate the inline threads, because its API calls failed. The response file now holds the exact query and its result, which is 0 unresolved threads.

### State of the build

- `main` is `9f27f12`. The effective head stays `6e0622a`, and the Gitar pass approves it.
- This round changes `docs/reviews/pr-18-response.md`, `docs/design.md`, `docs/roadmaps/phase-1-foundations.md`, and this file.
- `docs/design.md` and `docs/roadmaps/` are outside the metadata set, so this commit moves the effective head and needs a new Gitar review.
- `make verify` passes on the Mac of the owner. The nine CI checks passed on `6e0622a` and on `d8c31b8`.
- The new effective head is `d8c31b8`. The Gitar pass of that head gives `Approved`, with 1 comment, 1 with merit, 0 open issues, and 0 unresolved threads. Commit `0c402dd` answered that comment.
- That review is current. The head matches, and the dashboard edit time of 03:04:49Z is later than the push time of 03:00:57Z and later than the reply of 03:04:30Z.

### In flight

The repeat Codex review of effective head `d8c31b8` (T-4, D-17). The Gitar pass of that head is complete and approves it.

The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- A response file can hold a prose claim and a table that disagree. Read them together before the commit, and give one result for one command and one trigger (T-5).
- Two faults in one command can compose. Name each one, and say which fault a partial correction leaves.
- A wrong claim in a response file can also sit in the design register. Grep for the sentence, and not for the id.
- The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. A round that also changes `docs/design.md` moves the effective head.
- Gitar replaced its dashboard comment in each round of this PR. The id moved from `5706916715` to `5707572860`, and then to `5707837154`. Read the newest id in each check.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 63.

### Open questions that block progress

None for PR #18. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 back to Codex for the repeat review of `d8c31b8`. The session stays bound to PR #18 and answers each finding (D-582).

## Session 61: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #18 at effective head `6e0622a`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the current handoff, the PR response, and the repeat-review, review-record, contract, Gitar, and commit skills.
- Verified the provider gate. Session 60 names Claude Code as the author, and Codex remains the opposite provider (T-4, D-17).
- Verified that `a56a7ce` changes the handoff alone, so the effective head is `6e0622a`.
- Reproduced the F-60 build callback failure. Godot logged the error and returned 1, so P2-1's exact trigger is withdrawn.
- Verified P2-2. The Documents line now names both actual review files in the `Changed:` form.
- Verified the corrected smoke path. `make smoke` passed on a healthy tree and failed when a wrapper removed the managed assembly during the real Godot session.
- Found P2-4 in the response file: its prose says the old target reported success, but its table says that case ran without end.
- CI run 35175182671 passed all nine checks on tip `a56a7ce`.
- GitHub API calls for inline review threads failed. The current Gitar dashboard summary says the pass approved the correction head, with one closed finding and no open issue.
- Updated `docs/reviews/pr-18.md`. The current verdict remains `Blocked` for head `6e0622a`.
- The handoff held ten entries before this one, so Session 51 moves to the archive (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- The effective head is `6e0622a`. The current remote tip is `a56a7ce`, a metadata commit.
- CI run 35175182671 passed all nine checks on the remote tip.
- The healthy smoke run passed. The missing-assembly regression failed on the absent success line, as required.
- The inline review-thread export remains incomplete.

### In flight

P2-1 is withdrawn, P2-2 and P2-3 are fixed, and P2-4 remains open. The review cannot reach its hand-over point until the response text is corrected and the remaining review evidence is complete.

### Traps and gotchas

- The editor returns 1 on the F-60 build callback error. The original evidence read the code of `tail` through a pipe.
- A session with no loadable boot assembly waits without end. The new frame limit ends the session, and the missing success line fails the smoke check (F-64).
- `docs/reviews/pr-18-response.md` line 28 conflicts with the regression table at line 47.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 62.

### Open questions that block progress

No owner question blocks the review. P2-4 and the inline comment export remain unresolved.

### Next concrete action

Correct the conflicting statement in the response file. Then repeat the review of PR #18 at its new effective head.

## Session 60: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the Codex review of PR #18, in the same session that authored it (D-582).
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: #18. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read `docs/reviews/pr-18.md`. The verdict was `Blocked` for head `0c402dd`, with P2-1 and P2-2 open.
- P2-1 has partial merit. Its trigger does not reproduce, and the defect that it aims at is real.
- The editor gives an exit code of 1 when its build callback fails, and not 0. The first measurement of this PR read `$?` after a pipe to `tail`, so it read the exit code of `tail`. F-60 carried that wrong claim, and the row now marks that part refuted and keeps it.
- The verification found the real failure. A headless session whose managed assembly does not load never reaches `Quit`, and it runs without end. With `--quit-after` it ends with an exit code of 0 and no success line. This is F-64.
- The `smoke` target of the Makefile now writes each log to a file, fails on a nonzero build code, runs the session with `--quit-after 600`, and fails when the success line is absent.
- The `smoke` job of CI runs the session with `--quit-after 600` too, so a broken session fails in seconds and not at the time limit of 30 minutes.
- P2-2 has full merit. The `docs/reviews/` line of the Documents section matched none of the three forms of D-581, and it named a placeholder path. The PR description now names `docs/reviews/pr-18.md` and `docs/reviews/pr-18-response.md` in the `Changed` form.
- `docs/reviews/pr-18-response.md` records each disposition, the evidence, and the regression checks.

### State of the build

- `main` is `9f27f12`. The branch holds the scaffold, the two corrections, and the review records.
- `make verify` passes on the Mac of the owner: the build, the 8 tests, `dotnet format`, the STE check with 0 findings, and the smoke session.
- The regression checks pass. `make smoke` gives 2 on a failed Godot build, gives 2 in about 6.5 seconds on a boot class that the scene cannot instantiate, and gives 0 on a healthy tree.
- The Gitar review of `0c402dd` gave `Approved`, with 1 finding closed and 0 unresolved threads.
- The Gitar pass of the correction head `6e0622a` gives `Approved`, with 1 comment, 1 with merit, and 0 open issues. Commit `0c402dd` answered that comment, and the thread is resolved.
- That review is current. The head matches, the dashboard edit time of 02:33:02Z is later than the push time of 02:25:14Z and later than the `On it` reply of 02:29:01Z.
- CI run on `6e0622a` passed each of the nine checks, the three smoke legs with `--quit-after` included.

### In flight

The repeat Codex review of PR #18 at effective head `6e0622a` (T-4, D-17). The Gitar pass of that head is complete and approves it.

The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- A measurement of an exit code through a pipe reads the exit code of the last command of the pipe. Redirect to a file, or set `pipefail`, before you record an exit code as evidence.
- A headless Godot session that cannot instantiate its boot script waits without end. Always give `--quit-after` to a session that a check runs (F-64).
- An exit code of 0 from a smoke session proves nothing. The success line in the log is the proof (T-2).
- A finding can name a real defect through a trigger that does not reproduce. Reproduce the trigger, then look for the defect that the finding aims at.
- Gitar replaced its dashboard comment during this round. The id moved from `5706916715` to `5707572860`. Read the newest id in each check, and never a saved one.
- A string comparison with `\>` inside `[ ]` fails in zsh. Use `sort`, or read the times in Python.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 61.

### Open questions that block progress

None for PR #18. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 back to Codex for the repeat review of `6e0622a`. The session stays bound to PR #18 and answers each finding (D-582).

## Session 59: 2026-09-16, Codex

Author: Codex
Session: follow-up verification for the PR #18 review at effective head `0c402dd`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Verified that review commit `a9a3d07` is the remote branch tip and does not change the effective head.
- Verified that CI run 35171555340 passed all nine checks on the pushed review commit.
- Read the current Gitar PR comment through `gh pr view`. Its macOS finding is fixed in `0c402dd`, and the smoke job passes on macOS.
- Tried the required export of issue comments, review bodies, and inline threads. GitHub API access failed for the thread data.
- Updated `docs/reviews/pr-18.md` with the final CI results, the verified push, and the Gitar claim.
- The handoff held ten entries before this one, so Session 49 moves to the archive (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- The effective head is `0c402dd`. The review metadata commit `a9a3d07` is the remote tip.
- CI run 35171555340 passed all nine checks on the remote tip.
- The required inline thread export remains unavailable. The local `make verify` command did not complete.

### In flight

PR #18 has two open P2 findings. The review verdict is Blocked until the findings and required comment evidence are resolved.

### Traps and gotchas

- `gh pr view` returned the issue comment and review summaries, but the API calls for inline review threads failed.
- The workspace has an untracked `deck-test/` directory. It remains unchanged.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 60.

### Open questions that block progress

No owner question blocks the review. GitHub API access blocks the remaining thread evidence.

### Next concrete action

Export all inline threads when GitHub API access works, verify each claim and reply, and then update the review record.

## Session 58: 2026-09-16, Codex

Author: Codex
Session: review of PR #18 at effective head `0c402dd`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the top handoff entry, the one-PR skill, the PR review skill, the contract reference, the review-record reference, and the STE skill.
- Verified the provider gate. Session 57 names Claude Code as the author, and Codex is the eligible reviewer (T-4, D-17).
- Verified that `b32a971` changes the handoff alone, so the effective head remains `0c402dd`.
- Inspected all 34 paths of the effective diff and checked the PR description and current CI run.
- Found P2-1: the local smoke target can accept the Godot build callback failure that F-60 documents.
- Found P2-2: the PR description's review Documents line does not use the form of D-581.
- The required comment export failed twice because GitHub's API did not connect. Local `make verify` stalled in `dotnet build` and was interrupted.
- Wrote `docs/reviews/pr-18.md` with the verdict `Blocked` for effective head `0c402dd`.
- Moved Session 48 to the archive because this entry makes it the eleventh handoff entry (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- CI run 35169864293 on tip `b32a971` passed all nine checks. Its implementation head is `0c402dd`.
- The local build produced no output for 60 seconds. The review interrupted it, so local tests, format, STE, and smoke did not run.
- The comment export did not complete. The existing PR comments remain unverified.

### In flight

PR #18 needs both findings corrected. The review also needs the full comment export before it can give a final verdict.

### Traps and gotchas

- The Godot editor can return 0 when its build callback fails. The CI workflow reads the log, but the Makefile does not (F-60, T-2).
- The PR description line for `docs/reviews/` uses a placeholder path and omits the required `Changed:` form (D-577, D-581).
- The working tree held an untracked `deck-test/` directory before the review. It remains unchanged.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 59.

### Open questions that block progress

No owner question blocks the review. GitHub API access blocks the required comment evidence.

### Next concrete action

Export every comment on PR #18 when GitHub API access works. Verify the claims, then complete the review of the same head.

## Session 57: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold, which is the first code of the repository.
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: PR-1. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read the top handoff entry, `CLAUDE.md`, and the `one-pr-one-session`, `ste-writing`, `design-doc-style`, and `csharp-conventions` skills.
- Read `deck-test/handover.md` and `deck-test/readme.md` on `spike/deck-test`. That step has no PR, so no document held its eight owner answers.
- Wrote D-592 to D-599 for those answers, and resolved OQ-75, OQ-76, OQ-77, OQ-78, OQ-83, OQ-92, and OQ-93.
- D-599 revises D-160 in part. The renderer pick comes after PR-1, and PR-1 sets Forward+ as a provisional renderer. PR-82 sets the picked renderer in one line.
- Added the cost model rows of the Deck test to section 4 of `docs/design.md`, with M-7 for the frame time of the test scene.
- Added the PR-82 entry to `phase-1-foundations.md`, section 7.3, and renumbered the later entries of section 7.
- Built the scaffold: `TheThingBelow.slnx`, `global.json`, `Directory.Build.props`, the Makefile, the pre-commit hook, the four projects of D-217, and the CI workflow.
- Wrote eight tests: the agent-file match test, three Core reference tests, and four tests of the tools command line.
- Opened PR #18. The first CI run failed two smoke legs, and a second commit corrected the workflow (F-62, F-63).

### State of the build

- `main` is `9f27f12` (PR #17). The branch `feat/pr-1-scaffold` holds the scaffold.
- `make verify` passes on the Mac of the owner: the build, the 8 tests, `dotnet format`, the STE check with 0 findings, and the Godot smoke session.
- The smoke session prints the renderer as `forward_plus` and the frame as 1280 by 720, and it ends with no error.
- The CI workflow holds five jobs: `changed-paths`, `build-test-format`, `coverage`, `smoke`, and `ste-check`.
- The first run of PR #18, 35169279617 on `0b319f8`, failed `smoke (macos-26)` and `smoke (windows-2025)`. Each other check passed.
- The second run, 35169532179 on `0c402dd`, passed each of the nine checks, the two corrected legs included.
- The Gitar review of `0c402dd` gives `Approved`, with 1 finding closed and no open issue. That review is current: the head matches, and the dashboard edit time of 01:14:25Z is later than the push time of 01:10:42Z.

### In flight

The Codex review of PR #18 (T-4, D-17). The PR adds decision rows, so the `review-override` label does not apply (D-401). It also changes `.github/workflows/`, which is never exempt (D-560).

The effective head is `0c402dd`. The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- The Godot editor writes `net8.0` into a `.csproj` that holds no target framework, over `Directory.Build.props` (F-60). The Game project pins `net10.0` in its own file, and `.gitignore` holds `*.csproj.old`.
- The Godot editor build gives an exit code of 0 when its build callback fails. The smoke job reads the log for `build callback failed` (F-60, T-2).
- A coverage run instruments the Core copy in the test output folder and adds `System.Threading` to it (F-61). The reference test reads the file that the Core project built.
- The compiler writes no metadata entry for a project reference that no code uses. A second test reads the Core project file, so an added reference fails (F-61).
- The generated entry point of `xunit.v3` runs the console runner unless `UseMicrosoftTestingPlatformRunner` is on. The console runner reads no Coverlet option.
- Coverlet 10 takes `--coverlet`, and not `--coverage`. It writes its file to the results directory, and it takes no output path.
- The macOS archive of Godot holds `Godot_mono.app`, and not `Godot.app` (F-62). The find pattern of the smoke job reads `*.app/Contents/MacOS/Godot`.
- The git-bash of the Windows runner carries `sha512sum` and no `shasum` (F-63). The checksum step reads the digest itself and names both values.
- Gitar found the macOS fault of F-62 by reading the workflow, and CI found the same fault by running it. Gitar found no fault in the Windows checksum step, which only the Windows runner showed.
- Gitar paused automatic reviews for the trial period, and a review still ran on each push of this PR. Read the dashboard comment, and never the pause note alone.
- The owner ran no Deck test yet. PR-82 waits for that run.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 58.

### Open questions that block progress

None for PR-1. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 to Codex for the review of T-4. The session stays bound to PR #18 and answers each finding (D-582).

## Session 56: 2026-09-16, Claude Code

Author: Claude Code
Session: the Deck test of step 7.1 of `docs/roadmaps/phase-1-foundations.md`. That step has no PR and no review.
Repository: the-thing-below. Branch: `spike/deck-test`. PR: none. Role: author of a spike.

### What this session did, and why

- Built the throwaway test scene of D-160 and D-523 on the branch `spike/deck-test`, which never merges (D-597).
- The scene draws the load of D-160 at the frame of 1280 by 720: normal maps, point lights with shadows, glow, the four ambient kinds, fog, the CRT pass, and a wipe transition.
- The sweep runs 20 stages. Each stage holds 60 warm-up frames and 300 measured frames.
- `scripts/FrameMeter.cs` reads the time of each frame and counts each frame over 16.667 milliseconds (D-598).
- Ran the fetch script for the export templates, and the SHA-512 matched (D-596).
- The owner answered eight questions. `deck-test/handover.md` holds each one, and PR-1 records them as D-592 to D-599.

### State of the build

- The scene, the shaders, the meter, and the report all work. A run on the Mac proved them.
- The native Linux export `build/DeckTest.x86_64` exists. No machine ran it.
- `main` held no code during this session.

### In flight

The run on the Deck, which the owner does. That run answers D-160 and gives the first effect budget of D-523.

### Traps and gotchas

- The Godot export needs a solution file beside `project.godot`. With none, the export writes an ELF file, exits 0, and packs no managed assembly.
- An exit code of 0 hides an export fault. The export gives `completed with warnings` and exits 0. PR-54 must read the log.
- macOS caps the frame rate whatever the vsync setting says. A Mac run reports 16.67 milliseconds in every stage, and the report refuses to give a budget (T-2).
- Only the Deck run answers D-160. A Mac run tests the scene and the report, and nothing else.
- Godot drops each light past 15 on one canvas item with no message (F-46). The sweep stops the light row at 15.

### Open questions that block progress

None. OQ-92 and OQ-93 closed with D-597 and D-598.

### Next concrete action

The owner copies `build/DeckTest.x86_64` and `run-deck-test.sh` to the Deck and runs `./run-deck-test.sh`. PR-82 then sets the renderer.

## Session 55: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #16 at effective head `e3e6611`, after the owner confirmed Gitar approval.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the current handoff and the repeat-review, review-record, commit, and STE instructions.
- Verified that the local branch and fetched remote branch both point to `e3e6611`. This commit changes the runbook, so it is the effective head.
- Verified the provider gate. Claude Code authored the PR, and Codex reviews it (T-4, D-17).
- Reproduced P2-2 in a scratch repository under `set -e`. The corrected command committed review and handoff records with no eligible STE file.
- Set P2-2 to fixed in `e3e6611`. The full interim STE check, diff check, and identity check pass.
- The owner confirmed Gitar approval of the current changes. GitHub API access failed, so the dashboard comment and check query could not be read independently.
- Updated `docs/reviews/pr-16.md`, preserved the earlier verdicts, and set the current verdict to `Ready for owner merge` for `e3e6611`.

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`. The review and handoff commit `86b76b6` was pushed and verified as the remote head. The effective head remains `e3e6611`.
- The current verdict is `Ready for owner merge`, based on the fixed findings and the owner's Gitar confirmation.

### In flight

The review record and this entry are committed and pushed to PR #16. This entry records the verified push.

### Traps and gotchas

- GitHub API access failed during this session. The owner confirmed the current Gitar approval.
- The remote fetch succeeded. The PR page and comment API did not respond.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 56.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push this metadata update, fetch the remote, and verify the clean branch status and PR head.
