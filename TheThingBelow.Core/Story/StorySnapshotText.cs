using System;
using System.Collections.Generic;
using System.Text.Json;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Story;

/// <summary>
/// The text of the story state inside a snapshot line, from save format 9 (D-540, D-652). The
/// snapshot text calls it, so one writer and one reader serve a record and a save (T-1).
/// </summary>
/// <remarks>
/// The object holds `flags`, `paused`, and `entry` always. It holds `won` while a win waits
/// for its battle end triggers, and `scene` while a story scene runs. An absent `won` or
/// `scene` means that none holds, as an absent `stepping` means that the lead stands (D-652).
/// <para>
/// From save format 14, the `scene` object holds `step_id` beside `step`, so a resume finds a
/// step that an edit of the story scene moved (D-1112). A snapshot of an older format holds the
/// index alone.
/// </para>
/// </remarks>
public static class StorySnapshotText
{
    /// <summary>Writes the story state as the `story` field of the snapshot object.</summary>
    /// <param name="writer">The writer, inside the snapshot object.</param>
    /// <param name="story">The values.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static void Write(Utf8JsonWriter writer, StoryValues story)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(story);

        writer.WriteStartObject("story");
        writer.WriteStartArray("flags");
        foreach (ContentId flag in story.Flags)
        {
            writer.WriteStringValue(flag.Value);
        }

        writer.WriteEndArray();
        writer.WriteBoolean("paused", story.Paused);
        writer.WriteBoolean("entry", story.EntryPending);
        if (story.WonPatrol is ContentId won)
        {
            writer.WriteString("won", won.Value);
        }

        if (story.Scene is SceneValues scene)
        {
            WriteScene(writer, scene);
        }

        writer.WriteEndObject();
    }

    /// <summary>Reads the `story` field of a snapshot object.</summary>
    /// <param name="reader">The reader, at the value of the field.</param>
    /// <param name="format">The save format of the snapshot, 9 or later (D-166).</param>
    /// <returns>The values, which `StoryState.Resume` checks against the content of this build.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    public static StoryValues Read(ref ContentReader reader, int format)
    {
        List<ContentId>? flags = null;
        bool? paused = null;
        bool? entry = null;
        ContentId? won = null;
        SceneValues? scene = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "flags":
                    flags = ReadFlags(ref reader);
                    break;
                case "paused":
                    paused = reader.ReadBoolean();
                    break;
                case "entry":
                    entry = reader.ReadBoolean();
                    break;
                case "won":
                    won = reader.ReadContentId(Patrol.IdKind);
                    break;
                case "scene":
                    scene = ReadScene(ref reader, format);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StoryValues(
            reader.Require(flags, depth, "flags"),
            scene,
            reader.RequireValue(paused, depth, "paused"),
            reader.RequireValue(entry, depth, "entry"),
            won);
    }

    private static void WriteScene(Utf8JsonWriter writer, SceneValues scene)
    {
        writer.WriteStartObject("scene");
        writer.WriteString("id", scene.Scene.Value);
        writer.WriteNumber("step", scene.Step);
        writer.WriteString(
            "step_id",
            (scene.StepId ?? throw new ArgumentException($"The story scene '{scene.Scene.Value}' holds no step id, and this build writes save format 14, which holds one (D-1112, T-2).", nameof(scene))).Value);
        writer.WriteString("phase", ScenePhases.NameOf(scene.Phase));
        writer.WriteNumber("ticks_left", scene.TicksLeft);
        writer.WriteStartArray("actors");
        foreach (ActorValues actor in scene.Actors)
        {
            writer.WriteStartObject();

            // A cast member writes the field of save format 9, and a scene-only NPC writes a
            // field of its own, so a save of an older format never holds an NPC actor (D-1006).
            writer.WriteString(SceneActor.IsNpcId(actor.Actor) ? "npc" : "character", actor.Actor.Value);
            writer.WriteNumber("x", actor.At.X);
            writer.WriteNumber("y", actor.At.Y);
            writer.WriteString("facing", StepDirections.NameOf(actor.Facing));
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static List<ContentId> ReadFlags(ref ContentReader reader)
    {
        List<ContentId> flags = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, flags.Count))
        {
            flags.Add(reader.ReadContentId(FlagList.Kind));
        }

        return flags;
    }

    private static SceneValues ReadScene(ref ContentReader reader, int format)
    {
        ContentId? id = null;
        int? step = null;
        ContentId? stepId = null;
        string? phase = null;
        int? ticksLeft = null;
        List<ActorValues>? actors = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(StoryScene.Kind);
                    break;
                case "step":
                    step = reader.ReadInt();
                    break;

                // Save format 13 and older predate the step id (D-1112).
                case "step_id" when format < 14:
                    throw reader.Refuse($"the snapshot of save format {format} holds a step id, and that format predates it (D-1112)");
                case "step_id":
                    stepId = reader.ReadContentId(StoryScene.StepKind);
                    break;
                case "phase":
                    phase = reader.ReadString();
                    break;
                case "ticks_left":
                    ticksLeft = reader.ReadInt();
                    break;
                case "actors":
                    actors = ReadActors(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string name = reader.Require(phase, depth, "phase");
        if (!ScenePhases.TryOf(name, out ScenePhase parsed))
        {
            throw reader.RefuseField(depth, "phase", $"the phase is '{name}', and a story scene takes one of ready, wait_intent, ticks, pick, and battle (D-540)");
        }

        return new SceneValues(
            reader.Require(id, depth, "id"),
            reader.RequireInt(step, depth, "step"),
            format < 14 ? null : reader.Require(stepId, depth, "step_id"),
            parsed,
            reader.RequireInt(ticksLeft, depth, "ticks_left"),
            reader.Require(actors, depth, "actors"));
    }

    private static List<ActorValues> ReadActors(ref ContentReader reader)
    {
        List<ActorValues> actors = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, actors.Count))
        {
            actors.Add(ReadActor(ref reader));
        }

        return actors;
    }

    private static ActorValues ReadActor(ref ContentReader reader)
    {
        ContentId? character = null;
        ContentId? npc = null;
        int? x = null;
        int? y = null;
        string? facing = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "character":
                    character = reader.ReadContentId(BattleFixture.CharacterKind);
                    break;
                case "npc":
                    npc = reader.ReadContentId(Npc.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string name = reader.Require(facing, depth, "facing");
        StepDirection? parsed = null;
        foreach (StepDirection direction in StepDirections.All)
        {
            if (string.CompareOrdinal(StepDirections.NameOf(direction), name) == 0)
            {
                parsed = direction;
            }
        }

        if (character is not null && npc is not null)
        {
            throw reader.RefuseField(depth, "npc", "the actor holds the fields 'character' and 'npc', and a shown actor is one cast member or one scene-only NPC (D-1006)");
        }

        return new ActorValues(
            npc ?? reader.Require(character, depth, "character"),
            new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")),
            parsed ?? throw reader.RefuseField(depth, "facing", $"the facing is '{name}', and a facing is one of north, south, east, and west (D-716)"));
    }
}
