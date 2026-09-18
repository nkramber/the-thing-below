using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The hash of the rule content. A run record and a snapshot hold it, and a load that reads
/// another value names both values and refuses the file (G-5, D-259).
/// </summary>
/// <remarks>
/// The hash covers the files of `content/rules/` and no other file (D-495, D-648). Thus an
/// art batch, a music batch, an effect batch, or a text edit never breaks a stored record or
/// a replay fixture.
/// <para>
/// The function is the SHA-256 that Core holds (D-644, D-645). The hash resists a content
/// file that somebody builds to collide, which a 64-bit hash does not.
/// </para>
/// </remarks>
public static class ContentHash
{
    /// <summary>The number of bytes that the length of a part takes.</summary>
    private const int LengthSize = 4;

    /// <summary>Gives the content hash of a set of content files.</summary>
    /// <param name="files">Every file of `content/`, in any order.</param>
    /// <returns>The hash as 64 lowercase hexadecimal characters.</returns>
    /// <exception cref="ContentException">Two files carry the same path.</exception>
    /// <remarks>
    /// The hash reads the rule files in ordinal order of their paths, so the order of the
    /// host never reaches the value (G-4, F-39). Each file contributes the length of its
    /// path, its path, the length of its bytes, and its bytes. Thus no two sets of files
    /// give one value through a shift of a boundary between two parts.
    /// </remarks>
    public static string Compute(IReadOnlyList<ContentFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        List<ContentFile> ruleFiles = [];
        foreach (ContentFile file in files)
        {
            if (ContentPaths.IsRuleFile(file.Path))
            {
                ruleFiles.Add(file);
            }
        }

        ruleFiles.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        RefusePathTwice(ruleFiles);

        byte[][] paths = new byte[ruleFiles.Count][];
        int size = 0;
        for (int index = 0; index < ruleFiles.Count; index++)
        {
            paths[index] = Encoding.UTF8.GetBytes(ruleFiles[index].Path);
            size = checked(size + LengthSize + paths[index].Length + LengthSize + ruleFiles[index].Bytes.Length);
        }

        byte[] body = new byte[size];
        int written = 0;
        for (int index = 0; index < ruleFiles.Count; index++)
        {
            written = WritePart(body, written, paths[index]);
            written = WritePart(body, written, ruleFiles[index].Bytes);
        }

        return Sha256.ComputeHex(body);
    }

    private static void RefusePathTwice(List<ContentFile> ruleFiles)
    {
        for (int index = 1; index < ruleFiles.Count; index++)
        {
            string path = ruleFiles[index].Path;
            if (string.CompareOrdinal(ruleFiles[index - 1].Path, path) == 0)
            {
                throw ContentException.ForFile(path, "the content set holds this path two times");
            }
        }
    }

    private static int WritePart(byte[] body, int written, byte[] part)
    {
        // Four bytes, most significant first, so the length reads the same whatever the byte
        // order of the machine is (T-7).
        body[written] = (byte)(part.Length >> 24);
        body[written + 1] = (byte)(part.Length >> 16);
        body[written + 2] = (byte)(part.Length >> 8);
        body[written + 3] = (byte)part.Length;

        part.CopyTo(body, written + LengthSize);
        return written + LengthSize + part.Length;
    }
}
