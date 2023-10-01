using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayMusicById))]
internal class SoundManager_PlayMusicForLevel
{
    static void Prefix(string id, bool forceRestart)
    {
        System.Console.WriteLine(id);
        /*if (Utils.musicSources.ContainsKey(levelId))
        {
            Utils.LoadLevel(levelId);
            levelId = Utils.musicSources[levelId];
        }*/
    }
}