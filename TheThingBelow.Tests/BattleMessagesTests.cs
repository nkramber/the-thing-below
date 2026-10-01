using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The message line of each battle event: every line comes from the string table, and each
/// one wraps into two lines of 40 characters at most with the longest names and the cap of a
/// battle amount (D-213, D-241, D-1356, D-1357, G-7, G-20). A common name is lower case in the
/// middle of a line (D-1358). The tests read the built Game assembly, because Tests takes no
/// reference to Game (D-614).
/// </summary>
public sealed class BattleMessagesTests
{
    private const string MessagesTypeName = "TheThingBelow.Game.Ui.BattleMessages";

    /// <summary>The limit of one line of a battle message, from the `game-text-style` skill (D-241, D-1356).</summary>
    private const int MessageLimit = 40;

    /// <summary>The most lines of one battle message (D-1356).</summary>
    private const int MostLines = 2;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void EveryKindOfEventGivesALineFromTheTableOrNoneForATurnAWinOrTheSummary()
    {
        // Exit test 3 of PR-10. det-lint proves that Game shows no inline string (DL 8), and
        // this test proves that each event names an id of the table. A win shows no line, and the
        // experience, the level-ups, and each new form rise above each head instead of a line (D-835,
        // D-975, D-1027). A regain of AP shows on the bars alone (D-1211).
        foreach (BattleEvent played in EveryEvent())
        {
            object? line = LineOf(played, EnemyNamedFirst());
            if (played.Kind is BattleEventKind.Turn or BattleEventKind.Won or BattleEventKind.Experience or BattleEventKind.LevelUp or BattleEventKind.FormOpened or BattleEventKind.Regain)
            {
                Assert.Null(line);
                continue;
            }

            Assert.NotNull(line);
            ContentId id = (ContentId)line!.GetType().GetProperty("Id")!.GetValue(line)!;
            Assert.True(Content.Value.Strings.Contains(id), $"The event '{played.Kind}' names '{id.Value}', which the string table lacks (G-7).");
        }
    }

    [Fact]
    public void TheFormLineTakesOneSpaceOnEachSideOfTheDash()
    {
        // D-1209: one space on each side of the dash, and the owner also asked for one space
        // between the cost and AP: "4 AP - Fire on one foe, either row."
        StringTable strings = Content.Value.Strings;

        Assert.Equal("{ap} AP - {text}", strings.Text(ContentId.Parse("battle.form_help", "test", "form help")));
    }

    [Fact]
    public void EachLineOfEachEventFillsEachPlaceWithTheValuesOfTheCode()
    {
        // Finding P3-16 of the repository review: no test tied the places of a line to the values
        // that the code gives. Each line goes through the fill of the text helper, which refuses a
        // place with no value and a value with no place (T-2).
        MethodInfo fill = GameAssemblyFile.Type("TheThingBelow.Game.Ui.TextHelper").GetMethod("Fill")!;
        StringTable strings = Content.Value.Strings;
        foreach (BattleEvent played in EveryEvent())
        {
            if (LineOf(played, EnemyNamedFirst()) is not object line)
            {
                continue;
            }

            ContentId id = (ContentId)line.GetType().GetProperty("Id")!.GetValue(line)!;
            var values = (IReadOnlyDictionary<string, string>)line.GetType().GetProperty("Values")!.GetValue(line)!;
            try
            {
                Assert.NotEmpty((string)fill.Invoke(null, [strings.Text(id), id, values])!);
            }
            catch (TargetInvocationException thrown) when (thrown.InnerException is ContentException fault)
            {
                Assert.Fail($"The event '{played.Kind}' gives the line '{id.Value}' the values [{string.Join(", ", values.Keys)}]: {fault.Message}");
            }
        }
    }

