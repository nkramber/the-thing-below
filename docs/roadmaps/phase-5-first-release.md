# Phase roadmap: Phase 5, First release, the free prologue

Status: **active focused phase roadmap, which PR #11 merged on 2026-09-16.** This file gives each item of Phase 5 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 5 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 5 turns the prologue into something that a stranger downloads and runs. Phase 4 ended with a build that the owner and a few trusted players played from a CI artifact (D-469). This phase gives that build a front door, a tag, a rating, and a Steam page.

The order follows risk. The GitHub Release comes first, because it needs no account and no fee (PR-31). The title screen follows, because a release with no first screen reads as unfinished (PR-33). The Steam binding comes next, because the Deck pass needs its controller type call (PR-78, D-565). The Deck pass follows, because its findings can change the UI (PR-39). The macOS signature and the publish close the phase (PR-79, PR-40).

Three owner steps sit inside the order. The owner joins the Apple Developer Program before PR-78, and the owner cuts the first trailer before PR-40 (D-455, D-476, D-565). After PR-40, the owner requests the Deck compatibility review from Valve (D-565).

Gate 5 asks two runs: a fresh machine runs the tagged build, and the Deck runs the Steam demo.

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of Phase 5.

| # | Finding | Binds |
|---|---|---|
| F-18 | The CRT is on by default on the Deck | PR-39: the Deck pass reads the text with it on |
| F-32 | Steam needs a notarized macOS app, and notarization needs a paid program | PR-79: the signature in CI, and the owner step before it (D-455) |
| F-33 | The release block found five gaps, credits and the crash address included | PR-31 and PR-33: the credits screen and the version line (D-467, D-473) |
| F-34 | Steam needs five screenshots at 1920 by 1080 in 16:9 | PR-40: the store page that Gate 2 built already meets it (D-568) |

## 7. Roadmap

Each entry below gives one item of Phase 5 its scope, its exit tests, its review focus, and its questions. The area file of each entry says how the part works (D-144). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `CLAUDE.md` still applies to each PR, and these tests are the ones that this PR alone can fail.

### 7.1 PR-31: the release workflow

Area file: `area-release.md` section 7.3.

**Scope.**

- The workflow that publishes the three exports of the prologue as a GitHub Release on a release tag (D-53, D-448, D-457).
- The release notes of each tag, in the voice of the store text, which the owner approves (D-453, G-20).
- The tagged builds of the free prologue alone, from 0.5.0 on (D-457).
- The license files in each release file (D-467).
- The runbook steps: Open Anyway for macOS, and Run anyway for Windows (D-455, D-463).

**Out of scope.**

- The Steam publish (PR-40), which later takes this job over (D-470).
- Any build of a paid region, which never goes on GitHub Releases (D-456, D-457).
- The builds of Gates 2 to 4, which stay CI artifacts (D-457).

**Exit tests.**

1. A release tag publishes the three exports of D-481.
2. Each release file stays under 2 GiB.
3. Each export holds the license files of D-467.
4. The release notes appear on the release, and the owner approved them (D-453).
5. A fresh machine of each system runs its build with the runbook steps (D-463).
6. A build of a paid region never reaches the workflow, and a test proves the rule (D-456).

**Review focus.**

- The answer of OQ-168 says where the game version lives in the build (D-454).
- The answer of OQ-169 says where the release notes live.
- The notes name no agent, harness, or model (T-6).

**Questions.** OQ-168 and OQ-169.

> *In plain English:* one git tag turns into a download page with three builds and a short list of what changed. Later, Steam takes that job over.

### 7.2 PR-33: the title screen, the settings, and the exit

Area files: `area-release.md` sections 7.4 and 7.5, `area-ui-input.md` section 7.11.

**Scope.**

- The first screen, which plays the main theme (D-427).
- The title menu, with its entries from the answer of OQ-170.
- The settings entry, which opens the screen that PR-63 built (D-526).
- The clean exit, which saves (D-258).
- The boot splash with the studio mark, before the title (D-468, OQ-90).
- The game version in a corner of the title screen (D-454).
- The credits screen in the title menu, with the studio and the license notices (D-467, D-552).

**Out of scope.**

