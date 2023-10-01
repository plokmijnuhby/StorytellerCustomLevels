using HarmonyLib;
using System;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.LoadBookPages))]
internal class Storyteller_LoadBookPages
{
    // This function is only ever called once, when the game first loads.
    static void Postfix(Storyteller __instance)
    {
        // illustration_marco is a blank illustration, seen on the left of the list of levels.
        Campaign.BeginChapter("custom_levels", "Custom levels", "illustration_marco");
        var chapter = Campaign.curChapter;
        Campaign.EndChapter();

        // Add an index for the customChapter (ie a list of levels)
        var indexPage = new PageSpec()
        {
            id = "chapter_custom_levels",
            type = PageType.Index,
            chapterId = "custom_levels"
        };
        var pages = __instance.pages;
        pages.Insert(pages.Count - 4, indexPage);

        Utils.customChapter = chapter;
        Utils.LoadChapter();
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.UpdateSavegameCache))]
internal class Storyteller_UpdateSavegameCache
{
    static void Postfix(Storyteller __instance)
    {
        // The savegame cache statistics control whether the crown is unlocked.
        // We want our levels to be ignored when checking this,
        // so we undo the effect they had on the basegame levels.
        var savegameCache = __instance.savegameCache;
        if (savegameCache.firstIndexPageWithUnsolvedLevels == Utils.normalPages - 5)
        {
            savegameCache.firstIndexPageWithUnsolvedLevels = -1;
        }
        foreach (var levelEntry in Utils.customChapter.levels)
        {
            var levelId = levelEntry.id;
            bool levelSolved = true;
            foreach (var goalSpec in Campaign.levelSpecs[levelId].goals)
            {
                bool goalSolved = __instance.savegame.IsGoalSolved(levelId, goalSpec.id);
                if (goalSpec.type == GoalType.Main)
                {
                    savegameCache.totalMainGoals -= 1;
                    if (goalSolved)
                    {
                        savegameCache.solvedMainGoals -= 1;
                    }
                }
                else
                {
                    savegameCache.totalSubgoals -= 1;
                    if (goalSolved)
                    {
                        savegameCache.solvedSubgoals -= 1;
                    }
                }
                if (!goalSolved)
                {
                    levelSolved = false;
                }
            }
            if (levelSolved)
            {
                savegameCache.solvedLevels -= 1;
            }
        }
    }
}
