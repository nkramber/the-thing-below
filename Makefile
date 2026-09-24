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

# The frame limit of the capture session. Each capture waits 8 frames for the window, so the
# limit holds every capture of the list, and the screen-test job of CI holds the same number.
SHEET_FRAME_LIMIT := 1200

# The flag of the codex-review target that skips the check of the Gitar pass (D-946). The flag
# comes after `--` on the command line of make, so make reads it as a goal.
SKIP_GITAR_REVIEW := --skip-gitar-review
CODEX_REVIEW_FLAGS := $(filter $(SKIP_GITAR_REVIEW),$(MAKECMDGOALS))


.PHONY: verify where hooks build test lint format ste-check identity content atlas smoke sheet walk run clean codex-review screenplay evaluator-cost $(SKIP_GITAR_REVIEW)

## verify: every check that this machine can run.
verify: build test format lint ste-check identity content atlas smoke

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
# `build` target runs before it (D-614, F-65). The run builds the Tools project itself, so
# the target also runs alone after a change of a rule.
lint:
	dotnet run --project $(TOOLS_PROJECT) -- det-lint --root .

## ste-check: the STE checker, the reference check, and the session number check (D-10, D-605).
#
# The command reads every live document that git tracks (D-702). The four dated records stay out
# of the writing rules, and the command holds their paths itself (D-10, D-608). The run builds
# the Tools project itself, as the pre-commit hook does, so the target runs alone too.
ste-check:
	dotnet run --project $(TOOLS_PROJECT) -- ste-check --root .

## identity: compare each state hash with the identity file (G-5, D-504).
#
# This machine is the fourth leg beside the three CI legs. The command reads the build
# output of the Tools project, so the `build` target runs before it.
identity:
	dotnet run --project $(TOOLS_PROJECT) --no-build -- replay-identity --root .

## content: load every content file and compare the content hash (G-5, D-495, D-648).
#
# The command reads the build output of the Tools project, so the `build` target runs before
# it. A change of a rule file needs `content-hash --root . --write` and a review of the new
# value.
content:
	dotnet run --project $(TOOLS_PROJECT) --no-build -- content-hash --root .

## atlas: compare the committed atlas with the drawing files by pixel (D-666, G-24).
#
# The command reads the build output of the Tools project, so the `build` target runs before
# it. A change of a drawing file needs `atlas --root .` and a review of the new pages.
atlas:
	dotnet run --project $(TOOLS_PROJECT) --no-build -- atlas --root . --check

## smoke: build the Godot solution, then run the headless session (D-117).
#
# Each command writes its log to a file, and never through a pipe. A pipe gives the exit
# code of the last command of the pipe, and it hides the code of the Godot process (F-60).
#
# The session runs with `--quit-after`, because a session whose managed assembly does not
# load never reaches `Quit` and runs without end (F-64). The session then gives an exit
# code of 0 with no success line, so this target reads the log too (T-2).
#
# The target reads the exit code and the log, and a fault in either one fails the target. A
# target that drops the exit code passes a session that wrote the success line and then
# failed. The log checks must run first, so the target keeps the code and reads it after
# them (D-694, T-2).
smoke:
	@set -eu; \
	mkdir -p artifacts; \
	echo "smoke: the Godot build"; \
	"$(GODOT)" --headless --editor --path $(GAME_DIR) --build-solutions --quit \
	    > artifacts/godot-build.log 2>&1 \
	  || { echo "smoke: the Godot build failed. Read artifacts/godot-build.log (T-2)." >&2; \
	       tail -5 artifacts/godot-build.log >&2; exit 1; }; \
	if grep -q "build callback failed" artifacts/godot-build.log; then \
	    echo "smoke: the Godot build callback failed. Read artifacts/godot-build.log (T-2)." >&2; \
	    exit 1; \
	fi; \
	echo "smoke: the headless session"; \
	status=0; \
	"$(GODOT)" --headless --path $(GAME_DIR) --quit-after $(SMOKE_FRAME_LIMIT) -- --smoke \
	    > artifacts/smoke.log 2>&1 || status=$$?; \
	if ! grep -q "smoke: the session ends with no error." artifacts/smoke.log; then \
	    echo "smoke: the session wrote no success line. Read artifacts/smoke.log (T-2)." >&2; \
	    tail -5 artifacts/smoke.log >&2; exit 1; \
	fi; \
	if grep -E "^(ERROR|SCRIPT ERROR|USER ERROR)" artifacts/smoke.log; then \
	    echo "smoke: the session wrote an error line (T-2)." >&2; exit 1; \
	fi; \
	if [ "$$status" != "0" ]; then \
	    echo "smoke: the session ended with the exit code $$status (T-2)." >&2; \
	    tail -5 artifacts/smoke.log >&2; exit 1; \
	fi; \
	cat artifacts/smoke.log

