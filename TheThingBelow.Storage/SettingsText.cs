using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Storage;

/// <summary>The text of the settings file: one JSON object with an indent (D-860).</summary>
/// <remarks>
/// The reader is the content reader of Core, so an unknown field, an absent field, and a
/// repeated field each fail the load with the file and the field (D-570, G-6, T-2). No
/// setting takes a silent default.
/// <para>
/// The format version is the first field, so a later format can choose its migration step
/// before it reads the rest (D-570, D-869). The writer puts a line feed at the end of each
/// line on every system, so one set of settings gives the same bytes on each machine.
/// </para>
/// </remarks>
public static class SettingsText
{
    /// <summary>Writes the settings as text.</summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The text of the file, with a line feed at the end.</returns>
    /// <exception cref="ArgumentNullException">The settings are null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A value is outside its range (T-2).</exception>
    public static string Write(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Check();

        ArrayBufferWriter<byte> bytes = new();
        JsonWriterOptions options = new() { Indented = true, NewLine = "\n", SkipValidation = false };
        using (Utf8JsonWriter writer = new(bytes, options))
        {
            writer.WriteStartObject();
            writer.WriteNumber("format", SettingsFormat.Current);
            WriteDisplay(writer, settings.Display);
            WriteAudio(writer, settings.Audio);
            WriteControls(writer, settings.Controls);
            WriteBattle(writer, settings.Battle);
            WriteAccess(writer, settings.Access);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(bytes.WrittenSpan) + "\n";
    }

    /// <summary>Reads the settings from the text of the file.</summary>
    /// <param name="text">The text of the file.</param>
    /// <param name="file">The path of the file, which every error names (T-2).</param>
    /// <returns>The settings, which pass <see cref="GameSettings.Check"/>.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="ArgumentException">The file has no character (T-2).</exception>
    /// <exception cref="StorageException">
    /// The text is not JSON, a field is unknown, absent, or repeated, a value is outside its
    /// range, or this build reads no such format version (T-2, D-570).
    /// </exception>
    public static GameSettings Read(string text, string file)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(file);

        try
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes(text), file);
            GameSettings settings = ReadFile(ref reader);
            reader.ReadFileEnd();
            settings.Check();
            return settings;
        }
        catch (ContentException error)
        {
            throw StorageException.ForPath(file, $"the settings file breaks a rule: {error.Message}", error);
        }
        catch (ArgumentException error)
        {
            throw StorageException.ForPath(file, $"the settings file holds a value outside its range: {error.Message}", error);
        }
    }

    /// <summary>Gives the text of each value of <see cref="WindowSetting"/>.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The text that the file holds.</returns>
    public static string NameOf(WindowSetting value) => value switch
    {
        WindowSetting.Borderless => "borderless",
        WindowSetting.Window => "window",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    /// <summary>Gives the text of each value of <see cref="FitSetting"/>.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The text that the file holds.</returns>
    public static string NameOf(FitSetting value) => value switch
    {
        FitSetting.Fill => "fill",
        FitSetting.WholePixels => "wholePixels",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    private static string NameOf(TextSpeed value) => value switch
    {
        TextSpeed.Slow => "slow",
        TextSpeed.Normal => "normal",
        TextSpeed.Fast => "fast",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    private static string NameOf(MessageSpeed value) => value switch
    {
        MessageSpeed.Slow => "slow",
        MessageSpeed.Normal => "normal",
        MessageSpeed.Fast => "fast",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    private static string NameOf(EffectLevel value) => value switch
    {
        EffectLevel.Full => "full",
        EffectLevel.Reduced => "reduced",
        EffectLevel.Off => "off",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    private static string NameOf(BindingKind value) => value switch
    {
        BindingKind.Key => "key",
        BindingKind.Button => "button",
        BindingKind.Stick => "stick",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The value has no name (T-2)."),
    };

    private static void WriteDisplay(Utf8JsonWriter writer, DisplaySettings display)
    {
        writer.WriteStartObject("display");
        writer.WriteString("window", NameOf(display.Window));
        writer.WriteString("fit", NameOf(display.Fit));
        writer.WriteNumber("body", display.Body);
        writer.WriteEndObject();
    }

    private static void WriteAudio(Utf8JsonWriter writer, AudioSettings audio)
    {
        writer.WriteStartObject("audio");
        writer.WriteNumber("master", audio.Master);
        writer.WriteNumber("music", audio.Music);
        writer.WriteNumber("effects", audio.Effects);
        writer.WriteNumber("ambience", audio.Ambience);
        writer.WriteBoolean("muteInBackground", audio.MuteInBackground);
        writer.WriteBoolean("mono", audio.Mono);
        writer.WriteEndObject();
    }

    private static void WriteControls(Utf8JsonWriter writer, ControlSettings controls)
    {
        writer.WriteStartObject("controls");
        writer.WriteNumber("deadZone", controls.DeadZone);
        writer.WriteBoolean("vibration", controls.Vibration);
        writer.WriteStartObject("bindings");
        foreach (string action in controls.Bindings.Names)
        {
            writer.WriteStartArray(action);
            foreach (InputBinding binding in controls.Bindings.Of(action))
            {
                writer.WriteStartObject();
                writer.WriteString("kind", NameOf(binding.Kind));
                writer.WriteNumber("code", binding.Code);
                writer.WriteNumber("direction", binding.Direction);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteBattle(Utf8JsonWriter writer, BattleSettings battle)
    {
        writer.WriteStartObject("battle");
        writer.WriteString("messages", NameOf(battle.Messages));
        writer.WriteBoolean("rememberCursor", battle.RememberCursor);
        writer.WriteEndObject();
    }

    private static void WriteAccess(Utf8JsonWriter writer, AccessSettings access)
    {
        writer.WriteStartObject("access");
        writer.WriteString("effects", NameOf(access.Effects));
        writer.WriteString("text", NameOf(access.Text));
        writer.WriteBoolean("shapeIcons", access.ShapeIcons);
        writer.WriteEndObject();
    }

    private static GameSettings ReadFile(ref ContentReader reader)
    {
        int depth = reader.ReadObjectStart();
        if (!reader.ReadNextField(depth, out string first) || string.CompareOrdinal(first, "format") != 0)
        {
            throw reader.RefuseField(depth, "format", "the first field of a settings file is its format version (D-570)");
        }

        int format = reader.ReadInt();
        CheckFormat(ref reader, format);

        // Format 1 is the first format, and the chain holds no step yet (D-869). The PR that
        // raises the format adds the step from the version that it leaves here.
        return ReadFormatOne(ref reader, depth);
    }

    private static void CheckFormat(ref ContentReader reader, int format)
    {
        if (format > SettingsFormat.Current)
        {
            throw reader.Refuse(
                $"the file takes format version {format}, and this build writes format version {SettingsFormat.Current}. A newer build wrote it (D-570)");
        }

        if (format < SettingsFormat.Oldest)
        {
            throw reader.Refuse(
                $"the file takes format version {format}, and this build reads format version {SettingsFormat.Oldest} and later (D-570)");
        }
    }

    private static GameSettings ReadFormatOne(ref ContentReader reader, int depth)
    {
        DisplaySettings? display = null;
        AudioSettings? audio = null;
        ControlSettings? controls = null;
        BattleSettings? battle = null;
        AccessSettings? access = null;

        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "display":
                    display = ReadDisplay(ref reader);
                    break;
                case "audio":
                    audio = ReadAudio(ref reader);
                    break;
                case "controls":
                    controls = ReadControls(ref reader);
                    break;
                case "battle":
                    battle = ReadBattle(ref reader);
                    break;
                case "access":
                    access = ReadAccess(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new GameSettings(
            reader.Require(display, depth, "display"),
            reader.Require(audio, depth, "audio"),
            reader.Require(controls, depth, "controls"),
            reader.Require(battle, depth, "battle"),
            reader.Require(access, depth, "access"));
    }

    private static DisplaySettings ReadDisplay(ref ContentReader reader)
    {
        WindowSetting? window = null;
        FitSetting? fit = null;
        int? body = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "window":
                    window = ReadName(ref reader, Enum.GetValues<WindowSetting>(), NameOf);
                    break;
                case "fit":
                    fit = ReadName(ref reader, Enum.GetValues<FitSetting>(), NameOf);
                    break;
                case "body":
                    body = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new DisplaySettings(
            reader.RequireValue(window, depth, "window"),
            reader.RequireValue(fit, depth, "fit"),
            reader.RequireInt(body, depth, "body"));
    }

    private static AudioSettings ReadAudio(ref ContentReader reader)
    {
        int? master = null;
        int? music = null;
        int? effects = null;
        int? ambience = null;
        bool? muteInBackground = null;
        bool? mono = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "master":
                    master = reader.ReadInt();
                    break;
                case "music":
                    music = reader.ReadInt();
                    break;
                case "effects":
                    effects = reader.ReadInt();
                    break;
                case "ambience":
                    ambience = reader.ReadInt();
                    break;
                case "muteInBackground":
                    muteInBackground = reader.ReadBoolean();
                    break;
                case "mono":
                    mono = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new AudioSettings(
            reader.RequireInt(master, depth, "master"),
            reader.RequireInt(music, depth, "music"),
            reader.RequireInt(effects, depth, "effects"),
            reader.RequireInt(ambience, depth, "ambience"),
            reader.RequireValue(muteInBackground, depth, "muteInBackground"),
            reader.RequireValue(mono, depth, "mono"));
    }

    private static ControlSettings ReadControls(ref ContentReader reader)
    {
        int? deadZone = null;
        bool? vibration = null;
        ControlBindings? bindings = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "deadZone":
                    deadZone = reader.ReadInt();
                    break;
                case "vibration":
                    vibration = reader.ReadBoolean();
                    break;
                case "bindings":
                    bindings = ReadBindings(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ControlSettings(
            reader.Require(bindings, depth, "bindings"),
            reader.RequireInt(deadZone, depth, "deadZone"),
            reader.RequireValue(vibration, depth, "vibration"));
    }

    private static ControlBindings ReadBindings(ref ContentReader reader)
    {
        SortedDictionary<string, IReadOnlyList<InputBinding>> actions = new(StringComparer.Ordinal);

        // Each field name is the name of an action. Game compares the names with its own
        // actions when it applies the settings, and it fails on an unknown or an absent one.
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string action))
        {
            List<InputBinding> bindings = [];
            int arrayDepth = reader.ReadArrayStart();
            for (int index = 0; reader.ReadNextElement(arrayDepth, index); index += 1)
            {
                bindings.Add(ReadBinding(ref reader));
            }

            actions.Add(action, bindings);
        }

        return new ControlBindings(actions);
    }

    private static InputBinding ReadBinding(ref ContentReader reader)
    {
        BindingKind? kind = null;
        int? code = null;
        int? direction = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "kind":
                    kind = ReadName(ref reader, Enum.GetValues<BindingKind>(), NameOf);
                    break;
                case "code":
                    code = reader.ReadInt();
                    break;
                case "direction":
                    direction = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new InputBinding(
            reader.RequireValue(kind, depth, "kind"),
            reader.RequireInt(code, depth, "code"),
            reader.RequireInt(direction, depth, "direction"));
    }

    private static BattleSettings ReadBattle(ref ContentReader reader)
    {
        MessageSpeed? messages = null;
        bool? rememberCursor = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "messages":
                    messages = ReadName(ref reader, Enum.GetValues<MessageSpeed>(), NameOf);
                    break;
                case "rememberCursor":
                    rememberCursor = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new BattleSettings(
            reader.RequireValue(messages, depth, "messages"),
            reader.RequireValue(rememberCursor, depth, "rememberCursor"));
    }

    private static AccessSettings ReadAccess(ref ContentReader reader)
    {
        EffectLevel? effects = null;
        TextSpeed? text = null;
        bool? shapeIcons = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "effects":
                    effects = ReadName(ref reader, Enum.GetValues<EffectLevel>(), NameOf);
                    break;
                case "text":
                    text = ReadName(ref reader, Enum.GetValues<TextSpeed>(), NameOf);
                    break;
                case "shapeIcons":
                    shapeIcons = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new AccessSettings(
            reader.RequireValue(effects, depth, "effects"),
            reader.RequireValue(text, depth, "text"),
            reader.RequireValue(shapeIcons, depth, "shapeIcons"));
    }

    /// <summary>Reads a text, and gives the one value whose name it is.</summary>
    private static T ReadName<T>(ref ContentReader reader, T[] values, Func<T, string> nameOf)
        where T : struct, Enum
    {
        string text = reader.ReadString();
        List<string> names = [];
        foreach (T value in values)
        {
            string name = nameOf(value);
            if (string.CompareOrdinal(name, text) == 0)
            {
                return value;
            }

            names.Add(name);
        }

        throw reader.Refuse($"the value '{text}' is not one of {string.Join(", ", names)}");
    }
}
