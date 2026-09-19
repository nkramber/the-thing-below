using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Content;

/// <summary>
/// The `content-hash` command. It loads every file of `content/`, computes the content hash
/// of the rule files, and compares it with the committed file (G-5, D-495, D-648). The
/// `--write` option writes that file again after an intended change of a rule file.
/// </summary>
public static class ContentHashCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "content-hash";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that writes the hash file again.</summary>
    public const string WriteOption = "--write";

    /// <summary>The path of the committed hash file, under the root of the checkout.</summary>
    public const string HashFilePath = "TheThingBelow.Tests/identity/content-hash.txt";

    private static readonly IReadOnlyList<string> Header =
    [
        "# The content hash of this checkout (G-5, D-495, D-648). Every CI leg computes the",
        "# hash from the files of `content/rules/` and compares it with the line below. A",
        "# mismatch fails the build job on that leg.",
        "#",
        "# A PR that changes a rule file changes this line too, and the review reads the new",
        "# value. A file outside `content/rules/` never moves it.",
        "#",
        "# Write this file again with:",
        "#   dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj --",
        "#     content-hash --root . --write",
        string.Empty,
    ];

    /// <summary>Compares the hash with the committed file, or writes the file again.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the hash matches, and 1 when it does not.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        string root = ".";
        bool write = false;
        for (int index = 0; index < args.Count; index += 1)
        {
            string option = args[index];
            if (option == WriteOption)
            {
                write = true;
                continue;
            }

            if (option != RootOption)
            {
                errors.WriteLine(
                    $"Error: the option '{option}' is unknown. {Name} takes {RootOption} <path> and {WriteOption}.");
                return Program.FaultExitCode;
            }

            if (index + 1 >= args.Count)
            {
                errors.WriteLine($"Error: the option {RootOption} needs a value after it.");
                return Program.FaultExitCode;
            }

            string value = args[index + 1];
            if (OptionValue.ReportEmpty(RootOption, value, errors))
            {
                return Program.FaultExitCode;
            }

            root = value;
            index += 1;
        }

        try
        {
            return write ? WriteFile(root, output) : Compare(root, output, errors);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Reads the hash line of the committed file.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <returns>The expected content hash.</returns>
    /// <exception cref="InvalidDataException">The file holds no hash line (T-2).</exception>
    public static string ReadCommitted(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        string path = Path.Combine(root, HashFilePath.Replace('/', Path.DirectorySeparatorChar));
        foreach (string line in File.ReadAllLines(path))
        {
            string text = line.Trim();
            if (text.Length > 0 && !text.StartsWith('#'))
            {
                return text;
            }
        }

        throw new InvalidDataException($"The file '{path}' holds no hash line (T-2).");
    }

    private static int Compare(string root, TextWriter output, TextWriter errors)
    {
        string hash = Compute(root);
        string expected = ReadCommitted(root);
        if (string.CompareOrdinal(hash, expected) == 0)
        {
            output.WriteLine($"content-hash: {hash} matches the committed file.");
            return 0;
        }

        errors.WriteLine($"Error: the content hash is {hash}, and the committed file holds {expected}.");
        errors.WriteLine($"Run `{Name} {RootOption} {root} {WriteOption}` when the change of a rule file is intended.");
        return Program.FaultExitCode;
    }

    private static int WriteFile(string root, TextWriter output)
    {
        string hash = Compute(root);
        string path = Path.Combine(root, HashFilePath.Replace('/', Path.DirectorySeparatorChar));

        List<string> lines = new(Header) { hash };
        File.WriteAllText(path, string.Join('\n', lines) + "\n");

        output.WriteLine($"content-hash: wrote {path}");
        output.WriteLine($"  {hash}");
        output.WriteLine("content-hash: a changed hash breaks every stored record of the old content (G-5).");
        return 0;
    }

    /// <summary>
    /// Loads every content file and gives the hash of the rule files. The load runs first,
    /// so a broken content file fails here and never reaches the hash (T-2).
    /// </summary>
    private static string Compute(string root)
    {
        IReadOnlyList<ContentFile> files = ContentFolder.Read(root);
        ContentSet set = ContentSet.Load(files);
        return set.Hash;
    }
}
