# Area roadmap: Release

Status: **focused area roadmap, draft in PR #11.** This file says how the game reaches a player, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-ci.md` holds the export job and every other CI job. The file `area-ui-input.md` holds the title screen and the settings, and `area-art.md` holds the drawing of each store image. The file `area-core.md` holds the run record, the save, and the crash file, and `area-story.md` holds the story scene that the credits roll uses.

External facts: the external facts of `docs/design.md` hold every fact of this area, each read 2026-09-14. They cover Steam, the Steam Deck, Steam Cloud, Next Fest, the store assets, the trailer format, Apple notarization, Windows signing, GitHub Releases, GitHub runner prices, the Godot export templates, the Godot and OFL license notices, the Godot boot splash, and Movie Maker. This file cites them and never restates one.

Text rules: this file follows ASD-STE100 (D-10). The store text and the release notes are game text, and the `game-text-style` skill holds their voice (D-11, D-63, D-452). Tables are exempt from sentence-length counts.

## 1. Thesis

The game reaches players in three steps. From PR-54, every merge exports three builds that the owner can run (D-449, D-503). From Phase 5, a release tag publishes the prologue on GitHub Releases (D-53, D-457). Then the prologue becomes the free demo of the full game on Steam (D-133, D-143, D-478).

The game supports four targets and nothing else (D-481). They are Windows on x86_64, Linux on x86_64, macOS on Apple silicon, and the Steam Deck, which runs the Linux build. The Deck is the readability and performance floor (D-92, D-228).

Two dates in the plan are not code. At Gate 2 the owner pays the Steam Direct fee, and the store page goes public as Coming Soon (D-471). At Gate 5 the demo goes live, and GitHub Releases stop (D-470).

The order of the area follows the first need. The version and the export come first, at PR-6 and PR-54. The capture, the store text, and the store art follow the play of Gate 2. The release workflow, the title screen, the Deck pass, and the Steam work close Phase 5.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the release area.

| # | Finding | Binds |
|---|---|---|
| F-23 | `--headless` draws nothing, so no headless run captures an image | PR-74: the capture runs in a development build with a window, never in CI (D-172) |
| F-32 | The cost model listed the Steam Direct fee alone | PR-79: the Apple Developer Program at 99 USD a year (D-455) |
| F-33 | The release block found five gaps in the plan | PR-77, PR-79, and PR-61: the credits, the private repository, and the crash address (D-456, D-467, D-473, D-559) |
| F-34 | Steam needs five screenshots at 1920 by 1080 in 16:9 | PR-76: a screenshot comes from the 2x scale at 2560 by 1440 (D-568) |
| F-42 | No command-line option installs the Godot export templates | PR-54: the job unpacks them, and OQ-83 holds how (D-508) |

## 7. Roadmap

Each part below says how one part of the release works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The game version

Built by PR-6, PR-44, and PR-33. Phase files: `phase-1-foundations.md` and `phase-5-first-release.md`.

- The game version is 0.MINOR.PATCH until the full game ships as 1.0.0 (D-448). The minor number follows the gate: 0.2 at the first playable, and 0.5 at the prologue.
- A release tag has the form `v0.5.0` (D-448).
- The game version is a label for people. The simulation version, the snapshot format versions, and the content hash carry compatibility (G-17, D-166, D-448).
- PR-6 writes the game version into the run record header, and PR-44 into the crash file (D-447, D-448, D-491).
- A small line with the version sits in a corner of the title screen (D-454). It is a string table entry with the version as a value (G-7).
- OQ-168 holds where the version lives in the build.

> *In plain English:* every build carries a number that a person can read. A crash report and a save both name it, so a bug report says which build broke.

### 7.2 The exports

Built by PR-54. Phase file: `phase-2-first-playable.md`.

