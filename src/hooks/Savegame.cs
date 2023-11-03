using HarmonyLib;
using System.Linq;
using System.IO;
using System;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Savegame), nameof(Savegame.SetSolution))]
internal class Savegame_SetSolution
{
    static bool Prefix(LevelID levelID, string goalid, StoryConfig config)
    {
        if (!ChapterUtils.allowedIDs.Contains(levelID))
        {
            return true;
        }
        LevelUtils.solutions[(LevelUtils.filePaths[levelID], goalid)] = config;
        return false;
    }
}

[HarmonyPatch(typeof(Savegame), nameof(Savegame.GetGoalConfig))]
internal class Savegame_GetGoalConfig
{
    static bool Prefix(LevelID levelID, string goalid, ref StoryConfig __result)
    {
        if (!ChapterUtils.allowedIDs.Contains(levelID))
        {
            return true;
        }
        LevelUtils.solutions.TryGetValue((LevelUtils.filePaths[levelID], goalid), out __result);
        return false;
    }
}

[HarmonyPatch(typeof(Savegame), nameof(Savegame.IsGoalSolved))]
internal class Savegame_IsGoalSolved
{
    static bool Prefix(LevelID levelID, string goalid, ref bool __result)
    {
        if (!ChapterUtils.allowedIDs.Contains(levelID))
        {
            return true;
        }
        __result = LevelUtils.solutions.ContainsKey((LevelUtils.filePaths[levelID], goalid));
        return false;
    }
}

[HarmonyPatch(typeof(Savegame), nameof(Savegame.SaveToData))]
internal class Savegame_SaveToData
{
    static void Postfix(ref string __result)
    {
        foreach(var ((file, goal), config) in LevelUtils.solutions)
        {
            // We don't want the save file to become arbitrarily long,
            // storing details about levels that no longer exist.
            // If a level was totally deleted, it's best to remove it.
            if (File.Exists(file))
            {
                __result += $"custom:{file}:{goal}:{config.ToText()}\n";
            }
            else
            {
                LevelUtils.solutions.Remove((file, goal));
            }
        }
    }
}

[HarmonyPatch(typeof(Savegame), nameof(Savegame.LoadFromLines))]
internal class Savegame_LoadFromLines
{
    static void Postfix(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray lines)
    {
        foreach(var line in lines.Reverse())
        {
            string[] sections = line.Split(':');
            if (sections[0] != "custom")
            {
                break;
            }
            LevelUtils.solutions[(sections[1], sections[2])] = StoryConfig.FromText(sections[3]);
        }
    }
}

[HarmonyPatch(typeof(Savegame), nameof(Savegame.Reset))]
internal class Savegame_Reset
{
    static void Prefix()
    {
        LevelUtils.solutions.Clear();
    }
}