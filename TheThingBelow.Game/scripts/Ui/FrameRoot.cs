using System;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The one picture of the game: a frame of 1280 by 720, the world inside it at 2x, and the
/// fit of that frame to the screen (D-568, D-573, D-633, D-634).
/// </summary>
/// <remarks>
/// Every screen shape other than 16 to 9 shows black bars, the Steam Deck included (D-228,
/// D-568). Thus every screen shows the same part of the map, and no screen shape gains a view
/// of a patrol (D-37, D-566).
/// <para>
/// Godot has no stretch mode that scales up by a whole number and then scales down, so this
/// node builds both steps of D-573 itself (F-48). The stretch mode of the project is
/// `disabled`, so the root viewport is the window and this node owns every scale (F-45).
/// </para>
/// <para>
/// The world draws into a `SubViewport` of 640 by 360 at 1x, and the frame shows it at 2x.
/// One art pixel of the world is one viewport pixel, so the light of a later PR falls on art
/// pixels (D-230, D-634, F-48).
/// </para>
/// <para>
/// The world draws in HDR 2D with the glow of Godot, and its view turns linear light into sRGB
/// (D-910, F-103). An overlay view with no HDR 2D shares the world, and it draws the fog, the hit
/// bursts, and the light shafts above the glow, so the fog never glows (D-916, D-919).
/// </para>
/// <para>
/// A scene view of 640 by 360 joins the world and the overlay. The frame draws the scene with the
/// tilt-shift blur, and the vignette over it (D-849, D-919). A mark view shares the world too, and
/// it draws the marks above the passes, so each mark stays sharp. The UI draws above every view
/// (D-208, D-210). The pass of the hand-off draws above the UI, so a transition covers the
/// whole frame (D-195, D-939).
/// </para>
/// </remarks>
public partial class FrameRoot : Node
{
    /// <summary>The width of the world viewport, in art pixels (D-634).</summary>
    public const int WorldWidth = 640;

    /// <summary>The height of the world viewport, in art pixels (D-634).</summary>
    public const int WorldHeight = 360;

    /// <summary>The whole number that scales the world into the frame (D-633).</summary>
    public const int WorldScale = 2;

    private SubViewport worldViewport = null!;
    private SubViewport frameViewport = null!;
    private SubViewport overlayViewport = null!;
    private SubViewport markViewport = null!;
    private SubViewport sceneViewport = null!;
    private ShaderMaterial blur = null!;
    private ShaderMaterial vignette = null!;
    private SubViewport? stepViewport;
    private TextureRect screenView = null!;
    private ColorRect bars = null!;

    /// <summary>The fit of the frame to the screen of this session (D-232).</summary>
    public FitMode Mode { get; private set; } = FitMode.Fill;

    /// <summary>The place of the frame on the screen, as the last fit gave it.</summary>
    public ScreenFit Fit { get; private set; } = ScreenFit.Of(FitMode.Fill, ScreenFit.FrameWidth, ScreenFit.FrameHeight);

    /// <summary>The viewport of the world, at 640 by 360 art pixels (D-634).</summary>
    public SubViewport World => this.worldViewport;

    /// <summary>The place of every UI node, at the 1280 by 720 pixels of the frame (D-568).</summary>
    public Control Layer { get; private set; } = null!;

    /// <summary>The pass of the hand-off, above the UI, which covers the whole frame (D-195, D-210, D-939).</summary>
    public TransitionPass HandOffPass { get; private set; } = null!;

    /// <summary>Builds the frame, the world viewport, and the view on the screen.</summary>
    public override void _Ready()
    {
        this.BuildWorld();
        this.BuildOverlay();
        this.BuildMarks();
        this.BuildScene();
        this.BuildFrame();
        this.BuildScreenView();

        GetWindow().Oversampling = false;
        GetWindow().SizeChanged += this.OnScreenChanged;
        this.OnScreenChanged();
    }

    /// <summary>Gives the world the glow of the file, on a map or in a fight (D-910).</summary>
    /// <param name="glow">The glow file.</param>
    /// <exception cref="ArgumentNullException">The glow is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The frame is not in the tree yet, so it holds no world (T-2).</exception>
    /// <remarks>Each screen of the world calls this method with the same file, so a second call changes nothing.</remarks>
    public void ShowGlow(TheThingBelow.Core.Light.Glow glow)
    {
        ArgumentNullException.ThrowIfNull(glow);

        if (this.worldViewport is null)
        {
            throw new InvalidOperationException("The frame holds no world before it enters the tree, so it can show no glow (T-2).");
        }

        this.worldViewport.World3D.Environment = GlowPass.EnvironmentOf(glow);
    }

