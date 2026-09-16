#!/usr/bin/env python3
"""Fixes the three report faults that the first Mac run found."""
import pathlib
import sys


def swap(path: pathlib.Path, old: str, new: str) -> None:
    text = path.read_text()
    if old not in text:
        sys.exit(f"pattern absent in {path}:\n{old}")
    if text.count(old) != 1:
        sys.exit(f"pattern is not unique in {path}:\n{old}")
    path.write_text(text.replace(old, new))


meter = pathlib.Path("scripts/FrameMeter.cs")
main = pathlib.Path("scripts/Main.cs")

# 1. A stage holds the target when 95 of 100 frames hold it. One jittery frame is not a miss.
swap(
    meter,
    """    /// <summary>True when every measured frame held the target of D-161.</summary>
    public bool HoldsTarget => FramesOverBudget == 0;""",
    """    /// <summary>
    /// True when 95 of each 100 frames held the target of D-161. The count of every frame over
    /// the budget stays in FramesOverBudget, because one stray frame is jitter and not a miss.
    /// </summary>
    public bool HoldsTarget => P95Ms <= FrameMeter.BudgetMs;""",
)

# 2. The sweep row takes the last stage that held, so one stray frame never truncates the row.
swap(
    main,
    """            if (r.HoldsTarget)
            {
                held = r.Name;
                continue;
            }

            // The sweep goes up, so the first miss ends the row.
            break;""",
    """            if (r.HoldsTarget)
            {
                held = r.Name;
            }""",
)

# 3. The report says when the run measured the refresh rate and not the load (T-2).
swap(
    main,
    """        text.AppendLine($"frames each stage {WarmupFrames} warm-up, {MeasureFrames} measured");
        text.AppendLine();""",
    """        text.AppendLine($"frames each stage {WarmupFrames} warm-up, {MeasureFrames} measured");
        text.AppendLine($"vsync             {DisplayServer.WindowGetVsyncMode()}");
        text.AppendLine();

        if (LooksFrameCapped(out double capMs))
        {
            text.AppendLine(
                $"WARNING: every stage measured about {capMs.ToString("F2", culture)} ms, which is the " +
                "refresh interval of this screen.");
            text.AppendLine(
                "The frame rate is capped, so each number below is a floor and not the cost of the load.");
            text.AppendLine(
                "This run cannot pick a renderer or set a budget. Turn the cap off, then run it again.");
            text.AppendLine();
        }""",
)

# 4. The detector of the cap, and the same warning in the budget summary.
swap(
    main,
    """    /// <summary>The name of the last stage of a sweep that held the target, or a plain statement.</summary>""",
    """    /// <summary>
    /// True when every stage measured the same frame time, and that time matches the refresh
    /// interval of the screen. A capped run measures the screen and never the load (T-2).
    /// </summary>
    private bool LooksFrameCapped(out double capMs)
    {
        capMs = 0.0;

        if (_results.Count < 2)
        {
            return false;
        }

        double lowest = double.MaxValue;
        double highest = double.MinValue;
        foreach (StageResult r in _results)
        {
            lowest = Math.Min(lowest, r.MedianMs);
            highest = Math.Max(highest, r.MedianMs);
        }

        // The lightest stage and the heaviest stage differ by less than a tenth of a millisecond.
        if (highest - lowest > 0.10)
        {
            return false;
        }

        float refreshHz = DisplayServer.ScreenGetRefreshRate();
        if (refreshHz <= 0f)
        {
            return false;
        }

        double refreshMs = 1000.0 / refreshHz;
        if (Math.Abs(highest - refreshMs) > 0.50)
        {
            return false;
        }

        capMs = highest;
        return true;
    }

    /// <summary>The name of the last stage of a sweep that held the target, or a plain statement.</summary>""",
)

swap(
    main,
    """        var text = new StringBuilder();
        text.AppendLine("The effect budget of this run (D-523). Each row is the largest load that held the target.");""",
    """        var text = new StringBuilder();

        if (LooksFrameCapped(out _))
        {
            text.AppendLine("This run gives no effect budget, because the frame rate was capped.");
            return text.ToString();
        }

        text.AppendLine("The effect budget of this run (D-523). Each row is the largest load that held the target.");""",
)

print("patched FrameMeter.cs and Main.cs")
