using System;
using System.Collections.Generic;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The record of every tile that the party walked (D-567). The dungeon map screen of PR-62
/// draws those tiles, and the snapshot carries the record.
/// </summary>
public sealed class WalkedTilesTests
{
    /// <summary>The stream number of the generator of this test, apart from every run.</summary>
    private const ulong TestSequence = 0x57414c4b45443031;

    [Fact]
    public void AFreshRecordHoldsNoTile()
    {
        WalkedTiles walked = WalkedTiles.Empty(4, 3);

        Assert.Equal(0, walked.Count);
        Assert.False(walked.WasWalked(new TilePoint(1, 1)));
        Assert.Equal(["....", "....", "...."], walked.Rows());
    }

    [Fact]
    public void AMarkedTileReadsBack()
    {
        WalkedTiles walked = WalkedTiles.Empty(4, 3);

        walked.Mark(new TilePoint(2, 1));

        Assert.True(walked.WasWalked(new TilePoint(2, 1)));
        Assert.Equal(1, walked.Count);
        Assert.Equal(["....", "..x.", "...."], walked.Rows());
    }

    [Fact]
    public void ASecondMarkOfOneTileChangesNothing()
    {
        WalkedTiles walked = WalkedTiles.Empty(4, 3);

        walked.Mark(new TilePoint(2, 1));
        walked.Mark(new TilePoint(2, 1));

        Assert.Equal(1, walked.Count);
    }

    [Fact]
    public void TheRowsReadBackToTheSameRecord()
    {
        WalkedTiles walked = WalkedTiles.Empty(5, 4);
        walked.Mark(new TilePoint(0, 0));
        walked.Mark(new TilePoint(4, 3));

        WalkedTiles read = WalkedTiles.OfRows(walked.Rows(), "the test");

        Assert.Equal(walked.Rows(), read.Rows());
        Assert.Equal(walked.Count, read.Count);
        Assert.Equal(walked.Width, read.Width);
        Assert.Equal(walked.Height, read.Height);
    }

    [Fact]
    public void ATileOutsideTheRecordIsAnError()
    {
        WalkedTiles walked = WalkedTiles.Empty(4, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => walked.Mark(new TilePoint(4, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => walked.WasWalked(new TilePoint(0, -1)));
    }

    [Fact]
    public void ARowOfAnotherLengthIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => WalkedTiles.OfRows(["....", "..."], "the save"));

        Assert.Contains("the save", error.Message, StringComparison.Ordinal);
        Assert.Contains("row 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownCharacterIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => WalkedTiles.OfRows(["..?."], "the save"));

        Assert.Contains("'?'", error.Message, StringComparison.Ordinal);
        Assert.Contains("(2, 0)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoRowIsAnError()
    {
        Assert.Throws<ArgumentException>(() => WalkedTiles.OfRows([], "the save"));
    }

    [Fact]
    public void ThePartyMarksItsSpawnTileWhenItEntersAMap()
    {
        MapState party = MapState.Enter(TestMaps.Room);

        Assert.True(party.Walked.WasWalked(TestMaps.Room.Spawn));
        Assert.Equal(1, party.Walked.Count);
    }

    [Fact]
    public void AWalkOfOneThousandSeedsNeverForgetsAWalkedTile()
    {
        // Exit test 6 of section 7.3 of `phase-2-first-playable.md` (D-567). The record grows
        // and never shrinks, and it holds every tile that the lead stood on.
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            Simulation run = Simulation.Start(seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            Pcg32 generator = Pcg32.FromSeed(seed, TestSequence);
            List<TilePoint> stood = [TestMaps.Room.Spawn];
            int count = run.State.Party.Walked.Count;

            for (int tick = 0; tick < 400; tick += 1)
            {
                run.Step([Intent.OfPlayer(StepIntent(generator.Next() % 4))]);

                TilePoint at = run.State.Party.LeadAt;
                if (!stood.Contains(at))
                {
                    stood.Add(at);
                }

                Assert.True(
                    run.State.Party.Walked.Count >= count,
                    $"The record lost a tile at tick {tick} of seed {seed} (D-567).");
                count = run.State.Party.Walked.Count;
            }

            foreach (TilePoint at in stood)
            {
                Assert.True(
                    run.State.Party.Walked.WasWalked(at),
                    $"The record forgot the tile {at} of seed {seed} (D-567).");
            }

            Assert.Equal(stood.Count, run.State.Party.Walked.Count);
        }
    }

    private static Core.Content.ContentId StepIntent(ulong choice) => choice switch
    {
        0 => IntentIds.MoveNorth,
        1 => IntentIds.MoveSouth,
        2 => IntentIds.MoveEast,
        _ => IntentIds.MoveWest,
    };
}
