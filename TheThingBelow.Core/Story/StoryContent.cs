using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Story;

/// <summary>
/// The story content of one build: the flag file and every story scene (D-542, D-1003). The
/// load checks each id that a story scene shares with another file (T-2).
/// </summary>
/// <remarks>
/// A story scene of PR-68 holds no branch, so the load walks its steps once and knows which
/// actor a show put on the map at each step (D-1006, D-1007). A hide, a move, or a face of a cast
/// member that no earlier show put on the map fails the load, and so does a second show, or a
/// hide of an actor that no show put there. The rules check the tiles in play, because the place
/// of an actor depends on the run.
/// <para>
/// An NPC actor depends on the map of the trigger (D-1006). A move or a face of an NPC needs an
/// NPC that the map places or that an earlier show put on the map. A show of an NPC needs a
/// scene-only NPC, which the map does not place. A line can name an NPC speaker with no body on
/// the map, such as a voice through a door, so the load checks the kind of its id alone (D-1146). <see cref="RequireScenesOf"/> checks both for
/// each map that starts the story scene.
/// </para>
/// </remarks>
public sealed class StoryContent
{
    private readonly SortedDictionary<string, StoryScene> scenes;

    // The ids of the cast of this build, which a stored actor must name (D-166, D-1006).
    private readonly SortedSet<string> cast;

    private StoryContent(FlagList flags, SortedDictionary<string, StoryScene> scenes, SortedSet<string> cast)
    {
        this.Flags = flags;
        this.scenes = scenes;
        this.cast = cast;
    }

    /// <summary>The flag file of this build (D-1003).</summary>
    public FlagList Flags { get; }

    /// <summary>Every story scene, in the ordinal order of the ids (G-4).</summary>
    public IEnumerable<StoryScene> Scenes => this.scenes.Values;

    /// <summary>Checks the story scenes against the flags and the battle content (T-2).</summary>
    /// <param name="flags">The flag file.</param>
    /// <param name="scenes">Every story scene, in any order.</param>
    /// <param name="battle">The battle content, which holds the cast and the groups.</param>
    /// <returns>The story content.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// Two story scenes take one id, or a step names an undeclared flag, an absent cast member,
    /// an absent group, or a cast member that is not on the map at that step (T-2).
    /// </exception>
    public static StoryContent Load(FlagList flags, IReadOnlyList<StoryScene> scenes, BattleContent battle)
    {
        ArgumentNullException.ThrowIfNull(flags);
        ArgumentNullException.ThrowIfNull(scenes);
        ArgumentNullException.ThrowIfNull(battle);

        var byId = new SortedDictionary<string, StoryScene>(StringComparer.Ordinal);
        foreach (StoryScene scene in scenes)
        {
            ArgumentNullException.ThrowIfNull(scene);
            if (byId.TryGetValue(scene.Id.Value, out StoryScene? first))
            {
                throw ContentException.ForField(scene.File, "id", $"the story scene id '{scene.Id.Value}' is also the id of '{first.File}', and an id is permanent (D-166)");
            }

            CheckScene(scene, flags, battle);
            byId.Add(scene.Id.Value, scene);
        }

        var cast = new SortedSet<string>(StringComparer.Ordinal);
        foreach (CharacterRecord record in battle.Fixture.Characters)
        {
            _ = cast.Add(record.Id.Value);

            // The side aptitude of each character reads a flag of the flag file (D-538, D-556).
            flags.RequireDeclared(record.SideFlag, BattleFixture.Path, $"{record.Id.Value}.side_flag");
        }

        return new StoryContent(flags, byId, cast);
    }

    /// <summary>Finds a story scene by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The story scene.</returns>
    /// <exception cref="ContentException">No story scene has this id (T-2).</exception>
    public StoryScene Scene(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.scenes.TryGetValue(id.Value, out StoryScene? scene)
            ? scene
            : throw ContentException.ForField(StoryScene.Folder, id.Value, "no story scene file has this id (T-2, D-173)");
    }

