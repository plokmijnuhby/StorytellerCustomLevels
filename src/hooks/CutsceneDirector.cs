using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(CutsceneDirector), nameof(CutsceneDirector.StartOpenBook))]
internal class CutsceneDirector_StartOpenBook
{
    static void Prefix()
    {
        ChapterUtils.chapterAlreadyFixed = true;
    }
}
