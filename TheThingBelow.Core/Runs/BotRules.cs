using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The bot rules file: the start maps of the bot runs, the story flag that ends a bot run as
/// complete, and the tick budget of a bot run (D-1181, D-1184, D-1185). The file is `content/rules/bots.json`.
/// </summary>
/// <remarks>
/// No rule of the game reads the file. The headless runner of Tools reads it, and Core holds its
/// record, as it holds the record of every content file (D-517). The story content moves the goal
/// flag as the game grows, and PR-15 measures the budget (G-14).
/// </remarks>
public sealed class BotRules
{
    /// <summary>The path of the file under the content folder (D-1181).</summary>
    public const string Path = "rules/bots.json";

    private BotRules(string file, IReadOnlyList<ContentId> startMaps, ContentId goalFlag, long tickBudget)
    {
        this.File = file;
        this.StartMaps = startMaps;
        this.GoalFlag = goalFlag;
        this.TickBudget = tickBudget;
    }

    /// <summary>The path of the file, for an error (T-2).</summary>
    public string File { get; }

    /// <summary>The maps where the bot runs start, in turn by seed: the seed modulo the count picks the map (D-1185).</summary>
    public IReadOnlyList<ContentId> StartMaps { get; }

    /// <summary>The story flag that ends a bot run as complete when it is on (D-1181).</summary>
    public ContentId GoalFlag { get; }

    /// <summary>The count of ticks after which a bot run with no other end ends as budget (D-1184).</summary>
    public long TickBudget { get; }

    /// <summary>Reads the bot rules file, and refuses a budget below one tick (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The rules.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or repeated, or the budget is below one tick (G-6, T-2).</exception>
    public static BotRules Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<ContentId>? starts = null;
        ContentId? goal = null;
        long? budget = null;
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "start_maps":
                    starts = ReadStartMaps(ref reader);
                    break;
                case "goal_flag":
                    goal = reader.ReadContentId(FlagList.Kind);
                    break;
                case "tick_budget":
                    budget = reader.ReadLong();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        long readBudget = reader.RequireValue(budget, depth, "tick_budget");
        if (readBudget < 1)
        {
            throw reader.RefuseField(depth, "tick_budget", $"the budget is {readBudget} ticks, and a bot run takes one tick at least (D-1184)");
        }

        List<ContentId> readStarts = reader.Require(starts, depth, "start_maps");
        if (readStarts.Count == 0)
        {
            throw reader.RefuseField(depth, "start_maps", "the list is empty, and a bot run starts on one map at least (D-1185)");
        }

        var rules = new BotRules(file, readStarts, reader.Require(goal, depth, "goal_flag"), readBudget);
        reader.ReadFileEnd();
        return rules;
    }

    /// <summary>Gives the start map of the run of a seed (D-1185).</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The id of the map.</returns>
    public ContentId StartOf(ulong seed) => this.StartMaps[(int)(seed % (ulong)this.StartMaps.Count)];

    /// <summary>
    /// Fails when a start map is not a map of the content (D-1185, T-2). The runner checks it
    /// before a play, because a content set of a test can hold no map.
    /// </summary>
    /// <param name="maps">The maps of this build.</param>
    /// <exception cref="ContentException">The content holds no map with a start id.</exception>
    public void RequireStartsOf(MapSet maps)
    {
        ArgumentNullException.ThrowIfNull(maps);

        for (int index = 0; index < this.StartMaps.Count; index += 1)
        {
            if (!maps.TryFind(this.StartMaps[index], out _))
            {
                throw ContentException.ForField(this.File, $"start_maps[{index}]", $"the map '{this.StartMaps[index].Value}' is no map of the content (D-1185)");
            }
        }
    }

    /// <summary>Fails when the flag file does not declare the goal flag (D-542, T-2).</summary>
    /// <param name="flags">The flag file of this build.</param>
    /// <exception cref="ContentException">The flag file holds no such id.</exception>
    public void RequireGoalOf(FlagList flags)
    {
        ArgumentNullException.ThrowIfNull(flags);

        flags.RequireDeclared(this.GoalFlag, this.File, "goal_flag");
    }

    private static List<ContentId> ReadStartMaps(ref ContentReader reader)
    {
        List<ContentId> maps = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, maps.Count))
        {
            maps.Add(reader.ReadContentId(GameMap.IdKind));
        }

        return maps;
    }
}
