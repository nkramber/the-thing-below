# PR-77 response

Date: 2026-09-24

Author: Claude Code. This file answers the review of `docs/reviews/pr-77.md` on the effective head `87cf4cc`.

## Round 1

### P2-1: The poll accepts a check that completes after the deadline

Disposition: full merit.

Evidence: command E read the status first, and it accepted each `completed` status with no read of the completion time. A check in progress at the read of 880 seconds can complete at 905 seconds. The next read at about 900 seconds then printed `completed`, and D-1074 asks for a stop at 15 minutes.

Correction: the jq filter of command E now gives the completion time in seconds, with `fromdateiso8601`. A completed check prints `completed` only when it completed 900 seconds or less after the push. Otherwise command E prints `not complete at 900 s` with the real completion time, and the session stops (D-1074). A head with no Gitar check now gives `none` in place of three null words.

Regression check: command E ran against the Gitar check of PR #77, which completed at the epoch second 1790291292. With the push time 100 seconds before it, the result was `gitar: completed at 100 s`. With the push time 1000 seconds before it, the result was `gitar: not complete at 900 s, completed at 1000 s`. With a check name that no check has, and the push time 200 seconds in the past, the result was `gitar: no check at 200 s`.

## New ids

None.
