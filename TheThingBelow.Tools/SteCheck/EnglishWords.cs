using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The word lists and the patterns of the grammar rules. The passive rule and the participle
/// rule are heuristics, and these lists hold the exceptions that this repository needs
/// (the `ste-writing` skill).
/// </summary>
public static class EnglishWords
{
    /// <summary>The options of every pattern here. The invariant culture keeps the match stable.</summary>
    private const RegexOptions Options =
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

    /// <summary>
    /// A contraction, which rule 4.2 refuses. A possessive `'s` on a noun passes. The `'s` forms
    /// here are always a contraction, because the possessive of each one has no apostrophe:
    /// its, his, hers, and whose.
    /// </summary>
    public static readonly Regex Contraction = new Regex(
        @"\b(\w+n't|\w+'(re|ve|ll|d|m)|it's|he's|she's|who's|let's|that's|there's|what's|here's)\b",
        Options);

    /// <summary>A modal verb, which rule 3.2 refuses. "can", "must", and "will" pass.</summary>
    public static readonly Regex Modal = new Regex(
        @"\b(should|would|could|might|may|shall|ought)\b", Options);

    /// <summary>A form of "be", then up to two adverbs, then a word that can be a participle.</summary>
    public static readonly Regex Passive = new Regex(
        @"\b(is|are|was|were|be|been|being|am)\s+"
        + AdverbRun
        + @"([a-z]+)\b", Options);

    /// <summary>A form of "have", then up to two adverbs, then a word that can be a participle.</summary>
    public static readonly Regex Perfect = new Regex(
        @"\b(has|have|had)\s+"
        + AdverbRun
        + @"([a-z]+)\b", Options);

    /// <summary>A helper word or a preposition, then an "-ing" form, which rule 3.5 refuses.</summary>
    public static readonly Regex IngAfterWord = new Regex(
        @"\b(is|are|was|were|be|been|being|am|by|of|for|from|before|after|while|when|without"
        + @"|on|in|at|to|than|worth|start|starts|started|keep|keeps|kept|stop|stops|stopped"
        + @"|avoid|avoids|allow|allows|about|through|until|via)\s+(\w+ing)\b", Options);

    /// <summary>An "-ing" form as the first word of a sentence, which rule 3.5 refuses.</summary>
    public static readonly Regex IngAtStart = new Regex(@"^(\w+ing)\b", Options);

    /// <summary>Up to two adverbs between a helper verb and the word that follows it.</summary>
    private const string AdverbRun =
        @"(?:(?:not|also|then|now|never|always|still|only|often|already|both|all|each|first"
        + @"|last|later|again|usually|fully|partly|either|neither|just|rarely|soon|thus|so"
        + @"|well|hence|since|ever)\s+){0,2}";

    /// <summary>Past participles with no "ed" ending. The participle test cannot read these.</summary>
    private static readonly HashSet<string> Irregular = new HashSet<string>(StringComparer.Ordinal)
    {
        "written", "read", "built", "made", "set", "sent", "kept", "held", "run", "done",
        "given", "taken", "found", "seen", "known", "shown", "chosen", "put", "cut", "left",
        "lost", "met", "paid", "said", "told", "thought", "brought", "bought", "caught",
        "taught", "fought", "sought", "won", "begun", "sung", "drawn", "grown", "thrown",
        "broken", "spoken", "frozen", "stolen", "driven", "hidden", "ridden", "forgotten",
        "gotten", "bitten", "eaten", "fallen", "risen", "beaten", "blown", "flown", "torn",
        "worn", "born", "sworn", "understood", "withheld", "upheld", "split", "spread",
        "shut", "hit", "let", "bet", "cost", "hurt", "quit", "fed", "led", "bred", "sped",
        "lit", "slid", "struck", "stuck", "swung", "hung", "dug", "spun", "wound", "bound",
        "ground", "meant", "dealt", "felt", "dreamt", "learnt", "burnt", "leant", "spelt",
        "smelt", "spilt", "spoilt", "laid", "rebuilt", "reset", "rerun", "overwritten",
        "undone", "redone", "unset", "reread", "resent", "withdrawn", "overridden", "sold",
        "misspelt", "lent", "bent", "spent", "sat", "stood", "become", "come", "gone",
    };

