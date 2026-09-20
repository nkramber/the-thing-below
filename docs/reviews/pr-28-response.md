# PR-28 review response

Date: 2026-09-18

Author: Claude Code

## Identity

- PR: 28
- Branch: `feat/pr-5-content-and-string-table`
- Reviewed head: `55eb083d6bbca29d0101277ad285e659935d822c`
- Review record: `docs/reviews/pr-28.md`, verdict `Changes required`

## P2-1: Rule entry ids are not checked against their file kind

Disposition: **full merit.**

The finding reproduces. A copy of `content/` with `fixture.lamp` changed to `enemy.cave_rat` in
`content/rules/fixtures/tools.json` loaded with no error on `55eb083`:

```
content-hash --root <copy> --write
content-hash: wrote <copy>/TheThingBelow.Tests/identity/content-hash.txt
  778d3f0790c9e0f1155f32af9f131beb1ecfcf4d03027db9484aaf6856f34bfb
exit=0
```

The contract is D-646. Its Decision column says that the kind of an id agrees with the file that
holds the entry, and its Effect column names three tests for PR-5: the form of each id, the
agreement of a kind with its file, and no repeated id. The PR wrote the first and the third, and
it did not write the second.

### The correction

The record owns the kind, and not the path. One record reads every file of its kind, and the
folder name of a file is plural, so the path gives no kind. Thus:

- `RuleFixture.IdKind` holds the kind `fixture` (`TheThingBelow.Core/Content/RuleFixture.cs`).
- `ContentReader.ReadContentId(string kind)` reads an entry id and refuses another kind. The error
  names the file, the field, the id, both kinds, and D-646.
- `RuleFixture` reads the `id` field with that method.

The `label` field keeps `ReadContentId()` with no kind. A string id names where the player reads
the text, so its kind is `label` or `ui` and never the kind of the record that points at it (G-7).
A test holds this rule, so a later change cannot make the two fields one case by accident.

The same probe now fails on the corrected head:

```
Error: content-hash stopped on the root '<copy>': the id 'enemy.cave_rat' carries the kind
'enemy', and this file holds entries of the kind 'fixture' (D-646)
(file rules/fixtures/tools.json, field fixtures[0].id)
exit=1
```

### The regression check

`ContentReaderTests.AnEntryIdOfAnotherKindFails` covers three kinds: `enemy.cave_rat`,
`item.rusted_key`, and `label.lamp`. A run of that test against `55eb083`, in a worktree with the
new test file and the constant written out as a literal, gave three failures:

```
failed ContentReaderTests.AnEntryIdOfAnotherKindFails(id: "enemy.cave_rat")
  Assert.Throws() Failure: No exception was thrown
failed ContentReaderTests.AnEntryIdOfAnotherKindFails(id: "item.rusted_key")
  Assert.Throws() Failure: No exception was thrown
failed ContentReaderTests.AnEntryIdOfAnotherKindFails(id: "label.lamp")
  Assert.Throws() Failure: No exception was thrown
```

Two more tests join them:

- `ContentReaderTests.AStringIdTakesAKindOfItsOwn` holds the `label` field open to another kind.
- `CheckoutContentTests.EveryRuleEntryOfTheCheckoutCarriesTheKindOfItsRecord` reads the real tree.

## Documents

- `docs/roadmaps/phase-1-foundations.md`: exit test 6 of section 7.12 now names the kind rule, and
  the tests after it take their new numbers.
- `docs/roadmaps/area-core.md`: section 7.7 states that each rule record owns the kind of its entry
  ids.

No new D-# id and no new F-# id come from this round. D-646 already held the rule, and the PR did
not carry it into code.

## The simulation version (G-17)

`SimulationVersion.Current` stays at 2, and this round moves no line of the identity file.

The correction refuses content that the reader accepted before, and it changes the state of no
content set that loads. Every valid content set gives the same records, the same state hash, and
the same content hash, which stays `5ce12c64...f3c15f3`. PR-5 raised the version from 1 to 2 for
the content rules of this PR, and no build carries version 2 yet, because the PR does not merge
until the owner merges it. A second raise inside one unmerged PR would move the `state-hash` run
of the identity file for no change of behavior that any record can read.

## Verification

- `make verify`: passed. The build gave 0 warnings, 406 tests passed, and the format check,
  `det-lint`, `ste-check`, the identity check, the content hash, and the Godot smoke session
  passed.
- The test count moved from 401 to 406 with the five new tests.

## Out of scope

The review lists the atlas, the palette, and the maps under `## Out of scope`. This response adds
nothing there, and the corrections above touch no path of those items.
