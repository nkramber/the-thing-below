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

2026-09-16 release area pass: the store page work at Gate 2 splits into PR-75 and PR-76 (D-550). PR-74 adds the capture that takes each screenshot and each trailer shot (D-551). PR-77 holds the credits roll, right after PR-29, so the owner sees it at Gate 4 (D-552). PR-40 splits into PR-78, PR-79, and PR-40 (D-553). That closes the twelve area files of D-485, and the five phase files and the rebuild of sections 7 and 8 follow (D-488).

2026-09-16 phase files and rebuild pass: the five phase files of D-485 are complete. Each PR entry there has a scope, exit tests, a review focus, and its questions (D-144, D-487). Every active id from PR-1 to PR-79 has one entry in one phase file. Section 7 below becomes a high-level index that links to each phase file, and the full paragraph of each PR lives there alone (D-554). Section 8 takes the order that the phase files hold. G-22 now names PR-49 as the creator of the night gate (D-496).

2026-09-16 critic pass: the design-critic pass of PR #12 read the merged plan in five slices and found 52 defects, which F-57 records. The owner answered 18 questions (D-555 to D-572). A place stays cleared until a story event, and the game has no fog of war (D-555, D-566). The game draws one 16:9 frame of 1280 by 720 (D-568). PR-80 holds the enemy record, and PR-81 holds the sealed gallery (D-557, D-562).

2026-09-16 session pass: PR #14 binds each session to one PR and makes the PR the complete unit of its work (D-576, D-577). No PR exists only to record the merge or the documents of an earlier PR, and git holds the merge commit (D-578, D-580). The `one-pr-one-session` skill holds the gates, and PR-3 adds the document rules to the review gate (D-579). The author session answers each review of its PR (D-582). F-58 records that no check can see the conversation of a session.

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
- Godot's own CI runs the engine under `xvfb-run` with `--rendering-driver opengl3`. Source: `godotengine/godot/.github/actions/godot-project-test/action.yml` in the Godot repository, read 2026-09-12.
- In Movie Maker mode, faster hardware renders sooner, "but the visual output remains identical", and "the window size is clamped by your display's resolution". Source: Godot docs, "Creating movies", read 2026-09-12.
- HDR for 2D works "when using the Forward+ and Mobile rendering methods", and "When using the Compatibility rendering method, glow uses a different implementation". Source: Godot docs, "Environment and post-processing", read 2026-09-12.
- No Steam game carries the name "The Thing Below" (D-408). The store search API gave 0 results for "the thing below", "thing below", "the thing beneath", and "things below". The store search page for "the thing below" listed 50 titles, and none holds the phrase. Source: `https://store.steampowered.com/api/storesearch/?term=the+thing+below&l=english&cc=US`, one query per term, and `https://store.steampowered.com/search/?term=the+thing+below`, run 2026-09-14.
- A free horror jam game on itch.io has the close title "The Thing Beneath". Its game page says "The Thing Beneath" 7 times and never "The Thing Below". Its devlog of August 2026 says "The Thing Below" once and "The Thing Beneath" 12 times. The itch.io search for "the thing below" lists no game with that exact title. Source: `https://studio-laaya.itch.io/the-thing-beneath`, `https://studio-laaya.itch.io/the-thing-beneath/devlog/1616272/the-thing-beneath-directors-cut`, and `https://itch.io/search?q=the+thing+below`, read 2026-09-14.
- No USPTO record has the phrase in its word mark. A `match_phrase` query on the field `WM` gave 0 hits for "the thing below", "thing below", "thing beneath", "things below", and "thing from below". As a control, the same query gave 11 hits for "below deck" and 1,125 for "below". Each query was a POST of the body `{"query": {"match_phrase": {"WM": "<term>"}}, "size": 3}` to the search service behind the USPTO Trademark Search. The service has no public documentation, and the controls show that `WM` holds the word mark. Source: `https://tmsearch.uspto.gov/prod-stage-v1-0-0/tmsearch`, run 2026-09-14.
- The EU and WIPO registers have no script check yet. The owner checklist of PR-75 checks them before the store page goes public (D-408, D-550).
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
- The same page states two more rules of Verified. "The default controller configuration must provide users with the ability to access all content", and "Players must not need to adjust any in-game settings in order to enable controller support". A game with text entry must "either use a Steamworks API for text entry to open the on-screen keyboard for players using a controller, or have your own built-in entry". Source: `https://partner.steamgames.com/doc/steamdeck/compat`, read 2026-09-16. The design critic of PR #12 found that D-459 cited these two rules and that this list held neither.
- Valve recommends Steam Linux Runtime 4.0 for new native Linux games, and a developer picks the runtime on the Steamworks site. Source: `https://github.com/ValveSoftware/steam-runtime/blob/master/README.md`, read 2026-09-14.
- Under Steam, gamepad input "will appear in your game as regular Xbox controller input". Valve recommends the Steam Input API for glyphs, and a game without it can "detect the device type via our API". Source: `https://partner.steamgames.com/doc/features/steam_controller/getting_started_for_devs` and the Deck compatibility page, read 2026-09-14.
- Steam Auto-Cloud syncs listed folders "when the application launches and exits", with no code in the game. The page does not say how Steam settles a conflict. Its roots include WinAppDataRoaming, MacAppSupport, and LinuxXdgDataHome. Source: `https://partner.steamgames.com/doc/features/cloud`, read 2026-09-14.
- Capsule art shows only game art, the game name, and an official subtitle. A store page needs at least five screenshots of play, at 1920 by 1080 or larger, in 16:9. Source: `https://partner.steamgames.com/doc/store/assets/rules` and `https://partner.steamgames.com/doc/store/assets/standard`, read 2026-09-14.
- A trailer can have up to 1920 by 1080 pixels at 30 or 60 frames per second. The file is .mov, .wmv, or .mp4, and Valve prefers H.264 with AAC. Source: `https://partner.steamgames.com/doc/store/trailer`, read 2026-09-14.
- On 2026-09-14, Steamworks.NET last released on 2026-08-02 and Facepunch.Steamworks on 2026-04-23, both under MIT, and NuGet lags by years for both. GodotSteam has no C# version of its own, and it moved to Codeberg on 2026-09-04. Source: the GitHub releases of each project, `https://www.nuget.org/`, and `https://codeberg.org/GodotSteam/GodotSteam`, read 2026-09-14.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

The Thing Below, a tentative name (D-215), is a dark fantasy role-playing game in 32-pixel sprites on a 16:9 frame of 1280 by 720 (D-27, D-107, D-228, D-568). A fixed cast (D-33, D-299) travels between hubs of every shape, a castle town, a cave community, a boat, an airship (D-28). Between the hubs lie hand-authored dungeons with visible enemies, traps, puzzles, and secrets (D-37, D-39, D-41). Three fight at a time on a visible timeline where speed decides the order (D-29, D-31). Any character equips lessons, the rites and drills that give abilities, and each character does one kind of ability best (D-272, D-274, D-278).

Combat is hard because enemies think and resources run out (D-35), and a fallen character stays down until a hub (D-36). Decisions close routes, lose allies outside the cast, and change hubs (D-40, D-301).

The game runs on Godot 4 with C# (D-99). The simulation lives in an engine-free Core library that replays any run from a seed and an input record (D-100, T-7). Sprites, tiles, and portraits are text grids in content that a tool renders into an atlas (D-107). Particles, 2D light, and shaders enter the plan from the start (D-139).

The goal is a Steam release, and the Steam Deck is the readability and performance floor (D-85, D-92). The game supports Windows and Linux on x86_64, macOS on Apple silicon, and the Steam Deck, and nothing else (D-481).

A full roadmap comes before any code (D-142). The plan puts the foundations first, because every later system depends on them. Those are a deterministic core, a run record with replay, the content loader, the atlas tool, and the document gates. The first playable is the village, one hub, and one dungeon, with lessons and a shop (D-51, D-268, D-362, D-369). The owner judges feel there, on the desktop and on the Deck.

