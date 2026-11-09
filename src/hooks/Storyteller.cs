using HarmonyLib;
using System.Linq;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.Awake))]
internal class Storyteller_Awake
{
    // This function is only ever called once, when the game first loads.
    static void Postfix()
    {
        var game = Storyteller.game;
        var pages = game.pages;
        for (int i = 0; i < pages.Count;)
        {
            if (pages[i].type == PageType.Devil || pages[i].type == PageType.Empty)
            {
                pages.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        // Prevent displaying certain tutorial text
        game.savegame.seenStamps = true;
        // Prevent a check which crashes if there are too few levels in the game
        game.savegame.indexUnlocked = true;
    }
}

/* TODO: fix
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
}*/

/*
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
}*/