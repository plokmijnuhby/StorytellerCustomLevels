using HarmonyLib;
using System.IO;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Text), nameof(Text.GetActorLabel))]
internal class Text_GetActorLabel
{
    static bool Prefix(ActorId id, ref string __result)
    {
        foreach (string file in Directory.GetFiles("./custom_levels", "*.actors", SearchOption.AllDirectories))
        {
            foreach (string line in File.ReadLines(file))
            {
                string[] lineParts = line.Split();
                if (lineParts[0] != id.ToString())
                {
                    continue;
                }
                __result = lineParts[1];
                return false;
            }
        }
        return true;
    }
}
