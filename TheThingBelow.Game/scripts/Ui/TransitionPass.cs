using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The full-screen pass of the hand-off: the transition into a fight, the fade into it, and the
/// fade back to the map (D-195, D-523, D-938, D-939).
/// </summary>
/// <remarks>
/// One rectangle covers the whole frame above the UI, so a transition covers the menus too (D-195,
/// D-210). It draws one phase at a time, so the hand-off costs one full-screen pass (D-923). Each
/// look has its shader file, and the fade has one of its own (D-825). At the reduced level and at
/// off, the fade takes the place of the color split (D-863).
/// </remarks>
public sealed class TransitionPass
{
    /// <summary>The folder of the shader files of the transitions, as Godot reads it.</summary>
    public const string ShaderFolder = "res://shaders/";

    /// <summary>The start of the name of each shader file of a transition.</summary>
    public const string ShaderPrefix = "transition_";

    /// <summary>The path of the shader of the fade (D-938).</summary>
    public const string FadeShaderPath = "res://shaders/transition_fade.gdshader";

    /// <summary>The name of the uniform of the part of the way to a full cover, from 0 to 1.</summary>
    public const string ProgressName = "progress";

    /// <summary>The name of the uniform of the palette color of the cover (D-939).</summary>
    public const string CoverName = "cover";

    /// <summary>The name of the uniform of the colors of the palette, which the color split reads (G-27).</summary>
    public const string PaletteName = "palette";

    /// <summary>The name of the uniform of the count of colors of the palette.</summary>
    public const string PaletteCountName = "palette_count";

    /// <summary>The most colors that the uniform of the palette holds (D-181).</summary>
    public const int MostPaletteColors = 64;

    private readonly ColorRect rect;
    private readonly Dictionary<TransitionLook, ShaderMaterial> looks = [];
    private ShaderMaterial? fade;

    private TransitionPass(ColorRect rect)
    {
        this.rect = rect;
    }

    /// <summary>Gives the path of the shader file of one look.</summary>
    /// <param name="look">The look.</param>
    /// <returns>The path, such as `res://shaders/transition_shatter.gdshader`.</returns>
    public static string ShaderPathOf(TransitionLook look) => $"{ShaderFolder}{ShaderPrefix}{Transition.NameOf(look)}.gdshader";

    /// <summary>Builds the pass as the last child of a viewport of the frame, so it draws above the UI.</summary>
    /// <param name="frame">The viewport of the frame of 1280 by 720 (D-568).</param>
    /// <returns>The pass, which draws nothing until <see cref="Show"/>.</returns>
    /// <exception cref="ArgumentNullException">The viewport is null (T-2).</exception>
    public static TransitionPass Build(SubViewport frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var rect = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false,
        };

