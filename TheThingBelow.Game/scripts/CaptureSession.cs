using System;
using System.IO;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Game.Ui;

namespace TheThingBelow.Game;

/// <summary>
/// The capture session of the screen-test job (D-172, D-732). It draws each fixture of
/// <see cref="ScreenCaptures"/>, sets the window size and the fit of each capture, and
/// writes one PNG for each one. Then it quits with the success code.
/// </summary>
/// <remarks>
/// The session runs no tick. The frame time of the engine is a float clock, and a tick from
/// it would put the party in another place on each run (T-7, G-3). Thus every capture shows
/// the run of <see cref="Boot.FixtureSeed"/> at tick 0, and two runs give the same frames.
/// <para>
/// A capture needs a drawn frame, so the session waits <see cref="FramesBeforeCapture"/>
/// frames after each change of the window size. The world draws into a viewport, the frame
/// draws into a second one, and the fit of D-573 can add a third, so one change takes more
/// than one frame to reach the screen. The session then reads the size of the image, and it
/// fails when the window did not reach the size of the capture (T-2).
/// </para>
/// </remarks>
public sealed partial class CaptureSession : Node
{
    /// <summary>The line that a session writes when it wrote every capture (T-2).</summary>
    public const string SuccessLine = "capture: the session wrote every frame.";

    /// <summary>The count of frames that the session waits after each change of the window.</summary>
    public const int FramesBeforeCapture = 8;

    private ContentSet content = null!;
    private Action<Exception> reportFault = null!;
    private string folder = string.Empty;
    private FrameRoot? frame;
    private int next;
    private int waited;
    private bool stopped;

    /// <summary>Starts the capture session under one host node.</summary>
    /// <param name="host">The node that holds the session, which is the boot node.</param>
    /// <param name="content">The content set of this build.</param>
    /// <param name="folder">The folder that takes one PNG for each capture.</param>
    /// <param name="reportFault">The reporter of a fault, which writes the crash file (D-170).</param>
    /// <returns>The session, which draws from the next frame onward.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The folder is empty (T-2).</exception>
    public static CaptureSession Start(
        Node host, ContentSet content, string folder, Action<Exception> reportFault)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(folder);
        ArgumentNullException.ThrowIfNull(reportFault);

        var session = new CaptureSession
        {
            content = content,
            folder = folder,
            reportFault = reportFault,
        };

        host.AddChild(session);
        return session;
    }

    /// <summary>Makes the folder of the captures and draws the first one.</summary>
    public override void _Ready()
    {
        try
        {
            Directory.CreateDirectory(this.folder);
            // The screen-test job reads these three lines. Godot falls back to another driver
            // when it cannot start the one of the project, and the job fails on a fallback,
            // because another renderer draws another picture (D-616, D-731, T-2).
            GD.Print($"capture: the rendering method is {RenderingServer.GetCurrentRenderingMethod()}.");
            GD.Print($"capture: the rendering driver is {RenderingServer.GetCurrentRenderingDriverName()}.");
            GD.Print($"capture: the video adapter is {RenderingServer.GetVideoAdapterName()}.");
            GD.Print($"capture: the folder is {this.folder}.");
            this.Begin(0);
        }
        catch (Exception fault)
        {
            this.stopped = true;
            this.reportFault(fault);
        }
    }

    /// <summary>Waits for the draw of the current capture, writes it, and starts the next one.</summary>
    /// <param name="delta">The time of the frame, which this session never reads (T-7).</param>
    public override void _Process(double delta)
    {
        if (this.stopped)
        {
            return;
        }

        try
        {
            this.Step();
        }
        catch (Exception fault)
        {
            this.stopped = true;
            this.reportFault(fault);
        }
    }

    /// <summary>Writes the capture that the window now shows, and moves to the next one.</summary>
    /// <exception cref="InvalidOperationException">The window holds another size (T-2).</exception>
    /// <exception cref="IOException">The write of the file failed (T-2).</exception>
    private void Step()
    {
        if (this.waited < FramesBeforeCapture)
        {
            this.waited++;
            return;
        }

        ScreenCapture capture = ScreenCaptures.All[this.next];
        this.Write(capture);
        GD.Print($"capture: wrote {capture.FileName}.");

        this.next++;
        if (this.next >= ScreenCaptures.All.Count)
        {
            this.stopped = true;
            GD.Print(SuccessLine);
            this.GetTree().Quit(Boot.SuccessExitCode);
            return;
        }

        this.Begin(this.next);
    }

    /// <summary>Sets the window size of one capture, and builds its fixture.</summary>
    /// <param name="index">The place of the capture in <see cref="ScreenCaptures.All"/>.</param>
    /// <remarks>
    /// The window takes its new size first, so the frame reads that size when it builds its
    /// fit. Each capture builds its fixture again, because the default body size follows the
    /// fit of the screen, and the five captures of one fixture hold two body sizes (D-707).
    /// </remarks>
    private void Begin(int index)
    {
        ScreenCapture capture = ScreenCaptures.All[index];
        this.GetWindow().Size = new Vector2I(capture.Width, capture.Height);
        this.BuildFixture(capture);
        this.waited = 0;
    }

    /// <summary>
    /// Removes the frame that drew before, and builds the frame and the nodes of one fixture
    /// (D-734).
    /// </summary>
    /// <param name="capture">The capture that this fixture draws.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capture names no fixture of this session (T-2).</exception>
    private void BuildFixture(ScreenCapture capture)
    {
        if (this.frame is not null)
        {
            // The node leaves the tree at once, so it draws no frame of the next capture.
            // `QueueFree` alone would keep it on screen until the end of this frame (T-2).
            this.RemoveChild(this.frame);
            this.frame.QueueFree();
            this.frame = null;
        }

        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;
        built.SetMode(capture.Fit);

        // The body size comes from the capture and never from the window, so a resize that
        // the host reports late reaches no text of this capture (D-707, T-7).
        ScreenFit fit = ScreenFit.Of(capture.Fit, capture.Width, capture.Height);
        int body = BodySize.DefaultFor(fit.Height, this.content.Style.SmallBody, this.content.Style.LargeBody);
        UiBase @base = UiBase.Load(this.content, body);

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.MapFixture) == 0)
        {
            GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers());
            MapFixture.Build(built, @base, open.Party);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.UiFixture) == 0)
        {
            var panel = new UiFixture();
            built.Layer.AddChild(panel);
            panel.Build(@base, this.content.Strings);
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(capture),
            capture.Fixture,
            $"The capture list names the fixture '{capture.Fixture}', and the session builds none (T-2).");
    }

    /// <summary>Reads the window of this frame and writes one PNG.</summary>
    /// <param name="capture">The capture that this file holds.</param>
    /// <exception cref="InvalidOperationException">The window or the image holds another size (T-2).</exception>
    private void Write(ScreenCapture capture)
    {
        Image image = this.GetViewport().GetTexture().GetImage();
        if (image.GetWidth() != capture.Width || image.GetHeight() != capture.Height)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' asks for {capture.Width} by {capture.Height} pixels, " +
                $"and the window gave {image.GetWidth()} by {image.GetHeight()}. The screen of the " +
                $"session is too small, or the window did not resize in {FramesBeforeCapture} frames (T-2).");
        }

        byte[] bytes = image.SavePngToBuffer();
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Godot wrote no PNG bytes for the capture '{capture.FileName}' (T-2).");
        }

        File.WriteAllBytes(Path.Combine(this.folder, capture.FileName), bytes);
    }
}
