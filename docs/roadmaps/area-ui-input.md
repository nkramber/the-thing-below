# Area roadmap: UI and input

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the screens and the input of the game work, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-effects.md` holds the light, the effects, and the style of each effect inside the frame. The file `area-art.md` holds the drawings of the window frames, the icons, and the glyphs. The file `area-core.md` holds the intents and the run record, and `area-ci.md` holds the screen-test job. The `game-text-style` skill holds the voice and the length limits of every player string.

External facts, each with the date of its check. The Godot facts come from `godotengine/godot` at the tag `4.7.2-stable` and from `godotengine/godot-docs` on the branch `stable`, the source of `https://docs.godotengine.org/en/stable/`:

- The setting `display/window/stretch/mode` has the default `"disabled"`, the aspect the default `"keep"`, and the scale mode the default `"fractional"`. The editor of 4.7 writes `"canvas_items"` and `"expand"` into a new project. Sources: `doc/classes/ProjectSettings.xml` and `editor/editor_node.cpp`, read 2026-09-16.
- With `"canvas_items"`, "there is no longer a 1:1 correspondence between sprite pixels and screen pixels". With `"viewport"`, "The scene is rendered to this viewport first", and the docs name it "Recommended for games that use a pixel art aesthetic". Source: `doc/classes/ProjectSettings.xml`, read 2026-09-16.
- The pixel-art advice of the docs is: "Set the stretch mode to `viewport`.", "Set the stretch aspect to `keep`", and "Set the stretch scale mode to `integer`." With the integer mode, "The remaining space is filled with black bars on all four sides". Source: `tutorials/rendering/multiple_resolutions.rst`, read 2026-09-16.
- In the integer mode, `Window::_update_viewport_size` runs `content_scale_factor = Math::floor(content_scale_factor)`, with the comment "We always want to make sure that the content scale factor is a whole number". Source: `scene/main/window.cpp`, read 2026-09-16.
- For a `SubViewport`, "the scale factor of the root window will not be applied". A `SubViewportContainer` with `stretch` resizes the sub-viewport to the size of the control, and `stretch_shrink` "Divides the sub-viewport's effective resolution by this value while preserving its scale". Sources: `doc/classes/Viewport.xml` and `doc/classes/SubViewportContainer.xml`, read 2026-09-16.
- The setting `gui/common/snap_controls_to_pixels` has the default `true`, and it "snaps [Control] node vertices to the nearest pixel". The two settings under `rendering/2d/snap` have the default `false`, and each says "It is not recommended to use this setting together with" the other. Source: `doc/classes/ProjectSettings.xml`, read 2026-09-16.
- Godot 4.7.2 has no method that loads a font from bytes. The member `FontFile.data` holds the "Contents of the dynamic font source file". Source: `doc/classes/FontFile.xml`, read 2026-09-16.
- The font defaults are `antialiasing` 1, `hinting` 1, and `subpixel_positioning` 1. The docs warn: "Fonts that have a pixel art appearance should have their subpixel positioning mode set to Disabled." The setting `gui/fonts/dynamic_fonts/use_oversampling` has the default `true`, and it acts with the stretch mode `"canvas_items"`. Sources: `doc/classes/FontFile.xml`, `tutorials/ui/gui_using_fonts.rst`, and `doc/classes/ProjectSettings.xml`, read 2026-09-16.
- A `Theme` takes each item from code, through `set_font`, `set_font_size`, `set_color`, `set_constant`, and `set_stylebox`. The setting `gui/theme/custom` takes a "Path to a custom [Theme] resource file", so it cannot name a theme that code builds. Sources: `doc/classes/Theme.xml` and `doc/classes/ProjectSettings.xml`, read 2026-09-16.
- A `StyleBoxTexture` is "A texture-based nine-patch [StyleBox]", and its `region_rect` "is equivalent to first wrapping the [member texture] in an [AtlasTexture] with the same region". Source: `doc/classes/StyleBoxTexture.xml`, read 2026-09-16.
- A `Label` types out its text through `visible_characters`, which "can be useful when animating the text appearing in a dialog box". The behavior `VC_CHARS_AFTER_SHAPING` "Displays glyphs that are mapped to the first" characters, so the box keeps its layout. Sources: `doc/classes/Label.xml` and `doc/classes/TextServer.xml`, read 2026-09-16.
- A change to the input map at run time does not last. The docs say that the singleton "is not saved (must be modified manually)". A default `ui_*` action "cannot be removed", and "The events assigned to the action can however be modified". Sources: `tutorials/inputs/inputevent.rst` and `doc/classes/ProjectSettings.xml`, read 2026-09-16.
- The methods of `Input` "reflect the global input state and are not affected by [method Control.accept_event] or [method Viewport.set_input_as_handled]". A control with the mouse filter Ignore "will not receive any mouse movement input events nor mouse button input events". Sources: `doc/classes/Input.xml` and `doc/classes/Control.xml`, read 2026-09-16.
- The dead zone of a new action is 0.2, from `InputMap.add_action` and from `DEFAULT_DEADZONE` in the source. The built-in actions take `DEFAULT_TOGGLE_DEADZONE`, 0.5, and the controller page of the docs names 0.5 as "The default value". Sources: `doc/classes/InputMap.xml`, `core/input/input_map.h`, and `tutorials/inputs/controllers_gamepads_joysticks.rst`, read 2026-09-16.
- `Input.get_joy_name` gives a name from the "SDL2 game controller database", and `get_joy_info` can hold the "Steam Input gamepad index". The constant `JOY_BUTTON_A` "Corresponds to the bottom action button: Sony Cross, Xbox A, Nintendo B". Sources: `doc/classes/Input.xml` and `doc/classes/@GlobalScope.xml`, read 2026-09-16.
- Vibration has limits on macOS: "For macOS, vibration is only supported in macOS 11 and later." Source: `doc/classes/Input.xml`, read 2026-09-16.
- For `ConfigFile.get_value`, "If [param default] is not specified or set to [code]null[/code], an error is also raised", so a default value hides an absent key. Source: `doc/classes/ConfigFile.xml`, read 2026-09-16.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

The UI is the frame that the player reads and the input that the player gives. Every screen designs to one 16:9 frame of 1280 by 720 with 32-pixel tiles (G-19, D-568). The world draws at 2x and the UI draws at 1x or 2x, and the fit comes last (D-232, D-633, D-639). Keyboard and gamepad are equals, and the mouse works on menus alone (D-84, D-219). Every player string comes from the string table through one text helper (G-7, D-499).

The order of the area follows the first user of each part. PR-61 builds the base before the first screen: the frame, the fit, the fonts, the text helper, and the UI style (D-524). It also builds the input map and the intents (D-561). PR-55 removed the glyph sets of PR-61 (D-815). PR-62 builds the menu windows before the first system screen, and PR-63 builds the settings before the first PR that needs a setting (D-525, D-526). The title screen, the credits, and the Deck checklist close the work in Phase 5 (PR-33, PR-39).

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the UI and input area.

| # | Finding | Binds |
|---|---|---|
| F-23 | `--headless` draws nothing | PR-41 and each UI PR: a screen test under Xvfb (D-172) |
| F-24 | A 32-pixel tile holds four times the pixels of a 16-pixel tile | PR-61: the world at 2x, the UI font, and the Deck floor (D-92, D-633) |
| F-34 | Steam needs screenshots at 1920 by 1080 in 16:9 | PR-61: one 16:9 frame, which scales to a 16:9 screenshot (D-568) |
| F-45 | Three Godot defaults meet the pixel art | PR-61: the stretch mode of a new project is `canvas_items` |
| F-48 | Godot has no fit like the fit of D-232 | PR-61: a world `SubViewport` of 640 by 360, and both steps of the fit in Game (D-634) |
| F-49 | Three font defaults meet the pixel font | PR-61: the load from bytes and the font settings |
| F-50 | Five input facts of Godot meet the plan | PR-61, PR-62, and PR-63: the intents, the prompts, and the remap |

## 7. Roadmap

Each part below says how one part of the UI works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The frame

Built by PR-61. Phase file: `phase-2-first-playable.md`.

- The frame is 1280 by 720, one 16:9 shape (D-568). The world draws at 2x, so the frame holds 20 columns and 11.25 rows of 32-pixel tiles (D-633).
- Every screen shape other than 16:9 shows black bars around the frame, the Steam Deck included (D-568).
- The Deck shows the frame at 1x, with a bar of 40 pixels above and below (D-92, D-568).
- Game draws the world into a `SubViewport` of 640 by 360, and it scales that viewport by 2 into the frame. One art pixel of the world is one viewport pixel (D-230, D-634).
- A `SubViewport` keeps its own size, whatever the window does, so the light of `area-effects.md` falls on art pixels (F-48, the external facts above).
- The view shows 11.25 tile rows, so a quarter row sits at the edge. A quarter row is 16 frame pixels, a whole pixel count, so the art stays sharp (D-568, D-633).
- Every screen shows the same part of the map, so no screen shape gains a view of a patrol (D-566, D-568).

> *In plain English:* the game draws one picture of a fixed widescreen size. Every screen shows exactly that picture, and any other shape, the Steam Deck included, gets black bars.

### 7.2 The fit to a screen

Built by PR-61. Phase file: `phase-2-first-playable.md`.

- By default the frame scales to the full height of the screen, and a display setting switches to exact whole-number scale with bars (D-232).
- The Deck shows the frame at 1x in both modes (D-232, D-568).
- A desktop at 1920 by 1080 needs a scale of 1.5. There every pixel keeps the same size, with a slight softness at pixel edges (D-568, D-573).
- D-573 sets the method. Game scales up past the screen by a whole number with the Nearest filter, then scales down with a linear filter.
- A 16:9 screen at 2560 by 1440 or 3840 by 2160 takes an exact whole-number scale, 2x or 3x (D-568).
- Godot has no mode that does both steps, so Game builds them (F-48).
- The stretch settings of the project keep the world at 1x, against the `canvas_items` default of a new project (F-45).
- The screen tests capture both fit modes at 1080 and 1440 screen rows (D-232, `area-ci.md` section 7.12).
- The probe of D-621 set the scale on 2026-09-18. The world draws at 2x on every screen (D-633).
- The fit sets the default body size. A frame fit of 1x takes a body of 32, and every fit above 1x takes a body of 24 (D-707).
- Controls snap to whole pixels by default, and the two snap settings of the renderer stay off (the external facts above). The docs advise against both at once. Game rounds each map position itself (D-715).

> *In plain English:* the picture grows to fill the screen. On a common 1080p monitor the scale is not a whole number. So the game scales up past the screen and then shrinks the picture, which keeps the pixels crisp.

### 7.3 The fonts

Built by PR-61. Phase file: `phase-2-first-playable.md`.

- The body font is Terminus TTF, and the title font is Terminus TTF Bold (D-263, D-264).
- The body draws at 24 or 32 frame pixels, and the title at twice the body: 48 or 64 (D-707).
- Each body size draws its own bitmap strike, and each title size doubles a smaller strike (D-710, F-49).
- Game reads each font from the bytes of its own assembly into `FontFile.data`, because Godot has no byte-array load method (D-508, F-49).
- Each font sets the antialiasing, the hinting, and the subpixel positioning for pixel art, against the Godot defaults (D-710, F-49).
- Font oversampling stays off, so a scaled frame never re-draws a glyph at another size (F-49).
- Each body size draws its own bitmap strike, so a glyph pixel is always one frame pixel (D-230, D-707).
- The credits and the export carry the OFL notice of each font (D-263, D-467).
- English is the only language of the prologue, and each layout leaves room for a longer word (D-167).
- No text falls below 9 pixels on the Deck, which the rating Verified needs (D-459).

> *In plain English:* the game carries its two fonts inside its own program file. It draws them with hard pixel edges, at the size of the drawing.

### 7.4 The text helper and the string table

Built by PR-61. Phase file: `phase-2-first-playable.md`.

- Every player string has an id in the string table, and no string sits in code or in a scene file (G-7, D-167).
- One text helper puts a string table entry on screen, and det-lint fails a Godot text property outside it (D-499, `area-tools.md` section 7.4).
- The helper takes the id, the values to fill in, and the place. It never takes a literal (G-7).
- The length limits of each kind of text live in the `game-text-style` skill (D-241).
- A panel sizes to hold its longest string in the string table, and a test proves it (D-241).
- The owner approves each text batch in its PR (D-57, G-20).

> *In plain English:* every word the player reads comes from one list of text. One piece of code puts that text on screen, so no line of the game hides a word of its own.

### 7.5 The UI style file and the Theme

Built by PR-61. Phase file: `phase-2-first-playable.md`.

- One JSON content file holds the font sizes, the colors as palette keys, and the ids of the frame drawings (D-527).
- Core holds its record and its strict reader, and every number in it is an integer (D-517).
- Game builds the Godot `Theme` from that file at load, and no `.tres` theme exists (D-527, G-6).
- The project setting for a theme names a file path, so it stays empty (the external facts above).
- A window frame draws as nine parts from its drawing in the atlas, which holds the corners at their size (D-220, `area-art.md` section 7.4).
- The UI takes no scene light and no glow (D-210). No pass covers it, because the CRT left the plan (D-618).
- A color of the UI is a palette key, so the screen keeps one palette (D-89, D-181).

> *In plain English:* one small file says how menus look: which colors, which text sizes, and which drawn border. The game reads that file and builds its own look from it.

### 7.6 Menus and windows

Built by PR-62. Phase file: `phase-2-first-playable.md`.

- A main list opens one window for each task: party, lessons, gear, items, status, log, save, and settings (D-211, D-992). An entry whose window a later PR builds shows dim (D-988).
- Each window stacks over the last, back closes it, and the map stays visible behind (D-211).
- A menu pauses the world (D-162). The tick rises while a menu is open, and the world systems skip their work (D-650).
- A menu action is an intent, and the record holds no cursor move (D-493).
- The mouse works on menus alone, and a mouse action on a menu makes the same intent as a key (D-219, D-493). The mouse moves the cursor, and a click chooses the item under the pointer (D-872).
- The dungeon map screen draws each tile that the party walked, with the doors and the save points on those tiles (D-567). PR-16 adds the exit mark (D-993). Each walked tile draws at 16 pixels, so a dungeon of 80 by 45 tiles or less shows whole (D-982).
- A map action opens the dungeon map screen from the walk, with the M key and the Back button of the gamepad (D-986, D-990). Back or the map action closes it.
- The party window sets the starting row of each character, and the snapshot keeps the row (D-377, D-558).
- The status window shows the full sheet of each character (D-569, D-991). The sheet holds the level, the row, HP, MP, the experience, the stats, and the statuses that last.
- The status window shows the stats with the worn gear (D-1036).
- The gear window shows the stats with the gear, and the six gear slots with a dash for each empty slot (D-44).
- The gear window lists the pieces of the pack that fit a slot (D-1048).
- The gear list shows the name of each piece. A second line of stats shows each stat with the piece under the cursor, in grey, green, or red (D-1060).
- The status window and the gear window name the magic MAG and the resistance RES (D-1056).
- The item window shows each item with its owned count and its stack limit, and a use that changes nothing shows dim (D-1039, D-1049).
- PR-62 proves the stack with a fixture menu, and each later system PR adds one screen (D-525).

> *In plain English:* menus are windows that stack on each other, and the world stops while one is open. The keyboard, the gamepad, and the mouse all move the same cursor.

### 7.7 The map HUD and notices

Built by PR-7 and PR-62. Phase file: `phase-2-first-playable.md`.

- The HUD stays minimal. A health mark shows at the edge for a hurt or a down character, and nothing else (D-212).
- A status that lasts on the map gets a mark too (D-390).
- No sun or moon mark shows the time of day, because the story sets it and it never changes under the player (D-442, D-445).
- A notice slides in at the top edge, types out, holds, and fades, and the game continues (D-221, D-994). Confirm never skips it (D-996).
- A notice counts the ticks of the world, so it hides and waits under a menu (D-995).
- An important notice also lands in the log window of the main list (D-221, D-987). Content marks each notice that logs, the log keeps the 30 newest, and the snapshot holds it (D-983 to D-985).

> *In plain English:* the screen stays clear while you walk. A short line slides in when something matters, and the menu keeps the ones that matter.

### 7.8 The dialogue box

