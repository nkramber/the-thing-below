---
name: pr-review
description: Review a pull request at principal-engineer depth, or answer a review as the author. Require the opposite provider, precise evidence, regression checks, and a revision-specific verdict. A finding is a claim, not a fact, and the author can refute one with evidence. Use for PR reviews, repeat reviews after fixes, and any request to address, answer, or fix review findings or review feedback.
---

# PR review skill

Review the change as the engineer accountable for its effect on the whole system.
Judge correctness, contracts, failure recovery, test quality, and future maintenance.
Apply this standard to code, content, tools, CI, skills, and document PRs.
A green test suite or a persuasive PR description does not establish correctness.

## Reference files

This file holds the rules of every review. Each reference file holds the rules of one case (D-588). Load a reference file before the work of its case.

| Reference file | Load it when |
|---|---|
| `references/project-contracts.md` | The PR changes code, content, tools, or CI, or the text of a contract in one of its areas. |
| `references/repeat-review.md` | The session reviews a PR again after a correction. |
| `references/review-record.md` | The session writes or edits a review record, corrects the PR description, or reads the head and the verdict for the `review-gate` check. |
| `references/answer-review.md` | The session is the author, and it answers gitar or a review. |
| `references/commit-and-end.md` | The session commits a review record or a response file, and before the session ends. |

## Mandatory provider gate

**The reviewer MUST NOT come from the provider that wrote the PR.**
This requirement applies before the substantive review starts and before any approval (T-4, D-17).

| Provider that wrote the PR | Required reviewer |
|---|---|
| Claude Code, Anthropic | Codex, OpenAI |
| Codex, OpenAI | Claude Code, Anthropic |

A different model, account, session, or subagent from the same provider does not qualify.
A prompt that assigns the other provider's name does not change the actual provider.
Author self-checks and automated tests do not satisfy this gate.

Substantive changes alter code, data, configuration, requirements, or executable instructions.
Review findings and test reports alone do not make the reviewer a PR author.

1. Identify the actual reviewer provider from the active environment.
2. Identify every provider that contributed substantive changes or fixes to this PR.
3. Verify authorship from the owner's statement, each handoff entry of this PR, and the review records.
   `docs/runbooks/session-context.md` prints each handoff entry of one PR with one command (D-584).
4. Match each source to this PR and its revision.
5. Record the providers, source, and eligibility result in the review file.

The newest handoff entry can describe a review rather than authorship. A Git account alone does not identify the provider.
Do not infer authorship from prose style, commit email, or a branch name.

**Stop with `Blocked` if the providers match, authorship is unknown, or the evidence conflicts.**
State the fact or the eligible reviewer that the review needs. Ask the owner to supply that fact or start the opposite-provider session.
Do not perform a substitute review with another model from the same provider.

If both providers wrote substantive changes in the PR, neither qualifies for the whole PR.
Record the conflict and request an owner decision about how to separate the changes.
Do not approve through reciprocal review of selected hunks.

## Establish the review scope

- Read the start set of `AGENTS.md`. Read each other document with a targeted read by id or heading (D-583).
- Load `.claude/skills/one-pr-one-session/SKILL.md` first. A review session works on one PR alone (D-576).
- Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-10).
- Load `.claude/skills/csharp-conventions/SKILL.md` before any C# review (D-21, D-99).
- Read the PR request, its acceptance criteria, prior review, and applicable focused roadmap.
- Resolve decision revisions through the `Effect` column in `docs/decisions.md`. `Superseded by D-N` replaces the whole answer. `Revised in part by D-N` changes only the named part, and the rest of that decision stays current.
- Check `docs/questions.md` for unresolved choices that affect this change (D-19).
- Save every existing comment on the PR to one file with one command, then read that file (D-589). The comments are the automated pass of gitar and the author's replies (D-14). Take each one into the review as a claim to verify, and never as a finding of your own. See "Do not address the automated reviewer".
- Record the PR number, target branch, base commit, merge base, and head commit.
- Verify that the local checkout and diff represent those commits.
- Preserve unrelated local edits. Use an isolated checkout when necessary.
- Inspect the complete diff in stages (D-589). Run `git diff --stat` of the merge base and the head first.
- Then read the diff of each listed path: deleted files, renamed files, configuration, content, schemas, and tests.
- List each path in the review record as inspected, or name it as uninspected.
- Read each changed file in context. Follow affected callers, consumers, and persistence paths beyond the diff.
- Continue through the scope after the first finding. Record any area that remains uninspected.

The PR description states intent. The diff and verified behavior establish what the PR does.
Label an uncommitted patch review as provisional. It cannot satisfy a review gate for an unidentified PR revision.
If the base or head changes, assess the new diff and affected evidence before a final verdict.

## Stay inside the pull request

A review judges the change in front of it. It does not design the next one.

Read the roadmap entry for this PR and its exit tests before the first finding. Those two texts set the boundary. This section limits the reach of a review. It never lowers the standard for the code that the PR changes.

A concern is in scope when one of these holds:

- The changed code gives a wrong result under a supported condition.
- The change breaks a caller, a saved file, or a build that exists today.
- A stated exit test of this PR does not hold.
- A guardrail that the PR names does not hold for the code that this PR adds.

