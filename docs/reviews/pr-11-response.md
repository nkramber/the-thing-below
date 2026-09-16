# PR-11 review response

Date: 2026-09-16

Author: Claude Code. This file answers `docs/reviews/pr-11.md`, verdict `Changes required` for head `848de1e`.

## Summary

Two findings. One has partial merit, one has full merit. Both corrections landed. A scan for the class of defect that P1-1 names found one more instance that the review did not list, and that correction landed too.

## P1-1: PR-61 requires tests that its prerequisites schedule later

Disposition: **partial merit.**

### The part with merit: the screen capture

The trigger reproduces. The Phase 2 sequence puts PR-61 second and PR-41 sixth, and PR-41 creates the screen-test job (D-172, `phase-2-first-playable.md` section 8). Exit test 1 of PR-61 read "A screen test captures a fixture screen in both views of D-480". That test could not run on the day PR-61 lands.

The order itself cannot move. D-524 puts PR-61 right before PR-7, PR-41 captures the map scene that PR-7 builds, and D-492 puts PR-45 between them. So the exit test moves, not the PR.

Correction, in `docs/roadmaps/phase-2-first-playable.md`:

- PR-61 exit test 1 now reads "A test computes both views of D-480 and both steps of the fit, and it locks each size (D-232, F-48)". That test is a headless computation, and PR-61 owns every input to it.
- PR-61 out of scope gains "The screen captures of the frame and the fit. PR-41 creates the screen-test job and takes them (D-172, G-16)".
- PR-41 exit tests gain "The job captures the frame and the fit of PR-61, in both views and at both screen row counts (D-232, D-480)". The other PR-41 tests renumber.

The work does not disappear. It lands four PRs later, in the PR that owns the job.

### The part with no merit: the panel test

The finding states that the panels of PR-61 "are out of scope until PR-62". The area roadmap assigns that rule to PR-61, not to PR-62. Section 7.4 of `docs/roadmaps/area-ui-input.md`, "The text helper and the string table", is built by PR-61, and it holds the line "A panel sizes to hold its longest string in the string table, and a test proves it (D-241)". PR-62 owns the window stack and the menu screens (section 7.6), not the sizing rule.

PR-61 builds the UI style file, the Godot `Theme`, and the nine-part window frame (D-220, D-527). A fixture panel exists there, and a text-metric test needs no rendered screen and no menu.

The wording was loose, so the correction makes it exact: exit test 3 now reads "the fixture panel" and not "each panel". The rule and its owner stand.

### One more instance of the same class

The finding names a class: an exit test that needs a PR which lands later. A scan of every exit test in all five phase files against the creator of each tool and job found one more.

PR-68 exit test 2 read "A bot answers each wait intent at once, and it plays a fixture scene to its end (D-64)". PR-68 lands twenty-sixth in Phase 2 and PR-15 lands twenty-ninth, so the headless runner and its two policies do not exist yet.

Correction, in the same file:

- PR-68 exit test 2 now reads "A scripted intent list answers each wait intent at once, and it plays a fixture scene to its end". The wait intent is an intent in the record, so a scripted list drives it with no bot policy (D-493, D-540).
- PR-68 review focus gains "The bots of PR-15 answer the same wait intent, so they play every scene when PR-15 lands (D-64, D-540, G-16)".

The contract in `area-story.md` section 7.2 is unchanged and correct. It states what the bots do, and it is not an exit test.

### Regression check

A script reads the section 8 sequence of all five phase files, builds the global order, and compares each exit test against the PR that creates each tool or job. The tools and jobs are det-lint, the screen-test job, the identity job, the bots, the night, the budget test, the map preview, the screenplay tool, the sound room, the review sheets, and the capture.

Result before the correction: PR-61 on the screen-test job, and PR-68 on the bots.
Result after the correction: neither. Two lines remain, and both are false matches: PR-59 and PR-37 use "captures" as the verb of the screen-test job of PR-41, which lands before each of them.

## P2-1: D-543 and D-544 assign the condition format to different PRs

Disposition: **full merit.**

The trigger reproduces. D-543 reads "PR-18 defines one condition form in content". D-544 gives the condition form to PR-68 and lists D-543 under "Applies", not under a revision. `CLAUDE.md` requires the mark in the earlier row: "Use `Revised in part by D-N` when one part changes, and name the part that changed and the parts that stand."

Correction, in `docs/decisions.md`, the Effect column of D-543:

- Added: "Revised in part by D-544 on 2026-09-16, the PR that defines the form: PR-68 takes the condition form with the flag set, and PR-18 keeps the branches and the choice effects. The one-form rule, the five readers, and the load validation stand."
- Removed: the line "PR-18 lands before PR-35, PR-14, and PR-19, or each of those ships a fixture condition". D-544 states that no PR ships a fixture flag, because PR-68 lands in Phase 2, before all three. The removal is recorded in the new revision note, so the register still shows what changed.

### Regression check

The regression check asks that every roadmap citation identify PR-68 as the sole owner. A search of every tracked document for D-543 gives twelve citations outside the register and the dated records. Each one cites the one-form rule or names PR-68 as the owner. None claims that PR-18 defines the form:

- `phase-2-first-playable.md` lines 949 and 1252 read "the one condition form of PR-68".
- `phase-2-first-playable.md` line 1231 reads "through the condition form of PR-68".
- `phase-3-story-systems.md` line 46 reads "on the one condition form of PR-68", and the PR-18 out-of-scope list already reads "The flag set and the condition form, which PR-68 built (D-544)".
- `area-story.md` lines 72 and 229, `area-exploration.md` line 194, `phase-4-region-one.md` line 94, `questions.md` OQ-146, and the `ste-writing` glossary all cite the one-form rule alone.

So the defect was confined to the register row, and no roadmap text needed a change. A decision that is revised in part stays citable, so each of the twelve citations stands (`CLAUDE.md`, "How to work with the owner").

## New ids

None. This response adds no decision, no question, and no finding. The corrections restate three exit tests and one decision Effect column.

## Files changed

- `docs/decisions.md`: the Effect column of D-543.
- `docs/roadmaps/phase-2-first-playable.md`: PR-61 exit tests 1 and 3 and its out-of-scope list, PR-41 exit tests, PR-68 exit test 2 and its review focus.
- `docs/reviews/pr-11-response.md`: this file.
- `docs/session-handoff.md`: the Session 35 entry.

## Verification

- `python3 docs/tools/ste-check.py $(git ls-files '*.md' | grep -v -e '^docs/reviews/' -e '^docs/session-handoff' -e '^docs/archive/')`: 0 findings.
- The exit-test dependency scan: no exit test depends on a PR that lands later.
- The PR-id coverage check: every active id from PR-1 to PR-79 still has one entry in one phase file and one place in one phase sequence.
- The order check: the phase lists of section 7, the sequence of section 8, and the section 8 of each phase file still give the same 77 ids in the same order.
- `git diff --check`: clean.
- `cmp -s AGENTS.md CLAUDE.md`: identical.
- Build, test, and format commands: not run. The repository holds no code until PR-1 (G-16).

## The new head

The corrections change `docs/decisions.md` and `docs/roadmaps/phase-2-first-playable.md`, which sit outside the metadata set. So the effective head moves from `848de1e` to the commit that holds this response. The repeat review reads that head.
