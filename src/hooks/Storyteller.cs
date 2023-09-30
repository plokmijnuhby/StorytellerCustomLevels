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

        // Add an index for the chapter (ie a list of levels)
        var indexPage = new PageSpec()
        {
            id = "chapter_custom_levels",
            type = PageType.Index,
            chapterId = "custom_levels"
        };
        var pages = __instance.pages;
        pages.Insert(pages.Count - 1, indexPage);

        Utils.LoadChapter(chapter);
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.UpdateSavegameCache))]
internal class Storyteller_UpdateSavegameCache
{
    // This must be patched to make the crown work correctly.
    // My approach is to completely override this function and implement it myself.
    // "Jester goal" is another term for a subgoal.
    static bool Prefix(Storyteller __instance)
    {
        int mainGoalCount = 0;
        int solvedMainGoals = 0;
        int jesterGoalCount = 0;
        int solvedJesterGoals = 0;
        int firstChapterWithUnsolvedMainGoals = -1;

        // We ignore the chapter that we added
        Chapter[] chapters = Campaign.chapters.ToArray();
        foreach (var chapter in chapters[..^1])
        {
            foreach (var level in chapter.levels)
            {
                foreach (var goal in Campaign.levelSpecs[level.id].goals)
                {
                    if (goal.isSubgoal)
                    {
                        jesterGoalCount++;
                    }
                    else
                    {
                        mainGoalCount++;
                    }


                    if (__instance.savegame.IsGoalSolved(level.id, goal.id))
                    {
                        if (goal.isSubgoal)
                        {
                            solvedJesterGoals++;
                        }
                        else
                        {
                            solvedMainGoals++;
                        }
                    }
                    else if (firstChapterWithUnsolvedMainGoals == -1 && !goal.isSubgoal)
                    {
                        firstChapterWithUnsolvedMainGoals = Array.IndexOf(chapters, chapter);
                    }
                }
            }
        }
        var savegameCache = __instance.savegameCache;
        savegameCache.mainGoalCount = mainGoalCount;
        savegameCache.solvedMainGoals = solvedMainGoals;
        savegameCache.jesterGoalCount = jesterGoalCount;
        savegameCache.solvedJesterGoals = solvedJesterGoals;
        savegameCache.firstChapterWithUnsolvedMainGoals = firstChapterWithUnsolvedMainGoals;
        return false;
    }
}