# PR 81 response

The author answer to the review of `f66e314f07806d96e3f11125c9bfa6d07f1b94e3` in `docs/reviews/pr-81.md`. The review found no defect, and it gave `Blocked` for two open items.

## Item 1: the Steam Deck measurement of D-961

Disposition: resolved. The measurement ran before the review.

Evidence: the owner asked the session to run it over SSH on 2026-09-25. On the Steam Deck, at the head `830f146` in Release, `evaluator-cost` gave these values for one enemy turn over 5000 turns of six enemies against three characters with lessons:

- First run: median 48 us, 95th percentile 128 us, slowest 441 us.
- Second run: median 60 us, 95th percentile 120 us, slowest 400 us.

The limit is 1000 us at the 95th percentile (D-961). The commit `f66e314` after `830f146` changes three capture baselines alone, so the measured code is the code of the effective head. The PR description holds the same numbers since the edit of 2026-09-25 15:33 UTC. The handoff entry of the first round still named the run as in flight, and the entry of this round corrects it.

## Item 2: the note of the Gitar dashboard

Disposition: answered, no change.

Evidence: the note reads "Refused settings handling replaces prior retained files while simulation rules change." The behavior is the owner answer D-1099: the newest refused file replaces an older one, so the folder never fills. `SettingsStoreTests.ARefusedFileIsKeptAsideAndReplacesAnOlderKeptFile` asserts it. The version rises to 28 for the rule changes (G-17, D-504). The PR comment of 2026-09-25 answers the note with this evidence.

## Ids

No new D-# id and no new F-# id.

## Final head

The head of this answer is the commit that holds this file. It changes the metadata set alone, so the effective head stays `f66e314` (D-610).
