# PR-54 review response

Date: 2026-09-22

Author: Claude Code. This file answers `docs/reviews/pr-54.md`, verdict `Changes required` for head `c9429c4`.

## Summary

One finding, with no merit. No code changes. The effective head stays `c9429c4`.

## P1-1: The synchronize event has no previous head input

Disposition: **no merit.** The trigger does not reproduce.

The finding states that `github.event.before` is empty on a `pull_request` event with the `synchronize` action. The CI run of this PR shows the opposite:

- Run: https://github.com/nkramber/the-thing-below/actions/runs/35680701238, head `43c2edc`, event `pull_request`.
- The step "Write the facts of the change" logged `EVENT_NAME: pull_request`, `EVENT_ACTION: synchronize`, and `BEFORE_REF: c9429c40ee3a967e6cfa5226fb82d1367c14aa08`.
- The step then logged "The push changes these paths from c9429c40ee3a967e6cfa5226fb82d1367c14aa08:" and "The previous head has these check runs:". Thus it wrote both fact files.
- The step "Read the changed paths" logged "The push changes docs alone, and each skipped check passed on the previous head. Each other job skips (D-856, D-858)." and `documents-alone: true`.
- The six matrix and plain jobs skipped, and each gate job and ste-check reported `success`.

The payload of the `synchronize` action holds `before` and `after`. The run above read `before` from it, and the value is the previous head of the PR. The page that the review cites lists the fields of `push` in full, and it does not list every field of each `pull_request` action.

## The regression check of the review

The review asks for a run of two docs-only pushes in a row after a green code head. The first is `43c2edc`, after the code head `c9429c4`. The push of this response file, `38cface`, was meant as the second. It cancelled the run of the review push `3ba4dad`, so it read a head that did not pass and ran every job, as D-858 requires. Two spaced metadata pushes then gave the check:

- `54e8c5c`, after the green `38cface`: run 35681799485 gave `documents-alone: true`.
- `bed989b`, after `54e8c5c`, which skipped: run 35681880270 read `BEFORE_REF: 54e8c5c…` and gave `documents-alone: true`. Each build job skipped, and each gate job reported `success`.

Thus two docs-only pushes in a row both skip after a green code head. The gate jobs of `c9429c4` carry the pass from one skipped head to the next.

A test with the payload of the event cannot run outside GitHub, because no local runner reads a workflow. The command tests hold each decision, and the two live runs hold the event shape.

## New ids and head

- No new D-# or F-# id.
- The effective head stays `c9429c4`. This commit changes the metadata set alone (D-610).
