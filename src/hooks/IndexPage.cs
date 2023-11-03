using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(IndexPage), nameof(IndexPage.Load))]
internal class IndexPage_Load
{
    static void Prefix(string chapterId)
    {
        if (chapterId == "custom_levels")
        {
            ChapterUtils.LoadIndexPage();
        }
        else
        {
            ChapterUtils.chapterAlreadyFixed = false;
        }
    }
}