The story systems come third, because they need the loop. Region one, two hubs and five dungeons in one arc, is the first release (D-56, D-575). It ships free, as a Steam demo of the full game (D-133, D-143). Every plotline converges at the end of the game (D-131). Five gated phases hold that order.

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
| Content loader and schemas | Core | content bytes (D-508) | typed content, content hash | High. A silent default here breaks the economy |
| String table | Core | content bytes (D-508) | text by id | Medium |
| Run record, replay, and snapshot | Core | state, intents | record bytes, snapshot bytes | Total. The crash report and the save (D-62, D-493) |
| Tile map, movement, and sight | Core | layout content, intents | party position, sight | High. Patrols and ambushes (D-37) |
| Time of day | Core | layout content, story flags | the time of day of each map | Medium. Light, patrols, enemies, and music follow it, and the story sets it (D-193, D-442) |
| Battle and timeline | Core | party, enemies, abilities | turn order, damage, statuses | Total. The design risk (D-29, D-35) |
| Evaluator and profiles | Core | battle state, enemy profile | enemy actions | High. The largest single system (D-65) |
| Lessons, aptitudes, and levels | Core | lesson content, experience | character state | High. The build decision (D-34, D-272, D-274) |
| Gear and items | Core | item content, inventory | equipment state | Medium (D-44, D-45) |
| Story flags, story scenes, and quests | Core | story scene content, conditions, choices | flags, story scene state, quest state, hub state | High. Branches multiply (D-40, D-59, D-329). Core runs each scene and holds its step index, and one condition form serves every reader (D-540, D-542, D-543) |
| Hub services and the region map | Core | hub content, route content, gold | party, saves, position | Medium (D-59, D-113) |
| Debug assembly | Debug assembly | debug intents | Core state, through the seam of D-260 | High. A release build never loads it (D-260) |
| Save, record, crash, and log files | Storage | record bytes, snapshot bytes, crash context, log entries | save files, record files, crash files, log files | High. A torn write loses a save (D-178, D-494) |
| Map scene, battle scene, hub scene, scene runner | Game | Core state, content from the Game assembly, the atlas and its index, large pictures, edge files, string table | screen, intents | Medium. Cosmetic by design (D-106, D-111, D-114, D-501). Player text reaches the screen through the text helper (D-499). The content bytes come from the Game assembly (D-508). Art draws with the Nearest filter, and an art file names the content ids that it draws (D-519, F-45) |
| Dialogue box and portraits | Game | Core story scene state, string table | screen, wait intents, choice intents | Medium (D-109). Game draws each story scene step and sends a wait intent when it ends (D-540) |
| The frame and the fit | Game | settings | screen | Medium. The Deck floor, the one 16:9 frame, and the fit at 1920 by 1080 (D-228, D-568). The game draws no CRT pass (D-618). OQ-183 holds the scale of the frame on a screen |
| Light, particles, glow, and transitions | Game | effect files, light setups, normal maps, the effect budget, Core state | screen, wait intents | Medium. The effect budget of the Deck test holds them inside 60 frames per second, and no rule waits for an effect (D-139, D-160, D-182, D-183, D-522, D-523) |
| Crash file and replay viewer | Game | run record, crash file | screen, and a crash file through Storage | High. The crash report, and a viewer in development builds alone (D-170, D-175, D-494) |
| Audio player | Game | rendered audio from the Game assembly, Core state, audio files, settings | sound | Low. Music by place and time of day, ambience, stings, and the mix (D-413, D-424, D-429, D-435). An audio file names the content ids that it serves, and each stream comes from bytes with a checked return (D-547, D-548, F-56) |
| Steamworks glue | Game | Steam client | controller type for the glyphs | Low. Steam builds alone, with no cloud code (D-460, D-461). PR-78 adds the binding and the call, and PR-40 sets Auto-Cloud (D-553) |
| Atlas tool, large picture render, normal maps, PNG code, PNG import, audio synthesizer | Tools | drawing files, large pictures, palette, edited PNG files, tracker rows, parameter files | atlas PNG, atlas index, normal-map atlas, drawing files, review sheets, large picture renders, rendered audio, hash list | Medium. The atlas, its index, and the normal-map atlas are committed with a pixel test (D-107, D-184, F-19), and the audio renders at build against its hash list (D-432). Review sheets reach the PR description through `gh`, never git (D-514). Integer math gives the same output on every CI leg (D-502) |
| Map preview, tile edges, screenplay | Tools | map content, edge rules, story scene content, string table | preview PNG, edge files, screenplay text | Low. The owner approves maps and story scenes from them (D-165, D-173, D-204). An edge file stays outside the content hash, and a test proves that it matches its map (D-501) |
| STE checker, det-lint, review gate, night gate | Tools | source, docs, review records, night records | pass or fail | Gate. det-lint reads types through the Roslyn compiler library (D-498) |
| Headless runner and bots | Tools | policies, seeds | run records | High. The night gate (D-64) |
| CI checks and the night job | CI | source, content, the identity file, night records | pass or fail, a coverage report, night records as run artifacts | Gate. The actions of GitHub alone, with SHA pins, and bot runs on all three legs (D-504, D-505, D-509, D-511) |
| Export and release | CI | merge, release tag, a PR that changes the export | three exports as build artifacts, a smoke session on each, the GitHub Release of a prologue tag | Low (D-53, D-85, D-449, D-457, D-481, D-503, D-512). PR-79 signs and notarizes the macOS build from Phase 5 (D-455, D-553) |

## 4. Cost model (what we pay, what we do not know)

What we pay:

- Owner time: the interviews, the approvals of every text batch (D-57), and the play sign-off of every phase (D-52). The owner also approves every art batch from its review sheets (D-107, D-514), and every music and sound batch by ear (D-433). The owner also cuts each trailer (D-476), and reads the crash emails and the notes of the trusted players (D-469, D-473). The owner ran the Deck test with its effect budget on 2026-09-17, and PR-82 sets the Mobile renderer (D-616, D-617). The owner also runs the screen scale probe on three screens (D-621). The owner reads each new effect on the Mac, and a test runs on the Deck only when the answer needs the Deck (D-622, D-623).
- Tokens: two harnesses, Claude Code and Codex, on every PR (D-14, D-17). M-1 gives 68.4 million context tokens for the mean code PR (D-672).
- CI: GitHub-hosted minutes on three legs per PR, the bot runs included (D-2, D-481, D-505). Three exports run on each merge and on each PR that changes the export (D-449, D-512). A night plays fourteen thousand bot runs on three legs (D-507). The minutes are free while the repository stays public (D-4). From Phase 6 the repository is private, and minutes past the free quota cost money (D-456). M-2 gives 505 job seconds for the mean code PR. The build renders the audio, which adds to that time (D-432).
- Purchases: the Steam Direct fee, 100 USD, at Gate 2 (D-85, D-471). One month of the Starter plan of Sprite Fusion, 9 USD, for the art test before PR-34 (D-620). The Apple Developer Program costs 99 USD a year from PR-79 on (D-455, D-553). GitHub Pro comes before the switch to a private repository, so the required checks stay on `main` (D-456).
- No purchase: no code signing certificate for Windows (D-463), no asset license, and no font fee, because every font is OFL (D-104, D-122). Godot is free.

Measurements that answer the unknowns:

- M-1: tokens per PR from the harness usage reports, over the first ten code PRs in the order of section 8. Done on 2026-09-19. The number is the total context tokens of a PR (D-672). It sums the input, the cache write, the cache read, and the output of every session. The mean is 68.4 million tokens, and PR-1 is the worst at 127.1 million.
- M-2: CI wall time per PR, per platform, over the first ten code PRs in the order of section 8. Done on 2026-09-19. The number reads the last green `ci` run of each PR. The mean run spends 505 seconds across its jobs, and PR-5 is the worst at 614. The clock of the mean run is 133 seconds.
- M-3: the night run wall time and the crash and softlock counts, over the first seven nights (D-64).
- M-4: turns per encounter and party downs per dungeon by bot policy, on the first dungeon. Binds the resource numbers of D-35.
- M-5: the owner's play time from the first hub to the end of the arc, against D-56.
- M-7: the Deck frame time of the test scene under Forward+ and under Mobile, at the full load of D-160. Done on 2026-09-17 on an OLED Deck, at the frame of 1280 by 720. Mobile gave 2.78 ms at the 95th percentile of the full load, and Forward+ gave 3.33 ms. Each of the 20 stages held the target under both renderers, and the worst stage gave 4.55 ms under Mobile. It picked the Mobile renderer for PR-82 and gave the first effect budget (D-616, D-617, D-624).
- M-6: the Deck frame time on the first playable, against 60 frames per second (D-161). The readability of the 16-pixel font and the 32-pixel sprites at the scale that OQ-183 sets (D-92, D-228, D-621).
- M-8: the screen scale probe of D-621, on four screens. Done on 2026-09-18. The owner picked the world at 2x on every screen, so the frame holds 20 by 11.25 tiles (D-633). The Deck and the 27-inch 1080p screen take the UI at 2x. The 32-inch 1440p screen and the 27-inch 4K screen take the UI at 1x (D-639). The numbers come from the geometry of each screen, at 45 cm for the Deck and 70 cm for each desktop screen (D-636). The owner then ran a 27-inch 1080p screen, which takes a fit of 1.5x, and picked the UI at 2x there (D-638, D-639). That screen and the 27-inch 4K screen give the same apparent size at the same UI value, and the owner picked two different values. Thus the count of device pixels for each art pixel sets the UI value, and not the apparent size (F-77).

| Screen | Fit | Millimeters for one device pixel | A sprite at world 1x | A sprite at world 2x | A body line at UI 1x | A body line at UI 2x |
|---|---|---|---|---|---|---|
| OLED Deck, 1280 by 800, 7.4 inches | 1x | 0.1245 | 3.98 mm, 30.4' | 7.97 mm, 60.9' | 1.99 mm, 15.2' | 3.98 mm, 30.4' |
| 27-inch 1080p screen, 1920 by 1080 | 1.5x | 0.3113 | 14.94 mm, 73.4' | 29.89 mm, 146.8' | 7.47 mm, 36.7' | 14.94 mm, 73.4' |
| 27-inch 4K screen, 3840 by 2160 | 3x | 0.1557 | 14.94 mm, 73.4' | 29.89 mm, 146.8' | 7.47 mm, 36.7' | 14.94 mm, 73.4' |
| 32-inch 1440p screen, 2560 by 1440 | 2x | 0.2724 | 17.43 mm, 85.6' | 34.87 mm, 171.2' | 8.72 mm, 42.8' | 17.43 mm, 85.6' |

The apparent size of a sprite goes from 61 to 171 arcminutes across the four screens. One tile count for every screen gives that spread, and D-37 asks for it. Each number reads the line box of the body font, which is 16 pixels, and not the cap height of a glyph (F-71).

The M-1 numbers, in millions of context tokens, from the local record of each harness:

| PR | Claude Code | Codex | Both |
|---|---|---|---|
| PR-1 | 112.8 | 14.3 | 127.1 |
| PR-2 | 40.0 | 4.8 | 44.9 |
| PR-3 | 81.5 | 17.7 | 99.2 |
| PR-46 | 36.4 | 11.7 | 48.1 |
| PR-4 | 46.7 | 6.5 | 53.2 |
| PR-5 | 92.8 | 11.1 | 103.9 |
| PR-6 | 60.5 | 10.7 | 71.2 |
| PR-43 | 46.5 | 2.8 | 49.4 |
| PR-44 | 44.2 | 3.7 | 47.8 |
| PR-47 | 36.6 | 3.0 | 39.6 |
| Mean | 59.8 | 8.6 | 68.4 |

Claude Code wrote each of the ten PRs, and Codex reviewed each one, which explains the split (T-4). A cache read is most of each number, because every turn of a session sends the context again. The count reads the branch of each record, so work on `main` counts for no PR. PR-4 also holds the branch feat/pr-4-core-math-streams, which opened no PR. The Measures section of `docs/runbooks/session-context.md` says that an audit reads the record of each harness. No tool of this repository reads them (D-99).

The M-2 numbers, in seconds, from the last green `ci` run of each PR:

