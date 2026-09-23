# Area roadmap: Effects

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the effects of the game work, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-15 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-art.md` holds the drawing files, the palette, and the atlas, and `area-tools.md` holds the command that builds normal maps. The file `area-ci.md` holds the screen-test job, and `area-ui-input.md` holds the frame, the fit to a screen, and the settings. The files `area-exploration.md` and `area-battle.md` hold the map scene and the battle scene that the effects draw on.

External facts, each with the date of its check. The Godot facts come from `godotengine/godot` at the tag `4.7.2-stable` and from `godotengine/godot-docs` on the branch `stable`, the source of `https://docs.godotengine.org/en/stable/`:

- "The engine computes 2D lighting and shadows at the Viewport's pixel resolution, not at the source texture's texel resolution." For pixel art, "Nearest texture filtering will not achieve this effect". Source: `tutorials/2d/2d_lights_and_shadows.rst`, read 2026-09-15.
- "[CanvasModulate] applies a color tint to all nodes on a canvas. Only one can be used to tint a canvas". A light affects the objects from `range_layer_min` to `range_layer_max`, and the code compares those values with the canvas layer of each canvas. Sources: `doc/classes/CanvasModulate.xml`, `doc/classes/Light2D.xml`, and `servers/rendering/renderer_viewport.cpp`, read 2026-09-15.
- A light joins the lights of a frame only `if (cl->enabled && cl->texture.is_valid())`. The editor warns: "A texture with the shape of the light must be supplied". An `AtlasTexture` or a `CanvasTexture` "cannot be used as a PointLight2D texture". Sources: `servers/rendering/renderer_viewport.cpp` and `scene/2d/light_2d.cpp`, read 2026-09-15.
- The canvas renderer holds `MAX_LIGHTS_PER_ITEM = 16` and `MAX_LIGHTS_PER_RENDER = 256`. The loop for one item stops at `light_count == MAX_LIGHTS_PER_ITEM - 1`, with no message. The Compatibility renderer lowers the limit for one render to 64 when `max_uniform_buffer_size < 65536`. Sources: `servers/rendering/renderer_rd/renderer_canvas_render_rd.h` and `.cpp`, and `drivers/gles3/rasterizer_canvas_gles3.h` and `.cpp`, read 2026-09-15.
- In a `TileMapLayer`, "A quadrant is a group of tiles to be drawn together on a single canvas item", and "the default quadrant size groups together 16 * 16 = 256 tiles". A `TileSet` has occlusion layers, which "allow assigning occlusion polygons to atlas tiles". Sources: `doc/classes/TileMapLayer.xml` and `doc/classes/TileSet.xml`, read 2026-09-15.
- The `height` of a `PointLight2D` has the default value 0.0 and is "Used with 2D normal mapping". The canvas shaders put the pixel at height 0 with `vec3(vertex, 0.0)`, and they scale the light by `max(0.0, dot(normal, light_vec))`. The tutorial says that normal-mapped lights can look weaker: "To resolve this, increase the Height property". Sources: `doc/classes/PointLight2D.xml`, `servers/rendering/renderer_rd/shaders/canvas.glsl`, and the tutorial above, read 2026-09-15.
- The canvas shader of Godot 4.7.2 reads a normal map with `texture(...).xy * vec2(2.0, -2.0) - vec2(1.0, -1.0)`, and it computes the part toward the viewer from the other two. Thus red is the part to the right, and a high green value faces up the screen. Source: `servers/rendering/renderer_rd/shaders/canvas.glsl` at the tag `4.7.2-stable`, read 2026-09-21. The normal-map command of PR-48 follows this form (D-184).
- Both canvas shaders correct the normal of a flipped draw with `normal.xy *= sign(src_rect.zw)` or `normal.xy *= sign(read_draw_data_src_rect.zw)`, before `#CODE : FRAGMENT`. After the fragment code, a shader that uses `NORMAL_MAP` replaces that normal. Sources: `servers/rendering/renderer_rd/shaders/canvas.glsl` and `drivers/gles3/shaders/canvas.glsl`, read 2026-09-15.
- For a `CanvasTexture`, "The default value of 1.0 disables specular reflections entirely." Source: `doc/classes/CanvasTexture.xml`, read 2026-09-15.
- Both canvas shaders add a light with `color.rgb += light_color.rgb * light_color.a` and end with `frag_color = color`, with no upper clamp. With HDR 2D, "the end result of the Viewport will not be clamped to the 0-1 range". Sources: the two canvas shaders and `doc/classes/ProjectSettings.xml`, read 2026-09-15.
- "Since Godot 4.2, you can enable HDR for 2D rendering when using the Forward+ and Mobile rendering methods." The glow threshold "needs to be decreased below 1.0 when using glow in 2D, as 2D rendering is performed in SDR". With Compatibility, "glow uses a different implementation". Sources: `tutorials/3d/environment_and_post_processing.rst` and `doc/classes/Environment.xml`, read 2026-09-15.
- With `use_fixed_seed`, `GPUParticles2D` uses one seed "for every simulation", which "is useful for situations where the visual outcome should be consistent across replays". With no texture, "particles will be squares with a size of 1×1 pixels". Its `emit_particle` "is only supported on the Forward+ and Mobile rendering methods, not Compatibility". Source: `doc/classes/GPUParticles2D.xml`, read 2026-09-15.
- `GPUParticles2D.request_particles_process` "Requests the particles to process for extra process time during a single frame". With `SpeedScale` at 0.0, the call "is useful to be able to seek a particle system timeline". `restart` with `keep_seed` true keeps the random seed, "Useful for seeking and playback". Source: the `GpuParticles2D` members of `GodotSharp.xml` in the `GodotSharp` package 4.7.2, read 2026-09-22.
- `CPUParticles2D` seeds each particle with `rng->set_seed(p.seed)`, but its points shapes take `Math::rand()` and its ring shape takes `Math::randf()`, the random numbers of the engine. The docs "recommend using GPUParticles2D unless you have an explicit reason not to". Sources: `scene/2d/cpu_particles_2d.cpp` and `tutorials/2d/particle_systems_2d.rst`, read 2026-09-15.
- A shader that fails to compile ends with `ERR_FAIL_MSG("Shader compilation failed.")`, a message in the log. A `Shader` is a program "saved with the .gdshader extension", and it has a `code` member. Sources: `servers/rendering/renderer_rd/renderer_canvas_render_rd.cpp` and `doc/classes/Shader.xml`, read 2026-09-15.
- The `TIME_PROCESS` monitor gives the "Time it took to complete one frame, in seconds". `RenderingServer.viewport_set_measure_render_time` turns on the render times of a viewport. Sources: `doc/classes/Performance.xml` and `doc/classes/RenderingServer.xml`, read 2026-09-15.
- For the Steam Deck rating, "the game must ship with a default configuration that results in a playable framerate. On Steam Deck, this is 30fps at 800p". Source: `https://partner.steamgames.com/doc/steamdeck/compat`, read 2026-09-15.
- The screen of the LCD model of the Steam Deck has the refresh rate "60Hz", and the screen of the OLED model "up to 90Hz". Sources: `https://www.steamdeck.com/en/tech/deck` and `https://www.steamdeck.com/en/tech`, read 2026-09-15.
- WCAG 2.2 success criterion 2.3.1 reads: "Web pages do not contain anything that flashes more than three times in any one second period, or the flash is below the general flash and red flash thresholds." Source: `https://www.w3.org/TR/WCAG22/`, read 2026-09-15.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Effects give the game its weight: light and shadow on snow, blood on a hit, and fog in a dead gallery (D-139, D-186, D-187). Every effect draws with the palette of 64 colors and hard edges, so it reads as part of the art (G-27, D-622). Every effect is presentation. Game draws it from Core state and effect files, and no rule reads an effect, so a new effect never breaks a replay (D-495, D-522). Effect files are JSON content with integer values, like every other content file (D-182, D-517). The Steam Deck sets the limit: 60 frames per second with every effect on, and an effect budget holds each place inside it (D-161, D-523).

