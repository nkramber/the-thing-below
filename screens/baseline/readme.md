# The baseline of the screen tests

Status: active. Written in ASD-STE100. Decisions: D-172, D-729 to D-736, D-782, and D-784.

This folder holds one committed PNG for each capture of the screen-test job (D-736). The job
compares the captures of each run with these files by decoded pixel, and one changed pixel
fails the job (D-172, F-19).

## Where a file comes from

Each file comes from the `screen-test` job of `.github/workflows/ci.yml`. That job runs
Godot under Xvfb with the Mobile renderer on lavapipe, the software Vulkan driver of the
pinned Mesa (D-729, D-730, D-731). Another machine gives another picture, so no local run
makes a file of this folder.

## How to change a file

A change of a screen changes these files. The steps (D-733):

1. Push the change, and let the `screen-test` job run.
2. Read the report of the job, which names each capture that differs.
3. Download the artifact `screen-captures` of that run.
4. Look at each new frame, and confirm that the change is the intended one.
5. Copy the new files into this folder, and commit them with the change.

A pull request that moves the Mesa pin of D-730 writes new files too, because a Mesa update
can change a pixel.

## The list of captures

`TheThingBelow.Game/scripts/ScreenCaptures.cs` holds the list, and a test locks it. The map
fixture and the ui fixture each take the frame at 1x, and both fit modes at 1080 and 1440
screen rows (D-232, D-568, D-734). The walk fixture adds one frame at 1x after each
tick of one step north and one step south, such as walk-north-09.png (D-782). The picture
fixture takes the fixture large picture at 1x (D-819).

Each file holds the sRGB colors that the screen shows. The window renders in linear HDR 2D,
and the capture session turns each pixel back into sRGB before it writes the file (D-188).

## The contact sheet

The owner reads the real renderer on the Mac, and not these files. The `screens` command of
Tools makes that sheet under `artifacts/`, and no sheet enters git (D-735).

A session reads the same frames before it hands a change of a screen to the owner (D-784).
`make walk` captures the walk fixture alone, and a screen below 1080 rows cannot hold the
larger captures of `make sheet` (D-782).
