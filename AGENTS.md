# The Thing Below: agent instructions

`CLAUDE.md` and `AGENTS.md` are identical (D-20). Edit both together.

## First action

Read the top entry of `docs/session-handoff.md` now, before any other file. It gives the state of the build, what is in flight, and the next concrete action. Read an older entry only when the top entry points to it, or when a gate needs the entries of the PR (D-584). Then read the rest of this file.

Before any PR work, review work included, load `.claude/skills/one-pr-one-session/SKILL.md`. One session works on one PR, and the PR holds all of its work (D-576, D-577).

## Read order

The start set is this file, the top handoff entry, and the skills of the task. Read the start set in full. The list below indexes every document. Read each other document with targeted reads by id or heading, and cite each id that the work touches. `docs/runbooks/session-context.md` gives the commands (D-583).

1. `docs/session-handoff.md`: the top entry, the state, and the next action.
2. This file: the tenets and the rules.
3. `docs/design.md`: the design, the guardrails (section 6), and the roadmap (section 7). `docs/archive/` holds the refuted plans.
4. `docs/decisions.md`: every owner decision, from the first id onward. Cite a D-# id when you apply one.
5. `docs/questions.md`: the open questions register, OQ-1 onward. File a new question there.
6. `docs/reviews/`: one review file per PR, plus audits and audit responses.
7. `docs/roadmaps/`: focused roadmaps, one per phase and one per area (D-144). Start at `docs/roadmaps/readme.md`, the index of the folder.
8. `docs/world/`: the world, one file per topic (D-123). Read it before you write lore, content, or player text.
9. `docs/runbooks/`: procedures for the machine and the repository.
10. `docs/session-handoff-archive.md`: sessions older than the 10 in the handoff. Read it only when the handoff points to it.

Skip `docs/samples/` during automatic exploration. Read it only when the owner or the handoff points to it (D-403).

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
- Save a script of more than 10 lines to a file, and run that file. Edit a section, not a whole file (D-591).
- One session is one harness invocation, bound to one PR (D-576). A session never starts a second PR. No PR exists only to record an earlier PR (D-578).

## Session handoff

At the end of a session, fetch the remote and read the highest session number with `grep -m1 '^## Session' docs/session-handoff.md`. Add one. Then add a new entry at the top (D-18). Keep the 10 newest entries in that file. Move any older entry to the top of `docs/session-handoff-archive.md`.

Commit the entry with the review record or the work it describes. Push, then fetch, and check that the status shows no `[ahead N]`. Another provider can add an entry above yours while you work. Add your own entry, and never append to an older one.

Commit the entry of a round before the push of that round. While the PR waits for gitar or the other provider, tell the owner that the session is ready for a context compaction (D-587). Each entry has six parts:

- What the session did, and why.
- The state of the build, with the remote head.
- What is in flight.
- Traps and gotchas.
- The questions that block progress.
- The next concrete action.

### The transitional prompt

After the hand-over point, the owner says `Merged PR #x`. The session then writes one transitional prompt for the next clean session, and it does no other work (D-601). The session writes the prompt for its own PR alone.

The prompt is one fenced block that the owner pastes into the next session. Step 6 of the `one-pr-one-session` skill holds the template and the fields.

## Text rules

- All project skills live in `.claude/skills/` (D-21): `ste-writing`, `design-doc-style`, `pr-review`, `gitar-review`, `one-pr-one-session`, `csharp-conventions`, and `game-text-style`. Create every new project skill there.
- Read each required skill from `.claude/skills/<skill-name>/SKILL.md`, even if it is absent from the skill list.
- Every `.md`, skill, and agent file follows ASD-STE100 (D-10). Load the `ste-writing` skill before you write.
- Load the `design-doc-style` skill before you edit `docs/design.md` or a focused roadmap.
- One term per concept. The `ste-writing` skill and its glossary reference file list the project terms (D-12, D-590).
- Game text has its own voice and does not follow STE (D-11). Load the `game-text-style` skill before you write any player string (D-63).
- Document file names in `docs/` are lowercase (D-20).

## Code rules

- C# only, tools included (D-99, D-101). No GDScript. One Python file is the exception: the out-of-date atlas script stays as a reference until PR-34 ports it (D-406).
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
- Never commit on `main` (D-25). `make hooks` installs the pre-commit hook that refuses one.
- Commit subjects use a conventional prefix: `feat`, `fix`, `docs`, `test`, `chore`.
- One concern per PR (G-8).
- Run `make where` before every commit and push.

## Automated review pass

An automated reviewer, gitar, comments on every PR after a push (D-14). After each push, the author loads the `gitar-review` skill and follows it. The rules below add to the skill, and a rule of this repo wins over it.

