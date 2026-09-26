using System;
using System.Globalization;

namespace TheThingBelow.Game.Ui;

/// <summary>The parts of the dialogue box that one frame draws again.</summary>
/// <param name="Line">True when the line of a new say step shows, so its text goes to the box again.</param>
/// <param name="Speaker">True when the speaker changed, so the portrait and the name plate draw again.</param>
public readonly record struct DialogueParts(bool Line, bool Speaker);

/// <summary>
/// Tells the dialogue box which parts to draw again on each frame: the line when a new say step
/// shows, and the portrait and the name plate when the speaker changes (D-223, D-997).
/// </summary>
/// <remarks>
/// Two say steps can show one line with two speakers, so the line id alone never decides a draw.
/// The class holds no engine type, so a test drives it with <see cref="ScenePlay"/> alone (D-614).
/// </remarks>
public sealed class DialogueChange
{
    private string? line;
    private string? speaker;

    /// <summary>Gives the parts to draw again for the story scene on screen now, and keeps them as shown.</summary>
    /// <param name="play">The story scene on screen.</param>
    /// <returns>The parts. Both are false while the box hides.</returns>
    /// <exception cref="ArgumentNullException">The play is null (T-2).</exception>
    public DialogueParts Take(ScenePlay play)
    {
        ArgumentNullException.ThrowIfNull(play);

        if (play.Line is null)
        {
            // The box hides, so the next line draws in full, even the same line again.
            this.line = null;
            this.speaker = null;
            return new DialogueParts(false, false);
        }

        string lineKey = string.Create(CultureInfo.InvariantCulture, $"{play.Scene?.Value} {play.LineStep} {play.Line.Value}");
        string speakerKey = play.Speaker?.Describe() ?? string.Empty;
        var parts = new DialogueParts(
            string.CompareOrdinal(this.line, lineKey) != 0,
            this.speaker is null || string.CompareOrdinal(this.speaker, speakerKey) != 0);
        this.line = lineKey;
        this.speaker = speakerKey;
        return parts;
    }
}
