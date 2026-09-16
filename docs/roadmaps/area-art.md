# Area roadmap: Art

Status: **focused area roadmap, draft in PR #11.** This file says how the art of the game works, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-14 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-tools.md` holds the commands that render art, and `area-ci.md` holds the jobs that test it. The file `area-effects.md` holds light, normal maps, particles, and the CRT shader. The file `area-ui-input.md` holds the frame, the fit to a screen, the fonts, and the menus.

External facts, each with the date of its check:

- `gh` 2.99.0 of 2026-09-01 adds the flag `--attach`, which "uploads local images and videos and adds them to issue, pull request, or comment bodies". Source: `gh api repos/cli/cli/releases/tags/v2.99.0`, run 2026-09-14.
- The help of `gh pr create` says: "You can attach up to 50 files per command." The commands `gh pr edit` and `gh pr comment` take the flag too. The Mac of the owner has `gh` 2.100.0. Sources: `gh pr create --help`, `gh pr comment --help`, and `gh --version`, run 2026-09-14.
- "You need push access to the repository to attach files." Source: `https://docs.github.com/en/github-cli/github-cli/attaching-files-with-github-cli`, read 2026-09-14.
- "For public repositories, uploaded files can be accessed without authentication. In the case of private and internal repositories, only people with access to the repository can view the uploaded files." The largest upload is "10MB for images and gifs". Source: `https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files`, read 2026-09-14.
- GitHub compares two versions of an image "in three different modes: 2-up, swipe, and onion skin". "The maximum number of renderable files (such as images, PDFs, and GeoJSON files) in a single diff is limited to 25." Sources: `https://docs.github.com/en/repositories/working-with-files/using-files/working-with-non-code-files` and `https://docs.github.com/en/repositories/creating-and-managing-repositories/repository-limits`, read 2026-09-14.
- `ImageTexture.create_from_image` "Creates a new ImageTexture and initializes it by allocating and setting the data from an Image". For a null or an empty image, the code logs "Invalid image: null" or "Invalid image: image is empty" and returns a null reference. Sources: `doc/classes/ImageTexture.xml` and `scene/resources/image_texture.cpp` in `godotengine/godot` at the tag `4.7.2-stable`, read 2026-09-14.
- "The maximum texture size is 16384×16384 pixels due to graphics hardware limitations." "On desktops and laptops, textures larger than 8192×8192 may not be supported on older devices." Sources: `doc/classes/ImageTexture.xml` at `4.7.2-stable` and `https://docs.godotengine.org/en/stable/tutorials/3d/3d_rendering_limitations.html`, read 2026-09-14.
- The setting `rendering/textures/canvas_textures/default_texture_filter` has the default value 1, and the code names the values "Nearest,Linear,Linear Mipmap,Nearest Mipmap". The default is Linear. The Nearest filter "reads from the nearest pixel only". Sources: `doc/classes/ProjectSettings.xml`, `core/config/project_settings.cpp`, and `doc/classes/CanvasItem.xml` at `4.7.2-stable`, read 2026-09-14.
- The editor of 4.7 writes the stretch mode `canvas_items` and the aspect `expand` into a new project. In that mode, "there is no longer a 1:1 correspondence between sprite pixels and screen pixels, which may result in scaling artifacts". Sources: `doc/classes/ProjectSettings.xml` and `editor/editor_node.cpp` at `4.7.2-stable`, read 2026-09-14.
- "An atlas is a grid of tiles laid out on a texture. Each tile in the grid must be exposed using create_tile." When a tile does not fit, `create_tile` logs "Cannot create tile" and returns no value. The padding of an atlas source "avoids a common artifact where lines appear between tiles". Sources: `doc/classes/TileSetAtlasSource.xml` and `scene/resources/2d/tile_set.cpp` at `4.7.2-stable`, read 2026-09-14.
- The canvas shaders of both Godot renderer families multiply the normal by the sign of the source rectangle: `normal.xy *= sign(src_rect.zw)` and `normal.xy *= sign(read_draw_data_src_rect.zw)`. Issue 70517, "Normal Map doesn't flip properly with flip_h and flip_v in Sprite2D", closed as completed on 2023-01-17 for 4.0. Issue 101277, a flip of a normal map that a shader writes to `NORMAL_MAP`, closed as not planned on 2025-01-08. Sources: `servers/rendering/renderer_rd/shaders/canvas.glsl` and `drivers/gles3/shaders/canvas.glsl` at `4.7.2-stable`, and `https://github.com/godotengine/godot/issues/70517` and `/101277`, read 2026-09-14.
- The boot splash image is "Path to an image used as the boot splash", and "The only supported format is PNG". The engine reads the file by its path with `ImageLoader::load_image` at the start. With `use_filter` set to false, the splash "uses nearest-neighbor interpolation (recommended for pixel art)". Sources: `doc/classes/ProjectSettings.xml` and `main/main.cpp` at `4.7.2-stable`, read 2026-09-14.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Every picture of the game starts as text that a session writes and the owner approves (D-107, G-24). A drawing file holds palette keys, and a large picture places drawing files as pieces (D-515, D-516). A tool renders every drawing file into the atlas. Game draws from the atlas at whole pixels, so an art pixel matches a text pixel (D-228, D-230, D-508). The owner judges each batch from review sheets in its PR (D-514). Art never decides an outcome of play, so art stays out of the rule files and the content hash (D-495, D-519).

