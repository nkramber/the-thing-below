using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The message that a crash shows on screen (D-170, D-559). It names the crash file and the
/// address that takes it, through the one text helper (D-499, G-7).
/// </summary>
/// <remarks>
/// Exit test 9 of PR-61. The address is a placeholder in the reserved `.invalid` top-level
/// domain until the owner names the studio, and the string id is final (D-473, D-712,
/// OQ-57). RFC 2606 reserves `.invalid` for a name that is sure to be invalid.
/// </remarks>
public sealed class CrashMessageTests
{
    private const string ScreenTypeName = "TheThingBelow.Game.Ui.CrashScreen";

    /// <summary>The top-level domain that RFC 2606 reserves for a name that never resolves.</summary>
    private const string ReservedDomain = ".invalid";

    [Fact]
    public void EveryLineOfTheMessageIsInTheStringTable()
    {
        // Exit test 9, G-7. No C# file holds a word of the message.
        StringTable table = Content().Strings;

        foreach (string field in new[] { "TitleId", "FileId", "SendId", "AddressId", "QuitId" })
        {
            string id = Constant(field);
            Assert.True(
                table.Contains(ContentId.Parse(id, StringTable.Path, field)),
                $"The string table holds no id '{id}' of the crash message (G-7, D-559).");
        }
    }

    [Fact]
    public void TheAddressSitsInTheReservedDomain()
    {
        // D-712. The repository is public until Phase 6, so no personal address enters a
        // file (D-4, D-450, D-456). OQ-57 closes with the studio name.
        string address = Text(Constant("AddressId"));

        Assert.EndsWith(ReservedDomain, address, StringComparison.Ordinal);
        Assert.Contains("@", address, StringComparison.Ordinal);
    }

    [Fact]
    public void NoFileOfTheRepositoryHoldsThePersonalAddressOfTheOwner()
    {
        // D-4, D-712. The check reads the string table, which is the one file that carries an
        // address of the game.
        foreach (string id in Content().Strings.Ids)
        {
            string text = Text(id);
            if (!text.Contains('@', StringComparison.Ordinal))
            {
                continue;
            }

            Assert.True(
                text.EndsWith(ReservedDomain, StringComparison.Ordinal),
                $"The string '{id}' holds the address '{text}', and every address of a public "
                + $"build sits in the reserved '{ReservedDomain}' domain (D-4, D-712).");
        }
    }

    [Fact]
    public void TheFileLineAndTheSendLineEachHoldTheirPlace()
    {
        // D-559. The screen fills the name of the crash file and the address, and a place
        // with no value is an error (T-2).
        Assert.Contains($"{{{Constant("FilePlace")}}}", Text(Constant("FileId")), StringComparison.Ordinal);
        Assert.Contains($"{{{Constant("AddressPlace")}}}", Text(Constant("SendId")), StringComparison.Ordinal);
    }

    [Fact]
    public void TheMessageNamesNoFolderOfThePerson()
    {
        // D-170. The screen shows the name of the crash file and never its folder, because a
        // folder path names the person.
        foreach (string field in new[] { "TitleId", "FileId", "SendId", "QuitId" })
        {
            string text = Text(Constant(field));
            Assert.DoesNotContain("/", text, StringComparison.Ordinal);
            Assert.DoesNotContain("\\", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheBootNodeShowsTheMessageThroughTheTextHelper()
    {
        // D-499, D-559. Boot builds the crash screen, and the screen puts each line on a
        // label through the one helper. det-lint proves the second half (DL 8).
        string boot = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Game/scripts/Boot.cs"));

        Assert.Contains("CrashScreen", boot, StringComparison.Ordinal);
        Assert.Contains("ShowCrashMessage", boot, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFrameBuildsInsideTheTryBlockOfItsCaller()
    {
        // Finding P3-6 of the repository review: the frame built in `_Ready`, and the .NET bridge
        // of Godot printed an error of that callback and ran on. A shader that failed to load thus
        // left a half frame, and the crash file named a later error (T-2, G-18). The frame now
        // builds in `FrameRoot.AddTo`, which each caller runs inside its own try block.
        string frame = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Game/scripts/Ui/FrameRoot.cs"));
        Assert.DoesNotContain("override void _Ready", frame, StringComparison.Ordinal);
        Assert.Contains("public static FrameRoot AddTo(Node parent)", frame, StringComparison.Ordinal);

        foreach (string file in Directory.GetFiles(RepositoryRoot.PathTo("TheThingBelow.Game/scripts"), "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            Assert.False(
                text.Contains("new FrameRoot()", StringComparison.Ordinal) && !file.EndsWith("FrameRoot.cs", StringComparison.Ordinal),
                $"{file} builds a frame outside FrameRoot.AddTo, so an error of the build reaches no try block (T-2).");
        }
    }

    [Fact]
    public void ASessionWithNoDisplayQuitsWithTheCrashCode()
    {
        // D-117, T-2. The smoke job of CI runs with no window, so a crash there must end the
        // session and never wait for a press.
        string boot = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.Game/scripts/Boot.cs"));

        Assert.Contains("HeadlessDisplay", boot, StringComparison.Ordinal);
        Assert.Contains("DisplayServer.GetName()", boot, StringComparison.Ordinal);
    }

    private static ContentSet Content() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

    private static string Text(string id) =>
        Content().Strings.Text(ContentId.Parse(id, StringTable.Path, "id"));

    private static string Constant(string field) =>
        (string)GameAssemblyFile.Type(ScreenTypeName).GetField(field)!.GetValue(null)!;
}
