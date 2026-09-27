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
        foreach (object kind in new[] { GameValue.Enum("MenuWindowKind", "Rest"), GameValue.Enum("MenuWindowKind", "Save") })
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
        // D-1156, D-1158, D-1160, D-1165, D-1167: the shop menu, the gold panel, and the stats panel
        // stand as wide as the main list. Each column of the list window and each line of the popup
        // fit with the largest numbers of a region: a price of 9999, a count of 10, and a gold of 999999.
        int list = UiCharacters(body, (int)GameValue.Constant(Layout, "MainListWidth") - (Pad() * 2));
        foreach (object mode in (IEnumerable)GameValue.StaticProperty("ShopCursor", "Modes")!)
        {
            string label = Text((ContentId)GameValue.Static("ShopView", "ModeIdOf", mode)!);
            Assert.True(label.Length <= 16 && label.Length <= list, $"The label '{label}' passes 16 characters or the {list} of the shop menu at a body of {body}.");
        }

        Assert.True(Fill("menu.gold", "gold", "999999").Length <= list, $"The gold line passes the {list} characters of its panel at a body of {body}.");
        Assert.True(Fill("menu.stat", "stat", "ATK", "value", "-99").Length <= list, $"A stat line passes the {list} characters of the stats panel at a body of {body}.");
        Assert.True(Fill("menu.rest_priced", "price", "999").Length <= 16, "The priced rest passes 16 characters.");

        int task = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        int column = task * 25 / 100;
        foreach (string text in new[] { Fill("menu.shop_price", "price", "9999"), Text(Id("menu.shop_unwanted")) })
        {
            Assert.True(text.Length < column, $"The price '{text}' passes the {column} characters of its column at a body of {body}.");
        }

        foreach (string text in new[] { Fill("menu.shop_left", "left", "10"), Fill("menu.shop_held", "count", "10") })
        {
            Assert.True(text.Length <= task - (task * 70 / 100), $"The amount '{text}' passes its column at a body of {body}.");
        }

        foreach (string id in new[] { "menu.shop_buy_help", "menu.shop_sell_help", "menu.shop_not_bought", "menu.shop_empty", "menu.rest_short", "menu.shop_equip_ask", "menu.shop_equip_who", "menu.shop_replace", "menu.yes", "menu.no" })
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
    public void TheLineOfTheCharacterInTheGearWindowIsOneStringThatFits(int body)
    {
        // D-1214: one space on each side of the hyphen for every name, and a name at 8 characters
        // with level 40 fits a line of the window (D-981).
        Assert.Equal("Marrek - Level 1", Fill("menu.gear_who", "name", "Marrek", "level", "1"));

        int fits = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        string longest = Fill("menu.gear_who", "name", "12345678", "level", "40");
        Assert.True(longest.Length <= fits, $"The line '{longest}' passes the {fits} characters of the window at a body of {body}.");
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachNoticeFitsTheNoticeBoxAndALineOfTheLog(int body)
    {
        // D-221, D-987: the notice box and the log show a notice on one line. A notice of a chest
        // fills its place with the longest singular of the content and the largest gold, and it
        // never logs, because the log holds the id alone (D-1224).
        int box = (int)GameValue.Static(Layout, "NoticeCharacters", body)!;
        int log = (int)GameValue.Static(Layout, "TaskLineCharacters", body)!;
        string longest = string.Empty;
        foreach (string id in Content.Value.Strings.Ids)
        {
            if (id.StartsWith("single.", StringComparison.Ordinal) && Text(Id(id)).Length > longest.Length)
            {
                longest = Text(Id(id));
            }
        }

        foreach (NoticeRecord notice in Content.Value.Notices.Records)
        {
            string line = Text(notice.Id);
            Assert.False(notice.Logs && line.Contains('{', StringComparison.Ordinal), $"The notice '{notice.Id.Value}' logs and holds a place, and the log holds the id alone (D-1224).");
            string text = Fill(notice.Id.Value, "thing", longest, "count", int.MaxValue.ToString(CultureInfo.InvariantCulture));
            Assert.True(text.Length <= box && text.Length <= log, $"The notice '{notice.Id.Value}' holds {text.Length} characters, and the notice box holds {box} and a line of the log {log} at a body of {body}.");
            Assert.DoesNotContain("{", text, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void EachLineOfTheStatusSheetFitsItsColumnAtTheHighestValues(int body)
    {
        // D-991, D-981: the health and the AP stay at 999 or less, and a name at 8 characters.
        int fits = (int)GameValue.Static(Layout, "StatusColumnCharacters", body)!;
        string most = Content.Value.Battle.Rules.LevelExperience[^1].ToString(CultureInfo.InvariantCulture);
        string[] lines =
        [
            "12345678",
            Fill("menu.level", "level", "40"),
            Text(Id("menu.row_front")),
            Text(Id("menu.row_back")),
            Fill("battle.health", "health", "999", "full", "999"),
            Fill("battle.ap", "ap", "999", "full", "999"),
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
        // D-1060, D-1168, D-1169: a cell holds the name of a stat, or a stat of 999 with a change of 99.
        int fits = (int)GameValue.Static("GearView", "StatCellCharacters", body)!;
        string[] cells =
        [
            Text(Id("battle.stat_res")),
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
    [InlineData(24)]
    [InlineData(32)]
    public void AnEmptyLineStandsAboveTheHelpOfAServiceWindow(int body)
    {
        // D-1172, from the playtest of PR-65: the rest and the save windows hold two choices, an
        // empty line, and the line of help. The old window put the help right under "Leave".
        int line = body + 4;

        Assert.Equal(3, (int)GameValue.Constant(Layout, "ServiceHelpRow"));
        Assert.Equal((Pad() * 2) + (line * 4), Read(GameValue.Static(Layout, "ServiceBox", body)!, "Height"));
    }

    [Fact]
    public void TheDialogueBoxHoldsTheLineLimitAtTheLargerBody()
    {
        // D-635: the box holds 76 characters at a body of 32, the limit of a dialogue line.
        Assert.Equal(76, (int)GameValue.Static(Layout, "DialogueCharacters", 32)!);
        Assert.True((int)GameValue.Static(Layout, "DialogueCharacters", 24)! >= 76);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    public void TheDialogueWindowsStayInsideTheFrameAndApart(int body)
    {
        // Review focus of PR-36 (D-568, D-1175): four choices fit the frame, and the window of the
        // portrait and the window of the choices stand apart above the box.
        object box = GameValue.Static(Layout, "DialogueBox", body)!;
        object portrait = GameValue.Static(Layout, "PortraitBox", body)!;
        object choices = GameValue.Static(Layout, "ChoiceBox", body, 4)!;
        foreach (object place in new[] { box, portrait, choices })
        {
            int x = Read(place, "X");
            int y = Read(place, "Y");
            Assert.True(x >= 0 && y >= 0 && x + Read(place, "Width") <= 1280 && y + Read(place, "Height") <= 720, $"The box {place} leaves the frame.");
            Assert.True(place == box || y + Read(place, "Height") <= Read(box, "Y"), $"The box {place} covers the dialogue box.");
        }

        Assert.True(Read(portrait, "X") + Read(portrait, "Width") < Read(choices, "X"), "The portrait and the choices overlap.");
    }

    [Fact]
    public void EachLineOptionAndNameOfTheStoryScenesFitsItsPlace()
    {
        // G-7, D-635: a line wraps into three lines of the box at a body of 32, an option fits one row
        // of the window of the choices, and a name fits the name plate over the portrait.
        int across = (int)GameValue.Static(Layout, "DialogueCharacters", 32)!;
        int option = (int)GameValue.Static(Layout, "ChoiceCharacters", 32)!;
        int name = ((int)GameValue.Constant(Layout, "PortraitWidth") - 32) / 16;
        foreach (TheThingBelow.Core.Story.StoryScene scene in Content.Value.Story.Scenes)
        {
            foreach (TheThingBelow.Core.Story.SceneStep step in scene.Steps)
            {
                if (step is TheThingBelow.Core.Story.SayStep say)
                {
                    string text = Text(say.Line);
                    Assert.True(WrappedLines(text, across) <= 3, $"The line '{say.Line.Value}' takes more than three lines of {across} characters.");
                    if (say.Speaker is TheThingBelow.Core.Story.SceneActor speaker)
                    {
                        ContentId plate = (ContentId)GameValue.Static("ScenePlay", "NameIdOf", speaker)!;
                        Assert.True(Text(plate).Length <= name, $"The name '{plate.Value}' fills more than {name} characters.");
                    }
                }

                if (step is TheThingBelow.Core.Story.ChooseStep choose)
                {
                    foreach (TheThingBelow.Core.Story.ChooseOption each in choose.Options)
                    {
                        Assert.True(Text(each.Line).Length <= option, $"The option '{each.Line.Value}' fills more than {option} characters.");
                    }
                }
            }
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

    /// <summary>Gives the count of lines of a text that wraps at whole words into lines of one width.</summary>
    private static int WrappedLines(string text, int width)
    {
        int lines = 1;
        int used = 0;
        foreach (string word in text.Split(' '))
        {
            int needed = used == 0 ? word.Length : used + 1 + word.Length;
            if (needed > width)
            {
                lines += 1;
                used = word.Length;
            }
            else
            {
                used = needed;
            }
        }

        return lines;
    }

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
