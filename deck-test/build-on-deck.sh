#!/usr/bin/env bash
# Builds the Deck test on the Deck itself: the C# solution, then the native Linux export that
# D-458 measures. The build and run-deck-test.sh land in build/, so the next step is:
#   cd build && ./run-deck-test.sh
# GODOT names the Godot .NET binary of the Deck, such as ~/Godot/Godot_v4.7.2-stable_mono_linux.x86_64.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
godot="${GODOT:?set GODOT to the path of the Godot .NET binary}"

if [ ! -x "${godot}" ]; then
  echo "GODOT is ${godot}, and no program is there" >&2
  exit 1
fi

cd "${here}"
./fetch-export-templates.sh
mkdir -p build

echo "building the C# solution"
"${godot}" --headless --editor --path . --build-solutions --quit > build/build.log 2>&1
if grep -q "build callback failed" build/build.log; then
  echo "the C# build failed. Read ${here}/build/build.log" >&2
  exit 1
fi

echo "exporting build/DeckTest.x86_64"
"${godot}" --headless --path . --export-release "Linux x86_64" build/DeckTest.x86_64 > build/export.log 2>&1

# The export exits 0 when it packs no managed assembly, so the log is the check.
if grep -q "no solution file was found" build/export.log || ! grep -q "DeckTest.dll" build/export.log; then
  echo "the export packed no managed assembly. Read ${here}/build/export.log" >&2
  exit 1
fi

cp run-deck-test.sh build/
echo "ready: cd ${here}/build && ./run-deck-test.sh"