| PR | Linux | Windows | Mac | Shared | Every job | Run clock |
|---|---|---|---|---|---|---|
| PR-1 | 69 | 168 | 87 | 36 | 360 | 102 |
| PR-2 | 69 | 143 | 98 | 60 | 370 | 131 |
| PR-3 | 68 | 204 | 78 | 58 | 408 | 132 |
| PR-46 | 70 | 162 | 90 | 88 | 410 | 107 |
| PR-4 | 105 | 250 | 121 | 92 | 568 | 172 |
| PR-5 | 106 | 276 | 142 | 90 | 614 | 220 |
| PR-6 | 94 | 217 | 113 | 97 | 521 | 104 |
| PR-43 | 105 | 245 | 136 | 98 | 584 | 123 |
| PR-44 | 111 | 248 | 152 | 99 | 610 | 118 |
| PR-47 | 114 | 233 | 146 | 108 | 601 | 123 |
| Mean | 91 | 215 | 116 | 83 | 505 | 133 |

The run clock is shorter than the sum of the jobs, because the legs run at the same time. A shared job takes no leg and runs one time, such as `ste-check`, `det-lint`, and the coverage report. Windows takes two to three times the seconds of Linux, and 2.4 times at the mean. Each PR started the workflow 3 to 16 times, and the table reads the last green run alone.

## 5. Defect and finding register

Status: ✅ done (code merged, or "doc" for a document-only correction) · 🔧 planned (item listed) · ⚠ constraint (binds a pull request) · ❓ needs owner input · ⏸ out of scope · 🅿 parked.

