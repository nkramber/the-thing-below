using System;
using System.Collections.Generic;

namespace TheThingBelow.Storage;

/// <summary>The three kinds of input that an action can take (D-84).</summary>
public enum BindingKind
{
    /// <summary>A key of the keyboard, by its physical place.</summary>
    Key,

    /// <summary>A button of a gamepad, by its place and not its label (F-50).</summary>
    Button,

    /// <summary>One direction of one axis of a stick.</summary>
    Stick,
}

/// <summary>One key, one button, or one direction of a stick, as the settings file holds it.</summary>
/// <remarks>
/// Storage takes no reference to Godot, so a binding holds the number of the Godot value:
/// the `Key` of a physical key, the `JoyButton` of a button, or the `JoyAxis` of a stick.
/// Godot keeps each number across versions, and Game turns the number back into its value.
/// </remarks>
/// <param name="Kind">The kind of the input.</param>
/// <param name="Code">The number of the key, the button, or the axis.</param>
/// <param name="Direction">-1 or 1 for a stick, and 0 for a key or a button.</param>
public sealed record InputBinding(BindingKind Kind, int Code, int Direction)
{
    /// <summary>Makes the binding of one physical key.</summary>
    /// <param name="code">The number of the Godot `Key`.</param>
    /// <returns>The binding.</returns>
    public static InputBinding OfKey(int code) => new(BindingKind.Key, code, 0);

    /// <summary>Makes the binding of one gamepad button.</summary>
    /// <param name="code">The number of the Godot `JoyButton`.</param>
    /// <returns>The binding.</returns>
    public static InputBinding OfButton(int code) => new(BindingKind.Button, code, 0);

    /// <summary>Makes the binding of one direction of one stick axis.</summary>
    /// <param name="code">The number of the Godot `JoyAxis`.</param>
    /// <param name="direction">-1 for the low end of the axis, and 1 for the high end.</param>
    /// <returns>The binding.</returns>
    public static InputBinding OfStick(int code, int direction) => new(BindingKind.Stick, code, direction);

    /// <summary>Fails when the binding breaks a rule of its kind (T-2).</summary>
    /// <param name="action">The action that holds the binding, which the message names.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The kind has no name, the code is below zero, or the direction does not fit the kind.
    /// </exception>
    public void Check(string action)
    {
        if (!Enum.IsDefined(this.Kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Kind), this.Kind, $"A binding of '{action}' takes a key, a button, or a stick (T-2).");
        }

        if (this.Code < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Code), this.Code, $"A binding of '{action}' takes a code of 0 or more (T-2).");
        }

        bool stick = this.Kind == BindingKind.Stick;
        if (stick && this.Direction is not (-1 or 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Direction), this.Direction, $"A stick binding of '{action}' takes the direction -1 or 1 (T-2).");
        }

        if (!stick && this.Direction != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Direction), this.Direction, $"A key or a button binding of '{action}' takes the direction 0 (T-2).");
        }
    }
}

/// <summary>One binding that two or more actions hold (D-862).</summary>
/// <param name="Binding">The key, the button, or the stick direction.</param>
/// <param name="Actions">The names of the actions that hold it, in ordinal order.</param>
public sealed record BindingConflict(InputBinding Binding, IReadOnlyList<string> Actions);

/// <summary>
/// The bindings of each action of the game, which the remap changes (D-214, D-862).
/// </summary>
/// <remarks>
/// The `ui_*` actions of the menus take no remap, so this type never holds one of them, and
/// the menus always answer their default buttons (D-862, F-50). A binding can sit on two
/// actions while the player remaps, and <see cref="FindConflicts"/> names each one. The
/// settings screen and <see cref="SettingsStore.Write"/> both refuse a save with a conflict.
/// </remarks>
public sealed class ControlBindings
{
    /// <summary>The prefix of the built-in actions of Godot, which take no remap (D-862).</summary>
    public const string MenuPrefix = "ui_";

    private readonly SortedDictionary<string, IReadOnlyList<InputBinding>> actions;

