# The baseline of the screen tests

Status: active. Written in ASD-STE100. Decisions: D-172, D-729 to D-736.

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

`TheThingBelow.Game/scripts/ScreenCaptures.cs` holds the list, and a test locks it. The list
holds the frame at 1x and both fit modes at 1080 and 1440 screen rows, for each fixture
(D-232, D-568, D-734).

## The contact sheet

The owner reads the real renderer on the Mac, and not these files. The `screens` command of
Tools makes that sheet under `artifacts/`, and no sheet enters git (D-735).
