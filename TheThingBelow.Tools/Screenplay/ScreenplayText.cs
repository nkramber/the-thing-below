using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tools.Screenplay;

/// <summary>
/// Writes one story scene as a screenplay in Markdown (D-173, D-1017). Each step starts with
/// its number, a say step gives the speaker and the text, and each other step gives one
/// action line.
/// </summary>
/// <remarks>
/// The lead prints as THE LEAD, because the lead depends on the party order of the run. A cast
/// member prints the text of its `name.` entry (D-1017).
/// </remarks>
public static class ScreenplayText
{
    /// <summary>The speaker name of the lead in a say step (D-1017).</summary>
    public const string LeadSpeaker = "THE LEAD";

    /// <summary>The name of the lead at the start of an action line (D-1017).</summary>
    public const string LeadActor = "The lead";

    /// <summary>The kind of the string id that holds the name of a cast member.</summary>
    public const string NameKind = "name";

    /// <summary>Writes one story scene.</summary>
    /// <param name="scene">The story scene.</param>
    /// <param name="strings">The string table, which holds each line and each name.</param>
    /// <returns>The Markdown text, which ends with a line end.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// A step names a string id or a cast name that the table lacks. The error names the story
    /// scene file, the step, and the id (G-7, T-2).
    /// </exception>
    public static string Write(StoryScene scene, StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(strings);

        var text = new StringBuilder();
        text.Append($"### {scene.Id.Value} (`{scene.File}`)\n");
        for (int index = 0; index < scene.Steps.Count; index += 1)
        {
            text.Append('\n');
            text.Append($"`[{index}]` ");
            text.Append(StepText(scene, index, strings));
        }

        return text.ToString();
    }

    private static string StepText(StoryScene scene, int index, StringTable strings)
    {
        var names = new StepNames(scene, index, strings);
        return scene.Steps[index] switch
        {
            MoveStep move => Action($"{names.Actor(move.Actor)} walks {PathText(move.Path)}."),
            FaceStep face => Action($"{names.Actor(face.Actor)} faces {StepDirections.NameOf(face.Facing)}."),
            WaitStep wait => Action($"Pause: {wait.Ticks} ticks."),
            SayStep say => SayText(say, names),
            ChooseStep choose => ChooseText(choose, names),
            SetFlagStep flag => Action($"Sets {flag.Flag.Value}."),
            JoinStep join => Action($"{names.Cast(join.Character, "character")} joins the party."),
            ShowStep show => Action($"{names.Cast(show.Actor, "actor")} appears at {show.Marker.Value}, facing {StepDirections.NameOf(show.Facing)}."),
            HideStep hide => Action($"{names.Cast(hide.Actor, "actor")} leaves."),
            CameraStep camera => Action($"The view moves to {camera.Marker.Value}."),
            StartBattleStep battle => Action($"Battle: {battle.Group.Value}."),
            _ => throw ContentException.ForField(scene.File, scene.StepField(index), $"the step kind '{SceneStepKinds.NameOf(scene.Steps[index].Kind)}' has no screenplay text (T-2)"),
        };
    }

    private static string Action(string text) => $"*{text}*\n";

    private static string SayText(SayStep say, StepNames names)
    {
        string line = names.Line(say.Line, "line");
        if (say.Speaker is null)
        {
            return "*(no speaker)*\n" + Quote(line, italic: true);
        }

        return $"**{names.Speaker(say.Speaker)}**\n" + Quote(line, italic: false);
    }

    private static string ChooseText(ChooseStep choose, StepNames names)
    {
        var text = new StringBuilder("*Choice:*\n");
        for (int option = 0; option < choose.Options.Count; option += 1)
        {
            ChooseOption each = choose.Options[option];
            string line = names.Line(each.Line, $"options[{option}].line");
            text.Append($"> {option + 1}. {line} *(sets {each.Flag.Value})*\n");
        }

        return text.ToString();
    }

    /// <summary>Gives each line of a text as a line of a quote, so a text of two lines stays one quote.</summary>
    private static string Quote(string line, bool italic)
    {
        var text = new StringBuilder();
        foreach (string part in line.Split('\n'))
        {
            text.Append(italic ? $"> *{part}*\n" : $"> {part}\n");
        }

        return text.ToString();
    }

    /// <summary>Gives a path as runs of one direction, such as `north 2, east 1` (D-1012).</summary>
    private static string PathText(IReadOnlyList<StepDirection> path)
    {
        List<string> runs = [];
        int start = 0;
        while (start < path.Count)
        {
            int end = start;
            while (end + 1 < path.Count && path[end + 1] == path[start])
            {
                end += 1;
            }

            runs.Add($"{StepDirections.NameOf(path[start])} {end - start + 1}");
            start = end + 1;
        }

        return string.Join(", ", runs);
    }

    /// <summary>Finds the text of each id that one step names, with the step in each error (T-2).</summary>
    private sealed class StepNames(StoryScene scene, int index, StringTable strings)
    {
        public string Line(ContentId id, string field) => this.Text(id, field);

        public string Speaker(SceneActor speaker) =>
            speaker.Id is null ? LeadSpeaker : this.Cast(speaker.Id, "speaker").ToUpperInvariant();

        public string Actor(SceneActor actor) =>
            actor.Id is null ? LeadActor : this.Cast(actor.Id, "actor");

        public string Cast(ContentId character, string field)
        {
            ContentId name = ContentId.Parse($"{NameKind}.{character.Name}", scene.File, $"{scene.StepField(index)}.{field}");
            return this.Text(name, field);
        }

        private string Text(ContentId id, string field)
        {
            if (!strings.Contains(id))
            {
                throw ContentException.ForField(scene.File, $"{scene.StepField(index)}.{field}", $"the string table holds no id '{id.Value}' (G-7)");
            }

            return strings.Text(id);
        }
    }
}
