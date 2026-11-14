using HarmonyLib;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Campaign), nameof(Campaign.LoadCampaign))]
internal class Campaign_LoadCampaign
{
    public static string[] SplitWhitespace(string x, bool extension = false)
    {
        string name;
        if (extension)
        {
            name = Path.GetFileNameWithoutExtension(x);
        }
        else
        {
            name = Path.GetFileName(x);
        }
        return name.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
    }

    static int Compare(string x, string y)
    {
        if (x == null)
        {
            return y == null ? 0 : -1;
        }
        else if (y == null)
        {
            return 1;
        }
        string xPrefix = SplitWhitespace(x)[0];
        string yPrefix = SplitWhitespace(y)[0];

        // "C10" > "C2"
        int res = xPrefix.Length.CompareTo(yPrefix.Length);
        if (res == 0)
        {
            // "C2" > "C1"
            res = string.Compare(xPrefix, yPrefix, false, CultureInfo.InvariantCulture);
            if (res == 0)
            {
                // "C 2" > "C 1" 
                res = string.Compare(x, y, false, CultureInfo.InvariantCulture);
            }
        }
        return res;
    }
    static bool Prefix()
    {
        Campaign.levels = new();
        Campaign.levelSpecs.Clear();
        Campaign.chapters = new();
        Campaign.platformAchievements.Clear();
        Campaign.stamps.Clear();
        Campaign.goalDescriptions.Clear();

        // Dummy level, used as a placeholder by other hooks
        Campaign.Begin(LevelID.Invalid, LevelType.Playground, 3);
        Campaign.End();

        int levelId = 1;

        Directory.CreateDirectory("./custom_levels");
        var chapters = Directory.GetDirectories("./custom_levels");
        chapters.Sort(Compare);
        if (chapters.Length == 0)
        {
            chapters = ["./custom_levels"];
        }
        for (int chapterId = 0; chapterId < chapters.Length; chapterId++)
        {
            var chapter = chapters[chapterId];
            var files = Directory.GetFiles(chapter, "*.txt");
            files.Sort(Compare);

            var chapter_name = SplitWhitespace(chapter).Last();
            Campaign.BeginChapter(
                $"chapter_{chapterId}",
                chapter_name,
                "illustration_marco",
                "CHAPTER_LOCKED",
                chapter_name
            );
            foreach (string file in files)
            {
                LevelUtils.filePaths.Add(file);
                LevelUtils.goalInfos.Add([]);
                LevelUtils.musicSources.Add(default);
                LevelUtils.LoadLevel((LevelID)levelId, file);
                string name = SplitWhitespace(file, true).Last();
                Campaign.AddLevel(name.Replace('_', ' '), (LevelID)levelId++);
            }
            Campaign.EndChapter();
        }

        /*
        var newSolutions = Storyteller.game.savegame.solutions;
        var oldSolutions = newSolutions.ToArray();
        newSolutions.Clear();
        foreach (var solution in oldSolutions)
        {
            if (solution.levelId != id)
            {
                newSolutions.Add(solution);
            }
        }
        foreach (var ((file, goal), layout) in solutions)
        {
            if (file == filePaths[id])
            {
                newSolutions.Add(new GoalSolution
                {
                    goalId = goal,
                    levelId = id,
                    layout = layout
                });
            }
        }*/

        // TODO: Add stamps here?
        // TODO: Meta/stamp/story achievements?

        return false;
    }
}
