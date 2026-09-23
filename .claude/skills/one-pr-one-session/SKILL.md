---
name: one-pr-one-session
description: Bind a session to one repository, one branch, one PR, and one role, and make the PR the complete unit with its tests, documents, decisions, review records, and handoff. Load before PR work starts, when a PR opens or continues, before an answer to review findings, before a PR review, and before the final documents or handoff of a PR.
---

# One PR, one session

The owner set this rule to keep the context of each session small (D-576 to D-581). This skill holds the gates alone. `CLAUDE.md` holds the tenets, the PR gate, and the handoff rules. Load `ste-writing` before you write text.

## When to load

- Before work for a PR starts.
- When a session opens a PR or continues one.
- When a session answers review findings or gitar comments.
- When a session reviews a PR, with the `pr-review` skill.
- When a session writes the final documents or the handoff entry of a PR.

## 1. Bind the session

Write the binding in the first reply of the PR work and in the handoff entry:

- Repository: `the-thing-below`.
- Branch: `<prefix>/pr-<n>-<slug>` (D-8).
- PR: `#<n>`, the GitHub number, or the one PR intent before GitHub gives a number. A roadmap id such as PR-5 is a different number.
- Role: author, reviewer, or correction author.

A session can take many turns and many commits for its PR. The author session answers each gitar comment and each review of its PR, and a review answer needs no new session (D-582). A correction author continues the author role only when the author session no longer exists, and it comes from the provider of the author (T-4). The reviewer session repeats its review of the same PR after each correction. Each round adds a new handoff entry. A session never works on a second PR.

Commit the handoff entry of a round before the push of that round, and push one time. Do not push while the PR waits for gitar. While the PR waits for gitar or the other provider, tell the owner that the session is ready for a context compaction (D-587). Do the same when the context of the session passes 300k tokens. After the context compaction, read the top handoff entry again. A context compaction of this session keeps its binding.

Stop with this result, and do no other work, when one of the conditions below holds:

`Blocked: start a new clean session for this PR.`

- The conversation holds substantive work on another PR or another repository. Substantive work is a change, a commit, a push, a review record, or a PR comment. A file read alone is not.
- The conversation holds a PR that merged or closed. The transitional prompt of step 6, for the bound PR of the session, is the one exception (D-601).
- The request asks for a second PR or the next PR.
- The session is a fork, a subagent, a context compaction, or a summary of a session that worked on another PR.

After the hand-over point, a request for the next PR gets this result too. The transitional prompt of step 6 is not work on the next PR. Write that prompt for the bound PR of the session, and for the merge message of that PR alone. A merge message for another PR gets the stop result above.

A request for a second concern in the bound PR breaks G-8. Push back, and ask the owner (D-24). Never add the concern without an answer.

A clean session is a new top-level session that holds no work of another PR. The harness gives no session id that a check can read. Thus each session checks its own conversation, and the owner starts each clean session. Never invent a session id.

## 2. Start gate

Write no change until each line holds:

1. The binding of step 1 holds.
2. The session read the start set of `CLAUDE.md`: the top handoff entry and the skills of the task (D-583, D-584).
3. The PR has one concern (G-8).
4. `git fetch` ran, and the session wrote down the base commit.
5. A first line exists for each row of the table in step 3.

A reviewer skips line 5. The review checks the Documents section instead.

If a line does not hold, do not start. Ask the owner, or file the question (D-19).

## 3. Documents gate

The Documents section of the PR description has one line for each row (D-577):

| Document or category | Changes when the PR changes |
|---|---|
| `docs/design.md` | intent, a guardrail, a finding, a phase list, or the sequence position |
| `docs/decisions.md` | an owner answer, or a revision of a decision |
| `docs/questions.md` | an open question, or its answer |
| `docs/roadmaps/` | the scope, the exit tests, or the order of a PR |
| `docs/world/` | lore, a place, or a character |
| `docs/runbooks/` | a procedure of the machine or the repository |
| `docs/reviews/` | a review, or an answer to a review |
| `docs/session-handoff.md` | every session: each session adds an entry (D-18) |
| `CLAUDE.md` and `AGENTS.md` | a rule for sessions, in both files, which stay identical (D-20) |
| `.claude/skills/` and `.claude/agents/` | a procedure that a skill or an agent holds |
| `.github/pull_request_template.md` | a line of the PR gate |
| `README.md` | the description of the project |