The order of the area follows the first user of each part. The Deck test ran on 2026-09-17, and it picked the renderer and measured the first effect budget (D-616, D-617). Every other effect PR lands before the first playable, right after the first scene that it needs (D-520). Light meets both the map scene and the battle scene, so the normal maps and the light follow PR-10. The battle effects, the ambient effects, glow, and the transitions then build on the light. Each effect PR comes after the screen-test job of PR-41 (D-172).

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the effects area.

| # | Finding | Binds |
|---|---|---|
| F-66 | The Deck sweep found no limit, so each budget row is a floor | PR-56 to PR-60: a row rises only with a new measurement (D-617, G-14) |
| F-67 | A 32-pixel sprite at 1x covers 4.0 mm on the Deck, about 60 percent of the apparent size of a Game Boy Advance sprite | The probe of D-621 and every effect PR: the scale of the frame comes from OQ-183 |
| F-19 | Compressed PNG bytes depend on the encoder | PR-48 and PR-41: tests compare decoded pixels |
| F-23 | `--headless` draws nothing | PR-41 and each effect PR: every effect meets its screen test under Xvfb (D-172) |
| F-24 | A 32-pixel frame holds four times the pixels, and the Deck lights and fills four times the pixels of a 640 by 400 frame | The Deck test, PR-48, and PR-56: the effect budget at the frame of 1280 by 720 (D-523, D-568) |
| F-26 | The Deck test had no failure branch | The Deck test of 2026-09-17 held the target, so D-261 never fired (D-616) |
| F-38 | Double math can differ by platform | PR-48: integer math for normal maps (D-502) |
| F-45 | Three Godot defaults meet the pixel art | PR-56: the normal-map atlas takes the Nearest filter too |
| F-46 | Godot 2D light fails in silence in three ways | PR-56 and D-523: a check on each light texture, a limit of 15 lights on one canvas item, and a height on each light |
| F-47 | A bright light on a pale sprite can pass the glow threshold | PR-59: OQ-102 keeps glow on light alone (D-188) |

## 7. Roadmap

Each part below says how one part of the effects works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 Effects and the simulation

Built by PR-6 and every effect PR. Phase files: `phase-1-foundations.md` and `phase-2-first-playable.md`.

- Game draws each effect from Core state and events, and no rule in Core uses an effect file (D-168, D-495).
- Core holds the record and the strict reader of each effect file, so every number in it is an integer (D-517, `area-core.md` section 7.7).
- A strength is in basis points, a time in ticks, and a size in pixels (D-169, D-266). Game turns each integer into the value that Godot takes, at load.
- Effect timings count ticks, 60 to a second, so an effect lasts the same time on a faster screen (D-164, D-266).
- No rule waits for an effect (D-522). Where the world waits for one, Game counts the ticks of the effect on its fixed-step clock and sends a wait intent at the end.
- The run record holds each wait intent, a replay reads it, and a bot sends it at once (D-493, D-522).
- Each PR that adds a wait adds its intent type and a replay test (D-522). A record with that intent replays to the same state hash (G-5).
- Effects take their random numbers from sources outside Core, so a spark never moves a rule stream (G-4, `area-core.md` section 7.4). The screen tests and the trailer capture fix those seeds (D-172, D-476).
- An effect file names the content ids that it serves, such as a map, an ability, or a kind of encounter. A rule file never names an effect, as with art (D-495, D-519).
- Light on screen never feeds Core. A puzzle of light and dark keeps its state in Core, and Game draws its light from that state (D-41, G-1, G-23).
- The sight of a patrol follows the time of day of its map in the rules, never the light on screen (D-442).

> *In plain English:* effects are the show, not the rules. A spark, a shadow, or a slow transition never changes a fight, so an old replay still plays after a change to an effect.

### 7.2 The order of a frame

Built by PR-7, PR-10, PR-56, PR-59, and PR-60, inside the frame of `area-ui-input.md`. Phase file: `phase-2-first-playable.md`.

The table lists what a frame draws, from the bottom to the top.

| Step | What it draws | Scene light | Decisions |
|---|---|---|---|
| Backdrop | The large pictures of a battle place, with their drift | Yes | D-205, D-516 |
| Map | The tiles and the edge tiles of the map | Yes | D-110, D-501 |
| Figures | The party, the enemies, the NPCs, and objects such as chests | Yes | D-199, D-207 |
| Flames | The flame, the embers, and the smoke of each torch | No | D-890 |
| Particles | Blood, sparks, snow, embers, and dust | As its effect file sets | D-186, D-187 |
| Fog | The fog of the weather of the place: 1 to 3 layers in one pass | As its effect file sets | D-885, D-897, D-898, D-900 |
| Mark | The mark of a sight over an enemy | No | D-208 |
| Light | The ambient light of the time of day, the point lights, and the shadows | — | D-183, D-442 |
| Glow | A soft glow on light sources alone | No | D-188 |
| UI | Menus, the HUD, text, portraits, and damage numbers | No | D-210, D-213 |
| Transition | The full-screen effect that starts a battle | No | D-191, D-195 |
| Fit | The scale to the screen, with black bars | No | D-232, D-568 |

- Game draws the world, the UI, and the transition into the frame at 1x, 1280 by 720 (D-230, D-568). The game draws no CRT pass (D-618). The fit to the screen comes last (D-232). The world draws at 2x, from a `SubViewport` of 640 by 360 (D-633, D-634).
- Godot computes 2D light at the pixel size of the viewport, and the Nearest filter does not change that (the external facts above). So the frame at 1x gives light and shadows the pixel size of the art.
- PR-61 draws the world in a `SubViewport` at 1x, and `area-ui-input.md` holds the stretch mode and the fit (F-45, F-48). Otherwise light falls on screen pixels, not on art pixels.
- The UI sits on a canvas layer above the world, and a light reaches only the canvas layers in its range. So the UI never takes scene light (D-210).
- A transition is full-screen, so it covers the UI too (D-195, D-210).
- Fog draws above the figures, and a contrast test keeps each enemy visible (D-187, D-885, D-886, D-892).
- The Z index of each step above the figures follows the table: the flame of a torch 2, and the weather 3.
- The fog takes 4, a hit burst takes 5, and the mark of a sight takes 8. Fog never hides the mark (D-208).
- `area-ui-input.md` builds the frame and the fit. This file holds what draws inside the frame.