A concern belongs to a later PR when one of these holds:

- It asks a tool that this PR creates to cover a surface that no exit test names.
- It asks for behavior that the roadmap gives to a later PR.
- It repeats a class of defect that this PR corrected, in a surface that this PR does not touch.
- It needs an owner decision about scope, and not a correction.

Write the second kind under `## Out of scope` in the review record. Name the PR or the roadmap item that holds it. Give it no severity. A line in that section never blocks the merge.

A PR that creates a check must pass that check (G-16). A new check does not cover the whole platform on the first day. A gap in a new tool is a defect of this PR only when a stated exit test names the missing case.

## Principal-engineer review standard

Build an independent account of the behavior before comparison with the author's explanation.
For each changed behavior, trace the input, state transition, output, side effects, and recovery path.
State the invariant that each boundary must preserve.

### Correctness and system effects

- Check normal use, boundary values, absent data, invalid data, repeated actions, and interrupted actions where applicable.
- Trace state ownership and lifetime across Core, Storage, Game, Tools, Tests, and the debug assembly.
- Inspect initialization, cancellation, cleanup, restart, and replay when the change affects those paths.
- Check event order, resource disposal, integer bounds, and overflow where they affect the result.
- Inspect compatibility with current callers, content, saves, and records.
- Check whether a local fix creates a defect in another consumer of the same contract.
- Verify each acceptance criterion against implementation and evidence.

Do not expand the review into an unrelated rewrite.
Distinguish defects introduced by the PR, defects it exposes, and independent pre-existing defects.
A pre-existing defect blocks this PR only when it prevents the changed behavior or a required gate.

### Project contracts

The project contracts cover these areas: the Core boundary, determinism, replay, errors, content, strings, input and CI boundaries, gameplay, presentation, and dependencies and cost. Load `references/project-contracts.md` when the PR changes code, content, tools, or CI, or the text of a contract in one of these areas.

### Design, maintainability, and documents

- Confirm one concern per PR and a clear reason for every changed subsystem (G-8).
- Check helper depth against T-1: one level deep.
- Require two concrete uses before an abstraction (T-1).
- Prefer explicit ownership and visible control flow over hidden coupling.
- Explain the concrete maintenance cost of a design objection.
- Do not report personal style preferences as correctness defects.
- Check that design text, decisions, questions, code, and acceptance criteria agree.
- Check each roadmap prerequisite against the first gate that needs it.
- Distinguish proposed work, implemented work, measured behavior, and owner approval.
- Verify material external claims against dated primary sources.
- Check the Documents section against the documents gate of the `one-pr-one-session` skill. A deferral to a later PR is a finding (D-577, D-578).
- Confirm `AGENTS.md` and `CLAUDE.md` remain identical when either changes (D-20).
- Check attribution restrictions in commits, PR text, comments, and deliverables (D-22).

Documentation and skill PRs require the same provider independence and evidence discipline as code PRs.
For a skill change, examine its trigger, scope, instructions, references, and behavior on a realistic request.
Treat contradictory instructions and gates that cannot pass as defects.

## Verification

Run the focused checks that can falsify the changed behavior. Complete the applicable project gates.
Use the current build commands in `AGENTS.md`. Do not invent a successful command when no solution or tool exists.

- Read the tests as critically as the implementation.
- Verify that each bug fix has a regression test that fails on the old behavior (T-3).
- Use an isolated comparison when execution of the regression test against the base is practical.
- Otherwise, explain the causal reason the old behavior fails the assertion and state the execution limit.
- Check test assertions against the contract, not a copy of the implementation.
- Inspect seed coverage, state diversity, boundary cases, and failure context.
- Check test discovery, skipped tests, mocks, fixtures, and assertions that can pass without the intended behavior.
- Distinguish a passed check from a skipped, unavailable, failed, or author-reported check.
- Record the command, revision, environment, result, and relevant artifact for each required check.
- Verify CI results against the reviewed revision and configured test target.
- Check the `replay-identity` result and the smoke result on every CI leg (G-5, D-481). Check the bot runs and the night result once PR-15 and PR-49 create them (G-22, D-505).

Use the initial-check clause only as G-16 permits.
Name the absent check and the PR that creates it. A PR that creates a check must pass it.
The clause does not excuse a failed existing check.

Do not repeat broad suites without a new change, failure, or unresolved risk.
Do not weaken a test or threshold to obtain a pass.
Absent required evidence blocks approval. Optional evidence gaps belong in the limitations.
The section "A review with no network" gives the one exception (D-602).

## A review with no network

A review session can have no GitHub access, or a read-only `.git` directory. That session can still give a verdict (D-602). Each condition below must hold:

- The local branch holds the effective head of the review, and `git log` shows it.
- The review reads the whole diff from the merge base to the effective head.
- Each local check runs: the build, the tests, the format check, and the STE check. The record names each check that the machine refuses.
- The record marks each piece of evidence that comes from the author. The CI result and the comment export of the response file are such evidence.
- The record lists each item that the session cannot verify under `## Open questions and accepted risks`.

