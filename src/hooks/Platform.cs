using HarmonyLib;
using System;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Platform), nameof(Platform.GetLocale))]
internal class Platform_GetLocale
{
    // This can throw if steamworks isn't initialized. We catch that error here.
    static Exception Finalizer(ref string __result)
    {
        __result ??= "en-US";
        return null;
    }
}

[HarmonyPatch(typeof(Platform), nameof(Platform.GetPathForSaveSlot))]
internal class Platform_GetPathForSaveSlot
{
    static bool Prefix(int slot, ref string __result)
    {
        __result = $"./custom_levels/save_{slot}";
        return false;
    }
}