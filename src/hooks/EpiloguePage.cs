using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(EpiloguePage), nameof(EpiloguePage.OnBecomingActive))]
internal class EpiloguePage_OnBecomingActive
{
    static void Postfix(EpiloguePage __instance)
    {
        __instance.devilScratch.enabled = Storyteller.game.savegameCache.totalDevilLevels != 0;
    }
}