> *In plain English:* each frame stacks the same way: the world, then its light, then the menus, then a transition over all of it. Menus never catch the torchlight, and the whole stack scales to the screen at the end.

### 7.3 The Deck test

Built by the owner and a session, before PR-1. Phase file: `phase-1-foundations.md`.

- A throwaway scene runs on the Deck of the owner under Forward+ and under Mobile. The renderer that holds 60 frames per second with more room wins (D-160, D-161).
- The test scene runs at the frame of 1280 by 720 with the load of D-160 (D-228, D-568). That load holds particles, point lights with normal maps and shadows, glow, and the four ambient kinds.
- The load also holds a transition and a backdrop (D-160). It held the CRT too, which D-618 later removed.
- The test scene runs as the native Linux export (D-458).
- The test also finds the effect budget, the most load that still holds 60 frames per second (D-523). Section 7.4 holds the budget.
- The test picked the Mobile renderer on 2026-09-17, and PR-82 sets it in the Game project (D-616).
- If neither renderer holds 60 frames per second, the owner decides then (D-261).
- The test scene is throwaway, so it never merges to `main` (D-160). The branch `spike/deck-test` holds its source, and that branch never merges (D-597).
- The test scene measures itself, and `deck-test/scripts/FrameMeter.cs` reads the time of each frame (D-598). Each run writes a report file, which a session can read.
- The LCD Deck has a 60 Hz screen, and the OLED Deck runs up to 90 Hz (the external facts above). The test records the model of the Deck and its refresh rate.
- Valve asks for 30 frames per second at 800p for the Deck rating, so D-161 sets a stricter target (the external facts above).
- The screen tests of CI use the Mobile renderer of the shipped build, on a software Vulkan driver (D-616, D-731). The contact sheet on the Mac of the owner shows the same renderer on a real card.

> *In plain English:* before any code, a small test scene with every kind of effect runs on the owner's Deck. It picks the faster of two graphics modes and finds how many lights and particles the Deck can hold at full speed.

### 7.4 The effect budget

Built by the Deck test and PR-56, with rows from PR-57, PR-58, PR-94, PR-59, and PR-60. Phase file: `phase-2-first-playable.md`.

- The effect budget holds the load that the Deck test measured: lights with shadows on screen, live particles, and full-screen passes (D-523).
- The budget is a content file with integer limits (D-517). A change to it cites a Deck measurement before and after (G-14).
- A test fails a map or a battle place whose effect files pass the budget. The failure names the file, the view, and the count (T-2, D-523).
- The test moves a window of 640 by 360 art pixels over the map. It takes the highest count of lights whose range reaches the window (D-842).
- Godot drops each light past 15 on one canvas item with no message (F-46). A map layer draws a group of 256 tiles as one canvas item.
- So the budget test also fails more than 15 lights on one canvas item, whatever the Deck test measures (T-2).
- PR-56 adds the budget file and its test with the rows for light. PR-57 adds the particle row, and PR-58, PR-59, and PR-60 add their full-screen passes. PR-94 counts one pass for each fog (D-898).
- The particle row holds the 8192 live particles of the sweep of 2026-09-17 (D-617). The screen plays one hit at a time, so the test counts the largest burst of a hit (D-879).
- The first rows of the budget come from the run of 2026-09-17: 15 lights with shadows, 8192 live particles, and 3 full-screen passes (D-617).
- The light row rises to 24 after a new Deck sweep with 24 paired lights, before PR-56 merges (D-854). Each light source counts two lights (D-853). The sweep of 2026-09-21 held, with 4.55 ms at the 95th percentile for the full load (F-96).
- Each row is a floor, and not the ceiling of the Deck, because no stage of the sweep missed the target (F-66).
- The sweep measured those 3 passes with the CRT on, and D-618 later removed that pass, so the shipped stack carries one pass less.
- M-6 measures the first playable on the Deck against the budget (D-161). A miss changes the budget or the content in a PR with a measurement (G-14).

> *In plain English:* the Deck test finds how much the Deck can draw at full speed, and that number goes into a file. A test then refuses any place that asks for more, before the engine can drop a light in silence.

### 7.5 Normal maps

Built by PR-48. Phase file: `phase-2-first-playable.md`.

