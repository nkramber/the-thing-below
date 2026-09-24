using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The rule that holds the torch out or puts it away (D-848, D-1064). The torch never burns
/// out, and a torch held out is always lit.
/// </summary>
/// <remarks>
/// The state of the torch sets the sight of the party and of each patrol on a dark map
/// (D-1063). On any other map, the state changes no rule, and Game draws the carried light
/// and the torch in the hand alone (D-847, D-1066).
/// </remarks>
public static class TorchRules
{
    /// <summary>The file that holds the id, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/TorchRules.cs";

    /// <summary>The permanent id of the torch, a key item of the item file (D-166, D-1065).</summary>
    public static readonly ContentId Torch = ContentId.Parse("item.torch", Source, nameof(Torch));

    /// <summary>Holds the torch out or puts it away, on the walk (D-1064, D-1071).</summary>
    /// <param name="state">The run.</param>
    /// <param name="held">True to hold the torch out, and false to put it away.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// A menu is open, a battle holds the run, the pack holds no torch, or the torch already has
    /// that state (T-2).
    /// </exception>
    /// <exception cref="ContentException">The item file holds no torch, or holds it in a kind other than `key` (D-1065).</exception>
    /// <remarks>
    /// Game sends the intent on the walk alone, so an intent at any other time points at a fault
    /// in the screen that made it (D-1071, T-2). The rule of a story scene refuses the intent
    /// before this rule reads it (D-1010).
    /// </remarks>
    public static void Set(RunState state, bool held, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        string change = held ? "holds the torch out" : "puts the torch away";
        if (state.MenuOpen || state.Battle is not null)
        {
            throw new SimulationException($"an intent that {change} while a menu is open or a battle holds the run, and the torch works on the walk alone (D-1071)", context);
        }

        if (state.BattleContent.Item(Torch) is not KeyItem)
        {
            throw ContentException.ForField(ItemList.Path, "items", $"the item '{Torch.Value}' is not a key item, and the torch rule reads the kind `key` (D-1065)");
        }

        if (state.Characters.CountOf(Torch) < 1)
        {
            throw new SimulationException($"an intent that {change}, and the pack holds no '{Torch.Value}' (D-1071)", context);
        }

        state.Characters.SetTorchHeld(held, context);
    }
}
