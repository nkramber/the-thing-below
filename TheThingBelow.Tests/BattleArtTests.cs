using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The art of the battle screen: a battle drawing for each combatant, the attack pose of each
/// character, the pointer, and the 18 icons in the palette color of each element and status
/// (D-214, D-811, D-828, D-833). The hit flash shader keeps away from the normal map (D-183,
/// D-825).
/// </summary>
public sealed class BattleArtTests
{
    /// <summary>The use of the battle drawing of a combatant (D-828).</summary>
    private const string BattleUse = "battle";

    /// <summary>The use of the attack pose of a character (D-828).</summary>
    private const string AttackUse = "battle_attack";

    /// <summary>The use of an icon (D-214).</summary>
    private const string IconUse = "icon";

    /// <summary>The size of an icon, in pixels (`area-art.md` section 7.4).</summary>
    private const int IconSize = 16;

    /// <summary>The ink of the outline of each icon, which the count of the main color skips.</summary>
    private const char Ink = 'k';

    /// <summary>The path of the hit flash shader in the checkout (D-825).</summary>
    private const string ShaderPath = "TheThingBelow.Game/shaders/hit_flash.gdshader";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    /// <summary>
    /// The palette key of each element and each status, from section 7.2 of `area-art.md`.
    /// The review of PR-10 confirmed each pick (D-811).
    /// </summary>
    public static TheoryData<string, char> IconColors => new()
    {
        { "element.fire", 'X' },
        { "element.ice", 'C' },
        { "element.lightning", 'y' },
        { "element.earth", 't' },
        { "element.wind", '7' },
        { "element.water", 'c' },
        { "element.holy", '#' },
        { "element.dark", 'P' },
        { "status.poison", '%' },
        { "status.blind", 'D' },
        { "status.silence", '8' },
        { "status.sleep", '9' },
        { "status.slow", '!' },
        { "status.haste", '2' },
        { "status.stun", 'O' },
        { "status.bleed", 'R' },
        { "status.regen", 'v' },
        { "status.shell", '&' },
    };

    [Fact]
    public void EveryElementAndEveryStatusHasAnIcon()
    {
        // D-214: 18 icons, one for each of the eight elements and the ten statuses.
        var things = new List<string>();
        foreach (Element element in Elements.All)
        {
            things.Add($"element.{Elements.NameOf(element)}");
        }

        foreach (StatusKind status in Statuses.All)
        {
            things.Add($"status.{Statuses.NameOf(status)}");
        }

        Assert.Equal(18, things.Count);
        foreach (string thing in things)
        {
            ContentId id = ContentId.Parse(thing, "test", "thing");
            Assert.True(Content.Value.Atlas.Draws(id, IconUse), $"The atlas holds no icon of '{thing}' (D-214).");
            AtlasEntry entry = Content.Value.Atlas.Entry(id, IconUse);
            Assert.Equal(IconSize, entry.Width);
            Assert.Equal(IconSize, entry.Height);
            Assert.Equal("ui", entry.Page);
        }
    }

    [Theory]
    [MemberData(nameof(IconColors))]
    public void EachIconTakesThePaletteColorOfItsElementOrStatus(string thing, char key)
    {
        // D-811: the main color of an icon is the pick of the palette table, so a player who
        // knows the color knows the icon. The outline ink never counts.
        ContentId id = ContentId.Parse(thing, "test", "thing");
        Drawing drawing = Content.Value.DrawingOf(Content.Value.Atlas.Entry(id, IconUse).Id);
        var counts = new SortedDictionary<char, int>();
        foreach (string row in drawing.Frames[0].Rows)
        {
            foreach (char pixel in row)
            {
                if (pixel != Drawing.Transparent && pixel != Ink)
                {
                    counts[pixel] = counts.TryGetValue(pixel, out int count) ? count + 1 : 1;
                }
            }
        }

        char main = Ink;
        int most = 0;
        foreach (KeyValuePair<char, int> pair in counts)
        {
            if (pair.Value > most)
            {
                main = pair.Key;
                most = pair.Value;
            }
        }

        Assert.True(main == key, $"The icon of '{thing}' draws most in '{main}', and the palette table gives '{key}' (D-811).");
    }

    [Fact]
    public void EveryCombatantHasABattleDrawingAndEveryCharacterAnAttackPose()
    {
        // D-828: a fight needs a drawing of each character and each enemy. The screen fails
        // on an absent one, so this test finds it first (T-2).
        BattleContent battle = Content.Value.Battle;
        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            Assert.True(Content.Value.Atlas.Draws(character.Id, BattleUse), $"The atlas holds no battle drawing of '{character.Id.Value}'.");
            Assert.True(Content.Value.Atlas.Draws(character.Id, AttackUse), $"The atlas holds no attack pose of '{character.Id.Value}'.");
        }

        foreach (EnemyRecord enemy in battle.Enemies)
        {
            Assert.True(Content.Value.Atlas.Draws(enemy.Id, BattleUse), $"The atlas holds no battle drawing of '{enemy.Id.Value}'.");
            AtlasEntry entry = Content.Value.Atlas.Entry(enemy.Id, BattleUse);
            int size = EnemySizes.SideOf(enemy.Size) * AtlasPages.TileSize;
            Assert.True(entry.Width == size && entry.Height == size, $"The battle drawing of '{enemy.Id.Value}' is {entry.Width} by {entry.Height}, and its size gives {size} (D-236).");
        }
    }

    [Fact]
    public void ThePointerIsADrawingOfTheUiPage()
    {
        // D-833: a pointer of 8 by 8 pixels marks the target.
        AtlasEntry entry = Content.Value.Atlas.Entry(ContentId.Parse("ui.pointer", "test", "thing"), "pointer");

        Assert.Equal(8, entry.Width);
        Assert.Equal(8, entry.Height);
        Assert.Equal("ui", entry.Page);
    }

    [Fact]
    public void TheHitFlashShaderNeverWritesTheNormalMap()
    {
        // D-183, D-825: Godot corrects the normal of a flipped draw before the shader code, and
        // a write of NORMAL_MAP would replace that correction.
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ShaderPath));
        string code = CodeOf(text);

        Assert.Contains("shader_type canvas_item;", code, StringComparison.Ordinal);
        Assert.DoesNotContain("NORMAL_MAP", code, StringComparison.Ordinal);
        Assert.DoesNotContain("NORMAL", code, StringComparison.Ordinal);
    }

    [Fact]
    public void TheShaderHoldsTheValuesThatGameSets()
    {
        // D-825: Game sets `flash_amount` and `flash_color` by name, and a wrong name fails in
        // the log alone, so this test reads both names (T-2).
        string code = CodeOf(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ShaderPath)));
        Type screen = GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleScreen");

        foreach (string field in new[] { "FlashAmount", "FlashColor" })
        {
            string name = (string)screen.GetField(field)!.GetValue(null)!;
            Assert.Matches($@"uniform \w+ {name}\b", code);
        }

        string path = (string)screen.GetField("FlashShaderPath")!.GetValue(null)!;
        Assert.Equal("res://shaders/hit_flash.gdshader", path);
        Assert.EndsWith(path["res://".Length..], ShaderPath, StringComparison.Ordinal);
    }

    /// <summary>Gives the code of a shader with every comment line removed.</summary>
    private static string CodeOf(string text)
    {
        var lines = new List<string>();
        foreach (string line in text.Split('\n'))
        {
            if (!line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines.Add(line);
            }
        }

        return string.Join('\n', lines);
    }
}