    /// <summary>Finds a story scene by id, for a stored id that a load of a save reads (D-166).</summary>
    /// <param name="id">The id.</param>
    /// <param name="scene">The story scene, when this build holds it.</param>
    /// <returns>True when a story scene has this id.</returns>
    public bool TryScene(ContentId id, out StoryScene? scene)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.scenes.TryGetValue(id.Value, out scene);
    }

    /// <summary>Tells whether the battle content of this build holds a cast member, for a stored actor that a load reads (D-166).</summary>
    /// <param name="character">The id of the cast member.</param>
    /// <returns>True when the battle fixture holds a character with this id.</returns>
    public bool HoldsCast(ContentId character)
    {
        ArgumentNullException.ThrowIfNull(character);

        return this.cast.Contains(character.Value);
    }

    /// <summary>
    /// Fails when a trigger of the map names an absent story scene or an undeclared flag, or
    /// when its story scene names a marker that the map lacks, or an NPC actor that breaks the rules
    /// of this map (D-528, D-543, D-1006, T-2).
    /// </summary>
    /// <param name="map">The map.</param>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">A trigger breaks one of these rules. The error names the map and the trigger.</exception>
    public void RequireScenesOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        foreach (SceneTrigger trigger in map.Triggers)
        {
            string field = $"triggers.{trigger.Id.Value}";
            if (!this.scenes.TryGetValue(trigger.Scene.Value, out StoryScene? scene))
            {
                throw ContentException.ForField(map.File, $"{field}.scene", $"no story scene file has the id '{trigger.Scene.Value}' (T-2, D-173)");
            }

            trigger.Condition.RequireDeclared(this.Flags, map.File, $"{field}.condition");
            for (int index = 0; index < scene.Steps.Count; index += 1)
            {
                ContentId? marker = scene.Steps[index] switch
                {
                    ShowStep show => show.Marker,
                    CameraStep camera => camera.Marker,
                    _ => null,
                };
                if (marker is ContentId named && !map.TryMarker(named, out _))
                {
                    throw ContentException.ForField(
                        map.File,
                        field,
                        $"the trigger starts '{scene.Id.Value}', whose step {index} names the marker '{named.Value}', and this map holds no such marker (D-528, D-1006)");
                }
            }

            RequireNpcActorsOf(map, field, scene);
        }
    }

    /// <summary>Fails when the condition of a service of the map names an undeclared flag (D-543, T-2).</summary>
    /// <param name="map">The map.</param>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">A condition names an undeclared flag. The error names the map and the service.</exception>
    /// <remarks>A story flag closes a service through this condition (D-543, D-1131).</remarks>
    public void RequireServicesOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        foreach (MapService service in map.Services)
        {
            service.Condition.RequireDeclared(this.Flags, map.File, $"services.{service.Id.Value}.condition");
        }
    }

    /// <summary>Fails when a step names a string id that the table lacks (G-7, T-2).</summary>
    /// <param name="strings">The string table.</param>
    /// <exception cref="ArgumentNullException">The table is null (T-2).</exception>
    /// <exception cref="ContentException">A line of a say step or an option names an absent id. The error names the story scene, the step, and the id.</exception>
    public void RequireStrings(StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(strings);

        foreach (StoryScene scene in this.scenes.Values)
        {
            for (int index = 0; index < scene.Steps.Count; index += 1)
            {
                SceneStep step = scene.Steps[index];
                if (step is SayStep say)
                {
                    RequireString(strings, scene, index, "line", say.Line);
                }
                else if (step is ChooseStep choose)
                {
                    for (int option = 0; option < choose.Options.Count; option += 1)
                    {
                        RequireString(strings, scene, index, $"options[{option}].line", choose.Options[option].Line);
                    }
                }
            }
        }
    }

    private static void RequireString(StringTable strings, StoryScene scene, int index, string field, ContentId line)
    {
        if (!strings.Contains(line))
        {
            throw ContentException.ForField(scene.File, $"{scene.StepField(index)}.{field}", $"the string table holds no id '{line.Value}' (G-7)");
        }
    }

    /// <summary>
    /// Walks the steps of one story scene that a trigger of the map starts, and refuses an NPC actor
    /// that breaks the rules of the map (D-1006). A show names a scene-only NPC, which the map does
    /// not place. A move and a face of an NPC name an NPC that the map places or that an earlier
    /// show put on the map. A line of an NPC needs no body on the map (D-1146).
    /// </summary>
    private static void RequireNpcActorsOf(GameMap map, string field, StoryScene scene)
    {
        // The scene-only NPCs on the map, in the order of their shows (D-1006).
        List<string> shown = [];
        for (int index = 0; index < scene.Steps.Count; index += 1)
        {
            ContentId? named = null;
            switch (scene.Steps[index])
            {
                case ShowStep show when SceneActor.IsNpcId(show.Actor):
                    if (map.PlacesNpc(show.Actor))
                    {
                        throw ContentException.ForField(
                            map.File,
                            field,
                            $"the trigger starts '{scene.Id.Value}', whose step {index} shows the NPC '{show.Actor.Value}', which this map places. A show puts a scene-only NPC on the map, and a move or a face names an NPC of the map (D-1006)");
                    }

                    shown.Add(show.Actor.Value);
                    break;
                case HideStep hide:
                    _ = shown.Remove(hide.Actor.Value);
                    break;
                case MoveStep move:
                    named = move.Actor.Id;
                    break;
                case FaceStep face:
                    named = face.Actor.Id;
                    break;
            }

            if (SceneActor.IsNpcId(named) && !shown.Contains(named!.Value) && !map.PlacesNpc(named))
            {
                throw ContentException.ForField(
                    map.File,
                    field,
                    $"the trigger starts '{scene.Id.Value}', whose step {index} names the NPC '{named.Value}', which this map does not place and no earlier show put on the map (D-1006)");
            }
        }
    }

    private static void CheckScene(StoryScene scene, FlagList flags, BattleContent battle)
    {
        // The cast members and the scene-only NPCs on the map, in the order of their shows (D-1006).
        List<string> shown = [];
        for (int index = 0; index < scene.Steps.Count; index += 1)
        {
            string field = scene.StepField(index);
            switch (scene.Steps[index])
            {
                case MoveStep move:
                    RequireOnMap(scene, field, move.Actor, battle, shown);
                    break;
                case FaceStep face:
                    RequireOnMap(scene, field, face.Actor, battle, shown);
                    break;
                case SayStep say:
                    // A line of an NPC needs no body on the map, so the reader checks the kind of its id alone (D-1146).
                    if (say.Speaker?.Id is ContentId speaker && !SceneActor.IsNpcId(speaker))
                    {
                        RequireCast(scene, $"{field}.speaker", speaker, battle);
                    }

                    break;
                case ChooseStep choose:
                    for (int option = 0; option < choose.Options.Count; option += 1)
                    {
                        flags.RequireDeclared(choose.Options[option].Flag, scene.File, $"{field}.options[{option}].flag");
                    }

                    break;
                case SetFlagStep set:
                    flags.RequireDeclared(set.Flag, scene.File, $"{field}.flag");
                    break;
                case JoinStep join:
                    RequireCast(scene, $"{field}.character", join.Character, battle);
                    break;
                case ShowStep show:
                    if (!SceneActor.IsNpcId(show.Actor))
                    {
                        RequireCast(scene, $"{field}.actor", show.Actor, battle);
                    }

                    if (shown.Contains(show.Actor.Value))
                    {
                        throw ContentException.ForField(scene.File, $"{field}.actor", $"the actor '{show.Actor.Value}' is already on the map, and a second show needs a hide first (D-1006)");
                    }

                    shown.Add(show.Actor.Value);
                    break;
                case HideStep hide:
                    if (!shown.Contains(hide.Actor.Value))
                    {
                        throw ContentException.ForField(scene.File, $"{field}.actor", $"the actor '{hide.Actor.Value}' is not on the map at this step, and a hide takes an actor that a show step put there. An NPC of the map stays on it (D-1006)");
                    }

                    _ = shown.Remove(hide.Actor.Value);
                    break;
                case StartBattleStep start:
                    RequireGroup(scene, $"{field}.group", start.Group, battle);
                    break;
                case WaitStep:
                case CameraStep:
                    break;
                default:
                    throw ContentException.ForField(scene.File, field, $"the step '{SceneStepKinds.NameOf(scene.Steps[index].Kind)}' has no check (T-2)");
            }
        }
    }

    /// <summary>
    /// Refuses a move or a face of a cast member that no show put on the map (D-1006). The lead is
    /// always there, and the map of each trigger checks an NPC.
    /// </summary>
    private static void RequireOnMap(StoryScene scene, string field, SceneActor actor, BattleContent battle, List<string> shown)
    {
        if (actor.Id is not ContentId character || actor.IsNpc)
        {
            return;
        }

        RequireCast(scene, $"{field}.actor", character, battle);
        if (!shown.Contains(character.Value))
        {
            throw ContentException.ForField(scene.File, $"{field}.actor", $"the cast member '{character.Value}' is not on the map at this step, and a show step puts a cast member there (D-1006)");
        }
    }

    private static void RequireCast(StoryScene scene, string field, ContentId character, BattleContent battle)
    {
        foreach (CharacterRecord record in battle.Fixture.Characters)
        {
            if (string.CompareOrdinal(record.Id.Value, character.Value) == 0)
            {
                return;
            }
        }

        throw ContentException.ForField(scene.File, field, $"the battle fixture holds no character '{character.Value}' (T-2, D-563)");
    }

    private static void RequireGroup(StoryScene scene, string field, ContentId group, BattleContent battle)
    {
        foreach (GroupFile file in battle.GroupFiles)
        {
            if (file.Find(group) is not null)
            {
                return;
            }
        }

        throw ContentException.ForField(scene.File, field, $"no group file holds the group '{group.Value}' (T-2, D-998)");
    }
}