The order of the area follows the first user of each part. The PNG code and the atlas close Phase 1, because every screen draws from the atlas (PR-47, PR-34). The first screen loads the atlas in Game (PR-7). Large pictures land right before the first backdrop (D-518). The tools for hand edits and previews come before the first place of the game (D-497). The content PRs draw the art of each place, from PR-17 on.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the art area.

| # | Finding | Binds |
|---|---|---|
| F-17 | The 32-color palette had too few free colors | PR-34: 64 colors (D-181, D-185) |
| F-19 | Compressed PNG bytes depend on the encoder | PR-34 and PR-55: tests compare decoded pixels |
| F-20 | A repeated palette key passed in silence | PR-34: a repeated key fails |
| F-24 | A 32-pixel grid holds four times the pixels of a 16-pixel grid | Every art batch, and the PNG import of PR-51 |
| F-42 | The Godot export reads the project folder alone | PR-7: the atlas loads from the bytes of the Game assembly (D-508) |
| F-44 | One grid of a full-screen picture holds about 1.1 million palette keys | PR-55: large pictures of pieces (D-516, D-518) |
| F-45 | Three Godot defaults meet the pixel art | PR-7: the Nearest filter, and a check after each Godot call that logs a failure alone |

## 7. Roadmap

Each part below says how one part of the art works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The art pipeline

Built by PR-34, and kept by every art batch. Phase files: `phase-1-foundations.md` and every later phase file.

- Sessions draw every sprite, tile, portrait, and piece as a drawing file, and the owner approves each batch (D-57, D-107, G-24).
- The drawing files are the source. The atlas, the normal maps, and the review sheets come from them, and nobody edits those by hand (D-107, D-184).
- The owner can edit a drawing file, or edit a PNG that the PNG import of PR-51 reads back (D-107, `area-tools.md` section 7.11).
- The art keeps the style of D-201 and D-237: a dark outline for each material, three or four tones, and no dithering. The five sample grids set the look (D-402).
- Art lives in `content/`, outside the override set, so each art batch takes the review of the other provider (D-71, D-185).
- No art file decides an outcome of play, so the content hash never reads one (D-495).

> *In plain English:* every picture in the game is a text file that a session writes and the owner approves. The images that the game draws always come from those files, so nobody edits an image that a tool made.

### 7.2 The palette

Built by PR-34. Phase file: `phase-1-foundations.md`.

- One palette file of 64 colors, `content/sprites/palette.json`, holds every color that a drawing file names (D-89, D-181, D-238).
- The first 48 colors keep their indices and their keys (D-121, D-181). PR-34 adds the 16 colors of D-185 and proposes their keys with the swatch sheet.
- A key is one character, and the dot is transparent. JSON writes a quote mark or a backslash as two characters, so no key is one of them (D-515).
- A repeated key or a repeated index fails with the key and the index (F-20, T-2).
- Light on screen can reach any color, and a drawing file names palette keys alone (D-181).
- Effect files name palette colors too, so Game reads the palette (D-182). Core holds the record of the palette file (D-517).