- PR-48 builds a normal map for each sprite, tile, and piece from its drawing file, with an optional override grid (D-183, D-184, D-516). The rim, the height of each color, and the override grid follow D-838 to D-840. `area-tools.md` section 7.7 holds the command.
- A portrait, an icon, and a window frame have no normal map, because they never take scene light (D-210, `area-art.md` section 7.4).
- The normal-map atlas uses the atlas index of the color atlas, so each frame sits at the same place in both (D-184, D-517).
- The normal-map atlas takes the pixel test of the color atlas on every CI leg (D-184, D-502, F-19).
- A review sheet draws each sprite under eight fixed light directions, and the PR description names each drawing on it (D-514, D-521).
- The sheet computes light in Tools, so PR-56 shows the light of the engine for the first time (D-521).
- PR-48 lands right after PR-10 and right before PR-56 (D-520, D-521).
- From PR-48 on, each art PR commits the normal-map atlas with its test (`area-art.md` section 7.11).

> *In plain English:* a normal map tells the light which way each pixel faces. A tool makes one from each drawing, and the owner checks a sheet of each sprite lit from eight sides.

### 7.6 Light and shadows

Built by PR-56. Phase file: `phase-2-first-playable.md`.

- PR-56 is the first PR that draws light (D-520). It lights the map scene of PR-7 and the battle scene of PR-10.
- A light setup holds the ambient light and the point lights of one map at one time of day (D-442). It names the map id and the time of day, so the map file never changes for light (D-495, D-519).
- Most maps need one light setup, and a place needs one light setup for each time of day that the story gives it (D-205, D-442).
- A battle takes the ambient light and one key light from the light setup of the map where the fight began (D-205, D-442, D-850).
- The ambient light is the one `CanvasModulate` of the world, because Godot allows one on a canvas (the external facts above).
- Point lights come from torches, waystones, and spells (D-183). A decor file beside each map places each torch, and no rule reads it (D-844).
- Each decor kind that gives light holds its default light. The light setup can change the light of one piece by its id, or add a light at a tile (D-843).
- Each point light needs a light texture. Game builds it at load and checks it, because a light with no texture draws nothing in silence (F-46).
- An atlas texture cannot serve as a light texture, so the light textures stay outside the atlas (the external facts above).
- Each light sets a height, because at the default height of 0 a flat pixel of a normal-mapped sprite takes no light (F-46).
- Each light and the ambient light name a palette key with a strength in basis points (D-846).
- Walls cast hard shadows (D-183). Each wall that faces a walkable tile takes light on its face, and a strip at its back blocks the light (D-845, D-852).
- Each figure casts a shadow from its feet. Each light source is a pair of lights, so no figure darkens itself (D-853).
- A `TileSet` gives one occluder to each kind of tile, and a wall face needs a shape of its own place. Thus Game builds one occluder for each wall that faces a walkable tile, from the terrain (D-852).
- Game gives each sprite, tile, and piece its normal map through a `CanvasTexture` with the color atlas and the normal-map atlas. Both atlases draw with the Nearest filter (F-45).
- A `CanvasTexture` gives no specular light by default, and no decision asks for specular light (D-183).
- A shader on a lit sprite, tile, or piece never uses `NORMAL_MAP`. Godot corrects the normal of a flipped draw before the shader code, and `NORMAL_MAP` replaces that normal (the external facts above).
- The hit flash of PR-10 keeps this rule, and so does each later shader on lit art (`area-art.md` section 7.4).
- A puzzle of light and dark in PR-21 draws its light from Core state (D-41, section 7.1).
- The party carries a light that follows the drawn place of the lead on each frame. A switch of Game turns it on, and PR-91 connects the switch to the torch item (D-847, D-848).
- The screen tests capture a lit fixture map and a lit fixture battle (D-172).

> *In plain English:* each place gets its light from a small file: how dark the place is and where each torch glows. Walls throw hard shadows, and each sprite catches light on the side that faces the flame.

### 7.7 Effect files and particles

Built by PR-57. Phase file: `phase-2-first-playable.md`.

- An effect file holds its emitters, its palette colors, and its timings in ticks (D-182, D-266).
- Core holds the record and the strict reader of each effect file (D-517). A bad field fails the load with the file and the field (T-2).
- Game builds the Godot particle nodes from each effect file at load, and no Godot resource file holds an effect (D-182, G-6).
- Each palette key of an emitter takes a node of its own, so no particle takes a blend of two keys (D-181).
- Each node runs at speed zero. The screen seeks a burst to its age in ticks (D-172, D-875).
- A seek restarts the node with its fixed seed, and then it asks for the time of that age at 60 steps a second.
- The seed of a burst comes from the tick when its event started. Thus one tick of a fight shows one picture in play and in a capture (T-7).
- A particle color is a palette key, and scene light can still change it on screen (D-181, D-182).
- A particle with no texture draws as a square, so a spark of one color needs no drawing file (the external facts above).
- Game uses `GPUParticles2D` for each emitter (D-875).
- In the screen tests, each emitter takes a fixed seed (D-172). `CPUParticles2D` ignores that seed for some emission shapes (the external facts above).
- The screen tests of CI run the Mobile renderer, which holds particle trails and `emit_particle` (D-731). Thus each capture shows the effect of the Deck.
- The effect file of an ability, an element, or a place names the content ids that it serves (section 7.1).

