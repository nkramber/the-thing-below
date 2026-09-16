# The Thing Below: Design and Roadmap

Status: **design document v2, pre-production.** This file supersedes `docs/archive/design-v1-terminal-2026-09-12.md`, the terminal plan. Its source is the decision register `docs/decisions.md`, entries D-1 onward. The owner recorded D-1 to D-25 in the repository interview and D-26 to D-65 in the roadmap interview, both on 2026-09-12. D-66 to D-98 came in the pivot interviews, and D-99 to D-122 in the engine interview. D-123 to D-397 came in the world-building interview, which D-142 widened to a full roadmap, and D-398 onward in later sessions.

Nothing in this file is code. Each plan item ships as one pull request.

2026-09-12 pivot pass: v2 refutes the terminal premise of v1 twice. D-78 moved the game out of the terminal into a window with a terminal look. D-98 ended the terminal look and made the game sprite-based. D-99 replaced Rust with Godot 4 and C#. The v1 file stays in the archive unchanged. The ids of v1 stand: an item that keeps its purpose keeps its number, PR-32 is retired, and new items start at PR-34 (G-10).

2026-09-12 world and full-plan pass: the world-building interview set the setting (D-123 to D-159) in `docs/world/`. Two owner instructions widened the PR. D-139 plans 2D effects from the start, and D-142 puts a full roadmap before PR-1. Region one became a free prologue, a Steam demo of the full game (D-133, D-143). F-21 and F-22 record two faults of the interview options, and F-23 records a gate that headless CI cannot run. D-227 and D-228 replaced the 640 by 360 frame with 1280 by 800 and 32-pixel tiles.

2026-09-13 critic pass: the design-critic agent read the plan after the frame change and found 13 defects. F-25 to F-27 record them, and D-255 to D-258 close four.

2026-09-13 cast pass: the cast block of the world-building interview replaced the job system (D-267 to D-299). Characters gain abilities from lessons, the rites and drills that any character equips. Each character has a main aptitude and a hidden side aptitude. D-277 ended every port from another repository, so each tool is new work. PR-22 is retired, and PR-42 takes the lessons of region one in Phase 4 (D-304).

2026-09-13 second critic pass: the design-critic agent read the plan after the job system change and found 14 defects. F-28 records them. D-301 to D-308 settle C-1, C-3, C-5, C-7, C-10, and C-14. D-309, D-351, D-356, D-363, and D-375 answer OQ-48, OQ-49, OQ-38, OQ-50, and OQ-51, so no question of the pass remains.

2026-09-13 arc pass: the arc block set the story of region one in `docs/world/arc.md` (D-309 onward). Elio is the one death in the cast, after region one. The relationship value and the faction reputation left the game, and a choice is a story flag (D-328, D-329). F-29 records a count of PR ids that the second visit to the cells settles.

2026-09-13 systems pass: the systems block settled the lessons, the battle rules, the items, the statuses, and the law in play (D-356 to D-397). The owner moved the start of the game to the village of Marrek (D-368 to D-373).

2026-09-13 PR #2: the plan through the systems block goes to review as PR #2 (D-398). A docs PR that adds a decision takes the review of the other provider (D-401). The rename, the move, and one docs PR each for audio, release, and the roadmaps follow the merge (D-399, D-400).

2026-09-14 sample and rename: PR #2 merged, and PR #3 saved a sample of the cast sprites (D-402 to D-404). PR #3 also removed the 16 by 16 test sprites (D-405 to D-407). A name search found no conflict that stops the name, and the GitHub repository took the name the-thing-below (D-408 to D-410). An audit of every document comes before the rename PR and the move (D-411, F-30).

2026-09-14 rename pass: PR #4 merged the audit (D-411, F-30). The rename PR puts the tentative name The Thing Below in the live documents and the names of D-217 in the commands. The dated records keep the working title. The move to the external SSD follows the merge (D-400).

2026-09-14 move pass: PR #5 merged the rename (D-217). The checkout moved to the external SSD, and the owner deleted the old checkout, so the runbook `docs/runbooks/rename-and-move.md` is complete (D-400). The docs PRs for audio, release, and the roadmaps follow, one PR each (D-399).

2026-09-14 audio pass: the audio block set the music, the sound effects, and the audio settings (D-412 to D-435, D-438, D-439, D-443, D-444). In the same PR, the owner removed the day clock, and the story now sets the time of day of each map (D-436 to D-442, D-445). F-31 records the size of the rendered audio.

2026-09-14 reset pass: a Codex review found no defect in PR #7, and the owner merged it. Before a context reset, a docs PR brings every document current. The story places the wrong things at any time of day (D-446), and OQ-56 holds the levers for an earlier playable build.

2026-09-14 release pass: the release block set the builds, the versions, the Steam work, and the store (D-447 onward). The export job starts with PR-7, and the store page goes public at Gate 2 (D-449, D-471). F-32 records the Apple fee that a macOS build on Steam needs. F-33 records five gaps that the block closed, and F-34 records the screenshot format of Steam. The same PR sets two aspect ratios, 16:10 and 16:9, and four supported targets (D-479 to D-482).

2026-09-14 roadmaps shape pass: the review of the other provider found no defect in PR #9, and the owner merged it. OQ-56 closed with no change to the sequence (D-483). The owner split the roadmaps work into three docs PRs (D-484 to D-490). PR #10 holds the shape and brings every document current. PR #11 holds twelve area files, five phase files, and the rebuild of sections 7 and 8, and PR #12 holds the next design-critic pass.

2026-09-14 roadmaps pass: PR #11 writes the focused roadmaps. The core area moves saves, crash files, and log files from PR-6 to PR-43 and PR-44, and PR-45 creates the debug assembly (D-491, D-492). The run record holds intents, a storage project holds the file code, and the content hash covers the rule files (D-493 to D-495). F-35 and F-36 record two defaults of .NET that break Core rules. Sections 7 and 8 change in the rebuild at the end of PR #11 (D-488).

2026-09-14 tools area pass: the tools area gives det-lint, the PNG code, the normal maps, and the night gate PRs of their own (D-496). Four tools with no PR take ids, PR-50 to PR-53 (D-497). The `det-lint` command reads code through the Roslyn compiler library, and Game shows player text through one helper (D-498, D-499). Two checks that start only from `main` prove themselves in Tests first (D-500). An edge file keeps tile edges out of the content hash, and tools with compared output use integer math (D-501, D-502). F-37 to F-39 record two GitHub triggers, two float facts, and the default string order of .NET.

2026-09-14 CI area pass: the export job takes PR-54, right before PR-7, and PR-1 publishes the coverage report (D-503, D-506). The replay-identity job compares each leg with a committed identity file, and the Game assembly carries the content files (D-504, D-508). Bot runs play on all three CI legs, and a night plays fourteen thousand runs with its record as a run artifact (D-505, D-507, D-509). A night on the head of a PR passes that PR, and a docs-only PR passes the night gate (D-510, D-513). Workflows use the actions of GitHub alone with SHA pins, and a PR that changes the export runs the export job (D-511, D-512). F-40 to F-43 record the .NET test modes, four GitHub rules, two Godot export facts, and a night gate that blocked its own fix.

2026-09-14 art area pass: the owner sees each art batch as review sheets that `gh` attaches to the PR description (D-514). A drawing file is JSON with rows as strings, and a large picture places drawn pieces (D-515, D-516). PR-55 builds large pictures right before PR-10 (D-518). Core holds the record of every content file, and an art file names the content ids that it draws (D-517, D-519). F-44 records the size of a full-screen grid, and F-45 records three Godot defaults that meet the pixel art.

2026-09-15 effects area pass: every effect PR lands before PR-17, and PR-56 is the first PR that draws light (D-520). PR-48 lands right before it, with a review sheet of fixed light angles (D-521). No rule waits for an effect, and a wait intent ends each wait of the world (D-522). An effect budget from the Deck test holds each place inside 60 frames per second (D-523). F-46 records three silent failures of 2D light in Godot, and F-47 records a glow that can reach a lit sprite.

2026-09-16 UI and input area pass: PR-61 builds the UI base right before PR-7 (D-524). PR-62 builds the menus before PR-12, and PR-63 the settings before PR-57 (D-525, D-526). One JSON file holds the UI style, and Game builds the Godot `Theme` from it (D-527). F-48 records that Godot has no fit like the fit of D-232, and F-49 records three font defaults that meet the pixel font.

2026-09-16 exploration area pass: one rule file holds each map (D-528). PR-16 splits, and PR-64 takes the traps, the hazards, and the statuses on the map (D-529). The shop leaves PR-14 for PR-65 (D-530). One run holds the map and the battle, and no map system ticks during a fight (D-531). F-51 records four Godot defaults of the tile map. F-52 records a camera that centers a small map, with no page of the docs behind it.

2026-09-16 battle area pass: Core resolves each action at once, and Game plays the events that it emits (D-532). PR-9 splits, and PR-66 takes the eight elements and the ten statuses (D-533). The evaluator simulates each action and the strongest reply (D-534), and a group file for each region holds each enemy group (D-535). F-53 records that the evaluator of D-534 has no measurement of its cost.

2026-09-16 progression area pass: PR-12 splits, and PR-67 takes the character level, the experience, and MP (D-536). Each character carries its own stat curve in content, which closes the gap that F-54 records (D-537). The quest state of PR-19 holds every personal task (D-538), and each lesson lists its named forms with their point totals (D-539).

2026-09-16 story area pass: Core runs each scene, and Game draws it and sends a wait intent (D-540). PR-36 splits, and PR-68 takes the scene format and the scene runner (D-541). A story flag is a name that is on or off, and one condition form serves every reader (D-542, D-543). PR-68 also takes the flag set, and PR-50 lands right after it (D-544, D-545). F-55 records the gap that put the flags one phase after their first reader.

2026-09-16 audio area pass: three PRs take the audio work that PR-38 leaves, PR-69 to PR-71 (D-546). The build renders each track into the Game assembly, beside the content files (D-547). An audio file names the content ids that it serves, and a rule file names no track (D-548). PR-72 and PR-73 hold the music and the sounds of region one (D-549). F-56 records a Godot audio call that fails in silence and a position that is not exact.

External facts, each with the date of its check:

