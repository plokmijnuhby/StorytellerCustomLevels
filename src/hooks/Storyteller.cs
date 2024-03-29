﻿using HarmonyLib;
using System.Linq;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.LoadBookPages))]
internal class Storyteller_LoadBookPages
{
    // This function is only ever called once, when the game first loads.
    static void Postfix()
    {
        ChapterUtils.InitGame();
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.GoToPage))]
internal class Storyteller_GoToPage
{
    static void Prefix(ref int pageIndex, bool skipAnims)
    {
        if (!skipAnims)
        {
            ChapterUtils.GoToPage(ref pageIndex);
        }
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.ComputeSolvedRatioForSlot))]
internal class Storyteller_ComputeSolvedRatioForSlot
{
    // This method is used to determine completion percentage. 
    // We delete the levels while the method is running, and immediately add them back afterwards.
    // We also need to reload the current save slot when we're done,
    // since this fixes certain issues with the way saving works.

    static Il2CppSystem.Collections.Generic.List<ChapterLevelEntry> levels;

    static void Prefix()
    {
        levels = ChapterUtils.customChapter.levels;
        ChapterUtils.customChapter.levels = new();
    }

    static void Finalizer(Storyteller __instance)
    {
        ChapterUtils.customChapter.levels = levels;
        levels = null;
        __instance.LoadSlot(__instance.settings.currentSaveSlot, __instance.savegame);
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
        if (savegameCache.firstIndexPageWithUnsolvedLevels == ChapterUtils.insertionPoint)
        {
            savegameCache.firstIndexPageWithUnsolvedLevels = -1;
        }
        foreach (var levelEntry in ChapterUtils.customChapter.levels)
        {
            savegameCache.totalCrownLevels -= 1;

            var levelId = levelEntry.id;
            bool levelSolved = true;
            foreach (var goalSpec in Campaign.levelSpecs[levelId].goals)
            {
                levelSolved &= __instance.savegame.IsGoalSolved(levelId, goalSpec.id);
            }
            if (levelSolved)
            {
                savegameCache.solvedCrownLevels -= 1;
            }
        }
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.GrantStamp))]
internal class Storyteller_GrantStamp
{
    // It should not be possible to obtain stamps in a custom level.
    static bool Prefix(LevelID levelId)
    {
        return !ChapterUtils.allowedIDs.Contains(levelId);
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.VerifyClaimedSolutionsToLevel))]
internal class Storyteller_VerifyClaimedSolutionsToLevel
{
    static void Postfix(Storyteller __instance, LevelID id)
    {
        GoalSolution[] solutions = __instance.savegame.solutions.ToArray();
        foreach (GoalSpec goal in Campaign.levelSpecs[id].goals)
        {
            if (!solutions.Any(solution => solution.goalId == goal.id))
            {
                LevelUtils.solutions.Remove((LevelUtils.filePaths[id], goal.id));
            }
        }
    }
}