| # | Finding | Date | Status |
|---|---|---|---|
| F-1 | The harness default adds a co-author trailer to commits, and the reminder repeats in every session. D-22 forbids it | 2026-09-12 | ✅ doc. `.claude/settings.json` sets empty strings. The rule sits at the top of `CLAUDE.md` |
| F-2 | The what-you-carry review gate and STE checker are C# tools, and D-1 chose Rust. Neither tool runs here | 2026-09-12 | 🔧 D-10 and D-15. D-99 replaced Rust with C#. PR-2 wrote the STE checker as new code, and PR-3 writes the review gate (D-101, D-277) |
| F-3 | Rust is absent from the development machine | 2026-09-12 | ✅ doc. D-99 removed Rust, and OQ-2 closed with no action |
| F-4 | The repository had no commit, so no branch and no PR could exist | 2026-09-12 | ✅ D-25. The owner made the root commit `6b899dd` with an empty `CLAUDE.md` |
| F-5 | The Python checker applies the 20-word limit to every numbered item, and the C# tool applied it under a Sequence or Procedure heading alone | 2026-09-12 | ✅ D-604 and PR-2. The limit reads every numbered item, under any heading, and the skill text states it |
| F-6 | D-39 took seeded dungeon variation on a replay premise, and D-46 removed the premise | 2026-09-12 | ✅ D-47. No variation. L-13 |
| F-7 | D-36 leaves a fallen character down until a hub, and a three-character party (D-31) then fights with two. No decision balances the short-handed party | 2026-09-12 | ⚠ D-58 gives a reserve and a swap at save points. Binds PR-16 and M-4 |
| F-8 | D-42 empties a caster's MP across a dungeon, and no decision gives a job a no-MP action | 2026-09-12 | ⚠ Binds PR-9 and PR-12. D-359 gives every character a basic attack with no MP cost |
| F-9 | D-48 sets the floor at 120 by 40, and a default macOS Terminal window is 80 by 24 | 2026-09-12 | ✅ doc. D-80 superseded D-48, D-103 superseded D-80, D-228 superseded D-103, and D-98 ended the terminal, so PR-7 has no size message |
| F-10 | D-62 puts the run record in the save, and a record grows without bound over 20 to 40 hours (D-30) | 2026-09-12 | ✅ PR-6 wrote the compaction rule: the record takes a new snapshot at each save, and it drops every intent before it (D-651) |
| F-11 | The interim checker read an HTML comment as prose. A fixture comment with a semicolon, a modal, a passive, and 30 words raised four findings. The automated pass of PR #1 found it | 2026-09-12 | ✅ PR-2. The command removes a one-line comment, and the rule MD 1 fails a comment across lines |
| F-12 | The session wrote in `CLAUDE.md`, the PR template, the skill, and OQ-1 that gitar was absent, on no evidence. The pass ran on PR #1 within a minute | 2026-09-12 | ✅ doc. D-66. Every claim about a tool needs a check |
| F-13 | The first interview fixed the language before the medium. Two pivots in one day, D-78 and D-98, reopened 30 decisions | 2026-09-12 | ✅ doc. D-99. L-14 |
| F-14 | D-88 chose curvature and bleed, and the SDL2 2D renderer of D-83 ran no shader | 2026-09-12 | ✅ doc. D-91, then D-99 moved the shader to Godot. OQ-19. D-618 on 2026-09-17 removed the CRT from the plan |
| F-15 | A 16 by 16 sprite did not divide the 10 by 20 text cell of D-82 | 2026-09-12 | ✅ doc. D-103 sets a 16-pixel tile and a 640 by 360 frame. D-228 later sets a 32-pixel tile and a 1280 by 800 frame |
| F-16 | D-7 chose RON, and C# has no RON reader | 2026-09-12 | ✅ doc. D-116, JSON with a schema |
| F-17 | The 32-color palette (D-89) had 13 free colors for eight elements and ten statuses | 2026-09-12 | ✅ D-121 grows it to 48, and D-181 to 64. PR-34 writes the 64 colors, and section 7.2 of `docs/roadmaps/area-art.md` names the color of each element and each status |
| F-18 | The full CRT (D-105) is on by default on the Deck (D-120) before any Deck measurement | 2026-09-12 | ✅ doc. D-618 on 2026-09-17 removed the CRT, so M-6 and Gate 2 read the text with no pass over it |
| F-19 | D-119 and PR-34 promised an atlas match byte for byte. The compressed bytes depend on the zlib build and the encoder, so the C# tool of PR-34 cannot reproduce them. The automated pass of PR #1 found it | 2026-09-12 | ✅ The match test of PR-34 decodes each committed page and compares pixels. The `--check` option of the `atlas` command reads the same rule |
| F-20 | The interim atlas tool kept the last of two palette entries with one key, in silence, against T-2. The automated pass of PR #1 found it | 2026-09-12 | ✅ The reader of the palette of PR-34 fails on a repeated key with the key and the index. A drawing that names a key of no color fails with the file, the row, and the column |
| F-21 | The plan gives each region one story arc (D-56), and the glossary defined an arc as "the story of one region". No text said how an arc relates to the main story of D-28, or what the first release ends on. An interview option read the gap as a faction that falls inside region one, and the owner refuted it | 2026-09-12 | ✅ doc. D-131: every plotline converges at the end of the game. The glossary now defines an arc as one part of the main story. D-133 resolves OQ-26: region one is a free prologue on Steam. The Phase 4 summary and Phase 5 now name the prologue. Binds the arc block of OQ-18 |
| F-22 | The interview options used Final Fantasy Tactics as a template, not a feel. Three recorded answers sit close to its plot devices: unpaid veterans turned bandit (D-127), a hidden power behind the politics (D-128), and church leaders who know the faith is a lie (D-137). The waystones (D-134) risk a fourth: stones that carry the evil | 2026-09-12 | ⚠ D-136 and D-140: keep the shapes, and ban the devices. The list lives in `docs/world/`. Binds every later option of OQ-18 |
| F-23 | The gates of PR-10 and PR-37 need a rendered screen: a screen test of a fixture battle, and two screenshots of the CRT toggle. The smoke job runs Godot with `--headless` on hosted runners (D-117). Godot proposal 5790 says that `--headless` "disables all rendering code", and the Godot docs name no way to capture an image in that mode. what-you-carry met the same wall: its contact sheet needs a window and runs on a desktop alone (its D-306) | 2026-09-12 | ⚠ D-172: a Linux CI job renders under Xvfb with a pinned Mesa, and desktop contact sheets show the real renderer at milestones. Binds the technical and graphics roadmaps, PR-10, and PR-37. Sources: the Godot 4.7 command line page and proposal 5790, read 2026-09-12. D-618 on 2026-09-17 removes the two screenshots of the CRT toggle, and the screen test of the fixture battle stands |
| F-24 | D-228 doubles the tile size after the art, effect, and light decisions of this interview. Every grid holds four times the pixels: a 32 by 32 frame is 1,024 characters of text, and a party member has about twelve frames plus normal-map overrides (D-184, D-199, D-200). The Deck lights and fills four times the pixels of a 640 by 400 frame | 2026-09-12 | ⚠ Binds the graphics roadmap, the Deck test of D-160 at the frame of 1280 by 720 (D-568), and M-6. The PNG import of D-107 matters more for hand edits |
| F-25 | The design critic of 2026-09-13 found four holes in play and saves. Gate 2 could not reach the two hidden jobs (C-1), a save point gave endless rest (C-2), a quit autosave could trap a run (C-3), and a Core patch would refuse old saves (C-4) | 2026-09-13 | ✅ doc. D-256, D-257, and D-258 close the first three, and D-268 later supersedes D-256. D-259 closes the fourth: a load reads the snapshot |
| F-26 | The critic found gates that cannot pass. No PR created the screen-test job of D-172, the PR-37 gate relied on a headless run that draws nothing, the PR-7 and PR-8 gates met small maps and routes per phase, and the Deck test of D-160 had no sequence step and no failure branch | 2026-09-13 | ✅ doc. PR-41 creates the job with fixed capture and fit tests at 1080 and 1440 rows. The gates of PR-7, PR-8, and PR-37 changed, and section 8 gains the Deck test. D-261: the owner sets a fallback only if the test misses 60. The Deck test of 2026-09-17 held the target, so D-261 never fired (D-616). PR-37 is retired (D-618) |
| F-27 | The critic found gaps in the records. 24 earlier rows lacked their revision notes, several lines named superseded values, and D-193 disagreed with D-202 on ambient effects. Four choices had no owner: the first turn from behind, the place of systems, audio, and release in the order, effect timings in frames, and D-171 against the rule of no conditional compilation in Core | 2026-09-13 | ✅ doc for the notes and the stale text. D-260, D-262, D-265, and D-266 settle the four choices |
| F-28 | The second critic pass of 2026-09-13 read the plan after the job system change and found 14 defects. A player choice could remove a cast member (C-1), the PR-12 gate needed the tasks of PR-19 (C-2), PR-42 came before its places (C-3), two notes overstated the aptitude count (C-4), and the law split between a license and a stamp (C-5). Stale text and notes stayed (C-6), and session readings had no owner (C-7). No PR drew the lead or wrote the tasks (C-8), a reserve swap gave fresh MP (C-9), and the watcher could stamp rites (C-10). OQ-41 gave the wrong cost of the death (C-11), three cases had no rule (C-12), words clashed (C-13), and the distance rule covers FFT alone (C-14) | 2026-09-13 | ✅ doc for C-2, C-4, C-6, C-8, C-11, and C-13. The session rejected one claim of C-6: D-282 refines D-268 and D-274 and does not revise them. D-301 to D-304 settle C-1, C-3, C-5, and the reading of D-274 in C-7. D-305 and D-306 settle the other two readings of C-7, D-307 settles C-14, and D-308 settles C-10. D-309 and D-351 settle OQ-48 and OQ-49 of C-12. D-356 settles C-9: a swap at a save point can bring fresh MP, and the balance must hold with it. D-363 and D-375 settle OQ-50 and OQ-51, the rest of C-12 |
| F-29 | PR-23 to PR-26 held four ids for dungeons two to four, which are three dungeons. The count came unchanged from v1, and no text said what the fourth id held | 2026-09-13 | ✅ doc. The second visit to the hanging cells (D-327) makes four dungeon builds after the first, and each id names one in the order of play (D-313) |
| F-30 | Before the rename, an audit of every document found stale text in the design, the registers, the world files, the skills, the agent files, and the runbooks. Section 8 still put PR #2 next and a font pick before PR-7. Three rules read the override set as "no code". The design-doc skill swapped two sections, and nine resolved questions lacked the later decisions that changed them | 2026-09-14 | ✅ doc. D-411. One docs PR fixes each item before the rename PR |
| F-31 | D-115 commits every rendered WAV file. With about 25 tracks for region one, at about 21 MB for a two-minute 16-bit stereo WAV at 44.1 kHz, that is over 500 MB, and each change to a track adds a full copy to the history | 2026-09-14 | ✅ doc. D-432: the build renders the audio, and the repository commits a hash of each render. D-443 and D-444 bring the count to about 20 tracks. Binds PR-38 |
| F-32 | The cost model listed the Steam Direct fee alone. Steam requires notarized macOS apps since 2019-10-14, and notarization needs the Apple Developer Program at 99 USD a year | 2026-09-14 | ✅ doc. D-455: the cost model gains the fee, and PR-79 signs and notarizes the macOS build (D-553) |
| F-33 | The release block found five gaps. D-54 made the repository public before D-133 made the later regions paid (L-6). D-170 named no place for a crash file, and the plan had no credits for the notices that the Godot license and the OFL need. D-62 named a config directory, and the Godot user folder on Linux is a data folder. D-464 added two arm64 builds that no CI leg tested (T-3) | 2026-09-14 | ✅ doc. D-456, D-473, D-467, D-465, and D-474. D-481 later removed the arm64 builds |
| F-34 | Steam needs at least five screenshots at 1920 by 1080 or larger in 16:9, and the frame was 1280 by 800 in 16:10 (D-228). D-229 let no screen see more of the map than the Deck, so a 16:9 screenshot needed bars or a crop | 2026-09-14 | ✅ doc. D-480: the game supports a 16:9 view, and a screenshot at 1920 by 1080 comes straight from play D-568 on 2026-09-16 makes the frame 16:9, so a screenshot comes from the 2x scale at 2560 by 1440. |
| F-35 | Two hash paths of .NET break Core rules. The hash classes "defer to the OS libraries", against G-1. The hash code of a string "is not guaranteed to be stable", and two runs of one program can differ, against T-7. Sources: `https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography` and `https://learn.microsoft.com/en-us/dotnet/api/system.string.gethashcode`, read 2026-09-14 | 2026-09-14 | ⚠ D-644 answers it: Core holds xxHash64 for the state hash and the stream split, and a SHA-256 for the content hash. PR-4 wrote the first one, and PR-5 writes the second (D-645). The det-lint of PR-46 refuses both .NET paths in Core (D-496) |
| F-36 | D-177 puts the content reader on the JSON support of .NET, and the det-lint of PR-46 bans reflection in Core (D-496). "System.Text.Json uses reflection by default". Source: `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-5. D-647 answers it on 2026-09-18: Core reads with a hand reader on `Utf8JsonReader`, and Core calls `JsonSerializer` nowhere. `Directory.Build.props` sets `JsonSerializerIsReflectionEnabledByDefault` to `false`, because a class library writes no `runtimeconfig.json` (PR #12 critic pass). A test of PR-5 reads the switch back in the test host. |
| F-37 | GitHub starts `pull_request_target` and `schedule` only from a workflow file on the default branch. The review gate of PR-3 and the night job of PR-49 run on these triggers, so neither check can run on the PR that creates it, against G-16. Source: `https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-500: each PR proves its command in Tests, and G-16 gains a note. ⚠ Binds PR-3 and PR-49 |
| F-38 | Two float facts of .NET meet the tools. A real literal with no suffix is a double, so a scan of words misses a float. The results of double math "might differ slightly by platform", and the CI legs mix x86_64 and Apple silicon. Sources: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types` and `https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-double`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-498: det-lint reads types through the Roslyn compiler library. D-502: a tool whose output a test compares on every leg uses integer math. ⚠ Binds PR-46 and PR-48 |
| F-39 | The default string order of .NET depends on the machine. A `SortedDictionary` with string keys uses `Comparer<T>.Default`, which uses `CompareTo`, a culture-sensitive comparison with the current culture. String sort order also "differs between NLS and ICU", and a new ICU version can change it. `area-core.md` named `SortedDictionary` for a fixed order and named no comparer. Sources: `https://learn.microsoft.com/en-us/dotnet/api/system.string.compareto`, `https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2.-ctor`, and `https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-4 and PR-46: every string order in Core uses an ordinal comparison, and det-lint fails any other string order in Core (G-4, T-7). Section 7.5 of `area-core.md` and the `csharp-conventions` skill state the rule |
| F-40 | The test command of `CLAUDE.md`, `dotnet test TheThingBelow.slnx --no-build --filter "Category!=Smoke"`, works in VSTest mode alone. In MTP mode, a solution needs `--solution`, and "xUnit.net uses `--filter-trait` while MSTest uses `--filter`, and each framework rejects the other's options". The package `xunit.v3` 4.0.1 depends on `xunit.v3.mtp-v2`. Sources: `https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test` and the NuGet API, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-1: OQ-75 picks the mode, and the test commands of `CLAUDE.md`, `AGENTS.md`, and the `csharp-conventions` skill follow the answer |
| F-41 | Four rules of GitHub Actions meet the CI plan. A schedule can come late at "the start of every hour", and "some queued jobs may be dropped". A public repository disables its schedules after 60 days with no activity. A workflow that a path filter skips leaves a required check "Pending". The merge queue serves repositories of an organization, and a personal account owns this repository. Sources: the external facts of `docs/roadmaps/area-ci.md`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-1 and PR-49: OQ-78 holds the required checks on a docs PR, OQ-81 the age of a night gate result, and OQ-82 the time of the night |
| F-42 | Two Godot export facts meet the CI plan. The export code walks `res://`, the project folder, alone, and `content/` lies outside that folder (D-118). No command-line option installs export templates, and the .NET template file holds 1,202,598,411 bytes. Sources: the external facts of `docs/roadmaps/area-ci.md`, read 2026-09-14 | 2026-09-14 | ✅ doc. D-508: the Game assembly carries the content files. ⚠ Binds PR-5 and PR-54: OQ-83 holds how CI gets the templates |
| F-43 | The night gate of G-22 blocked every PR after a failed night: the PR that fixes the night, and a docs PR too. A gate that no PR can pass breaks L-11 | 2026-09-14 | ✅ doc. D-510: a night on the head commit of a PR passes that PR. D-513: a docs-only PR passes the gate. ⚠ Binds PR-49 |
| F-44 | D-205 gives each backdrop three or four layers, and D-480 makes full-screen art cover the 16:9 view, about 1422 by 800 pixels. One grid of that size holds 1,137,600 palette keys, more than four times the size of `docs/decisions.md`. Region one has at least five places with fights (D-244, D-370), so at least 15 layers. D-475 drew the store images as grids too | 2026-09-14 | ✅ doc. D-516: a large picture places drawn pieces. ⚠ Binds PR-55 (D-518) D-568 on 2026-09-16 shrinks full-screen art to the frame of 1280 by 720. |
| F-45 | Three Godot defaults meet the pixel art. A canvas texture takes the Linear filter by default. The editor of 4.7 writes the stretch mode `canvas_items` into a new project, where "there is no longer a 1:1 correspondence between sprite pixels and screen pixels". `TileSetAtlasSource.create_tile` and `ImageTexture.create_from_image` report a failure in the log alone, and the second returns null. Sources: the external facts of `docs/roadmaps/area-art.md`, read 2026-09-14 | 2026-09-14 | ⚠ Binds PR-7: the project sets the Nearest filter, and Game checks each such call (T-2). `area-ui-input.md` sets the stretch |
| F-46 | Godot 2D light fails in silence in three ways. A light joins a frame only `if (cl->enabled && cl->texture.is_valid())`, and only the editor warns about a light with no texture. One canvas item takes 15 lights at most, because the loop stops at `light_count == MAX_LIGHTS_PER_ITEM - 1`, and one render takes 256, or 64 on a small GL uniform buffer. Neither loop prints a message, and a `TileMapLayer` draws 256 tiles as one canvas item by default. At the default `height` of 0, a light gives no light to a flat pixel of a normal-mapped sprite, and the tutorial says "increase the Height property". Sources: the external facts of `docs/roadmaps/area-effects.md`, read 2026-09-15 | 2026-09-15 | ⚠ Binds PR-56 and D-523: Game checks the texture of each light at load, the budget test fails more than 15 lights on one canvas item, and each light sets its height (T-2) |
| F-47 | D-188 reads HDR 2D as the way to glow light alone and keep sprites and tiles clean. The canvas shaders add each light with no upper clamp, and HDR 2D keeps values above 1.0. The glow threshold starts at 1.0, and in SDR it "needs to be decreased below 1.0 when using glow in 2D". So a bright light on a pale sprite can glow, and in SDR a bright art pixel can glow too. The Compatibility renderer of the screen tests "uses a different implementation" of glow. Sources: the external facts of `docs/roadmaps/area-effects.md`, read 2026-09-15 | 2026-09-15 | ⚠ Binds PR-59: OQ-102 holds how glow stays off sprites and tiles (D-188) |
| F-48 | D-232 sets the default fit: a whole-number upscale, then a smooth fit to the height of the screen. Godot has no mode that does both. With the integer scale mode, `Window::_update_viewport_size` floors the factor, and the docs say "The remaining space is filled with black bars on all four sides". The stretch mode `canvas_items`, which the editor of 4.7 writes into a new project, gives "no longer a 1:1 correspondence between sprite pixels and screen pixels". A `SubViewport` keeps its own size, because the scale factor of the root window "will not be applied" to it. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61: Game draws the world in a `SubViewport` at 1x and builds the two steps of the fit itself (D-232, D-568, F-45) |
| F-49 | Three font defaults of Godot meet the pixel font of D-263. Godot 4.7.2 has no method that loads a font from bytes, and `FontFile.data` holds the "Contents of the dynamic font source file", so Game sets that member from the bytes of its assembly (D-508). The default `antialiasing` is gray and the default `hinting` is light, and the docs say that a pixel font "should have their subpixel positioning mode set to Disabled", where the default is automatic. Font oversampling is on by default with the stretch mode `canvas_items`. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61: the load from bytes and the font settings, with a test that reads each one back (T-2) |
| F-50 | Five input facts of Godot meet the plan. A change to the input map at run time "is not saved (must be modified manually)", so the game saves each remap itself. The methods of `Input` "are not affected by [method Control.accept_event]", so an intent from a poll sees input that a menu already took, and the replay of D-493 then drifts. A default `ui_*` action "cannot be removed", and only its events change. The dead zone of a new action is 0.2 in the source, and the docs name 0.5, which is the value of the built-in actions alone. `JOY_BUTTON_A` "Corresponds to the bottom action button: Sony Cross, Xbox A, Nintendo B", so one constant needs three glyphs. Sources: the external facts of `docs/roadmaps/area-ui-input.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-61, PR-62, and PR-63: intents from events alone, a glyph set for each device, a saved remap, and one dead zone value (D-222, D-493) |
| F-51 | Four Godot defaults meet the tile map. `TileSet.tile_size` and `TileSetAtlasSource.texture_region_size` are both `Vector2i(16, 16)`, and this game draws 32-pixel tiles (D-228). `TileMapLayer.collision_enabled` and `navigation_enabled` are both `true`, so a layer makes physics bodies and navigation regions that no rule reads (G-1, G-23). The coordinates of a layer "are limited to 16-bit signed integers". Sources: the external facts of `docs/roadmaps/area-exploration.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-7: the tile size, the region size, and the two switches, with a test that reads each one back (T-2) |
| F-52 | The camera meets two facts that no page of the docs states. When the limit rectangle is smaller than the view, the camera centers the view: the source reads "Split the difference horizontally (center it)". The gate of PR-7 for a small map rests on that source alone. The same function carries a FIXME: "smoothing is not currently applied only once per frame / tick, which will result in some haphazard results". The docs add that the position of the node "doesn't represent the actual position of the screen". Sources: the external facts of `docs/roadmaps/area-exploration.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-7: a test locks the centering of a small map, and Game moves the camera from the tick of Core, never from the smoothing of Godot (D-203) |
| F-53 | D-534 takes an evaluator that simulates each legal action and the strongest reply of the other side, and no measurement of its cost exists. The cost grows with the count of legal actions times the replies. The same code runs on the Deck at 60 frames per second (D-161), and a night plays fourteen thousand runs through it (D-507). The plan holds no number until M-3, M-4, and M-6 | 2026-09-16 | ⚠ Binds PR-11: the PR reports the count of legal actions and the time of a turn before Gate 2, and a miss changes the depth or the profiles (G-14) |
| F-54 | The end of the job system left the stats of a character with no source. D-34 gives the character level "for stats", and D-77 gave the rest to job multipliers. D-268 removed the jobs, and no later row replaced those multipliers. No PR could set the health, the MP, the attack, the defense, or the speed of a character | 2026-09-16 | ✅ doc. D-537: each character carries its own stat curve in content, and PR-30 balances the eight curves against the M-4 band |
| F-55 | A scene step can set a story flag (D-173), and the scene runner lands in Phase 2. PR-18, which defines the flags and the condition form, sat in Phase 3. PR-14 and PR-35 also read a condition in Phase 2, for a hub line and a closed route (D-59, D-113) | 2026-09-16 | ✅ doc. D-544: PR-68 takes the flag set and the condition form with the scene runner, and PR-18 keeps the branches and the choice effects. ⚠ Binds PR-68 |
| F-56 | Two Godot audio calls meet the plan. `AudioStreamWAV.load_from_buffer` returns an empty reference on data that is not WAV, and it prints the reason to the log alone, as `ImageTexture.create_from_image` does (F-45). `AudioStreamPlayer.get_playback_position` "Returns 0.0 if no sounds are playing", and its note says that "The position is not always accurate, as the [AudioServer] does not mix audio every processed frame". Sources: the external facts of `docs/roadmaps/area-audio.md`, read 2026-09-16 | 2026-09-16 | ⚠ Binds PR-69 and PR-70: Game checks every stream that it makes and fails with the id of the render, and the audio player holds its own count for the crossfade of D-428 and the resume of D-429 (T-2) |
| F-57 | The third design-critic pass read the merged plan of PR #11 in five slices and found 52 defects. Seven exit tests needed a PR that lands later, such as a shop flag in PR-65 before the flags of PR-68. A party could leave a dungeon and return for fresh enemies, fresh MP, and free health. No PR built the enemy record, the starting row, the party join, the party and status windows, or the sealed gallery. D-538 and D-544 clashed, a workflow change could merge on its own label, and "scene" named two concepts. Five area citations pointed at text that the rebuild of D-554 removed | 2026-09-16 | ✅ doc. D-555 to D-572 answer the owner questions, and PR #12 fixes the rest. D-647 answers OQ-179 on 2026-09-18 |
| F-58 | The harness gives no session id and no record of the conversation to a check. No check can prove that a session is clean or that it worked on one PR alone. A check can read the diff, the changed paths, and the PR description | 2026-09-16 | ⚠ Binds PR-3: the document rules read the diff and the description alone (D-579). The session and the owner enforce the binding of D-576 |
| F-59 | A token audit of 20 sessions found that each call of a harness sends the whole context again. The Claude Code sessions sent a median of 350k tokens in each call. Whole reads of the read order, one call for each STE check, and one call for each GitHub poll held most of the cost | 2026-09-16 | ✅ doc. D-583 to D-591 and `docs/runbooks/session-context.md` |
| F-60 | The Godot editor writes a target framework into a `.csproj` that has none, and Godot 4.7.2 writes `net8.0`. The scaffold sets .NET 10 in `Directory.Build.props` (D-99), and the editor wrote `net8.0` into the Game project over it. The Godot build then failed with `NU1201`, because Core builds for `net10.0`. The editor also leaves a `.csproj.old` backup. **Refuted in part on 2026-09-16:** this row first said that the editor build gave an exit code of 0 on that failure. That measurement read the code of `tail` through a pipe, and not the code of the editor. A direct run gives an exit code of 1 | 2026-09-16 | ✅ PR-1: the Game project pins `net10.0` in its own file, and `.gitignore` holds `*.csproj.old`. The `smoke` target of the Makefile writes each log to a file, and never through a pipe. The log check of the smoke job stays as a second guard |
| F-61 | A coverage run instruments the copy of each assembly in the test output folder. Coverlet added `System.Threading` and `System.Threading.Thread` to the Core copy, and the reference test of G-1 failed in the coverage run and passed in the plain run. The compiler also writes no metadata entry for a project reference that no code uses, so the metadata alone cannot see an added reference | 2026-09-16 | ✅ PR-1: the reference test reads the file that the Core project built, through `MetadataReader`, and a second test reads the project file for a declared reference (G-1, G-13) |
| F-62 | The macOS archive of the Godot .NET editor holds `Godot_mono.app`, and not `Godot.app`. The find pattern of the smoke job matched no file, and the job failed with `holds 0 Godot executables` (run 35169279617, 2026-09-16) | 2026-09-16 | ✅ PR-1: the pattern reads `*.app/Contents/MacOS/Godot`, and the step still fails when the count is not one (T-2) |
| F-63 | The git-bash of the `windows-2025` image carries `sha512sum` and no `shasum`. The checksum step failed with `shasum: command not found`, and its message named a checksum mismatch, which was wrong (run 35169279617, 2026-09-16) | 2026-09-16 | ✅ PR-1: the step reads the digest with `sha512sum` or `shasum`, names both values on a mismatch, and fails with its own message when the image carries neither (T-2) |
| F-64 | A headless Godot session whose managed assembly does not load never reaches `Quit`, and it runs without end. The session writes `Cannot instantiate C# script` and then waits. With `--quit-after` the same session ends with an exit code of 0 and no success line, so the exit code hides the fault. The `smoke` target of the Makefile held two faults that compose: the session had no frame limit, so this trigger made the target run without end, and the target read no log, so a session that ends with an exit code of 0 passes it. A frame limit alone turns the first fault into the second, which is a false pass | 2026-09-16 | ✅ PR-1: the smoke session of the Makefile and of CI runs with `--quit-after 600`, and each one fails when the success line is absent. Both parts are needed. A broken session now fails in about 6 seconds, and not at the time limit of the job (T-2) |
| F-65 | Godot.NET.Sdk writes the build output of a Godot project to `TheThingBelow.Game/.godot/mono/temp/bin/<configuration>/`, and the `bin` folder of the project stays empty. That output folder holds `GodotSharp.dll` from the NuGet restore, so a build of the solution gives the Godot assembly on each CI leg with no Godot editor. Read from the `OutputPath` property of the project on 2026-09-17 | 2026-09-17 | ✅ PR-46: `DetLintCommand.GameOutputFolder` reads that folder, and the text rule of det-lint sees each Godot type through it (D-614). The `det-lint` job and `make verify` build before the lint |

