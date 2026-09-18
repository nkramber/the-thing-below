#!/usr/bin/env bash
# Runs the screen scale probe on the Mac from the installed editor (D-621).
# The editor and an export draw the same pixels, so the measurement is the same.
# The first argument sets the distance from the eye to the screen in centimeters.
set -euo pipefail

distance="${1:-70}"
here="$(cd "$(dirname "$0")" && pwd)"
godot="/Applications/Godot_mono.app/Contents/MacOS/Godot"
label="mac-27-4k"
diagonal="27"

if [ ! -x "${godot}" ]; then
  echo "the editor is absent at ${godot}" >&2
  exit 1
fi

echo "label ${label}, diagonal ${diagonal} inches, distance ${distance} cm"
"${godot}" --path "${here}" -- \
  --screen="${label}" --diagonal="${diagonal}" --distance="${distance}" \
  --report="${here}/reports"
echo "the report is in ${here}/reports"