- From PR-54, every merge to `main` exports three builds, and CI keeps each as a build artifact (D-449, D-503, D-512).
- Each CI leg exports the build of its own system: Windows and Linux on x86_64, and the universal macOS build (D-481, D-482).
- The game supports and tests Apple silicon alone, and an Intel Mac gets no support and no test (D-482).
- The owner downloads the Linux build artifact for each Deck play (D-92, D-458).
- Every export carries the license files of Godot, the Terminus fonts, and the .NET runtime (D-467).
- The file `area-ci.md` holds the job, the export templates of F-42, and the smoke session on each export.

> *In plain English:* from the first walkable build, every merge makes a game that runs on a desktop and on the Deck. The owner never has to build one by hand.

### 7.3 The release workflow and GitHub Releases

Built by PR-31. Phase file: `phase-5-first-release.md`.

- On a release tag, the workflow publishes the three exports of the prologue as a GitHub Release with its release notes (D-53, D-448, D-453, D-457).
- Only the tagged builds of the free prologue go there, from 0.5.0 on. The builds of Gates 2 to 4 stay CI artifacts (D-457).
- No build of a paid region ever goes on GitHub Releases (D-456, D-457).
- Each release file must stay under 2 GiB (the external facts of `docs/design.md`).
- GitHub Releases stop when the demo goes live on Steam, and the releases that exist stay up (D-470).
- Each release tag and each Steam update carries short notes in the voice of the store text, and the owner approves them (D-453, G-20).
- The runbook of D-53 gives the Open Anyway steps for macOS and the Run anyway step for Windows (D-455, D-463).
- OQ-169 holds where the release notes live.

> *In plain English:* one git tag turns into a download page with three builds and a short list of what changed. Later, Steam takes that job over.

### 7.4 The title screen, the settings, and the exit

Built by PR-33. Phase file: `phase-5-first-release.md`.

- The first screen plays the main theme, and the settings hold the CRT toggle, the bindings, the audio group, and the vibration setting (D-427, D-434, D-435).
- The settings screen itself comes with PR-63, and `area-ui-input.md` holds it (D-526).
- A clean exit saves (D-258).
- The boot splash shows a short studio mark before the title (D-468). OQ-90 holds where that mark shows, and OQ-57 holds the studio name.
- A corner of the title screen shows the game version (D-454).
- The title menu holds a credits screen with the studio and the license notices (D-467).
- OQ-170 holds what the title menu holds.

> *In plain English:* the first thing a player sees: a name, a theme, and a short list of choices. One of them is the settings the Deck rating needs.

### 7.5 The credits and the license notices

Built by PR-77 and PR-33. Phase files: `phase-4-region-one.md` and `phase-5-first-release.md`.

- Credits appear in three places (D-467). They are a credits roll after the last story scene of region one, and a screen in the title menu. The license files in each export are the third place.
- PR-77 holds the credits roll as a story scene, its text, its timing, and the license notices, and it lands right after PR-29 (D-552).
- The credits roll plays under the main theme (D-427). The file `area-story.md` holds the story scene runner that plays it.
- The owner sees the roll at Gate 4, when they play region one end to end and sign off (D-56, D-552).
- PR-33 shows the same text on the credits screen of the title menu (D-552).
- The credits name the studio, and no agent, harness, or model (D-450, T-6).
- The Godot MIT notice must reach the player, and each OFL font needs its notice and its license with every copy (the external facts of `docs/design.md`).
- The credits text lives in the string table (G-7), and OQ-57 holds the studio name.

> *In plain English:* the game says who made it and which free tools it uses. It does that at the end of the story, in a menu, and in a file next to the program.

### 7.6 The capture

Built by PR-74. Phase file: `phase-2-first-playable.md`.