Built by PR-36, on the base of PR-61. Phase file: `phase-2-first-playable.md`.

- The box sits at the bottom, with the portrait, a name plate, and the choices (D-109, D-114, D-223).
- One line of the box holds 156 characters at the UI scale of 1x, and 76 at 2x, so the limit is 76 (D-635, F-69).
- The text types out at the chosen speed, in silence, and the box holds its layout as it types (D-223, D-709). The three speeds and the skip follow D-864.
- The text speed and the skip are accessibility settings of PR-63 (D-214).
- A choice in the box becomes an intent, and Core holds its result (D-493, PR-36).
- `area-story.md` holds the story scene format and the runner that drive the box. Core runs each step, and Game draws it (D-540).

> *In plain English:* people speak in a box at the bottom of the screen, with a face beside the words. The words appear at the speed the player picks, with no beeps.

### 7.9 Intents and the input map

Built by PR-61, on the intents of PR-6. Phase files: `phase-1-foundations.md` and `phase-2-first-playable.md`.

- Keyboard and gamepad play every screen, and the mouse works on menus alone (D-84, D-219).
- Game makes an intent from each key, button, and mouse action, and Core reads intents alone (D-493, `area-core.md` section 7.8).
- Game makes each intent from an input event, never from a poll of `Input`, because a poll ignores what a menu took (F-50, the external facts above).
- The input map, the remap, and the device kind stay in Game, and no record holds them (D-214, D-493).
- The built-in `ui_*` actions drive the menu focus. A remap changes their events, and it never removes one (F-50).
- A change to the input map does not last by itself, so PR-63 writes each remap to the settings file (F-50).
- The dead zone of a stick is a setting, and its default follows the value that PR-63 sets, not the docs (F-50, D-226).
- Each binding matches every device, because Godot gives a new event the device 0, and a pad of another device then matched nothing (D-1077, F-107).
- `ui_accept` takes the A button and `ui_cancel` takes the B button, because Godot 4.7.2 gives them keys alone (D-1077, F-107).
- Each action acts on the first press of a hold alone. A stick past the dead zone gives a press on each motion event, and a pad can show as two devices (D-1077, F-107).
- A key press, a pad button, or a stick push hides the mouse pointer, and a move of the mouse shows it again (D-1078).
- Game logs each pad that connects, with its device and its name. Thus a report of a pad fault names the devices (D-1077).

> *In plain English:* the rules of the game never see a key or a stick. They see choices, so a replay, a robot, and a player all speak the same language.

### 7.10 Button prompts and glyphs

PR-61 built the prompts, and PR-55 removed them. Phase file: `phase-2-first-playable.md`.

- The game shows no button prompt on any screen (D-815). The row of prompts, the device tracker, the device table, and the glyph drawings left the build in PR-55.
- D-815 supersedes the prompts of D-222 and the glyph set of D-711, and PR-78 adds no controller type call (D-460, D-553).
- A button constant of Godot names the place of a button, not its label, so one constant means Cross, A, or B (F-50).
- The glyph check of Verified holds when no text and no picture names a key or a button, and PR-39 proves that (D-459, D-815).

> *In plain English:* the screen shows no row of buttons, as most games do. No text on the screen names a key or a button, so no controller shows a wrong symbol.

### 7.11 The settings screen

Built by PR-63. Phase file: `phase-2-first-playable.md`.

- PR-63 lands right before PR-57, the first PR that needs a setting (D-526). Until PR-62, the menu action opens the screen, and the world pauses (D-871).
- The screen holds five groups: display, audio, controls, battle, and accessibility (D-214, D-226). D-226 names the four groups beyond accessibility.
- Display holds the window mode, the scale of D-232, and the body size of D-707 (D-226, D-232, D-618).
- The body size takes two values, 24 and 32 frame pixels (D-707, G-28). A frame fit of 1x takes 32 by default, and a fit above 1x takes 24.
- Audio holds the master, music, effects, and ambience volumes, the mute in the background, and the mono toggle (D-435).
- Controls hold the remap, the stick dead zone, and the vibration setting (D-214, D-226, D-434). The dead zone is 0.5 for every action by default, with a slider from 0.2 to 0.8 (D-861).
- Battle holds the message speed and the remembered cursor (D-226).
- The settings file lives outside the save files and never enters a run record (T-7, D-494). It is `settings.json` at the root of the folder of D-465, and Storage writes it (D-860).
- The settings file carries a format version, and each new setting ships with a migration step and a fixture file (D-570). PR-63 writes format 1, with no step yet (D-869).
- A key that no version declares fails the load with the file and the key, and no setting takes a silent default (T-2, D-570).
- Vibration has limits on macOS, and the setting turns it off for any player (F-50).
- A remap conflict blocks the save and the exit of the screen, and the screen shows each conflict (D-862).
- The values and the defaults of each setting follow D-864 to D-868, with the dead zone of D-861 and the body size of D-707.

