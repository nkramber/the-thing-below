namespace ScreenScaleProbe;

/// <summary>One of the four states of the probe: a scale for the world, and a scale for the UI.</summary>
public sealed class ScaleState
{
    private ScaleState(string name, int world, int ui)
    {
        Name = name;
        World = world;
        Ui = ui;
    }

    /// <summary>The name of the state on the screen and in the report.</summary>
    public string Name { get; }

    /// <summary>Device pixels for each art pixel of the world, before the fit of the frame.</summary>
    public int World { get; }

    /// <summary>Device pixels for each art pixel of the UI, before the fit of the frame.</summary>
    public int Ui { get; }

    /// <summary>The four states of OQ-183, in the order that one key steps through.</summary>
    public static ScaleState[] All { get; } =
    [
        new ScaleState("world 1x, UI 1x", 1, 1),
        new ScaleState("world 2x, UI 2x", 2, 2),
        new ScaleState("world 2x, UI 1x", 2, 1),
        new ScaleState("world 1x, UI 2x", 1, 2),
    ];
}