- The settings screen itself (PR-63) and the credits roll (PR-77).
- The Steam calls (PR-78).

**Exit tests.**

1. A screen test captures the title screen and the credits screen.
2. The main theme plays on the first screen (D-427).
3. A clean exit writes the resume file, and the next start offers it (D-258).
4. The version line matches the version in the run record header (D-448, D-454).
5. The credits screen holds the Godot notice and each font notice (D-467).
6. Every string comes from the string table, and det-lint proves it (G-7).

**Review focus.**

- The answer of OQ-57 gives the studio name for the mark and the credits (D-450).
- The answer of OQ-90 says where the studio mark shows (D-468).
- The credits name no agent, harness, or model (T-6).
- The title screen reads on the Deck at 1x, with the CRT on (D-92, F-18).

**Questions.** OQ-57, OQ-90, and OQ-170.

> *In plain English:* the first thing a player sees: a name, a theme, and a short list of choices. One of them is the settings that the handheld rating needs.

### 7.3 The owner step: the Apple Developer Program

Owner work, before PR-78. Area file: `area-release.md` section 7.12.

**Scope.**

- The owner joins the Apple Developer Program, at 99 USD a year (D-455, F-32).
- The cost model carries the yearly fee (D-455).

**Out of scope.**

- No code. PR-79 uses the identity that this step creates.

**Exit tests.**

1. The program membership is active before PR-79 starts.
2. The cost model holds the yearly fee with its date.

**Review focus.** No PR and no review. The session records the step in the sequence and the cost model.

**Questions.** None. OQ-177 holds where the secrets live, and PR-79 asks it.

> *In plain English:* Apple charges a yearly fee before it will check a program. The owner pays it before the step that needs it.

### 7.4 PR-78: the Steamworks binding and the glyphs

Area files: `area-release.md` section 7.11, `area-ui-input.md` section 7.10.

**Scope.**

- The pick of the C# binding for Steamworks, with its license, which the PR brings to the owner (D-462, D-553, G-13, OQ-58).
- The start of Steamworks in the game.
- The call that reports the controller type, which the prompts of D-222 read (D-460, OQ-176).
- The glyph sets that the SDK version knows (D-460).

**Out of scope.**

- The Steam Input API, which the game does not use (D-460).
- The achievements and their stats calls, which Phase 6 holds (D-466).
- The Steam publish (PR-40).

**Exit tests.**

1. The game starts with no Steam client, in CI and in a GitHub build (D-460).
2. Under Steam, the prompts show the glyph set of the controller in use (D-460).
3. On the Deck under Steam, the prompts show the Deck glyphs (D-222).
4. The binding has its decision row with its license (D-462, G-13).
5. A controller type that the SDK does not know falls back to the rule of PR-61 (OQ-107).

**Review focus.**

- Godot reads the gamepad in every build, so Steam is never a requirement (D-460).
- A remap in the Steam settings can show a wrong button, and the PR states the limit (D-460).
- Phase 6 needs the same binding, so the pick carries that weight (D-466).

**Questions.** OQ-58 and OQ-176.

> *In plain English:* on Steam the game asks which controller a player holds, so the button pictures match. That is the only thing Steam tells it.

### 7.5 PR-39: the Deck verification pass

Area files: `area-release.md` section 7.10, `area-ui-input.md` section 7.10.

**Scope.**

- The walk of the Steam Deck checklist to the rating Verified (D-459).
- The four checks of Verified: the glyphs, the default bindings, the text height, and the on-screen keyboard (the external facts of `docs/design.md`).
- The check of suspend and resume, and of the 1x frame (D-85, D-92, D-228).
- The fixes that the walk finds, inside this PR.

**Out of scope.**

- The Steamworks controller type call, which PR-78 adds (D-460).
- The Steam publish (PR-40).

**Exit tests.**

1. No text falls below 9 pixels at 1280 by 800 (D-459).
2. The default bindings play the whole game on the Deck (D-459).
3. Under Steam, the game shows the Deck glyph set for the Deck controller, through the call of PR-78 (D-222, D-565).
4. A suspend and a resume leave the run in the same state (D-85).
5. The frame draws at 1x on the Deck, and a screen test locks it (D-228).
6. The game holds no text entry, so the keyboard rule does not apply (D-459).