    [Fact]
    public void EveryLineWrapsIntoTwoLinesOfFortyWithTheLongestNamesAndTheCap()
    {
        // D-1356: a battle message wraps at a space into two lines of 40 characters at most. The
        // test fills each place with the longest name of its kind, the longest status, and the
        // cap of a battle amount (D-1357). A piece of gear never enters a battle line, so its name
        // takes no place (D-1046).
        StringTable strings = Content.Value.Strings;
        BattleContent battle = Content.Value.Battle;
        string longestName = LongestNameOf(strings, NamedInFights(battle));

        // D-1194: a combatant place holds a character, or an enemy with its letter, and the last
        // letter is the widest case. A form takes no letter.
        string letters = strings.Text(ContentId.Parse("battle.enemy_letters", "test", "letters"));
        string longestEnemy = strings.Text(ContentId.Parse("battle.lettered_name", "test", "name"))
            .Replace("{name}", LongestNameOf(strings, EnemiesOf(battle)), StringComparison.Ordinal)
            .Replace("{letter}", letters[^1].ToString(), StringComparison.Ordinal);
        string longestCharacter = LongestNameOf(strings, CharactersOf(battle));
        string longestCombatant = longestEnemy.Length >= longestCharacter.Length ? longestEnemy : longestCharacter;
        string longestItem = LongestNameOf(strings, battle.Items.Ids);
        string longestStatus = Longest(strings, "status.");
        string amount = BattleRules.MostAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);