| F-66 | The Deck sweep of 2026-09-17 found no limit. Each of the 20 stages held 60 frames per second under both renderers, and the heaviest stage, 15 lights with 8192 particles and a transition, gave 4.55 ms at the 95th percentile under Mobile, against 5.00 ms under Forward+. The sweep stopped at 15 lights because of F-46, and it stopped at 8192 particles by its own table. So the effect budget of D-523 holds the largest load that the test measured, and not the ceiling of the machine | 2026-09-17 | ⚠ Binds PR-56 to PR-60 and D-617: each budget row is a floor, and a row rises only with a new measurement before it and after it (G-14) |
| F-67 | A 32-pixel sprite at 1x covers 4.0 mm on an OLED Deck, about 30 arcminutes at 45 cm. A 16-pixel sprite of a Game Boy Advance covers 4.09 mm, about 50 arcminutes at 28 cm, so the Deck at 1x shows about 60 percent of that apparent size. The body font of D-228 gives about a 10-arcminute glyph, near the 9-pixel floor of D-459. D-228 never set this scale: the option that it refused holds 40 tiles across the frame and the same size on screen | 2026-09-17 | ✅ doc. D-633 answers the world half: the world draws at 2x, and the frame holds 20 by 11.25 tiles. M-8 records each number. The UI half reads D-635 and the row of PR-61 |
| F-68 | On macOS the full screen gives the true pixel count, and a window does not. The Mac reported a screen of 3840 by 2160 at full screen, and the system reported a screen scale of 2 there. A windowed run reported 1280 by 720 alone, so exit test 4 of section 7.8 fails for such a run. A picture of the frame in a viewer proves nothing, because the system maps a picture pixel to a point and not to a device pixel | 2026-09-18 | ✅ doc. The probe of D-621 draws at full screen, and each report names the two pixel counts. ⚠ Binds PR-41: a screen capture on macOS reads the window, so the job pins the pixel count (D-172) |
| F-69 | The dialogue box of the mock frame holds 156 characters in one line at the UI scale of 1x, and 76 at 2x. The count does not depend on the fit of the frame, because the fit cancels. Thus the limit of 80 characters in the `game-text-style` skill held at the UI scale of 1x alone | 2026-09-18 | ✅ doc. D-635 drops the dialogue limit and the lore entry limit to 76 characters |
| F-70 | The export templates of the Deck test cover each platform of D-481, so the Windows export of the probe cost no download. One install of the templates serves every export of the same Godot version | 2026-09-18 | ✅ doc. D-629 added the 32-inch screen at no cost. 🔧 PR-54 caches the templates for CI (D-596, F-42) |
| F-71 | The probe measures the line box of the body font, which is 16 pixels, and not the cap height of a glyph. Thus it gives a larger apparent size than the glyph number of F-67. The two numbers measure two things, and neither one is wrong | 2026-09-18 | ✅ doc. M-8 names the line box in each row. A rule about a text floor reads the glyph, so D-459 keeps the glyph number |
| F-72 | The REF 2 rule of `ste-check` resolves a bare file name in backticks as the one file of the checkout that ends with that name. The spike added a second `TheThingBelow.Game/project.godot`, so that name matched two files, and the rule gave a finding on text that was correct the day before. The pre-commit hook then refused the commit | 2026-09-18 | ✅ doc. D-637: two files now cite `TheThingBelow.Game/project.godot`, and a second project of the same engine takes the exact form. The `ste-writing` skill holds the rule |
| F-73 | A Godot export drops each file that the engine does not import. The `.grid` files and the map file of the probe left the first two exports, and each exported build stopped at the first absent file. A run of the editor did not show the fault, and a run of the export showed it at once. The palette came through, because Godot imports JSON | 2026-09-18 | ✅ doc for the spike: each preset now holds an include filter. The export of the game does not meet this rule, because Game embeds `content/` and each font in its own assembly, and D-508 states that content needs no export filter. The handover of the spike claimed the opposite, and D-508 refutes that claim |
| F-74 | The macOS export of Godot needs the universal binary format and the ETC2 ASTC import setting. An export with another pair of settings fails | 2026-09-18 | ✅ doc for the spike (D-632). 🔧 PR-54: the macOS export of the game takes the same two settings (D-481, D-482) |
| F-75 | The Steam Deck in desktop mode has no keyboard, and the probe needed six commands. Each command took a button of the pad as well: A, B, X, Y, View, and Menu | 2026-09-18 | ✅ doc (D-631). ⚠ Binds PR-39: the Deck checklist reads every screen with the pad alone (D-565) |
| F-76 | The probe of the first build computed the fit of the frame with integer division, so a 1920 by 1080 screen drew at 1x with wide bars. It never drew the fit of 1.5x that D-573 gives. Answer 2 and answer 3 of OQ-183 disagree on that screen: the floor of 2 device pixels for each art pixel asks for the UI at 2x, and the words of answer 3 give the UI at 1x and 1.5 device pixels | 2026-09-18 | ✅ doc. D-638: the probe gained two fit modes, and the owner runs a 1920 by 1080 screen. ✅ doc. D-639 sets the boundary at a fit of 2x, from the run of a 27-inch 1080p screen |
| F-77 | The 27-inch 1080p screen and the 27-inch 4K screen give one line of body text the same apparent size at the same UI value. Both are 27-inch 16:9 screens at 70 cm, and the frame fills the same glass, so one art pixel covers 0.4670 mm on each. The owner picked the UI at 2x on the first and at 1x on the second. Thus the apparent size does not set the UI value, and the count of device pixels for each art pixel sets it: the 1080p screen gives 1.5 device pixels at the UI scale of 1x, and the 4K screen gives 3 | 2026-09-18 | ✅ doc. D-639 and G-28 take the floor of 2 device pixels as the rule. It also refutes the band of apparent size that the first three runs suggested: the fourth pick sits at 73.4 arcminutes, outside that band |
| F-78 | The property `JsonSerializerIsReflectionEnabledByDefault` in `Directory.Build.props` reaches every program of the solution. The run of PR-5 proved it: 27 tests of the review gate then failed with `Reflection-based serialization has been disabled for this application`, because the fixture wrote its file with `JsonSerializer`. The claim of the PR #12 critic, that the property on a class library reaches no program, holds for Core alone | 2026-09-18 | ✅ D-647 puts the property in `Directory.Build.props`, and PR-5 writes the fixture file with `Utf8JsonWriter`. A test reads the switch back in the test host |
| F-79 | A project reference from Tests to Game breaks the det-lint fixtures. `ReferenceSet.WithOutputOf` reads the assembly list of the running process as the framework set, and it then holds `GodotSharp.dll` and the two project assemblies. Each Game fixture thus compiles against no project assembly, and the run stops with `holds no assembly beside the framework`. Five tests failed this way in PR-5 | 2026-09-18 | ✅ PR-5 takes no such reference. The embedded-content test loads the file that the Game project built, as `CoreReferenceTests` does (F-61) |
| F-80 | `dotnet format` on Windows writes the line ending of the machine when it rewrites a line, and `.gitattributes` pins every file to one line feed. A comment between the arrow of an expression body and its expression makes the tool rewrite that line, so the format check fails on the Windows leg and passes on the Mac. Three lines of `TheThingBelow.Tests/SaveFolderTests.cs` failed this way in PR-43, and the same run passed every other check on Windows | 2026-09-18 | ✅ PR-43 gives that test a body with braces, and the comment sits inside the body. The `csharp-conventions` skill holds the rule |
| F-81 | Section 7.15 of `docs/roadmaps/phase-1-foundations.md` pointed PR-44 at section 7.10 of `area-release.md`, which holds the Deck verification pass of PR-39. The crash file work of PR-44 sits in section 7.1 of that file, the game version. PR-44 read the wrong section first | 2026-09-18 | ✅ PR-44 names section 7.1. The reference check of `ste-check` reads a path and not a section number, so a reader of each pointer is the one guard (D-605) |
| F-82 | The Tools scan of `det-lint` asked `ReferenceSet.WithOutputOf` for the build output of Tools. The command is the Tools program, so the framework list of its own process already holds each of those assemblies, and the method added none and failed with "holds no assembly beside the framework". No PR reached the scan before, because each folder of D-502 was absent | 2026-09-18 | ✅ PR-47: the Tools scan takes `ReferenceSet.Framework()`, as the Core scan does, and a test writes a float in a folder of D-502 and one outside it |
| F-83 | Five Tools commands read an empty option value with no check at the parse. The value reaches `ArgumentException.ThrowIfNullOrEmpty`, and the catch filter of the command reads that type nowhere. A probe of every command found seven option values with no parse check. Five of the seven end with a stack trace and the exit code of a crash: `ste-check --root`, `det-lint --configuration`, `replay-identity --root`, `content-hash --root`, and `review-gate --pull-request`. The other two give a message that names no empty option: `det-lint --root` names an absent folder, and `review-gate --head-files` names an access fault of the path. The automated pass of PR #33 found the shape in the `atlas` command | 2026-09-19 | ✅ PR-34 reads an empty option value at the parse of the `atlas` command. PR-87 gives every other command the same parse, through the one helper of D-679 (D-678). The first record of this row named the `content-hash` command alone. It also gave two counts that a later probe refuted, and the review of PR #35 found the first of them |
| F-84 | PR #33 merged with no review record at `docs/reviews/pr-33.md`, so it took no cross-provider review. The `review-gate` check passed, because RG 3 reads the verdict of a review file that exists and reports nothing when the file is absent. T-4 and D-17 ask for the record on every PR outside the override set | 2026-09-19 | ✅ doc. Every claim of this row is false, and D-680 records the refutation with its evidence. The record `docs/reviews/pr-33.md` arrived in the squash commit 3204545 of PR #33. It names Claude Code as the author, Codex as the reviewer, and the verdict for the effective head `d1b2305`. RG 3 faults on an absent file, and `TheThingBelow.Tools/ReviewGate/ReviewRecordRules.cs` has one commit, 9787b2d of PR #21. The gate of PR #33 faulted at head `d1b2305` and passed at head `320a9bd`, so it showed both halves of the behavior. No PR builds the check, because it ships already |

