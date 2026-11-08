using HarmonyLib;

namespace CustomLevels.hooks;
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.HasClipsFor))]
internal class SoundManager_HasClipsFor
{
    static bool Prefix(ref string id, ref bool __result)
    {
        id = LevelUtils.FixMusic(id);
        __result = id != null;
        return __result;
    }
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PickClip))]
internal class SoundManager_PickClip
{
    static bool Prefix(ref string id)
    {
        id = LevelUtils.FixMusic(id);
        return id != null;
    }
}