Each line starts with the row name and a colon, as the PR template does. Then it has one of three forms (D-581):

- `Changed: <path>. <reason>`
- `No change needed because <reason that names the path>`, for a document that the PR can affect and does not.
- `Not applicable because <specific reason>`, for a category that the PR cannot reach.

The `docs/session-handoff.md` line is always `Changed`. Before the review, the author line for `docs/reviews/` takes the form `No change needed because`, and it names the review that the PR waits for. A `Changed:` line with no record in the diff fails RG 7. The reviewer corrects the line when the record lands (the `pr-review` skill).

Each line also reads true against the diff (D-577). A `Changed:` line names a row whose path the diff changes, and a changed path of a row takes a `Changed:` line. The `docs/reviews/` row is the exception to the second rule alone, because the reviewer adds the record after the author wrote the description. The `review-gate` command reads both rules under RG 7.

Correct the PR when a line or a record holds one of these:

- A deferral of a document or a record of this PR, such as "later PR", "after the merge", "TBD", or "a docs PR". The `review-gate` command reads the full set of phrases, and `DocumentRules.DeferralPhrases` holds it (D-579). The command reads `pull request` as `pr`, so the spelled-out form of a phrase fails too.
- A general claim, such as "no documentation impact", with no path or category.
- A handoff line in a form other than `Changed`.
- A handoff entry that describes work that the PR does not hold.
- A design, a decision, a roadmap, or a question that the diff contradicts.

A document that the diff makes wrong changes in this PR (D-577). A line that names the PR of independent roadmap work is not a deferral, for example the PR that creates an absent check (G-16, D-579).

## 4. Merge facts

A PR cannot hold its own squash commit or its merge time. Before the merge, the records give:

- the complete state of the PR and its effective head (the `pr-review` skill).
- the checks that ran, and their results.
- the review verdict, or the label of D-401.
- the words "waits for the auto-merge", or "waits for the owner merge" when the owner merges the PR (D-930, D-931).

Git and GitHub hold the merge commit and the merge time (D-578, D-930). The next PR reads its base from git, and its documents gate corrects any state text that the merge made old. A docs PR with its own concern, such as a critic pass, is a PR of its own (D-580).

No PR exists only to record the merge, the handoff, the review record, or the documents of an earlier PR. A new concern that edits a file of a merged PR is a PR of its own. Refuse such a request with this result, and start no PR:

`Refused: no PR records the merge or the documents of an earlier PR (D-578).`

A commit that changes only the metadata set never moves the effective head. That set holds the two review files of this PR and the two handoff files (D-610).

After an approval, a commit that changes paths of the skip set alone keeps the approval, and the PR needs no new review (D-943). That commit still gets its Gitar pass, and the author answers each comment and each claim of the pass (D-944). `docs/runbooks/merge.md` gives the steps.

## 5. Completion gate

The PR reaches its hand-over point only when each line holds:

1. The change and its tests are in the PR (T-3).
2. `docs/decisions.md` and `docs/questions.md` hold each answer and each question.
3. `docs/design.md` and the roadmaps agree with the PR.
4. The Documents section has a line for each row of step 3.
5. The handoff entry of this session is on the PR branch.
6. Each review record and each response file is on the PR branch.
7. The checks of `CLAUDE.md` pass, and each gitar comment has its answer.
8. No document, handoff entry, review record, or merge record of this PR waits for another PR. A line that names the PR of independent roadmap work holds this line.

A reviewer checks lines 1 to 4, 7, and 8 in the review, and it does not make them hold. The reviewer reaches the hand-over point when its review record gives `Ready for owner merge` for the effective head, or when the owner ends the review.