- PR-74 adds the capture in a development build. It replays a run record under Movie Maker into PNG frames and a WAV file, with fixed effect seeds (D-476, D-551).
- It lands right before PR-75, so the same path gives the five screenshots of PR-76 and every trailer shot (D-551).
- A run record gives the same shot on every take, and Movie Maker output stays identical on faster hardware (T-7, D-175, the external facts of `docs/design.md`).
- The capture lives behind the seam of the debug assembly, and a release export never loads it (D-171, D-260, D-492).
- It needs a window, because a headless run draws nothing, so it never runs in CI (F-23, D-172).
- Movie Maker clamps the window size to the resolution of the display (the external facts of `docs/design.md`).
- OQ-171 holds the input of the command, and OQ-172 holds its output format.

> *In plain English:* the game can replay a recorded run and write every frame to disk. That gives the same picture each time, so a screenshot or a trailer shot is repeatable.

### 7.7 The store text and the owner steps

Built by PR-75. Phase file: `phase-2-first-playable.md`.

- PR-75 holds the store text: the short description, the long description, and the feature list (D-452, D-550).
- Sessions draft the text under the `game-text-style` skill, and the owner approves the batch in the PR (D-57, D-452, G-20, G-25).
- The game never shows the store text, so G-7 does not bind it (D-452).
- PR-75 also holds the checklist of the steps that only the owner can do:
  - Pay the Steam Direct fee, 100 USD, which starts a wait of 30 days before a release (D-85, D-471).
  - Pick the studio name, and search it with the game name on Steam and in the trademark registers (D-408, D-450, D-451). OQ-57 holds the pick.
  - Answer the AI disclosure of the content survey, before the review of the store page (D-477). OQ-59 holds the answer.
- The store page goes public at Gate 2 in the Coming Soon state, and it stays there for years before the full game (D-471).
- Valve wants a page in that state for two weeks before a release. A store page review takes 3 to 5 business days (the external facts of `docs/design.md`).

> *In plain English:* the shop page words get written and approved like any other text in the game. The owner pays the fee and answers the questions only Valve asks.

### 7.8 The store art and the screenshots

Built by PR-76. Phase file: `phase-2-first-playable.md`.

- PR-76 holds the capsules, the logo, and the library images, and it takes at least five screenshots (D-475, D-550).
- Each store image is a large picture of drawn pieces, in the pixel style of the game (D-475, D-516). The file `area-art.md` holds the drawing and the render.
- The owner approves each art batch from its review sheets, which `gh` attaches to the PR description (D-514, G-25).
- Capsule art shows only game art, the game name, and an official subtitle. The capsules of the demo mark it as a demo (the external facts of `docs/design.md`).
- A screenshot comes from the frame at 2x, 2560 by 1440, with no bars and no crop (D-568, F-34). That size is 16:9 and larger than 1920 by 1080.
- The capture of PR-74 takes each screenshot from a run record, so the owner can take it again (D-551).
- OQ-173 holds the sizes of the store images, and OQ-174 holds which five screenshots.

> *In plain English:* a session draws the pictures on the shop page the same way as everything else in the game. The screenshots come from real play.

### 7.9 The trailer

Built by the owner, on the capture of PR-74. Phase file: `phase-5-first-release.md`.

- Sessions write a shot list, the capture writes the frames and the audio, and the owner cuts the trailer in a video editor (D-476).
- The first trailer comes before the prologue demo (D-476).
- Steam takes a trailer up to 1920 by 1080 at 30 or 60 frames each second, as a .mov, .wmv, or .mp4 file. Valve prefers H.264 with AAC (the external facts of `docs/design.md`).
- The trailer is owner work, and the cost model counts it (D-476).
- OQ-175 holds the shot list of the first trailer.

> *In plain English:* the owner cuts the trailer by hand from recorded play. The game replays a record exactly, so a second take gives the same picture.

### 7.10 The Deck verification pass

Built by PR-39. Phase file: `phase-5-first-release.md`.

- PR-39 walks the Steam Deck checklist to the rating Verified (D-459).
- Verified needs four things (the external facts of `docs/design.md`):
  - Glyphs that match the input in use.
  - Playable default bindings.
  - No text under 9 pixels high at 1280 by 800.
  - An on-screen keyboard wherever the player types.