    /// <summary>Past participles that name a state in this repository. Rule 3.3 makes each one an adjective.</summary>
    private static readonly HashSet<string> StateAdjective = new HashSet<string>(StringComparer.Ordinal)
    {
        "done", "gone", "over", "ready", "broken", "frozen", "retired", "merged", "pinned",
        "owned", "locked", "banned", "legal", "known", "unknown", "verified", "unverified",
        "scored", "dated", "exempt", "open", "closed", "empty", "full", "set", "green",
        "red", "left", "right", "flat", "clean", "dirty", "stale", "stuck", "wrong", "complete",
        "incomplete", "present", "absent", "missing", "unavailable", "available", "idle",
        "unattended", "identical", "silent", "aligned", "interested", "dead", "alive",
        "correct", "incorrect", "wired", "unwired", "worth", "sure", "unsure", "related",
        "unrelated", "sound", "bound", "supported", "unsupported", "legendary", "authenticated",
        "expected", "unexpected", "used", "unused", "installed", "uninstalled", "cached",
        "outdated", "limited", "unlimited", "finished", "unfinished", "logged", "welcome",
        "detailed", "advanced", "fixed", "tied", "united", "rooted", "based", "sized",
        "colored", "named", "numbered", "dated", "signed", "enabled", "disabled", "blocked",
        "defined", "undefined", "unchanged", "unread", "untouched", "unresolved",
        "resolved", "committed", "uncommitted", "tracked", "untracked", "unmerged", "unscored",
        "seeded", "unreviewed", "unpushed", "hosted", "self-hosted",
        "unapproved", "equipped", "unequipped",
    };

    /// <summary>Words that end in "ed" and are never a past participle.</summary>
    private static readonly HashSet<string> NeverParticiple = new HashSet<string>(StringComparer.Ordinal)
    {
        "need", "seed", "feed", "speed", "breed", "bleed", "proceed", "succeed", "exceed",
        "indeed", "agreed", "freed", "red", "bed", "shed", "wed", "sled", "fled", "hundred",
        "naked", "wicked", "sacred", "unlimited", "rugged", "wretched", "crooked", "jagged",
        "beloved", "biased", "coed", "med", "greed", "reed", "deed", "creed", "steed", "weed",
        "tweed", "ahead", "instead", "dead", "lead", "read", "bread", "thread", "spread",
    };

    /// <summary>Technical names and nouns that end in "ing". They are not verb forms (rule 3.5).</summary>
    private static readonly HashSet<string> IngTechnicalName = new HashSet<string>(StringComparer.Ordinal)
    {
        "thing", "nothing", "something", "anything", "everything", "during", "string",
        "ring", "king", "bring", "spring", "ping", "wing", "sing", "morning", "evening",
        "sibling", "sling", "swing", "fling", "cling", "sting", "wring", "ceiling", "sterling",
        "meaning", "warning", "setting", "settings", "ranking", "rankings", "logging",
        "caching", "streaming", "routing", "rendering", "scoring", "pricing", "scaling",
        "engineering", "tagging", "listing", "listings", "ordering", "spelling", "heading",
        "headings", "sizing", "mapping", "mappings", "binding", "bindings", "batching",
        "polling", "tuning", "linting", "tooling", "styling", "formatting", "monitoring",
        "alerting", "encoding", "encodings", "embedding", "embeddings", "chunking", "wiring",
        "wording", "timing", "timings", "sampling", "stemming", "finding", "findings",
        "grouping", "groupings", "pairing", "pairings", "nesting", "spacing", "docstring",
        "docstrings", "lightning", "healing", "leveling", "targeting", "crafting", "pathfinding",
        "scripting", "modding", "casting", "ending", "endings", "opening", "openings",
        "dungeon-crawling", "missing", "existing", "following", "remaining", "underlying",
        "leading", "trailing", "pending", "outstanding", "according", "including", "excluding",
        "regarding",
    };

    /// <summary>Reads a word as a past participle, for the passive rule and the perfect rule.</summary>
    /// <param name="word">The word that follows the helper verb.</param>
    /// <returns>True when the word can be a past participle.</returns>
    public static bool IsParticiple(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        string lower = word.ToLowerInvariant();
        if (NeverParticiple.Contains(lower))
        {
            return false;
        }

        return Irregular.Contains(lower)
            || (lower.Length > 3 && lower.EndsWith("ed", StringComparison.Ordinal));
    }

    /// <summary>Reads a word as an adjective that names a state (rule 3.3).</summary>
    /// <param name="word">The word that follows the helper verb.</param>
    /// <returns>True when the word names a state and passes the passive rule.</returns>
    public static bool IsStateAdjective(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return StateAdjective.Contains(word.ToLowerInvariant());
    }

    /// <summary>Reads an "-ing" word as a technical name or a noun (rule 3.5).</summary>
    /// <param name="word">The word that ends in "ing".</param>
    /// <returns>True when the word is a name and passes the "-ing" rule.</returns>
    public static bool IsIngTechnicalName(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return IngTechnicalName.Contains(word.ToLowerInvariant());
    }
}
