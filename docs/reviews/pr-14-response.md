# PR-14 review response

Date: 2026-09-16

Author: Claude Code. This file answers `docs/reviews/pr-14.md`, verdict `Changes required` for head `9837c1d`.

## Summary

One finding. P2-1 has full merit, and its correction landed. During the answer, the owner corrected the end point of a session, which D-582 records.

## P2-1: The completion gate rejects valid roadmap ownership

Disposition: **full merit.**

The trigger reproduces. Completion line 8 said "No line gives work to another PR." D-579 gives the machine checks to PR-3, and G-16 asks each absent check to name the PR that creates it. So the gate rejected PR #14 and every PR that uses the PR template.

The cause was the wording of the gate, which the session wrote. D-577 and D-578 forbid a later PR for a document or a record of the current PR alone. They never forbid a roadmap line that names the owner of independent work.

Correction:

- `.claude/skills/one-pr-one-session/SKILL.md`: line 8 now reads "No document, handoff entry, review record, or merge record of this PR waits for another PR." The documents gate limits the deferral bullet to a document or a record of this PR. It states that a line that names the PR of independent roadmap work is not a deferral (G-16, D-579). The enforcement table uses the same boundary.
- `CLAUDE.md` and `AGENTS.md`: the PR gate line on documents uses the same boundary.
- `.github/pull_request_template.md`: the no-deferral line uses the same boundary and names the valid case.
- `docs/decisions.md`: D-579 now says that the command fails a PR that defers a document or a record of the PR itself, and that a line that names independent roadmap work passes. The row is new in this PR, so the correction changes its text in place.
- `docs/roadmaps/phase-1-foundations.md`: PR-3 exit test 10 fails a PR that defers one of its own documents or records. The new exit test 11 passes a PR with complete documents that names the PR of an absent check. The scope line uses the same boundary.
- `docs/roadmaps/area-tools.md`: section 7.3 uses the same boundary.

Regression check: a separate evaluator applied the corrected skill to both fixtures of the review. The PR description gives the result.

- A PR that defers one of its affected documents to a follow-up PR.
- A PR that completes its own documents and names PR-3 as the creator of an absent check.

## The owner correction: a review answer stays in the session

The first answer to this review came from the author session, and the skill blocked it. The old rule ended the author session when the PR waited for the other provider. The owner answered: "Addressing Codex/gitar review feedback does NOT qualify for a new session."

The owner instruction of the same day said: "After the PR reaches its handoff or merge boundary, the session ends." The session applied the newer statement.

Correction:

- D-582 records the rule and revises D-576 in part: the end point of the author session.
- The skill: the author session answers each gitar comment and each review. It ends at the verdict `Ready for owner merge` for the effective head, or at the label. The block condition names a PR that the owner merged or closed, not a PR at its hand-over point. The reviewer session repeats its review after each correction, and it ends at the same verdict or when the owner ends the review. A correction author comes from the provider of the author (T-4).
- `pr-review`: the session end gate ends a session only at the hand-over point of its role.
- `ste-writing`: the glossary row "hand-over point" follows D-582.
- `docs/design.md`: the session pass line cites D-582, and section 8 cites D-576 to D-582.

## New ids

- D-582.
- No new F-# or OQ-# id.

## Final head

The commit that holds this file, the corrections, and the handoff entry of Session 48. That commit changes paths outside the metadata set, so it is the new effective head.