- The 16-pixel font and the target of 60 frames per second clear the text and frame rules (D-263, G-19). The game has no text entry (D-459).
- The glyph rule is the main work, and PR-78 gives it the controller type (D-460).
- The pass also checks suspend and resume, and the 1x frame (D-85, D-92, D-228).
- Valve tests the native Linux build first, and it tests the Windows build under Proton only when the Linux build fails (D-458).

> *In plain English:* the Deck has a checklist, and this pass walks it. The game meets most of it, and the button pictures are the real work.

### 7.11 The Steamworks binding and the glyphs

Built by PR-78. Phase file: `phase-5-first-release.md`.

- PR-78 picks the C# binding for Steamworks, and it brings the pick to the owner with its license (D-462, D-553, G-13). OQ-58 holds the pick.
- It starts Steamworks and adds the call that reports the controller type, and the prompts of D-222 pick their glyph set from it (D-460).
- The game does not use the Steam Input API. Under Steam, a gamepad reaches the game as an Xbox controller, so the type call is what shows Deck glyphs on the Deck (D-460).
- Godot reads the gamepad in every build, so input works with no Steam client, in CI, and in the GitHub builds (D-460).
- The glyphs cover the controller types that the SDK version knows, and a remap in the Steam settings can show a wrong button (D-460).
- Phase 6 needs the same binding for the achievements and their stats calls (D-466).
- OQ-176 holds how the game reads the controller type.

> *In plain English:* on Steam the game asks which controller a player holds, so the button pictures match. That is the only thing Steam tells it.

### 7.12 The macOS signature and the notarization

Built by PR-79. Phase file: `phase-5-first-release.md`.

- From PR-79, CI signs and notarizes the universal macOS export on the hosted macOS leg (D-455, D-482, D-553).
- Steam requires a notarized macOS app from 2019-10-14, and notarization needs the paid Apple Developer Program at 99 USD a year (F-32).
- The owner joins the program before this PR, and the cost model carries the yearly fee (D-455).
- Builds before PR-79 stay unsigned, and the runbook of D-53 gives the Open Anyway steps (D-455, D-463).
- Windows builds carry no code signature, and the runbook gives the Run anyway step (D-463). That is an accepted risk where Smart App Control is on.
- No personal name goes on a file, so no managed certificate for an individual joins the plan (D-4, D-450, D-463).
- OQ-177 holds where the signing identity and the notarization secrets live.

> *In plain English:* a Mac refuses to run a program that Apple did not check. This step sends each Mac build to Apple for that check.

### 7.13 The Steam publish

Built by PR-40. Phase file: `phase-5-first-release.md`.

- PR-40 publishes the prologue as the demo app "The Thing Below: Prologue" (D-143, D-478, D-553).
- It sets Auto-Cloud on the save folder, and the game holds no cloud code (D-461, D-465).
- Auto-Cloud syncs when the game launches and when it exits (D-461). The Steamworks page does not say how it settles a conflict. PR-40 tests a conflict on two machines against D-93 and reports the result.
- PR-40 also tests that the shared cloud of the demo works with Auto-Cloud (D-143, D-461).
- It sets the Linux build on the Steam Linux Runtime that the Godot export needs. Valve recommends Steam Linux Runtime 4.0 for a new native Linux game (D-458).
- It verifies whether the demo app needs a Steam Direct fee of its own, because no page that a session read states one (D-478).
- A demo needs the store page of the full game visible as Coming Soon, which Gate 2 gave it (D-471, the external facts of `docs/design.md`).
- OQ-178 holds what the demo build changes from the full build.

> *In plain English:* this is the step where a stranger can find the game on Steam and play it for free. Their save follows them between machines.

### 7.14 The players before and after the release

Built by PR-44 and the owner. Phase files: `phase-4-region-one.md` and `phase-5-first-release.md`.

