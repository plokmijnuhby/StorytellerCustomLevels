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
