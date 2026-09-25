using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Story;

/// <summary>
/// One story scene: a JSON list of steps, which Core runs and Game draws (D-173, D-540). One
/// file under `content/rules/scenes/` holds each story scene.
/// </summary>
/// <remarks>
/// The files are rule files, so a change to one changes the content hash (D-495). A story
/// scene names no art and no track: an art file names the actor ids that it draws, and an
/// audio file names the story scene that its cue serves (D-519, D-548).
/// <para>
/// This reader checks the shape of each step. `StoryContent` checks the ids that a step
/// shares with another file: the flags, the cast, the groups, the strings, and the markers of
/// each map whose trigger names the story scene (T-2).
/// </para>
/// </remarks>
public sealed class StoryScene
{
    /// <summary>The kind of a story scene id (D-646).</summary>
    public const string Kind = "scene";

    /// <summary>The folder that holds every story scene file, under `content/` (D-173).</summary>
    public const string Folder = "rules/scenes/";

    /// <summary>The word that marks a line with no speaker in a say step (D-997).</summary>
    public const string NoSpeaker = "none";

    /// <summary>The kind of a step id, which is unique inside its story scene (D-1112).</summary>
    public const string StepKind = "step";

    /// <summary>
    /// The step id that a snapshot stores when the story scene ran past its last step. No step
    /// of a story scene file takes it (D-1112).
    /// </summary>
    public const string EndStepId = "step.end";

    /// <summary>The id of <see cref="EndStepId"/>, which a snapshot stores past the last step (D-1112).</summary>
    public static readonly ContentId EndStep = ContentId.Parse(EndStepId, "code", nameof(EndStep));

    private StoryScene(string file, ContentId id, IReadOnlyList<SceneStep> steps, IReadOnlyList<ContentId> stepIds)
    {
        this.File = file;
        this.Id = id;
        this.Steps = steps;
        this.StepIds = stepIds;
    }

    /// <summary>The path of the file, under `content/`, for every error (T-2).</summary>
    public string File { get; }

    /// <summary>The permanent id of the story scene (D-166).</summary>
    public ContentId Id { get; }

    /// <summary>The steps, in the order that Core runs them (D-540).</summary>
    public IReadOnlyList<SceneStep> Steps { get; }

    /// <summary>
    /// The id of each step, at the index of its step. An edit of the file never changes the id
    /// of a step, so a resume finds a moved step by its id (D-1112, D-166).
    /// </summary>
    public IReadOnlyList<ContentId> StepIds { get; }

