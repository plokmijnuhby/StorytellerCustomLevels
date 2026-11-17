using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(CutsceneDirector), nameof(CutsceneDirector.StartOpenBook))]
internal class CutsceneDirector_StartOpenBook
{
    static void Prefix(bool fromBack)
    {
        var game = Storyteller.game;
        if (fromBack && game.savegameCache.totalDevilLevels == 0)
        {
            game.GoToPageImmediate(game.FindFirstPage(PageType.Epilogue), false, true);
        }
    }
}