    /// <summary>Gives the frame the tilt-shift blur and the vignette of the passes, on a map or in a fight (D-917, D-920).</summary>
    /// <param name="passes">The passes of the file, in the mode that the screen shows.</param>
    /// <param name="palette">The palette, which gives the key of the vignette its color (D-181).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The frame is not in the tree yet, so it holds no passes (T-2).</exception>
    /// <remarks>Until a screen calls this method, the blur and the vignette draw nothing, so a screen with no world stays as it was.</remarks>
    public void ShowPasses(Hd2dPasses passes, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(passes);
        ArgumentNullException.ThrowIfNull(palette);

        if (this.blur is null)
        {
            throw new InvalidOperationException("The frame holds no passes before it enters the tree, so it can show none (T-2).");
        }

        LookPasses.Show(this.blur, this.vignette, passes, palette);
    }

    /// <summary>Changes the fit of the frame, which the fit of the display settings sets (D-232, D-860).</summary>
    /// <param name="mode">The fit to take from now on.</param>
    public void SetMode(FitMode mode)
    {
        this.Mode = mode;
        this.OnScreenChanged();
    }

    /// <summary>
    /// Gives one input event to the nodes of <see cref="Layer"/>, such as the entry of the
    /// debug console (D-725).
    /// </summary>
    /// <param name="signal">A key event of the root viewport.</param>
    /// <remarks>
    /// The screen shows the frame viewport through a texture and not through a viewport
    /// container, so the engine gives that viewport no event of its own. A focused node of the
    /// frame thus reads a key only through this method.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    public void PushToLayer(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);
        this.frameViewport.PushInput(signal);
    }

    /// <summary>
    /// Measures the screen again and places the frame on it. The method runs at the start and
    /// after every change of the window size.
    /// </summary>
    public void OnScreenChanged()
    {
        Vector2I screen = GetWindow().Size;
        this.Fit = ScreenFit.Of(this.Mode, Math.Max(1, screen.X), Math.Max(1, screen.Y));

        this.BuildStep();
        this.screenView.Position = new Vector2(this.Fit.Left, this.Fit.Top);
        this.screenView.Size = new Vector2(this.Fit.Width, this.Fit.Height);
        this.bars.Size = new Vector2(screen.X, screen.Y);
    }

    private void BuildWorld()
    {
        this.worldViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth, WorldHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            TransparentBg = true,

            // Font oversampling re-draws a glyph at the size of the scaled frame, which
            // loses the bitmap strike of the pixel font (D-710, F-49).
            Oversampling = false,

            // The world keeps linear light above full white, so a light source can glow and the
            // lit art stays below the threshold (D-910, F-47).
            UseHdr2D = true,

            // The world holds the environment of the glow. A world of its own keeps that glow off
            // the frame and the window, which share the world of the root (D-210).
            World3D = new World3D(),

            // The fog, the hit bursts, and each mark draw in the overlay alone, above the glow (D-916).
            CanvasCullMask = GlowPass.WorldLayers,
        };

        this.AddChild(this.worldViewport);
    }

    private void BuildOverlay()
    {
        // The overlay shares the world, so each node stands at its place in the view with no copy.
        // It has no HDR 2D, so the fog blends as it did before the glow, and it never glows
        // (D-916, F-104).
        this.overlayViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth, WorldHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            TransparentBg = true,
            Oversampling = false,
            World2D = this.worldViewport.World2D,
            CanvasCullMask = GlowPass.AboveGlowLayer,
        };

        this.AddChild(this.overlayViewport);
    }

    private void BuildMarks()
    {
        // The mark view shares the world as the overlay does, and it draws the marks alone, above
        // the tilt-shift blur and the vignette, so each mark stays sharp (D-208, D-919).
        this.markViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth, WorldHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            TransparentBg = true,
            Oversampling = false,
            World2D = this.worldViewport.World2D,
            CanvasCullMask = GlowPass.MarkLayer,
        };

        this.AddChild(this.markViewport);
    }

    private void BuildScene()
    {
        // The scene joins the world and the overlay at the size of the art, so the tilt-shift blur
        // reads the fog, the hit bursts, and the light shafts with the world (D-919).
        this.sceneViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth, WorldHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Oversampling = false,
        };

        this.sceneViewport.AddChild(new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Texture = this.worldViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth, WorldHeight),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,

            // The world holds linear light, and the scene has no HDR 2D, so the view turns each
            // pixel into sRGB (F-103).
            Material = GlowPass.ViewMaterial(),
        });
        this.sceneViewport.AddChild(ViewOf(this.overlayViewport, WorldWidth, WorldHeight));
        this.AddChild(this.sceneViewport);
    }

    private void BuildFrame()
    {
        this.frameViewport = new SubViewport
        {
            Size = new Vector2I(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Oversampling = false,
        };

        // The scene fills the frame at 2x, so one art pixel covers 2 by 2 frame pixels
        // (D-230, D-633). The view reads the scene with a linear filter for the taps of the blur,
        // and the shader reads each sharp pixel at the middle of its art pixel, so the filter
        // never softens it.
        this.blur = LookPasses.BlurMaterial();
        this.vignette = LookPasses.VignetteMaterial();
        var sceneView = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Texture = this.sceneViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth * WorldScale, WorldHeight * WorldScale),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Linear,
            Material = this.blur,
        };

        // The vignette covers the scene, under the marks and the UI (D-919).
        var dark = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth * WorldScale, WorldHeight * WorldScale),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = this.vignette,
        };

        this.Layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        this.frameViewport.AddChild(sceneView);
        this.frameViewport.AddChild(dark);
        this.frameViewport.AddChild(ViewOf(this.markViewport, WorldWidth * WorldScale, WorldHeight * WorldScale));
        this.frameViewport.AddChild(this.Layer);

        // The transition covers the whole frame, the UI included, so it draws last (D-195, D-210).
        this.HandOffPass = TransitionPass.Build(this.frameViewport);
        this.AddChild(this.frameViewport);
    }

    /// <summary>Builds the view of a transparent viewport that shares the world, such as the overlay, at a size.</summary>
    private static TextureRect ViewOf(SubViewport viewport, int width, int height)
    {
        return new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Texture = viewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(width, height),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,

            // A transparent view holds its color times its alpha, so it draws with the blend of
            // premultiplied alpha, and the fog keeps its strength over the world.
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha },
        };
    }

    private void BuildScreenView()
    {
        // The bars are the black around the frame on every shape that is not 16 to 9 (D-228).
        this.bars = new ColorRect { Color = Colors.Black, Position = Vector2.Zero };
        this.screenView = new TextureRect
        {
            StretchMode = TextureRect.StretchModeEnum.Scale,

            // The default expand mode keeps a texture rect at the size of its texture or
            // larger. The first step at 1920 by 1080 draws 2560 by 1440, so the rect then
            // stayed at that size and the window cut the frame, where the rect must scale the
            // picture down (D-573). The rect takes the size that the fit gives it alone.
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };

        this.AddChild(this.bars);
        this.AddChild(this.screenView);
    }

    /// <summary>
    /// Builds or removes the viewport of the first step. The step runs when the whole number
    /// of the fit overshoots the drawn size, such as at 1920 by 1080 (D-573).
    /// </summary>
    private void BuildStep()
    {
        if (this.stepViewport is not null)
        {
            this.stepViewport.QueueFree();
            this.stepViewport = null;
        }

        if (!this.Fit.NeedsSmoothStep)
        {
            // The frame reaches the screen at a whole number, so the Nearest filter alone
            // gives the picture and every pixel keeps its size (D-232, D-573).
            this.screenView.Texture = this.frameViewport.GetTexture();
            this.screenView.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
            return;
        }

        this.stepViewport = new SubViewport
        {
            Size = new Vector2I(this.Fit.StepWidth, this.Fit.StepHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Oversampling = false,
        };

        var stepView = new TextureRect
        {
            Texture = this.frameViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(this.Fit.StepWidth, this.Fit.StepHeight),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };

        this.stepViewport.AddChild(stepView);
        this.AddChild(this.stepViewport);

        // The second step scales that picture down to the screen with a linear filter, which
        // keeps every pixel the same size and allows a slight softness at the edges (D-573).
        this.screenView.Texture = this.stepViewport.GetTexture();
        this.screenView.TextureFilter = CanvasItem.TextureFilterEnum.Linear;
    }
}
