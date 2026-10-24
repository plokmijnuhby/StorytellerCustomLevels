using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(IndexChapterPanel), nameof(IndexChapterPanel.Load))]
internal class IndexChapterPanel_Load
{
    static void Prefix(Chapter chapter, bool isLocked)
    {
        if (chapter.id == "custom_levels")
        {
            ChapterUtils.LoadIndexPage();
        }
    }
}