> *In plain English:* a burst of sparks or a drift of snow is a small data file: how many bits, which colors, and how long. The game builds the effect from that file when it starts.

### 7.8 Battle effects

Built by PR-10, PR-57, and PR-12. Phase file: `phase-2-first-playable.md`.

- Battle effects are heavy and short (D-186).
- PR-10 draws the attack pose, the lunge of an enemy, the hit flash, and the damage numbers (D-96, D-108, D-213, D-832). The hit flash is a `.gdshader` file of Game (D-825).
- PR-10 keeps each timing as a constant of Game, in ticks, and PR-57 moves each one into the battle file (D-829, D-883).
- PR-57 adds blood or sparks on a hit, from the hit file that serves the target (D-186, D-879).
- PR-57 also adds a short screen shake and a brief hit-stop on a heavy blow (D-186, D-880).
- A heavy blow is a hit on an element that the target is weak to (D-877).
- PR-12 adds the flash of a spell, with a point light of PR-56 for its length (D-183, D-878).
- A heavy blow also starts the vibration of D-434, and `area-ui-input.md` holds it.
- A screen shake moves the battle picture alone, by whole pixels of the frame. The UI stays still (D-876).
- The flash and shake reduction gives each flash and each shake a reduced form (D-214). The hit flash of PR-10 stays at each level (D-881). The screen tests capture each of the three levels: full, reduced, and off (D-863).
- Core resolves each action and emits its events, and Game plays the effects from them (D-168). `area-battle.md` holds how the battle scene paces the events of a turn under D-522.

> *In plain English:* a hit in battle shows blood, sparks, and a jolt, and it passes fast so the fight keeps its pace. A player who needs calm can turn the flashes and the shakes down.

### 7.9 Ambient effects

Built by PR-58. Phase file: `phase-2-first-playable.md`.

- Region one has four ambient kinds: snow and wind, fog and mist, fire with embers and smoke, and dust with drips and motes (D-187).
- Each map has its own weather, and its time of day changes its light alone (D-202, D-442).
- The ambient effects of a place play over its battle backdrop (D-205).
- A fire is a decor kind, and it carries a point light of PR-56 (D-183, D-888).
- Each wall torch and the carried light take a flame, embers, and a light that changes in steps from the tick (D-890, D-891).
- Fog never hides an enemy that the player must see (D-187). Fog draws above the figures, and the luma test of D-886 caps its strength (D-885).
- Fog is a noise shader of 1 to 3 layers in one pass. Each layer fades from clear to its strength in a few steps, over blocks of art pixels, in one palette key, and drifts slowly (D-897, D-900, D-907). The strongest layer wins where layers overlap, and the density is even over the view (D-899, D-902). PR-94 replaced the text grid of D-887, because the grid repeats over the view (F-101).
- The test content holds one ambient file of each kind for the fixture dungeon, and the shipped dungeon takes dust and drips (D-889).
- Fog and each other full-screen ambient effect count against the effect budget (D-523). A fog counts as one pass, whatever its count of layers (D-898).
- The ambience of each map matches its ambient effects, and `area-audio.md` holds the sound (D-424).
- PR-17 adds the ambient effects of the village, the land near it, the mining town, and the hanging cells (D-362, D-369, D-520).

> *In plain English:* each place has its own weather: snow in the pass, smoke by a fire, dust in the mine. The weather never hides an enemy that the player needs to see.

### 7.10 Glow

Built by PR-59. Phase file: `phase-2-first-playable.md`.

- Fire, spells, waystones, and the thing below glow a little, and sprites and tiles never glow (D-188).
- UI never glows (D-210).
- Glow is a full-screen pass, so it counts against the effect budget (D-523).
- HDR 2D works on Forward+ and Mobile, and the screen tests of CI run the Mobile renderer (D-731, the external facts above). So a CI capture of glow shows the glow of the Deck.
- Godot adds light to a pixel with no upper clamp, so a bright light on a pale sprite can pass the glow threshold (F-47).
- OQ-102 holds how glow stays off sprites and tiles.

> *In plain English:* flames and magic give off a soft haze of light, and the people and walls that they light stay crisp.

### 7.11 Transitions

Built by PR-60. Phase file: `phase-2-first-playable.md`.

