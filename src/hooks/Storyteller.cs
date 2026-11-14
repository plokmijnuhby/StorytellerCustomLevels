using HarmonyLib;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Storyteller), nameof(Storyteller.Awake))]
internal class Storyteller_Awake
{
    // This function is only ever called once, when the game first loads.
    static void Postfix()
    {
        var game = Storyteller.game;
        var pages = game.pages;
        for (int i = 0; i < pages.Count;)
        {
            if (pages[i].type == PageType.Devil || pages[i].type == PageType.Empty)
            {
                pages.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        // Prevent displaying certain tutorial text
        game.savegame.seenStamps = true;
        // Prevent a check which crashes if there are too few levels in the game
        game.savegame.indexUnlocked = true;
    }
}
