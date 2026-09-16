using Godot;

namespace DeckTest;

/// <summary>
/// Makes every texture of the test scene in memory, so the spike commits no binary file.
/// Each tile and sprite carries a normal map, because the light model of D-183 needs one.
/// </summary>
public static class TextureFactory
{
    /// <summary>A tile with a bevelled normal map, so light wraps around its edge (D-183).</summary>
    public static CanvasTexture MakeBevelledTile(int size, Color tint, ulong seed)
    {
        var rng = new RandomNumberGenerator { Seed = seed };
        var diffuse = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var normal = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float bevel = size * 0.25f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float grain = rng.RandfRange(-0.06f, 0.06f);
                diffuse.SetPixel(x, y, new Color(
                    Mathf.Clamp(tint.R + grain, 0f, 1f),
                    Mathf.Clamp(tint.G + grain, 0f, 1f),
                    Mathf.Clamp(tint.B + grain, 0f, 1f),
                    1f));

                float nx = EdgeSlope(x, size, bevel);
                float ny = EdgeSlope(y, size, bevel);
                normal.SetPixel(x, y, EncodeNormal(nx, ny));
            }
        }

        return new CanvasTexture
        {
            DiffuseTexture = ImageTexture.CreateFromImage(diffuse),
            NormalTexture = ImageTexture.CreateFromImage(normal),
        };
    }

    /// <summary>A round sprite with a dome normal map, which stands for a character or a prop.</summary>
    public static CanvasTexture MakeDomedSprite(int size, Color tint)
    {
        var diffuse = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var normal = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - radius) / radius;
                float dy = (y + 0.5f - radius) / radius;
                float r2 = (dx * dx) + (dy * dy);

                if (r2 > 1f)
                {
                    diffuse.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    normal.SetPixel(x, y, EncodeNormal(0f, 0f));
                    continue;
                }

                diffuse.SetPixel(x, y, new Color(tint.R, tint.G, tint.B, 1f));
                normal.SetPixel(x, y, EncodeNormal(dx, dy));
            }
        }

        return new CanvasTexture
        {
            DiffuseTexture = ImageTexture.CreateFromImage(diffuse),
            NormalTexture = ImageTexture.CreateFromImage(normal),
        };
    }

    /// <summary>The backdrop of the place, drawn behind the map (D-205).</summary>
    public static ImageTexture MakeBackdrop(int width, int height, ulong seed)
    {
        var rng = new RandomNumberGenerator { Seed = seed };
        var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);

        for (int y = 0; y < height; y++)
        {
            float depth = (float)y / height;
            for (int x = 0; x < width; x++)
            {
                float grain = rng.RandfRange(-0.03f, 0.03f);
                image.SetPixel(x, y, new Color(
                    Mathf.Clamp(0.06f + (depth * 0.10f) + grain, 0f, 1f),
                    Mathf.Clamp(0.07f + (depth * 0.09f) + grain, 0f, 1f),
                    Mathf.Clamp(0.12f + (depth * 0.14f) + grain, 0f, 1f),
                    1f));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    /// <summary>A soft round dot. The light texture and the particle texture both use it.</summary>
    public static ImageTexture MakeSoftDot(int size, float falloff)
    {
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - radius) / radius;
                float dy = (y + 0.5f - radius) / radius;
                float d = Mathf.Sqrt((dx * dx) + (dy * dy));
                float a = Mathf.Clamp(1f - d, 0f, 1f);
                a = Mathf.Pow(a, falloff);
                image.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    /// <summary>The slope of the bevel at one axis: -1 at the low edge, +1 at the high edge.</summary>
    private static float EdgeSlope(int position, int size, float bevel)
    {
        float low = position + 0.5f;
        float high = size - low;

        if (low < bevel)
        {
            return -(1f - (low / bevel));
        }

        if (high < bevel)
        {
            return 1f - (high / bevel);
        }

        return 0f;
    }

    /// <summary>Packs a tangent-space normal into a colour, with +Y up as Godot reads it.</summary>
    private static Color EncodeNormal(float nx, float ny)
    {
        var n = new Vector3(nx, ny, 1f).Normalized();
        return new Color((n.X * 0.5f) + 0.5f, (-n.Y * 0.5f) + 0.5f, (n.Z * 0.5f) + 0.5f, 1f);
    }
}
