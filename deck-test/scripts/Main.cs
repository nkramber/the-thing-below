using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godot;

namespace DeckTest;

/// <summary>
/// The Deck test of D-160. It runs one load sweep, it measures the time of each frame, and it
/// writes a report. The renderer comes from the command line, so one export measures both
/// Forward+ and Mobile. D-523 reads the sweep for the effect budget.
/// </summary>
public partial class Main : Node2D
{
    private const int WarmupFrames = 60;
    private const int MeasureFrames = 300;

    /// <summary>The length of the wipe of the transition stage, in seconds.</summary>
    private const double TransitionSeconds = 1.2;

    private readonly List<Stage> _stages = new();
    private readonly List<StageResult> _results = new();
    private readonly FrameMeter _meter = new();

    private SceneRig _rig = null!;
    private int _stageIndex = -1;
    private double _stageClock;

    public override void _Ready()
    {
        // Every frame must show its true cost, so nothing caps the frame rate.
        Engine.MaxFps = 0;
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

        _rig = new SceneRig(this);
        BuildStages();
        StartNextStage();
    }

    public override void _Process(double delta)
    {
        if (_stageIndex < 0 || _stageIndex >= _stages.Count)
        {
            return;
        }

        Stage stage = _stages[_stageIndex];

        if (stage.Transition)
        {
            _stageClock += delta;
            float progress = (float)((_stageClock % TransitionSeconds) / TransitionSeconds);
            _rig.SetTransition(progress);
        }

        if (!_meter.Tick(delta))
        {
            return;
        }

        _results.Add(_meter.Result(
            stage.Name,
            stage.Lights + (2 * stage.Pairs),
            stage.Emitters,
            stage.Emitters * SceneRig.ParticlesForEachEmitter,
            _rig.ActivePassCount()));

        StartNextStage();
    }

    private void BuildStages()
    {
        // The fixed cost: the map, the props, and then each full-screen pass.
        _stages.Add(new Stage("world-only", Lights: 0, Emitters: 0, Crt: false, Glow: false, Fog: false, Transition: false));
        _stages.Add(new Stage("pass-crt", 0, 0, Crt: true, Glow: false, Fog: false, Transition: false));
        _stages.Add(new Stage("pass-crt-glow", 0, 0, Crt: true, Glow: true, Fog: false, Transition: false));
        _stages.Add(new Stage("pass-crt-glow-fog", 0, 0, Crt: true, Glow: true, Fog: true, Transition: false));

        // The light sweep, with every pass on. It gives the light row of the effect budget.
        foreach (int lights in new[] { 1, 2, 4, 6, 8, 10, 12, 15 })
        {
            _stages.Add(new Stage($"lights-{lights}", lights, 0, Crt: true, Glow: true, Fog: true, Transition: false));
        }

        // The particle sweep, with every pass on. It gives the particle row of the effect budget.
        foreach (int emitters in new[] { 1, 2, 4, 8, 16 })
        {
            int particles = emitters * SceneRig.ParticlesForEachEmitter;
            _stages.Add(new Stage($"particles-{particles}", 0, emitters, Crt: true, Glow: true, Fog: true, Transition: false));
        }

        // The full load of D-160, and then the same load with the transition over it.
        _stages.Add(new Stage("full-load", 8, 4, Crt: true, Glow: true, Fog: true, Transition: false));
        _stages.Add(new Stage("full-load-transition", 8, 4, Crt: true, Glow: true, Fog: true, Transition: true));
        _stages.Add(new Stage("worst-case", SceneRig.MaxLights, SceneRig.MaxEmitters, Crt: true, Glow: true, Fog: true, Transition: true));

        // The light row of D-854: paired sources of D-853, from 4 to 24 Godot lights. The CRT
        // left the plan (D-618), so these stages run the glow and the fog alone.
        foreach (int pairs in new[] { 2, 4, 6, 8, 10, 12 })
        {
            _stages.Add(new Stage($"pairs-{pairs * 2}", 0, 0, Crt: false, Glow: true, Fog: true, Transition: false, Pairs: pairs));
        }

        _stages.Add(new Stage("full-load-24", 0, 4, Crt: false, Glow: true, Fog: true, Transition: true, Pairs: SceneRig.MaxPairs));
    }

    private void StartNextStage()
    {
        _stageIndex++;

        if (_stageIndex >= _stages.Count)
        {
            Report();
            GetTree().Quit();
            return;
        }

        Stage stage = _stages[_stageIndex];
        _rig.SetLightCount(stage.Lights);
        _rig.SetPairCount(stage.Pairs);
        _rig.SetEmitterCount(stage.Emitters);
        _rig.SetPasses(stage.Crt, stage.Glow, stage.Fog);
        _rig.SetTransition(stage.Transition ? 0f : -1f);
        _stageClock = 0.0;
        _meter.Start(WarmupFrames, MeasureFrames);

        GD.Print($"stage {_stageIndex + 1}/{_stages.Count}: {stage.Name}");
    }

    private void Report()
    {
        string method = RenderingServer.GetCurrentRenderingMethod();
        string text = BuildReport(method);

        GD.Print(text);

        string path = $"user://deck-test-{method}.txt";
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file is null)
        {
            // T-2: an absent file is an error, and the error carries its path and its cause.
            throw new InvalidOperationException(
                $"The report did not open at {path}: {FileAccess.GetOpenError()}.");
        }

