# The build entry of the repository (D-3). Run each target from the checkout root.
#
# The name `Godot` is not on the command path of this machine, so `GODOT` holds the full
# path. Set `GODOT` in the environment to run against another Godot build.

SOLUTION := TheThingBelow.slnx
GAME_DIR := TheThingBelow.Game
TOOLS_PROJECT := TheThingBelow.Tools/TheThingBelow.Tools.csproj
GODOT ?= /Applications/Godot_mono.app/Contents/MacOS/Godot

# The frame limit of the smoke session. It ends a session that does not reach `Quit` (F-64).
SMOKE_FRAME_LIMIT := 600


.PHONY: verify where hooks build test lint format ste-check identity smoke run clean

## verify: every check that this machine can run.
verify: build test format lint ste-check identity smoke

## build: build every project of the solution.
build:
	dotnet build $(SOLUTION)

## test: run every test outside the Smoke category (D-592).
test:
	dotnet test --solution $(SOLUTION) --no-build -- --filter-not-trait "Category=Smoke"

## format: fail when a file needs a format change.
format:
	dotnet format $(SOLUTION) --verify-no-changes

## lint: the determinism and string lint (D-496, G-2, G-3, G-7).
#
# The command reads the Godot assembly from the build output of the Game project, so the
# `build` target runs before it (D-614, F-65).
lint:
	dotnet run --project $(TOOLS_PROJECT) -- det-lint --root .

## ste-check: the STE checker, the reference check, and the session number check (D-10, D-605).
#
# The command reads every live document of the checkout. The four dated records stay out of
# the writing rules, and the command holds their paths itself (D-10, D-608).
ste-check:
	dotnet run --project $(TOOLS_PROJECT) -- ste-check --root .

## identity: compare each state hash with the identity file (G-5, D-504).
#
# This machine is the fourth leg beside the three CI legs. The command reads the build
# output of the Tools project, so the `build` target runs before it.
identity:
	dotnet run --project $(TOOLS_PROJECT) --no-build -- replay-identity --root .

## smoke: build the Godot solution, then run the headless session (D-117).
#
# Each command writes its log to a file, and never through a pipe. A pipe gives the exit
# code of the last command of the pipe, and it hides the code of the Godot process (F-60).
#
# The session runs with `--quit-after`, because a session whose managed assembly does not
# load never reaches `Quit` and runs without end (F-64). The session then gives an exit
# code of 0 with no success line, so this target reads the log and not the code (T-2).
smoke:
	@set -eu; \
	mkdir -p artifacts; \
	echo "smoke: the Godot build"; \
	"$(GODOT)" --headless --editor --path $(GAME_DIR) --build-solutions --quit \
	    > artifacts/godot-build.log 2>&1 \
	  || { echo "smoke: the Godot build failed. Read artifacts/godot-build.log (T-2)." >&2; \
	       tail -5 artifacts/godot-build.log >&2; exit 1; }; \
	echo "smoke: the headless session"; \
	"$(GODOT)" --headless --path $(GAME_DIR) --quit-after $(SMOKE_FRAME_LIMIT) -- --smoke \
	    > artifacts/smoke.log 2>&1 || true; \
	if ! grep -q "smoke: the session ends with no error." artifacts/smoke.log; then \
	    echo "smoke: the session wrote no success line. Read artifacts/smoke.log (T-2)." >&2; \
	    tail -5 artifacts/smoke.log >&2; exit 1; \
	fi; \
	if grep -E "^(ERROR|SCRIPT ERROR|USER ERROR)" artifacts/smoke.log; then \
	    echo "smoke: the session wrote an error line (T-2)." >&2; exit 1; \
	fi; \
	cat artifacts/smoke.log

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