With each condition, the verdict can be `Ready for owner merge`. The record says which evidence is live, and which evidence comes from the author. Give `Blocked` when the local branch does not hold the effective head, or when the diff and the records of the PR disagree.

A session with no network cannot commit or push. The author of the PR then commits the review record and the handoff entry of that session, with no change to their text.

## Precise findings

Investigate each suspected defect before it becomes a finding.
Search for a caller guarantee, validation layer, existing test, or later decision that can refute the concern.
Use a reproduction, failed assertion, or complete causal trace as evidence.
Separate a verified defect from an unresolved question or an optional suggestion.

Each finding contains:

- A stable local id, severity, and short title that states the defect.
- The reviewed commit and the smallest useful file and line range.
- The input or state that triggers the defect.
- Expected behavior, with the relevant contract or D-# id.
- Actual behavior and its consequence for the player, data, build, or maintainer.
- Evidence, with the seed, command, trace, or artifact when applicable.
- A correction direction and the regression check that will establish the fix.

Group repeated symptoms under one cause. Identify other affected locations without duplicate findings.
Do not prescribe a broad rewrite when a smaller correction restores the contract.
Do not invent findings to meet a quota. A thorough review can produce no actionable findings.

Answer two questions before a finding enters the record:

1. Does the changed code break a contract that this PR names?
2. Does a stated exit test of this PR fail?

A finding needs one yes. A concern with two answers of no goes under `## Out of scope`.

| Severity | Meaning |
|---|---|
| P0 | Immediate critical failure, such as broad durable data loss or a release that cannot start. State the demonstrated scope. |
| P1 | Major correctness, recovery, determinism, or required-gate failure. Resolve before merge. |
| P2 | A concrete defect or material contract gap under a supported condition. Resolve before merge or obtain an explicit owner disposition. |
| P3 | An optional improvement with no broken required contract. It does not block merge. |

Scope decides whether a concern enters the table at all. Severity decides how much it blocks. A concern outside the scope of this PR takes no severity.

Severity describes impact and urgency. It does not replace evidence or the project gate.
Do not reduce severity because the patch is small or the author calls the change safe.
Quote both statements when owner decisions conflict. File the question in `docs/questions.md` and stop dependent work (D-19).

## Verdicts

| Verdict | Required condition |
|---|---|
| Blocked | Provider independence, the review target, a necessary owner decision, or required evidence remains unresolved. Record any verified defects too. A session with no network follows D-602. |
| Changes required | The eligible review found in-scope defects or contract violations that need correction. List the required changes. |
| Ready for owner merge | The provider gate passes, the complete scope has review coverage, all required checks pass, and no blocking finding remains. |

A line under `## Out of scope` never gives the verdict `Changes required`.
No findings does not mean no risk. State material limits without a claim of zero regressions.
Approval applies only to the recorded revision. A new base or head requires assessment of the changed scope and evidence.
The owner alone merges the PR (D-8).

When the review record enters the PR, retain the assessed implementation head in that file.
Check any later metadata commit before the final verdict.
Do not require the review file to contain its own commit hash.
A metadata commit cannot hide code, content, requirement, or test changes.

## Do not raise a tool name as attribution

T-6 and D-22 prohibit text that names an agent, harness, or model **as the source of the work**.
A tool name that identifies a configured file, a schema, or a verified version is not attribution.

| Raise it | Do not raise it |
|---|---|
| A commit body that says an agent wrote the change. | The path `.claude/settings.json`. |
| A co-author trailer or a generation line. | A decision that names the schema it was verified against. |
| A PR description that credits a model. | A document that records which tool rejected a file. |

Apply the same test to every file before a finding. A reading that condemns the decision register is too broad.

## Do not address the automated reviewer

The reviewing provider reads the existing PR comments and takes them into its own review context (D-14). It never replies to gitar, never resolves a thread, and never writes a comment on the PR.

- A comment of the automated pass is a claim about the code, like any finding. Verify it against the head, and record the result under `## PR comments` in the review record.
- An author reply is evidence, and the review checks it: the trigger, the contract, and the commit it names.
- Check that the pass is current with the rule "Prove that a review is current" in the `gitar-review` skill. Use the read commands alone. A pass on an older commit is not an answered pass.
- An automated comment that the author refuted with evidence is not a finding. An automated comment that the author fixed is a fix to verify. An automated comment that stays open without an answer blocks the verdict, because the author's pass is not complete.
- The automated pass does not make gitar an author. The provider gate reads the providers of the substantive commits alone.

## Scope limits

A review request authorizes these actions and no other:

- Inspection, verification, the review record, and the handoff entry.
- A commit of those two files, and a push of that commit to the PR branch.
- A correction of a stale fact in the PR description, under "Correct the PR description" in `references/review-record.md`.

It does not by itself authorize a code fix, a merge, or another external message. A reply to the automated reviewer is an external message, and the reviewer never writes one (D-14). A reviewer never pushes to `main` (D-8). Honor explicit authorization already present in the session.

If the reviewer writes a substantive fix, reassess provider eligibility. The reviewer cannot approve its own contribution. Do not disguise a fix as review metadata to bypass the provider gate.
