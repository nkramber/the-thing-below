using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// Draws one large picture in the world viewport, from the pieces of the atlas (D-516, D-518).
/// The picture holds art pixels, so the frame shows it at 2x like the world (D-816).
/// </summary>
/// <remarks>
/// Each copy is one sprite, and the sprites draw in the order of <see cref="PictureCopies"/>,
/// so a later entry covers an earlier one. A copy at the edge of the picture shows the part
/// of its piece inside the picture alone, as the render of Tools does (D-817).
/// <para>
/// The battle scene of PR-10 draws each backdrop layer with this node, and the drift moves the
/// whole node (D-205, D-522).
/// </para>
/// </remarks>
public partial class PictureView : Node2D
{
    private readonly List<Sprite2D> copies = [];

    /// <summary>The count of sprites that the node draws, one for each copy.</summary>
    public int CopyCount => this.copies.Count;

    /// <summary>Builds one sprite for each copy of a picture.</summary>
    /// <param name="atlas">The pages of the atlas, as textures (D-666).</param>
    /// <param name="picture">The picture.</param>
    /// <param name="content">The content set, which holds the drawing of each piece.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The node already holds a picture (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no drawing of a piece (T-2).</exception>
    public void Build(GameAtlas atlas, LargePicture picture, ContentSet content)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(picture);
        ArgumentNullException.ThrowIfNull(content);

        if (this.copies.Count > 0)
        {
            throw new InvalidOperationException(
                $"The picture view already holds {this.copies.Count} copies, and it builds one picture (T-2).");
        }

        foreach (PictureCopy copy in PictureCopies.Of(picture, content))
        {
            AtlasTexture piece = atlas.Frame(copy.Piece, 0);
            var shown = new AtlasTexture
            {
                Atlas = piece.Atlas,
                Region = new Rect2(piece.Region.Position, new Vector2(copy.Width, copy.Height)),
                FilterClip = true,
            };

            var sprite = new Sprite2D
            {
                Texture = shown,
                Centered = false,
                Position = new Vector2(copy.X, copy.Y),
            };

            this.AddChild(sprite);
            this.copies.Add(sprite);
        }
    }
}
