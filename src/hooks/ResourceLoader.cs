﻿using HarmonyLib;
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

        byte[] data;
        try
        {
            data = File.ReadAllBytes("custom_levels/illustration.png");
        }
        // If there is no illustration, load the blank illustration "illustration_marco" instead.
        catch (IOException)
        {
            id = "illustration_marco";
            return true;
        }
        // Exact dimensions should be 818x1228, but these will be overwritten.
        Texture2D tex = new(0, 0);
        tex.LoadImage(data, true);
        // The 614 here (=1228*0.5) determines how much the image is scaled up or down.
        __result = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 614);
        return false;
    }
}