- After Gate 4, a few players whom the owner picks play the CI build artifacts and send their notes outside Steam (D-469). No Steam Playtest runs.
- The owner gives each player the artifact, and each player installs an unsigned build with the steps of the runbook (D-463, D-469).
- The builds of those players carry the license files of D-467.
- A crash writes a crash file beside the save, and the crash message gives the studio email address (D-170, D-473). The owner files each report by hand.
- The crash file holds no personal data, and the address is a public string table entry (D-170, D-473, G-7).
- The mailbox needs the studio name of OQ-57, and `area-core.md` holds the crash file itself.

> *In plain English:* a few trusted people play the game before anybody else, and they write to the owner. After release, a crash tells the player where to send the file.

### 7.15 The money, the repository, and Phase 6

Parked until Gate 5. Phase file: none yet.

- The repository stays public through the free prologue, and it goes private before the first content of a paid region, in Phase 6 (D-456).
- A private repository pays for CI minutes past the free quota, and its required checks need GitHub Pro or Team (D-456, the external facts of `docs/design.md`).
- Before the switch, a session checks that gitar, the review gate, and every workflow work on a private repository (D-456).
- The full game has Steam achievements, planned in Phase 6, and the prologue demo has none (D-466).
- The game enters the last Next Fest before the release date of the full game, with the prologue as its demo (D-472). A game enters one Next Fest alone.
- The prologue launch gets no fest (D-472).

> *In plain English:* the free part stays open for anybody to read. When paid work starts, the repository closes, and the costs begin.

### 7.16 Release in the tests

Built by PR-31, PR-54, PR-74, PR-78, PR-79, and PR-40. Phase files: `phase-2-first-playable.md` and `phase-5-first-release.md`.

- Each export starts with `--headless` and runs the smoke session, on every CI leg (D-512).
- The export job runs on each merge and on each PR that changes the export, so a break shows before that merge (D-512).
- PR-31 proves the release workflow on a test tag before the first real tag (G-16, L-11).
- A test proves that each export carries the license files of D-467 (T-3).
- PR-74 proves that two captures of one run record give the same frames (T-7, D-476).
- PR-78 adds a test that the game starts and reads input with no Steam client (D-460, T-3).
- PR-79 proves the signature and the notarization on the CI macOS leg, and a failure fails the job (T-2).
- PR-40 reports the result of the Auto-Cloud conflict test to the owner (D-461).
- No release change bumps the simulation version, because no release step changes a rule (G-17).

> *In plain English:* every build starts once in CI before a person sees it. The release steps each carry a proof that they worked.

### 7.17 Release by PR

| PR | Work | Decisions |
|---|---|---|
| PR-6 and PR-44 | The game version in the run record and in the crash file | D-448, D-491 |
| PR-54 | The export job on every merge | D-449, D-503, D-512 |
| PR-74 | The capture in a development build | D-476, D-551 |
| PR-75 | The store text, the release notes voice, and the owner checklist | D-452, D-471, D-550 |
| PR-76 | The capsules, the logo, the library images, and the screenshots | D-475, D-550 |
| PR-77 | The credits roll, its text, its timing, and the license notices | D-467, D-552 |
| PR-31 | The release workflow, the GitHub Release, and the runbook | D-53, D-453, D-457 |
| PR-33 | The title screen, the version line, the boot splash, and the credits screen | D-427, D-454, D-467, D-468 |
| PR-39 | The Steam Deck verification pass | D-459 |
| PR-78 | The Steamworks binding, the start, and the controller type call | D-460, D-462, D-553 |
| PR-79 | The signature and the notarization of the macOS build | D-455, D-553 |
| PR-40 | Auto-Cloud, the demo app, and the Linux runtime | D-458, D-461, D-478 |

### 7.18 Release that other area files hold

