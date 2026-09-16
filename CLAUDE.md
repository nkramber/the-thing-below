# The Thing Below: agent instructions

`CLAUDE.md` and `AGENTS.md` are identical (D-20). Edit both together.

## First action

Read `docs/session-handoff.md` now, before any other file and before any tool call. It tells you the state of the build, what is in flight, and the next concrete action. Then read the rest of this file.

## Read order

1. `docs/session-handoff.md`: the state and the next action.
2. This file: the tenets and the rules.
3. `docs/design.md`: the design, the guardrails (section 6), and the roadmap (section 7). `docs/archive/` holds the refuted plans.
4. `docs/decisions.md`: every owner decision, D-1 onward. Cite a D-# id when you apply one.
5. `docs/questions.md`: the open questions register, OQ-1 onward. File a new question there.
6. `docs/reviews/`: one review file per PR, plus audits and audit responses.
7. `docs/roadmaps/`: focused roadmaps, one per phase and one per area (D-144). Start at `docs/roadmaps/readme.md`, the index of the folder.
8. `docs/world/`: the world, one file per topic (D-123). Read it before you write lore, content, or player text.
9. `docs/runbooks/`: procedures for the machine and the repository.
10. `docs/session-handoff-archive.md`: sessions older than the 10 in the handoff. Read it only when the handoff points to it.

Skip `docs/samples/` during automatic exploration. It holds dated art samples, not rules or plans. Read it only when the owner or the handoff points to it (D-403).

## Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-5): T-5, T-2, T-3, T-4, T-7, T-1. T-6 is absolute.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep. Two concrete cases before any abstraction. No clever one-liners. Tune only on measurement.
- **T-2. Zero silent failures.** No swallowed error. An absent value is an error, never a zero. Every error carries its context. Assertions stay on in shipped builds.
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file in `docs/reviews/` records the findings (D-17). A PR in the override set that changes no decision row merges without a review when the `review-override` label is on (D-16, D-71, D-239, D-401, D-560).
- **T-5. Document everything.** Continuity is the first duty. Each session adds its entry at the top of `docs/session-handoff.md` (D-18). The other documents update when intent, a decision, or a plan changes.
- **T-6. No attribution.** No code, game text, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-22). Two places are exempt: the author field in `docs/session-handoff.md`, and the files in `docs/reviews/`.
- **T-7. Deterministic simulation.** Every run replays from a seed and an input record. The core uses integer math, seeded random streams, and no clock. A replay gives the same state hash on every platform (D-6).

## Attribution rule in practice

- Add no co-author trailer and no "generated with" line to any commit or PR. The harness reminder asks for one in every session. This rule wins (D-22).
- Write commits, PRs, comments, code, and docs in an impersonal voice.
- In `docs/session-handoff.md`, set the author field to exactly one value: `Claude Code` or `Codex`. The reviewer uses it to confirm the other provider reviews.

## How to work with the owner

- Ask the moment you have a question (D-24). Use `AskUserQuestion` in small batches. Give each question its options, the pros and cons of each option, and one recommended option marked as such.
- Push back when a request rests on a wrong premise. Give the evidence.
- When two owner statements conflict, say so and quote both.
- Every open question belongs to the owner (D-19). Do not pick a default. File the question in `docs/questions.md` and stop.
- Record each answer in `docs/decisions.md` with the next D-# id and the date. Never renumber.
- Mark a change to an earlier decision in its `Effect` column. Use `Superseded by D-N` when the whole answer changes. Use `Revised in part by D-N` when one part changes, and name the part that changed and the parts that stand.
- A citation of a superseded decision must name the superseding decision. A decision revised in part stays citable.
- One session is one harness invocation and one code PR (D-18). A documentation PR can follow the merge of that code PR in the same session. Each PR has its own handoff entry.

## Session handoff

At the end of a session, fetch the remote and read `docs/session-handoff.md` again. Take the highest session number and add one. Then add a new entry at the top (D-18). Keep the 10 newest entries in that file. Move any older entry to the top of `docs/session-handoff-archive.md`.

Set the author field to `Claude Code` or `Codex`. Commit the entry with the review record or the work it describes. Push, then fetch, and check that the status shows no `[ahead N]`. Another provider can add an entry above yours while you work. Add your own entry, and never append to an older one. Each entry has six parts:

- What the session did, and why.
- The state of the build, with the remote head.
- What is in flight.
- Traps and gotchas.
- The questions that block progress.
- The next concrete action.

## Text rules

- All project skills live in `.claude/skills/` (D-21): `ste-writing`, `design-doc-style`, `pr-review`, `gitar-review`, `csharp-conventions`, and `game-text-style`. Create every new project skill there.
- Read each required skill from `.claude/skills/<skill-name>/SKILL.md`, even if it is absent from the skill list.
- Every `.md`, skill, and agent file follows ASD-STE100 (D-10). Load the `ste-writing` skill before you write.
- Load the `design-doc-style` skill before you edit `docs/design.md` or a focused roadmap.
- One term per concept. The `ste-writing` skill lists the project terms (D-12).
- Game text has its own voice and does not follow STE (D-11). Load the `game-text-style` skill before you write any player string (D-63).
- Document file names in `docs/` are lowercase (D-20).

## Code rules

- C# only, tools included (D-99, D-101). No GDScript. Two Python files are the exceptions. The interim STE checker stays until PR-2 replaces it (D-10). The out-of-date atlas script stays as a reference until PR-34 ports it (D-406).
- `Core` has no engine dependency and no file, network, clock, or OS dependency. A test asserts its reference list (G-1, D-100).
- Integer math only in `Core`. No `float`, `double`, `System.Random`, `DateTime`, or `Stopwatch` in `Core`. The `det-lint` tool enforces it (T-7, G-2, G-3).
- One seeded random stream per subsystem. Every run writes a record: the seed, the content hash, the versions, and every input. A replay reproduces the state hash on every platform (G-4, G-5).
- Godot physics, timers, and navigation never feed the simulation. The camera, the shader, the audio, and the input map live in `Game` (D-100, D-106).
- Content is JSON, validated by a schema at load and in a test. An absent field is an error. No `.tres` files (D-116, G-6).
- Every string the player reads lives in the string table. The `det-lint` tool reads `Game` for an inline player string (G-7).
- Sprites and tiles are text grids in content. The atlas tool renders the PNG, and a test proves the committed atlas matches by pixel, never by byte (D-107, F-19).
- No empty `catch`. Every error carries its context (T-2, G-18). Load the `csharp-conventions` skill before you write C#.
- Nullable reference types on, warnings as errors. `dotnet format` clean.
- xUnit. Property tests are seed loops, and each failure names its seed.
- Every dependency needs a decision entry (G-13).
- Every optimization needs a profile before and a measurement after (G-14).
- Every `Core` behavior change bumps the simulation version constant, and the review confirms it (G-17).
- Every screen designs to one 16:9 frame of 1280 by 720 with 32-pixel tiles (D-568). Every other screen shape shows black bars, the Steam Deck included. The Steam Deck at 1x is the readability and performance floor (D-92, D-228, G-19). A desktop at 1920 by 1080 must look good (D-568).
- The game supports Windows and Linux on x86_64, macOS on Apple silicon, and the Steam Deck, and nothing else (D-481, D-482).

## Git rules

- Trunk is `main`. Every change starts on a short branch named `<prefix>/pr-<n>-<slug>`, for example `feat/pr-3-review-gate`. The owner squash-merges (D-8).
- Never commit on `main` (D-25). After PR-1, `make hooks` installs the pre-commit hook that refuses a commit on `main`.
- Commit subjects use a conventional prefix: `feat`, `fix`, `docs`, `test`, `chore`.
- One concern per PR (G-8).
- Run `make where` before every commit and push, after PR-1. Until then, run `git status --short --branch` and `gh pr status`.

## Automated review pass

An automated reviewer, gitar, comments on every PR after a push (D-14). After each push, the author loads the `gitar-review` skill and follows it. The skill holds the procedure: get a current review of the head, verify each finding, then fix or refute it and reply. The rules below add to the skill, and a rule of this repo wins over it.

