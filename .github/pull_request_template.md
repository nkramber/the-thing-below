## Summary

<!-- What the PR changes, and why. One concern per PR (G-8). -->
<!-- The session binding: the branch, the PR, and the role (D-576). -->

## PR gate

Each line holds before the merge: the gated auto-merge or the owner merge (`CLAUDE.md`, PR gate, D-930).

- [ ] Tests written and green (T-3).
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The build, test, and format job is green on every CI leg (D-2, D-117, D-481).
- [ ] The `smoke` job is green on every CI leg: the headless Godot session (D-117, D-481).
- [ ] The `det-lint` job is green (G-2, G-3, G-7).
- [ ] The `replay-identity` job is green on every CI leg (G-5, D-481).
- [ ] The `screen-test` job is green (D-172, F-23, D-731).
- [ ] A screen change: the author read each frame of `make sheet` or `make walk` that the change reaches. The PR records the result (D-784). A PR that changes no screen says so.
- [ ] The bot job is green on every CI leg (D-64, D-505). PR-15 creates it.
- [ ] The `night-gate` job is green (G-22). PR-49 creates it.
- [ ] The `ste-check` job is green: the writing, reference, session number, size, and Documents row rules (G-12, D-605, D-607, D-611, D-696).
- [ ] The automated pass of gitar approved the head, or each Gitar item of the pass has its answer (D-14, D-66, D-964). The review is current under the `gitar-review` skill.
- [ ] The other provider reviewed it through `make codex-review`, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-17). A commit of the skip set alone after it keeps the verdict (D-943). A PR in the override set that changes no decision row is exempt when the `review-override` label is on (D-16, D-401, D-560). A change to `.github/workflows/` or to `.claude/settings.json` is never exempt (D-700).
- [ ] The `review-gate` check is green (D-15, D-500, F-37).
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Every check that does not exist yet has a line above with the PR that creates it (G-16).
- [ ] `docs/session-handoff.md` is current, and the handoff entry of each session is on the PR branch.
- [ ] No document, handoff entry, review record, or merge record of this PR waits for a later PR (D-577, D-578). A line that names the PR of independent roadmap work is valid (G-16).
- [ ] No attribution in code, game text, a commit, a PR description, or a GitHub comment (T-6, D-703). No commit subject or body names an agent, harness, or model as the source of the work (D-22).

## Documents

One line per row, in one of three forms (D-581): `Changed: <path>. <reason>`, `No change needed because <reason that names the path>`, or `Not applicable because <reason>`. The `one-pr-one-session` skill holds the rows.

- `docs/design.md`:
- `docs/decisions.md`:
- `docs/questions.md`:
- `docs/roadmaps/`:
- `docs/world/`:
- `docs/runbooks/`:
- `docs/reviews/`:
- `docs/session-handoff.md`:
- `CLAUDE.md` and `AGENTS.md`:
- `.claude/skills/` and `.claude/agents/`:
- `.github/pull_request_template.md`:
- `README.md`:
