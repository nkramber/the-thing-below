using System;

namespace TheThingBelow.Storage;

/// <summary>The three systems that the game supports, for the folder rule (D-465, D-481).</summary>
public enum SaveSystem
{
    /// <summary>Windows on x86_64 (D-481).</summary>
    Windows,

    /// <summary>macOS on Apple silicon (D-481).</summary>
    MacOs,

    /// <summary>Linux on x86_64, and the Steam Deck (D-481, D-482).</summary>
    Linux,
}

/// <summary>
/// The folder of the game and the folder of the saves on each system (D-465, D-656).
/// </summary>
/// <remarks>
/// The folder carries the name of the repository, so a change of the title never moves a save
/// (D-465). Godot writes to the same folder, because the Godot project sets the custom user
/// folder to <see cref="Name"/>. A test of Tests reads the project file and compares the two
/// names, and Game compares the two resolved folders at the boot of every session (F-33, T-2).
/// <para>
/// Each rule below takes the environment of its system as an argument, so one test reads all
/// three folders on one machine (F-33). The path also takes the separator of that system, and
/// never the separator of the machine that runs the code.
/// </para>
/// </remarks>
public static class SaveFolder
{
    /// <summary>The name of the folder of the game inside the data folder of the person (D-465).</summary>
    public const string Name = "the-thing-below";

    /// <summary>The name of the folder of the saves inside the folder of the game (D-656).</summary>
    public const string SavesName = "saves";

    /// <summary>The variable that names the data folder on Windows.</summary>
    public const string WindowsVariable = "APPDATA";

    /// <summary>The variable that names the home folder of the person on macOS and Linux.</summary>
    public const string HomeVariable = "HOME";

    /// <summary>
    /// The variable of the XDG base directory specification that moves the data folder on
    /// Linux.
    /// </summary>
    public const string LinuxDataVariable = "XDG_DATA_HOME";

    /// <summary>Gives the folder of the game on the system that runs this process.</summary>
    /// <returns>The full path of the folder of the game (D-465).</returns>
    /// <exception cref="StorageException">
    /// The system is not one of the three, or the environment names no data folder (T-2).
    /// </exception>
    public static string OfThisSystem() => Of(ThisSystem(), ReadWindowsVariable(), ReadHome(), ReadLinuxVariable());

    /// <summary>Gives the folder of the saves on the system that runs this process.</summary>
    /// <returns>The full path of the folder of the saves (D-656).</returns>
    /// <exception cref="StorageException">
    /// The system is not one of the three, or the environment names no data folder (T-2).
    /// </exception>
    public static string SavesOfThisSystem() =>
        SavesOf(ThisSystem(), ReadWindowsVariable(), ReadHome(), ReadLinuxVariable());

    /// <summary>Gives the folder of the game on one system, from the environment of it.</summary>
    /// <param name="system">The system.</param>
    /// <param name="appData">The value of `APPDATA`, which Windows alone reads.</param>
    /// <param name="home">The value of `HOME`, which macOS and Linux read.</param>
    /// <param name="dataHome">The value of `XDG_DATA_HOME`, which Linux alone reads.</param>
    /// <returns>The full path of the folder of the game on that system.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static string Of(SaveSystem system, string? appData, string? home, string? dataHome) =>
        Join(system, DataFolder(system, appData, home, dataHome), Name);

    /// <summary>Gives the folder of the saves on one system, from the environment of it.</summary>
    /// <param name="system">The system.</param>
    /// <param name="appData">The value of `APPDATA`, which Windows alone reads.</param>
    /// <param name="home">The value of `HOME`, which macOS and Linux read.</param>
    /// <param name="dataHome">The value of `XDG_DATA_HOME`, which Linux alone reads.</param>
    /// <returns>The full path of the folder of the saves on that system.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static string SavesOf(SaveSystem system, string? appData, string? home, string? dataHome) =>
        Join(system, Of(system, appData, home, dataHome), SavesName);

    /// <summary>Gives the separator of the paths of one system.</summary>
    /// <param name="system">The system.</param>
    /// <returns>A backslash on Windows, and a slash on macOS and Linux.</returns>
    public static char SeparatorOf(SaveSystem system) => system == SaveSystem.Windows ? '\\' : '/';

    private static string DataFolder(SaveSystem system, string? appData, string? home, string? dataHome)
    {
        switch (system)
        {
            case SaveSystem.Windows:
                return Require(appData, WindowsVariable);

            case SaveSystem.MacOs:
                return Join(system, Require(home, HomeVariable), "Library", "Application Support");

            case SaveSystem.Linux:
                // The XDG base directory specification says that a relative value is invalid
                // and that the reader takes the default instead. Godot reads the variable by
                // the same rule, so the game and the engine land in one folder (D-465, F-33).
                if (!string.IsNullOrEmpty(dataHome) && dataHome[0] == '/')
                {
                    return dataHome;
                }

                return Join(system, Require(home, HomeVariable), ".local", "share");

            default:
                throw StorageException.ForSystem(
                    system.ToString(),
                    "the system is not one of the three systems of the game (D-481)");
        }
    }

    /// <summary>
    /// Joins each name to the folder with the separator of the system. A value of the
    /// environment can end with a separator, and the join then makes no empty step.
    /// </summary>
    private static string Join(SaveSystem system, string folder, params string[] names)
    {
        char separator = SeparatorOf(system);
        string path = folder.TrimEnd('/', '\\');
        if (path.Length == 0)
        {
            // The folder is the root of the file system, and the root is its own separator.
            path = separator.ToString();
        }

        foreach (string name in names)
        {
            path = path.EndsWith(separator) ? path + name : path + separator + name;
        }

        return path;
    }

    private static string Require(string? value, string variable)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw StorageException.ForVariable(
                variable,
                "the environment gives the variable no value, so the game cannot find the folder of the saves (D-465)");
        }

        return value;
    }

    private static string? ReadWindowsVariable() => Environment.GetEnvironmentVariable(WindowsVariable);

    private static string? ReadHome() => Environment.GetEnvironmentVariable(HomeVariable);

    private static string? ReadLinuxVariable() => Environment.GetEnvironmentVariable(LinuxDataVariable);

    private static SaveSystem ThisSystem()
    {
        if (OperatingSystem.IsWindows())
        {
            return SaveSystem.Windows;
        }

        if (OperatingSystem.IsMacOS())
        {
            return SaveSystem.MacOs;
        }

        if (OperatingSystem.IsLinux())
        {
            return SaveSystem.Linux;
        }

        throw StorageException.ForSystem(
            Environment.OSVersion.Platform.ToString(),
            "the system is not Windows, macOS, or Linux, and the game supports those three alone (D-481)");
    }
}
