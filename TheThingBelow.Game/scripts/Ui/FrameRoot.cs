using System;
using Godot;
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
/// The glow of each light source draws on a layer that the world view never draws. A mask view
/// shares the world and draws that layer alone, a quarter view blurs the mask, and the frame adds
/// the blur over the world, under the UI (D-188, D-210, D-913, <see cref="GlowPass"/>).
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
    private SubViewport maskViewport = null!;
    private SubViewport blurViewport = null!;
    private TextureRect glowView = null!;
    private ShaderMaterial glowMaterial = null!;
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

    /// <summary>Builds the frame, the world viewport, and the view on the screen.</summary>
    public override void _Ready()
    {
        this.BuildWorld();
        this.BuildGlow();
        this.BuildFrame();
        this.BuildScreenView();

        GetWindow().Oversampling = false;
        GetWindow().SizeChanged += this.OnScreenChanged;
        this.OnScreenChanged();
    }

    /// <summary>Shows the glow pass over the world with the values of the glow file, on a map or in a fight (D-913).</summary>
    /// <param name="glow">The glow file.</param>
    /// <exception cref="ArgumentNullException">The glow is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The frame is not in the tree yet, so it holds no glow pass (T-2).</exception>
    /// <remarks>Each screen of the world calls this method with the same file, so a second call changes nothing.</remarks>
    public void ShowGlow(Glow glow)
    {
        ArgumentNullException.ThrowIfNull(glow);

        if (this.glowView is null)
        {
            throw new InvalidOperationException("The frame holds no glow pass before it enters the tree (T-2).");
        }

        GlowPass.SetValues(this.glowMaterial, glow);
        this.glowView.Visible = true;
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

            // The world never draws the glow of a source, which the glow pass alone reads (D-913).
            CanvasCullMask = GlowPass.WorldLayers,
        };

        this.AddChild(this.worldViewport);
    }

    private void BuildGlow()
    {
        // The mask shares the world, so each glow stands at its place in the view with no copy.
        this.maskViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth, WorldHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            TransparentBg = true,
            World2D = this.worldViewport.World2D,
            CanvasCullMask = GlowPass.GlowLayer,
        };

        this.blurViewport = new SubViewport
        {
            Size = new Vector2I(WorldWidth / GlowPass.BlurCell, WorldHeight / GlowPass.BlurCell),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };

        // The blur reads the mask with the linear filter, so four reads give the mean of 4 by 4 art
        // pixels. The expand mode comes before the size: the default mode holds the rect at the size
        // of its texture, and the blur then read a quarter of the mask at 1 to 1.
        this.blurViewport.AddChild(new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Texture = this.maskViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth / GlowPass.BlurCell, WorldHeight / GlowPass.BlurCell),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Linear,
            Material = GlowPass.BlurMaterial(),
        });

        this.AddChild(this.maskViewport);
        this.AddChild(this.blurViewport);
    }

    private void BuildFrame()
    {
        this.frameViewport = new SubViewport
        {
            Size = new Vector2I(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Oversampling = false,
        };

        // The world fills the frame at 2x, so one art pixel covers 2 by 2 frame pixels
        // (D-230, D-633).
        var worldView = new TextureRect
        {
            Texture = this.worldViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth * WorldScale, WorldHeight * WorldScale),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };

        // The glow adds over the world and under the UI (D-210). It shows once a screen gives it
        // the glow file, so a screen with no world draws no pass.
        this.glowMaterial = GlowPass.AddMaterial();
        this.glowView = new TextureRect
        {
            Texture = this.blurViewport.GetTexture(),
            Position = Vector2.Zero,
            Size = new Vector2(WorldWidth * WorldScale, WorldHeight * WorldScale),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            TextureFilter = CanvasItem.TextureFilterEnum.Linear,
            Material = this.glowMaterial,
            Visible = false,
        };

        this.Layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        this.frameViewport.AddChild(worldView);
        this.frameViewport.AddChild(this.glowView);
        this.frameViewport.AddChild(this.Layer);
        this.AddChild(this.frameViewport);
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
