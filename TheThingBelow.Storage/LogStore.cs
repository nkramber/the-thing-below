using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Logging;

namespace TheThingBelow.Storage;

/// <summary>
/// The log file of one session, in the logs folder beside the saves folder (D-179, D-658). It
/// writes one line for each entry that a step of Core returned, and Core makes the text of a
/// line (D-494).
/// </summary>
/// <remarks>
/// <see cref="Open"/> makes the file of the session, and every later call adds its lines to
/// the end of that file. A save takes the safe write of D-178, and a log file takes an append
/// instead: the file grows through a session, and a torn append leaves one partial last line.
/// <see cref="Read"/> then fails on that line and names it, so no partial line passes as a
/// whole one (D-178, T-2).
/// <para>
/// The store writes an entry of <see cref="Minimum"/> or above, so the file of a player holds
/// the changes that a report follows and not every step of a subsystem (D-660). The folder
/// keeps the newest <see cref="KeepCount"/> files (D-659).
/// </para>
/// </remarks>
public sealed class LogStore
{
    /// <summary>The name of the folder inside the folder of the game (D-658).</summary>
    public const string FolderName = "logs";

    /// <summary>The first part of the name of a log file (D-658).</summary>
    public const string FilePrefix = "session";

    /// <summary>The file type of a log file, which is JSON text, one object on each line (D-179).</summary>
    public const string FileExtension = ".json";

    /// <summary>The count of log files that the folder keeps (D-659).</summary>
    public const int KeepCount = 10;

    private static readonly UTF8Encoding TextEncoding = new(encoderShouldEmitUTF8Identifier: false);

    private readonly string folder;
    private readonly LogLevel minimum;
    private string? file;

    /// <summary>Makes a store of the log files in one folder.</summary>
    /// <param name="folder">The full path of the folder of the log files.</param>
    /// <param name="minimum">The lowest level that the file holds (D-660).</param>
    /// <exception cref="ArgumentException">The folder has no character (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The level is no level (T-2).</exception>
    public LogStore(string folder, LogLevel minimum)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);
        if (minimum < LogLevel.Debug || minimum > LogLevel.Error)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimum), minimum, $"The log holds no level with the number {(int)minimum} (D-179).");
        }

        this.folder = folder;
        this.minimum = minimum;
    }

    /// <summary>The folder that holds the log files.</summary>
    public string Folder => this.folder;

    /// <summary>The lowest level that the file of this store holds (D-660).</summary>
    public LogLevel Minimum => this.minimum;

    /// <summary>The full path of the file of this session, after <see cref="Open"/> ran.</summary>
    /// <exception cref="StorageException">No call to <see cref="Open"/> made the file (T-2).</exception>
    public string SessionFile => this.file ?? throw StorageException.ForPath(
        this.folder, "the session opened no log file, and a write needs the file of the session (T-2)");

    /// <summary>Makes the store of the log files of the person on this system (D-465, D-658).</summary>
    /// <param name="minimum">The lowest level that the file holds (D-660).</param>
    /// <returns>The store.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static LogStore OfThisSystem(LogLevel minimum) =>
        new(Path.Combine(SaveFolder.OfThisSystem(), FolderName), minimum);

    /// <summary>Makes the log file of this session, and removes the older files (D-658, D-659).</summary>
    /// <param name="time">The wall-clock time of the start of the session, in UTC (G-3).</param>
    /// <returns>The full path of the file.</returns>
    /// <exception cref="ArgumentException">The time is not a UTC time (T-2).</exception>
    /// <exception cref="StorageException">
    /// The session opened a file already, or the system refused the folder, the file, or a
    /// removal (T-2).
    /// </exception>
    /// <remarks>
    /// The file exists after this call, with no line in it. A session that writes no line thus
    /// leaves a file that names the session, and a boot that cannot write fails here and not
    /// at the first line (T-2).
    /// </remarks>
    public string Open(DateTime time)
    {
        if (this.file is not null)
        {
            throw StorageException.ForPath(
                this.file, "the session opened this log file already, and one session writes one file");
        }

        string stamp = TimeText.Stamp(time);
        FolderFiles.MakeFolder(this.folder);
        string path = FolderFiles.FreePath(this.folder, FilePrefix, stamp, FileExtension);

        try
        {
            File.WriteAllText(path, string.Empty, TextEncoding);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the game could not make the log file of the session", fault);
        }

        this.file = path;
        FolderFiles.KeepNewest(this.folder, FilePrefix, FileExtension, KeepCount, path);
        return path;
    }

    /// <summary>Adds the entries of one step, or of one frame, to the file (D-179).</summary>
    /// <param name="entries">The entries that Core returned, in the order of the steps.</param>
    /// <param name="time">The wall-clock time of the host, in UTC, which every line carries (G-3).</param>
    /// <returns>The count of lines that the file holds now, which drops the levels below the minimum.</returns>
    /// <exception cref="ArgumentNullException">The list or one entry is null (T-2).</exception>
    /// <exception cref="ArgumentException">The time is not a UTC time (T-2).</exception>
    /// <exception cref="StorageException">No file is open, or the system refused the write (T-2).</exception>
    public int Write(IReadOnlyList<LogEntry> entries, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(entries);

        string path = this.SessionFile;
        string moment = TimeText.Moment(time);

        StringBuilder text = new();
        int count = 0;
        foreach (LogEntry entry in entries)
        {
            ArgumentNullException.ThrowIfNull(entry);
            if (entry.Level < this.minimum)
            {
                continue;
            }

            text.Append(LogLineText.Write(new LogLine(moment, entry))).Append('\n');
            count += 1;
        }

        if (count == 0)
        {
            return 0;
        }

        try
        {
            File.AppendAllText(path, text.ToString(), TextEncoding);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, $"the game could not add {count} lines to the log file", fault);
        }

        return count;
    }

    /// <summary>Reads every line of the file of this session (D-179).</summary>
    /// <returns>The lines, in the order of the file.</returns>
    /// <exception cref="StorageException">No file is open, or the system refused the read (T-2).</exception>
    /// <exception cref="TheThingBelow.Core.Content.ContentException">A line is not a log line (T-2).</exception>
    public IReadOnlyList<LogLine> Read()
    {
        string path = this.SessionFile;

        string text;
        try
        {
            text = Encoding.UTF8.GetString(File.ReadAllBytes(path));
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the game could not read the log file of the session", fault);
        }

        List<LogLine> lines = [];
        foreach (string line in text.Split('\n'))
        {
            if (line.Length > 0)
            {
                lines.Add(LogLineText.Read(line));
            }
        }

        return lines;
    }

    /// <summary>Gives the names of the log files, the newest one first (D-659).</summary>
    /// <returns>The names with no folder. The list is empty when the folder is absent.</returns>
    /// <exception cref="StorageException">The system refused the read (T-2).</exception>
    public IReadOnlyList<string> Names() => FolderFiles.Names(this.folder, FilePrefix, FileExtension);
}
