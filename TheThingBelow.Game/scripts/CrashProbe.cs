using System;
using Godot;

namespace TheThingBelow.Game;

/// <summary>
/// The node of the crash fixture of the smoke session and the capture session (P3-26, T-3). It
/// throws <see cref="PlantedMessage"/> inside its first `_Process` callback, and its catch hands
/// the error to the reporter of the boot node, as each callback of <see cref="Boot"/> does. The
/// reporter then writes the crash file and the log line, and it shows the message (D-170, D-559).
/// </summary>
/// <remarks>
/// A search of the source text of Boot proves no crash path. This node runs the path inside the
/// engine, and the session checks what the reporter left before it writes its success line. A
/// failed check goes to the same reporter as a real crash, so the session ends with no success
/// line (T-2).
/// </remarks>
public sealed partial class CrashProbe : Node
{
    /// <summary>The message of the error that the probe throws, which the crash file holds.</summary>
    public const string PlantedMessage = "the crash fixture threw this error on purpose, and the session checks the crash path (P3-26)";

    /// <summary>
    /// The time of the planted crash. The name of the crash file carries it, and the message shows
    /// that name, so one time gives one picture on every run of the screen test (D-172, T-7).
    /// </summary>
    public static readonly DateTime PlantedTime = new(2026, 9, 18, 0, 0, 0, DateTimeKind.Utc);

    private Action<Exception> reportFault = null!;
    private Action check = null!;
    private bool thrown;

    /// <summary>Adds the probe under one host node, and the probe throws on its first frame.</summary>
    /// <param name="host">The node that holds the probe, which is the boot node.</param>
    /// <param name="reportFault">The reporter of a crash of the boot node (D-170).</param>
    /// <param name="check">Checks what the reporter left, and goes on with the session.</param>
    /// <returns>The probe.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static CrashProbe Plant(Node host, Action<Exception> reportFault, Action check)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(reportFault);
        ArgumentNullException.ThrowIfNull(check);

        var probe = new CrashProbe
        {
            reportFault = reportFault,
            check = check,
        };

        host.AddChild(probe);
        return probe;
    }

    /// <summary>Throws the planted error once, hands it to the reporter, and runs the check.</summary>
    /// <param name="delta">The time of the frame, which the probe never reads (T-7).</param>
    public override void _Process(double delta)
    {
        if (this.thrown)
        {
            return;
        }

        this.thrown = true;
        try
        {
            Throw();
        }
        catch (Exception fault)
        {
            this.reportFault(fault);
        }

        try
        {
            this.check();
        }
        catch (Exception fault)
        {
            // The check failed, so the crash path is broken. The error takes the whole path of a
            // crash, and the session ends with no success line (T-2).
            this.reportFault(fault);
        }
    }

    /// <summary>Throws the planted error.</summary>
    /// <exception cref="InvalidOperationException">Always, on purpose (P3-26).</exception>
    private static void Throw() => throw new InvalidOperationException(PlantedMessage);
}
