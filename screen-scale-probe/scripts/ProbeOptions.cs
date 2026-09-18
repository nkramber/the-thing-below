using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace ScreenScaleProbe;

/// <summary>The start flags of one probe run. An absent number stays absent, and never a zero (T-2).</summary>
public sealed class ProbeOptions
{
    /// <summary>The name of the screen in the report, such as deck-oled.</summary>
    public string ScreenLabel { get; private set; } = "unnamed-screen";

    /// <summary>The diagonal of the screen in inches. With none, the report gives no millimeters.</summary>
    public double? DiagonalInches { get; private set; }

    /// <summary>The distance from the eye to the screen in centimeters.</summary>
    public double? DistanceCm { get; private set; }

    /// <summary>True keeps the window at 1280 by 720, for a check on a machine of the author.</summary>
    public bool Windowed { get; private set; }

    /// <summary>A path for one PNG of the frame. The probe saves it with a report, and then stops.</summary>
    public string? ShotPath { get; private set; }

    /// <summary>The state that the run starts with, from 1 to 4.</summary>
    public int StartState { get; private set; } = 1;

    /// <summary>The folder of the report file. With none, the folder is reports/ beside the build.</summary>
    public string? ReportDir { get; private set; }

    /// <summary>Reads the flags after the two dashes of the Godot command line.</summary>
    public static ProbeOptions Parse(string[] args)
    {
        var options = new ProbeOptions();
        var unknown = new List<string>();
        foreach (string arg in args)
        {
            string[] parts = arg.Split('=', 2);
            string name = parts[0];
            string value = parts.Length == 2 ? parts[1] : string.Empty;
            switch (name)
            {
                case "--screen":
                    options.ScreenLabel = RequireText(name, value);
                    break;
                case "--diagonal":
                    options.DiagonalInches = RequireNumber(name, value);
                    break;
                case "--distance":
                    options.DistanceCm = RequireNumber(name, value);
                    break;
                case "--state":
                    options.StartState = RequireState(name, value);
                    break;
                case "--report":
                    options.ReportDir = RequireText(name, value);
                    break;
                case "--shot":
                    options.ShotPath = RequireText(name, value);
                    break;
                case "--windowed":
                    options.Windowed = true;
                    break;
                default:
                    unknown.Add(arg);
                    break;
            }
        }

        if (unknown.Count > 0)
        {
            throw new ArgumentException($"the probe does not know these flags: {string.Join(", ", unknown)}");
        }

        return options;
    }

    private static string RequireText(string name, string value)
    {
        if (value.Length == 0)
        {
            throw new ArgumentException($"the flag {name} needs a value, as in {name}=value");
        }

        return value;
    }

    private static double RequireNumber(string name, string value)
    {
        string text = RequireText(name, value);
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
        {
            throw new ArgumentException($"the flag {name} needs a number, and it got \"{text}\"");
        }

        if (number <= 0.0)
        {
            throw new ArgumentException($"the flag {name} needs a number above zero, and it got {text}");
        }

        return number;
    }

    private static int RequireState(string name, string value)
    {
        string text = RequireText(name, value);
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int state))
        {
            throw new ArgumentException($"the flag {name} needs a whole number, and it got \"{text}\"");
        }

        if (state < 1 || state > ScaleState.All.Length)
        {
            throw new ArgumentException($"the flag {name} needs a number from 1 to {ScaleState.All.Length}");
        }

        return state;
    }
}
