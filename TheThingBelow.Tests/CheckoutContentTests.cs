using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The content of this checkout loads with no error, and its content hash is the same on
/// every platform (G-5, G-6, D-495, D-648).
/// </summary>
/// <remarks>
/// The expected hash lives in a committed file, as the replay-identity set does (D-504). A
/// PR that changes a rule file changes the file too, and the review reads the new value.
/// Each CI leg runs this test, so the three legs and the Mac compare the same value.
/// </remarks>
public sealed class CheckoutContentTests
{
    /// <summary>The file that holds the expected content hash of this checkout.</summary>
    private const string HashFilePath = "TheThingBelow.Tests/identity/content-hash.txt";

    [Fact]
    public void EveryContentFileOfTheCheckoutLoads()
    {
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.NotEmpty(set.Palette.Colors);
        Assert.NotEmpty(set.RuleEntries);
        Assert.True(set.Strings.Count > 0);
    }

    [Fact]
    public void TheContentHashMatchesTheCommittedValue()
    {
        string expected = ReadExpectedHash();

        string hash = ContentHash.Compute(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.Equal(expected, hash);
    }

    [Fact]
    public void EveryStringIdThatARuleNamesIsInTheTable()
    {
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        foreach (RuleFixtureEntry entry in set.RuleEntries)
        {
            Assert.True(
                set.Strings.Contains(entry.Label),
                $"The rule entry '{entry.Id.Value}' names the string id '{entry.Label.Value}', " +
                "and the string table holds no such id (G-7).");
        }
    }

    [Fact]
    public void EveryRuleEntryOfTheCheckoutCarriesTheKindOfItsRecord()
    {
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        foreach (RuleFixtureEntry entry in set.RuleEntries)
        {
            Assert.Equal(RuleFixture.IdKind, entry.Id.Kind);
        }
    }

    [Fact]
    public void EveryContentFileHoldsNoCarriageReturn()
    {
        // The content hash reads the bytes of each file, and Git for Windows checks out CRLF
        // by default. The `eol=lf` rule of `.gitattributes` keeps one set of bytes (T-7).
        // A page of the atlas is a PNG, and the compressed bytes of one hold 13 as data, so
        // the `*.png binary` rule of `.gitattributes` covers it instead (D-666).
        foreach (ContentFile file in ContentFolder.Read(RepositoryRoot.Find()))
        {
            if (ContentPaths.IsAtlasPage(file.Path))
            {
                continue;
            }

            Assert.DoesNotContain((byte)'\r', file.Bytes);
        }
    }

    [Fact]
    public void TheRuleFolderHoldsAtLeastOneFile()
    {
        IReadOnlyList<ContentFile> files = ContentFolder.Read(RepositoryRoot.Find());

        Assert.Contains(files, file => ContentPaths.IsRuleFile(file.Path));
    }

    [Fact]
    public void ThePaletteAndTheStringTableStayOutsideTheRuleFolder()
    {
        // An art batch or a text batch must never move the content hash (D-495, D-648).
        Assert.False(ContentPaths.IsRuleFile(Palette.Path));
        Assert.False(ContentPaths.IsRuleFile(StringTable.Path));
    }

    [Fact]
    public void TheGitAttributesFileHoldsTheLineEndRule()
    {
        string attributes = File.ReadAllText(RepositoryRoot.PathTo(".gitattributes"));

        Assert.Contains("eol=lf", attributes);
    }

    private static string ReadExpectedHash() => ContentHashCommand.ReadCommitted(RepositoryRoot.Find());
}