## 6. Guardrails (the safety contract for every PR)

### 6.1 Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins (D-5): T-5, T-2, T-3, T-4, T-7, T-1. T-6 is absolute and never conflicts.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep. Two concrete cases before any abstraction. No clever one-liners. Tune only on measurement.
- **T-2. Zero silent failures.** No swallowed error. An absent value is an error, never a zero. Every error carries its context. Assertions stay on in shipped builds.
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it. The review file records the findings (D-17). A PR in the override set that changes no decision row merges without a review when the `review-override` label is on (D-16, D-71, D-239, D-401, D-560).
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
19. **G-19.** Every screen designs to one 16:9 frame of 1280 by 720 with 32-pixel tiles (D-568). The Steam Deck is the readability floor, with black bars above and below (D-92, D-228). The world draws at 2x, so the frame holds 20 by 11.25 tiles (D-633). Every other screen shape shows black bars too. A desktop at 1920 by 1080 must look good, and the fit of D-232 holds that rule (D-568).
20. **G-20.** Every player string follows the `game-text-style` skill, and the owner approves each text batch in its PR (D-57, D-63).
21. **G-21.** Every enemy profile validates at load, and a profile that can never act fails the load (D-65, T-2).
22. **G-22.** The night gate is green before merge, once PR-49 creates it (D-496). It needs a success record from a night inside 48 hours (D-64). A night on the head commit of a PR passes that PR, and a docs-only PR passes the gate (D-510, D-513).
23. **G-23.** Godot physics, timers, and navigation never feed the simulation. The camera, the shader, the audio, and the input map live in Game (D-100, D-106).
24. **G-24.** Every sprite, tile, and portrait is a text grid in content. The atlas tool renders the PNG, and a test proves the committed atlas matches (D-107). A normal map comes from the grid, and its atlas gets the same test (D-184). A drawing file is JSON with its rows as strings, and a large picture places drawn pieces (D-515, D-516).
25. **G-25.** Every content batch the owner approves, sprites and text alike, appears in its PR description in full (D-57, D-107). A tool renders each art batch as review sheets, and the session attaches them with `gh` (D-514).
26. **G-26.** One session works on one PR, and the PR holds its tests, its documents, its review records, and its handoff. No later PR carries them, and no PR only records a merge (D-576 to D-578).
27. **G-27.** Every effect draws with the palette of 64 colors and hard edges, and no smooth gradient. The rule covers each particle, the fog, the glow, and each transition (D-181, D-622). The owner reads each new effect as its PR lands (D-623).
28. **G-28.** The UI never draws below 2 device pixels for each art pixel (D-639). A display setting gives the player two UI values, 1x and 2x. The default is the smallest value that meets the floor. A frame fit below 2x takes 2x, and a fit of 2x or above takes 1x. Each UI layout holds at both values, on every screen shape (D-640). A screen test captures each screen at both values (D-172).

