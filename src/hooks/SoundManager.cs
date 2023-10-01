using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.HasClipsFor))]
internal class SoundManager_HasClipsFor
{
    static void Prefix(ref string id)
    {
        id = Utils.FixMusic(id);
    }
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayMusicById))]
internal class SoundManager_PlayMusicById
{
    static void Prefix(ref string id)
    {
        id = Utils.FixMusic(id);
    }
}