        foreach (BattleEvent played in EveryEvent())
        {
            if (LineOf(played, EnemyNamedFirst()) is not object line)
            {
                continue;
            }

            ContentId id = (ContentId)line.GetType().GetProperty("Id")!.GetValue(line)!;
            string text = strings.Text(id)
                .Replace("{actor}", longestCombatant, StringComparison.Ordinal)
                .Replace("{target}", longestCombatant, StringComparison.Ordinal)
                .Replace("{status}", longestStatus, StringComparison.Ordinal)
                .Replace("{form}", longestName, StringComparison.Ordinal)
                .Replace("{item}", longestItem, StringComparison.Ordinal)
                .Replace("{amount}", amount, StringComparison.Ordinal);
            IReadOnlyList<string> lines = WrapOf(text);
            Assert.True(lines.Count <= MostLines, $"The line '{id.Value}' reads '{text}', {lines.Count} lines (D-1356).");
            foreach (string part in lines)
            {
                Assert.True(part.Length <= MessageLimit, $"The line '{id.Value}' reads '{part}', {part.Length} characters, above {MessageLimit} (D-1356).");
            }
        }
    }

    [Fact]
    public void AMessageWrapsAtTheLastSpaceThatKeepsFortyAndNeverSplitsAWord()
    {
        // D-1356: the first line takes as many whole words as 40 characters hold.
        string forty = new('a', 40);
        string words = "Starved boar B absorbs the hit. Regains 9999.";

        Assert.Equal(["Starved boar B absorbs the hit. Regains", "9999."], WrapOf(words));
        Assert.Equal([forty], WrapOf(forty));
        Assert.Equal([forty, "b"], WrapOf($"{forty} b"));
        Assert.Equal(["Marrek braces."], WrapOf("Marrek braces."));
    }

    [Fact]
    public void AMessageOfThreeLinesOrAWordLongerThanALineIsAnError()
    {
        // D-1356, T-2: the box holds two lines, and a word never splits.
        string forty = new('a', 40);

        TargetInvocationException three = Assert.Throws<TargetInvocationException>(() => Method("Wrap").Invoke(null, [$"{forty} {forty} {forty}"]));
        TargetInvocationException word = Assert.Throws<TargetInvocationException>(() => Method("Wrap").Invoke(null, [new string('a', 41)]));

        Assert.Contains("3 lines", three.InnerException!.Message, StringComparison.Ordinal);
        Assert.Contains("41 characters", word.InnerException!.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACommonNameIsLowerCaseInTheMiddleOfALineAndTakesACapitalAtTheStart()
    {
        // D-1358: "Marrek hits grunt B." A common name at the start of a line takes a capital.
        var marrek = new BattleTarget(BattleSide.Party, 0);
        var second = new BattleTarget(BattleSide.Enemy, 1);
        object view = EnemyNamedFirst();

        Assert.Equal("Marrek covers grunt B.", TextOf(new BattleEvent(BattleEventKind.Cover, marrek, second, 0), view));
        Assert.Equal("Grunt B takes 16.", TextOf(new BattleEvent(BattleEventKind.Hit, marrek, second, 16), view));
        Assert.Equal("Grunt B covers Marrek.", TextOf(new BattleEvent(BattleEventKind.Cover, second, marrek, 0), view));
    }

    [Fact]
    public void AProperNameKeepsItsCapitalInTheMiddleOfALine()
    {
        // D-1358: an enemy file names a proper name, such as the name of a boss, and the name
        // keeps its capital. The test content marks the grunt proper.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, "rules/enemies/fixture-grunt.json") == 0);
        string text = System.Text.Encoding.UTF8.GetString(files[index].Bytes).Replace("\"proper\": false", "\"proper\": true", StringComparison.Ordinal);
        files[index] = new ContentFile(files[index].Path, System.Text.Encoding.UTF8.GetBytes(text));
        ContentSet proper = ContentSet.Load(files);
        object view = ViewOf("group.fixture_pair", proper);

        Assert.True(proper.Battle.Enemy(ContentId.Parse("enemy.fixture_grunt", "test", "enemy")).Proper);
        Assert.Equal("Marrek covers Grunt B.", TextOf(new BattleEvent(BattleEventKind.Cover, new BattleTarget(BattleSide.Party, 0), new BattleTarget(BattleSide.Enemy, 1), 0), view, proper));
    }

    [Fact]
    public void EveryCombatantAndItemOfTheContentHasAName()
    {
        // A name reads `name.` and the name part of the content id.
        BattleContent battle = Content.Value.Battle;
        var things = new List<ContentId>();
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            things.Add(character.Id);
        }

        foreach (EnemyRecord enemy in battle.Enemies)
        {
            things.Add(enemy.Id);
        }

        foreach (ItemRecord item in battle.Items.Records)
        {
            things.Add(item.Id);
        }

        foreach (ContentId thing in things)
        {
            ContentId name = (ContentId)Method("NameIdOf").Invoke(null, [thing])!;
            Assert.True(Content.Value.Strings.Contains(name), $"The string table holds no name '{name.Value}' for '{thing.Value}' (G-7).");
        }
    }

    [Fact]
    public void EveryStatusHasANameAndALineOfItsOwn()
    {
        foreach (StatusKind status in Statuses.All)
        {
            ContentId name = (ContentId)Method("StatusIdOf").Invoke(null, [status])!;
            ContentId on = (ContentId)Method("StatusOnIdOf").Invoke(null, [status])!;
            Assert.True(Content.Value.Strings.Contains(name), $"The string table holds no '{name.Value}' (G-7).");
            Assert.True(Content.Value.Strings.Contains(on), $"The string table holds no '{on.Value}' (G-20).");
        }
    }

    [Fact]
    public void AStepLineReadsTheRowThatTheActorReached()
    {
        // D-380, D-836: a step to the back row backs up, and a step to the front row steps
        // forward.
        Assert.Equal("battle.step_back", ((ContentId)Method("StepIdOf").Invoke(null, [BattleRow.Back])!).Value);
        Assert.Equal("battle.step_front", ((ContentId)Method("StepIdOf").Invoke(null, [BattleRow.Front])!).Value);
    }

    [Fact]
    public void AHitLineNamesAWeakSpotAndAResist()
    {
        // D-794: the affinity of the target names the rate of the hit.
        Assert.Equal("battle.hit_weak", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Weak])!).Value);
        Assert.Equal("battle.hit_resist", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Resist])!).Value);
        Assert.Equal("battle.hit", ((ContentId)Method("HitIdOf").Invoke(null, [Affinity.Normal])!).Value);
    }

    [Fact]
    public void AnAbsorbAtFullHealthShowsNoNumberAndAnyOtherAbsorbShowsItsHeal()
    {
        // P3-34 (D-1055, D-1106): a heal of an absorb is 1 at least, so an amount of 0 means a
        // target at full health, and its line holds no number.
        var party = new BattleTarget(BattleSide.Party, 0);
        var enemy = new BattleTarget(BattleSide.Enemy, 0);

        object full = LineOf(new BattleEvent(BattleEventKind.Absorb, party, enemy, 0, null, Affinity.Absorb), EnemyNamedFirst())!;
        object healed = LineOf(new BattleEvent(BattleEventKind.Absorb, party, enemy, 1, null, Affinity.Absorb), EnemyNamedFirst())!;

        Assert.Equal("battle.absorb_full", ((ContentId)full.GetType().GetProperty("Id")!.GetValue(full)!).Value);
        Assert.Equal(["target"], ((IReadOnlyDictionary<string, string>)full.GetType().GetProperty("Values")!.GetValue(full)!).Keys);
        Assert.Equal("battle.absorb", ((ContentId)healed.GetType().GetProperty("Id")!.GetValue(healed)!).Value);
        Assert.Equal("1", ((IReadOnlyDictionary<string, string>)healed.GetType().GetProperty("Values")!.GetValue(healed)!)["amount"]);
    }

    private static readonly ContentId Form = ContentId.Parse("ability.fixture_blaze", "test", "ability");

    /// <summary>Gives one event of each kind, each affinity of a hit, each status, and each side of a fall.</summary>
    private static List<BattleEvent> EveryEvent()
    {
        var party = new BattleTarget(BattleSide.Party, 0);
        var enemy = new BattleTarget(BattleSide.Enemy, 0);
        var events = new List<BattleEvent>();
        foreach (BattleEventKind kind in Enum.GetValues<BattleEventKind>())
        {
            // A lesson event and a form event name the form of a checkout lesson (D-1027).
            events.Add(new BattleEvent(kind, party, enemy, 12, StatusKind.Poison, Affinity.Normal, Form));
            events.Add(new BattleEvent(kind, enemy, party, 12, StatusKind.Poison, Affinity.Normal, Form));
        }

        foreach (Affinity affinity in Elements.Affinities)
        {
            events.Add(new BattleEvent(BattleEventKind.Hit, party, enemy, 12, null, affinity));
        }

        // An absorb at full health restores 0 and shows its own line (D-1106).
        events.Add(new BattleEvent(BattleEventKind.Absorb, party, enemy, 0, null, Affinity.Absorb));
        events.Add(new BattleEvent(BattleEventKind.Absorb, enemy, party, 0, null, Affinity.Absorb));

        foreach (StatusKind status in Statuses.All)
        {
            foreach (BattleEventKind kind in new[] { BattleEventKind.StatusOn, BattleEventKind.StatusOff, BattleEventKind.Immune, BattleEventKind.StatusHurt })
            {
                events.Add(new BattleEvent(kind, enemy, null, 12, status));
            }
        }

        return events;
    }

    /// <summary>Gives a view of a fight of the fixture content, with one character and the pair.</summary>
    [Fact]
    public void TwoEnemiesOfOneKindTakeALetterInSlotOrderAndACharacterTakesNone()
    {
        // D-1194: the pair holds two grunts, so each takes a letter, and the name of Marrek stays plain.
        object view = EnemyNamedFirst();

        Assert.Equal("Grunt A", NameOf(view, new BattleTarget(BattleSide.Enemy, 0)));
        Assert.Equal("Grunt B", NameOf(view, new BattleTarget(BattleSide.Enemy, 1)));
        Assert.Equal("Marrek", NameOf(view, new BattleTarget(BattleSide.Party, 0)));
    }

    [Fact]
    public void ALoneEnemyOfItsKindKeepsItsPlainName()
    {
        // D-1194: the elite group holds one brute and one waiting grunt, so neither takes a letter.
        object view = ViewOf("group.fixture_elite");

        Assert.Equal("Brute", NameOf(view, new BattleTarget(BattleSide.Enemy, 0)));
        Assert.Equal("Grunt", NameOf(view, new BattleTarget(BattleSide.Enemy, 1)));
    }

    [Fact]
    public void ALineNamesTheLetteredEnemy()
    {
        // D-1194: the message of a blow names the grunt that it reached, in lower case inside the
        // line, because its name is common (D-1358).
        var played = new BattleEvent(BattleEventKind.Hit, new BattleTarget(BattleSide.Party, 0), new BattleTarget(BattleSide.Enemy, 1), 16);
        object line = LineOf(played, EnemyNamedFirst()) ?? throw new InvalidOperationException("The hit gave no line.");
        var values = (IReadOnlyDictionary<string, string>)line.GetType().GetProperty("Values")!.GetValue(line)!;

        Assert.Equal("grunt B", values["target"]);
    }

    private static object ViewOf(string group) => ViewOf(group, Content.Value);

    private static object ViewOf(string group, ContentSet content)
    {
        Simulation run = Simulation.Start(20260918, BattleRuns.Map(group), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Type viewType = GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleView");
        return viewType.GetMethod("AtStart")!.Invoke(null, [run.State, viewType.GetMethod("PartyOf")!.Invoke(null, [run.State])])!;
    }

    private static string NameOf(object view, BattleTarget target)
    {
        object pair = Method("NameLineOf").Invoke(null, [view, target, Content.Value.Strings])!;
        var id = (ContentId)pair.GetType().GetField("Item1")!.GetValue(pair)!;
        var values = (IReadOnlyDictionary<string, string>)pair.GetType().GetField("Item2")!.GetValue(pair)!;
        MethodInfo fill = GameAssemblyFile.Type("TheThingBelow.Game.Ui.TextHelper").GetMethod("Fill")!;
        return (string)fill.Invoke(null, [Content.Value.Strings.Text(id), id, values])!;
    }

    private static object EnemyNamedFirst()
    {
        Simulation run = Simulation.Start(
            20260918,
            BattleRuns.Map("group.fixture_pair"),
            Content.Value.Battle,
            Content.Value.Notices,
            Content.Value.Story,
            DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        return GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleView").GetMethod("AtStart")!.Invoke(null, [run.State, GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleView").GetMethod("PartyOf")!.Invoke(null, [run.State])])!;
    }

    private static object? LineOf(BattleEvent played, object view) =>
        Method("Of").Invoke(null, [played, view, Content.Value.Strings]);

    /// <summary>Gives the text of the line of one event as the message box shows it (D-1356, D-1358).</summary>
    private static string TextOf(BattleEvent played, object view) => TextOf(played, view, Content.Value);

    private static string TextOf(BattleEvent played, object view, ContentSet content)
    {
        object line = Method("Of").Invoke(null, [played, view, content.Strings]) ?? throw new InvalidOperationException($"The event '{played.Kind}' gave no line.");
        return string.Join(" ", (IReadOnlyList<string>)Method("LinesOf").Invoke(null, [line, content.Strings])!);
    }

    private static IReadOnlyList<string> WrapOf(string text) => (IReadOnlyList<string>)Method("Wrap").Invoke(null, [text])!;

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(MessagesTypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The battle messages hold no '{name}' method (T-2).");

    /// <summary>Gives the ids whose names a combatant place or a form place of a line reads: each character, each enemy, and each form of a lesson.</summary>
    private static List<ContentId> EnemiesOf(BattleContent battle)
    {
        var ids = new List<ContentId>();
        foreach (EnemyRecord enemy in battle.Enemies)
        {
            ids.Add(enemy.Id);
        }

        return ids;
    }

    private static List<ContentId> CharactersOf(BattleContent battle)
    {
        var ids = new List<ContentId>();
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            ids.Add(character.Id);
        }

        return ids;
    }

    private static List<ContentId> NamedInFights(BattleContent battle)
    {
        var ids = new List<ContentId>();
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            ids.Add(character.Id);
        }

        foreach (EnemyRecord enemy in battle.Enemies)
        {
            ids.Add(enemy.Id);
        }

        foreach (LessonRecord lesson in battle.Lessons.Records)
        {
            foreach (LessonForm form in lesson.Forms)
            {
                ids.Add(form.Ability);
            }
        }

        return ids;
    }

    private static string LongestNameOf(StringTable strings, IReadOnlyList<ContentId> ids)
    {
        string longest = string.Empty;
        foreach (ContentId id in ids)
        {
            string text = strings.Text(ContentId.Parse($"name.{id.Name}", StringTable.Path, id.Value));
            if (text.Length > longest.Length)
            {
                longest = text;
            }
        }

        Assert.NotEmpty(longest);
        return longest;
    }

    private static string Longest(StringTable strings, string prefix)
    {
        string longest = string.Empty;
        foreach (string id in strings.Ids)
        {
            string text = strings.Text(ContentId.Parse(id, StringTable.Path, "id"));
            if (id.StartsWith(prefix, StringComparison.Ordinal) && text.Length > longest.Length)
            {
                longest = text;
            }
        }

        Assert.NotEmpty(longest);
        return longest;
    }
}
