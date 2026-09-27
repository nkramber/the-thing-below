using System;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The place of the merge commit of a promotion against the commit of the newest night of `main`,
/// as the compare API of GitHub gives it (D-1202). `main` holds a linear history, so the two
/// commits never diverge.
/// </summary>
public enum NightOrder
{
    /// <summary>No order applies: no promotion, or no night of `main`.</summary>
    None,

    /// <summary>The merge commit comes after the commit of the night, so the promotion is newer.</summary>
    Ahead,

    /// <summary>The merge commit comes before the commit of the night, so the night is newer.</summary>
    Behind,

    /// <summary>The night played the merge commit, and a real night wins over a promotion.</summary>
    Identical,
}

/// <summary>
/// One promotion of D-1202: a green night of a PR that `night-promote` keeps as the newest
/// evidence of `main` at the merge commit of that PR. The artifact `night-promotion` holds the
/// file of the promotion, the facts of the night run, and each night record of the branch night.
/// </summary>
/// <param name="MergeCommit">The full hash of the merge commit on `main`.</param>
/// <param name="PullRequest">The number of the merged PR.</param>
/// <param name="FailedRun">The id of the failed night of `main` that the promotion covers.</param>
/// <param name="Night">The branch night, with its facts and its records.</param>
public sealed record NightPromotion(string MergeCommit, long PullRequest, long FailedRun, NightEvidence Night)
{
    /// <summary>The name of the artifact of a promotion.</summary>
    public const string ArtifactName = "night-promotion";

    /// <summary>The name of the file of the promotion, inside the folder of the artifact.</summary>
    public const string PromotionFile = "promotion.json";

    private static readonly string[] Fields = ["merge-commit", "pull-request", "failed-run"];

    /// <summary>Gives the order of a compare status of the GitHub API.</summary>
    /// <param name="status">The status: `ahead`, `behind`, or `identical`.</param>
    /// <returns>The order.</returns>
    /// <exception cref="InvalidOperationException">The status is another text, such as `diverged` (T-2).</exception>
    public static NightOrder OrderOf(string status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return status switch
        {
            "ahead" => NightOrder.Ahead,
            "behind" => NightOrder.Behind,
            "identical" => NightOrder.Identical,
            _ => throw new InvalidOperationException($"The compare status '{status}' of the merge commit of the promotion is not ahead, behind, or identical. `main` holds a linear history, so the two commits cannot diverge (D-1202, T-2)."),
        };
    }

    /// <summary>Reads the folder of a promotion that `gh run download` wrote.</summary>
    /// <param name="folder">The folder.</param>
    /// <returns>The promotion, or null when the folder holds no file of a promotion, because no promotion exists.</returns>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist: the job wrote no folder (T-2).</exception>
    /// <exception cref="InvalidOperationException">A file of the folder holds no valid facts, or the night is absent.</exception>
    public static NightPromotion? Read(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"The folder of the promotion '{folder}' does not exist. The job writes it, and it can hold no promotion (T-2).");
        }

        string path = Path.Combine(folder, PromotionFile);
        if (!File.Exists(path))
        {
            return null;
        }

        string where = $"the promotion '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        NightJson.RequireFields(root, where, Fields);
        string merge = NightJson.Text(root, "merge-commit", where);
        if (!NightJson.IsCommit(merge))
        {
            throw new InvalidOperationException($"The field 'merge-commit' of {where} holds '{merge}', and a promotion needs a full commit hash.");
        }

        NightEvidence night = NightEvidence.Read(folder)
            ?? throw new InvalidOperationException($"{where} holds no facts of its night run '{NightEvidence.RunFile}', and a promotion keeps them (D-1202, T-2).");
        return new NightPromotion(merge, NightJson.Whole(root, "pull-request", where, 1), NightJson.Whole(root, "failed-run", where, 1), night);
    }

    /// <summary>Writes the promotion: the file of the promotion, and a copy of the folder of the branch night.</summary>
    /// <param name="nightFolder">The folder of the branch night: the facts of the run and each record.</param>
    /// <param name="folder">The new folder of the artifact.</param>
    public void Write(string nightFolder, string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(nightFolder);
        ArgumentException.ThrowIfNullOrEmpty(folder);

        Directory.CreateDirectory(folder);
        File.Copy(Path.Combine(nightFolder, NightEvidence.RunFile), Path.Combine(folder, NightEvidence.RunFile));
        foreach (NightRecord record in this.Night.Records)
        {
            string artifact = NightLegs.ArtifactOf(record.Leg);
            Directory.CreateDirectory(Path.Combine(folder, artifact));
            File.Copy(Path.Combine(nightFolder, artifact, NightLegs.FileOf(record.Leg)), Path.Combine(folder, artifact, NightLegs.FileOf(record.Leg)));
        }

        using FileStream stream = File.Create(Path.Combine(folder, PromotionFile));
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WriteString("merge-commit", this.MergeCommit);
        writer.WriteNumber("pull-request", this.PullRequest);
        writer.WriteNumber("failed-run", this.FailedRun);
        writer.WriteEndObject();
    }
}

/// <summary>
/// The evidence of `main` that the night gate and the promotion read: the newest completed night of
/// `main`, the newest promotion, and the order of their commits (D-1202).
/// </summary>
/// <param name="Night">The newest completed night of `main`, or null when none exists.</param>
/// <param name="Promotion">The newest promotion, or null when none exists.</param>
/// <param name="Order">The order of the merge commit of the promotion against the commit of the night.</param>
public sealed record NightMainFacts(NightEvidence? Night, NightPromotion? Promotion, NightOrder Order)
{
    /// <summary>Gets a value indicating whether the promotion is the newest evidence of `main`. A night on the merge commit or later wins (D-1202).</summary>
    public bool PromotionWins => this.Promotion is not null && (this.Night is null || this.Order == NightOrder.Ahead);

    /// <summary>Makes the facts, and checks that an order comes with a night and a promotion alone.</summary>
    /// <param name="night">The newest completed night of `main`, or null.</param>
    /// <param name="promotion">The newest promotion, or null.</param>
    /// <param name="order">The order of the two commits, or <see cref="NightOrder.None"/> when either is absent.</param>
    /// <returns>The facts.</returns>
    /// <exception cref="InvalidOperationException">A night and a promotion come with no order, or an order comes with no pair (T-2).</exception>
    public static NightMainFacts Of(NightEvidence? night, NightPromotion? promotion, NightOrder order)
    {
        bool both = night is not null && promotion is not null;
        if (both && order == NightOrder.None)
        {
            throw new InvalidOperationException($"The night run {night!.Run.Id} of `main` and the promotion at {promotion!.MergeCommit} come with no order of their commits, and the gate needs one to find the newer (D-1202, T-2).");
        }

        if (!both && order != NightOrder.None)
        {
            throw new InvalidOperationException($"The order '{order}' comes with no pair of a night of `main` and a promotion (D-1202, T-2).");
        }

        return new NightMainFacts(night, promotion, order);
    }
}
