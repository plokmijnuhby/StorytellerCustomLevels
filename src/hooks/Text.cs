using HarmonyLib;
using System.IO;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Text), nameof(Text.GetActorLabel))]
internal class Text_GetActorLabel
{
    static bool Prefix(ActorId id, ref string __result)
    {
        if (!ChapterUtils.InCustomLevel())
        {
            return true;
        }
        string file = ChapterUtils.GetFile("actors.txt");
        if (file == null)
        {
            return true;
        }
        foreach (string line in File.ReadLines(file))
        {
            string[] lineParts = line.Split();
            if (lineParts.Length >= 2 && lineParts[0] == id.ToString())
            {
                __result = lineParts[1];
                return false;
            }
        }
        return true;
    }
}
