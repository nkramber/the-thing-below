using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tools.Screenplay;

/// <summary>
/// The `screenplay` command (D-173, G-25). It loads the content of the head, finds each story
/// scene that the PR changes against the base folder, and writes the screenplay of the batch
/// into the Screenplay section of a PR body file (D-1015, D-1016, D-1017).
/// </summary>
/// <remarks>
/// The session fills the base folder from the base commit, and it sends the body file with
/// `gh pr edit --body-file`. The `make screenplay` target does the first step. The tool reads
/// no git data and no network, so a test runs it on two folders.
/// </remarks>
public static class ScreenplayCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "screenplay";

    /// <summary>The option that names the root of the checkout of the head.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the base folder, which holds the `content` folder of the base commit.</summary>
    public const string BaseOption = "--base";

    /// <summary>The option that names the PR body file, which the command writes again.</summary>
    public const string BodyOption = "--body";

    /// <summary>Writes the Screenplay section into the body file.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the body file holds the new section, and 1 on a fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, BaseOption, BodyOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? baseRoot = options.Value(BaseOption);
        string? bodyFile = options.Value(BodyOption);
        if (baseRoot is null || bodyFile is null)
        {
            errors.WriteLine($"Error: {Name} needs {BaseOption} <folder> and {BodyOption} <file> (D-1015, D-1016).");
            return Program.FaultExitCode;
        }

        try
        {
            return Write(root, baseRoot, bodyFile, output);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException or InvalidDataException)
        {
            // `InvalidDataException` does not derive from `IOException`, so a body with a
            // broken marker needs its own name in this list (T-2).
            errors.WriteLine($"Error: {Name} stopped on the root '{root}', the base '{baseRoot}', and the body '{bodyFile}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Gives the Screenplay section of one batch, without the two marker lines.</summary>
    /// <param name="batch">The batch.</param>
    /// <param name="strings">The string table of the head.</param>
    /// <returns>The Markdown text, which ends with a line end.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">A step names a string id that the table lacks (G-7, T-2).</exception>
    public static string Section(ScreenplayBatch batch, StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(strings);

        var text = new StringBuilder("## Screenplay\n\n");
        if (batch.Changed.Count == 0 && batch.Removed.Count == 0)
        {
            text.Append("No story scene changes in this PR (D-1015).\n");
            return text.ToString();
        }

        text.Append($"Changed story scenes: {batch.Changed.Count}. Removed story scene files: {batch.Removed.Count} (D-1015).\n");
        foreach (StoryScene scene in batch.Changed)
        {
            text.Append('\n');
            text.Append(ScreenplayText.Write(scene, strings));
        }

        foreach (string removed in batch.Removed)
        {
            text.Append($"\nRemoved: `{removed}`\n");
        }

        return text.ToString();
    }

    private static int Write(string root, string baseRoot, string bodyFile, TextWriter output)
    {
        // The full load checks every id of the head first, so a line with an absent string
        // fails with its story scene, its step, and its id (G-7, T-2).
        IReadOnlyList<ContentFile> head = ContentFolder.Read(root);
        ContentSet set = ContentSet.Load(head);

        IReadOnlyList<ContentFile> baseFiles = ContentFolder.Read(baseRoot);
        ScreenplayBatch batch = ScreenplayBatch.Find(head, set.Story.Scenes, set.Strings, baseFiles);
        string section = Section(batch, set.Strings);

        string body = File.ReadAllText(bodyFile);
        string result = PullRequestBody.Insert(body, section);
        File.WriteAllText(bodyFile, result);

        output.WriteLine($"{Name}: changed story scenes: {batch.Changed.Count}. Removed story scene files: {batch.Removed.Count}.");
        output.WriteLine($"{Name}: wrote {bodyFile}, with {result.Length} of {PullRequestBody.MaxCharacters} characters.");
        return 0;
    }
}
