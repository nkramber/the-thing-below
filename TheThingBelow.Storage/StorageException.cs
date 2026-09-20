using System;

namespace TheThingBelow.Storage;

/// <summary>
/// The error of a file of the game: an absent folder of the system, a file that no read can
/// reach, or a write that the system refused (T-2, D-494).
/// </summary>
/// <remarks>
/// Every message names the path, because a person reads this error in a report and cannot
/// see the folders of the machine (T-2, G-18).
/// </remarks>
public sealed class StorageException : Exception
{
    private StorageException(string message, string path)
        : base(message) => this.Path = path;

    private StorageException(string message, string path, Exception inner)
        : base(message, inner) => this.Path = path;

    /// <summary>The path of the file or the folder, which the message also names.</summary>
    public string Path { get; }

    /// <summary>Makes the error of one path.</summary>
    /// <param name="path">The path of the file or the folder.</param>
    /// <param name="message">What the code found.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The path or the message has no character (T-2).</exception>
    public static StorageException ForPath(string path, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new StorageException(Describe(message, path), path);
    }

    /// <summary>Makes the error of one path, over the error that the system gave.</summary>
    /// <param name="path">The path of the file or the folder.</param>
    /// <param name="message">What the code did when the system refused it.</param>
    /// <param name="inner">The error of the system.</param>
    /// <returns>The error, ready to throw.</returns>
    /// <exception cref="ArgumentException">The path or the message has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The inner error is null (T-2).</exception>
    public static StorageException ForPath(string path, string message, Exception inner)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(inner);

        return new StorageException(Describe(message, path), path, inner);
    }

    /// <summary>Makes the error of an environment variable that names no folder (D-465).</summary>
    /// <param name="variable">The name of the variable, such as `HOME`.</param>
    /// <param name="message">What the code found.</param>
    /// <returns>The error, ready to throw. Its path is the name of the variable.</returns>
    /// <exception cref="ArgumentException">The variable or the message has no character (T-2).</exception>
    public static StorageException ForVariable(string variable, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(variable);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new StorageException($"{message} (the variable '{variable}')", variable);
    }

    /// <summary>Makes the error of a system that the game does not support (D-481).</summary>
    /// <param name="system">The name of the system, as the code read it.</param>
    /// <param name="message">What the code found.</param>
    /// <returns>The error, ready to throw. Its path is the name of the system.</returns>
    /// <exception cref="ArgumentException">The system or the message has no character (T-2).</exception>
    public static StorageException ForSystem(string system, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(system);
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new StorageException($"{message} (the system '{system}')", system);
    }

    private static string Describe(string message, string path) => $"{message} (the path '{path}')";
}