The author reaches the hand-over point when a review record gives `Ready for owner merge` for the effective head, or the label is on. While the PR waits for gitar or the other provider, the author session stays bound to the PR and answers each finding (D-582). A message that the PR is ready for the other provider is not the hand-over point.

The author loop reaches the hand-over point. `docs/runbooks/merge.md` holds its commands:

1. Push the round, with its handoff entry.
2. Get a complete Gitar pass with the `gitar-review` skill, and answer each comment (D-14).
3. Run `make codex-review PR=<n>` in the background, and read its outcome line (D-926).
4. On `changes-required`, answer each finding with the `pr-review` skill, then go to step 1.
5. On `three-strike-stop`, turn off the auto-merge, stop the loop, and ask the owner (D-929).
6. On `approve`, the hand-over point holds. The owner confirms the merge before the auto-merge (D-933, D-942).

At the hand-over point, the reviewer writes this result and stops:

`This session is bound to PR #N and is complete. End this session. Start a new clean session before beginning another PR.`

At the hand-over point, the author asks the owner to confirm the merge (D-933). The question block of `AskUserQuestion` holds a summary in four sections: What, How, CI, and Codex review (D-942). `docs/runbooks/merge.md` gives the content of each section. Then it turns on the auto-merge under `docs/runbooks/merge.md` (D-930), waits for the checks one time, and it reads the state of the PR. When the PR merged, the author writes the transitional prompt of step 6 at once. When the owner merges the PR by hand, the author writes the result above and waits for `Merged PR #x` (D-931).

Do not offer to start the next PR. After the merge, write the transitional prompt of step 6.

## 6. The transitional prompt

After the hand-over point, the PR merges. The session then writes one transitional prompt, and it does no other work (D-601). After the auto-merge, the session writes the prompt as soon as it reads the merge (D-930). After an owner merge, the owner says `Merged PR #x`, and the session writes the prompt then. Write the prompt for the PR of the session alone. A merge message for another PR gets the blocked result of step 1.

Get the merge commit from git first:

```
git fetch origin && git log --oneline -1 origin/main
```

Read `docs/roadmaps/readme.md` and the phase file, and name the next PR. The pick is provisional, and the owner can name a different PR. Read `docs/questions.md`, and name each open question of that PR. The next session gets an answer for each one before it writes a change (D-19).

The prompt is one fenced block, and the owner pastes it into the next clean session:

```
Start PR-<n>: <the one concern>

PR #<x> merged to `main` as <sha>. Read the top handoff entry first.
Repository: the-thing-below. Branch: `<prefix>/pr-<n>-<slug>`. Base: `<sha>`. Role: author.
Load the `one-pr-one-session` skill and the skills of the task before any change.
Open questions for this PR: <each OQ-# with its subject, or `none`>.
First action: <the first concrete action>.
```

The session ends with this prompt. It makes no branch and no change for the next PR (D-576).

## Enforcement

| Rule | Enforced by |
|---|---|
| The handoff changes, each row has a line, each line reads true against the diff, and no line defers a document or a record of the PR | Machine: the `review-gate` check (D-15, D-579) |
| The review record and the effective head | Machine: the `review-gate` check (D-15). The record lives in the metadata set, so the gate reads the record and never its author, and the reviewer session reads the record on the effective head before the owner merges (D-610). RG 5 also accepts an earlier approved head when each later commit changes the skip set alone (D-943) |
| `CLAUDE.md` and `AGENTS.md` stay identical | Machine: rule AGENTS 1 of the ste-check job on every PR, and a test of Tests (D-20, D-857) |
| The binding, the start gate, and the completion gate | Agent |
| One PR in each session, and a clean session for each PR | Owner. No check can see the conversation |
| The merge | Machine: the protection of `main` and the auto-merge that the author turns on (D-930, D-931). The owner can merge too (D-8) |
| The Gitar pass before the review, and the three-strike stop | Machine: the `codex-review` command refuses a run and gives exit code 3 (D-926, D-929) |
