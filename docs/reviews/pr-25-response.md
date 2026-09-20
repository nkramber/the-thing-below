# PR-25 review response

Date: 2026-09-18

Author response to `docs/reviews/pr-25.md`, which gives the verdict `Changes required` for head `83c17f7`.

## P2-1: The Phase 1 sequence repeats item 11

Disposition: full merit.

The trigger reproduces. Section 8 of `docs/roadmaps/phase-1-foundations.md` gave the number 11 to two
steps: the new PR-86 line, and the owner step that requires the checks on `main`. The PR-86 entry of
this PR added the line and left each later number as it was.

A sweep for the same cause found a second list with the same defect, which the review did not name.
Section 7 of `docs/design.md` gave the number 10 to the new PR-86 line and to the same owner step.
One command reads every ordered list of the design doc and the roadmaps, and it found both:

```
python3 - <<'PY'
import pathlib, re
for name in [...]:
    # one block for each run of numbered lines, then the repeated numbers of that block
PY
```

The sweep also found two stale references that the same insert left behind:

| Place | The fault | The correction |
|---|---|---|
| `phase-1-foundations.md` section 8 | Two steps hold the number 11 | The owner step and each later item move up by one, to 12 through 23 |
| `docs/design.md` section 7, Phase 1 | Two steps hold the number 10 | The owner step and each later item move up by one, to 11 through 23 |
| `phase-1-foundations.md` section 8, the gate line | It names section 7.19 | Gate 1 moved to section 7.20 when the PR-86 entry took 7.9, so the line names 7.20 |
| `phase-1-foundations.md` section 7.8 | It names the round as D-626 to D-638 | The fourth screen took the round to D-640, so the line names D-640 |

The order of each list does not change, and no item moves to another place. The PR-86 entry keeps
its position after the probe step in both lists.

Regression check, run on the corrected tree:

- The ordered-list sweep over `docs/design.md`, the five phase files, `docs/roadmaps/readme.md`, and
  the four area files of this PR prints "no duplicate item number in any ordered list".
- The Phase 1 sequence of `phase-1-foundations.md` reads 1 to 23, each number one time, in the order
  that it held before the correction.
- The Phase 1 list of `docs/design.md` reads 1 to 23 under the same rule.
- A search for `Section 7.19` in `phase-1-foundations.md` returns nothing, and section 7.20 is the
  Gate 1 entry.
- A search for `D-626 to D-638` in every live document returns nothing.
- `ste-check`: 0 findings.
- `make verify`: passed. The build succeeded with 0 warnings, 187 tests passed, the format check
  passed, `det-lint` gave 0 findings, `ste-check` gave 0 findings, and the smoke session ended with
  no error.

## The verification notes of the review

Two notes of the review record a result that this machine does not reproduce. Neither one is a
finding, and neither one changes the diff.

- `make verify` failed at build after 5:00 in the review session, with 0 warnings and 0 errors. The
  run above passed on this machine in about 40 seconds. Session 98 recorded the same stop on a
  repeat review, at 7:41. The cause sits in the review environment and not in the tree.
- The comment export failed in the review session, so the review could not read the two inline
  replies to the Gitar findings. Both threads are resolved, and Gitar replied at 06:47:45Z:
  "Confirmed - both fixes are in `83c17f7`." The next review can read them with command B of the
  `gitar-review` skill.

## What changes in this round

- `docs/roadmaps/phase-1-foundations.md`: the sequence numbers, the gate line, and the round of
  section 7.8.
- `docs/design.md`: the Phase 1 list numbers.
- `docs/reviews/pr-25.md` and this file: the review record and the response.
- `docs/session-handoff.md` and `docs/session-handoff-archive.md`: the entry of the review session
  and the entry of this round.

The first two paths sit outside the metadata set, so this round moves the effective head (D-610).
The new head needs a Gitar pass and a repeat review.
