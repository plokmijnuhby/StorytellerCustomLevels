using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.Awake))]
internal class Storyteller_Awake
{
    // This function is only ever called once, when the game first loads.
    static void Postfix()
    {
        var game = Storyteller.game;
        // Prevent displaying certain tutorial text
        game.savegame.seenStamps = true;
        // Prevent a check which crashes if there are too few levels in the game
        game.savegame.indexUnlocked = true;
    }
}

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.GoToPage))]
class Storyteller_GoToPage
{
    static bool Prefix(Storyteller __instance, int pageIndex)
    {
        if (__instance.savegameCache.totalDevilLevels != 0)
        {
            return true;
        }
        var target = __instance.pages[pageIndex].type;
        if (target == PageType.Empty || target == PageType.Devil)
        {
            __instance.StartCredits();
            return false;
        }
        return true;
    }
}