    /// <summary>Makes the bindings of each action.</summary>
    /// <param name="actions">The bindings of each action, by the name of the action.</param>
    /// <exception cref="ArgumentNullException">The map, a list, or a binding is null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// The map is empty, an action has no name, a menu action is in it, or an action has no
    /// binding or the same binding twice (T-2).
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">A binding breaks a rule of its kind (T-2).</exception>
    public ControlBindings(IReadOnlyDictionary<string, IReadOnlyList<InputBinding>> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        if (actions.Count == 0)
        {
            throw new ArgumentException("The bindings hold no action (T-2).", nameof(actions));
        }

        this.actions = new SortedDictionary<string, IReadOnlyList<InputBinding>>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, IReadOnlyList<InputBinding>> entry in actions)
        {
            CheckAction(entry.Key, entry.Value);
            this.actions.Add(entry.Key, new List<InputBinding>(entry.Value));
        }
    }

    /// <summary>The names of the actions, in ordinal order.</summary>
    public IReadOnlyCollection<string> Names => this.actions.Keys;

    /// <summary>Gives the bindings of one action.</summary>
    /// <param name="action">The name of the action.</param>
    /// <returns>The bindings, in the order of the file.</returns>
    /// <exception cref="KeyNotFoundException">The bindings hold no such action (T-2).</exception>
    public IReadOnlyList<InputBinding> Of(string action)
    {
        if (!this.actions.TryGetValue(action, out IReadOnlyList<InputBinding>? bindings))
        {
            throw new KeyNotFoundException(
                $"The bindings hold no action '{action}'. They hold {string.Join(", ", this.actions.Keys)} (T-2).");
        }

        return bindings;
    }

    /// <summary>Gives new bindings with one binding of one action replaced (D-862).</summary>
    /// <param name="action">The name of the action.</param>
    /// <param name="index">The place of the binding in the list of the action.</param>
    /// <param name="binding">The new binding. It can sit on another action too.</param>
    /// <returns>The new bindings. This object does not change.</returns>
    /// <exception cref="KeyNotFoundException">The bindings hold no such action (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the list (T-2).</exception>
    /// <exception cref="ArgumentException">The action already holds the binding at another place (T-2).</exception>
    public ControlBindings Replace(string action, int index, InputBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        IReadOnlyList<InputBinding> old = this.Of(action);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, old.Count);

        List<InputBinding> changed = new(old);
        changed[index] = binding;

        SortedDictionary<string, IReadOnlyList<InputBinding>> next = new(this.actions, StringComparer.Ordinal);
        next[action] = changed;
        return new ControlBindings(next);
    }

    /// <summary>Says whether two sets of bindings hold the same actions with the same lists.</summary>
    /// <param name="obj">The other object.</param>
    /// <returns>True when each action holds the same bindings in the same order.</returns>
    /// <remarks>
    /// A record of <see cref="GameSettings"/> compares its groups by value, and this type is
    /// one of its values. Thus a read of a written file equals the settings of the write.
    /// </remarks>
    public override bool Equals(object? obj)
    {
        if (obj is not ControlBindings other || other.actions.Count != this.actions.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, IReadOnlyList<InputBinding>> entry in this.actions)
        {
            if (!other.actions.TryGetValue(entry.Key, out IReadOnlyList<InputBinding>? bindings) ||
                bindings.Count != entry.Value.Count)
            {
                return false;
            }

            for (int index = 0; index < bindings.Count; index += 1)
            {
                if (bindings[index] != entry.Value[index])
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Gives a hash that agrees with <see cref="Equals(object?)"/>.</summary>
    /// <returns>The hash of the names of the actions and the count of their bindings.</returns>
    public override int GetHashCode()
    {
        HashCode hash = default;
        foreach (KeyValuePair<string, IReadOnlyList<InputBinding>> entry in this.actions)
        {
            hash.Add(entry.Key, StringComparer.Ordinal);
            hash.Add(entry.Value.Count);
        }

        return hash.ToHashCode();
    }

    /// <summary>Names each binding that two or more actions hold (D-862).</summary>
    /// <returns>The conflicts, in the order of the first action that holds each binding.</returns>
    public IReadOnlyList<BindingConflict> FindConflicts()
    {
        List<BindingConflict> conflicts = [];
        List<InputBinding> seen = [];
        foreach (KeyValuePair<string, IReadOnlyList<InputBinding>> entry in this.actions)
        {
            foreach (InputBinding binding in entry.Value)
            {
                if (seen.Contains(binding))
                {
                    continue;
                }

                seen.Add(binding);
                List<string> holders = this.HoldersOf(binding);
                if (holders.Count > 1)
                {
                    conflicts.Add(new BindingConflict(binding, holders));
                }
            }
        }

        return conflicts;
    }

    private static void CheckAction(string action, IReadOnlyList<InputBinding> bindings)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentNullException.ThrowIfNull(bindings, action);

        if (action.StartsWith(MenuPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"The action '{action}' is a menu action, and a menu action takes no remap (D-862).", action);
        }

        if (bindings.Count == 0)
        {
            throw new ArgumentException($"The action '{action}' has no binding (T-2).", action);
        }

        for (int index = 0; index < bindings.Count; index += 1)
        {
            InputBinding binding = bindings[index] ?? throw new ArgumentNullException(
                action, $"Binding {index} of the action '{action}' is null (T-2).");
            binding.Check(action);

            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (bindings[earlier] == binding)
                {
                    throw new ArgumentException(
                        $"The action '{action}' holds the binding {binding} at places {earlier} and {index} (T-2).", action);
                }
            }
        }
    }

    private List<string> HoldersOf(InputBinding binding)
    {
        List<string> holders = [];
        foreach (KeyValuePair<string, IReadOnlyList<InputBinding>> entry in this.actions)
        {
            foreach (InputBinding held in entry.Value)
            {
                if (held == binding)
                {
                    holders.Add(entry.Key);
                    break;
                }
            }
        }

        return holders;
    }
}
