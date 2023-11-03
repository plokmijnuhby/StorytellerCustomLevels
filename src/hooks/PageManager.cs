using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(PageManager), nameof(PageManager.FlipToPage))]
internal class PageManager_FlipToPage
{
    static void Prefix(ref int targetPageIndex, bool skipAnimations)
    {
        if (!skipAnimations)
        {
            ChapterUtils.GoToPage(ref targetPageIndex);
        }
    }
}
