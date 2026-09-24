using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tools.Screenplay;

/// <summary>
/// The story scenes that one PR changes, against the content of its base commit (D-1015).
/// </summary>
/// <param name="Changed">Each changed story scene, in the order of the story scenes that the caller gives.</param>
/// <param name="Removed">Each story scene file of the base that the head lacks, in ordinal order.</param>
public sealed record ScreenplayBatch(IReadOnlyList<StoryScene> Changed, IReadOnlyList<string> Removed)
{
    /// <summary>Finds the batch.</summary>
    /// <param name="head">The files of the head, as `ContentFolder` reads them.</param>
    /// <param name="scenes">Every story scene of the head, which the load of the head checked, in the order of the screenplay.</param>
    /// <param name="headStrings">The string table of the head.</param>
    /// <param name="baseFiles">The files of the base folder.</param>
    /// <returns>The batch.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The base folder holds no string table (T-2).</exception>
    /// <remarks>
    /// A story scene changes when the bytes of its file change, when the base lacks its file,
    /// or when the text of a say line or a choose option changes. A change of a speaker name
    /// alone changes no story scene (D-1015).
    /// </remarks>
    public static ScreenplayBatch Find(
        IReadOnlyList<ContentFile> head,
        IEnumerable<StoryScene> scenes,
        StringTable headStrings,
        IReadOnlyList<ContentFile> baseFiles)
    {
        ArgumentNullException.ThrowIfNull(head);
        ArgumentNullException.ThrowIfNull(scenes);
        ArgumentNullException.ThrowIfNull(headStrings);
        ArgumentNullException.ThrowIfNull(baseFiles);

        SortedDictionary<string, byte[]> headScenes = SceneFiles(head);
        SortedDictionary<string, byte[]> baseScenes = SceneFiles(baseFiles);
        StringTable baseStrings = BaseStrings(baseFiles);

        List<StoryScene> changed = [];
        foreach (StoryScene scene in scenes)
        {
            if (!headScenes.TryGetValue(scene.File, out byte[]? after))
            {
                throw ContentException.ForField(scene.File, "id", $"the story scene '{scene.Id.Value}' has no file among the files of the head (T-2)");
            }

            bool fileChanged = !baseScenes.TryGetValue(scene.File, out byte[]? before)
                || !before.AsSpan().SequenceEqual(after);
            if (fileChanged || LineChanged(scene, headStrings, baseStrings))
            {
                changed.Add(scene);
            }
        }

        List<string> removed = [];
        foreach (string path in baseScenes.Keys)
        {
            if (!headScenes.ContainsKey(path))
            {
                removed.Add(path);
            }
        }

        return new ScreenplayBatch(changed, removed);
    }

    private static SortedDictionary<string, byte[]> SceneFiles(IReadOnlyList<ContentFile> files)
    {
        var scenes = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (ContentFile file in files)
        {
            if (StoryScene.IsSceneFile(file.Path))
            {
                scenes.Add(file.Path, file.Bytes);
            }
        }

        return scenes;
    }

    private static StringTable BaseStrings(IReadOnlyList<ContentFile> files)
    {
        foreach (ContentFile file in files)
        {
            if (string.CompareOrdinal(file.Path, StringTable.Path) == 0)
            {
                return StringTable.Read(file.Bytes, $"base:{StringTable.Path}");
            }
        }

        throw ContentException.ForField(
            $"base:{StringTable.Path}",
            "strings",
            "the base folder holds no string table, so the batch cannot read which line changed (D-1015)");
    }

    private static bool LineChanged(StoryScene scene, StringTable headStrings, StringTable baseStrings)
    {
        foreach (SceneStep step in scene.Steps)
        {
            if (step is SayStep say && TextChanged(say.Line, headStrings, baseStrings))
            {
                return true;
            }

            if (step is ChooseStep choose)
            {
                foreach (ChooseOption option in choose.Options)
                {
                    if (TextChanged(option.Line, headStrings, baseStrings))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool TextChanged(ContentId line, StringTable headStrings, StringTable baseStrings) =>
        !baseStrings.Contains(line) || string.CompareOrdinal(baseStrings.Text(line), headStrings.Text(line)) != 0;
}
