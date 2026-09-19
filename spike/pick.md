# The pick of the Sprite Fusion test

Status: the record of the pick, written 2026-09-19. Written in ASD-STE100 (D-10).

The branch `spike/sprite-fusion` never merges (D-620). This file holds each change that a short documents PR must make on `main`. That PR carries the rows below, the changes of the documents, and the handoff entry of this session.

Each draft row is in a fenced block. A register on `main` holds no id of a draft row yet, and the reference check of D-605 reads a fenced block as text. The documents PR gives each row its number at the end of the table, and it corrects each number below if a row lands first.

## The rows for `docs/decisions.md`

```
| D-686 | 2026-09-19 | The source of the art (D-620, D-675) | The Sprite Fusion generator draws every picture of the game: each character, each enemy, each portrait, and each tile. The session repairs each tile by hand until it repeats with no grid. Every picture enters the pipeline as a text grid on the palette of 64 colors, through the PNG import of PR-51, so G-24 stands. The owner rejected the recommendation of the session, which was a mix: the tool for each character, each enemy, and each portrait, and the session for each tile. | Owner answer, 2026-09-19, from the sheets of the test on `spike/sprite-fusion` at `e3c50b4`. Answers the pick of D-620 and D-675, and it holds the tenth line of Gate 1 (D-676). Revises in part D-57 and D-107: the session no longer draws the first picture of a subject, and each other part of both rows stands. Revises in part section 7.1 of `docs/roadmaps/area-art.md`. The four findings of the test are F-86 to F-89. The evidence against the pick is F-87 and F-88: the tool holds no size, and it draws a tile as a framed block. The owner read both findings before the pick. |
| D-687 | 2026-09-19 | The Starter plan of Sprite Fusion | The Starter plan stays, at 9 USD each month for 450 credits. The session recommended this. | Owner answer, 2026-09-19. Applies G-13 and D-620. One call costs 15 credits, so the plan gives about 30 calls each month, and each call returns 9 to 12 pictures. The credits of a month do not carry to the next month. The test of 2026-09-19 spent 165 credits, and 330 remain. The terms say: "We do not claim ownership over Outputs You create through the Service. You may use Outputs for personal or commercial projects." A paid plan keeps each picture private. The terms forbid the use of a picture to train or to measure a machine-learning model. The API key is at `~/.config/sprite-fusion/api-key` on the machine of the owner, and no file of the repository holds it. Sources: `https://www.spritefusion.com/pixel-art-generator`, `https://www.spritefusion.com/docs/pixel-art-generator/credits-costs`, and `https://www.spritefusion.com/terms`, read 2026-09-19. |
```

## The rows for the findings register of `docs/design.md`

```
| F-86 | The Sprite Fusion generator draws a character and a portrait better than a session. The portrait shows the largest difference of the test | 2026-09-19 | ✅ D-686: the tool draws every picture |
| F-87 | The generator does not hold the size of the call. A call that asks for 32 pixels returned 32, 33, 36, 37, 38, 39, and 42 pixels across the eight calls of the test. A picture of another size breaks the grid of 32 pixels of D-228 and D-633 | 2026-09-19 | 🔧 PR-51: the PNG import gives each picture a new frame of 32 or 64 pixels, and it fails on a picture that no frame fits (T-2) |
| F-88 | The generator draws a tile as one framed block, with a dark edge on each of the four sides. A floor of these tiles shows a grid of 32 pixels. The tile of the session repeats with no grid. `spike/sheets/tile-repeat.png` holds the evidence | 2026-09-19 | 🔧 D-686: the session repairs each tile by hand. PR-17 draws the first tile set under this rule |
| F-89 | The palette of 64 colors holds the look of a picture of the generator. A picture uses 206 to 1275 colors, and almost none of them is a color of the palette. The nearest color of the palette lies 10 to 19 RGB units away on average | 2026-09-19 | ✅ PR-51: the import maps each pixel to the nearest color of the palette (D-181) |
```

## The changes of the other documents

- `docs/roadmaps/area-art.md`, section 7.1: the first line says that sessions draw every picture. Change it to name the generator as the source, and keep the approval of each batch by the owner (D-57). Keep the line about the drawing files as the source. Add the repair of each tile by hand.
- `docs/roadmaps/phase-1-foundations.md`, section 7.17: mark the three exit tests as met, and name the new decision row as the pick.
- `docs/roadmaps/phase-1-foundations.md`, section 7.22: Gate 1, line 10 asks for the test and the row. The new row holds it.
- `docs/roadmaps/phase-2-first-playable.md`, section 7.39: PR-51 gains two parts in its scope. The first part gives each picture a new frame of 32 or 64 pixels. The second part maps each pixel to the nearest color of the palette.
- `docs/session-handoff.md`: carry the entry of Session 134 from this branch, above the entry of the documents PR.
- G-24 needs no change. The text grid stays the source of every picture.

## What the documents PR must not do

- It must not merge this branch (D-620).
- It must not copy a picture of `spike/generated/` into `content/`. No picture enters `content/` before the import of PR-51 gives it a frame and the palette (G-24).
