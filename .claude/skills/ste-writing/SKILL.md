---
name: ste-writing
description: Write and review text in ASD-STE100 Simplified Technical English. Load before you write any .md, skill, or agent file in this repo. Holds the process glossary. Its reference file holds the glossary of the project areas, one term per concept.
---

# STE writing skill

Use this skill before you write text in this repo. The owner requires ASD-STE100 for every doc, skill, and agent file (D-10). Game text that the player reads is exempt and has its own voice (D-11).

Source: ASD-STE100 Issue 8 (2021-04-30), Part 1, Writing rules. Issue 9 (2025-01) supersedes it with the same 53 rules. The full standard is free at https://www.asd-ste100.org/. This skill gives the 53 rules in short form. It does not copy the dictionary.

## Procedure

1. Write the text.
2. Check each sentence against the checklist below.
3. Correct each sentence that fails.
4. Run the checker in the commit command (D-585).
5. Read the text again as a reader who does not know the subject.

## Checklist (the rules that fail most often)

- Max 20 words in a procedural sentence. Max 25 words in a descriptive sentence (5.1, 6.3).
- One instruction per sentence (5.2).
- Instructions in the imperative: "Load the file." Not "The file should be loaded." (5.3).
- Active voice in procedures. Active voice as much as possible in descriptions (3.6).
- No "-ing" verb forms. "Sync the data" not "Syncing the data". The rules permit an "-ing" word only in a technical name (3.5).
- No helping verbs for complex tenses: "we did", not "we have been doing" (3.4).
- Tenses allowed: infinitive, imperative, simple present, simple past, past participle as adjective, future (3.2).
- No semicolons (8.1).
- No contractions (4.2).
- Max three words in a noun cluster. Write longer names in full, then use hyphens or a short name (2.1, 2.2).
- Use "the", "a", "this" before nouns (2.3).
- One term per concept. Do not use synonyms for variety (1.11, 9.4).
- Each paragraph: one topic, max six sentences (6.5, 6.6).
- Use vertical lists for complex content (4.3).
- Start a safety note with the risk word: WARNING, CAUTION (7.1).
- Notes give information, not instructions (5.5).
- American English spelling (1.14).
- No phrasal verbs: "remove" not "take out" (9.3).
- Do not use a technical name as a verb (1.7). Write "make a backup", not "backup the data".

## The 53 rules in short form

### Section 1 - Words
- 1.1 Use only approved dictionary words, technical names, and technical verbs.
- 1.2 Use approved words only as the part of speech given.
- 1.3 Use approved words only with their approved meaning.
- 1.4 Use only approved forms of verbs and adjectives.
- 1.5 You can use words that fit a technical name category.
- 1.6 Use an unapproved word only when it is a technical name or part of one.
- 1.7 Do not use technical names as verbs.
- 1.8 Use technical names that agree with approved nomenclature.
- 1.9 Select technical names that are short and easy to understand.
- 1.10 Do not use slang or jargon as technical names.
- 1.11 Do not use different technical names for the same item.
- 1.12 You can use verbs that fit a technical verb category.
- 1.13 Do not use technical verbs as nouns.
- 1.14 Use American English spelling, unless an official directive says otherwise.

### Section 2 - Noun clusters
- 2.1 Write noun clusters of max three words.
- 2.2 Write a long technical name in full, then give a short name or use hyphens.
- 2.3 Use an article or demonstrative adjective before a noun.

### Section 3 - Verbs
- 3.1 Use only verb forms given in the dictionary.
- 3.2 Make only: infinitive, imperative, simple present, simple past, past participle as adjective, future.
- 3.3 Use the past participle only as an adjective.
- 3.4 Do not use helping verbs to make complex verb structures.
- 3.5 Use the "-ing" form only as a technical name or in a technical name.
- 3.6 Use the active voice in procedures. Use it as much as possible in descriptions.
- 3.7 Use an approved verb to describe an action, not a noun.

### Section 4 - Sentences
- 4.1 Write short and clear sentences.
- 4.2 Do not omit words or use contractions to make sentences shorter.
- 4.3 Use a vertical list for complex text.
- 4.4 Use connecting words to connect sentences with related topics.

### Section 5 - Procedures
- 5.1 Max 20 words in each sentence.
- 5.2 One instruction in each sentence, unless actions occur at the same time.
- 5.3 Write instructions in the imperative.
- 5.4 Divide a descriptive statement from the command with a comma.
- 5.5 Write notes only to give information, not instructions.

### Section 6 - Descriptions
- 6.1 Give information gradually.
- 6.2 Use key words and phrases to organize the text.
- 6.3 Max 25 words in each sentence.
- 6.4 Use paragraphs to show related information.
- 6.5 Each paragraph has only one topic.
- 6.6 No paragraph has more than six sentences.

### Section 7 - Safety instructions
- 7.1 Use a word such as "WARNING" or "CAUTION" to identify the risk level.
- 7.2 Start a safety instruction with a clear command or condition.
- 7.3 Give an explanation that shows the risk or the possible result.

