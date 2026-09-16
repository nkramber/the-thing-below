#!/usr/bin/env bash
# Runs the Deck test under both renderers and collects every report (D-160).
# Copy this script and DeckTest.x86_64 to the Deck, then run this script in desktop mode.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
binary="${here}/DeckTest.x86_64"
reports="${here}/reports"

if [ ! -x "${binary}" ]; then
  echo "no DeckTest.x86_64 beside this script at ${binary}" >&2
  exit 1
fi

mkdir -p "${reports}"

for method in forward_plus mobile; do
  echo "=== ${method} ==="
  "${binary}" --rendering-method "${method}" --fullscreen 2>&1 | tee "${reports}/console-${method}.txt"
done

data="${HOME}/.local/share/godot/app_userdata/DeckTest"
if [ -d "${data}" ]; then
  cp "${data}"/deck-test-*.txt "${reports}/"
fi

echo
echo "every report is in ${reports}"
