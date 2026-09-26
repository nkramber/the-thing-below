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
    public void EachChoiceOfTheWindowOfAServiceFitsWithItsHelp(int body)
    {
        // D-1131, D-1132: a label holds 16 characters at most, and a line of help one line of the window.
        int fits = (int)GameValue.Static(Layout, "ServiceLineCharacters", body)!;
        foreach (Core.Maps.ServiceKind kind in new[] { Core.Maps.ServiceKind.Rest, Core.Maps.ServiceKind.Save })
        {
            foreach (object option in (IEnumerable)GameValue.StaticProperty("ServiceChoice", "Options")!)
            {
                string label = Text((ContentId)GameValue.Static("ServiceView", "LabelOf", kind, option)!);
                string help = Text((ContentId)GameValue.Static("ServiceView", "HelpOf", kind, option)!);
                Assert.True(label.Length <= 16 && label.Length <= fits, $"The label '{label}' passes 16 characters or the {fits} of the window at a body of {body}.");
                Assert.True(help.Length <= fits, $"The help '{help}' passes the {fits} characters of the window at a body of {body}.");
            }
        }
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachLineOfTheShopWindowTheGoldAndThePricedRestFits(int body)
    {
        // D-1156, D-1158 to D-1160, D-1164: the shop menu and the gold panel stand as wide as the main
        // list, and each line of the list window fits the task line with the largest numbers of a
        // region: a price of 9999, a count of 10, and a gold of 999999.
        int list = UiCharacters(body, (int)GameValue.Constant(Layout, "MainListWidth") - (Pad() * 2));
        foreach (object mode in (IEnumerable)GameValue.StaticProperty("ShopCursor", "Modes")!)
        {
            string label = Text((ContentId)GameValue.Static("ShopView", "ModeIdOf", mode)!);
            Assert.True(label.Length <= 16 && label.Length <= list, $"The label '{label}' passes 16 characters or the {list} of the shop menu at a body of {body}.");
        }

        Assert.True(Fill("menu.gold", "gold", "999999").Length <= list, $"The gold line passes the {list} characters of its panel at a body of {body}.");
        Assert.True(Fill("menu.rest_priced", "price", "999").Length <= 16, "The priced rest passes 16 characters.");

        int task = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        int right = task - (task * 45 / 100);
        string[] rights =
        [
            Fill("menu.shop_entry_left", "price", "9999", "count", "10", "limit", "10", "left", "10"),
            Fill("menu.shop_sale", "each", "9999", "count", "10"),
            Fill("menu.shop_unwanted", "count", "10"),
        ];
        foreach (string text in rights)
        {
            Assert.True(text.Length <= right, $"The entry '{text}' passes the {right} characters of the right column at a body of {body}.");
        }

        foreach (string id in new[] { "menu.shop_buy_help", "menu.shop_sell_help", "menu.shop_no_room", "menu.shop_no_gold", "menu.shop_not_bought", "menu.shop_empty", "menu.rest_short" })
        {
            Assert.True(Text(Id(id)).Length <= task, $"The line of '{id}' passes the {task} characters of the window at a body of {body}.");
        }

        Assert.True(Fill("menu.shop_count", "count", "10", "total", "99990").Length <= task, $"The count line passes the {task} characters at a body of {body}.");
        int cell = (int)GameValue.Static("ShopView", "StatCellCharacters", body)!;
        Assert.True(Fill("menu.gear_gain", "change", "99", "value", "999").Length < cell, $"A stat cell of {cell} characters holds no change of 99 on 999 at a body of {body}.");
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void ThePartyWindowHoldsTheReserveOfTheFirstRegionAndEachOfItsLinesFits(int body)
    {
        // D-58, D-1136: two characters wait in the reserve by the end of region one, and each line
        // of help and each action fits a line of the window.
        int title = body * Content.Value.Style.TitleScale;
        int room = (int)GameValue.Static("PartyView", "MostReserveLines", body, title)!;
        Assert.True(room >= 2, $"The party window holds {room} reserve lines at a body of {body}.");

        int fits = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        foreach (string id in new[] { "menu.party_help", "menu.party_reserve_help", "menu.swap_help", "menu.reserve" })
        {
            Assert.True(Text(Id(id)).Length <= fits, $"The line of '{id}' passes the {fits} characters of the window at a body of {body}.");
        }

        foreach (object action in (IEnumerable)GameValue.StaticProperty("PartyList", "Actions")!)
        {
            string label = Text((ContentId)GameValue.Static("PartyView", "ActionIdOf", action)!);
            Assert.True(label.Length <= 16, $"The action '{label}' passes 16 characters.");
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

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachLineOfTheStatusSheetFitsTheHeightOfTheWindow(int body)
    {
        // D-1056: the sheet gained MAG and RES, and each line still stands above the bottom edge.
        int title = body * Content.Value.Style.TitleScale;
        int sheet = (int)GameValue.StaticProperty("StatusView", "SheetLines")!;
        int room = (int)GameValue.Static(Layout, "LogLines", body, title)!;

        Assert.True(sheet <= room, $"The status sheet holds {sheet} lines, and the window holds {room} at a body of {body}.");
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachCellOfTheStatsOfTheGearWindowFitsAtTheHighestValues(int body)
    {
        // D-1060: a cell holds a stat of 999 with its name, or a change of 99 with the stat.
        int fits = (int)GameValue.Static("GearView", "StatCellCharacters", body)!;
        string[] cells =
        [
            Fill("menu.stat", "stat", Text(Id("battle.stat_res")), "value", "999"),
            Fill("menu.gear_gain", "change", "99", "value", "999"),
            Fill("menu.gear_loss", "change", "99", "value", "999"),
            Fill("menu.gear_same", "value", "999"),
        ];

        foreach (string cell in cells)
        {
            Assert.True(cell.Length < fits, $"The cell '{cell}' fills the {fits} characters of a cell at a body of {body}, and the next cell needs a gap.");
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
        foreach (object box in new[] { GameValue.Static(Layout, "MainListBox", body)!, GameValue.Static(Layout, "ServiceBox", body)!, GameValue.Static(Layout, "TaskBox")!, GameValue.Static(Layout, "NoticePlace", body)! })
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
