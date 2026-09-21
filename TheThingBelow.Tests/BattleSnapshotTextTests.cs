using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of the party and the battle in a snapshot (save format 4), and the target and the
/// item of an intent in a record (record format 2). D-531, D-764, D-765, D-780.
/// </summary>
public sealed class BattleSnapshotTextTests
{
    private const ulong Seed = 20260922;

    [Fact]
    public void ASnapshotInsideABattleReadsBackToTheSameState()
    {
        // D-531: one snapshot covers the map and the battle.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.fixture_elite", TestBattles.WithParty(2));
        run.Step([BattleRuns.AttackFirst(run)]);
        RunSnapshot snapshot = run.Snapshot();

        string line = RunSnapshotText.Write(snapshot);
        RunSnapshot read = Read(line);
        Simulation resumed = Simulation.Resume(Seed, read, BattleRuns.Map("group.fixture_elite"), TestBattles.WithParty(2), DebugIntentHandlers.None);

        Assert.Equal(line, RunSnapshotText.Write(read));
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ASnapshotOfSaveFormatFourWithNoPartyFails()
    {
        string line = WithoutObject(RunSnapshotText.Write(BattleRuns.IntoBattle(Seed, "group.one").Snapshot()), "party", "battle");

        ContentException error = Assert.Throws<ContentException>(() => Read(line));

        Assert.Contains("party", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfSaveFormatThreeWithAPartyFails()
    {
        // D-765: format 3 predates the party, so a party in it names a fault of the file.
        string line = RunSnapshotText.Write(BattleRuns.IntoBattle(Seed, "group.one").Snapshot());

        ContentException error = Assert.Throws<ContentException>(() => ReadFormatThree(line));

        Assert.Contains("predates", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownPlaceOfACombatantFails()
    {
        string line = RunSnapshotText.Write(BattleRuns.IntoBattle(Seed, "group.one").Snapshot())
            .Replace("\"place\":\"field\"", "\"place\":\"sky\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(line));

        Assert.Contains("field, waiting, down", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABattleWithNoEncounterOfItsMapFailsTheResume()
    {
        // D-531, T-2: a battle and its encounter name one enemy and one group.
        RunSnapshot snapshot = BattleRuns.IntoBattle(Seed, "group.one").Snapshot();
        RunSnapshot broken = snapshot with { Battle = snapshot.Battle! with { Group = ContentId.Parse("group.other", "test", "group") } };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None));

        Assert.Contains("group.other", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACombatantHealthOutsideItsRangeFailsTheResume()
    {
        RunSnapshot snapshot = BattleRuns.IntoBattle(Seed, "group.one").Snapshot();
        List<CombatantValues> combatants = [.. snapshot.Battle!.Combatants];
        combatants[1] = combatants[1] with { Health = 31 };
        RunSnapshot broken = snapshot with { Battle = snapshot.Battle with { Combatants = combatants } };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None));

        Assert.Contains("0 to 30", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordHoldsTheTargetAndTheItemOfAnIntent()
    {
        // D-764, D-780: the record reads the use of one item on one character.
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild("0123456789abcdef", Seed), run.Snapshot());
        Intent use = Intent.OfPlayer(IntentIds.BattleItem, new BattleTarget(BattleSide.Party, 0), ContentId.Parse("item.fixture_draught", "test", "item"));
        recorder.Step(1, [Intent.OfPlayer(IntentIds.MoveEast)]);
        recorder.Step(2, [use]);

        string text = RunRecordText.Write(recorder.Build());
        RunRecord read = RunRecordText.Read(text);

        Assert.Contains("\"item\":\"item.fixture_draught\",\"target\":{\"side\":\"party\",\"slot\":0}", text, StringComparison.Ordinal);
        Assert.Equal(use.Describe(), read.Ticks[1].Intents[0].Describe());
        Assert.Equal(use.Target, read.Ticks[1].Intents[0].Target);
        Assert.Equal("intent.battle_item with item.fixture_draught at party 0", use.Describe());
    }

    [Fact]
    public void ARecordWithAnUnknownSideOfATargetFailsWithTheLine()
    {
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild("0123456789abcdef", Seed), run.Snapshot());
        recorder.Step(1, [Intent.OfPlayer(IntentIds.BattleAttack, new BattleTarget(BattleSide.Enemy, 0), null)]);
        string text = RunRecordText.Write(recorder.Build()).Replace("\"side\":\"enemy\"", "\"side\":\"moon\"", StringComparison.Ordinal);

        RunRecordException error = Assert.Throws<RunRecordException>(() => RunRecordText.Read(text));

        Assert.Equal(3, error.Line);
        Assert.Contains("party, enemy", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordOfFormatOneFailsWithItsLine()
    {
        // D-764: a record replays on its own simulation version alone, so no reader of format 1 exists.
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild("0123456789abcdef", Seed), run.Snapshot());
        string text = RunRecordText.Write(recorder.Build()).Replace("{\"format\":2,", "{\"format\":1,", StringComparison.Ordinal);

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunReplay.Play(RunRecordText.Read(text), "0123456789abcdef", BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None));

        Assert.Equal(1, error.Line);
        Assert.Contains("format version 1", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false, false, null)]
    [InlineData(true, false, "slot")]
    [InlineData(false, true, "autosave")]
    [InlineData(true, true, "autosave")]
    public void AWipeReloadsTheNewerSave(bool slot, bool autosave, string? picked)
    {
        // D-231, D-776: the later tick of the run wins, and no save means a new start.
        SaveDocument slotSave = SaveOf(100);
        SaveDocument autoSave = SaveOf(250);

        SaveDocument? chosen = SavePick.NewerOf(slot ? slotSave : null, autosave ? autoSave : null);

        SaveDocument? wanted = picked switch
        {
            "slot" => slotSave,
            "autosave" => autoSave,
            _ => null,
        };
        Assert.Same(wanted, chosen);
    }

    [Fact]
    public void TwoSavesOfOneTickReloadTheSlotSave()
    {
        SaveDocument slot = SaveOf(100);

        Assert.Same(slot, SavePick.NewerOf(slot, SaveOf(100)));
    }

    private static SaveDocument SaveOf(long tick)
    {
        RunSnapshot snapshot = Simulation.Start(Seed, BattleRuns.Map("group.one"), TestBattles.Content, DebugIntentHandlers.None).Snapshot();
        return new SaveDocument(SaveHeader.ForThisBuild("0123456789abcdef", Seed), snapshot with { Tick = tick, WorldTick = 0 });
    }

    private static RunSnapshot Read(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the save");
        RunSnapshot snapshot = RunSnapshotText.Read(ref reader);
        reader.ReadFileEnd();
        return snapshot;
    }

    private static RunSnapshot ReadFormatThree(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the save");
        return RunSnapshotText.ReadFormatThree(ref reader);
    }

    /// <summary>Removes the named objects of the top level of one snapshot line.</summary>
    private static string WithoutObject(string line, string first, string second)
    {
        int start = line.IndexOf($",\"{first}\":", StringComparison.Ordinal);
        int end = line.IndexOf(",\"streams\":", StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, $"The line holds no '{first}' and '{second}' before the streams.");
        return string.Concat(line.AsSpan(0, start), line.AsSpan(end));
    }
}
