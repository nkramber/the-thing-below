namespace ScreenScaleProbe;

/// <summary>One command of the probe. A key and a button of the pad each give the same command.</summary>
public enum ProbeCommand
{
    /// <summary>The input has no command.</summary>
    None,

    /// <summary>Shows the next scale state.</summary>
    NextState,

    /// <summary>Shows the state before the current one.</summary>
    StateBefore,

    /// <summary>Marks the world scale of the current state as the pick of the owner.</summary>
    WorldPick,

    /// <summary>Marks the UI scale of the current state as the pick of the owner.</summary>
    UiPick,

    /// <summary>Hides the panel of the probe, or shows it again.</summary>
    HidePanel,

    /// <summary>Writes the report of the run.</summary>
    Report,

    /// <summary>Writes the report and then stops the probe.</summary>
    ReportAndQuit,
}