### Section 8 - Punctuation and word count
- 8.1 Use all standard punctuation except the semicolon.
- 8.2 Use hyphens to connect closely related words.
- 8.3 Use parentheses for references, item identifiers, step identifiers, abbreviations, and singular/plural forms.
- 8.4 In a vertical list, a colon counts as the end of a sentence.
- 8.5 Text in parentheses counts as one word.
- 8.6 Count each number, unit, abbreviation, identifier, quoted text, and title as one word.
- 8.7 A hyphenated word counts as one word.

### Section 9 - Writing practices
- 9.1 Use a different construction when a word-for-word replacement is not enough.
- 9.2 Use each approved word correctly.
- 9.3 Do not make phrasal verbs.
- 9.4 Use a consistent style for terminology and wording.

## Technical names in this project

The rules permit these as written. They are technical names (rule 1.5):

- The tentative name, with capitals: The Thing Below (D-215). The repository: the-thing-below (D-217). The dated records keep the working title terminal-rpg (D-9).
- Tools and platforms: Godot, C#, .NET, xUnit, dotnet format, JSON, Steam, Steamworks, Steam Deck, Aseprite, Makefile, GitHub Actions, gitar, Python.
- Release services: Steam Input, Steam Cloud, Auto-Cloud, Steam Playtest, Next Fest, Steam Linux Runtime, Proton, Movie Maker, Gatekeeper, SmartScreen, notarization.
- The two harnesses: Claude Code, Codex.
- Project names: Core, Game, Tools, Tests, once PR-1 creates them (D-118).
- Process terms: session handoff, decision register, questions register, PR gate, cross-provider review, review record, response file, effective head, property test, seed loop, replay, state hash, simulation version, content hash, string table, night gate, smoke session.
- Art terms: sprite, tile, tile set, atlas, palette, portrait, backdrop, grid, frame, flip, pixel font.
- The standard itself: ASD-STE100, STE.
- Code identifiers in backticks.

## Glossary

One term per concept (D-12). Add a row for each term the owner sets, with the refused synonyms. Put a term of a project area in `references/glossary.md`.

| Term | Use for | Do not use |
|---|---|---|
| owner | the person who owns the repository and answers every question | user, maintainer, Nate in prose |
| session | one harness invocation, bound to one PR (D-576) | run, conversation |
| clean session | a new top-level session that holds no work of another PR (D-576) | fresh context, new chat |
| provider | Anthropic or OpenAI, as the source of a harness | vendor, model |
| PR | a GitHub pull request | MR, change request |
| run | one play of the game from a seed | playthrough, game |
| tick | one simulation step | frame, turn, unless the design sets turn |
| seed | the integer that starts a run's random streams | random seed |
| record | the file that holds a run's seed and inputs | replay file, log |
| replay | a run driven from a record | playback |

Process terms:

| Term | Use for | Do not use |
|---|---|---|
| docs-only PR | a PR that changes only `docs/`, `README.md`, `CLAUDE.md`, `AGENTS.md`, `.claude/`, and the PR template (D-513) | documentation PR, when the text means this set |
| Documents section | the part of the PR description with one line for each required document (D-577, D-581) | documentation-impact matrix, doc checklist |
| hand-over point | the end of the work of a session on its PR: the verdict `Ready for owner merge` for the effective head, or the label (D-576, D-582) | handoff, which names the entry in `docs/session-handoff.md` |
| start set | the files that a session reads in full at the start: `CLAUDE.md` or `AGENTS.md`, the top handoff entry, and the skills of the task (D-583) | read order, when the text means these files |
| targeted read | a read of one row, one section, or one PR block by its id or its heading (D-583) | lookup, retrieval |
| context compaction | the harness step that replaces the conversation of a session with a summary (D-587) | compaction alone, which F-10 uses for the run record |
| reference file | a file in the `references/` folder of a skill, which the skill loads for one case (D-588, D-590) | appendix, sub-skill |

The terms of the game, the world, the art, the audio, the effects, the UI, the story, the tools, CI, and the release are in `references/glossary.md`. Load that file before you write or review text about these areas (D-590). The checker does not read the glossary.

## The checker

The `ste-check` command of Tools is the checker (D-10, D-101). It reads every live document of the checkout, and it takes no file list (D-608):

```
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- ste-check --root .
```

Run it in the commit command of `docs/runbooks/session-context.md`, and one time before the first push of a PR (D-585). The command prints one line per finding: the file, the line, the rule id, and what the rule saw. It exits 1 on any finding. The rules and the exemptions:

| Rule id | What the checker flags |
|---|---|
| STE 5.1 | More than 20 words in a sentence of a numbered list item, under any heading (D-604) |
| STE 6.3 | More than 25 words in any other sentence |
| STE 8.1 | A semicolon |
| STE 4.2 | A contraction: `n't`, or a pronoun with `'s`, `'re`, `'ve`, `'ll`, `'d`, or `'m`. A possessive passes |
| STE 3.6 | Passive voice: is, are, was, were, be, been, or being, then a past participle. Two adverbs can stand between them |
| STE 3.2/3.4 | A helper verb: should, would, could, might, may, shall, ought. Also has, have, or had before a participle |
| STE 3.5 | An -ing form as the first word of a sentence, or after a preposition or a helper word |
| STE 6.6 | More than six sentences in a paragraph |
| MD 1 | An HTML comment across lines. The removal of a comment then hides prose from every rule (F-11) |
| REF 1 | A citation of a `D-`, `OQ-`, `F-`, `G-`, `T-`, `L-`, `M-`, or `PR-` id that no register holds |
| REF 2 | A path of this repository in backticks that no file or folder holds |
| REF 3 | A citation of a superseded decision that names no decision which superseded it |
| HANDOFF 1 | A session number that the handoff or its archive holds two times (L-12) |
| HANDOFF 2 | A session entry out of order. The two files hold one list, newest first (D-18) |
| HANDOFF 3 | More than 10 entries in `docs/session-handoff.md` (D-18, D-607) |
| SIZE 1 | More than 16 KB in `CLAUDE.md` or `AGENTS.md` (D-583, D-611) |
| SIZE 2 | More than 5 KB in the top entry of `docs/session-handoff.md` (D-583, D-611) |
| SIZE 3 | More than 36 KB in one `.md` file of `.claude/skills/` (D-583, D-611) |

Dated records are exempt by path: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, and `docs/archive/`. A dated record is history, and a rewrite falsifies it.

The passive and participle rules are heuristics. A past participle is an irregular form from a list, or a word that ends in "ed". "is closed" is a finding, and so is "is required". Rewrite the sentence with the actor as the subject: "the build needs the SDK". "must", "can", and "will" pass, because the standard approves them.

An -ing word that is a noun or a technical name passes: nothing, during, warning, heading, finding, and a list in the tool. A hyphenated word never counts as an -ing form. To add a technical name, add it to the list in `TheThingBelow.Tools/SteCheck/EnglishWords.cs`.

## The reference check and the session number check

The command also reads each citation of a live document (D-605) and the two handoff files (D-607).

- A register defines each id. `docs/decisions.md` defines `D-`, `docs/questions.md` defines `OQ-`, and `docs/design.md` defines `F-`, `G-`, `T-`, `L-`, and `M-`. Section 8 of `docs/design.md` and the PR headings of the phase files define `PR-`.
- A path in backticks is a path of this repository in two cases. Its first part names a top-level folder, or it is a bare file name with a file type of the repository. A path that starts with another name points outside the repository, and the rule reads none of them.
- A path resolves from the root, from the folder of the document, or from the folder above it. It also resolves as the one file of the checkout that ends with the name.
- Write the exact path from the root when a bare file name matches more than one file. A second project of the same engine makes a name such as `TheThingBelow.Game/project.godot` match two files (D-637, F-72).
- A line that names a `PR-#` marks each path of that PR (G-16). A document can name a file that a later PR creates.
- Write a name that is not a path of this repository without backticks. A branch name and a refused file name each take this rule.
- The rule of a superseded decision reads every live document except `docs/decisions.md`. The Effect column of that register records each supersession (D-606).
- Front matter and fenced code blocks take no reference rule.

## The size rules of the context budget

A session reads the start set in full, so each file of that set has a byte limit (D-583, D-611). The checker gives one finding for each file above its limit, and the finding names the size and the limit.

| File | Limit |
|---|---|
| `CLAUDE.md` and `AGENTS.md` | 16 KB |
| The top entry of `docs/session-handoff.md` | 5 KB |
| Each `.md` file of `.claude/skills/` | 36 KB |

- One kilobyte is 1024 bytes. `TheThingBelow.Tools/SteCheck/SizeRules.cs` holds the three numbers, and this skill repeats them.
- The count reads the lines of the file, and each line ending counts as one byte. Thus every CI leg reads the same number.
- The handoff rule reads the top entry alone, from its `## Session` heading to the heading of the next entry. An older entry takes no limit, because a session reads the top entry alone (D-584).
- Move text to a skill or a runbook when a file comes near its limit. A targeted read costs less context than a full read of the start set.

## Markdown notes

- Tables, fenced code blocks, and front matter are exempt from every rule. Keep cell text short.
- Headings are titles. They count as one word (8.6). The checker reads no rule on a heading.
- Text in backticks, in double quotes, or in parentheses is one word (8.5, 8.6). The grammar rules do not read inside it.
- An HTML comment on one line is not prose, and the checker removes it. Keep each comment on one line. A comment across lines is a finding, because the removal of it hides prose from every rule (F-11).
- A numbered list item is a procedural step, under any heading. Rule 5.1 applies, max 20 words (D-604).
- A bullet list item is one unit. Rule 6.3 applies, max 25 words.
- The "plain-English" paragraphs in the design doc are descriptive text. Rule 6.3 applies.
