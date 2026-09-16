# PR review: project contracts

Part of the `pr-review` skill (D-588). Load this file when the PR changes code, content, tools, or CI, or the text of a contract in one of the areas below.

## Project contracts

Apply each relevant row. Record why an area does not apply when its omission can mislead a reviewer.

| Area | Required examination |
|---|---|
| Core boundary | No engine dependency or gameplay input from Godot physics, timers, or navigation in Core. Trace data flow, not only imports (G-1, D-100). |
| Determinism | Integer math, seed ownership, one stream per subsystem, fixed iteration and event order, an ordinal order for strings, no clock or OS random (T-7, G-2 to G-4, F-39). A tool whose output a test compares on every CI leg keeps the same rules (D-502). |
| Replay | The record holds the seed, the content hash, the versions, and every intent (D-493). A replay reproduces the state hash. Verify the simulation version bump for a Core behavior change (G-5, G-17). A changed hash in the identity file comes with its simulation version bump (D-504). |
| Errors | Required context, visible failure, safe recovery, and assertions in release builds. An empty `catch` or a silent default violates T-2 (G-18). |
| Content | JSON schemas at load and in tests. An absent field reports the file, the field, and the reason. Check identifier references and file name case (D-116, G-6). |
| Strings | No inline player string. Every player string has an id in the string table and reaches the screen through the text helper (G-7, D-499). |
| Input and CI boundaries | Check size limits, file paths, and validation at affected external inputs. Inspect CI permissions, secret access, and execution of untrusted content when those boundaries change. Each action comes from the `actions` organization of GitHub, pinned to a full commit SHA (D-511). |
| Gameplay | The rules the design doc and the decisions set for the affected system. Trace repeated runs as well as one run. |
| Presentation | Gamepad and keyboard play, the one 16:9 frame of 1280 by 720 with 32-pixel tiles, black bars for every other shape and the Deck, a fit that looks good at 1920 by 1080, the Deck readability floor, the CRT toggle, and the atlas test (D-84, D-92, D-105, D-107, D-228, D-568). Each art batch has its review sheets in the PR description, each drawing file is JSON, and a rule file never names art (D-514, D-515, D-519). Each effect stays inside the effect budget, the world waits for an effect only through a wait intent, and each flash and shake has its reduced form (D-214, D-522, D-523). Game draws the world at 1x and builds the fit of D-232 itself, and each font keeps its pixel settings (F-48, F-49). Headless tests do not establish visual quality or game feel. |
| Dependencies and cost | A decision justifies each dependency (G-13). Performance claims include a profile before the change and a measurement after it (G-14). |

Do not reintroduce an earlier contract that a later decision supersedes.