## sheet: the contact sheet of this machine, with the real renderer (D-172, D-735).
#
# The capture session opens a window, so this target runs alone and never inside `verify`.
# The picture of this machine never matches the baseline of CI, which draws on the software
# Vulkan driver of Linux. Thus the target makes the sheet and never compares (D-731, D-733).
#
# The sheet lands under `artifacts/`, which git ignores. `gh pr edit --attach` puts it in a
# PR description (D-514, D-735).
#
# The target builds the Godot solution first, so the captures never show an old build (D-782).
# `FIXTURE=<name>` takes the captures of one fixture alone, such as `make sheet FIXTURE=walk`.
# A screen below 1080 rows cannot hold the larger captures of the map fixture (D-782).
sheet:
	@set -eu; \
	mkdir -p artifacts; \
	rm -rf artifacts/captures; \
	echo "sheet: the Godot build"; \
	"$(GODOT)" --headless --editor --path $(GAME_DIR) --build-solutions --quit \
	    > artifacts/godot-build.log 2>&1 \
	  || { echo "sheet: the Godot build failed. Read artifacts/godot-build.log (T-2)." >&2; \
	       tail -5 artifacts/godot-build.log >&2; exit 1; }; \
	if grep -q "build callback failed" artifacts/godot-build.log; then \
	    echo "sheet: the Godot build callback failed. Read artifacts/godot-build.log (T-2)." >&2; \
	    exit 1; \
	fi; \
	echo "sheet: the capture session"; \
	status=0; \
	"$(GODOT)" --path $(GAME_DIR) --quit-after $(SHEET_FRAME_LIMIT) \
	    -- --capture "$(CURDIR)/artifacts/captures" $(if $(FIXTURE),--fixture $(FIXTURE)) > artifacts/capture.log 2>&1 || status=$$?; \
	if ! grep -q "capture: the session wrote every frame." artifacts/capture.log; then \
	    echo "sheet: the session wrote no success line. Read artifacts/capture.log (T-2)." >&2; \
	    tail -5 artifacts/capture.log >&2; exit 1; \
	fi; \
	if grep -E "^(ERROR|SCRIPT ERROR|USER ERROR)" artifacts/capture.log; then \
	    echo "sheet: the session wrote an error line (T-2)." >&2; exit 1; \
	fi; \
	if [ "$$status" != "0" ]; then \
	    echo "sheet: the session ended with the exit code $$status (T-2)." >&2; exit 1; \
	fi; \
	dotnet run --project $(TOOLS_PROJECT) -- \
	    screens --captures artifacts/captures --sheet artifacts/contact-sheet.png

## walk: the frames of one step north and one step south, after each tick (D-782).
#
# A session reads these frames before it hands a change of the map screen to the owner. Each
# frame is one PNG under `artifacts/captures/`, and the sheet joins them.
walk:
	@$(MAKE) --no-print-directory sheet FIXTURE=walk

## run: the play session of this machine (D-3).
run:
	"$(GODOT)" --path $(GAME_DIR)

## codex-review: the cross-provider review of one PR through the Codex CLI (D-926 to D-929).
#
# `PR=<n>` names the GitHub number of the PR. The command of Tools installs the newest CLI, probes
# the model, refuses a run on a closed PR, a checkout that differs from origin, a working tree
# with changes, or a Gitar pass that is not complete. It then runs the review in a separate
# worktree, and it reads the verdict from the record on origin.
#
# The command gives 0 for an approval, 2 for `Changes required` or `Blocked`, 3 for the
# three-strike stop, and 1 for a fault or a refusal. Make gives 2 for each code other than 0, so
# read the last line of the output: `codex-review: outcome <name> (exit <code>)`.
#
# `make codex-review PR=<n> -- --skip-gitar-review` skips the check of the Gitar pass (D-946).
# Make reads each word after `--` as a goal and not as an option of make. Thus the flag reaches
# this file as a goal, and the target passes it to the command of Tools. The goal of the flag
# does nothing, and it fails without the `codex-review` goal. The flag is permanent.
codex-review:
	@test -n "$(PR)" || { echo "codex-review: set PR=<number>, such as make codex-review PR=63 (T-2)." >&2; exit 1; }
	@test -z "$(filter-out codex-review $(SKIP_GITAR_REVIEW),$(MAKECMDGOALS))" || { echo "codex-review: the goal '$(filter-out codex-review $(SKIP_GITAR_REVIEW),$(MAKECMDGOALS))' is unknown. The one flag is $(SKIP_GITAR_REVIEW) (T-2)." >&2; exit 1; }
	dotnet run --project $(TOOLS_PROJECT) -- codex-review --root . --pull-request $(PR) $(CODEX_REVIEW_FLAGS)

$(SKIP_GITAR_REVIEW):
	@test -n "$(filter codex-review,$(MAKECMDGOALS))" || { echo "codex-review: $(SKIP_GITAR_REVIEW) needs the codex-review goal, such as make codex-review PR=63 -- $(SKIP_GITAR_REVIEW) (T-2)." >&2; exit 1; }

## screenplay: write the screenplay of the changed story scenes into a PR body file (D-1015, D-1016).
#
# `BODY=<file>` names the body file, such as the output of `gh pr view <n> --json body -q .body`.
# `BASE=<commit>` names the base commit, and the merge base with origin/main is the default.
# The target fills artifacts/screenplay-base with the content of the base commit through
# `git archive`, then the command of Tools writes the Screenplay section of the body file.
# Send the file with `gh pr edit <n> --body-file <file>`.
screenplay:
	@test -n "$(BODY)" || { echo "screenplay: set BODY=<file>, such as make screenplay BODY=artifacts/pr-body.md (T-2)." >&2; exit 1; }
	@set -eu; \
	base="$(BASE)"; \
	if [ -z "$$base" ]; then base=$$(git merge-base origin/main HEAD); fi; \
	rm -rf artifacts/screenplay-base; \
	mkdir -p artifacts/screenplay-base; \
	git archive "$$base" content | tar -x -C artifacts/screenplay-base; \
	echo "screenplay: the base content of $$base"; \
	dotnet run --project $(TOOLS_PROJECT) -- screenplay --root . --base artifacts/screenplay-base --body "$(BODY)"

## evaluator-cost: time one enemy turn of the worst fight, from a Release build (D-961, F-53).
#
# Six enemies stand against three characters. The limit is 1 ms at the 95th percentile on the
# Steam Deck, and the command fails a miss. A Debug build is slower, so the target builds Release.
evaluator-cost:
	dotnet run -c Release --project $(TOOLS_PROJECT) -- evaluator-cost --root .

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
