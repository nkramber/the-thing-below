using System;
using System.Collections;
using System.Globalization;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Notices;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The places of the menu windows and of the notice box, and the fit of each string of the
/// content in its place at both body sizes (D-241, D-707, D-991). The tests read the built Game
/// assembly (D-614).
/// </summary>
public sealed class MenuLayoutTests
{
    private const string Layout = "MenuLayout";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachLabelOfTheMainListFitsTheListAndTheLimitOfAMenuLabel(int body)
    {
        // The game-text-style skill holds a menu label to 16 characters.
        int fits = UiCharacters(body, (int)GameValue.Constant(Layout, "MainListWidth") - (Pad() * 2));
        foreach (object entry in (IEnumerable)GameValue.StaticProperty("MainList", "Entries")!)
        {
            string text = Text((ContentId)GameValue.Static("MainListView", "LabelOf", entry)!);
            Assert.True(text.Length <= 16 && text.Length <= fits, $"The label '{text}' of the entry {entry} passes 16 characters or the {fits} of the list at a body of {body}.");
        }
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachNoticeFitsTheNoticeBoxAndALineOfTheLog(int body)
    {
        // D-221, D-987: the notice box and the log show a notice on one line.
        int box = (int)GameValue.Static(Layout, "NoticeCharacters", body)!;
        int log = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        foreach (NoticeRecord notice in Content.Value.Notices.Records)
        {
            string text = Text(notice.Id);
            Assert.True(text.Length <= box && text.Length <= log, $"The notice '{notice.Id.Value}' holds {text.Length} characters, and the notice box holds {box} and a line of the log {log} at a body of {body}.");
            Assert.DoesNotContain("{", text, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachLineOfTheStatusSheetFitsItsColumnAtTheHighestValues(int body)
    {
        // D-991, D-981: the health and the MP stay at 999 or less, and a name at 8 characters.
        int fits = (int)GameValue.Static(Layout, "StatusColumnCharacters", body)!;
        string most = Content.Value.Battle.Rules.LevelExperience[^1].ToString(CultureInfo.InvariantCulture);
        string[] lines =
        [
            "12345678",
            Fill("menu.level", "level", "40"),
            Text(Id("menu.row_front")),
            Text(Id("menu.row_back")),
            Fill("battle.health", "health", "999", "full", "999"),
            Fill("battle.mp", "mp", "999", "full", "999"),
            Fill("menu.experience", "amount", most),
            Fill("menu.next", "amount", most),
            Fill("menu.stat", "stat", Text(Id("battle.stat_spd")), "value", "999"),
            Text(Id("menu.sound")),
        ];

        foreach (string line in lines)
        {
            Assert.True(line.Length <= fits, $"The status line '{line}' passes the {fits} characters of a column at a body of {body}.");
        }

        foreach (StatusKind status in Statuses.All)
        {
            string name = Text(Id($"status.{Statuses.NameOf(status)}"));
            Assert.True(name.Length <= fits, $"The status '{name}' passes a column at a body of {body}.");
        }
    }

    [Fact]
    public void EachWordOfTheKeyOfTheMapFitsItsPlace()
    {
        int place = (int)GameValue.Constant(Layout, "KeyWordCharacters");
        foreach (string id in new[] { "menu.map_you", "menu.map_door", "menu.map_waystone" })
        {
            Assert.True(Text(Id(id)).Length < place, $"The word of '{id}' fills the place of {place} characters, and the key needs a gap.");
        }
    }

    [Theory]
    [InlineData(24, 48, 20)]
    [InlineData(32, 64, 15)]
    public void TheLogWindowShowsALineCountForEachBodySize(int body, int title, int lines)
    {
        // D-707: a body of 32 holds fewer lines, and the page scrolls the rest of the 30 entries.
        Assert.Equal(lines, (int)GameValue.Static(Layout, "LogLines", body, title)!);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachWindowStaysInsideTheFrame(int body)
    {
        // D-568: every window fits the frame of 1280 by 720.
        foreach (object box in new[] { GameValue.Static(Layout, "MainListBox", body)!, GameValue.Static(Layout, "TaskBox")!, GameValue.Static(Layout, "NoticePlace", body)! })
        {
            int x = Read(box, "X");
            int y = Read(box, "Y");
            Assert.True(x >= 0 && y >= 0 && x + Read(box, "Width") <= 1280 && y + Read(box, "Height") <= 720, $"The box {box} leaves the frame.");
        }
    }

    private static int Pad() => (int)GameValue.Constant(Layout, "Pad");

    private static int UiCharacters(int body, int width) => width / (body / 2);

    private static int Read(object box, string name) => (int)box.GetType().GetProperty(name)!.GetValue(box)!;

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, "test");

    private static string Text(ContentId id) => Content.Value.Strings.Text(id);

    private static string Fill(string id, params string[] pairs)
    {
        string text = Text(Id(id));
        for (int index = 0; index < pairs.Length; index += 2)
        {
            text = text.Replace("{" + pairs[index] + "}", pairs[index + 1], StringComparison.Ordinal);
        }

        return text;
    }
}
