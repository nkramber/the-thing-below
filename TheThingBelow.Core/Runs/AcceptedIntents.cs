using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Shops;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// Lists the intents of the player that the state accepts on the next tick, in the forms that the
/// screens of Game make (D-493, D-1179). The bots of PR-15 pick from the list, and the runner
/// finds a softlock with it.
/// </summary>
/// <remarks>
/// The query reads the state and changes nothing, so the simulation version stays (G-17). It
/// asks the same refusal functions that the menus of Game ask, and a seed loop of Tests steps
/// each listed intent on a copy of the state with no refusal.
/// <para>
/// The list holds no debug intent, no cancel, and no map id, because Game makes none of them from
/// a key (D-493, D-1133). A wipe gives an empty list, because the host reloads the run then
/// (D-776, D-1181).
/// </para>
/// </remarks>
public static class AcceptedIntents
{
    /// <summary>Gives the intents that the state accepts, in a fixed order (T-7).</summary>
    /// <param name="state">The state of the run.</param>
    /// <returns>The intents. Each one steps with no refusal on the next tick.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public static IReadOnlyList<Intent> Of(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        List<Intent> accepted = [];
        StoryState story = state.Story;
        if (story.Paused)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.StoryResume));
            return accepted;
        }

        if (story.Running && story.Phase != ScenePhase.Battle)
        {
            AddStory(state, accepted);
            return accepted;
        }

        if (state.Battle is Battle battle)
        {
            AddBattle(state, battle, story.Running, accepted);
            return accepted;
        }

        // A wipe on the map takes no intent until Game reloads the newer save (D-397).
        if (state.MapWiped)
        {
            return accepted;
        }

        if (state.MenuOpen)
        {
            AddMenu(state, accepted);
            return accepted;
        }

        AddWalk(state, accepted);
        return accepted;
    }

    /// <summary>Adds the step end, each pick, and the pause of a story scene out of battle (D-1007, D-1009, D-1010, D-1335).</summary>
    private static void AddStory(RunState state, List<Intent> accepted)
    {
        StoryState story = state.Story;
        if (story.Phase == ScenePhase.WaitIntent)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.StoryStepEnd));
        }

        if (story.Phase == ScenePhase.Pick && story.Scene is StoryScene scene && scene.Steps[story.Step] is ChooseStep choose)
        {
            for (int option = 0; option < choose.Options.Count; option += 1)
            {
                accepted.Add(Intent.OfPick(option));
            }
        }

        // Both answers to an offer hold with any gold: a yes with too little gold gives the refusal line (D-1335).
        if (story.Phase == ScenePhase.Pick && story.Scene is StoryScene offer && offer.Steps[story.Step] is PayStep)
        {
            accepted.Add(Intent.OfPick(PayStep.PayOption));
            accepted.Add(Intent.OfPick(PayStep.DeclineOption));
        }

        accepted.Add(Intent.OfPlayer(IntentIds.StoryPause));
    }

    /// <summary>
    /// Adds the intents of a battle: the close of the pause of a fight, the commands of the
    /// character whose turn it is, or the wait of a won or fled battle (D-522, D-532, D-764).
    /// </summary>
    private static void AddBattle(RunState state, Battle battle, bool storyBattle, List<Intent> accepted)
    {
        if (state.MenuOpen)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.CloseMenu));
            return;
        }

        if (battle.Outcome == BattleOutcome.Wiped)
        {
            return;
        }

        if (battle.Outcome == BattleOutcome.Running)
        {
            AddCommands(state, battle, accepted);
        }
        else
        {
            accepted.Add(Intent.OfPlayer(IntentIds.WaitBattleEnd));
        }

        // A story scene refuses the menu, also in its battle (D-1009).
        if (!storyBattle)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.OpenMenu));
        }
    }

    /// <summary>Adds each command that the command menu of Game offers to the character whose turn it is (D-764, D-780, D-1027).</summary>
    private static void AddCommands(RunState state, Battle battle, List<Intent> accepted)
    {
        if (battle.Next() is not Combatant next || next.Side != BattleSide.Party)
        {
            return;
        }

        AddTargets(state, IntentIds.BattleAttack, BattleAction.Attack, battle.Enemies, null, accepted);
        foreach (PackValues entry in state.Characters.Pack)
        {
            if (entry.Count > 0)
            {
                AddTargets(state, IntentIds.BattleItem, BattleAction.Item, battle.Party, entry.Id, accepted);
            }
        }

        PartyMember member = state.Characters.Members[next.Slot];
        foreach (ContentId? carried in member.Slots)
        {
            if (carried is ContentId lesson)
            {
                AddBattleLesson(state, battle, member, lesson, accepted);
            }
        }

        AddAlone(state, IntentIds.BattleDefend, BattleAction.Defend, accepted);
        AddAlone(state, IntentIds.BattleStep, BattleAction.Step, accepted);
        AddAlone(state, IntentIds.BattleFlee, BattleAction.Flee, accepted);
    }

    /// <summary>Adds the attack or the item use at each combatant of one side that the rules take (D-377, D-382).</summary>
    private static void AddTargets(RunState state, ContentId action, BattleAction kind, IReadOnlyList<Combatant> side, ContentId? item, List<Intent> accepted)
    {
        foreach (Combatant combatant in side)
        {
            if (BattleTurns.RefusalOf(state, new BattleChoice(kind, combatant.Target, item)) is null)
            {
                accepted.Add(Intent.OfPlayer(action, combatant.Target, item));
            }
        }
    }

    /// <summary>Adds each opened form of a lesson at each combatant that the rules take, enemies first (D-1027).</summary>
    private static void AddBattleLesson(RunState state, Battle battle, PartyMember member, ContentId lesson, List<Intent> accepted)
    {
        LessonRecord record = state.BattleContent.Lessons.Lesson(lesson);
        int opened = record.OpenedAt(member.PointsOf(lesson));
        for (int form = 0; form < opened; form += 1)
        {
            foreach (IReadOnlyList<Combatant> side in new[] { battle.Enemies, battle.Party })
            {
                foreach (Combatant combatant in side)
                {
                    if (BattleTurns.RefusalOf(state, new BattleChoice(BattleAction.Lesson, combatant.Target, null, lesson, form)) is null)
                    {
                        accepted.Add(Intent.OfBattleLesson(lesson, form, combatant.Target));
                    }
                }
            }
        }
    }

    /// <summary>Adds a command with no target when the rules take it (D-378, D-380, D-755).</summary>
    private static void AddAlone(RunState state, ContentId action, BattleAction kind, List<Intent> accepted)
    {
        if (BattleTurns.RefusalOf(state, new BattleChoice(kind, null, null)) is null)
        {
            accepted.Add(Intent.OfPlayer(action));
        }
    }

    /// <summary>
    /// Adds the intents of an open menu outside a fight: the close, each window of the main list,
    /// and the window of the service that the lead faces (D-162, D-558, D-1131).
    /// </summary>
    private static void AddMenu(RunState state, List<Intent> accepted)
    {
        accepted.Add(Intent.OfPlayer(IntentIds.CloseMenu));
        AddParty(state, accepted);
        AddMenuItems(state, accepted);
        AddGear(state, accepted);
        AddLessons(state, accepted);
        if (ServiceRules.FacedService(state) is MapService service && service.Condition.Holds(state.Story.Flags))
        {
            AddService(state, service, accepted);
        }

        if (SavePointRules.FacedSavePoint(state) is not null)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.Save));
        }
    }

    /// <summary>Adds the row change of each character and each swap with the reserve that the rules take (D-558, D-1134).</summary>
    private static void AddParty(RunState state, List<Intent> accepted)
    {
        PartyState party = state.Characters;
        for (int slot = 0; slot < party.Members.Count; slot += 1)
        {
            accepted.Add(Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, slot), null));
        }

        for (int slot = 0; slot < party.Members.Count; slot += 1)
        {
            for (int reserve = 0; reserve < party.Reserve.Count; reserve += 1)
            {
                if (party.RefusalOfReserveSwap(slot, reserve) is null)
                {
                    accepted.Add(Intent.OfPartySwap(slot, reserve));
                }
            }
        }
    }

    /// <summary>Adds each item use of the item window at each character that the rules take (D-1046).</summary>
    private static void AddMenuItems(RunState state, List<Intent> accepted)
    {
        foreach (PackValues entry in state.Characters.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Kind, ItemList.Kind) != 0)
            {
                continue;
            }

            for (int target = 0; target < state.Characters.Members.Count; target += 1)
            {
                if (ItemRules.RefusalOfMenuUse(state, entry.Id, target) is null)
                {
                    accepted.Add(Intent.OfMenuItem(entry.Id, target));
                }
            }
        }
    }

    /// <summary>Adds each change of gear of the gear window: the empty entry, then each piece of the pack of the kind of the slot (D-44, D-1048).</summary>
    private static void AddGear(RunState state, List<Intent> accepted)
    {
        PartyState party = state.Characters;
        for (int character = 0; character < party.Members.Count; character += 1)
        {
            for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
            {
                foreach (ContentId? piece in GearEntriesOf(state, slot))
                {
                    if (party.RefusalOfWear(character, slot, piece, state.BattleContent) is null)
                    {
                        accepted.Add(Intent.OfGearWear(character, slot, piece));
                    }
                }
            }
        }
    }

    /// <summary>Gives the empty entry, then each piece of the pack that fits the slot, in the order of the pack.</summary>
    private static List<ContentId?> GearEntriesOf(RunState state, int slot)
    {
        List<ContentId?> entries = new() { null };
        GearSlotKind kind = GearRules.KindOf(slot);
        foreach (PackValues entry in state.Characters.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Kind, GearList.Kind) == 0 && state.BattleContent.Piece(entry.Id).Slot == kind)
            {
                entries.Add(entry.Id);
            }
        }

        return entries;
    }

    /// <summary>
    /// Adds each swap of the lesson window, the empty entry and each lesson of the lesson pack, and
    /// each cast of an opened form at each character that the rules take (D-391, D-1030, D-1050).
    /// </summary>
    private static void AddLessons(RunState state, List<Intent> accepted)
    {
        PartyState party = state.Characters;
        for (int character = 0; character < party.Members.Count; character += 1)
        {
            PartyMember member = party.Members[character];
            for (int slot = 0; slot < member.Slots.Count; slot += 1)
            {
                AddSwaps(state, character, slot, accepted);
                if (member.Slots[slot] is ContentId held)
                {
                    AddCasts(state, character, member, held, accepted);
                }
            }
        }
    }

    /// <summary>Adds the swap of one lesson slot to the empty entry and to each lesson of the lesson pack that the rules take (D-1030).</summary>
    private static void AddSwaps(RunState state, int character, int slot, List<Intent> accepted)
    {
        PartyState party = state.Characters;
        if (party.RefusalOfSwap(character, slot, null) is null)
        {
            accepted.Add(Intent.OfLessonSwap(character, slot, null));
        }

        foreach (ContentId lesson in party.LessonPack)
        {
            if (party.RefusalOfSwap(character, slot, lesson) is null)
            {
                accepted.Add(Intent.OfLessonSwap(character, slot, lesson));
            }
        }
    }

    /// <summary>Adds each cast from the menu of an opened form of a held lesson at each character that the rules take (D-391).</summary>
    private static void AddCasts(RunState state, int caster, PartyMember member, ContentId held, List<Intent> accepted)
    {
        LessonRecord record = state.BattleContent.Lessons.Lesson(held);
        int opened = record.OpenedAt(member.PointsOf(held));
        for (int form = 0; form < opened; form += 1)
        {
            for (int target = 0; target < state.Characters.Members.Count; target += 1)
            {
                if (LessonRules.RefusalOfMenuCast(state, caster, held, form, target) is null)
                {
                    accepted.Add(Intent.OfMenuCast(caster, held, form, target));
                }
            }
        }
    }

    /// <summary>Adds the rest, or each buy and sale of the shop of the faced service (D-1141, D-1149, D-1156, D-1158).</summary>
    private static void AddService(RunState state, MapService service, List<Intent> accepted)
    {
        switch (service.Kind)
        {
            case ServiceKind.Rest:
                if (service.Price is int price && price <= state.Characters.Gold)
                {
                    accepted.Add(Intent.OfPlayer(IntentIds.HubRest));
                }

                return;
            case ServiceKind.Shop:
                AddShop(state, service, accepted);
                return;
            default:
                throw new SimulationException($"the service '{service.Id.Value}' has the kind '{ServiceKinds.NameOf(service.Kind)}', which the query of the accepted intents does not know (D-1179)", state.Context("accepted"));
        }
    }

    /// <summary>Adds each count of each buy that the shop shows, and each count of each sale that it takes (D-1152, D-1158).</summary>
    private static void AddShop(RunState state, MapService service, List<Intent> accepted)
    {
        ContentId id = service.Shop
            ?? throw new SimulationException($"the shop service '{service.Id.Value}' names no shop, and the reader refuses such a service (D-1149)", state.Context("accepted"));
        ShopRecord shop = state.BattleContent.Shops.Shop(id);
        foreach (StockEntry entry in ShopRules.Shown(state, shop))
        {
            int most = ShopRules.LimitOf(state, shop, entry).Most;
            for (int count = 1; count <= most; count += 1)
            {
                accepted.Add(Intent.OfShopBuy(entry.Thing, count));
            }
        }

        foreach (ContentId thing in ShopRules.Sellable(state))
        {
            if (ShopRules.SaleOf(state, shop, thing) == 0)
            {
                continue;
            }

            int held = state.Characters.CountOf(thing);
            for (int count = 1; count <= held; count += 1)
            {
                accepted.Add(Intent.OfShopSell(thing, count));
            }
        }
    }

    /// <summary>Adds the four steps, the confirm, the menu, and the torch of the walk (D-493, D-1064, D-1131).</summary>
    private static void AddWalk(RunState state, List<Intent> accepted)
    {
        accepted.Add(Intent.OfPlayer(IntentIds.MoveNorth));
        accepted.Add(Intent.OfPlayer(IntentIds.MoveSouth));
        accepted.Add(Intent.OfPlayer(IntentIds.MoveEast));
        accepted.Add(Intent.OfPlayer(IntentIds.MoveWest));
        accepted.Add(Intent.OfPlayer(IntentIds.Confirm));
        accepted.Add(Intent.OfPlayer(IntentIds.OpenMenu));
        if (state.Characters.CountOf(TorchRules.Torch) > 0)
        {
            accepted.Add(Intent.OfPlayer(state.Characters.TorchHeld ? IntentIds.PutTorchAway : IntentIds.HoldTorch));
        }
    }
}
