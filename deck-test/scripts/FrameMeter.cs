using System;
using System.Collections.Generic;

namespace DeckTest;

/// <summary>The measurement of one stage: every number that D-161 and D-523 compare.</summary>
public readonly record struct StageResult(
    string Name,
    int Lights,
    int Emitters,
    int Particles,
    int Passes,
    int Frames,
    double AverageMs,
    double MedianMs,
    double P95Ms,
    double WorstMs,
    int FramesOverBudget)
{
    /// <summary>
    /// True when 95 of each 100 frames held the target of D-161. The count of every frame over
    /// the budget stays in FramesOverBudget, because one stray frame is jitter and not a miss.
    /// </summary>
    public bool HoldsTarget => P95Ms <= FrameMeter.BudgetMs;
}

/// <summary>
/// Counts the time of each frame of one stage. It drops the warm-up frames, because the
/// first frames of a stage pay for shader compilation and for the first draw of a texture.
/// </summary>
public sealed class FrameMeter
{
    /// <summary>16.667 ms. A frame longer than this misses 60 frames each second (D-161).</summary>
    public const double BudgetMs = 1000.0 / 60.0;

    private readonly List<double> _samples = new();
    private int _warmupLeft;
    private int _measureLeft;

    public bool Running { get; private set; }

    public void Start(int warmupFrames, int measureFrames)
    {
        if (warmupFrames < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(warmupFrames), warmupFrames, "The warm-up count is negative.");
        }

        if (measureFrames < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(measureFrames), measureFrames, "A stage measures at least one frame.");
        }

        _samples.Clear();
        _samples.Capacity = measureFrames;
        _warmupLeft = warmupFrames;
        _measureLeft = measureFrames;
        Running = true;
    }

    /// <summary>Takes one frame. It gives true when the stage has every frame that it needs.</summary>
    public bool Tick(double deltaSeconds)
    {
        if (!Running)
        {
            throw new InvalidOperationException("Tick ran before Start, so the meter has no stage.");
        }

        if (_warmupLeft > 0)
        {
            _warmupLeft--;
            return false;
        }

        _samples.Add(deltaSeconds * 1000.0);
        _measureLeft--;

        if (_measureLeft > 0)
        {
            return false;
        }

        Running = false;
        return true;
    }

    public StageResult Result(string name, int lights, int emitters, int particles, int passes)
    {
        if (_samples.Count == 0)
        {
            throw new InvalidOperationException($"Stage {name} has no measured frame, so it has no result.");
        }

        var sorted = new List<double>(_samples);
        sorted.Sort();

        double total = 0.0;
        int over = 0;
        foreach (double ms in _samples)
        {
            total += ms;
            if (ms > BudgetMs)
            {
                over++;
            }
        }

        return new StageResult(
            Name: name,
            Lights: lights,
            Emitters: emitters,
            Particles: particles,
            Passes: passes,
            Frames: _samples.Count,
            AverageMs: total / _samples.Count,
            MedianMs: Percentile(sorted, 0.50),
            P95Ms: Percentile(sorted, 0.95),
            WorstMs: sorted[^1],
            FramesOverBudget: over);
    }

    private static double Percentile(List<double> sorted, double fraction)
    {
        int index = (int)Math.Round(fraction * (sorted.Count - 1), MidpointRounding.AwayFromZero);
        return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
    }
}
