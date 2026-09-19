# The Sprite Fusion test

Status: spike, started 2026-09-19. Branch: `spike/sprite-fusion`. This branch never merges (D-620). Written in ASD-STE100 (D-10).

This folder holds the test of D-620 and D-675. The test compares the pixel art of a session with the pixel art of the Sprite Fusion generator. The owner looks at one sheet with both sets, and the owner picks the source of the art. The pick takes a decision row of its own (D-620, D-676).

Item 24 of section 8 of `docs/roadmaps/phase-1-foundations.md` holds the position of the test. Section 7.17 of that file holds the scope, the exit tests, and the review focus.

## The subjects

The owner picked one anchor and three new subjects on 2026-09-19:

| Subject | Size | Page | State before the test |
|---|---|---|---|
| Marrek, the map sprite, front view | 32 by 32 | `map_sprites` | The owner approved it (D-402) |
| The floor of the deep mine | 32 by 32 | `tiles` | No art before this test |
| A winter beast, the map sprite, front view | 32 by 32 | `map_sprites` | No art before this test |
| Marrek, the portrait | 64 by 64 | `portraits` | No art before this test |

The anchor calibrates the eye of the owner. The three new subjects are the kinds of art that Phase 1 and Phase 2 still need: a tile, an enemy, and a portrait. The world file `docs/world/places.md` gives the deep mine, and D-370 gives the winter beast.

## The folders

- `session/content/sprites/`: the drawing files of the session, with a copy of the palette of 64 colors. The atlas command reads this folder as its root, so the test changes no file of `content/`.
- `sheets/`: the review sheets that the atlas command writes (D-514, D-668).
- `generated/`: the pictures of the Sprite Fusion generator.

## The commands

The atlas command writes the pages and the sheets of the test:

```
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- \
  atlas --root spike/session --sheets spike/sheets
```

## The facts of the tool

Each fact below comes from a page of the supplier, read on 2026-09-19:

- The generator has an API. A key goes in the environment variable `SPRITE_FUSION_API_KEY`, and the endpoint is `POST https://www.spritefusion.com/api/v1/generate`.
- The API holds five operations: `generate`, `edit`, `style-reference`, `direction-set`, and `animate`. The size of a still picture is 16, 32, or 64 pixels.
- Each operation costs 15 credits. The Starter plan costs 9 USD each month and gives 450 credits. The credits of a month do not carry to the next month.
- The terms say: "We do not claim ownership over Outputs You create through the Service. You may use Outputs for personal or commercial projects."
- The terms also say: "You are solely responsible for reviewing Outputs before use." A paid plan keeps each picture private.
- Sources: `https://www.spritefusion.com/pixel-art-generator`, `https://www.spritefusion.com/docs/pixel-art-generator/api`, and `https://www.spritefusion.com/terms`.

## The state of the test

- The four drawing files of the session are complete, and the sheets show them.
- The generator side waits for the API key of the owner.
- No decision row records a pick yet. A short documents PR carries the row to `main`, because this branch never merges.