**Review focus.**

- The 16-pixel font and the 60-frame target clear the text and frame rules (D-263, G-19).
- Valve tests the native Linux build first (D-458).
- PR-78 lands first, so this PR proves the Deck glyphs under Steam (D-460, D-565).
- This PR proves each check. Valve grants the rating later, after the owner requests the review (D-565).

**Questions.** None. OQ-176 blocks the controller type of PR-78.

> *In plain English:* the handheld has a checklist, and this pass walks it. The game meets most of it, and the button pictures are the real work.

### 7.6 PR-79: the macOS signature and the notarization

Area file: `area-release.md` section 7.12.

**Scope.**

- The signature and the notarization of the universal macOS export, on the hosted macOS leg (D-455, D-482, D-553).
- The change to the export job of PR-54, from this PR on (D-503).
- The place of the signing identity and the notarization secrets, from the answer of OQ-177.

**Out of scope.**

- A Windows code signature, which the plan accepts as absent (D-463).
- Any certificate for an individual, because no personal name goes on a file (D-4, D-450, D-463).

**Exit tests.**

1. The macOS leg signs and notarizes its export on each merge to `main`.
2. A fresh Mac runs the notarized build with no Open Anyway step (D-455).
3. A failed notarization fails the job with the reason (T-2).
4. No secret reaches a log line (T-2).
5. Builds before this PR keep their runbook steps, and the runbook says so (D-463).

**Review focus.**

- The answer of OQ-177 keeps each secret out of the repository.
- Steam requires a notarized macOS app, so this PR blocks PR-40 (F-32).
- The Windows risk stays accepted where Smart App Control is on (D-463).

**Questions.** OQ-177.

> *In plain English:* a Mac refuses to run a program that Apple did not check. This step sends each Mac build to Apple for that check.

### 7.7 The owner step: the first trailer

Owner work, before PR-40. Area file: `area-release.md` section 7.9.

**Scope.**

- A session writes the shot list, and the owner approves it (D-476, OQ-175).
- The capture of PR-74 writes the frames and the audio of each shot (D-476, D-551).
- The owner cuts the trailer in a video editor (D-476).
- The trailer comes before the prologue demo (D-476).

**Out of scope.**

- No code. The capture already exists (PR-74).
- The store art and the screenshots, which PR-76 holds (D-550).

**Exit tests.**

1. The shot list names each shot with the run record that takes it.
2. Each shot repeats exactly on a second capture (T-7, D-551).
3. The trailer meets the Steam limits: 1920 by 1080 at 30 or 60 frames each second.
4. The cost model counts the owner time of the cut (D-476).

**Review focus.** No PR for the cut. The shot list comes to the owner in the PR that writes it (G-25).

**Questions.** OQ-175.

> *In plain English:* the game records real play as frames on disk, and the owner cuts those frames into a trailer for the shop page.

### 7.8 PR-40: the Steam publish

Area file: `area-release.md` section 7.13.

**Scope.**

- The publish of the prologue as the demo app "The Thing Below: Prologue" (D-143, D-478, D-553).
- Auto-Cloud on the save folder, with no cloud code in the game (D-461, D-465).
- The test of the conflict rule of D-93 on two machines, with its result in the PR (D-461).
- The test that the shared cloud of the demo works with Auto-Cloud (D-143, D-461).
- The Linux build on the Steam Linux Runtime that the Godot export needs (D-458).
- The check of whether the demo app needs a Steam Direct fee of its own (D-478).
- What the demo build changes from the full build, from the answer of OQ-178.
- The end of the GitHub Releases, where the releases that exist stay up (D-470).

**Out of scope.**

- The achievements, which the full game gets in Phase 6 (D-466).
- The Next Fest, which the full game enters once (D-472).
- The store page and the name checks, which Gate 2 held (D-471).

**Exit tests.**

1. The Deck runs the Steam demo from the store page.
2. A save follows the player between two machines through Auto-Cloud (D-461).
3. A cloud conflict follows D-93, and the PR reports what Steam did (D-461).
4. The Linux build runs under the Steam Linux Runtime of D-458.
5. The demo app carries the capsules that PR-76 drew, which mark it as a demo (D-475, D-478).
6. The PR states whether the demo needed a fee of its own (D-478).