- The GitHub repository `nkramber/the-thing-below` is public. Its name changed from the working title on 2026-09-14 (D-410). Source: `gh repo view`, run 2026-09-14.
- Branch protection with required status checks is free on a public repository. Source: docs.github.com, "About protected branches", read 2026-09-12.
- Godot 4.7.2 is the current release for both editions, dated 2026-08-18, and the .NET LTS pin is .NET 10. Source: the what-you-carry design header, verified there on 2026-09-07. PR-1 verifies both again on the Godot download page.
- The development machine has .NET 10.0.400 and Godot 4.7.2 .NET at `/Applications/Godot_mono.app`. Source: `dotnet --version` and the what-you-carry runbook, 2026-09-12.
- The Steam Deck screen is 1280 by 800, a 16 to 10 aspect. Source: the Steam Deck tech specs page, read 2026-09-12.
- A Steam demo is a separate app ID linked to the full game. It can launch while the store page of the full game says "Coming Soon". Source: Steamworks, "Demos", read 2026-09-12.
- A demo save can move to the cloud storage of the full game through the `Shared cloud APP ID` field. Source: Steamworks, "Demos" and "Steam Cloud", read 2026-09-12. No page states whether a demo needs its own Steam Direct fee.
- The Godot 4.7 renderer table marks 2D rendering features as supported on Forward+, Mobile, and Compatibility. Compatibility lacks 2D MSAA, particle trails, and particle SDF collision. Source: Godot docs, "Overview of renderers", read 2026-09-12.
- In Godot, `--headless` "disables all rendering code". Source: godot-proposals issue 5790, open, read 2026-09-12.
- Godot's own CI runs the engine under `xvfb-run` with `--rendering-driver opengl3`. Source: `.github/actions/godot-project-test/action.yml` in the Godot repository, read 2026-09-12.
- In Movie Maker mode, faster hardware renders sooner, "but the visual output remains identical", and "the window size is clamped by your display's resolution". Source: Godot docs, "Creating movies", read 2026-09-12.
- HDR for 2D works "when using the Forward+ and Mobile rendering methods", and "When using the Compatibility rendering method, glow uses a different implementation". Source: Godot docs, "Environment and post-processing", read 2026-09-12.
- No Steam game carries the name "The Thing Below" (D-408). The store search API gave 0 results for "the thing below", "thing below", "the thing beneath", and "things below". The store search page for "the thing below" listed 50 titles, and none holds the phrase. Source: `https://store.steampowered.com/api/storesearch/?term=the+thing+below&l=english&cc=US`, one query per term, and `https://store.steampowered.com/search/?term=the+thing+below`, run 2026-09-14.
- A free horror jam game on itch.io has the close title "The Thing Beneath". Its game page says "The Thing Beneath" 7 times and never "The Thing Below". Its devlog of August 2026 says "The Thing Below" once and "The Thing Beneath" 12 times. The itch.io search for "the thing below" lists no game with that exact title. Source: `https://studio-laaya.itch.io/the-thing-beneath`, `https://studio-laaya.itch.io/the-thing-beneath/devlog/1616272/the-thing-beneath-directors-cut`, and `https://itch.io/search?q=the+thing+below`, read 2026-09-14.
- No USPTO record has the phrase in its word mark. A `match_phrase` query on the field `WM` gave 0 hits for "the thing below", "thing below", "thing beneath", "things below", and "thing from below". As a control, the same query gave 11 hits for "below deck" and 1,125 for "below". Each query was a POST of the body `{"query": {"match_phrase": {"WM": "<term>"}}, "size": 3}` to the search service behind the USPTO Trademark Search. The service has no public documentation, and the controls show that `WM` holds the word mark. Source: `https://tmsearch.uspto.gov/prod-stage-v1-0-0/tmsearch`, run 2026-09-14.
- The EU and WIPO registers have no script check yet, so PR-40 checks them before the store page goes public (D-408). The EUIPO eSearch and TMview pages give a script no search results, and the WIPO Global Brand Database answers with a CAPTCHA. Source: `https://euipo.europa.eu/eSearch/`, `https://www.tmdn.org/tmview/`, and `https://branddb.wipo.int/en/`, read 2026-09-14.
- "The Thing Below" is also the title of a horror film of 2004 by Jim Wynorski. Source: `https://en.wikipedia.org/wiki/The_Thing_Below`, read 2026-09-14.
- Since 2019-10-14, Steam requires Apple notarization and a 64-bit build for every new macOS app. The same page states no rule on Windows code signing and names no ARM build. Source: `https://partner.steamgames.com/doc/store/application/platforms`, read 2026-09-14.
- The Apple Developer Program costs 99 USD a year, and Mac notarization needs the paid membership. Source: `https://developer.apple.com/help/account/membership/program-enrollment` and `https://developer.apple.com/support/compare-memberships/`, read 2026-09-14.
- On macOS Sequoia and later, a Control-click no longer opens an app that Apple did not notarize. The user opens it with Open Anyway in Privacy & Security. Source: `https://developer.apple.com/news/?id=saqachfa` and `https://support.apple.com/en-us/102445`, read 2026-09-14.
- An unsigned Windows file from the internet shows "Windows protected your PC", and the user must choose "Run anyway". A signed file still warns until it gains reputation, and "EV certificates no longer bypass SmartScreen". Where Smart App Control is on, it blocks unsigned files with no positive reputation. Source: `https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation`, read 2026-09-14.
- Artifact Signing, the managed signing service of Microsoft, starts at 9.99 USD a month. An individual must live in the United States or Canada, and the certificate shows the legal name with the city, state or province, and country. Source: `https://learn.microsoft.com/en-us/azure/artifact-signing/quickstart` and the SmartScreen page above, read 2026-09-14.
- Standard GitHub-hosted runners are free on a public repository, and the standard macOS runners run on arm64. A private repository on GitHub Free gets 2,000 minutes a month. The list rates are 0.006 USD a minute on Linux, 0.010 on Windows, and 0.062 on macOS. Source: `https://docs.github.com/en/billing/concepts/product-billing/github-actions`, `https://docs.github.com/en/billing/reference/actions-runner-pricing`, and `https://docs.github.com/en/actions/reference/runners/github-hosted-runners`, read 2026-09-14.
- In a private repository, protected branches need GitHub Pro, Team, or Enterprise. Source: `https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches`, read 2026-09-14.
- Each file in a GitHub release must be under 2 GiB, and a release has no limit on total size or bandwidth. Source: `https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases`, read 2026-09-14.
- A game made with Godot must make the Godot MIT license text available to the player. A credits screen or a file that ships with the game can carry it. A copy of an OFL font must carry its copyright notice and the license. Source: `https://docs.godotengine.org/en/stable/about/complying_with_licenses.html` and `https://openfontlicense.org/open-font-license-official-text/`, read 2026-09-14.
- Godot 4.7.2 is still the current stable release. Its .NET editor ships for Windows and Linux on x86_64 and arm64, and for macOS as a universal build. C# projects export to the desktop systems and not to the web. Source: `https://godotengine.org/download/archive/4.7.2-stable/` and `https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html`, read 2026-09-14.
- A macOS export can target `x86_64`, `arm64`, or `universal`, but "Official export templates include `universal` binaries only". A universal build runs on Intel Macs and on Apple silicon. Source: `https://docs.godotengine.org/en/stable/classes/class_editorexportplatformmacos.html` and `https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_macos.html`, read 2026-09-14.
- A CI export needs `--headless`, and `--export-release` exports a preset. The docs name no command-line option that installs export templates. Source: `https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html`, read 2026-09-14.
- `Debug.Assert` carries `[Conditional("DEBUG")]`, and the Godot .NET SDK defines `DEBUG` for `ExportDebug` alone, so a release export drops every `Debug.Assert`. Source: `Sdk.targets` of Godot.NET.Sdk at the tag `4.7.2-stable`, and `https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.assert`, read 2026-09-14.
- The project setting `application/boot_splash/show_image` hides the Godot logo, and no Godot license page asks for the splash. With `use_custom_user_dir`, the user folder is `%APPDATA%\<name>`, `~/Library/Application Support/<name>`, or `~/.local/share/<name>`. Source: `ProjectSettings.xml` of Godot at `4.7.2-stable`, and `https://docs.godotengine.org/en/stable/tutorials/io/data_paths.html`, read 2026-09-14.
- Movie Maker writes an OGV file in editor builds alone, an AVI file with MJPEG video, or a PNG sequence with a WAV file. Source: the Godot docs, "Creating movies", read 2026-09-14.
- A Steam demo that comes before the full game needs the store page of the full game visible as Coming Soon. Valve recommends no achievements in a demo, and the capsules of a demo must mark it as a demo. No page that the session read states a fee for a demo app. Source: `https://partner.steamgames.com/doc/store/application/demos` and `https://partner.steamgames.com/doc/gettingstarted/appfee`, read 2026-09-14.
- The Steam Direct fee is 100 USD per app, and Valve returns it after 1,000 USD of adjusted gross revenue. A wait of 30 days follows the payment before a release. Source: `https://partner.steamgames.com/doc/gettingstarted/appfee` and `https://partner.steamgames.com/doc/gettingstarted/onboarding`, read 2026-09-14.
- A new product needs a Coming Soon page for at least two weeks before release. Valve recommends the page "as soon as you are ready to start talking publicly about your game". A store page review and a build review each take 3 to 5 business days, and Valve asks for 7. Source: `https://partner.steamgames.com/doc/store/coming_soon` and `https://partner.steamgames.com/doc/store/review_process`, read 2026-09-14.
- The content survey comes "Before submitting your store page and product build for review". It asks about "Pre-Generated" AI content that ships with the game, and efficiency gains from AI tools are not its focus. After approval, some answers change only through Steam Support. Source: `https://partner.steamgames.com/doc/gettingstarted/contentsurvey`, read 2026-09-14.
- A game enters one Next Fest alone. Its store page must be public, its demo must be playable by the start, and the game must stay unreleased until the fest ends. The rules exclude "a prologue, preview, or short-form version of an existing game already released on Steam". The next editions run 2027-02-22 to 2027-03-01 and 2027-06-14 to 2027-06-21. Source: `https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest` and its edition pages, read 2026-09-14.
- Steam Playtest is free, and it runs as a separate app with its own reviews, wishlists, and playtime. Source: `https://partner.steamgames.com/doc/features/playtest`, read 2026-09-14.
- For the Steam Deck rating Verified, glyphs must match the input in use. No text falls below 9 pixels high at 1280 by 800. Valve tests a native Linux build first, and it tests the Windows build under Proton only when the Linux build fails. Source: `https://partner.steamgames.com/doc/steamdeck/compat`, read 2026-09-14.
- Valve recommends Steam Linux Runtime 4.0 for new native Linux games, and a developer picks the runtime on the Steamworks site. Source: `https://github.com/ValveSoftware/steam-runtime/blob/master/README.md`, read 2026-09-14.
- Under Steam, gamepad input "will appear in your game as regular Xbox controller input". Valve recommends the Steam Input API for glyphs, and a game without it can "detect the device type via our API". Source: `https://partner.steamgames.com/doc/features/steam_controller/getting_started_for_devs` and the Deck compatibility page, read 2026-09-14.
- Steam Auto-Cloud syncs listed folders "when the application launches and exits", with no code in the game. The page does not say how Steam settles a conflict. Its roots include WinAppDataRoaming, MacAppSupport, and LinuxXdgDataHome. Source: `https://partner.steamgames.com/doc/features/cloud`, read 2026-09-14.
- Capsule art shows only game art, the game name, and an official subtitle. A store page needs at least five screenshots of play, at 1920 by 1080 or larger, in 16:9. Source: `https://partner.steamgames.com/doc/store/assets/rules` and `https://partner.steamgames.com/doc/store/assets/standard`, read 2026-09-14.
- A trailer can have up to 1920 by 1080 pixels at 30 or 60 frames per second. The file is .mov, .wmv, or .mp4, and Valve prefers H.264 with AAC. Source: `https://partner.steamgames.com/doc/store/trailer`, read 2026-09-14.
- On 2026-09-14, Steamworks.NET last released on 2026-08-02 and Facepunch.Steamworks on 2026-04-23, both under MIT, and NuGet lags by years for both. GodotSteam has no C# version of its own, and it moved to Codeberg on 2026-09-04. Source: the GitHub releases of each project, `https://www.nuget.org/`, and `https://codeberg.org/GodotSteam/GodotSteam`, read 2026-09-14.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

