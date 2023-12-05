using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
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
        string folder = ChapterUtils.currentChapterPath ?? "./custom_levels";
        try
        {
            data = File.ReadAllBytes(folder + "/illustration.png");
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


[HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.HasAnimation))]
internal class ResourceLoader_HasAnimation
{
    static void Postfix(string id, ref bool __result)
    {
        try
        {
            __result = __result || Directory.GetFiles("./custom_levels/extra", id + "_*.png").Length != 0;
        }
        catch (IOException) { }
    }
}


[HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.GetAnimation))]
internal class ResourceLoader_GetAnimation
{
    // This function is called once every frame of the animation,
    // so we must maintain a cache to avoid reading the image file every time,
    // which is slow

    static Dictionary<string, (DateTime, FrameSpec[])> cache = new();

    static bool Prefix(string id, ref Il2CppReferenceArray<FrameSpec> __result)
    {
        string file;
        byte[] data;
        DateTime time;
        try
        {
            string[] files = Directory.GetFiles("./custom_levels/extra", id + "_*.png");
            if (files.Length == 0)
            {
                return true;
            }
            file = files[0];
            time = File.GetLastWriteTimeUtc(file);
            if(cache.TryGetValue(id, out var cached) && cached.Item1 == time)
            {
                __result = cached.Item2;
                return false;
            }
            data = File.ReadAllBytes(file);
        }
        catch (IOException)
        {
            return true;
        }
        string frameString = file[(file.LastIndexOf('_')+1)..].Split('.')[0];
        if (!int.TryParse(frameString, out int frames))
        {
            return true;
        }
        Texture2D tex = new(0, 0);
        tex.LoadImage(data, true);
        __result = new(frames);
        float width = tex.width / (float)frames;
        for (int i = 0; i < frames; i++)
        {
            __result[i] = new FrameSpec()
            {
                sprite = Sprite.Create(tex, new Rect(width * i, 0, width, tex.height), new Vector2(0.5f, 0.1f), 614),
                details = new FrameDetail[0],
                padding = 15,
                ppu = 614
            };
        }
        cache[id] = (time, __result);
        return false;
    }
}