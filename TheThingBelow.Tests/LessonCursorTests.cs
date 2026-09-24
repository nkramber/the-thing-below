using System;
using System.Collections;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the lesson window: the slots, the swap at a swap place, and the cast from the
/// menu (D-356, D-391, D-1030). A move makes no intent, and a whole choice makes one (D-493).
/// The tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class LessonCursorTests
{
    private const ulong Seed = 5;

    [Fact]
    public void OutsideASwapPlaceASlotWithNoHealOffersNothing()
    {
        // D-1030: the hew and the cinder act in a fight alone, and no swap works away from a swap place.
        Simulation run = InMenu(TestBattles.Content, swapPlace: false);
        GameValue cursor = GameValue.New("LessonCursor", run.State);

        Assert.Empty((IList)cursor.Call("UsesOf", 0)!);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Slot", cursor.Name("Stage"));
    }

    [Fact]
    public void AtASwapPlaceASlotSwapsToALessonOfThePack()
    {
        Simulation run = InMenu(TestBattles.Content, swapPlace: true);
        GameValue cursor = GameValue.New("LessonCursor", run.State);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Pack", cursor.Name("Stage"));
        IList entries = (IList)cursor.Read<object>("PackEntries");
        Assert.Null(entries[0]);
        Assert.Equal("lesson.fixture_salve", ((ContentId)entries[1]!).Value);

        // The cursor opens on the first lesson of the pack, after the empty entry.
        Assert.Equal(1, cursor.Read<int>("Cursor"));
        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The window sent no intent.");

        Assert.Equal(IntentIds.LessonSwap.Value, made.Action.Value);
        Assert.Equal((0, 0, "lesson.fixture_salve"), (made.Actor, made.Option, made.Lesson?.Value));
        Assert.Equal("Slot", cursor.Name("Stage"));
        run.Step([made]);
        Assert.Equal("lesson.fixture_salve", run.State.Characters.Members[0].Slots[0]?.Value);
    }

    [Fact]
    public void AMendRiteAtASwapPlaceOffersTheCastAndTheSwap()
    {
        // D-391: the salve casts from the menu, and the swap place adds the swap.
        Simulation run = InMenu(WithSalve(), swapPlace: true, hurt: true);
        GameValue cursor = GameValue.New("LessonCursor", run.State);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Choice", cursor.Name("Stage"));
        Assert.Equal(["Cast", "Swap"], Names((IList)cursor.Read<object>("Uses")));

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Form", cursor.Name("Stage"));
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Target", cursor.Name("Stage"));
        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The window sent no intent.");

        Assert.Equal(IntentIds.MenuCast.Value, made.Action.Value);
        Assert.Equal((0, "lesson.fixture_salve", 0), (made.Actor, made.Lesson?.Value, made.Option));
        Assert.Equal(new BattleTarget(BattleSide.Party, 0), made.Target);
        int before = run.State.Characters.Members[0].Health;
        run.Step([made]);
        Assert.True(run.State.Characters.Members[0].Health > before);
    }

    [Fact]
    public void ACancelWalksBackThroughEachListAndClosesTheWindowFromTheSlots()
    {
        Simulation run = InMenu(WithSalve(), swapPlace: true, hurt: true);
        GameValue cursor = GameValue.New("LessonCursor", run.State);
        cursor.Call("Confirm");
        cursor.Call("Confirm");
        cursor.Call("Confirm");

        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("Form", cursor.Name("Stage"));
        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("Choice", cursor.Name("Stage"));
        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("Slot", cursor.Name("Stage"));
        Assert.True((bool)cursor.Call("Cancel")!);
    }

    [Fact]
    public void SilenceDimsTheCastFromTheMenu()
    {
        // D-393: silence on the party stops a rite from the menu, and the form stays on the list.
        Simulation run = InMenu(WithSalve(), swapPlace: false, hurt: true, silenced: true);
        GameValue cursor = GameValue.New("LessonCursor", run.State);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Form", cursor.Name("Stage"));
        Assert.False((bool)cursor.Call("AllowsForm", 0)!);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Form", cursor.Name("Stage"));
    }

    [Fact]
    public void TheWindowShowsAnEmptyMarkUntilTheFlagOfTheSideAptitudeIsOn()
    {
        // Exit test 2 of PR-12 (D-283, D-538, D-1033): a story flag of PR-68 unlocks the guard of Marrek.
        Simulation run = InMenu(TestBattles.Content, swapPlace: false);
        CharacterRecord marrek = run.State.Characters.Members[0].Record;

        Assert.Equal("menu.aptitude_hidden", ((ContentId)GameValue.Static("LessonsView", "SideIdOf", marrek, run.State.Story.Flags)!).Value);
        Assert.True(run.State.Story.Flags.TurnOn(marrek.SideFlag));
        Assert.Equal("aptitude.guard", ((ContentId)GameValue.Static("LessonsView", "SideIdOf", marrek, run.State.Story.Flags)!).Value);
    }

    [Fact]
    public void TheCharacterTurnsInTheSlotStageAlone()
    {
        Simulation run = InMenu(TestBattles.WithParty(2), swapPlace: true);
        GameValue cursor = GameValue.New("LessonCursor", run.State);

        cursor.Call("Turn", 1);
        Assert.Equal(1, cursor.Read<int>("Character"));
        cursor.Call("Turn", 1);
        Assert.Equal(0, cursor.Read<int>("Character"));

        cursor.Call("Confirm");
        cursor.Call("Turn", 1);
        Assert.Equal(0, cursor.Read<int>("Character"));
    }

    /// <summary>Gives the content of the tests with the salve in the second slot of Marrek, in place of the cinder.</summary>
    private static BattleContent WithSalve() => TestBattles.WithLessonFiles(fixture: TestBattles.FixtureFile
        .Replace("[\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]", "[\"lesson.fixture_salve\", \"lesson.fixture_cinder\"]", StringComparison.Ordinal)
        .Replace("\"lesson_pack\": [\"lesson.fixture_salve\", ", "\"lesson_pack\": [\"lesson.fixture_hew\", ", StringComparison.Ordinal));

    /// <summary>Starts a run on the room map, hurts or silences Marrek through a load, and opens the menu.</summary>
    private static Simulation InMenu(BattleContent content, bool swapPlace, bool hurt = false, bool silenced = false)
    {
        var story = TheThingBelow.Core.Story.StoryContent.Load(
            TheThingBelow.Core.Story.FlagList.Read(System.Text.Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), TheThingBelow.Core.Story.FlagList.Path),
            [],
            content);
        GameMap map = TestMaps.Room;
        RunSnapshot start = Simulation.Start(Seed, map, content, TestBattles.Notices, story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters!;
        CharacterValues marrek = party.Characters[0] with
        {
            Health = hurt ? 20 : party.Characters[0].Health,
            Statuses = silenced ? [StatusKind.Silence] : [],
        };
        List<CharacterValues> characters = [marrek];
        for (int slot = 1; slot < party.Characters.Count; slot += 1)
        {
            characters.Add(party.Characters[slot]);
        }

        Simulation run = Simulation.Resume(Seed, start with { Characters = party with { Characters = characters } }, map, content, TestBattles.Notices, story, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        if (swapPlace)
        {
            run.State.Characters.MarkSwapPlace();
        }

        return run;
    }

    private static List<string> Names(IList values)
    {
        List<string> names = [];
        foreach (object? value in values)
        {
            names.Add(value!.ToString()!);
        }

        return names;
    }
}