        file.StoreString(text);
        GD.Print($"report written to {ProjectSettings.GlobalizePath(path)}");
    }

    private string BuildReport(string method)
    {
        var text = new StringBuilder();
        var culture = CultureInfo.InvariantCulture;

        text.AppendLine("The Thing Below: the Deck test of D-160 and D-523.");
        text.AppendLine($"renderer          {method}");
        text.AppendLine($"frame             {SceneRig.FrameWidth} by {SceneRig.FrameHeight} (D-568)");
        text.AppendLine($"target            60 frames each second, {FrameMeter.BudgetMs.ToString("F3", culture)} ms each frame (D-161)");
        text.AppendLine($"engine            {Engine.GetVersionInfo()["string"]}");
        text.AppendLine($"machine           {MachineName()}");
        text.AppendLine($"system            {OS.GetName()} {OS.GetDistributionName()}");
        text.AppendLine($"processor         {OS.GetProcessorName()}");
        text.AppendLine($"adapter           {RenderingServer.GetVideoAdapterName()} ({RenderingServer.GetVideoAdapterVendor()})");
        text.AppendLine($"driver api        {RenderingServer.GetVideoAdapterApiVersion()}");
        text.AppendLine($"screen            {DisplayServer.ScreenGetSize()} at {DisplayServer.ScreenGetRefreshRate().ToString("F1", culture)} Hz");
        text.AppendLine($"frames each stage {WarmupFrames} warm-up, {MeasureFrames} measured");
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
        }

        text.AppendLine("stage                  lights  particles  passes  avg ms  p50 ms  p95 ms  worst ms  over  holds");
        foreach (StageResult r in _results)
        {
            text.AppendLine(string.Join(string.Empty,
                r.Name.PadRight(23),
                r.Lights.ToString(culture).PadLeft(6),
                r.Particles.ToString(culture).PadLeft(11),
                r.Passes.ToString(culture).PadLeft(8),
                r.AverageMs.ToString("F2", culture).PadLeft(8),
                r.MedianMs.ToString("F2", culture).PadLeft(8),
                r.P95Ms.ToString("F2", culture).PadLeft(8),
                r.WorstMs.ToString("F2", culture).PadLeft(10),
                r.FramesOverBudget.ToString(culture).PadLeft(6),
                (r.HoldsTarget ? "yes" : "no").PadLeft(7)));
        }

        text.AppendLine();
        text.AppendLine(BudgetSummary(culture));
        return text.ToString();
    }

    /// <summary>The rows that the effect budget of D-523 takes from this run.</summary>
    private string BudgetSummary(CultureInfo culture)
    {
        var text = new StringBuilder();

        if (LooksFrameCapped(out _))
        {
            text.AppendLine("This run gives no effect budget, because the frame rate was capped.");
            return text.ToString();
        }

        text.AppendLine("The effect budget of this run (D-523). Each row is the largest load that held the target.");
        text.AppendLine($"  lights with shadows   {LargestHeld("lights-")}");
        text.AppendLine($"  paired lights         {LargestHeld("pairs-")} (the row of D-854 asks for 24)");
        text.AppendLine($"  live particles        {LargestHeld("particles-")}");
        text.AppendLine($"  full-screen passes    {LargestHeld("pass-")}");

        StageResult? full = Find("full-load");
        if (full is not null)
        {
            text.AppendLine(
                $"  the load of D-160 held the target: {(full.Value.HoldsTarget ? "yes" : "no")} " +
                $"(p95 {full.Value.P95Ms.ToString("F2", culture)} ms)");
        }

        StageResult? paired = Find("full-load-24");
        if (paired is not null)
        {
            text.AppendLine(
                $"  24 paired lights with particles and a transition held the target: {(paired.Value.HoldsTarget ? "yes" : "no")} " +
                $"(p95 {paired.Value.P95Ms.ToString("F2", culture)} ms)");
        }

        text.AppendLine();
        text.AppendLine("Run the other renderer, then compare. The renderer with more room wins (D-160).");
        text.AppendLine("If neither renderer holds the target, the owner decides then (D-261).");
        return text.ToString();
    }

    /// <summary>
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

    /// <summary>The name of the last stage of a sweep that held the target, or a plain statement.</summary>
    private string LargestHeld(string prefix)
    {
        string? held = null;

        foreach (StageResult r in _results)
        {
            if (!r.Name.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (r.HoldsTarget)
            {
                held = r.Name;
            }
        }

        return held ?? $"no stage of {prefix} held the target";
    }

    private StageResult? Find(string name)
    {
        foreach (StageResult r in _results)
        {
            if (r.Name == name)
            {
                return r;
            }
        }

        return null;
    }

    /// <summary>The model of the machine. SteamOS names the LCD Deck Jupiter and the OLED Deck Galileo.</summary>
    private static string MachineName()
    {
        const string dmiPath = "/sys/class/dmi/id/product_name";

        if (!FileAccess.FileExists(dmiPath))
        {
            return "unknown (no DMI product name on this system)";
        }

        using FileAccess file = FileAccess.Open(dmiPath, FileAccess.ModeFlags.Read);
        if (file is null)
        {
            return $"unknown ({dmiPath} did not open: {FileAccess.GetOpenError()})";
        }

        return file.GetAsText().Trim();
    }

    /// <summary>One step of the sweep. A stage never adds a node, so it pays for the load alone.</summary>
    private readonly record struct Stage(
        string Name,
        int Lights,
        int Emitters,
        bool Crt,
        bool Glow,
        bool Fog,
        bool Transition,
        int Pairs = 0);
}
