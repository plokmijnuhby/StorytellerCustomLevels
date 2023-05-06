using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayMusicForLevel))]
internal class SoundManager_PlayMusicForLevel
{
    static void Prefix(ref LevelID levelId)
    {
        if (Utils.musicSources.ContainsKey(levelId))
        {
            Utils.LoadLevel(levelId);
            levelId = Utils.musicSources[levelId];
        }
    }
}
