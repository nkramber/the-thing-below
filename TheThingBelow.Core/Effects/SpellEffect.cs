using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Core.Effects;

/// <summary>The shape of the light of a spell over its length (D-1032).</summary>
public enum FlashShape
{
    /// <summary>The light jumps to its peak at the start and fades to the end.</summary>
    Spike,

    /// <summary>The light rises to its peak in the middle and fades to the end.</summary>
    Swell,

    /// <summary>The light peaks two times: at the start and in the middle, and it fades after each peak.</summary>
    Double,
}

/// <summary>
/// The flash of one spell: a point light at the target with its shape, a tint of the frame, and a
/// burst of particles (D-183, D-186, D-878, D-1032). No rule reads a spell file, so the file
/// lies outside the rule folder (D-495, D-522).
/// </summary>
/// <remarks>
/// The file names each ability that it serves, as an art file does, and a rule file never names
/// an effect (D-519). The load checks that each form of each rite takes one spell file, and that
/// no two spell files show one look (<see cref="EffectContent"/>). The tint of the frame rises
/// with the first peak alone, so one spell gives one flash of the whole frame (D-863).
/// </remarks>
public sealed class SpellEffect
{
    /// <summary>The folder of the spell files, under `content/`.</summary>
    public const string Folder = "effects/spells/";

    /// <summary>The fewest ticks of a flash.</summary>
    public const int FewestTicks = 4;

    /// <summary>The most strength of the tint of the frame, in basis points: a quarter of the color (D-186).</summary>
    public const int MostTint = 2500;

    private SpellEffect(
        string file,
        ContentId id,
        IReadOnlyList<ContentId> serves,
        FlashShape shape,
        int lengthTicks,
        PointLightValues light,
        LightColor tint,
        IReadOnlyList<ParticleEmitter> emitters)
    {
        this.File = file;
        this.Id = id;
        this.Serves = serves;
        this.Shape = shape;
        this.LengthTicks = lengthTicks;
        this.Light = light;
        this.Tint = tint;
        this.Emitters = emitters;
        this.Burst = HitEffect.OfSpell(file, id, serves, emitters);
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the effect, such as `effect.spell_cinder`.</summary>
    public ContentId Id { get; }

    /// <summary>The abilities that this flash serves: forms of rites (D-1032).</summary>
    public IReadOnlyList<ContentId> Serves { get; }

    /// <summary>The shape of the light over the length.</summary>
    public FlashShape Shape { get; }

    /// <summary>The length of the flash, in ticks. The flash keeps its own ticks at each message speed.</summary>
    public int LengthTicks { get; }

    /// <summary>The point light at the peak: its color, strength, range, and height (D-183).</summary>
    public PointLightValues Light { get; }

    /// <summary>The tint of the frame at the peak: a palette key and a strength up to <see cref="MostTint"/>.</summary>
    public LightColor Tint { get; }

    /// <summary>The emitters of the burst at the target, in the order of the file.</summary>
    public IReadOnlyList<ParticleEmitter> Emitters { get; }

    /// <summary>The burst of the flash in the form of a hit, which the screen draws with the hit bursts (D-879, D-1032).</summary>
    public HitEffect Burst { get; }

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

    /// <summary>
    /// The look of the flash as one text: every value but the id, the serves, and the comment.
    /// Two spell files with one look fail the load (D-1032).
    /// </summary>
    public string Look
    {
        get
        {
            var parts = new List<string>
            {
                FlashShapes.NameOf(this.Shape),
                Number(this.LengthTicks),
                this.Light.Color.Key.ToString(),
                Number(this.Light.Color.Strength),
                Number(this.Light.Range),
                Number(this.Light.Height),
                this.Tint.Key.ToString(),
                Number(this.Tint.Strength),
            };
            foreach (ParticleEmitter emitter in this.Emitters)
            {
                // A loop, because a spread of the list makes the compiler call `System.Linq` (G-1).
                var colors = new char[emitter.Colors.Count];
                for (int index = 0; index < colors.Length; index += 1)
                {
                    colors[index] = emitter.Colors[index];
                }

                parts.Add(string.Join(",", Number(emitter.Amount), Number(emitter.LifetimeTicks), new string(colors), Number(emitter.Size), Number(emitter.Area), Number(emitter.Direction), Number(emitter.Spread), Number(emitter.SlowestSpeed), Number(emitter.FastestSpeed), Number(emitter.Gravity)));
            }

            return string.Join("|", parts);
        }
    }

    /// <summary>
    /// Gives the strength of the light at one tick of the flash, in basis points of its peak
    /// strength. It falls to zero at the end of the length (D-1032).
    /// </summary>
    /// <param name="tick">The ticks since the start of the flash.</param>
    /// <returns>From 0 to 10000. A tick past the length gives zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public int LightAt(int tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        if (tick >= this.LengthTicks)
        {
            return 0;
        }

        int half = this.LengthTicks / 2;
        return this.Shape switch
        {
            FlashShape.Spike => Fall(tick, this.LengthTicks),
            FlashShape.Swell => tick < half ? Rise(tick, half) : Fall(tick - half, this.LengthTicks - half),
            _ => tick < half ? Fall(tick, half) : Fall(tick - half, this.LengthTicks - half),
        };
    }

