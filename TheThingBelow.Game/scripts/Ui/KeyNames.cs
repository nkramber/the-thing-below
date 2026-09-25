using System.Collections.Generic;
using System.Globalization;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The name of a key on the remap screen, as a string id and its values (G-7, D-1128). The
/// screen reads the key of the layout of the player, so a French keyboard shows Z on the key
/// that a US keyboard calls W.
/// </summary>
/// <remarks>
/// A key that prints a letter, a digit, or a sign shows that sign, through the string of a
/// sign alone. Each other key takes a string id of its own, so no word of the engine reaches
/// the screen. The engine names English words such as "QuoteLeft" (finding P3-24).
/// </remarks>
public static class KeyNames
{
    /// <summary>The string of a key that prints a sign: the sign alone.</summary>
    public const string SignId = "settings.binding_key";

    /// <summary>The string of a function key, with its number.</summary>
    public const string FunctionId = "settings.key_function";

    /// <summary>The string of a digit of the number pad, with its digit.</summary>
    public const string KeypadDigitId = "settings.key_keypad_digit";

    /// <summary>The string of a key with no name of its own, with its code.</summary>
    public const string OtherId = "settings.key_other";

    /// <summary>The highest function key that takes a number: F1 to F35 lie in one run of codes.</summary>
    private const int LastFunctionKey = 35;

    /// <summary>Each key with a name of its own, by its code of the engine.</summary>
    private static readonly Dictionary<long, string> NamedKeys = new()
    {
        [(long)Godot.Key.Space] = "settings.key_space",
        [(long)Godot.Key.Escape] = "settings.key_escape",
        [(long)Godot.Key.Tab] = "settings.key_tab",
        [(long)Godot.Key.Backspace] = "settings.key_backspace",
        [(long)Godot.Key.Enter] = "settings.key_enter",
        [(long)Godot.Key.KpEnter] = "settings.key_keypad_enter",
        [(long)Godot.Key.Insert] = "settings.key_insert",
        [(long)Godot.Key.Delete] = "settings.key_delete",
        [(long)Godot.Key.Pause] = "settings.key_pause",
        [(long)Godot.Key.Print] = "settings.key_print",
        [(long)Godot.Key.Home] = "settings.key_home",
        [(long)Godot.Key.End] = "settings.key_end",
        [(long)Godot.Key.Left] = "settings.key_left",
        [(long)Godot.Key.Up] = "settings.key_up",
        [(long)Godot.Key.Right] = "settings.key_right",
        [(long)Godot.Key.Down] = "settings.key_down",
        [(long)Godot.Key.Pageup] = "settings.key_page_up",
        [(long)Godot.Key.Pagedown] = "settings.key_page_down",
        [(long)Godot.Key.Shift] = "settings.key_shift",
        [(long)Godot.Key.Ctrl] = "settings.key_ctrl",
        [(long)Godot.Key.Meta] = "settings.key_meta",
        [(long)Godot.Key.Alt] = "settings.key_alt",
        [(long)Godot.Key.Capslock] = "settings.key_caps_lock",
        [(long)Godot.Key.Numlock] = "settings.key_num_lock",
        [(long)Godot.Key.Scrolllock] = "settings.key_scroll_lock",
        [(long)Godot.Key.Menu] = "settings.key_menu",
        [(long)Godot.Key.KpMultiply] = "settings.key_keypad_multiply",
        [(long)Godot.Key.KpDivide] = "settings.key_keypad_divide",
        [(long)Godot.Key.KpSubtract] = "settings.key_keypad_subtract",
        [(long)Godot.Key.KpPeriod] = "settings.key_keypad_period",
        [(long)Godot.Key.KpAdd] = "settings.key_keypad_add",
    };

    /// <summary>Gives each string id that a key name can take, for the test of the string table.</summary>
    public static IReadOnlyList<string> Ids
    {
        get
        {
            List<string> ids = [SignId, FunctionId, KeypadDigitId, OtherId];
            ids.AddRange(NamedKeys.Values);
            ids.Sort(System.StringComparer.Ordinal);
            return ids;
        }
    }

    /// <summary>Gives the string id and the values of the name of one key of the layout.</summary>
    /// <param name="layoutKey">
    /// The code of the key in the layout of the player, from
    /// <c>DisplayServer.KeyboardGetKeycodeFromPhysical</c>, as a number.
    /// </param>
    /// <returns>The string id, and the value of each place of its text.</returns>
    /// <remarks>
    /// The engine gives the code of a sign key as the sign in Unicode, with a letter in upper
    /// case. Each other key holds the special bit of the engine, above every sign.
    /// </remarks>
    public static (string Id, IReadOnlyList<(string Place, string Value)> Values) Of(long layoutKey)
    {
        if (NamedKeys.TryGetValue(layoutKey, out string? named))
        {
            return (named, []);
        }

        long function = layoutKey - (long)Godot.Key.F1;
        if (function >= 0 && function < LastFunctionKey)
        {
            return (FunctionId, [("number", Number(function + 1))]);
        }

        long digit = layoutKey - (long)Godot.Key.Kp0;
        if (digit >= 0 && digit <= 9)
        {
            return (KeypadDigitId, [("digit", Number(digit))]);
        }

        if (IsSign(layoutKey))
        {
            string sign = char.ConvertFromUtf32((int)layoutKey).ToUpperInvariant();
            return (SignId, [("key", sign)]);
        }

        return (OtherId, [("code", Number(layoutKey))]);
    }

    /// <summary>
    /// Tells whether a code is a sign that a key prints: a visible character of Unicode below the
    /// special bit of the engine. A space, a control character, and a surrogate are not signs.
    /// </summary>
    private static bool IsSign(long code)
    {
        if (code <= ' ' || code >= (long)Godot.Key.Special || (code >= 0x7F && code <= 0xA0)
            || (code >= 0xD800 && code <= 0xDFFF))
        {
            return false;
        }

        UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory((int)code);
        return category is not (UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.OtherNotAssigned
            or UnicodeCategory.PrivateUse or UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator
            or UnicodeCategory.ParagraphSeparator);
    }

    private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
}