> *In plain English:* one screen holds every choice about the game: the picture, the sound, the buttons, and the pace of battle.

### 7.12 The accessibility settings

Built by PR-63. Phase file: `phase-2-first-playable.md`.

- Three accessibility settings come with the screen (D-214, D-870). They are the flash and shake reduction, the text speed and skip, and the button remap. D-870 removed the shape icons. The remap covers the actions of the game, and the `ui_*` actions keep their default buttons (D-862).
- The reduction covers the shake and the flash of D-186, and the color split of D-195 (D-214, D-618). It has three levels: full, reduced, and off, and no level flashes more than three times in one second (D-863).
- The screen tests capture each effect at each of the three levels of the reduction (D-172, D-863, `area-effects.md` section 7.13).

> *In plain English:* a player who needs calm can turn off the flashes and the shakes. They can also slow the text and remap the buttons.

### 7.13 UI in the tests

Built by PR-41 and every UI PR. Phase file: `phase-2-first-playable.md`.

- `--headless` draws nothing, so the smoke session never tests a screen (F-23).
- The screen-test job captures each screen at both body sizes, and both fit modes at 1080 and 1440 screen rows (D-172, D-232, D-707).
- A test proves that each panel holds its longest string from the string table (D-241).
- A test reads the stretch settings and the font settings from the project and the code (F-45, F-49).
- det-lint fails a Godot text property outside the text helper, and a text value in a scene file (D-499).
- The owner reads the screens on the Deck at Gate 2 (D-92, D-623).

> *In plain English:* the computers that check each change take fixed pictures of every screen, in both shapes, and compare them pixel by pixel.

### 7.14 Screens by PR

| PR | Screens and parts | Decisions |
|---|---|---|
| PR-61 | The 16:9 frame, the fit, the fonts, the text helper, the UI style file, the window frames, the input map, the intents, and the glyph sets | D-524, D-527, D-561, D-568 |
| PR-7 | The map scene, the camera, and the map HUD | D-106, D-212, D-306 |
| PR-10 | The battle screen: the timeline strip, the command menu with its keyboard and gamepad input, the pointer of a target, the status, the health bars, and the damage numbers | D-111, D-213, D-826, D-827, D-833 |
| PR-62 | The window stack, the party window with the starting row, the status window, the notices, the notice log, and the dungeon map screen | D-211, D-218, D-221, D-525, D-558, D-567, D-569 |
| PR-63 | The settings screen, the settings file, and the three accessibility settings | D-214, D-226, D-526, D-870 |
| PR-12 | The lesson window with the aptitudes, the slots, the swap, and the cast from the menu, and the Lessons command of a fight in two rows of commands | D-211, D-1030, D-1031, D-1033, D-1034 |
| PR-13 | The gear window, the item window, and the stats with the gear in the status window | D-44, D-1036, D-1039, D-1048, D-1049 |
| PR-99 | MAG and RES on each screen, the line of trial stats in the gear window, and the lesson swap anywhere outside a fight | D-1050, D-1056, D-1060 |
| PR-103 | The pause of a fight, with a dim and "Paused" above the hand-off, the crash message above the hand-off, and the line of the item under the cursor in a fight | D-1083, D-1084, D-1093 |
| PR-14 and PR-16 | The service and save screens, one for each system | D-211 |
| PR-36 | The dialogue box, the name plate, and the choices | D-114, D-223 |
| PR-35 | The region map screen | D-113 |
| PR-33 | The title screen, the version line, the settings entry, and the credits screen | D-454, D-467 |
| PR-39 | The Deck checklist: no text names a key or a button, the default bindings, and the 9-pixel text floor | D-459, D-815 |
| PR-78 | The Steamworks binding and the start, with no controller type call | D-460, D-462, D-553, D-815 |