- The library holds ten transitions (D-195).
- Content assigns a transition to each kind of encounter, with a default for each region (D-196). The table names encounter kinds and region ids (section 7.1).
- A transition is an effect file with its shader in a `.gdshader` file of Game (D-182, D-191, D-825).
- A transition is a full-screen pass, so it counts against the effect budget (D-523).
- Color split meets the flash and shake reduction (D-195, D-214).
- Snow whiteout fits region one, and each later region adds transitions of its own (D-194).
- After a battle, the map waits for the screen to change back, and a wait intent ends the wait (D-522).

> *In plain English:* each fight starts with a screen effect, such as shattered glass or a whiteout of snow. The kind of fight picks the effect, so a boss always looks different from a common fight.

### 7.12 The style of an effect

Built by every effect PR, and kept by the screen tests. Phase file: `phase-2-first-playable.md`.

- Every effect draws with the palette of 64 colors and hard edges, and no smooth gradient (G-27, D-181, D-622). The fog alone fades, in steps over blocks of art pixels (D-900, D-907).
- The rule covers each particle, the fog, the glow, and each transition (D-187, D-188, D-195).
- A full-screen pass draws at the pixel size of the frame, so an effect pixel matches an art pixel (D-230, F-67).
- The owner reads each new effect on the Mac as its PR lands, and not on the Deck (D-622, D-623).
- The screen tests of PR-41 capture each effect, so a change of style fails the job (D-172).
- The glow of D-188 blooms by design, and OQ-102 holds how it stays off sprites and tiles (F-47).
- The fog of the Deck test drew a smooth gradient with colors outside the palette, and no shipped effect draws that way (D-622).

> *In plain English:* every effect uses the same colors as the art and keeps hard pixel edges. Smoke, fog, and light look drawn, and never like a modern filter over a drawing.

### 7.13 Effects in the tests

Built by PR-41 and every effect PR. Phase file: `phase-2-first-playable.md`.

- `--headless` draws nothing, so the smoke session never tests an effect (F-23).
- The screen-test job of PR-41 captures each effect in a fixture under Xvfb with the Mobile renderer of the shipped build. A changed pixel fails the job (D-172, D-731, `area-ci.md` section 7.12).
- Each effect PR adds its captures, and the reduced form of each flash and shake (D-214).
- A shader that fails to compile writes its failure to the log (the external facts above). So the screen-test job fails on an error line in the Godot log (T-2).
- The test job loads each effect file and light setup, and it runs the budget test on every CI leg (D-517, D-523).
- The contact sheet shows the real renderer on the Mac of the owner at milestones (D-172).
- The owner judges the feel of the effects on the Deck at Gate 2 (D-52, D-92).

> *In plain English:* the CI computers have no screen, so a software screen takes fixed pictures of each effect. One wrong pixel fails the change. The owner still judges the real look on the Deck.

### 7.14 Effects by PR

| PR | Effects | Decisions |
|---|---|---|
| The Deck test | The test scene, the renderer pick, and the effect budget. Done 2026-09-17 | D-160, D-523, D-616, D-617 |
| PR-10 | The attack pose, the hit flash, the damage numbers, and the drift of the backdrop | D-96, D-205, D-213 |
| PR-48 | The normal maps and their review sheets | D-184, D-521 |
| PR-56 | Light setups, decor files, point lights, the carried light, shadows, and the budget test | D-183, D-442, D-523, D-842 to D-847, D-850 |
| PR-57 | Effect files, particles, the shake, and the hit-stop | D-182, D-186, D-877 to D-883 |
| PR-12 | The flash of a spell | D-186, D-878 |
| PR-58 | The four ambient kinds | D-187, D-202 |
| PR-94 | The procedural fog: a soft noise shader of 1 to 3 layers in one pass | D-896 to D-907 |
| PR-59 | Glow | D-188 |
| PR-92 | The tilt-shift blur, the vignette, and the light shafts of the HD-2D look | D-849 |
| PR-60 | The ten transitions and the table of kinds | D-195, D-196 |
| PR-17 | The light setups, the ambient effects, and the effect files of the first places | D-362, D-520 |
| PR-21 | The light of the puzzles of light and dark | D-41 |
| PR-23 to PR-27 and PR-81 | The light setups and the effects of each later place, the sealed gallery included | D-313, D-575 |

- A place that the story shows at another time of day adds a light setup in the PR of that story scene (D-442). The arc batch of PR-28 or PR-29 that writes the night pass through the mining town adds its night light setup (D-333, D-442).
- Each later region adds its own transitions (D-194).

### 7.15 Effects that other area files hold

