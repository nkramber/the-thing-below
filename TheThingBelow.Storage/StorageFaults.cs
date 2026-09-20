using System;
using System.IO;

namespace TheThingBelow.Storage;

/// <summary>
/// The errors that the file system gives, which a catch of Storage turns into a
/// <see cref="StorageException"/> with the path (T-2, G-18).
/// </summary>
/// <remarks>
/// `NotSupportedException` comes from a path with a colon inside a name on Windows, which an
/// odd value of an environment variable can make. Every other error is a fault of the code.
/// </remarks>
internal static class StorageFaults
{
    /// <summary>Tells whether an error came from the file system, so a catch can add the path.</summary>
    /// <param name="fault">The error that the file code caught.</param>
    /// <returns>True for an error of the file system, and false for every other error.</returns>
    internal static bool IsFileFault(Exception fault) =>
        fault is IOException or UnauthorizedAccessException or NotSupportedException;
}