> *In plain English:* the game has one box of 64 paints, and every drawing uses only those paints. Lights on screen can mix new shades, but the drawings never leave the box.

### 7.3 Drawing files

Built by PR-34. Phase file: `phase-1-foundations.md`.

- Each drawing file holds its id, its size, and its frames. Each frame lists its rows as strings, with one palette key per pixel (D-515).
- The strict reader of Core refuses an absent field, an unknown field, a row of the wrong length, and an unknown key (D-177, D-517).
- A read error names the file, the frame, the row, and the column (T-2).
- A drawing file names the content ids that it draws, with a kind where one thing has more than one drawing (D-519). A rule file never names art.
- A test proves that each thing that Game draws has its drawing (D-519).
- OQ-88 holds the unit of the time of a frame.
- PR-34 converts the five sample grids of `docs/samples/` into drawing files, and the sample keeps its `.grid` files (D-402, D-515).
- PR-34 retires `docs/tools/make-atlas.py`, which reads the old format (D-406).

> *In plain English:* each drawing is a small data file that still reads like a picture made of letters. The file says what it draws, so a new drawing never touches the rules of the game.

### 7.4 Sizes and frames

Built by PR-34, and drawn by the content PRs. Phase files: every phase file.

| Drawing | Size in pixels | Frames | Decisions |
|---|---|---|---|
| Tile | 32 by 32 | One | D-110, D-228 |
| Party member on the map | 32 by 32 | Front, back, and side views, each with an idle frame and a two-frame walk | D-199 |
| Party member in battle | 32 by 32 | An attack pose, a hurt flinch, and a down pose | D-108, D-200 |
| Moving enemy or NPC on the map | One, two, or three tiles square | Front, back, and side views, each with a two-frame walk | D-206, D-207 |
| Standing enemy on the map | One, two, or three tiles square | One frame that flips | D-108, D-207 |
| Enemy in battle | 32 by 32 for a common enemy, 64 by 64 for an elite, and 96 by 96 or larger for a boss | Common enemies flip. Elites and bosses have a few frames | D-189, D-236 |
| Portrait | 64 by 64 | One expression | D-109, D-234 |
| Piece of a large picture | Set by the format of PR-55 | One | D-516, D-518 |
| Window frame | 48 by 48, which Game draws as nine parts that stretch | One | D-220, `area-ui-input.md` |
| Icon for an element or a status | 16 by 16 | One | D-214, `area-ui-input.md` |
| Glyph for a button | 16 by 16 | One | D-222, `area-ui-input.md` |

- A side view faces one way, and Game mirrors it for the other side with `flip_h` (D-199). The atlas holds no mirrored frame.
- Both renderer families correct the normal map of a sprite that `flip_h` mirrors, so its light falls on the correct side (the external facts above).
- A custom shader that writes `NORMAL_MAP` gets no such correction (the external facts above). Section 7.6 of `area-effects.md` keeps each shader on a lit sprite away from `NORMAL_MAP`.
- Each sprite, tile, and piece has a normal map. A portrait, an icon, a glyph, and a window frame have none, because they never take scene light (D-183, D-210, D-516).

> *In plain English:* tiles and characters are 32 pixels square, faces are 64, and big enemies are larger. A character that faces left is the same drawing turned over, and the light still falls on the correct side.

### 7.5 Large pictures

Built by PR-55. Phase file: `phase-2-first-playable.md`.

- A large picture places pieces at pixel positions, with repeats (D-516). A piece is a drawing file of section 7.3.
- A backdrop layer, other full-screen art, and a store image are each a large picture (D-205, D-475, D-516). Full-screen art covers the frame of 1280 by 720 (D-568).
- PR-55 adds the format with its load test, a render as a PNG in Tools, and the draw in Game (D-518).
- PR-55 lands right before PR-10, the first PR that draws a backdrop (D-518).
- A large picture that names an absent piece fails with the file and the entry (T-2).
- A test decodes the render of a fixture large picture and compares its pixels with its pieces (F-19).
- OQ-91 holds which operations a large picture offers on a piece.
- The battle scene of PR-10 draws the drift of the backdrop layers and the ambient effects over them (D-205).
- `area-release.md` holds the sizes and the upload of the store images, in PR-76 (D-475, D-550).

> *In plain English:* a battle background is too big to write as one text picture. The game builds it like a stage set from small drawn parts. Each part stays small enough to draw and to check.