### 7.15 UI that other area files hold

| Part | Area file | PR |
|---|---|---|
| The light and the effects inside the frame, with the style of each one | `area-effects.md` | PR-56 to PR-60 |
| The drawings of the window frames and the icons | `area-art.md` | PR-17 and the art PRs |
| The intents, the run record, and the tick of a menu | `area-core.md` | PR-6 |
| The screen-test job and its baselines | `area-ci.md` | PR-41 |
| The menu sounds | `area-audio.md` | PR-70 |
| The studio mark, the boot splash, and the credits text | `area-release.md` | PR-33 and PR-77 |
| The story scene format and the story scene runner behind the box | `area-story.md` | PR-68 |

### 7.16 The contract of every later UI PR

Each later PR that adds or changes a screen keeps this list. The phase files make exit tests from it.

1. Draw every string through the text helper, by its id (G-7, D-499).
2. Fit the frame of 1280 by 720, and prove the fit in a test (D-568).
3. Make each player action an intent, from an event and never from a poll (D-493, F-50).
4. Take the style from the UI style file, never from a constant in code (D-527).
5. Keep the UI out of scene light and glow (D-210).
6. Add the screen-test capture of each new screen (D-172).
7. Keep every text at 9 pixels or taller on the Deck (D-459).
8. Hold the layout at both body sizes, and prove each one in a test (D-707).

> *In plain English:* every new screen follows the same eight steps. It reads its words from one list, fits both screen shapes, and proves itself in a fixed picture.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The UI work keeps this order inside it:

1. PR-61: the UI base, right before PR-7 (D-524).
2. PR-7: the map scene and the map HUD. PR-61 already built the input map (D-561).
3. PR-41: the screen tests of the frame and of both fit modes.
4. PR-10: the battle screen.
5. PR-48 and PR-56: the normal maps and the light (`area-effects.md`).
6. PR-63: the settings and the accessibility settings, right before PR-57 (D-526).
7. PR-57 to PR-60: the effects that the settings turn down.
8. PR-62: the menu windows, the party window, and the status window, right before PR-68 (D-525, D-558, D-569).
9. PR-68: the story scene runner, before PR-12 (D-541, D-556).
10. PR-12, PR-13, PR-14, and PR-16: one screen for each system.
11. PR-36: the dialogue box. PR-35: the region map screen.
12. PR-17: the first playable, read on the Deck (M-6).
13. **← GATE 2 (first playable).**
15. PR-33: the title screen, the version line, and the credits.
16. PR-78 and PR-39: the Steam binding, then the Deck checklist (D-565).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block UI PRs, and each PR asks its questions when it starts (D-487):

- OQ-104: the font settings and the load from bytes. Blocks PR-61.
- OQ-106: where the settings file lives, and its form. Resolved 2026-09-22 by D-860.
- OQ-107: how Game knows the last device of the player. Resolved 2026-09-20 by D-711, which D-815 superseded on 2026-09-21.
- OQ-108: where a remap lives, and what a conflict does. Resolved 2026-09-22 by D-862.
- OQ-109: the dead zone of a stick, and its range in the settings. Resolved 2026-09-22 by D-861.
- OQ-110: the cursor rules of a menu, and the mouse on it. Resolved 2026-09-22 by D-872.
- OQ-111: the scale of the dungeon map screen. Resolved 2026-09-23 by D-982.
- OQ-112: the text speeds, and the type-out of the dialogue box. Resolved 2026-09-22 by D-709 and D-864.
- OQ-113: the notice log, and how many notices it keeps. Resolved 2026-09-23 by D-983 to D-985.
- OQ-89: pixel snap in Game. Resolved 2026-09-20 by D-715.
- OQ-64: the tick while a menu is open. Resolved 2026-09-18 by D-650.
- OQ-90: where the studio mark shows. Blocks PR-33.

No open question blocks this file.