    /// <summary>
    /// Gives the strength of the tint of the frame at one tick, in basis points of its peak
    /// strength. The tint follows the first peak of the light alone, so a double pulse tints
    /// the frame one time (D-863).
    /// </summary>
    /// <param name="tick">The ticks since the start of the flash.</param>
    /// <returns>From 0 to 10000.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public int TintAt(int tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        return this.Shape == FlashShape.Double && tick >= this.LengthTicks / 2 ? 0 : this.LightAt(tick);
    }

    /// <summary>Tells whether a content path is a spell file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the spell files.</returns>
    public static bool IsSpellFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one spell effect from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The effect.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static SpellEffect Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        SpellEffect effect = Read(ref reader);
        reader.ReadFileEnd();
        return effect;
    }

    private static int Fall(int tick, int length) => length <= 0 ? 0 : BasisPoints.One * (length - tick) / length;

    private static int Rise(int tick, int length) => length <= 0 ? BasisPoints.One : BasisPoints.One * (tick + 1) / length;

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static SpellEffect Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        List<ContentId>? serves = null;
        string? shape = null;
        int? length = null;
        PointLightValues? light = null;
        LightColor? tint = null;
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
                    id = reader.ReadContentId(HitEffect.IdKind);
                    break;
                case "serves":
                    serves = ReadServes(ref reader);
                    break;
                case "shape":
                    shape = reader.ReadString();
                    break;
                case "length_ticks":
                    length = reader.ReadInt();
                    break;
                case "light":
                    light = ReadLight(ref reader);
                    break;
                case "tint":
                    tint = ReadTint(ref reader);
                    break;
                case "emitters":
                    emitters = ReadEmitters(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        string readShape = reader.Require(shape, depth, "shape");
        if (!FlashShapes.TryOf(readShape, out FlashShape flash))
        {
            throw reader.RefuseField(depth, "shape", $"the shape '{readShape}' is not one of {FlashShapes.EveryName} (D-1032)");
        }

        int readLength = reader.RequireInt(length, depth, "length_ticks");
        if (readLength < FewestTicks || readLength > ParticleEmitter.MostLifetimeTicks)
        {
            throw reader.RefuseField(depth, "length_ticks", $"the length is {readLength} ticks, and a flash takes {FewestTicks} to {ParticleEmitter.MostLifetimeTicks} (D-186)");
        }

        List<ContentId> readServes = reader.Require(serves, depth, "serves");
        if (readServes.Count == 0)
        {
            throw reader.RefuseField(depth, "serves", "the flash serves no ability, and each spell file serves one or more (D-1032)");
        }

        List<ParticleEmitter> readEmitters = reader.Require(emitters, depth, "emitters");
        if (readEmitters.Count == 0)
        {
            throw reader.RefuseField(depth, "emitters", "the flash holds no emitter, and each flash shows a burst (D-1032)");
        }

        return new SpellEffect(
            reader.File,
            reader.Require(id, depth, "id"),
            readServes,
            flash,
            readLength,
            reader.Require(light, depth, "light"),
            reader.Require(tint, depth, "tint"),
            readEmitters);
    }

    private static PointLightValues ReadLight(ref ContentReader reader)
    {
        string? color = null;
        int? strength = null;
        int? range = null;
        int? height = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "color":
                    color = reader.ReadString();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                case "range":
                    range = reader.ReadInt();
                    break;
                case "height":
                    height = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        LightColor made = LightValues.BuildColor(ref reader, depth, color, strength, LightValues.MostLightStrength);
        return LightValues.BuildPoint(ref reader, depth, made, range, height);
    }

    private static LightColor ReadTint(ref ContentReader reader)
    {
        string? color = null;
        int? strength = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "color":
                    color = reader.ReadString();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return LightValues.BuildColor(ref reader, depth, color, strength, MostTint);
    }

    private static List<ContentId> ReadServes(ref ContentReader reader)
    {
        var served = new List<ContentId>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, served.Count))
        {
            ContentId id = reader.ReadContentId("ability");
            foreach (ContentId before in served)
            {
                if (string.CompareOrdinal(before.Value, id.Value) == 0)
                {
                    throw reader.Refuse($"the flash names '{id.Value}' two times");
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

/// <summary>The names of the shapes of a flash in content (D-1032).</summary>
public static class FlashShapes
{
    /// <summary>The names of every shape, for an error (T-2).</summary>
    public const string EveryName = "spike, swell, double";

    /// <summary>Gives the name of a shape in content.</summary>
    /// <param name="shape">The shape.</param>
    /// <returns>The name, such as `spike`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no shape (T-2).</exception>
    public static string NameOf(FlashShape shape) => shape switch
    {
        FlashShape.Spike => "spike",
        FlashShape.Swell => "swell",
        FlashShape.Double => "double",
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "the value names no shape of a flash (D-1032)"),
    };

    /// <summary>Finds the shape of a name.</summary>
    /// <param name="name">The name, such as `spike`.</param>
    /// <param name="shape">The shape, when the name is one.</param>
    /// <returns>True when the name names a shape.</returns>
    public static bool TryOf(string name, out FlashShape shape)
    {
        foreach (FlashShape candidate in new[] { FlashShape.Spike, FlashShape.Swell, FlashShape.Double })
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                shape = candidate;
                return true;
            }
        }

        shape = FlashShape.Spike;
        return false;
    }
}
