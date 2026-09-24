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
/// cast member stands on the map at each step (D-1006, D-1007). A hide, a move, or a face of a
/// cast member that no earlier show put on the map fails the load, and so does a second show.
/// The rules check the tiles in play, because the place of an actor depends on the run.
/// </remarks>
public sealed class StoryContent
{
    private readonly SortedDictionary<string, StoryScene> scenes;

    private StoryContent(FlagList flags, SortedDictionary<string, StoryScene> scenes)
    {
        this.Flags = flags;
        this.scenes = scenes;
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

        return new StoryContent(flags, byId);
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

    /// <summary>
    /// Fails when a trigger of the map names an absent story scene or an undeclared flag, or
    /// when its story scene names a marker that the map lacks (D-528, D-543, T-2).
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

    private static void CheckScene(StoryScene scene, FlagList flags, BattleContent battle)
    {
        // The cast members on the map, in the order of their shows (D-1006).
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
                    if (say.Speaker?.Character is ContentId speaker)
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
                    RequireCast(scene, $"{field}.actor", show.Character, battle);
                    if (shown.Contains(show.Character.Value))
                    {
                        throw ContentException.ForField(scene.File, $"{field}.actor", $"the cast member '{show.Character.Value}' is already on the map, and a second show needs a hide first (D-1006)");
                    }

                    shown.Add(show.Character.Value);
                    break;
                case HideStep hide:
                    RequireOnMap(scene, field, new SceneActor(hide.Character), battle, shown);
                    _ = shown.Remove(hide.Character.Value);
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

    private static void RequireOnMap(StoryScene scene, string field, SceneActor actor, BattleContent battle, List<string> shown)
    {
        if (actor.Character is not ContentId character)
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
