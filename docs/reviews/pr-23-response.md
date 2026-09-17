# PR-23 response

Date: 2026-09-17

The answer of the author to `docs/reviews/pr-23.md`, the review of head `f5eba68`.

## P2-1: Escaped quotes bypass the scene text rule

Disposition: full merit.

- The trigger reproduces. On `f5eba68`, a scene file with the line `text = "Say \"hello\""` gave `0 finding(s)`, and the command exited 0.
- The cause: the value part of the pattern was `[^"]*`, which stops at the first quote. Godot writes a quote inside a string value as `\"`, so the pattern matched no part of the line, and DL 9 read no property.
- The contract: D-499 and G-7 keep every player string in the string table, and a scene file holds layout alone. A value with an escaped quote is such a string, so the rule missed a supported case.
- The correction: the value part is now `(?:[^"\\]|\\.)*`, which reads an escaped character as one unit. `TheThingBelow.Tools/DetLint/SceneTextRule.cs` holds the one changed line and its comment.
- The regression test: `ATextValueWithAnEscapedQuoteFails` in `TheThingBelow.Tests/DetLintSceneTextTests.cs`. It fails on the old code and passes on the new code.
- The probe of the reviewer now gives one finding: `TheThingBelow.Game/Probe.tscn:2: rule DL 9: the property `text` holds the text "Say \"hello\"".` The command exits 1.

## Checks

- `dotnet test --solution TheThingBelow.slnx --no-build`: passed, 187 tests. The count was 186 before the regression test.
- The same test on the code before the correction: `ATextValueWithAnEscapedQuoteFails` failed, and the other 186 tests passed.
- `make verify`: passed. The build with 0 warnings, 187 tests, the format check, `det-lint` with 0 findings, `ste-check` with 0 findings, and the smoke session.

## New ids

None. This round adds no decision, no question, and no finding of the design register.