| Part | Area file | PR |
|---|---|---|
| The command that builds normal maps | `area-tools.md` | PR-48 |
| The frame, the fit, and the reduction setting | `area-ui-input.md` | PR-7 and the PRs that `area-ui-input.md` names |
| The screen-test job | `area-ci.md` | PR-41 |
| The battle scene and the pace of a turn | `area-battle.md` | PR-10 |
| The map scene, the slide of a step, and the puzzles of light and dark | `area-exploration.md` | PR-7 and PR-21 |
| The ambience and the vibration | `area-audio.md` and `area-ui-input.md` | PR-70 and PR-63 |
| The trailer capture with fixed seeds | `area-release.md` | PR-74 |

### 7.16 The contract of every later effect PR

Each later PR that adds or changes an effect keeps this list. The phase files make exit tests from it.

1. Write each effect as an effect file or a light setup with integer values (D-182, D-517).
2. Name the content ids in the effect file, never an effect in a rule file (D-495).
3. Time each effect in ticks, and make no rule wait for it (D-266, D-522).
4. Add a wait intent and its replay test where the world waits (D-522).
5. Keep each map and each battle inside the effect budget (D-523, F-46).
6. Add the screen-test captures, with the reduced form of each flash and shake (D-172, D-214).
7. Keep the UI out of scene light and glow (D-210).

> *In plain English:* every new effect follows the same seven steps. It lives in a data file, never touches the rules, fits the Deck, and proves its look in a fixed picture.

### 7.17 The HD-2D look

Built by PR-56 and PR-92. Phase file: `phase-2-first-playable.md`.

- The art target is the look of Octopath Traveler, in 2D (D-849). The game stays a flat 2D view, with no 3D scene.
- PR-56 gives the base of the look: a dark ambient light, warm pools of torch light, normal-mapped sprites, and hard shadows (D-183, D-843).
- PR-59 gives the glow on light alone (D-188).
- PR-92 adds three full-screen passes: a tilt-shift blur at the top and the bottom of the frame, a vignette, and light shafts (D-849).
- Each pass of PR-92 counts against the effect budget (D-523). The budget of D-617 holds 3 passes, so a new Deck sweep measures the heavier stack before PR-92 merges (G-14).
- The passes draw the world alone, and the UI above it stays sharp and unlit (D-210).

> *In plain English:* the goal is a storybook diorama: dark places, warm pools of light, and a soft blur at the edges. The light comes first, and the blur and the shafts come later, after a test on the Deck.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The effects work keeps this order inside it:

1. Owner and a session: the Deck test picks the renderer and measures the effect budget, before PR-1 (D-160, D-523).
2. PR-1: the Game project with the renderer of the Deck test.
3. PR-34: the atlas that the normal maps read.
4. **← GATE 1 (foundation).**
5. PR-7: the map scene.
6. PR-41: the screen-test job, before the first effect PR (D-172).
7. PR-55 and PR-10: large pictures, the battle scene, and the hit flash.
8. PR-48: the normal maps, right before PR-56 (D-521).
9. PR-56: light, shadows, and the budget test, the first PR that draws light (D-520, D-523).
10. PR-57: effect files, particles, and the battle effects.
11. PR-58: the ambient effects.
12. PR-94: the procedural fog (D-896).
13. PR-59: glow.
14. PR-92: the three passes of the HD-2D look, after a new Deck sweep (D-849).
15. PR-60: the transitions.
16. PR-17: the first places with their light and effects.
17. M-6: the Deck against the effect budget.
18. **← GATE 2 (first playable).** The owner plays every effect on the Deck (D-161).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block effect PRs, and each PR or step asks its questions when it starts (D-487):

- OQ-92 and OQ-93 are resolved. D-597 and D-598 hold the answers, and the branch `spike/deck-test` holds the scene.
- OQ-94 to OQ-97 are resolved. D-842, D-843, D-845, and D-846 hold the answers.
- OQ-98 and OQ-99 are resolved. D-875 and D-876 hold the answers.
- OQ-100: the reduced form of a flash and a shake. Resolved 2026-09-22 by D-863.
- OQ-101: how fog keeps an enemy visible. Resolved 2026-09-22 by D-885.
- OQ-102: how glow stays off sprites. Blocks PR-59.
- OQ-103: where shader code lives. Resolved by D-825.
- OQ-220 to OQ-231: the place, the form, the passes, the overlap, the edges, the resolution, the spread, the color, the test floor, the strength, and the pixel look of the procedural fog. Resolved 2026-09-22 by D-896 to D-907.
- OQ-79: how the screen-test job pins Mesa. Closed 2026-09-20 by D-729, and D-730 holds the pin.
- OQ-89: pixel snap in Game. Blocks PR-7.
- OQ-183: the scale of the frame on a screen. Blocks PR-7 and PR-34, and the probe of D-621 answers it.

No open question blocks this file.