The Thing Below, a tentative name (D-215), is a dark fantasy role-playing game in 32-pixel sprites at 1280 by 800 (D-27, D-107, D-228). A fixed cast (D-33, D-299) travels between hubs of every shape, a castle town, a cave community, a boat, an airship (D-28). Between the hubs lie hand-authored dungeons with visible enemies, traps, puzzles, and secrets (D-37, D-39, D-41). Three fight at a time on a visible timeline where speed decides the order (D-29, D-31). Any character equips lessons, the rites and drills that give abilities, and each character does one kind of ability best (D-272, D-274, D-278).

Combat is hard because enemies think and resources run out (D-35), and a fallen character stays down until a hub (D-36). Decisions close routes, lose allies outside the cast, and change hubs (D-40, D-301).

The game runs on Godot 4 with C# (D-99). The simulation lives in an engine-free Core library that replays any run from a seed and an input record (D-100, T-7). Sprites, tiles, and portraits are text grids in content that a tool renders into an atlas (D-107). A full CRT shader sits over the frame with a toggle (D-105, D-120). Particles, 2D light, and shaders enter the plan from the start (D-139).

The goal is a Steam release, and the Steam Deck is the readability and performance floor (D-85, D-92). The game supports Windows and Linux on x86_64, macOS on Apple silicon, and the Steam Deck, and nothing else (D-481).

A full roadmap comes before any code (D-142). The plan puts the foundations first, because every later system depends on them. Those are a deterministic core, a run record with replay, the content loader, the atlas tool, and the document gates. The first playable is the village, one hub, and one dungeon, with lessons and a shop (D-51, D-268, D-362, D-369). The owner judges feel there, on the desktop and on the Deck.

The story systems come third, because they need the loop. Region one, two hubs and four dungeons in one arc, is the first release (D-56). It ships free, as a Steam demo of the full game (D-133, D-143). Every plotline converges at the end of the game (D-131). Five gated phases hold that order.

## 2. Lessons learned (carry into every PR)

From the two reference repositories, decktome and what-you-carry:

1. **L-1. One concern per PR.** A bundled PR froze behind one review objection.
2. **L-2. Commit the evidence.** An uncommitted script hid its gaps from reviewers.
3. **L-3. Audit design claims before you trust them.** A first draft carried refutable claims, and an audit found nine defects in one pass.
4. **L-4. Verify a platform claim before you build on it.** A documented cap that nobody enforced, and a machine that lost its power margin once a new target arrived.
5. **L-5. A plan entry is a claim, and it ages.** A fix shape survived three weeks unexamined because it read as one `if` statement.
6. **L-6. A new decision can remove the premise of an old one.** Re-check the old one when a related answer arrives.
7. **L-7. Ask about what the document does not say.** The gap in a design doc is where the next contradiction lives.
8. **L-8. Two answers can conflict when neither one is wrong.** Quote both and settle it at once.
9. **L-9. An exploit hides in every convenience.** A resume feature became a free heal.
10. **L-10. One language, one format, one term.** A second tool language, a second content format, and a borrowed skill with foreign names each cost a correction PR.
11. **L-11. A gate that names an absent check cannot pass.** Name the PR that creates each check, and a PR that creates a check passes it (G-16).
12. **L-12. Two writers need a serialization point.** Two providers picked the same session number on the same day. Fetch and re-read before the handoff commit (D-18).

From the roadmap interview of 2026-09-12:

13. **L-13. A recommendation carries its reason, and the reason can be the wrong one.** D-39 took seeded variation for replay value. D-46 said replay has no value. D-47 dropped the variation (F-6).
14. **L-14. Ask the medium question first.** The first interview asked about the language before it asked what the player sees. Two pivots followed in one day (D-78, D-98). The next project asks about the screen before the stack.
15. **L-15. Never claim a tool is absent without a check.** Six files said gitar was not installed. It was (F-12).

## 3. System map

| Component | Project | Reads | Writes | Sensitivity |
|---|---|---|---|---|
| Simulation loop | Core | intent stream, seed, content | state, run record | Total. Every replay depends on it |
| Random streams | Core | seed | numbers | Total. Cross-platform identity |
| Content loader and schemas | Core | JSON files | typed content, content hash | High. A silent default here breaks the economy |
| String table | Core | JSON files | text by id | Medium |
| Run record, replay, and snapshot | Core | state, intents | record bytes, snapshot bytes | Total. The crash report and the save (D-62, D-493) |
| Tile map, movement, and sight | Core | layout content, intents | party position, sight | High. Patrols and ambushes (D-37) |
| Time of day | Core | layout content, story flags | the time of day of each map | Medium. Light, patrols, enemies, and music follow it, and the story sets it (D-193, D-442) |
| Battle and timeline | Core | party, enemies, abilities | turn order, damage, statuses | Total. The design risk (D-29, D-35) |
| Evaluator and profiles | Core | battle state, enemy profile | enemy actions | High. The largest single system (D-65) |
| Lessons, aptitudes, and levels | Core | lesson content, experience | character state | High. The build decision (D-34, D-272, D-274) |
| Gear and items | Core | item content, inventory | equipment state | Medium (D-44, D-45) |
| Story flags, scenes, and quests | Core | scene content, conditions, choices | flags, scene state, quest state, hub state | High. Branches multiply (D-40, D-59, D-329). Core runs each scene and holds its step index, and one condition form serves every reader (D-540, D-542, D-543) |
| Hub services and the region map | Core | hub content, route content, gold | party, saves, position | Medium (D-59, D-113) |
| Debug assembly | Debug assembly | debug intents | Core state, through the seam of D-260 | High. A release build never loads it (D-260) |
| Save, record, crash, and log files | Storage | record bytes, snapshot bytes, crash context, log entries | save files, record files, crash files, log files | High. A torn write loses a save (D-178, D-494) |
| Map scene, battle scene, hub scene, scene runner | Game | Core state, content from the Game assembly, the atlas and its index, large pictures, edge files, string table | screen, intents | Medium. Cosmetic by design (D-106, D-111, D-114, D-501). Player text reaches the screen through the text helper (D-499). The content bytes come from the Game assembly (D-508). Art draws with the Nearest filter, and an art file names the content ids that it draws (D-519, F-45) |
| Dialogue box and portraits | Game | Core scene state, string table | screen, wait intents, choice intents | Medium (D-109). Game draws each scene step and sends a wait intent when it ends (D-540) |
| CRT shader and the frame | Game | settings | screen | Medium. The Deck floor and the two views (D-105, D-228, D-480) |
| Light, particles, glow, and transitions | Game | effect files, light setups, normal maps, the effect budget, Core state | screen, wait intents | Medium. The effect budget of the Deck test holds them inside 60 frames per second, and no rule waits for an effect (D-139, D-160, D-182, D-183, D-522, D-523) |
| Crash file and replay viewer | Game | run record, crash file | screen, and a crash file through Storage | High. The crash report, and a viewer in development builds alone (D-170, D-175, D-494) |
| Audio player | Game | rendered audio from the Game assembly, Core state, audio files, settings | sound | Low. Music by place and time of day, ambience, stings, and the mix (D-413, D-424, D-429, D-435). An audio file names the content ids that it serves, and each stream comes from bytes with a checked return (D-547, D-548, F-56) |
| Steamworks glue | Game | Steam client | controller type for the glyphs | Low. Steam builds alone, with no cloud code (D-460, D-461) |
| Atlas tool, large picture render, normal maps, PNG code, PNG import, audio synthesizer | Tools | drawing files, large pictures, palette, edited PNG files, tracker rows, parameter files | atlas PNG, atlas index, normal-map atlas, drawing files, review sheets, large picture renders, rendered audio, hash list | Medium. The atlas, its index, and the normal-map atlas are committed with a pixel test (D-107, D-184, F-19), and the audio renders at build against its hash list (D-432). Review sheets reach the PR description through `gh`, never git (D-514). Integer math gives the same output on every CI leg (D-502) |
| Map preview, tile edges, screenplay | Tools | map content, edge rules, scene content, string table | preview PNG, edge files, screenplay text | Low. The owner approves maps and scenes from them (D-165, D-173, D-204). An edge file stays outside the content hash, and a test proves that it matches its map (D-501) |
| STE checker, det-lint, review gate, night gate | Tools | source, docs, review records, night records | pass or fail | Gate. det-lint reads types through the Roslyn compiler library (D-498) |
| Headless runner and bots | Tools | policies, seeds | run records | High. The night gate (D-64) |
| CI checks and the night job | CI | source, content, the identity file, night records | pass or fail, a coverage report, night records as run artifacts | Gate. The actions of GitHub alone, with SHA pins, and bot runs on all three legs (D-504, D-505, D-509, D-511) |
| Export and release | CI | merge, release tag, a PR that changes the export | three exports as build artifacts, a smoke session on each, the GitHub Release of a prologue tag | Low (D-53, D-85, D-449, D-457, D-481, D-503, D-512) |

## 4. Cost model (what we pay, what we do not know)

What we pay:

- Owner time: the interviews, the approvals of every text batch (D-57), and the play sign-off of every phase (D-52). The owner also approves every art batch from its review sheets (D-107, D-514), and every music and sound batch by ear (D-433). The owner also cuts each trailer (D-476), and reads the crash emails and the notes of the trusted players (D-469, D-473). Before PR-1, the owner runs the Deck test with its effect budget (D-160, D-523).
- Tokens: two harnesses, Claude Code and Codex, on every PR (D-14, D-17). The amount per PR is unknown until M-1.
- CI: GitHub-hosted minutes on three legs per PR, the bot runs included (D-2, D-481, D-505). Three exports run on each merge and on each PR that changes the export (D-449, D-512). A night plays fourteen thousand bot runs on three legs (D-507). The minutes are free while the repository stays public (D-4). From Phase 6 the repository is private, and minutes past the free quota cost money (D-456). Wall time per PR is unknown until M-2. The build renders the audio, which adds to that time (D-432).
- Purchases: the Steam Direct fee, 100 USD, at Gate 2 (D-85, D-471). The Apple Developer Program costs 99 USD a year from PR-40 on (D-455). GitHub Pro comes before the switch to a private repository, so the required checks stay on `main` (D-456).
- No purchase: no code signing certificate for Windows (D-463), no asset license, and no font fee, because every font is OFL (D-104, D-122). Godot is free.

Measurements that answer the unknowns:

- M-1: tokens per PR from the harness usage reports, over the first ten PRs.
- M-2: CI wall time per PR, per platform, over the first ten PRs.
- M-3: the night run wall time and the crash and softlock counts, over the first seven nights (D-64).
- M-4: turns per encounter and party downs per dungeon by bot policy, on the first dungeon. Binds the resource numbers of D-35.
- M-5: the owner's play time from the first hub to the end of the arc, against D-56.
- M-6: the Deck frame time on the first playable, against 60 frames per second (D-161). The readability of the 16-pixel font and the 32-pixel sprites at 1x, with the CRT on and off (D-92, D-120, D-228).

## 5. Defect and finding register

