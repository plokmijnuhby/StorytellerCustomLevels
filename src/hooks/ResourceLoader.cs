using HarmonyLib;
using System.IO;
using UnityEngine;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.GetSprite))]
internal class ResourceLoader_GetSprite
{
    static bool Prefix(ref string id, ref Sprite __result)
    {
        if (id != "illustration_custom")
        {
            return true;
        }
        byte[] data = File.ReadAllBytes("custom_levels/illustration.png");
        Texture2D tex = new(818, 1228);
        tex.LoadImage(data, true);
        __result = Sprite.Create(tex, new Rect(0, 0, 818, 1228), new Vector2(0.5f, 0.5f), 614);
        return false;
    }
}