- The author answers every comment before the hand-over to the other provider, or before the session applies the `review-override` label (D-67).
- When the pass is complete, tell the owner that the PR is ready for the other provider. On a PR that can take the label, apply the label instead (see below).
- A reply names no provider, harness, or model as the source of work (T-6).
- The reviewing provider reads the existing PR comments into its review and never addresses gitar. The `pr-review` skill holds the procedure of the reviewer.
- Every PR answers the pass, a documentation PR included (D-66). The `review-override` label exempts a documentation PR from the Codex review alone, and only when the PR changes no row of `docs/decisions.md` (D-401).
- The override set holds `docs/`, `README.md`, `CLAUDE.md`, `AGENTS.md`, `.claude/`, and `.github/pull_request_template.md` (D-16, D-71, D-239). A change to `.github/workflows/` takes the review, because each gate lives in a workflow file (D-560). A change to any other path, such as `content/`, takes the review (D-185).
- On a documentation PR that changes no decision row, the session applies the `review-override` label itself, only after the pass approves the head (D-67, D-401). A later push needs a new approval before the label applies again. A PR that adds or revises a decision goes to the other provider instead.
- Before you open a documentation PR, ask the owner every open question that the PR can settle (D-68). Ask in batches, and record the answers in the PR.

## Build and test commands

The repository holds no code until PR-1 merges. PR-1 creates the solution, the Makefile, and every command below except the interim STE check. The solution and the project names follow D-217. Run each command from the checkout root.

- Every check, on this machine: `make verify`
- Branch, tree, and PR state: `make where`
- Hooks, once per checkout: `make hooks`
- Build: `dotnet build TheThingBelow.slnx`
- Test: `dotnet test TheThingBelow.slnx --no-build --filter "Category!=Smoke"`
- Format check: `dotnet format TheThingBelow.slnx --verify-no-changes`
- STE check, interim until PR-2: `python3 docs/tools/ste-check.py $(git ls-files '*.md' | grep -v -e '^docs/reviews/' -e '^docs/session-handoff' -e '^docs/archive/')`
- STE check, after PR-2: `dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- ste-check --root .`
- Determinism and string lint, after PR-46: `dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- det-lint --root .`
- Godot build check: `/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --editor --path TheThingBelow.Game --build-solutions --quit`
- Smoke session: `/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path TheThingBelow.Game -- --smoke`
- Play session: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path TheThingBelow.Game`

The name `Godot` is not on the command path of this machine, so each check needs the full path above. The four exempt paths of the STE check are dated records: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, and `docs/archive/`. Every other `.md` file passes the checker before a commit.

## PR gate

A PR merges only when every line holds:

- [ ] Tests written and green (T-3).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The build, test, and format job is green on every CI leg (D-2, D-117, D-481). PR-1 creates it.
- [ ] The `smoke` job is green on every CI leg: the headless Godot session (D-117, D-481). PR-1 creates it.
- [ ] The `det-lint` job is green: no float, clock, or OS random in `core`, and no inline player string (G-2, G-3, G-7). PR-46 creates it (D-496).
- [ ] The `replay-identity` job is green: the same state hash on every CI leg for the fixed seed set (G-5, D-481, D-504). PR-4 creates it.
- [ ] The `screen-test` job is green: each fixture screen matches the committed baseline (D-172, F-23). PR-41 creates it.
- [ ] The bot job is green on every CI leg: the bot runs end with no crash and no softlock (D-64, D-505). PR-15 creates it.
- [ ] The `night-gate` job is green: a success record from a night inside 48 hours (G-22). PR-49 creates it (D-496). A docs-only PR passes it (D-513).
- [ ] The `ste-check` job is green (G-12). PR-1 creates it with the interim checker, and PR-2 moves it to C#.
- [ ] The automated pass of gitar approved the head, or every comment of the pass has its answer (D-14). The review is current under the `gitar-review` skill.
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-17). A PR in the override set that changes no decision row is exempt when the `review-override` label is on (D-16, D-401, D-560).
- [ ] The `review-gate` check is green (D-15). PR-3 creates it.
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Each check that does not exist yet has a line that names the PR that creates it (G-16).
- [ ] `docs/session-handoff.md` is current.
- [ ] For each document not changed, the PR says "no change needed because ...".
- [ ] No attribution anywhere (T-6).