    /// <summary>Tells whether a path of this repository is a story scene file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the story scene folder and is a JSON file.</returns>
    /// <exception cref="ArgumentNullException">The path is null (T-2).</exception>
    public static bool IsSceneFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal) &&
            path.EndsWith(".json", StringComparison.Ordinal);
    }

    /// <summary>Reads one story scene file from its bytes, and checks the shape of each step (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The story scene.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static StoryScene Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        ContentId? id = null;
        List<SceneStep>? steps = null;
        List<ContentId> stepIds = [];

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "steps":
                    steps = ReadSteps(ref reader, stepIds);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        List<SceneStep> read = reader.Require(steps, depth, "steps");
        if (read.Count == 0)
        {
            throw reader.RefuseField(depth, "steps", "the story scene holds no step, and a story scene holds one or more (D-173)");
        }

        var scene = new StoryScene(file, reader.Require(id, depth, "id"), read, stepIds);
        reader.ReadFileEnd();
        return scene;
    }

    /// <summary>Finds the index of the step that holds one id (D-1112).</summary>
    /// <param name="stepId">The id of the step.</param>
    /// <param name="index">The index of that step, or -1 when no step holds the id.</param>
    /// <returns>True when a step of this story scene holds the id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool TryIndexOfStep(ContentId stepId, out int index)
    {
        ArgumentNullException.ThrowIfNull(stepId);

        for (int at = 0; at < this.StepIds.Count; at += 1)
        {
            if (string.CompareOrdinal(this.StepIds[at].Value, stepId.Value) == 0)
            {
                index = at;
                return true;
            }
        }

        index = -1;
        return false;
    }

    /// <summary>Gives the field path of one step, for an error (T-2).</summary>
    /// <param name="index">The index of the step.</param>
    /// <returns>The path, such as `scene.x.steps[3]`.</returns>
    public string StepField(int index) => $"{this.Id.Value}.steps[{index}]";

    private static List<SceneStep> ReadSteps(ref ContentReader reader, List<ContentId> stepIds)
    {
        List<SceneStep> steps = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, steps.Count))
        {
            steps.Add(ReadStep(ref reader, stepIds));
        }

        return steps;
    }

    private static SceneStep ReadStep(ref ContentReader reader, List<ContentId> stepIds)
    {
        StepFields fields = new();
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            ReadStepField(ref reader, field, fields);
        }

        string name = reader.Require(fields.Kind, depth, "kind");
        if (!SceneStepKinds.TryOf(name, out SceneStepKind kind))
        {
            throw reader.RefuseField(depth, "kind", $"the step takes the kind '{name}', and a step takes one of {SceneStepKinds.EveryName} (D-997)");
        }

        fields.RefuseFieldsOutside(ref reader, depth, kind);
        stepIds.Add(StepIdOf(ref reader, depth, fields, stepIds));
        return kind switch
        {
            SceneStepKind.Move => new MoveStep(reader.Require(fields.Actor, depth, "actor"), PathOf(ref reader, depth, fields)),
            SceneStepKind.Face => new FaceStep(reader.Require(fields.Actor, depth, "actor"), reader.RequireValue(fields.Facing, depth, "facing")),
            SceneStepKind.Wait => new WaitStep(TicksOf(ref reader, depth, fields)),
            SceneStepKind.Say => new SayStep(SpeakerOf(ref reader, depth, fields), reader.Require(fields.Line, depth, "line")),
            SceneStepKind.Choose => new ChooseStep(OptionsOf(ref reader, depth, fields)),
            SceneStepKind.SetFlag => new SetFlagStep(reader.Require(fields.Flag, depth, "flag")),
            SceneStepKind.Join => new JoinStep(reader.Require(fields.Character, depth, "character")),
            SceneStepKind.Show => new ShowStep(CastOf(ref reader, depth, fields), reader.Require(fields.At, depth, "at"), reader.RequireValue(fields.Facing, depth, "facing")),
            SceneStepKind.Hide => new HideStep(CastOf(ref reader, depth, fields)),
            SceneStepKind.Camera => new CameraStep(reader.Require(fields.At, depth, "at")),
            SceneStepKind.StartBattle => new StartBattleStep(reader.Require(fields.Group, depth, "group")),
            _ => throw reader.RefuseField(depth, "kind", $"the step kind '{name}' has no reader (T-2)"),
        };
    }

    /// <summary>
    /// Reads the id of one step. The id is unique inside the story scene, and no step takes
    /// the id that marks the end, so a snapshot names each step with no doubt (D-1112).
    /// </summary>
    private static ContentId StepIdOf(ref ContentReader reader, int depth, StepFields fields, List<ContentId> earlier)
    {
        ContentId stepId = reader.Require(fields.Id, depth, "id");
        if (string.CompareOrdinal(stepId.Value, EndStepId) == 0)
        {
            throw reader.RefuseField(depth, "id", $"the step takes the id '{EndStepId}', which marks the end of a story scene in a snapshot (D-1112)");
        }

        foreach (ContentId other in earlier)
        {
            if (string.CompareOrdinal(other.Value, stepId.Value) == 0)
            {
                throw reader.RefuseField(depth, "id", $"two steps take the id '{stepId.Value}', and each step of a story scene takes its own id (D-1112)");
            }
        }

        return stepId;
    }

    private static void ReadStepField(ref ContentReader reader, string field, StepFields fields)
    {
        switch (field)
        {
            case "id":
                fields.Id = reader.ReadContentId(StepKind);
                break;
            case "kind":
                fields.Kind = reader.ReadString();
                break;
            case "actor":
                fields.Actor = ReadActor(ref reader);
                break;
            case "path":
                fields.Path = ReadPath(ref reader);
                break;
            case "facing":
                fields.Facing = ReadDirection(ref reader);
                break;
            case "ticks":
                fields.Ticks = reader.ReadInt();
                break;
            case "speaker":
                fields.Speaker = reader.ReadString();
                break;
            case "line":
                // A string id names where the player reads the text, so it takes the kind of
                // that place and never the kind of this record (G-7, D-646).
                fields.Line = reader.ReadContentId();
                break;
            case "options":
                fields.Options = ReadOptions(ref reader);
                break;
            case "flag":
                fields.Flag = reader.ReadContentId(FlagList.Kind);
                break;
            case "character":
                fields.Character = reader.ReadContentId(BattleFixture.CharacterKind);
                break;
            case "at":
                fields.At = reader.ReadContentId(MapThingKinds.NameOf(MapThingKind.Marker));
                break;
            case "group":
                fields.Group = reader.ReadContentId(Patrol.GroupKind);
                break;
            default:
                throw reader.UnknownField(field);
        }

        fields.Names.Add(field);
    }

    private static SceneActor ReadActor(ref ContentReader reader)
    {
        string text = reader.ReadString();
        if (string.CompareOrdinal(text, SceneActor.LeadName) == 0)
        {
            return SceneActor.Lead;
        }

        ContentId id = ContentId.Parse(text, reader.File, "actor");
        if (string.CompareOrdinal(id.Kind, BattleFixture.CharacterKind) != 0)
        {
            throw reader.Refuse($"the actor '{text}' is neither '{SceneActor.LeadName}' nor an id of the kind '{BattleFixture.CharacterKind}'. PR-14 adds the NPC as an actor (D-1005, D-1006)");
        }

        return new SceneActor(id);
    }

    private static List<StepDirection> ReadPath(ref ContentReader reader)
    {
        List<StepDirection> path = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, path.Count))
        {
            path.Add(ReadDirection(ref reader));
        }

        return path;
    }

    private static StepDirection ReadDirection(ref ContentReader reader)
    {
        string name = reader.ReadString();
        foreach (StepDirection direction in StepDirections.All)
        {
            if (string.CompareOrdinal(StepDirections.NameOf(direction), name) == 0)
            {
                return direction;
            }
        }

        throw reader.Refuse($"the direction is '{name}', and a direction is one of north, south, east, and west (D-716)");
    }

    private static List<ChooseOption> ReadOptions(ref ContentReader reader)
    {
        List<ChooseOption> options = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, options.Count))
        {
            options.Add(ReadOption(ref reader));
        }

        return options;
    }

    private static ChooseOption ReadOption(ref ContentReader reader)
    {
        ContentId? line = null;
        ContentId? flag = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "line":
                    line = reader.ReadContentId();
                    break;
                case "flag":
                    flag = reader.ReadContentId(FlagList.Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ChooseOption(reader.Require(line, depth, "line"), reader.Require(flag, depth, "flag"));
    }

    private static List<StepDirection> PathOf(ref ContentReader reader, int depth, StepFields fields)
    {
        List<StepDirection> path = reader.Require(fields.Path, depth, "path");
        if (path.Count == 0)
        {
            throw reader.RefuseField(depth, "path", "the path holds no direction, and a move walks one tile or more (D-1012)");
        }

        return path;
    }

    private static int TicksOf(ref ContentReader reader, int depth, StepFields fields)
    {
        int ticks = reader.RequireInt(fields.Ticks, depth, "ticks");
        if (ticks <= 0)
        {
            throw reader.RefuseField(depth, "ticks", $"the wait is {ticks} ticks, and a wait lasts one tick or more (D-1000)");
        }

        return ticks;
    }

    private static SceneActor? SpeakerOf(ref ContentReader reader, int depth, StepFields fields)
    {
        string speaker = reader.Require(fields.Speaker, depth, "speaker");
        if (string.CompareOrdinal(speaker, NoSpeaker) == 0)
        {
            return null;
        }

        if (string.CompareOrdinal(speaker, SceneActor.LeadName) == 0)
        {
            return SceneActor.Lead;
        }

        ContentId id = ContentId.Parse(speaker, reader.File, "speaker");
        if (string.CompareOrdinal(id.Kind, BattleFixture.CharacterKind) != 0)
        {
            throw reader.RefuseField(depth, "speaker", $"the speaker '{speaker}' is not '{NoSpeaker}', '{SceneActor.LeadName}', or an id of the kind '{BattleFixture.CharacterKind}' (D-997)");
        }

        return new SceneActor(id);
    }

    private static List<ChooseOption> OptionsOf(ref ContentReader reader, int depth, StepFields fields)
    {
        List<ChooseOption> options = reader.Require(fields.Options, depth, "options");
        if (options.Count < 2)
        {
            throw reader.RefuseField(depth, "options", $"the choice holds {options.Count} options, and a choice holds two or more (D-1007)");
        }

        for (int index = 0; index < options.Count; index += 1)
        {
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (string.CompareOrdinal(options[earlier].Flag.Value, options[index].Flag.Value) == 0)
                {
                    throw reader.RefuseField(depth, $"options[{index}].flag", $"two options set the flag '{options[index].Flag.Value}', and each pick sets its own flag (D-1007)");
                }
            }
        }

        return options;
    }

    private static ContentId CastOf(ref ContentReader reader, int depth, StepFields fields)
    {
        SceneActor actor = reader.Require(fields.Actor, depth, "actor");
        return actor.Character ?? throw reader.RefuseField(
            depth,
            "actor",
            "a show or a hide names the lead, and the lead stays on the map. A show or a hide names a cast member (D-1006)");
    }

    /// <summary>The fields of one step as the reader finds them, before the kind picks its record.</summary>
    private sealed class StepFields
    {
        public List<string> Names { get; } = [];

        public ContentId? Id { get; set; }

        public string? Kind { get; set; }

        public SceneActor? Actor { get; set; }

        public List<StepDirection>? Path { get; set; }

        public StepDirection? Facing { get; set; }

        public int? Ticks { get; set; }

        public string? Speaker { get; set; }

        public ContentId? Line { get; set; }

        public List<ChooseOption>? Options { get; set; }

        public ContentId? Flag { get; set; }

        public ContentId? Character { get; set; }

        public ContentId? At { get; set; }

        public ContentId? Group { get; set; }

        /// <summary>Refuses a field that the kind of the step does not read, so no value passes in silence (T-2).</summary>
        public void RefuseFieldsOutside(ref ContentReader reader, int depth, SceneStepKind kind)
        {
            string[] allowed = FieldsOf(kind);
            foreach (string name in this.Names)
            {
                if (string.CompareOrdinal(name, "kind") == 0 || string.CompareOrdinal(name, "id") == 0 || Array.IndexOf(allowed, name) >= 0)
                {
                    continue;
                }

                throw reader.RefuseField(depth, name, $"a step of the kind '{SceneStepKinds.NameOf(kind)}' reads no field '{name}' (T-2)");
            }
        }

        private static string[] FieldsOf(SceneStepKind kind) => kind switch
        {
            SceneStepKind.Move => ["actor", "path"],
            SceneStepKind.Face => ["actor", "facing"],
            SceneStepKind.Wait => ["ticks"],
            SceneStepKind.Say => ["speaker", "line"],
            SceneStepKind.Choose => ["options"],
            SceneStepKind.SetFlag => ["flag"],
            SceneStepKind.Join => ["character"],
            SceneStepKind.Show => ["actor", "at", "facing"],
            SceneStepKind.Hide => ["actor"],
            SceneStepKind.Camera => ["at"],
            SceneStepKind.StartBattle => ["group"],
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no story scene step kind (D-997)"),
        };
    }
}