Status: ✅ done (code merged, or "doc" for a document-only correction) · 🔧 planned (item listed) · ⚠ constraint (binds a pull request) · ❓ needs owner input · ⏸ out of scope · 🅿 parked.

| # | Finding | Date | Status |
|---|---|---|---|
| F-1 | The harness default adds a co-author trailer to commits, and the reminder repeats in every session. D-22 forbids it | 2026-09-12 | ✅ doc. `.claude/settings.json` sets empty strings. The rule sits at the top of `CLAUDE.md` |
| F-2 | The what-you-carry review gate and STE checker are C# tools, and D-1 chose Rust. Neither tool runs here | 2026-09-12 | 🔧 D-10 and D-15. D-99 replaced Rust with C#, and PR-2 and PR-3 write both tools as new code (D-101, D-277). The Python script is the interim checker |
| F-3 | Rust is absent from the development machine | 2026-09-12 | ✅ doc. D-99 removed Rust, and OQ-2 closed with no action |
| F-4 | The repository had no commit, so no branch and no PR could exist | 2026-09-12 | ✅ D-25. The owner made the root commit `6b899dd` with an empty `CLAUDE.md` |
| F-5 | The Python checker applies the 20-word limit to every numbered item, and the C# tool applied it under a Sequence or Procedure heading alone | 2026-09-12 | ⚠ Binds PR-2. The new checker picks one rule, and the skill text follows it (D-277) |
| F-6 | D-39 took seeded dungeon variation on a replay premise, and D-46 removed the premise | 2026-09-12 | ✅ D-47. No variation. L-13 |
| F-7 | D-36 leaves a fallen character down until a hub, and a three-character party (D-31) then fights with two. No decision balances the short-handed party | 2026-09-12 | ⚠ D-58 gives a reserve and a swap at save points. Binds PR-16 and M-4 |
| F-8 | D-42 empties a caster's MP across a dungeon, and no decision gives a job a no-MP action | 2026-09-12 | ⚠ Binds PR-9 and PR-12. D-359 gives every character a basic attack with no MP cost |
| F-9 | D-48 sets the floor at 120 by 40, and a default macOS Terminal window is 80 by 24 | 2026-09-12 | ✅ doc. D-80 superseded D-48, D-103 superseded D-80, D-228 superseded D-103, and D-98 ended the terminal, so PR-7 has no size message |
| F-10 | D-62 puts the run record in the save, and a record grows without bound over 20 to 40 hours (D-30) | 2026-09-12 | ⚠ Binds PR-6: the record format needs a compaction rule, a snapshot plus the inputs since it |
| F-11 | The interim checker read an HTML comment as prose. A fixture comment with a semicolon, a modal, a passive, and 30 words raised four findings. The automated pass of PR #1 found it | 2026-09-12 | ✅ doc. The script removes a one-line comment. ⚠ Binds PR-2: the new checker carries the rule (D-277) |
| F-12 | The session wrote in `CLAUDE.md`, the PR template, the skill, and OQ-1 that gitar was absent, on no evidence. The pass ran on PR #1 within a minute | 2026-09-12 | ✅ doc. D-66. Every claim about a tool needs a check |
| F-13 | The first interview fixed the language before the medium. Two pivots in one day, D-78 and D-98, reopened 30 decisions | 2026-09-12 | ✅ doc. D-99. L-14 |
| F-14 | D-88 chose curvature and bleed, and the SDL2 2D renderer of D-83 ran no shader | 2026-09-12 | ✅ doc. D-91, then D-99 moved the shader to Godot. OQ-19 |
| F-15 | A 16 by 16 sprite did not divide the 10 by 20 text cell of D-82 | 2026-09-12 | ✅ doc. D-103 sets a 16-pixel tile and a 640 by 360 frame. D-228 later sets a 32-pixel tile and a 1280 by 800 frame |
| F-16 | D-7 chose RON, and C# has no RON reader | 2026-09-12 | ✅ doc. D-116, JSON with a schema |
| F-17 | The 32-color palette (D-89) had 13 free colors for eight elements and ten statuses | 2026-09-12 | ✅ doc. D-121 grows it to 48, and D-181 to 64. Binds PR-34 |
| F-18 | The full CRT (D-105) is on by default on the Deck (D-120) before any Deck measurement | 2026-09-12 | ⚠ Binds M-6 and Gate 2: the Deck play measures readability with it on |
| F-19 | D-119 and PR-34 promised an atlas match byte for byte. The compressed bytes depend on the zlib build and the encoder, so the C# tool of PR-34 cannot reproduce them. The automated pass of PR #1 found it | 2026-09-12 | ✅ doc. The match test compares decoded pixels. The interim tool gained `--check`. Binds PR-34 |
| F-20 | The interim atlas tool kept the last of two palette entries with one key, in silence, against T-2. The automated pass of PR #1 found it | 2026-09-12 | ✅ doc. The tool fails on a repeated key. Binds PR-34 to the same rule |
| F-21 | The plan gives each region one story arc (D-56), and the glossary defined an arc as "the story of one region". No text said how an arc relates to the main story of D-28, or what the first release ends on. An interview option read the gap as a faction that falls inside region one, and the owner refuted it | 2026-09-12 | ✅ doc. D-131: every plotline converges at the end of the game. The glossary now defines an arc as one part of the main story. D-133 resolves OQ-26: region one is a free prologue on Steam. The Phase 4 summary and Phase 5 now name the prologue. Binds the arc block of OQ-18 |
| F-22 | The interview options used Final Fantasy Tactics as a template, not a feel. Three recorded answers sit close to its plot devices: unpaid veterans turned bandit (D-127), a hidden power behind the politics (D-128), and church leaders who know the faith is a lie (D-137). The waystones (D-134) risk a fourth: stones that carry the evil | 2026-09-12 | ⚠ D-136 and D-140: keep the shapes, and ban the devices. The list lives in `docs/world/`. Binds every later option of OQ-18 |
| F-23 | The gates of PR-10 and PR-37 need a rendered screen: a screen test of a fixture battle, and two screenshots of the CRT toggle. The smoke job runs Godot with `--headless` on hosted runners (D-117). Godot proposal 5790 says that `--headless` "disables all rendering code", and the Godot docs name no way to capture an image in that mode. what-you-carry met the same wall: its contact sheet needs a window and runs on a desktop alone (its D-306) | 2026-09-12 | ⚠ D-172: a Linux CI job renders under Xvfb with a pinned Mesa, and desktop contact sheets show the real renderer at milestones. Binds the technical and graphics roadmaps, PR-10, and PR-37. Sources: the Godot 4.7 command line page and proposal 5790, read 2026-09-12 |
| F-24 | D-228 doubles the tile size after the art, effect, and light decisions of this interview. Every grid holds four times the pixels: a 32 by 32 frame is 1,024 characters of text, and a party member has about twelve frames plus normal-map overrides (D-184, D-199, D-200). The Deck lights and fills four times the pixels of a 640 by 400 frame | 2026-09-12 | ⚠ Binds the graphics roadmap, the Deck test of D-160 at 1280 by 800, and M-6. The PNG import of D-107 matters more for hand edits |
| F-25 | The design critic of 2026-09-13 found four holes in play and saves. Gate 2 could not reach the two hidden jobs (C-1), a save point gave endless rest (C-2), a quit autosave could trap a run (C-3), and a Core patch would refuse old saves (C-4) | 2026-09-13 | ✅ doc. D-256, D-257, and D-258 close the first three, and D-268 later supersedes D-256. D-259 closes the fourth: a load reads the snapshot |
| F-26 | The critic found gates that cannot pass. No PR created the screen-test job of D-172, the PR-37 gate relied on a headless run that draws nothing, the PR-7 and PR-8 gates met small maps and routes per phase, and the Deck test of D-160 had no sequence step and no failure branch | 2026-09-13 | ✅ doc. PR-41 creates the job with fixed capture and fit tests at 1080 and 1440 rows. The gates of PR-7, PR-8, and PR-37 changed, and section 8 gains the Deck test. D-261: the owner sets a fallback only if the test misses 60 |
| F-27 | The critic found gaps in the records. 24 earlier rows lacked their revision notes, several lines named superseded values, and D-193 disagreed with D-202 on ambient effects. Four choices had no owner: the first turn from behind, the place of systems, audio, and release in the order, effect timings in frames, and D-171 against the rule of no conditional compilation in Core | 2026-09-13 | ✅ doc for the notes and the stale text. D-260, D-262, D-265, and D-266 settle the four choices |
| F-28 | The second critic pass of 2026-09-13 read the plan after the job system change and found 14 defects. A player choice could remove a cast member (C-1), the PR-12 gate needed the tasks of PR-19 (C-2), PR-42 came before its places (C-3), two notes overstated the aptitude count (C-4), and the law split between a license and a stamp (C-5). Stale text and notes stayed (C-6), and session readings had no owner (C-7). No PR drew the lead or wrote the tasks (C-8), a reserve swap gave fresh MP (C-9), and the watcher could stamp rites (C-10). OQ-41 gave the wrong cost of the death (C-11), three cases had no rule (C-12), words clashed (C-13), and the distance rule covers FFT alone (C-14) | 2026-09-13 | ✅ doc for C-2, C-4, C-6, C-8, C-11, and C-13. The session rejected one claim of C-6: D-282 refines D-268 and D-274 and does not revise them. D-301 to D-304 settle C-1, C-3, C-5, and the reading of D-274 in C-7. D-305 and D-306 settle the other two readings of C-7, D-307 settles C-14, and D-308 settles C-10. D-309 and D-351 settle OQ-48 and OQ-49 of C-12. D-356 settles C-9: a swap at a save point can bring fresh MP, and the balance must hold with it. D-363 and D-375 settle OQ-50 and OQ-51, the rest of C-12 |
| F-29 | PR-23 to PR-26 held four ids for dungeons two to four, which are three dungeons. The count came unchanged from v1, and no text said what the fourth id held | 2026-09-13 | ✅ doc. The second visit to the hanging cells (D-327) makes four dungeon builds after the first, and each id names one in the order of play (D-313) |
| F-30 | Before the rename, an audit of every document found stale text in the design, the registers, the world files, the skills, the agent files, and the runbooks. Section 8 still put PR #2 next and a font pick before PR-7. Three rules read the override set as "no code". The design-doc skill swapped two sections, and nine resolved questions lacked the later decisions that changed them | 2026-09-14 | ✅ doc. D-411. One docs PR fixes each item before the rename PR |
| F-31 | D-115 commits every rendered WAV file. With about 25 tracks for region one, at about 21 MB for a two-minute 16-bit stereo WAV at 44.1 kHz, that is over 500 MB, and each change to a track adds a full copy to the history | 2026-09-14 | ✅ doc. D-432: the build renders the audio, and the repository commits a hash of each render. D-443 and D-444 bring the count to about 20 tracks. Binds PR-38 |
| F-32 | The cost model listed the Steam Direct fee alone. Steam requires notarized macOS apps since 2019-10-14, and notarization needs the Apple Developer Program at 99 USD a year | 2026-09-14 | ✅ doc. D-455: the cost model gains the fee, and PR-40 signs and notarizes the macOS build |
| F-33 | The release block found five gaps. D-54 made the repository public before D-133 made the later regions paid (L-6). D-170 named no place for a crash file, and the plan had no credits for the notices that the Godot license and the OFL need. D-62 named a config directory, and the Godot user folder on Linux is a data folder. D-464 added two arm64 builds that no CI leg tested (T-3) | 2026-09-14 | ✅ doc. D-456, D-473, D-467, D-465, and D-474. D-481 later removed the arm64 builds |
| F-34 | Steam needs at least five screenshots at 1920 by 1080 or larger in 16:9, and the frame was 1280 by 800 in 16:10 (D-228). D-229 let no screen see more of the map than the Deck, so a 16:9 screenshot needed bars or a crop | 2026-09-14 | ✅ doc. D-480: the game supports a 16:9 view, and a screenshot at 1920 by 1080 comes straight from play |
| F-35 | Two hash paths of .NET break Core rules. The hash classes "defer to the OS libraries", against G-1. The hash code of a string "is not guaranteed to be stable", and two runs of one program can differ, against T-7. Sources: `https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography` and `https://learn.microsoft.com/en-us/dotnet/api/system.string.gethashcode`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-4 and PR-5: the state hash, the content hash, and the stream split use a hash function that Core holds, and never `GetHashCode` (OQ-61, OQ-62). The det-lint of PR-46 refuses both paths in Core (D-496) |
| F-36 | D-177 puts the content reader on the JSON support of .NET, and the det-lint of PR-46 bans reflection in Core (D-496). "System.Text.Json uses reflection by default". Source: `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-5: the reader runs with no runtime reflection, through generated metadata or a hand reader, and the Core project sets `JsonSerializerIsReflectionEnabledByDefault` to `false` |
| F-37 | GitHub starts `pull_request_target` and `schedule` only from a workflow file on the default branch. The review gate of PR-3 and the night job of PR-49 run on these triggers, so neither check can run on the PR that creates it, against G-16. Source: `https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-500: each PR proves its command in Tests, and G-16 gains a note. ⚠ Binds PR-3 and PR-49 |
| F-38 | Two float facts of .NET meet the tools. A real literal with no suffix is a double, so a scan of words misses a float. The results of double math "might differ slightly by platform", and the CI legs mix x86_64 and Apple silicon. Sources: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types` and `https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-double`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-498: det-lint reads types through the Roslyn compiler library. D-502: a tool whose output a test compares on every leg uses integer math. ⚠ Binds PR-46 and PR-48 |
| F-39 | The default string order of .NET depends on the machine. A `SortedDictionary` with string keys uses `Comparer<T>.Default`, which uses `CompareTo`, a culture-sensitive comparison with the current culture. String sort order also "differs between NLS and ICU", and a new ICU version can change it. `area-core.md` named `SortedDictionary` for a fixed order and named no comparer. Sources: `https://learn.microsoft.com/en-us/dotnet/api/system.string.compareto`, `https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2.-ctor`, and `https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-4 and PR-46: every string order in Core uses an ordinal comparison, and det-lint fails any other string order in Core (G-4, T-7). Section 7.5 of `area-core.md` and the `csharp-conventions` skill state the rule |
| F-40 | The test command of `CLAUDE.md`, `dotnet test TheThingBelow.slnx --no-build --filter "Category!=Smoke"`, works in VSTest mode alone. In MTP mode, a solution needs `--solution`, and "xUnit.net uses `--filter-trait` while MSTest uses `--filter`, and each framework rejects the other's options". The package `xunit.v3` 4.0.1 depends on `xunit.v3.mtp-v2`. Sources: `https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test` and the NuGet API, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-1: OQ-75 picks the mode, and the test commands of `CLAUDE.md`, `AGENTS.md`, and the `csharp-conventions` skill follow the answer |
| F-41 | Four rules of GitHub Actions meet the CI plan. A schedule can come late at "the start of every hour", and "some queued jobs may be dropped". A public repository disables its schedules after 60 days with no activity. A workflow that a path filter skips leaves a required check "Pending". The merge queue serves repositories of an organization, and a personal account owns this repository. Sources: the external facts of `docs/roadmaps/area-ci.md`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-1 and PR-49: OQ-78 holds the required checks on a docs PR, OQ-81 the age of a night gate result, and OQ-82 the time of the night |
| F-42 | Two Godot export facts meet the CI plan. The export code walks `res://`, the project folder, alone, and `content/` lies outside that folder (D-118). No command-line option installs export templates, and the .NET template file holds 1,202,598,411 bytes. Sources: the external facts of `docs/roadmaps/area-ci.md`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-508: the Game assembly carries the content files. ⚠ Binds PR-5 and PR-54: OQ-83 holds how CI gets the templates |
| F-43 | The night gate of G-22 blocked every PR after a failed night: the PR that fixes the night, and a docs PR too. A gate that no PR can pass breaks L-11 | 2026-09-14 | ✅ doc. D-510: a night on the head commit of a PR passes that PR. D-513: a docs-only PR passes the gate. ⚠ Binds PR-49 |
| F-44 | D-205 gives each backdrop three or four layers, and D-480 makes full-screen art cover the 16:9 view, about 1422 by 800 pixels. One grid of that size holds 1,137,600 palette keys, more than four times the size of `docs/decisions.md`. Region one has at least five places with fights (D-244, D-370), so at least 15 layers. D-475 drew the store images as grids too | 2026-09-14 | ✅ doc. D-516: a large picture places drawn pieces. ⚠ Binds PR-55 (D-518) |
| F-45 | Three Godot defaults meet the pixel art. A canvas texture takes the Linear filter by default. The editor of 4.7 writes the stretch mode `canvas_items` into a new project, where "there is no longer a 1:1 correspondence between sprite pixels and screen pixels". `TileSetAtlasSource.create_tile` and `ImageTexture.create_from_image` report a failure in the log alone, and the second returns null. Sources: the external facts of `docs/roadmaps/area-art.md`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-7: the project sets the Nearest filter, and Game checks each such call (T-2). `area-ui-input.md` sets the stretch |
| F-46 | Godot 2D light fails in silence in three ways. A light joins a frame only `if (cl->enabled && cl->texture.is_valid())`, and only the editor warns about a light with no texture. One canvas item takes 15 lights at most, because the loop stops at `light_count == MAX_LIGHTS_PER_ITEM - 1`, and one render takes 256, or 64 on a small GL uniform buffer. Neither loop prints a message, and a `TileMapLayer` draws 256 tiles as one canvas item by default. At the default `height` of 0, a light gives no light to a flat pixel of a normal-mapped sprite, and the tutorial says "increase the Height property". Sources: the external facts of `docs/roadmaps/area-effects.md`, read 2026-09-15 | 2026-09-15 | ⚠ Binds PR-56 and D-523: Game checks the texture of each light at load, the budget test fails more than 15 lights on one canvas item, and each light sets its height (T-2) |
| F-47 | D-188 reads HDR 2D as the way to glow light alone and keep sprites and tiles clean. The canvas shaders add each light with no upper clamp, and HDR 2D keeps values above 1.0. The glow threshold starts at 1.0, and in SDR it "needs to be decreased below 1.0 when using glow in 2D". So a bright light on a pale sprite can glow, and in SDR a bright art pixel can glow too. The Compatibility renderer of the screen tests "uses a different implementation" of glow. Sources: the external facts of `docs/roadmaps/area-effects.md`, read 2026-09-15 | 2026-09-15 | ⚠ Binds PR-59: OQ-102 holds how glow stays off sprites and tiles (D-188) |
| F-48 | D-232 sets the default fit: a whole-number upscale, then a smooth fit to the height of the screen. Godot has no mode that does both. With the integer scale mode, `Window::_update_viewport_size` floors the factor, and the docs say "The remaining space is filled with black bars on all four sides". The stretch mode `canvas_items`, which the editor of 4.7 writes into a new project, gives "no longer a 1:1 correspondence between sprite pixels and screen pixels". A `SubViewport` keeps its own size, because the scale factor of the root window "will not be applied" to it. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61: Game draws the world in a `SubViewport` at 1x and builds the two steps of the fit itself (D-232, D-480, F-45) |
| F-49 | Three font defaults of Godot meet the pixel font of D-263. Godot 4.7.2 has no method that loads a font from bytes, and `FontFile.data` holds the "Contents of the dynamic font source file", so Game sets that member from the bytes of its assembly (D-508). The default `antialiasing` is gray and the default `hinting` is light, and the docs say that a pixel font "should have their subpixel positioning mode set to Disabled", where the default is automatic. Font oversampling is on by default with the stretch mode `canvas_items`. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61: the load from bytes and the font settings, with a test that reads each one back (T-2) |
| F-50 | Five input facts of Godot meet the plan. A change to the input map at run time "is not saved (must be modified manually)", so the game saves each remap itself. The methods of `Input` "are not affected by [method Control.accept_event]", so an intent from a poll sees input that a menu already took, and the replay of D-493 then drifts. A default `ui_*` action "cannot be removed", and only its events change. The dead zone of a new action is 0.2 in the source, and the docs name 0.5, which is the value of the built-in actions alone. `JOY_BUTTON_A` "Corresponds to the bottom action button: Sony Cross, Xbox A, Nintendo B", so one constant needs three glyphs. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61, PR-62, and PR-63: intents from events alone, a glyph set for each device, a saved remap, and one dead zone value (D-222, D-493) |
| F-51 | Four Godot defaults meet the tile map. `TileSet.tile_size` and `TileSetAtlasSource.texture_region_size` are both `Vector2i(16, 16)`, and this game draws 32-pixel tiles (D-228). `TileMapLayer.collision_enabled` and `navigation_enabled` are both `true`, so a layer makes physics bodies and navigation regions that no rule reads (G-1, G-23). The coordinates of a layer "are limited to 16-bit signed integers". Sources: the external facts of `docs/roadmaps/area-exploration.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-7: the tile size, the region size, and the two switches, with a test that reads each one back (T-2) |
| F-52 | The camera meets two facts that no page of the docs states. When the limit rectangle is smaller than the view, the camera centers the view: the source reads "Split the difference horizontally (center it)". The gate of PR-7 for a small map rests on that source alone. The same function carries a FIXME: "smoothing is not currently applied only once per frame / tick, which will result in some haphazard results". The docs add that the position of the node "doesn't represent the actual position of the screen". Sources: the external facts of `docs/roadmaps/area-exploration.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-7: a test locks the centering of a small map, and Game moves the camera from the tick of Core, never from the smoothing of Godot (D-203) |
| F-53 | D-534 takes an evaluator that simulates each legal action and the strongest reply of the other side, and no measurement of its cost exists. The cost grows with the count of legal actions times the replies. The same code runs on the Deck at 60 frames per second (D-161), and a night plays fourteen thousand runs through it (D-507). The plan holds no number until M-3, M-4, and M-6 | 2026-09-16 | ⚠ Binds PR-11: the PR reports the count of legal actions and the time of a turn before Gate 2, and a miss changes the depth or the profiles (G-14) |
| F-54 | The end of the job system left the stats of a character with no source. D-34 gives the character level "for stats", and D-77 gave the rest to job multipliers. D-268 removed the jobs, and no later row replaced those multipliers. No PR could set the health, the MP, the attack, the defense, or the speed of a character | 2026-09-16 | ✅ doc. D-537: each character carries its own stat curve in content, and PR-30 balances the eight curves against the M-4 band |
| F-55 | A scene step can set a story flag (D-173), and the scene runner lands in Phase 2. PR-18, which defines the flags and the condition form, sat in Phase 3. PR-14 and PR-35 also read a condition in Phase 2, for a hub line and a closed route (D-59, D-113) | 2026-09-16 | ✅ doc. D-544: PR-68 takes the flag set and the condition form with the scene runner, and PR-18 keeps the branches and the choice effects. ⚠ Binds PR-68 |
| F-56 | Two Godot audio calls meet the plan. `AudioStreamWAV.load_from_buffer` returns an empty reference on data that is not WAV, and it prints the reason to the log alone, as `ImageTexture.create_from_image` does (F-45). `AudioStreamPlayer.get_playback_position` "Returns 0.0 if no sounds are playing", and its note says that "The position is not always accurate, as the [AudioServer] does not mix audio every processed frame". Sources: the external facts of `docs/roadmaps/area-audio.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-69 and PR-70: Game checks every stream that it makes and fails with the id of the render, and the audio player holds its own count for the crossfade of D-428 and the resume of D-429 (T-2) |

## 6. Guardrails (the safety contract for every PR)

### 6.1 Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-5): T-5, T-2, T-3, T-4, T-7, T-1. T-6 is absolute and never conflicts.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep. Two concrete cases before any abstraction. No clever one-liners. Tune only on measurement.
- **T-2. Zero silent failures.** No swallowed error. An absent value is an error, never a zero. Every error carries its context. Assertions stay on in shipped builds.
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file records the findings (D-17). A PR in the override set that changes no decision row merges without a review when the `review-override` label is on (D-16, D-71, D-239, D-401).
- **T-5. Document everything.** Continuity is the first duty. Each session adds its handoff entry. The other documents update when intent, a decision, or a plan changes.
- **T-6. No attribution.** No code, game text, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-22). Two places are exempt: the author field in the session handoff, and the review files.
- **T-7. Deterministic simulation.** Every run replays from a seed and an input record. The core uses integer math, seeded random streams, and no clock. A replay gives the same state hash on every platform (D-6).

### 6.2 Guardrails

1. **G-1.** Core has no engine dependency and no file, network, clock, or OS dependency. A test asserts the reference list (D-100).
2. **G-2.** No `float`, `double`, or `decimal` in Core. Fixed-point integers carry every rate. The `det-lint` tool enforces it (D-6).
3. **G-3.** No `System.Random`, `DateTime`, `Stopwatch`, or `Environment.TickCount` in Core. The seed and the tick are the only sources of randomness and time (D-6).
4. **G-4.** One seeded stream per subsystem, and a fixed iteration order wherever the order reaches the state (D-6).
5. **G-5.** Every run records its seed, content hash, versions, and inputs from the first tick. A replay reproduces the state hash, and the `replay-identity` job proves it on every CI leg before merge (D-6, D-481).
6. **G-6.** Every content file validates against its schema at load and in a test. An absent field is an error. No `.tres` files (D-116).
7. **G-7.** No inline string that the player sees. Every player string has an id in the string table (D-7, D-116).
8. **G-8.** One concern per PR (L-1).
9. **G-9.** No unowned decision. A session that hits an open question files it and stops (D-19).
10. **G-10.** Ids in this file and in the registers never change (D-13).
11. **G-11.** No co-author trailer, generation line, or model name in a commit, PR, or comment (T-6).
12. **G-12.** Every document follows ASD-STE100. The `ste-check` job runs the checker in the PR gate (D-10).
13. **G-13.** Every dependency has a decision entry that justifies it.
14. **G-14.** Every optimization has a profile before it and a measurement after it.
15. **G-15.** Squash merge by the owner, from a short branch, with a conventional commit subject (D-8).
16. **G-16.** A PR that creates a check passes that check. A PR names any check that does not exist yet, with the PR that creates it (L-11). A check on `pull_request_target` or `schedule` cannot run on the PR that creates it (F-37). That PR proves its command in Tests, and the live check first runs after the merge (D-500).
17. **G-17.** Every `core` behavior change bumps the simulation version constant, and the review confirms it.
18. **G-18.** No empty `catch` and no silent default. Every error carries its context (T-2).
19. **G-19.** Every screen designs to 1280 by 800 with 32-pixel tiles. It also holds the 16:9 view, about 1422 by 800 (D-480). The Steam Deck at 1x with the CRT on is the floor (D-92, D-120, D-228). Other screens fit the height, with whole-number scale as a setting (D-232). Every shape but 16:10 and 16:9 shows black bars (D-480).
20. **G-20.** Every player string follows the `game-text-style` skill, and the owner approves each text batch in its PR (D-57, D-63).
21. **G-21.** Every enemy profile validates at load, and a profile that can never act fails the load (D-65, T-2).
22. **G-22.** The night gate is green before merge, once PR-15 creates it. It needs a success record from a night inside 48 hours (D-64).
23. **G-23.** Godot physics, timers, and navigation never feed the simulation. The camera, the shader, the audio, and the input map live in Game (D-100, D-106).
24. **G-24.** Every sprite, tile, and portrait is a text grid in content. The atlas tool renders the PNG, and a test proves the committed atlas matches (D-107). A normal map comes from the grid, and its atlas gets the same test (D-184). A drawing file is JSON with its rows as strings, and a large picture places drawn pieces (D-515, D-516).
25. **G-25.** Every content batch the owner approves, sprites and text alike, appears in its PR description in full (D-57, D-107). A tool renders each art batch as review sheets, and the session attaches them with `gh` (D-514).

## 7. Roadmap

Five phases. Gate 1 is a foundation gate with no play. Gates 2 to 5 are builds that the owner plays on the desktop and on the Deck. Each has a written exit test and a sign-off on feel (D-52, D-92). Ids: PR-# code changes, M-# measurements.

An item that kept its purpose through the pivots kept its number. PR-32 is retired. New items start at PR-34 (G-10). Five phase files and twelve area files in `docs/roadmaps/` expand each phase and each area before PR-1 (D-142, D-144, D-485). A phase roadmap gives per-PR scope and exit tests, and an area roadmap says how its area works. Each entry cites its decisions and never restates them.

### Phase 1: Foundations (gate: every CI leg green with an identical state hash, the smoke session green, docs and PR gate live, no play)

**PR-1: Repository scaffold.**
Create the solution with the Core, Game, Tools, and Tests projects (D-118). Pin the .NET 10 SDK in `global.json` and Godot 4.7.2 .NET in the runbook and in CI (D-99). Core is a class library with no references. Game is a Godot .NET project that references Core. Tools is a console project. Tests is an xUnit project that references Core and Tools.

Add `Directory.Build.props` with nullable on and warnings as errors. Add the Makefile with `verify`, `where`, `hooks`, `test`, `lint`, `ste-check`, and `run` (D-3). Add the pre-commit hook (D-8).

Add the CI workflow that builds, tests, and checks the format on three hosted legs (D-2, D-117, D-481). The legs are Linux and Windows on x86_64, and macOS on Apple silicon. Add the `smoke` workflow that installs the pinned Godot binary and runs the headless smoke session on each leg. The session boots, starts a run, and quits with no log errors. Add the `ste-check` workflow on the interim Python script (D-10).

Add a test that asserts `CLAUDE.md` and `AGENTS.md` are identical (D-20) and a test that asserts the Core reference list (G-1). No game code.
Gate: `make verify` passes on this machine, and the three CI legs, the smoke job, and `ste-check` pass on the PR.
> *In plain English:* this makes the empty project with its four parts and the checks that every future change must pass. It adds nothing that plays. It is safe because it changes no behavior.

**PR-2: STE checker in C#.**
Write the `ste-check` command in Tools as new code (D-101, D-277). It carries the reference check, the session number check, and the HTML comment rule (F-11). Settle F-5 on one rule for numbered items. Retire the Python script and move the `ste-check` workflow to the tool.
Gate: the checker passes on itself, on this file, and on the skills, and it fails a fixture file for each rule.
> *In plain English:* this replaces the borrowed script with a tool in the project language. Documents are the project's memory, so the tool guards that memory.

**PR-3: Review gate.**
Write the `review-gate` command in Tools as new code, with its workflow on `pull_request_target` (D-15, D-101, D-277). The workflow runs the tool from the base branch and fetches the PR head as data. The tool applies the three rules of the `pr-review` skill and the override rules of D-16 with the eligible set of D-71, D-239, and D-401. It publishes a check run.
Gate: the job gives success on a fixture PR with an approved record, and failure on a stale head. It gives success on a documentation PR with the label, and failure on a PR with the label that changes a row of `docs/decisions.md` (D-401).
> *In plain English:* this adds a check that turns red when a change has no approved review from the other provider. The owner then requires it on `main` (OQ-3).

**PR-4: Random streams, fixed-point math, det-lint, and replay identity.**
Implement the seeded streams, one per subsystem, split from the run seed (G-4). Implement the fixed-point types. Write the `det-lint` command in Tools as new code, with these rules (D-101, D-277). In Core: no float type, no clock, no OS random, no reflection, and no `Dictionary` where order reaches the state (G-2, G-3). In Game: no inline player string (G-7).
Implement the state hash and the `replay-identity` job that runs a fixed seed set on every CI leg and compares the hashes (G-5, D-481).

Gate: this PR passes its own lint and its own identity job, and the lint fails a fixture that uses `double`.
> *In plain English:* different computers can give different answers for decimal math. This adds our own integer math and a check that proves the same result everywhere on every change.

**PR-5: Content loader, schemas, and the string table.**
Write the JSON loader as new code, with one schema type per content type (D-116, D-177, D-277, G-6). A load failure names the file, the field, and the reason. A test loads every content file. Implement the id-keyed string table (G-7).
Gate: a content file with an absent field fails the load test with the field name.
> *In plain English:* every lesson, enemy, and item lives in a data file with a strict shape. A file with a gap fails loudly instead of a silent zero.

**PR-6: Simulation loop, intent record, replay, and save.**
Implement the fixed-rate loop, the intent record for keyboard, gamepad, and mouse (D-84), and the run record. The record header holds the format version, the simulation version, the content hash, the seed, and the initial state (G-5). Implement the recorder, the replay, and the compaction rule: a snapshot plus the intents since it (F-10). A load reads the snapshot of the save alone, so a patch never breaks a save (D-259).

Implement the save file as a record in the Godot user folder `the-thing-below` (D-62, D-465): one slot, one autosave, and a one-use resume file (D-258). A snapshot stores content ids and state, never copies of content, and each snapshot format has a version and a migration (D-163, D-166). A save writes to a temporary file with a checksum, then replaces the old save in one step (D-178). A crash writes a crash file beside the save, and every log line is one JSON object (D-170, D-179). The record header and the crash file carry the game version (D-448). The crash message names the studio email address of D-473, which waits for OQ-57.

Property tests over one thousand seeds assert that a replay reproduces the end-state hash and that a version mismatch produces a contextual report.
Gate: the replay of a recorded run gives the same hash on every CI leg, and a save reloads to the same hash.
> *In plain English:* the game writes down its start state and every input. That record then plays any run again, so every bug becomes repeatable, and the save file is that record.

**PR-34: Atlas tool, palette, and the grid format.**
Port `docs/tools/make-atlas.py` into Tools as the `atlas` command for the 32 by 32 grids (D-107, D-119, D-406). Define the grid schema (D-108, D-109). A tile or a character sprite is a 32 by 32 grid, and a portrait is a 64 by 64 grid (D-228, D-234). Enemy grids are 32 by 32, 64 by 64, or 96 by 96 and larger (D-236). The cast grids come into `content/sprites/` from the approved sample in `docs/samples/` (D-233, D-402, D-405). A sprite has a frame list.

The palette grows from 48 to 64 colors, with a swatch sheet for the owner's approval (D-181, D-185, D-238). The tool also builds a normal map for each grid, with optional override grids (D-183, D-184). A test decodes the committed atlas and proves that its pixels match the grids. It never compares file bytes, because the compressed bytes depend on the encoder (F-19). Retire the Python script.

Gate: the tool builds `content/sprites/atlas.png` from the five cast grids, and the pixel test of F-19 passes. A grid with an unknown key fails with the file, the line, and the column. A palette with a repeated key fails with the key.
> *In plain English:* every picture in the game is a text file of letters, one per pixel. A tool turns those letters into the image the engine draws, and a test proves the image matches the letters.

**M-1: Tokens per PR.** Record the harness usage per PR for the first ten PRs.

**M-2: CI wall time per PR.** Record the wall time of each CI job per platform for the first ten PRs.

### Phase 2: First playable (gate: the owner plays the village, one hub, and one dungeon with lessons and a shop, on the desktop and on the Deck, D-51, D-92, D-268, D-362, D-369)

**PR-7: Tile map, movement, sight, and the map scene.**
Define the layout content format: a grid of tile ids, doors, pickable locks, traps, chests, save points, spawn points, and markers for secrets (D-39, D-41, D-386). A layout also sets the time of day of its map (D-442). Implement tile-locked movement, sight, and the fog over tiles the party never saw, in Core.

Draw the map scene in Game at 1280 by 800, fit to the window, with the camera on the lead (D-106, D-228, D-232, D-292, D-306). A 16:9 screen gets a view about 1422 by 800, and every other shape gets black bars (D-480). Map the arrow keys and the gamepad stick and pad to intents (D-84, D-219).

The export job of D-449 starts with this PR. Each merge to `main` exports the three builds of D-481 as build artifacts, with the license files of D-467. The roadmaps PR gives the job its PR id.
Gate: the party walks a fixture dungeon on all three platforms with a keyboard and with a gamepad. The camera never scrolls past the edge of a map larger than the view, and a smaller map sits centered, in both views.
> *In plain English:* this is the first thing you can open and move in. The dungeon is a grid of tiles, the party walks it one tile at a time, and the view follows.

**PR-41: Screen-test job.**
Add the Linux CI job of D-172. It installs a pinned Mesa and runs Godot under Xvfb with the OpenGL driver. It captures fixture scenes and compares the frames by pixel with a committed CI baseline.

Fix every source of change at capture: the particle seeds, the CRT flicker phase, and the time of day. Capture both views of D-480, and the fit of D-232 at 1080 and 1440 screen rows, to catch aliasing in the scanlines (D-240). Add the desktop command that makes a contact sheet with the real renderer (D-172).
Gate: the job passes on the map scene of PR-7, and it fails when one pixel of the baseline changes. Two runs give the same frames.
> *In plain English:* the computers that check each change have no screen. This job gives them one with a fixed picture, so a broken screen fails before it merges.

**PR-8: Enemies on the map.**
Implement fixed enemies and patrols with sight (D-37). A patrol that sees the party starts an encounter, and the side that reaches the other from behind acts first (D-265). After a flee, the group returns to its route, and no battle with it starts for a short grace time (D-381). No random encounters. Enemies that move walk with three views, and enemies that stand flip on the tick (D-108, D-207).

Gate: property tests over one thousand seeds assert that a patrol never leaves its route and never sees through a wall. The time of day of the map picks the route (D-193, D-442). A large enemy never leaves its area (D-209). A fled group starts no battle inside its grace time (D-381).
> *In plain English:* enemies stand and walk in the dungeon where you can see them. You choose the fight, or you sneak past, or they catch you.

**PR-9: Battle core and timeline.**
Implement the encounter state and the timeline, where each action pushes its user back by a delay (D-29, D-376). Implement actions, a basic attack for every character, damage in fixed-point, the eight elements with weakness, resist, and absorb, and the ten statuses (D-74, D-75, D-359). Implement haste, slow, and heavy actions as timeline shifts. One to three characters and up to six enemies (D-31, D-336). Down and party wipe (D-36).

Implement a front row and a back row for each side, where melee reaches the front row while anyone stands in it (D-377). Implement a flee command whose chance rises with party speed, with a lost turn on a failure and no flight from a boss (D-378). A step to the other row and the use of an item each cost a delay (D-380, D-382).
Gate: property tests over one thousand seeds assert that the timeline never stalls. Every status but poison, blind, and silence ends with its battle, and those three remain after it (D-390). Melee never reaches a back row while its front row stands, and no flee starts in a boss fight (D-377, D-378).
> *In plain English:* this is the fight itself, with the order of turns visible and shaped by speed. Nothing draws it yet.

**PR-10: Battle scene.**
Draw the side view with Terminus TTF, the body font that the owner picked (D-104, D-111, D-122, D-263). Enemies sit on the left and the party on the right, each side in a front row and a back row (D-377). The timeline strip runs across the top, and the command menu and the status sit at the bottom. The attack pose plays on an action, and a color flash on a hit (D-96, D-108). Every message comes from the string table in the game voice (G-7, G-20). A backdrop per place.

Gate: a screen test renders a fixture battle, and the owner reads a fight from the screen alone.
> *In plain English:* the fight appears on screen: who acts next, who is low, what you can do. Every line reads in the voice of the game.

**PR-11: Evaluator and enemy profiles.**
Implement the tactical evaluator that scores every legal action by its simulated outcome: damage, kills, threat, healing, timeline shift, and row placement (D-65, D-377). Define the profile content format with the term weights and the traits, and its validator (G-21). Each profile carries a steal list of items and gold (D-383). Four profiles for the first dungeon. Boss phases come in PR-20.
Gate: a fixture enemy with a protector profile heals its ally before it attacks, and a profile with no legal action fails the load.
> *In plain English:* enemies think. Each one weighs what a move does before it acts, and each kind of enemy weighs it differently.

**PR-12: Lessons, aptitudes, and levels.**
Implement the character level from experience, and half experience for the reserve and for a downed character (D-34, D-73, D-387). Experience from an enemy shrinks as the party outlevels it (D-388). Implement lessons, the rites and drills that any character equips to gain abilities (D-272, D-275, D-278). Implement the main aptitude and the side aptitude of each character, with the side aptitude hidden until its task ends (D-274, D-282, D-283). Implement MP and its recovery rule (D-42).

Lesson slots sit on the character, grow with the character level, and swap at hubs and save points (D-356). An equipped lesson grows for the character who carries it, and each character keeps that growth (D-357, D-361). An aptitude adds a bonus to lessons of its kind, and a side aptitude adds half (D-358, D-360). Mend rites and rites that cure afflictions also work from the menu outside battle (D-391). The first playable holds Marrek, Bergit, and Dagvar (D-362).

Gate: a character equips a lesson and uses its ability in a fixture battle. A fixture flag unlocks a side aptitude, and the menu shows nothing there before the flag (D-283). An equipped lesson gains points from a fixture battle, and a save point swaps lessons (D-356, D-357). A lesson passed back to a character resumes at the level of that character (D-361).
> *In plain English:* abilities come from rites and drills that anyone can carry. Each character is best at one kind, and a hidden second kind opens through a personal task.

**PR-13: Gear, items, and inventory.**
Implement the six equipment slots and the inventory (D-44). Fixed items with rarity tiers as content (D-45). Any character wears any gear (D-374). The pack holds a small, fixed number of each item, and an item restores less in battle (D-382). A small set of items gets used up, and a find over the limit stays where it lies (D-384, D-385).

Gate: a character equips and removes gear in each slot, and the screen shows each empty slot. A find over the stack limit stays in its chest, and the save records what remains (D-385).
> *In plain English:* weapons, armor, and accessories go on the characters, and the screen shows what each one wears.

**PR-14: Hub map, NPCs, and services.**
Implement the hub as a walkable map with NPC sprites (D-112). The services are buildings and NPCs: rest, save, party swap, and the shop with gold (D-59, D-60, D-62, D-268). Define the hub content format with the services each hub offers (D-28). Draw the service screens.

Gate: a fixture party of four walks the hub, rests, buys, swaps the reserve and a lesson, and saves (D-356, D-362). The save reloads to the same hash. The lead moves to the reserve, and the lead still walks the map with the camera on it (D-292, D-306).
> *In plain English:* the hub is a place you walk through, where the party recovers, trades, and reshapes itself before the next dungeon.

**PR-36: Scene runner, dialogue box, and portraits.**
Define the scene script format (D-109, D-114). Sprites move and face by script, and a script can name a music cue (D-418). A dialogue box with the portrait and the choices sits at the bottom. Implement the runner in Game and the choice result in Core. Fixture portraits as 64 by 64 grids, because PR-28 and PR-29 draw the portraits of the cast (D-234).
Gate: a fixture scene walks two sprites, shows a line with a portrait, and records a choice in the run record.
> *In plain English:* the story plays out on the map with the characters you already know, and your choices land in the box under them.

**PR-15: Headless runner, bots, and the night gate.**
Implement the headless runner in Tools with a random policy and a greedy policy (D-64). A few hundred runs per PR and ten thousand each night. Write the night job and the `night-gate` command as new code: the night writes a result record, and the `night-gate` job reads it (G-22, D-101, D-277). Each run ends as complete, softlock, crash, or budget, and each failure names its seed.
Gate: ten thousand night runs of the two policies on the fixture dungeon complete with zero crashes and zero softlocks.
> *In plain English:* simple robots play thousands of runs every night without a screen. They find crashes and dead ends before a person ever sees them.

**PR-16: Dungeon parts, death, and save points.**
Implement treasure, locked doors and keys, traps and hazards, and save points with the party swap and the lesson swap (D-36, D-41, D-58, D-356). A Theft drill on one of the three who fight opens a lock marked as pickable, and it reveals and disarms traps (D-386). A wipe reloads the newer of the slot save and the autosave (D-231). A save point restores MP once per visit and no health, and a killed enemy stays dead until the party leaves (D-257, D-389). The dungeon exit returns the party to the region map.

Poison, blind, and silence last past a battle until a cure or a rest at a hub (D-390). Poison ticks on the map and can down a character, and silence stops rites cast from the menu (D-392, D-393). When poison downs all three who fight on the map, the party wipes, even with a healthy reserve (D-397).
Gate: a bot run that wipes reloads and continues, and a two-character party after a down can still reach the exit in the fixture. A fixture party that poison downs on the map wipes and reloads, even with a healthy reserve (D-397).
> *In plain English:* the dungeon gains its chests, doors, traps, and resting places, and death now costs what the design says it costs.

**PR-35: Region map.**
Implement the region map screen with nodes and routes (D-113). The party moves node to node. A route opens and closes with a flag. One hub and one dungeon as the first nodes.
Gate: a closed route refuses the move and the screen shows why, and a replay reproduces the path.
> *In plain English:* between places the party travels on a map of the region, along routes the story opens and closes.

**PR-37: CRT shader and the toggle.**
Implement the full CRT as a Godot screen shader: curvature, bleed, flicker, and faint scanlines, on by default with a toggle in settings (D-105, D-120, D-240).
Gate: the screen-test job of PR-41 captures the toggle on and off, and the two frames differ.
> *In plain English:* the whole screen looks like an old monitor, and one setting turns it off.

**PR-38: Audio synthesizer and the first sounds.**
Write the synthesizer in Tools as new code, with integer math, so a render gives the same bytes on every CI leg (D-101, D-277, D-432, D-481). It renders 16-bit style instrument voices with filters and reverb, for music and sound effects alike (D-412, D-423). A track is tracker rows in content, and a sound effect is a parameter file (D-438). The build renders the audio, and a committed list holds the hash of each render (D-432). Add the listen command that renders a batch and plays it for the owner (D-439). Six sound effects and one track for the first dungeon.

The roadmaps PR gives PR ids to the rest of the audio player, the first music, and the sound room (D-399, D-439).
Gate: every render matches its hash on every CI leg, a changed parameter fails the hash test, and the battle scene plays a hit sound.
> *In plain English:* every sound comes from a small text file that the tool turns into audio. Music is rows of notes, as a sprite is rows of letters. The first fight makes noise.

**PR-17: The village, the first hub, and the first dungeon.**
Author the village and the land near it, the mining town, and the hanging cells as content (D-28, D-39, D-110, D-313, D-369, D-370). That is the tile sets, the layouts, the enemies with their sprites and profiles, and the backdrop. It also holds the treasure, the shop stock, the NPC sprites, the sprite set of Marrek, and a placeholder scene (D-292). Marrek, Bergit, and Dagvar and the lessons of the first playable have their text in the voice (G-20, D-362).

Gate: the owner plays from the village until Dagvar joins in the hanging cells, on the desktop and on the Deck (D-362). The owner signs off on feel (D-52, D-92). The M-4 numbers land inside the band the sign-off sets, and M-6 records the Deck.
> *In plain English:* the first real place to play. Everything before this was machinery.

**M-3: Night run wall time.** Record the night duration and the crash and softlock counts for seven nights.

**M-4: Encounter numbers.** Record turns per encounter and party downs per dungeon by bot policy on the first dungeon. Binds the resource numbers of D-35.

**M-6: The Deck.** Record the frame time on the first playable against 60 frames per second (D-161). Record the readability of the font and the sprites at 1x, with the CRT on and off (D-92, D-120, D-228, F-18).

**The store page, after Gate 2.** The store page of the full game goes public as Coming Soon (D-471). The owner pays the Steam Direct fee, and a session searches the EU and WIPO registers for the game name and the studio name (D-408, OQ-57). The page needs the store text, the capsule grids, the screenshots, and the system requirements from M-6, with Apple silicon as the only Mac (D-452, D-475, D-482, F-34). The content survey with its AI disclosure comes before the review (OQ-59). The roadmaps PR gives the work a PR id.
> *In plain English:* once the first playable feels right, the game gets a public page on Steam. People can find it and add it to a wishlist years before the full game.

### Phase 3: Story systems (gate: the owner plays a branch that closes a route and a hub that changes with an earlier choice, D-329)

**PR-18: Story flags, branches, and scene choices.**
Implement the flag set, the branch conditions in content, and the choice effects (D-40, D-329). A closed route, a lost ally outside the cast, a changed hub, and a new time of day are four flag effects (D-301, D-442).
Gate: a fixture branch closes a route on the region map, and a replay reproduces the branch.

**PR-19: Quests and the rumor board.**
Implement the quest state and the rumor board NPC in the hub (D-59). The quest state carries the personal tasks, and a missed task closes at the end of its region (D-282, D-375). No reputation and no relationship value exist (D-329).
Gate: a fixture quest completes, a hub line changes with a story flag, and a finished task unlocks a side aptitude (D-282).

**PR-20: Boss phases and signature moves.**
Implement the scripted phase layer over the evaluator (D-65). A phase changes the profile and adds a move. One boss for the first dungeon, with its sprite.
Gate: the boss changes phase at the scripted threshold in every one of one thousand seeds.

**PR-21: Puzzles and secrets.**
Implement switches, pushable blocks, light and dark, hidden rooms, and secret markers (D-41).
Gate: a fixture puzzle opens a door, and a hidden room stays hidden until found.

**PR-22: Retired.** Jobs five to eight have no purpose after D-268. No later item takes the id (G-10). PR-42 in Phase 4 takes the lessons of region one (D-304).

> *In plain English for Phase 3:* the game learns to remember what you chose and to answer it. Bosses gain their set pieces, and dungeons gain their puzzles.

### Phase 4: Region one content (gate: the owner plays region one end to end on the desktop and on the Deck and signs off, D-56)

**PR-23 to PR-26: Dungeons two to four, and the return to the cells.** One content PR per dungeon build, in the order of play (D-313). PR-23 is the deep mine, and PR-24 is the second visit to the hanging cells (D-327, F-29). PR-25 is the border fort, and PR-26 is the ice crossing. Each holds a tile set, enemies with sprites and profiles, a boss, a backdrop, treasure, puzzles, and secrets.

**PR-27: The second hub.** A hub of another shape than the first (D-28), with its tile set, its NPC sprites, its services, and its scenes.

**PR-42: The lessons of region one.** The rites and drills of region one as content, across the eight kinds, with icons and text in the voice (D-275, D-281, D-304, G-20). Each lesson has a place in a dungeon, a hub, or a scene. Gate: every lesson has a place, and bot runs of region one with each side aptitude absent in turn stay inside the M-4 band (D-282).

**PR-28 and PR-29: The arc.** The scenes, the set choices, the portraits, the personal tasks, and the cast text of region one, in two batches (D-56, D-57, D-282, D-350). The story follows `docs/world/arc.md`. The PRs propose each personal task and one or two more set choices for approval (D-352, D-355).

**PR-30: Balance pass.** Tune the numbers of D-35, D-60, D-382, and D-388 on the M-4 band and the night runs. Every change reports the number before and after (G-14).

**M-5: Region one play time.** The owner's play time from the first hub to the end of the arc, against the six to eight hours of D-56.

**Trusted players, after Gate 4.** A few players whom the owner picks play the build artifacts of region one and send notes outside Steam (D-469). Their notes feed the fixes before the release.

> *In plain English for Phase 4:* the free prologue takes shape. Four dungeons, two hubs, their lessons, the first part of the story, and the numbers tuned by robots and by play.

### Phase 5: First release, the free prologue (gate: a tagged build on GitHub that a fresh machine runs, then the Steam demo on the Deck)

**PR-31: Release workflow.** On a release tag, publish the three exports of the prologue as a GitHub Release with its release notes (D-53, D-448, D-453, D-457). GitHub Releases stop when the demo goes live on Steam (D-470). The runbook gives the Open Anyway steps for macOS and the Run anyway step for Windows (D-455, D-463). Every export carries the license files (D-467).

**PR-32: Retired.** The fallback pass of v1 has no purpose after D-98. No later item takes the id (G-10).

**PR-33: The title screen, settings, and the exit.** The first screen plays the main theme (D-427). The settings hold the CRT toggle, the bindings, the audio group of D-435, and the vibration setting of D-434. A clean exit saves.

The boot splash shows the studio mark (D-468). A corner of the title screen shows the game version (D-454). The title menu holds a credits screen with the studio and the license notices (D-467).

**PR-39: Deck verification pass.** Walk the Steam Deck checklist to the rating Verified (D-459). It checks glyphs that match the input, default bindings, and the 1x frame (D-222, D-228, D-460). It also checks text of 9 pixels or taller, and suspend and resume (D-85, D-92).

**PR-40: Steam integration.** Start Steamworks through the binding of OQ-58, and add the call that reports the controller type for the glyphs (D-460, D-462). Set Auto-Cloud on the save folder, and test the conflict rule of D-93 on two machines (D-461, D-465). Publish the prologue as the demo app "The Thing Below: Prologue", and verify whether the demo needs a fee of its own (D-143, D-478).

Set the Linux build on the Steam Linux Runtime that the Godot export needs (D-458). From this PR on, CI signs and notarizes the universal macOS build (D-455, D-482). The store page and the name checks moved to Gate 2 (D-471).

> *In plain English for Phase 5:* the prologue becomes something a person downloads and runs. Then it becomes a free demo they find on Steam and play on the Deck.

### Phase 6: Region two and later

Parked until Gate 5. Each later region repeats Phase 4 with its own roadmap. Before the first content of a paid region, the repository goes private (D-456). Phase 6 also plans the achievements of the full game and its one Next Fest (D-466, D-472).

## 8. Sequence (strict order, single owner)

1. Owner: create no label, install no tool. gitar and the label exist (D-66, D-67).
2. PR #2 to PR #9 merged on 2026-09-14 (D-398, D-400, D-402, D-411, D-217, D-446, D-447). OQ-56 closed with no change to this sequence (D-483). PR #10 holds the shape of the roadmaps, and PR #11 writes them and rebuilds sections 7 and 8 (D-484 to D-490). PR #11 places the export job, the store page work, and the credits roll (D-449, D-467, D-471). PR #12 holds the next design-critic pass. The owner runs the Deck test of D-160 on the Linux export before PR-1 (D-458).
3. PR-1, PR-2, PR-3.
4. Owner: require the checks on `main` (OQ-3).
5. PR-4, PR-5, PR-6, PR-34.
6. M-1, M-2.
7. **← GATE 1 (foundation).** The identity job, `dotnet test`, the smoke job, and `ste-check` are green on every CI leg.
8. The fonts are set: Terminus TTF and Terminus TTF Bold (D-263, D-264).
9. PR-7, PR-41, PR-8, PR-9, PR-10.
10. PR-11, PR-12, PR-13, PR-14, PR-36.
11. PR-15. One night runs, then the `night-gate` job joins the PR gate.
12. PR-16, PR-35, PR-37, PR-38.
13. PR-17.
14. M-3, M-4, M-6.
15. **← GATE 2 (first playable).** The owner plays the village, one hub, and one dungeon on both machines and signs off on feel (D-362). Then the owner pays the Steam Direct fee, and the store page goes public (D-471).
16. PR-18, PR-19, PR-20, PR-21.
17. **← GATE 3 (story systems).** The owner plays a branch and a hub that changes with an earlier choice.
18. PR-23 to PR-26, PR-27, PR-42.
19. PR-28, PR-29, PR-30.
20. M-5.
21. **← GATE 4 (region one).** The owner plays region one end to end on both machines. Then trusted players play the build artifacts (D-469).
22. PR-31, PR-33, PR-39.
23. Owner: join the Apple Developer Program (D-455). The Steam Direct fee moved to Gate 2 (D-471).
24. PR-40.
25. **← GATE 5 (first release).** A fresh machine runs the tagged build, and the Deck runs the Steam demo.
26. Phase 6 stays parked.

## 9. Open questions

The open questions register is `docs/questions.md` (D-19). It holds OQ-1 onward with options, recommendations, what each blocks, and the date and decision that resolve each one. File a new question there, not here. Ids never change.
