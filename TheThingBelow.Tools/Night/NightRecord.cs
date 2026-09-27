using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using TheThingBelow.Tools.Bots;

namespace TheThingBelow.Tools.Night;

/// <summary>The count of each end of the runs of one policy in a night, and their wall time (D-509).</summary>
/// <param name="Policy">The policy.</param>
/// <param name="Runs">The count of runs of the policy (D-1191). The four ends add up to it.</param>
/// <param name="Complete">The count of runs that reached the goal.</param>
/// <param name="Softlock">The count of runs that ended as a softlock.</param>
/// <param name="Crash">The count of runs that ended as a crash.</param>
/// <param name="Budget">The count of runs that played the tick budget.</param>
/// <param name="Seconds">The wall time of the runs, in whole seconds, for M-3. No rule reads it.</param>
public sealed record NightPolicyCounts(BotPolicyKind Policy, long Runs, long Complete, long Softlock, long Crash, long Budget, long Seconds)
{
    /// <summary>Gets the sum of the four ends.</summary>
    public long Ended => checked(this.Complete + this.Softlock + this.Crash + this.Budget);

    /// <summary>Gets a value that tells whether a run ended as a softlock or a crash (T-2).</summary>
    public bool Fails => this.Softlock > 0 || this.Crash > 0;
}

/// <summary>
/// The night record: the result file that one leg of a night uploads as an artifact of its run
/// (D-509). It names the commit, the leg, the seed range, the count of runs and of each end of each policy,
/// and the status.
/// </summary>
/// <param name="Commit">The full hash of the commit that the night played.</param>
/// <param name="Leg">The runner label of the leg.</param>
/// <param name="FirstSeed">The first seed of the range. Each policy plays the same range (D-1190).</param>
/// <param name="Policies">The counts of each policy: greedy, then random.</param>
public sealed record NightRecord(string Commit, string Leg, ulong FirstSeed, IReadOnlyList<NightPolicyCounts> Policies)
{
    /// <summary>The status of a record with no softlock and no crash.</summary>
    public const string Success = "success";

    /// <summary>The status of a record with a softlock or a crash.</summary>
    public const string Failure = "failure";

    private static readonly string[] RecordFields = ["commit", "leg", "firstSeed", "status", "policies"];
    private static readonly string[] PolicyFields = ["policy", "runs", "complete", "softlock", "crash", "budget", "seconds"];

    /// <summary>The order of the policies in a record.</summary>
    public static readonly IReadOnlyList<BotPolicyKind> PolicyOrder = [BotPolicyKind.Greedy, BotPolicyKind.Random];

    /// <summary>Gets the status: <see cref="Failure"/> when a run of a policy failed, else <see cref="Success"/>.</summary>
    public string Status
    {
        get
        {
            foreach (NightPolicyCounts counts in this.Policies)
            {
                if (counts.Fails)
                {
                    return Failure;
                }
            }

            return Success;
        }
    }

    /// <summary>Writes the record as JSON with a line end of `\n` on every system.</summary>
    /// <param name="path">The path of the file.</param>
    public void Write(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        // `JsonSerializer` needs runtime reflection, which the solution turns off (D-647, F-36).
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            writer.WriteStartObject();
            writer.WriteString("commit", this.Commit);
            writer.WriteString("leg", this.Leg);
            writer.WriteNumber("firstSeed", this.FirstSeed);
            writer.WriteString("status", this.Status);
            writer.WriteStartArray("policies");
            foreach (NightPolicyCounts counts in this.Policies)
            {
                writer.WriteStartObject();
                writer.WriteString("policy", BotPolicyKinds.NameOf(counts.Policy));
                writer.WriteNumber("runs", counts.Runs);
                writer.WriteNumber("complete", counts.Complete);
                writer.WriteNumber("softlock", counts.Softlock);
                writer.WriteNumber("crash", counts.Crash);
                writer.WriteNumber("budget", counts.Budget);
                writer.WriteNumber("seconds", counts.Seconds);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        stream.WriteByte((byte)'\n');
        File.WriteAllBytes(path, stream.ToArray());
    }

    /// <summary>
    /// Reads a record, and checks each field: the commit, the leg, the policies in their order,
    /// the count of runs of each policy, and a status that matches the counts (T-2).
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The record.</returns>
    /// <exception cref="InvalidOperationException">The file holds no valid record.</exception>
    public static NightRecord Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string where = $"the night record '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        NightJson.RequireFields(root, where, RecordFields);

        string commit = NightJson.Text(root, "commit", where);
        if (!NightJson.IsCommit(commit))
        {
            throw new InvalidOperationException($"The field 'commit' of {where} holds '{commit}', and the night gate needs a full commit hash.");
        }

        NightRecord record = new(commit, NightJson.Text(root, "leg", where), NightJson.Seed(root, "firstSeed", where), PoliciesOf(root, where));
        string status = NightJson.Text(root, "status", where);
        if (!string.Equals(status, record.Status, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"The field 'status' of {where} holds '{status}', and its counts give '{record.Status}'.");
        }

        return record;
    }

    private static List<NightPolicyCounts> PoliciesOf(JsonElement root, string where)
    {
        JsonElement list = root.GetProperty("policies");
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() != PolicyOrder.Count)
        {
            throw new InvalidOperationException($"The field 'policies' of {where} holds '{list}', and the night gate needs one entry for each policy: greedy, then random.");
        }

        List<NightPolicyCounts> policies = [];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            string itemWhere = $"{where}, policy {index.ToString(CultureInfo.InvariantCulture)}";
            NightJson.RequireFields(item, itemWhere, PolicyFields);
            string name = NightJson.Text(item, "policy", itemWhere);
            if (!BotPolicyKinds.TryOf(name, out BotPolicyKind policy) || policy != PolicyOrder[index])
            {
                throw new InvalidOperationException($"The field 'policy' of {itemWhere} holds '{name}', and the night gate needs '{BotPolicyKinds.NameOf(PolicyOrder[index])}' there.");
            }

            NightPolicyCounts counts = new(
                policy,
                NightJson.Whole(item, "runs", itemWhere, 1),
                NightJson.Whole(item, "complete", itemWhere, 0),
                NightJson.Whole(item, "softlock", itemWhere, 0),
                NightJson.Whole(item, "crash", itemWhere, 0),
                NightJson.Whole(item, "budget", itemWhere, 0),
                NightJson.Whole(item, "seconds", itemWhere, 0));
            if (counts.Ended != counts.Runs)
            {
                throw new InvalidOperationException($"The four ends of {itemWhere} count {counts.Ended.ToString(CultureInfo.InvariantCulture)} runs, and its field 'runs' gives {counts.Runs.ToString(CultureInfo.InvariantCulture)}.");
            }

            policies.Add(counts);
            index += 1;
        }

        return policies;
    }
}