| Part | Area file | PR |
|---|---|---|
| The export job, its templates, and the smoke on each export | `area-ci.md` | PR-54 |
| The run record header, the save folder, and the crash file | `area-core.md` | PR-6, PR-43, and PR-44 |
| The debug assembly that holds the capture | `area-core.md` | PR-45 |
| The settings screen and the button prompts | `area-ui-input.md` | PR-63 |
| The drawing and the render of each store image | `area-art.md` | PR-55 and PR-76 |
| The main theme under the credits roll | `area-audio.md` | PR-72 |
| The story scene runner that plays the credits roll | `area-story.md` | PR-68 |
| The effect budget that holds 60 frames per second on the Deck | `area-effects.md` | The Deck test and PR-56 |

### 7.19 The contract of every later release PR

Each later PR that changes how the game reaches a player keeps this list. The phase files make exit tests from it.

1. Keep the four supported targets, and add none (D-481).
2. Put every player string in the string table, the version line and the credits included (G-7).
3. Name no agent, harness, or model in a credit, a store text, or a release note (T-6).
4. Keep the name of the owner and every personal detail out of every file and every build (D-4, D-450).
5. Carry the license files of Godot, the fonts, and the .NET runtime in every export (D-467).
6. Prove each new step in CI, or name the owner step that no job can run (G-16).
7. Take the approval of the owner for each text batch and each art batch (D-57, G-25).

> *In plain English:* every way the game leaves this repository follows the same rules: four systems, no names, and the licenses in the box.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). The release work keeps this order inside it:

1. PR-6: the game version in the run record header (D-448).
2. PR-44: the game version in the crash file. PR-61 adds the studio address to the message (D-473, D-559).
3. PR-54: the export job, right before PR-7 (D-503).
4. PR-17: the first playable.
5. **← GATE 2 (first playable).** The owner plays on both machines and signs off on feel (D-52, D-362).
6. PR-74: the capture in a development build (D-551).
7. PR-75: the store text and the owner checklist (D-550).
8. PR-76: the store art and the five screenshots (D-550).
9. Owner: pay the Steam Direct fee, and put the store page public as Coming Soon (D-471).
10. PR-28 and PR-29: the arc of region one (`area-story.md`).
11. PR-77: the credits roll, right after PR-29 (D-552).
12. **← GATE 4 (region one).** Then the trusted players play the build artifacts (D-469).
13. PR-31: the release workflow and the first GitHub Release of the prologue.
14. PR-33: the title screen, the settings, the version line, and the credits screen.
15. PR-39: the Steam Deck verification pass.
16. Owner: join the Apple Developer Program (D-455).
17. PR-78: the Steamworks binding, the start, and the controller type call (D-553).
18. PR-79: the signature and the notarization of the macOS build (D-553).
19. PR-40: Auto-Cloud, the demo app, and the Linux runtime.
20. **← GATE 5 (first release).** A fresh machine runs the tagged build, and the Deck runs the Steam demo.
21. Phase 6 stays parked (D-456, D-466, D-472).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block release PRs, and each PR asks its questions when it starts (D-487):

- OQ-57: the studio name. Blocks PR-33, PR-44, and PR-75.
- OQ-58: the C# binding for Steamworks. Blocks PR-78.
- OQ-59: the AI disclosure of the Steam content survey. Blocks PR-75.
- OQ-90: where the studio mark shows. Blocks PR-33.
- OQ-168: where the game version lives in the build. Blocks PR-6 and PR-31.
- OQ-169: where the release notes live. Blocks PR-31.
- OQ-170: what the title menu holds. Blocks PR-33.
- OQ-171: what the capture command takes. Blocks PR-74.
- OQ-172: the output format of the capture. Blocks PR-74.
- OQ-173: the sizes of the store images. Blocks PR-76.
- OQ-174: which five screenshots. Blocks PR-76.
- OQ-175: the shot list of the first trailer. Blocks the trailer.
- OQ-176: how the game reads the controller type. Blocks PR-78.
- OQ-177: where the signing identity and the notarization secrets live. Blocks PR-79.
- OQ-178: what the demo build changes from the full build. Blocks PR-40.

No open question blocks this file.
