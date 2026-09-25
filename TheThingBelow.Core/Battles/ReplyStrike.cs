namespace TheThingBelow.Core.Battles;

/// <summary>
/// One strike that a character can use on its next turn: the basic attack, or a form of an
/// equipped lesson that the rules allow now. The reply of the evaluator scores each one against
/// the enemy side (D-534, D-960, D-1101).
/// </summary>
/// <param name="Stat">The stat of the character that the hit reads (D-1052).</param>
/// <param name="Power">The power in basis points, with the aptitude bonus of the lesson (D-1028).</param>
/// <param name="Element">The element of the strike, or no value (D-796).</param>
/// <param name="Reach">The reach: melee, or any combatant of the other side on the field (D-377, D-955).</param>
public sealed record ReplyStrike(StrikeStat Stat, int Power, Element? Element, AbilityReach Reach);
