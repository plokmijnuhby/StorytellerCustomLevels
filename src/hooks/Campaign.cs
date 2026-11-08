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

        int currentId = 1;

        Directory.CreateDirectory("./custom_levels");
        var files = Directory.EnumerateFiles("./custom_levels", "*.txt").ToArray();
        files.Sort(Compare);

        Campaign.BeginChapter("test_chapter", "Test Chapter", "illustration_marco", "CHAPTER_LOCKED", "Test");
        foreach (string file in files)
        {
            LevelUtils.filePaths.Add(file);
            LevelUtils.goalInfos.Add([]);
            LevelUtils.musicSources.Add(default);
            LevelUtils.LoadLevel((LevelID)currentId, file);
            string name = SplitWhitespace(file, true).Last();
            Campaign.AddLevel(name.Replace('_', ' '), (LevelID)currentId++);
        }
        Campaign.EndChapter();

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
