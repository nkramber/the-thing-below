# PR-89 review response

Date: 2026-09-27

Author: Claude Code. Review record: `docs/reviews/pr-89.md`, verdict `Changes required` at head `e8a8cd9f3407a5f4a345607d17d8780d83b82bb5`.

## P1-1: The promotion concurrency group can drop a pending push

Disposition: full merit.

Evidence: GitHub keeps at most one pending run in a concurrency group, and a new queued run cancels the pending one. With one run active, a second push waits, and a third push cancels the second. The second push then has no promotion check, and no later run checks its PR.

Correction: `.github/workflows/night-promote.yml` has no concurrency group now. Each push gets its own run. The first step of each run waits until each earlier run of the workflow ends, and it fails the run after 15 minutes with the ids of the open runs (T-2). The facts and the promotion check come after the wait, so the checks follow the order of the pushes (D-1202). `docs/runbooks/night.md` states the order.

Regression check: `NightWorkflowTests.EachPushKeepsItsPromotionRunInTheOrderOfThePushes` asserts no concurrency block, the wait before the read of the facts, and the filter of the open earlier runs. It fails on `e8a8cd9`. The live queue of several pushes needs the workflow on `main`, so it first runs after the merge (F-37).

## The command fault

The `codex-review` command stopped after the record landed: the record names head `e8a8cd9`, and the effective head is `9944c29`, because `e8a8cd9` changes the handoff files alone (D-610). The next review round writes a new record for the new effective head.

## New ids

None.

## Final head

The head of this round follows the push of the correction and this response.