### 7.6 The atlas and its index

Built by PR-34. Phase file: `phase-1-foundations.md`.

- The `atlas` command renders every drawing file into the atlas in `content/sprites/` (D-107, `area-tools.md` section 7.6).
- Beside the atlas, the command writes the atlas index: the place of each frame in the atlas. Core holds its record (D-517).
- The repository commits the atlas and the index. A test proves that both match the drawing files, by decoded pixels (D-107, F-19, G-24).
- A stale atlas fails that test until the command runs again.
- The atlas and the index sit in `content/`, so the Game assembly carries them (D-508).
- OQ-85 holds whether the atlas splits into pages, and OQ-86 holds how the atlas places tiles.
- No page passes the largest texture that Godot names, 16384 by 16384 pixels (the external facts above).

> *In plain English:* a tool packs every drawing into one image that the engine loads, and it writes a list of where each drawing sits. A test proves that the image still matches the text drawings.

### 7.7 Art in Game

Built by PR-7. Phase file: `phase-2-first-playable.md`.

- Game reads the atlas and the index from its own assembly (D-508). It makes an image with `Image.LoadPngFromBuffer` and a texture with `ImageTexture.CreateFromImage`.
- An error code from the load, or a null texture, stops the game with the resource name and the reason (T-2). Godot only logs these failures (F-45).
- Game finds each frame through the index and the content ids of D-519. A content id with no drawing fails with the id (T-2).
- Every texture draws with the Nearest filter. The project setting starts as Linear, so PR-7 sets it, and a test reads it in `project.godot` (F-45).
- Game draws art at 1x on the frame of 1280 by 720, so art pixels match text pixels (D-228, D-230, D-568).
- `area-ui-input.md` holds the frame, the fit, and the stretch mode, which the editor of 4.7 sets to `canvas_items` in a new project (F-45).
- Core positions stay on whole tiles, and each step slides between tiles on screen (D-203). OQ-89 holds how each sprite stays on a whole pixel during a slide.
- No Godot resource file holds art, such as a `SpriteFrames` file or a `TileSet` file (G-6). Game builds each Godot object from the atlas at load.

> *In plain English:* the game loads its art image from inside its own program file and draws each picture with hard pixel edges. If the image or a drawing is absent, the game stops and says which one.

### 7.8 Review sheets

Built by PR-34, and used by every art PR. Phase files: every phase file.

- A Tools command renders the review sheets of an art batch as PNG files, and no sheet enters git (D-514). OQ-87 holds the form of a sheet.
- The session uploads the sheets into the PR description with `gh pr edit --attach` (D-514, G-25).
- The description names each drawing on each sheet, and the commit that the sheets show.
- A changed batch gets new sheets in the description.
- The swatch sheet, the map preview, and the normal-map review sheet reach the owner the same way (D-165, D-185, D-514, D-521).
- The committed atlas also shows in the Files changed tab, where GitHub compares two versions of an image. That tab shows 25 images at most (the external facts above).
- An upload needs push access and `gh` 2.99.0 or later (the external facts above). `docs/runbooks/dev-machine.md` names the version.
- After the repository goes private, only people with access see the uploads (D-456).

> *In plain English:* for each batch of art, a tool draws large sample sheets, and the session puts them straight into the pull request page. The owner looks at the pictures there, and the repository holds no sample image.

### 7.9 Art batches by PR

The phase files give each batch its scope. This table names the art that the decisions put in each PR.

| PR | Art | Decisions |
|---|---|---|
| PR-34 | The 16 new colors and the swatch sheet. The front views of the five cast members, from the sample | D-185, D-402, D-405 |
| PR-7 | Fixture tiles for the fixture dungeon, and the lead on the map | D-306, PR-7 in `docs/design.md` |
| PR-10 | The attack pose, the hit flash, and a fixture backdrop as a large picture | D-96, D-108, D-111, D-516 |
| PR-36 | Fixture portraits | D-234 |
| PR-17 | The tile sets of the village, the land near it, the mining town, and the hanging cells, with their edge tiles. The enemies, the NPCs, and the backdrop. The map and battle frames of Marrek, Bergit, and Dagvar | D-110, D-199, D-200, D-204, D-362, D-369 |
| PR-23 to PR-27 | The tile sets, enemies, bosses, and backdrops of each later place, and the NPCs of the second hub | D-110, D-313 |
| PR-28 and PR-29 | The portraits of the cast | D-109, D-234 |
| PR-42 | The icons of the lessons of region one | PR-42 in `docs/design.md` |
| PR-33 | The studio mark (OQ-57, OQ-90) | D-468 |
| The store page work at Gate 2 | The capsules, the logo, and the library images, as large pictures | D-475, D-516 |
| Phase 6 | The icons of the achievements | D-466 |