- The author answers every comment before the hand-over to the other provider, or before the session applies the `review-override` label (D-67).
- Wait for gitar with the one command of `docs/runbooks/session-context.md`, not a call for each poll (D-586).
- When the pass is complete, tell the owner that the PR is ready for the other provider, or apply the label below.
- A reply names no provider, harness, or model as the source of work (T-6).
- The reviewing provider reads the existing PR comments into its review and never addresses gitar. The `pr-review` skill holds the procedure of the reviewer.
- Every PR answers the pass, a documentation PR included (D-66). The `review-override` label exempts a documentation PR from the Codex review alone, and only when the PR changes no row of `docs/decisions.md` (D-401).
- The override set holds `docs/`, `README.md`, `CLAUDE.md`, `AGENTS.md`, `.claude/`, and `.github/pull_request_template.md` (D-16, D-71, D-239). Every other path takes the review, `.github/workflows/` and `content/` included (D-185, D-560).
- On a documentation PR that changes no decision row, the session applies the `review-override` label itself, only after the pass approves the head (D-67, D-401). A change to a decision row is a change to a line of a decision table (D-609). A later push needs a new approval before the label applies. A PR that adds or revises a decision goes to the other provider instead.
- Before you open a documentation PR, ask the owner every open question that the PR can settle (D-68). Ask in batches, and record the answers in the PR.

## Build and test commands

The solution and the project names follow D-217. Run each command from the checkout root.

- Every check, on this machine: `make verify`
- Branch, tree, and PR state: `make where`
- Hooks, once per checkout: `make hooks`
- Build: `make build`. Test: `make test`. Format check: `make format`.
- STE check: `make ste-check`. Determinism and string lint: `make lint`.
- Identity check: `make identity`. Content hash: `make content`.
- Godot build and the smoke session: `make smoke`.
- Coverage report: `dotnet test --solution TheThingBelow.slnx --no-build -- --coverlet --coverlet-output-format cobertura --results-directory artifacts/coverage`
- A Tools command with its options: `dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- atlas --root . --check`
- Review gate: the same form, with `review-gate --pull-request <file> --head-files <folder>`
- Play session: `/Applications/Godot_mono.app/Contents/MacOS/Godot --path TheThingBelow.Game`

Every option of the test application comes after `--` (D-592). The coverage command writes a Cobertura file, and the CI job makes a Markdown summary (D-593). The content-hash command takes `--write` after an intended change of a rule file (D-648). The atlas command writes each page and the atlas index, and `--check` compares the committed atlas (D-666). The `--sheets <folder>` option writes the review sheets, and no sheet enters git (D-514).

The name `Godot` is not on the command path of this machine. The play session needs the full path above, and the `smoke` target holds the same path. The STE check reads every live document and takes no file list (D-608). It also runs the reference check, the session number check, and the size rules (D-605, D-607, D-611). The `ste-writing` skill holds each rule and each exempt path. Run it in the commit command of `docs/runbooks/session-context.md`, and one time before the first push of a PR (D-585).

## PR gate

A PR merges only when every line holds:

- [ ] Tests written and green (T-3).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The build, test, and format job is green on every CI leg (D-2, D-117, D-481).
- [ ] The `smoke` job is green on every CI leg: the headless Godot session (D-117, D-481).
- [ ] The `det-lint` job is green: no float, clock, or OS random in `core`, and no inline player string (G-2, G-3, G-7).
- [ ] The `replay-identity` job is green: the same state hash and content hash on every CI leg (G-5, D-481, D-504, D-648).
- [ ] The `screen-test` job is green: each fixture screen matches the committed baseline (D-172, F-23). PR-41 creates it.
- [ ] The bot job is green on every CI leg: the bot runs end with no crash and no softlock (D-64, D-505). PR-15 creates it.
- [ ] The `night-gate` job is green: a success record from a night inside 48 hours (G-22). PR-49 creates it (D-496). A docs-only PR passes it (D-513).
- [ ] The `ste-check` job is green: the writing rules, the reference check, the session number check, and the size rules (G-12, D-605, D-607, D-611).
- [ ] The automated pass of gitar approved the head, or every comment of the pass has its answer (D-14). The review is current under the `gitar-review` skill.
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-17). The label of D-401 exempts a PR of the override set that changes no decision row.
- [ ] The `review-gate` check is green (D-15, D-500, F-37).
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Each check that does not exist yet has a line that names the PR that creates it (G-16).
- [ ] `docs/session-handoff.md` is current.
- [ ] The Documents section has a line for each row of the `one-pr-one-session` skill (D-581). No line defers a document or a record of the PR (D-577, D-579).
- [ ] No attribution anywhere (T-6).