## 7. Roadmap

Five phases. Gate 1 is a foundation gate with no play. Gates 2 to 5 are builds that the owner plays on the desktop and on the Deck. Each has a written exit test and a sign-off on feel (D-52, D-92). Ids: PR-# code changes, M-# measurements.

This section is the high-level roadmap (D-554). Each phase below gives its gate, its items in order, and one line for each item. The phase file of that phase gives each PR its scope, its exit tests, its review focus, and its questions (D-144, D-487). An area file says how one area works and which PR builds each part. The index of both sets is `docs/roadmaps/readme.md`.

An item that kept its purpose through the pivots kept its number. PR-22 and PR-32 are retired, and no later item takes either id (G-10). The new ids of D-486 run from PR-43 to PR-79. PR-80 to PR-86 are in use, and a new item takes the next number after them.

### Phase 1: Foundations (gate: every CI leg green with an identical state hash, the smoke session green, docs and PR gate live, no play)

Phase file: `docs/roadmaps/phase-1-foundations.md`.

1. Owner: enable the repository setting that requires a SHA pin for each action (D-511). Done on 2026-09-14.
2. Owner and a session: the Deck test, which picks the renderer and measures the effect budget (D-160, D-523). Done on 2026-09-17.
3. Owner: run that test on the Linux export (D-458). Done on 2026-09-17: the run gave M-7, the Mobile renderer, and the first effect budget (D-616, D-617).
4. PR-1: the solution, the four projects, the Makefile, the hook, and the build, test, format, smoke, and STE jobs (D-118, D-217, D-506). It sets Forward+ as a provisional renderer (D-599).
5. PR-82: the Mobile renderer that the Deck test picked, which is one line of `TheThingBelow.Game/project.godot` (D-599, D-616).
6. PR-2: the `ste-check` command in C#, which replaces the Python script (D-10, D-101).
7. PR-3: the `review-gate` command and its workflow, with the document rules (D-15, D-500, D-579).
8. PR-85: the result of the Deck test, the removal of the CRT, and two tests before PR-34 (D-616).
9. Owner and a session: the screen scale probe on three screens, right after PR-85 (D-621, D-625).
10. PR-86: the answers of the probe, and the close of OQ-183 (D-626 to D-640).
11. Owner: require the checks on `main` (OQ-3).
12. PR-46: `det-lint`, before the first Core code (D-496, D-498).
13. PR-4: integer math, the random streams, the state hash, the simulation version, and the `replay-identity` job (D-169, D-504).
14. PR-5: the content reader, the content ids, the content hash, the string table, and the content embed (D-116, D-495, D-508).
15. PR-6: the tick, the intents, the run record, replay, and the debug seam (D-164, D-260, D-493, D-650 to D-653).
16. PR-43: the Storage project, the snapshot versions and migrations, and the save files (D-491, D-494).
17. PR-44: the crash files and the log files (D-170, D-179, D-491, D-658 to D-662).
18. PR-47: the PNG reader and writer, right before the atlas (D-176, D-496).
19. PR-34: the `atlas` command, the drawing files, the palette of 64 colors, and the atlas index (D-107, D-181, D-517, D-666).
20. M-1: the harness usage of each of the first ten code PRs. Done on 2026-09-19 (D-672).
21. M-2: the CI wall time of each job of the first ten code PRs. Done on 2026-09-19.
22. PR-87: the empty option value of every Tools command, before Gate 1 (D-674, D-677, D-678).
23. Owner and a session: the Sprite Fusion test of the art, after M-2 (D-620, D-675).
24. **← GATE 1 (foundation).**

> *In plain English:* this phase builds the machinery and the checks, and nothing that a player can see. At the end of it, four computers play the same run and agree on one number.

### Phase 2: First playable (gate: the owner plays the village, one hub, and one dungeon with lessons and a shop, on the desktop and on the Deck, D-51, D-92, D-268, D-362, D-369)

Phase file: `docs/roadmaps/phase-2-first-playable.md`. This is the largest phase: 46 PRs, and 43 of them land before Gate 2. Each system, each tool, and each group of screens takes an id of its own (D-486, G-8).

1. Owner: set the fonts, Terminus TTF and Terminus TTF Bold (D-263, D-264).
2. PR-54: the export job, right before PR-7 (D-449, D-503).
3. PR-61: the 16:9 frame, the fit, the fonts, the text helper, the input, and the crash message (D-524, D-559, D-561, D-568).
4. PR-7: the map file, tile-locked movement, sight, the walked-tile record, the camera, and the map scene (D-106, D-528, D-566, D-567).
5. PR-45: the debug assembly and the console, right after PR-7 (D-492).
6. PR-41: the screen-test job under Xvfb, with its committed baseline (D-172).
7. PR-8: the enemies and the patrols on the map (D-37).
8. PR-9: the encounter state, the timeline, the actions, the rows, the wipe, and the hand-off (D-29, D-376, D-377, D-531).
9. PR-80: the enemy record, with the stats and the ability ids of each enemy (D-557).
10. PR-66: the eight elements and the ten statuses (D-74, D-75, D-533).
11. PR-55: the large pictures, right before PR-10 (D-516, D-518).
12. PR-10: the battle scene, its message line, and its backdrop (D-111, D-213).
13. PR-48: the normal maps and their review sheet, right before PR-56 (D-184, D-521).
14. PR-56: the light setups, the shadows, and the effect budget (D-183, D-520, D-523).
15. PR-63: the settings screen, the four accessibility settings, and a versioned settings file (D-214, D-526, D-570).
16. PR-57: the effect files, the particles, and the battle effects (D-182, D-186).
17. PR-58: the four ambient kinds of region one (D-187).
18. PR-59: the glow on fire, spells, and waystones (D-188).
19. PR-60: the ten transitions and their table (D-195, D-196).
20. PR-11: the evaluator, the enemy profiles, and the groups, with the cost of a turn (D-65, D-534, F-53).
21. PR-67: the character level, the experience, MP, and the stat curves (D-34, D-42, D-536, D-537).
22. PR-62: the menu windows, the party and status windows, the dungeon map screen, and the notices (D-211, D-558, D-567, D-569).
23. PR-68: the story scene format and runner, the join step, the flags, and the conditions, before PR-12 (D-541, D-544, D-556, D-563).
24. PR-50: the screenplay tool, right after PR-68 (D-173, D-545).
25. PR-12: the lessons, the slots, the forms, and the aptitudes (D-272, D-356, D-539).
26. PR-13: the six gear slots, the items, and the pack (D-44, D-382).
27. PR-14: the hub map, the NPCs, the rest, the save, and the party and lesson swaps (D-59, D-112, D-356).
28. PR-65: the shop and the gold economy, after PR-13 (D-60, D-530).
29. PR-36: the dialogue box, the portraits, and the story scene on screen (D-114, D-223).
30. PR-15: the headless runner, the two bot policies, and the bot job (D-64, D-505).
31. PR-49: the night job and the `night-gate` command, right after PR-15 (D-496, D-507).
32. Owner: require the bot and `night-gate` checks on `main` after their first runs.
33. PR-16: the treasure, the doors, the keys, and the save points (D-41, D-555).
34. PR-64: the traps, the hazards, and the statuses that last on the map (D-390, D-529).
35. PR-35: the region map of nodes and routes (D-113).
36. PR-38: the synthesizer, the two note formats, the render hashes, and the `listen` command (D-432, D-438).
37. PR-69: the audio player, the four buses, and the mute (D-435, D-546).
38. PR-70: every rule of what plays when (D-413, D-546).
39. PR-71: the sound room in a development build (D-439, D-546).
40. PR-51: the PNG import for a hand edit (D-107, D-497).
41. PR-52: the map preview as a PNG (D-165, D-497).
42. PR-53: the tile-edge tool and the edge files (D-204, D-501).
43. PR-72: the music, the themes, and the sounds of the first playable (D-549).
44. PR-17: the village, the mining town, and the hanging cells as content (D-362, D-369, D-370).
45. M-3: the wall time of each leg, and the crash and softlock counts of seven nights (D-507, D-509).
46. M-4: the turns of each encounter and the party downs of each dungeon, by policy.
47. M-6: the frame time and the readability on the Deck, at the scale of OQ-183 (D-161, D-621).
48. Owner: set the M-4 band from the M-4 numbers, before the sign-off (D-571).
49. **← GATE 2 (first playable).**
50. PR-74: the capture, which replays a record into frames and audio (D-476, D-551).
51. PR-75: the store text and the checklist of the owner steps (D-452, D-550).
52. PR-76: the store art and the five screenshots (D-475, D-550).
53. Owner: pay the Steam Direct fee, and put the store page public as Coming Soon (D-471).

