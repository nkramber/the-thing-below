using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The statuses in the snapshot of save format 5, the migration of format 4, and each state
/// that the resume refuses (PR-66, D-390, D-792, D-798, D-800, D-801, D-805).
/// </summary>
public sealed class StatusSnapshotTests
{
    private const ulong Seed = 20260921;

    private static readonly BattleTarget Marrek = new(BattleSide.Party, 0);

    private static readonly BattleTarget Grunt = new(BattleSide.Enemy, 0);

    [Fact]
    public void ASnapshotWithStatusesReadsBackToTheSameState()
    {
        // G-5, D-792: the text holds each status and each end, and the resume gives the same
        // state hash, the push rate of haste included.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one");
        Give(run, Marrek, StatusKind.Poison);
        Give(run, Marrek, StatusKind.Haste);
        Give(run, Grunt, StatusKind.Shell);
        run.Step([BattleRuns.AttackFirst(run)]);

        string line = RunSnapshotText.Write(run.Snapshot());
        Simulation resumed = Simulation.Resume(Seed, Read(line), BattleRuns.Map("group.one"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Contains("{\"status\":\"poison\"}", line, StringComparison.Ordinal);
        Assert.Contains("{\"status\":\"haste\",\"ends_at\":400}", line, StringComparison.Ordinal);
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal(7500, BattleRuns.BattleOf(resumed).Party[0].PushRate);
    }

    [Theory]
    [InlineData("{\"status\":\"poison\",\"ends_at\":5}", "holds no end")]
    [InlineData("{\"status\":\"haste\"}", "ends_at")]
    [InlineData("{\"status\":\"charm\",\"ends_at\":5}", "charm")]
    public void AStatusOfTheWrongShapeFailsTheRead(string status, string reason)
    {
        // D-390, D-797, T-2: a lasting status holds no end, a timed one holds an end, and the
        // name is one of the ten.
        string line = RunSnapshotText.Write(BattleRuns.IntoBattle(Seed, "group.one").Snapshot())
            .Replace("\"defending\":false,\"statuses\":[]", $"\"defending\":false,\"statuses\":[{status}]", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(line));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusOnADownCombatantFailsTheResume()
    {
        // D-801: a down takes every status.
        ArgumentException error = ResumeWithEnemy(
            enemy => enemy with { Health = 0, Place = CombatantPlace.Down, Statuses = [new StatusValues(StatusKind.Poison, null)] });

        Assert.Contains("'down'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusPastItsEndFailsTheResume()
    {
        // D-798: a status ends when the timeline reaches its end, so no stored end is at or
        // before the timeline.
        ArgumentException error = ResumeWithEnemy(enemy => enemy with { Statuses = [new StatusValues(StatusKind.Slow, 0)] });

        Assert.Contains("past its end", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HasteWithSlowFailsTheResume()
    {
        // D-800: each removes the other.
        ArgumentException error = ResumeWithEnemy(
            enemy => enemy with { Statuses = [new StatusValues(StatusKind.Slow, 400), new StatusValues(StatusKind.Haste, 400)] });

        Assert.Contains("haste and slow", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusThatTheEnemyRefusesFailsTheResume()
    {
        // D-805: an immune enemy never holds the status.
        BattleContent content = TestBattles.WithGrunt(Element.Fire, Affinity.Normal, [StatusKind.Sleep], exact: false);
        RunSnapshot snapshot = BattleRuns.IntoBattle(Seed, "group.one", content).Snapshot();
        RunSnapshot broken = WithEnemy(snapshot, enemy => enemy with { Statuses = [new StatusValues(StatusKind.Sleep, 300)] });

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("refuses", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(60, StatusKind.Haste, "ends with its fight")]
    [InlineData(0, StatusKind.Poison, "down character")]
    public void APartyStatusThatNoRunMakesFailsTheResume(int health, StatusKind status, string reason)
    {
        // D-390, D-801: outside a fight a character holds poison, blind, and silence alone,
        // and a down character holds none.
        RunSnapshot snapshot = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).Snapshot();
        List<CharacterValues> characters = [.. snapshot.Characters!.Characters];
        characters[0] = characters[0] with { Health = health, Statuses = [status] };
        RunSnapshot broken = snapshot with { Characters = snapshot.Characters with { Characters = characters } };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStoredSaveOfFormatFourMigratesWithNoStatus()
    {
        // D-792: format 4 holds no status, and its push rate of 10000 names no haste or slow.
        RunSnapshot snapshot = ReadFormatFour(StoredFormatFourLine());

        Assert.All(snapshot.Battle!.Combatants, combatant => Assert.Empty(combatant.Statuses));
        Assert.All(snapshot.Characters!.Characters, character => Assert.Empty(character.Statuses));
    }

    [Fact]
    public void APushRateOfFormatFourOtherThanNoneFailsTheMigration()
    {
        // D-798, T-2: a rate of haste or slow in format 4 holds no end, so no status of
        // format 5 can take it.
        string line = StoredFormatFourLine().Replace("\"push_rate\":10000", "\"push_rate\":7500", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ReadFormatFour(line));

        Assert.Contains("push_rate", error.Message, StringComparison.Ordinal);
        Assert.Contains("7500", error.Message, StringComparison.Ordinal);
    }

    private static void Give(Simulation run, BattleTarget target, StatusKind status) =>
        BattleTurns.GiveStatus(run.State, target, status, run.State.Context("test"));

    private static ArgumentException ResumeWithEnemy(Func<CombatantValues, CombatantValues> change)
    {
        RunSnapshot broken = WithEnemy(BattleRuns.IntoBattle(Seed, "group.one").Snapshot(), change);
        return Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));
    }

    private static RunSnapshot WithEnemy(RunSnapshot snapshot, Func<CombatantValues, CombatantValues> change)
    {
        List<CombatantValues> combatants = [.. snapshot.Battle!.Combatants];
        combatants[1] = change(combatants[1]);
        return snapshot with { Battle = snapshot.Battle with { Combatants = combatants } };
    }

    private static string StoredFormatFourLine()
    {
        string[] lines = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Tests/saves/format-4.json")).TrimEnd('\n').Split('\n');
        Assert.Contains("\"push_rate\":10000", lines[1], StringComparison.Ordinal);
        return lines[1];
    }

    private static RunSnapshot Read(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the save");
        return RunSnapshotText.Read(ref reader);
    }

    private static RunSnapshot ReadFormatFour(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the save");
        return RunSnapshotText.ReadFormatFour(ref reader, 20260923);
    }
}
