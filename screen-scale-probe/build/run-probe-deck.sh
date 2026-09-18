#!/usr/bin/env bash
# Runs the screen scale probe on the Steam Deck in desktop mode (D-621, D-458).
# Put this script and ScreenScaleProbe.x86_64 in one folder, then run ./run-probe-deck.sh
# The first argument sets the distance from the eye to the screen in centimeters.
set -euo pipefail

distance="${1:-45}"
here="$(cd "$(dirname "$0")" && pwd)"
model="$(cat /sys/devices/virtual/dmi/id/product_name 2>/dev/null || echo unknown)"

# SteamOS names the LCD Deck Jupiter, and the OLED Deck Galileo. The two screens differ.
case "${model}" in
  Galileo) label="deck-oled"; diagonal="7.4" ;;
  Jupiter) label="deck-lcd";  diagonal="7.0" ;;
  *)       label="deck-unknown"; diagonal="7.4" ;;
esac

echo "model ${model}, label ${label}, diagonal ${diagonal} inches, distance ${distance} cm"
chmod +x "${here}/ScreenScaleProbe.x86_64"
"${here}/ScreenScaleProbe.x86_64" -- \
  --screen="${label}" --diagonal="${diagonal}" --distance="${distance}" \
  --report="${here}/reports"
echo "the report is in ${here}/reports"