- The design text of PR-17 names the sprite set of Marrek alone. Bergit and Dagvar fight and act in story scenes of the first playable, so PR-17 also needs their frames (D-114, D-200, D-362).
- The frames of Ottild and Elio land before the place where each joins the party (D-342). PR-23 draws them, because both join between the hanging cells and the deep mine.
- `area-ui-input.md` places the window frames of D-220, the icons of D-214, and the glyphs of D-222.

### 7.10 Art that other area files hold

| Part | Area file | PR |
|---|---|---|
| The `atlas` command, the PNG code, and the PNG import | `area-tools.md` | PR-34, PR-47, and PR-51 |
| The map preview and the tile-edge tool | `area-tools.md` and `area-exploration.md` | PR-52 and PR-53 |
| Normal maps, light, particles, glow, transitions, and the CRT shader | `area-effects.md` | PR-48, PR-56 to PR-60, and PR-37 |
| The frame, the fit, the fonts, the window frames, the icons, and the glyphs | `area-ui-input.md` | PR-7, PR-10, and the PRs that `area-ui-input.md` names |
| The pixel tests in the test job, and the screen tests | `area-ci.md` | PR-34, PR-55, and PR-41 |
| The store images on the store page | `area-release.md` | PR-76 |

### 7.11 The contract of every later art PR

Each later PR that adds or changes art keeps this list. The phase files make exit tests from it.

1. Draw each picture as a drawing file or a large picture (D-515, D-516).
2. Use palette keys alone (D-181).
3. Name the content ids in the drawing file, never a drawing in a rule file (D-519).
4. Run the atlas command, and commit the atlas and its index (D-107, D-517).
5. Pass the pixel test on every CI leg (F-19, G-24).
6. Attach the review sheets to the PR description (D-514, G-25).
7. From PR-48 on, commit the normal-map atlas with its test (D-184).

> *In plain English:* every new batch of art follows the same seven steps. It uses the palette alone, never touches the rules, passes the picture test, and shows itself to the owner in the pull request.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). The art work keeps this order inside it:

1. PR-47: the PNG code, right before the atlas (D-496).
2. PR-34: the palette, the drawing files, the atlas and its index, the review sheets, and the five cast drawings.
3. **← GATE 1 (foundation).** The pixel test of the atlas passes on every CI leg.
4. PR-7: Game loads the atlas and draws the first map with the Nearest filter.
5. PR-55: large pictures, right before PR-10 (D-518).
6. PR-10: the first backdrop.
7. PR-48: the normal maps of every drawing file, right before PR-56, the first PR that draws light (D-520, D-521).
8. PR-36: the fixture portraits.
9. PR-51, PR-52, and PR-53: the PNG import, the map preview, and the tile-edge tool, before PR-17 (D-497).
10. PR-17: the art of the first playable, with a normal map for each new drawing.
11. **← GATE 2 (first playable).** Then the store images of the store page (D-471, D-475).

`area-effects.md` holds the order of the effect PRs between PR-10 and PR-17 (D-520).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block art PRs, and each PR asks its questions when it starts (D-487):

- OQ-85: the pages of the atlas. Blocks PR-34.
- OQ-86: how the atlas places tiles, and how Game draws a map. Blocks PR-34 and PR-7.
- OQ-87: the form of a review sheet. Blocks PR-34.
- OQ-88: the unit of the time of a frame. Blocks PR-34.
- OQ-89: pixel snap in Game. Blocks PR-7.
- OQ-91: the operations of a large picture on a piece. Blocks PR-55.
- OQ-90: where the studio mark shows. Blocks PR-33.
- OQ-57: the studio name. Blocks the studio mark of PR-33 (D-468).

No open question blocks this file.
