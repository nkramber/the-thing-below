#!/usr/bin/env bash
# Gets the Godot export templates and checks the SHA-512, as OQ-83 sets for CI.
# The templates let this machine export the native Linux build that D-458 measures.
set -euo pipefail

version="4.7.2-stable"
flavour="mono"
file="Godot_v${version}_${flavour}_export_templates.tpz"
base="https://github.com/godotengine/godot-builds/releases/download/${version}"
work="${TMPDIR:-/tmp}/godot-templates-${version}"
# macOS and Linux keep the templates in different folders. The Deck runs Linux.
if [ "$(uname)" = "Darwin" ]; then
  target="${HOME}/Library/Application Support/Godot/export_templates/4.7.2.stable.mono"
else
  target="${XDG_DATA_HOME:-${HOME}/.local/share}/godot/export_templates/4.7.2.stable.mono"
fi

mkdir -p "${work}"

if [ -d "${target}" ]; then
  echo "templates already installed at ${target}"
  exit 0
fi

echo "fetching ${file}"
curl --fail --location --silent --show-error --output "${work}/${file}" "${base}/${file}"
curl --fail --location --silent --show-error --output "${work}/SHA512-SUMS.txt" "${base}/SHA512-SUMS.txt"

expected="$(grep " ${file}\$" "${work}/SHA512-SUMS.txt" | awk '{print $1}')"
if [ -z "${expected}" ]; then
  echo "no SHA-512 for ${file} in SHA512-SUMS.txt" >&2
  exit 1
fi

if command -v sha512sum >/dev/null; then
  actual="$(sha512sum "${work}/${file}" | awk '{print $1}')"
else
  actual="$(shasum -a 512 "${work}/${file}" | awk '{print $1}')"
fi
if [ "${expected}" != "${actual}" ]; then
  echo "SHA-512 mismatch for ${file}" >&2
  echo "  expected ${expected}" >&2
  echo "  actual   ${actual}" >&2
  exit 1
fi
echo "SHA-512 matches"

rm -rf "${work}/unpacked"
mkdir -p "${work}/unpacked"
unzip -q "${work}/${file}" -d "${work}/unpacked"

mkdir -p "${target}"
mv "${work}/unpacked/templates/"* "${target}/"
echo "installed ${target}"
ls "${target}" | head
