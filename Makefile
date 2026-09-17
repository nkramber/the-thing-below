# The build entry of the repository (D-3). Run each target from the checkout root.
#
# The name `Godot` is not on the command path of this machine, so `GODOT` holds the full
# path. Set `GODOT` in the environment to run against another Godot build.

SOLUTION := TheThingBelow.slnx
GAME_DIR := TheThingBelow.Game
TOOLS_PROJECT := TheThingBelow.Tools/TheThingBelow.Tools.csproj
GODOT ?= /Applications/Godot_mono.app/Contents/MacOS/Godot

# The documents that the STE checker reads. Four paths are dated records and stay out (D-10).
STE_EXCLUDE := -e '^docs/reviews/' -e '^docs/session-handoff' -e '^docs/archive/'
STE_FILES = $(shell git ls-files '*.md' | grep -v $(STE_EXCLUDE))

.PHONY: verify where hooks build test lint format ste-check smoke run clean

## verify: every check that this machine can run.
verify: build test format lint ste-check smoke

## build: build every project of the solution.
build:
	dotnet build $(SOLUTION)

## test: run every test outside the Smoke category (D-592).
test:
	dotnet test --solution $(SOLUTION) --no-build -- --filter-not-trait "Category=Smoke"

## format: fail when a file needs a format change.
format:
	dotnet format $(SOLUTION) --verify-no-changes

## lint: the determinism and string lint. PR-46 creates the command (D-496, G-16).
lint:
	@echo "lint: the det-lint command does not exist yet. PR-46 creates it (G-16)."

## ste-check: the interim STE checker. PR-2 replaces it with the C# command (D-10).
ste-check:
	python3 docs/tools/ste-check.py $(STE_FILES)

## smoke: build the Godot solution, then run the headless session (D-117).
smoke:
	"$(GODOT)" --headless --editor --path $(GAME_DIR) --build-solutions --quit
	"$(GODOT)" --headless --path $(GAME_DIR) -- --smoke

## where: the branch, the tree, and the PR state.
where:
	git status --short --branch
	gh pr status

## hooks: install the pre-commit hook in this checkout (D-8, D-25).
hooks:
	git config core.hooksPath .githooks
	@echo "hooks: the hook path is .githooks."

## clean: remove the build output of every project.
clean:
	dotnet clean $(SOLUTION)
	rm -rf $(GAME_DIR)/.godot