        frame.AddChild(rect);
        return new TransitionPass(rect);
    }

    /// <summary>Draws the phase of the hand-off at one tick, or nothing outside a phase.</summary>
    /// <param name="handOff">The hand-off of the run.</param>
    /// <param name="transitions">The transitions and their table.</param>
    /// <param name="tick">The tick of the run now.</param>
    /// <param name="level">The level of the flash and shake reduction (D-863).</param>
    /// <param name="palette">The palette, which gives each cover key its color (D-181).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">A phase plays with no transition, or Godot loaded no shader (T-2).</exception>
    public void Show(ScreenHandOff handOff, TransitionContent transitions, long tick, EffectLevel level, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(handOff);
        ArgumentNullException.ThrowIfNull(transitions);
        ArgumentNullException.ThrowIfNull(palette);

        int progress = handOff.ProgressOf(tick);
        int toCover = ScreenHandOff.ProgressScale - progress;
        switch (handOff.Phase)
        {
            case HandOffPhase.None:
            case HandOffPhase.Waiting:
                this.rect.Visible = false;
                break;
            case HandOffPhase.Into:
                this.ShowLook(TransitionOf(handOff, tick), progress, level, palette);
                break;
            case HandOffPhase.FadeIn:
                this.ShowFade(TransitionOf(handOff, tick).Cover, toCover, palette, TransitionOf(handOff, tick).File);
                break;
            case HandOffPhase.Back:
                this.ShowFade(transitions.Table.BackCover, toCover, palette, TransitionTable.Path);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(handOff), handOff.Phase, "The value names no phase of the hand-off (T-2).");
        }
    }

    /// <summary>
    /// Draws one look at a fixed progress, with no run, for a capture of the screen test (D-172,
    /// exit tests 1 and 2 of PR-60).
    /// </summary>
    /// <param name="transition">The transition.</param>
    /// <param name="progress">The progress, from 0 to <see cref="ScreenHandOff.ProgressScale"/>.</param>
    /// <param name="level">The level of the flash and shake reduction (D-863).</param>
    /// <param name="palette">The palette.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public void ShowAt(Transition transition, int progress, EffectLevel level, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(transition);
        ArgumentNullException.ThrowIfNull(palette);

        this.ShowLook(transition, progress, level, palette);
    }

    /// <summary>Gives the look that one level draws: the fade in the place of the color split at the reduced level and at off (D-863).</summary>
    /// <param name="look">The look of the transition.</param>
    /// <param name="level">The level of the flash and shake reduction.</param>
    /// <returns>The look, or no value for the fade.</returns>
    public static TransitionLook? DrawnLookOf(TransitionLook look, EffectLevel level) =>
        look == TransitionLook.ColorSplit && level != EffectLevel.Full ? null : look;

    private static Transition TransitionOf(ScreenHandOff handOff, long tick) =>
        handOff.Transition ?? throw new InvalidOperationException(
            $"The phase '{handOff.Phase}' of the hand-off plays at tick {tick} with no transition (D-939, T-2).");

    private void ShowLook(Transition transition, int progress, EffectLevel level, Palette palette)
    {
        if (DrawnLookOf(transition.Look, level) is not TransitionLook look)
        {
            this.ShowFade(transition.Cover, progress, palette, transition.File);
            return;
        }

        ShaderMaterial material = this.MaterialOf(look, palette);
        material.SetShaderParameter(CoverName, LookPasses.ColorOf(palette, transition.Cover, transition.File));
        material.SetShaderParameter(ProgressName, (float)progress / ScreenHandOff.ProgressScale);
        this.rect.Material = material;
        this.rect.Visible = true;
    }

    private void ShowFade(char cover, int toCover, Palette palette, string file)
    {
        this.fade ??= new ShaderMaterial { Shader = LoadShader(FadeShaderPath) };
        this.fade.SetShaderParameter(CoverName, LookPasses.ColorOf(palette, cover, file));
        this.fade.SetShaderParameter(ProgressName, (float)toCover / ScreenHandOff.ProgressScale);
        this.rect.Material = this.fade;
        this.rect.Visible = true;
    }

    private ShaderMaterial MaterialOf(TransitionLook look, Palette palette)
    {
        if (this.looks.TryGetValue(look, out ShaderMaterial? found))
        {
            return found;
        }

        var material = new ShaderMaterial { Shader = LoadShader(ShaderPathOf(look)) };
        if (look == TransitionLook.ColorSplit)
        {
            // The color split takes the nearest palette color for each pixel, so no pixel leaves
            // the palette (G-27).
            if (palette.Colors.Count > MostPaletteColors)
            {
                throw new InvalidOperationException(
                    $"The palette holds {palette.Colors.Count} colors, and the shader of the color split holds {MostPaletteColors} (D-181, T-2).");
            }

            var colors = new Color[MostPaletteColors];
            for (int index = 0; index < palette.Colors.Count; index += 1)
            {
                PaletteColor color = palette.Colors[index];
                colors[index] = Color.Color8((byte)color.Red, (byte)color.Green, (byte)color.Blue);
            }

            material.SetShaderParameter(PaletteName, colors);
            material.SetShaderParameter(PaletteCountName, palette.Colors.Count);
        }

        this.looks.Add(look, material);
        return material;
    }

    private static Shader LoadShader(string path)
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(path);
        return loaded ?? throw new InvalidOperationException($"Godot loaded no shader from '{path}' (D-825, T-2).");
    }
}
