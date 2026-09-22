using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// The particles of a hit on one kind of target, such as blood or sparks (D-186, D-879). No
/// rule reads a hit file, so the file lies outside the rule folder (D-495, D-522).
/// </summary>
/// <remarks>
/// The file names each character and each enemy that it serves, as an art file does, and a
/// rule file never names an effect (D-519). The load checks that each combatant takes one hit
/// file (<see cref="EffectContent"/>).
/// </remarks>
public sealed class HitEffect
{
    /// <summary>The folder of the hit files, under `content/`.</summary>
    public const string Folder = "effects/hits/";

    /// <summary>The kind of the id of each effect (D-646).</summary>
    public const string IdKind = "effect";

    private HitEffect(string file, ContentId id, IReadOnlyList<ContentId> serves, bool lit, IReadOnlyList<ParticleEmitter> emitters)
    {
        this.File = file;
        this.Id = id;
        this.Serves = serves;
        this.Lit = lit;
        this.Emitters = emitters;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the effect, such as `effect.blood`.</summary>
    public ContentId Id { get; }

    /// <summary>The characters and the enemies that this effect serves, in the order of the file (D-879).</summary>
    public IReadOnlyList<ContentId> Serves { get; }

    /// <summary>True when the scene light falls on the particles, as on a figure (D-183).</summary>
    public bool Lit { get; }

    /// <summary>The emitters of the burst, in the order of the file.</summary>
    public IReadOnlyList<ParticleEmitter> Emitters { get; }

    /// <summary>The count of particles of the whole burst, which the effect budget counts (D-523).</summary>
    public int Particles
    {
        get
        {
            int total = 0;
            foreach (ParticleEmitter emitter in this.Emitters)
            {
                total = checked(total + emitter.Amount);
            }

            return total;
        }
    }

    /// <summary>Tells whether a content path is a hit file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the hit files.</returns>
    public static bool IsHitFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one hit effect from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The effect.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static HitEffect Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        HitEffect effect = Read(ref reader);
        reader.ReadFileEnd();
        return effect;
    }

    private static HitEffect Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        List<ContentId>? serves = null;
        bool? lit = null;
        List<ParticleEmitter>? emitters = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "serves":
                    serves = ReadServes(ref reader);
                    break;
                case "lit":
                    lit = reader.ReadBoolean();
                    break;
                case "emitters":
                    emitters = ReadEmitters(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        List<ParticleEmitter> read = reader.Require(emitters, depth, "emitters");
        if (read.Count == 0)
        {
            throw reader.RefuseField(depth, "emitters", "the effect holds no emitter, and a hit shows one burst or more");
        }

        return new HitEffect(
            reader.File,
            reader.Require(id, depth, "id"),
            reader.Require(serves, depth, "serves"),
            reader.RequireValue(lit, depth, "lit"),
            read);
    }

    private static List<ContentId> ReadServes(ref ContentReader reader)
    {
        var served = new List<ContentId>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, served.Count))
        {
            ContentId id = reader.ReadContentId();
            foreach (ContentId before in served)
            {
                if (string.CompareOrdinal(before.Value, id.Value) == 0)
                {
                    throw reader.Refuse($"the effect names '{id.Value}' two times");
                }
            }

            served.Add(id);
        }

        return served;
    }

    private static List<ParticleEmitter> ReadEmitters(ref ContentReader reader)
    {
        var emitters = new List<ParticleEmitter>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, emitters.Count))
        {
            emitters.Add(ParticleEmitter.Read(ref reader));
        }

        return emitters;
    }
}
