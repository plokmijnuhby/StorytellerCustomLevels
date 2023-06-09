﻿using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(IndexPage), nameof(IndexPage.Load))]
internal class IndexPage_Load
{
    static void Prefix(Chapter chapter)
    {
        if (chapter.id == "custom_levels")
        {
            Utils.LoadChapter(chapter);
        }
    }
}
