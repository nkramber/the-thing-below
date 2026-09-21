# PR-48 review response

Date: 2026-09-21

The author answers `docs/reviews/pr-48.md`, the review of head `fd97eb2` with the verdict `Changes required`.

## P1-1: Enemy records omit the required body size and map consistency check

Disposition: full merit.

Contract: D-754 says that the enemy record of PR-80 gives the size, and that the load fails a map whose size disagrees with the record. The PR-80 entry of the phase file did not name that rule, and the change of `fd97eb2` missed it.

Reproduction: at `fd97eb2`, `EnemyRecord` holds no size, and `ContentSet.Load` reads no size of a patrol against a record. A brute record of any size loads with the elite patrol of the fixture map.

Owner answers: a patrol names a group, and a group can mix sizes, so D-754 needed a pick of one record. The owner chose the largest enemy of the group, the waiting enemies included (D-788). The fixture grunt is common, and the fixture brute is elite (D-789).

Correction: commit `ef02f4a`.

- `EnemyRecord` reads the required `size` field, and refuses a name outside common, elite, and boss (D-206).
- `BattleContent.RequireGroupsOf` compares the size of each patrol with the largest enemy record of its group. The error names the map file, the patrol, the group, the enemy, and both sizes (D-754, D-788, T-2). The content set and each start or resume of a run call this check.
- The two fixture records and the two test records take the sizes of D-789. The identity records take common, because each identity map places a common patrol.
- `BattleRuns.Map` gives an elite guard an area one column wider than its body, because a large enemy holds an area (D-209).
- The PR-80 entry gains the size in its scope and exit test 6. Section 7.7 of `area-battle.md`, the design pass line, and the glossary follow.

Regression check: `EnemyRecordTests.AMapWhoseSizeDiffersFromTheRecordFailsTheContentSet` changes the brute record of the checkout to boss, and the load fails on `rules/maps/fixture-dungeon.json`. The error names `patrol.fixture_dungeon_deep`, `group.fixture_elite`, `enemy.fixture_brute`, `'elite'`, and `'boss'`. The test fails on `fd97eb2`, because that record holds no size. `AWaitingEnemyCountsForTheSizeOfTheGroup` proves that a waiting brute sets the size of its group. The absent-field theory gains `size`. `make verify` passes with 1587 tests.

Simulation version: this PR already raises the version from 7 to 8 (G-17). The size check reads content at load alone, and each valid map runs as before. The seven identity hashes stay the same. The content hash changes, and the committed file follows.

## New ids

D-788 and D-789.

## Final head

The effective head is `ef02f4a`. The commit after it changes the metadata set alone: this file and the handoff (D-610).
