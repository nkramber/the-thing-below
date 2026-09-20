using System;
using System.Globalization;
using System.Text;
using Godot;

namespace ScreenScaleProbe;

/// <summary>Writes one Markdown report for one screen. The report holds the rows of M-8.</summary>
public static class ProbeReport
{
    /// <summary>Writes the report, and gives the path of the file that it wrote.</summary>
    public static string Write(
        string folder, ProbeOptions options, ScreenFacts facts, string? worldPick, string? textPick)
    {
        Error made = DirAccess.MakeDirRecursiveAbsolute(folder);
        if (made != Error.Ok)
        {
            throw new InvalidOperationException($"cannot make the report folder {folder}: {made}");
        }

        string path = FreePath(folder, options.ScreenLabel);
        using FileAccess? file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file is null)
        {
            throw new InvalidOperationException($"cannot write the report at {path}: {FileAccess.GetOpenError()}");
        }

        file.StoreString(Build(options, facts, worldPick, textPick));
        return path;
    }

    private static string FreePath(string folder, string label)
    {
        string path = $"{folder}/{label}.md";
        int next = 2;
        while (FileAccess.FileExists(path))
        {
            path = $"{folder}/{label}-{next}.md";
            next++;
        }

        return path;
    }

    private static string Build(
        ProbeOptions options, ScreenFacts facts, string? worldPick, string? textPick)
    {
        var text = new StringBuilder();
        text.AppendLine($"# Screen scale probe: {options.ScreenLabel}");
        text.AppendLine();
        text.AppendLine($"Written {Time.GetDatetimeStringFromSystem(true)} by the probe of D-621. It answers OQ-183.");
        text.AppendLine();
        text.AppendLine("## The facts of this run");
        text.AppendLine();
        text.AppendLine("| Fact | Value |");
        text.AppendLine("|---|---|");
        text.AppendLine($"| Screen label | {options.ScreenLabel} |");
        text.AppendLine($"| Godot version | {Engine.GetVersionInfo()["string"]} |");
        text.AppendLine($"| Renderer | {ProjectSettings.GetSetting("rendering/renderer/rendering_method")} |");
        text.AppendLine($"| Platform | {OS.GetName()} |");
        text.AppendLine($"| Screen pixels | {facts.ScreenPixels.X} by {facts.ScreenPixels.Y} |");
        text.AppendLine($"| Window pixels | {facts.WindowPixels.X} by {facts.WindowPixels.Y} |");
        text.AppendLine($"| Screen scale of the system | {Number(facts.OsScale)} |");
        text.AppendLine($"| Diagonal | {Inches(options.DiagonalInches)} |");
        text.AppendLine($"| Distance | {Centimeters(options.DistanceCm)} |");
        text.AppendLine($"| Fit of the frame, whole number (D-568) | {facts.WholeFit}x |");
        text.AppendLine($"| Fit of the frame, D-573 | {Number(facts.FitFactor)}x |");
        text.AppendLine($"| Whole-number step of D-573 | {facts.NearestSteps}x |");
        text.AppendLine($"| Millimeters for each device pixel | {Millimeters(facts.MmPerPixel, 4)} |");
        text.AppendLine();

        if (facts.FitIsWhole)
        {
            text.AppendLine("The fit of this screen is a whole number, so the two fit modes draw the same picture.");
            text.AppendLine();
            AppendStates(text, options, facts, FitMode.Whole, "The four states");
        }
        else
        {
            text.AppendLine("The fit of this screen is fractional, so the two fit modes draw two pictures. "
                + "Mode whole holds the frame at the whole number below the fit, with wide black bars. "
                + "Mode fill takes the two steps of D-573.");
            text.AppendLine();
            AppendStates(text, options, facts, FitMode.Whole, "The four states in mode whole");
            AppendStates(text, options, facts, FitMode.Fill, "The four states in mode fill");
        }

        text.AppendLine("## The pick of the owner");
        text.AppendLine();
        text.AppendLine($"- The scale of the world: {worldPick ?? "no pick in this run"}");
        text.AppendLine($"- The sizes of the text: {textPick ?? "no pick in this run"}");
        text.AppendLine();
        text.AppendLine("## Notes");
        text.AppendLine();
        if (facts.Notes.Count == 0)
        {
            text.AppendLine("- The run holds every number, and the frame filled the screen.");
        }
        else
        {
            foreach (string note in facts.Notes)
            {
                text.AppendLine($"- {note}");
            }
        }

        return text.ToString();
    }

    private static void AppendStates(
        StringBuilder text, ProbeOptions options, ScreenFacts facts, FitMode mode, string heading)
    {
        text.AppendLine($"## {heading}");
        text.AppendLine();
        text.AppendLine("| State | Tiles across | Device pixels for one art pixel of the world "
            + "| Device pixels for one glyph pixel | Sprite of 32 pixels | Apparent sprite "
            + "| Line of body text | Apparent line | Line of title text "
            + "| Characters in a dialogue line | Characters across the frame |");
        text.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (ScaleState state in ScaleState.All)
        {
            double? spriteMm = facts.SpriteMm(state, mode);
            double? glyphMm = facts.GlyphMm(state, mode);
            double? titleMm = facts.TitleMm(state, mode);
            text.Append($"| {state.Name} ");
            text.Append($"| {ScreenFacts.TilesAcross(state).ToString("0.##", CultureInfo.InvariantCulture)} ");
            text.Append($"| {Number(facts.Scale(mode) * state.WorldScale)} ");
            text.Append($"| {Number(facts.Scale(mode) * state.BodyUnit)} ");
            text.Append($"| {Millimeters(spriteMm, 2)} ");
            text.Append($"| {Arcminutes(spriteMm, options.DistanceCm)} ");
            text.Append($"| {Millimeters(glyphMm, 2)} ");
            text.Append($"| {Arcminutes(glyphMm, options.DistanceCm)} ");
            text.Append($"| {Millimeters(titleMm, 2)} ");
            text.Append($"| {ScreenFacts.DialogueColumns(state)} ");
            text.AppendLine($"| {ScreenFacts.FrameColumns(state)} |");
        }

        text.AppendLine();
    }

    private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Inches(double? value) =>
        value is double number ? $"{number.ToString("0.##", CultureInfo.InvariantCulture)} inches" : "absent";

    private static string Centimeters(double? value) =>
        value is double number ? $"{number.ToString("0.#", CultureInfo.InvariantCulture)} cm" : "absent";

    private static string Millimeters(double? value, int digits)
    {
        if (value is not double number)
        {
            return "absent";
        }

        string format = digits == 4 ? "0.0000" : "0.00";
        return $"{number.ToString(format, CultureInfo.InvariantCulture)} mm";
    }

    private static string Arcminutes(double? millimeters, double? distanceCm)
    {
        double? value = ScreenFacts.Arcminutes(millimeters, distanceCm);
        return value is double number
            ? $"{number.ToString("0.#", CultureInfo.InvariantCulture)} arcminutes"
            : "absent";
    }
}
