using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(LevelPage), nameof(LevelPage.LoadLevel))]
internal class LevelPage_LoadLevel
{
    static void Prefix(LevelID id)
    {
        Utils.LoadLevel(id);
    }
}