PR-37 is retired. The CRT pass of the first plan has no purpose after D-618, and no later item takes the id (G-10).

> *In plain English:* this phase turns the machinery into a game. It ends when the owner walks a village, fights in a mine, and says whether it feels right. Then the game gets a public page on Steam.

### Phase 3: Story systems (gate: the owner plays a branch that closes a route and a hub that changes with an earlier choice, D-329)

Phase file: `docs/roadmaps/phase-3-story-systems.md`.

1. PR-18: the branch conditions and the four choice effects (D-40, D-301, D-329).
2. PR-19: the quest state, the rumor board, and the personal tasks (D-59, D-538).
3. PR-20: the scripted boss phases over the evaluator, on a fixture boss (D-65, D-564).
4. PR-21: the switches, the blocks, the light and dark, and the secrets (D-41).
5. **← GATE 3 (story systems).**

PR-22 is retired. Jobs five to eight have no purpose after D-268, and no later item takes the id (G-10). PR-42 in Phase 4 takes the lessons of region one (D-304).

> *In plain English:* the game learns to remember what you chose and to answer it. Bosses gain their set pieces, and dungeons gain their puzzles.

### Phase 4: Region one content (gate: the owner plays region one end to end on the desktop and on the Deck and signs off, D-56)

Phase file: `docs/roadmaps/phase-4-region-one.md`. Each item here is content, and the balance pass is the one item that moves a number that a replay reads.

1. PR-23: the deep mine, and the sprite frames of Ottild and Elio (D-313, D-342).
2. PR-24: the second visit to the hanging cells (D-327, F-29).
3. PR-81: the sealed gallery, the fifth dungeon, where the flight begins (D-343, D-575).
4. PR-27: the second hub, the refuge of the old faith, right after PR-81 (D-28, D-574).
5. PR-25: the border fort.
6. PR-26: the ice crossing.
7. PR-42: the lessons of region one, across the eight kinds (D-275, D-304).
8. PR-73: the rest of the music and the sounds of region one (D-549).
9. PR-28: the arc of region one, the first batch (D-56, D-350).
10. PR-29: the arc of region one, the second batch (D-352, D-355).
11. PR-77: the credits roll, right after PR-29 (D-467, D-552).
12. PR-30: the balance pass over the numbers of D-35, D-60, D-382, and D-388.
13. M-5: the play time of the owner from the first hub to the end of the arc (D-56).
14. **← GATE 4 (region one).**
15. The trusted players play the CI build artifacts and send their notes outside Steam (D-469).

> *In plain English:* the free prologue takes shape. Five more dungeon builds, a second town, their lessons, the story, and the numbers tuned by robots and by play.

### Phase 5: First release, the free prologue (gate: a tagged build on GitHub that a fresh machine runs, then the Steam demo on the Deck)

Phase file: `docs/roadmaps/phase-5-first-release.md`.

1. PR-31: the release workflow, the GitHub Release, and the release notes (D-53, D-453, D-457).
2. PR-33: the title screen, the settings entry, the version line, the credits screen, and the exit (D-427, D-454, D-467).
3. Owner: join the Apple Developer Program (D-455).
4. PR-78: the Steamworks binding, the start, and the controller type call (D-460, D-553).
5. PR-39: the Steam Deck verification pass, which proves each check of Verified (D-459, D-565).
6. PR-79: the signature and the notarization of the macOS build in CI (D-455, D-553).
7. Owner and a session: the shot list and the cut of the first trailer (D-476).
8. PR-40: Auto-Cloud, the demo app, and the Steam Linux Runtime (D-458, D-461, D-478).
9. Owner: request the Deck compatibility review from Valve (D-565).
10. **← GATE 5 (first release).**

PR-32 is retired. The fallback pass of the first plan has no purpose after D-98, and no later item takes the id (G-10).

> *In plain English:* the prologue becomes something a person downloads and runs. Then it becomes a free demo they find on Steam and play on the Deck.

### Phase 6: Region two and later

Parked until Gate 5, with no phase file yet. Each later region repeats Phase 4 with a roadmap of its own (D-144). Before the first content of a paid region, the repository goes private (D-456). Phase 6 also plans the achievements of the full game and its one Next Fest (D-466, D-472).

> *In plain English:* the paid part of the game starts only after the free part is out and the owner knows what it cost.

## 8. Sequence (strict order, single owner)

Section 7 gives the same order inside each phase, with a link to each phase file. This list is the one strict order across the phases, and it holds each owner step and the current position.

1. Owner: create no label, install no tool. gitar and the label exist (D-66, D-67).
2. PR #2 to PR #10 merged on 2026-09-14, and PR #11 to PR #13 on 2026-09-16 (D-554, D-555 to D-575). PR #14 sets one PR for each session (D-576 to D-582).
3. Owner: enable the setting that requires a SHA pin for each action (D-511). Done on 2026-09-14.
4. Owner and a session: the Deck test of D-160 on the Linux export (D-458, D-523). Done on 2026-09-17, from the test scene of the branch spike/deck-test (D-597, D-616).
5. PR-1, PR-2, PR-3, PR-84, PR-85, PR-86. PR-82 follows the Deck test run, and it can land at any point after PR-1 (D-599, D-616).
6. Owner and a session: the screen scale probe, in the session right after the merge of PR-85 (D-621, D-625). PR-86 records the answers.
7. Owner: require the checks on `main` (OQ-3).
8. PR-46, PR-4, PR-5, PR-6, PR-43, PR-44, PR-47, PR-34.
9. M-1, M-2. Done on 2026-09-19, and section 4 holds each number (D-672).
10. PR-87: the empty option value of every Tools command (D-677, D-678).
11. Owner and a session: the Sprite Fusion test of the art, on the branch spike/sprite-fusion (D-620, D-675).
12. **← GATE 1 (foundation).** The identity job, `dotnet test`, the smoke job, `det-lint`, and `ste-check` are green on every CI leg.
13. Owner: set the fonts, Terminus TTF and Terminus TTF Bold (D-263, D-264).
14. PR-54, PR-61, PR-7, PR-45, PR-41, PR-8.
15. PR-9, PR-80, PR-66, PR-55, PR-10.
16. PR-48, PR-56, PR-63, PR-57, PR-58, PR-59, PR-60.
17. PR-11, PR-67, PR-62.
18. PR-68, PR-50.
19. PR-12, PR-13, PR-14, PR-65.
20. PR-36.
21. PR-15, PR-49. One night runs, then the `night-gate` job joins the PR gate.
22. Owner: require the bot and `night-gate` checks on `main` after their first runs.
23. PR-16, PR-64, PR-35.
24. PR-38, PR-69, PR-70, PR-71.
25. PR-51, PR-52, PR-53, PR-72.
26. PR-17.
27. M-3, M-4, M-6.
28. Owner: set the M-4 band from the M-4 numbers (D-571).
29. **← GATE 2 (first playable).** The owner plays the village, one hub, and one dungeon on both machines and signs off on feel (D-362).
30. PR-74, PR-75, PR-76.
31. Owner: pay the Steam Direct fee, and put the store page public as Coming Soon (D-471).
32. PR-18, PR-19, PR-20, PR-21.
33. **← GATE 3 (story systems).** The owner plays a branch and a hub that changes with an earlier choice.
34. PR-23, PR-24, PR-81, PR-27, PR-25, PR-26.
35. PR-42, PR-73.
36. PR-28, PR-29, PR-77.
37. PR-30.
38. M-5.
39. **← GATE 4 (region one).** The owner plays region one end to end on both machines. Then trusted players play the build artifacts (D-469).
40. PR-31, PR-33.
41. Owner: join the Apple Developer Program (D-455).
42. PR-78, PR-39.
43. PR-79.
44. Owner and a session: the shot list and the cut of the first trailer (D-476).
45. PR-40.
46. Owner: request the Deck compatibility review from Valve (D-565).
47. **← GATE 5 (first release).** A fresh machine runs the tagged build, and the Deck runs the Steam demo.
48. Valve answers the review, and Phase 6 stays parked.

## 9. Open questions

The open questions register is `docs/questions.md` (D-19). It holds OQ-1 onward with options, recommendations, what each blocks, and the date and decision that resolve each one. File a new question there, not here. Ids never change.