**Review focus.**

- A demo needs the store page of the full game visible as Coming Soon, which Gate 2 gave it (D-471).
- Valve recommends Steam Linux Runtime 4.0 for a new native Linux game (D-458).
- The GitHub Releases stop, and the runbook says so (D-470).

**Questions.** OQ-178.

> *In plain English:* this is the step where a stranger can find the game on Steam and play it for free. Their save follows them between machines.

### 7.9 The owner step: the Deck compatibility review

Owner work, after PR-40. Area file: `area-release.md` section 7.10.

**Scope.**

- The owner requests the Deck compatibility review from Valve for the demo app (D-565).
- The request comes after PR-40, because Valve reviews a game that is on Steam.

**Out of scope.**

- No code. PR-39 already proved each check (D-565).
- The rating itself, which Valve grants after Gate 5.

**Exit tests.**

1. The owner request reaches Valve, and the date enters the cost model.
2. The rating that Valve gives enters `docs/design.md` with its date, after Gate 5.

**Review focus.** No PR and no review. The session records the request and the rating.

**Questions.** None.

> *In plain English:* the handheld rating comes from Valve, not from us. After the game is on Steam, the owner asks Valve to test it.

### 7.10 PR-32: retired

PR-32 held the fallback pass of the first plan, which has no purpose after D-98. No later item takes the id (G-10). This entry exists so that a reader of the sequence finds the gap and its reason.

> *In plain English:* one planned change no longer exists, and its number stays empty forever, so old notes never point at new work.

### 7.11 Gate 5: the first release

**The gate.** Gate 5 passes when every line holds:

1. A fresh machine of each system runs the tagged build from GitHub (D-53, D-463).
2. The Deck runs the Steam demo from the store page (D-459).
3. The macOS build opens with no Open Anyway step (D-455).
4. PR-39 proves each check of the Deck checklist (D-459, D-565).
5. A save follows the player between two machines (D-461).
6. Every job of the PR gate is green on every leg (D-481).

**After the gate.** Valve answers the Deck compatibility review that the owner requested, and the rating follows (D-565). Phase 6 stays parked (D-456, D-466, D-472). Before the first content of a paid region, the repository goes private (D-456). A session first checks that gitar, the review gate, and every workflow work on a private repository.

> *In plain English:* the free part of the game is out. Anybody can download it, and anybody on Steam can play it on the handheld.
## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). Phase 5 holds this order:

1. PR-31: the release workflow and the first GitHub Release of the prologue.
2. PR-33: the title screen, the settings, the version line, and the credits screen.
3. Owner: join the Apple Developer Program (D-455, D-565).
4. PR-78: the Steamworks binding, the start, and the controller type (D-553).
5. PR-39: the Steam Deck verification pass, after PR-78 (D-565).
6. PR-79: the signature and the notarization of the macOS build (D-553).
7. Owner and a session: the shot list and the cut of the first trailer (D-476).
8. PR-40: Auto-Cloud, the demo app, and the Linux runtime.
9. Owner: request the Deck compatibility review from Valve (D-565).
10. **← GATE 5 (first release).** Section 7.11 holds each line.
11. Valve answers the review, and Phase 6 stays parked (D-456, D-466, D-472, D-565).

Phase 6 has no phase file yet. Each later region repeats Phase 4 with a roadmap of its own (D-144).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 5, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-57 | The studio name | PR-33 |
| OQ-58 | The C# binding for Steamworks | PR-78 |
| OQ-90 | Where the studio mark shows | PR-33 |
| OQ-168 | Where the game version lives in the build | PR-31 |
| OQ-169 | Where the release notes live | PR-31 |
| OQ-170 | What the title menu holds | PR-33 |
| OQ-175 | The shot list of the first trailer | The trailer |
| OQ-176 | How the game reads the controller type | PR-78 |
| OQ-177 | Where the signing identity and the secrets live | PR-79 |
| OQ-178 | What the demo build changes from the full build | PR-40 |

No open question blocks this file.
