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
        if (!ChapterUtils.IsInCustomLevel())
        {
            return;
        }
        try
        {
            __result = __result
                || ChapterUtils.GetFile(id + ".png") != null
                || ChapterUtils.GetFile(id + "_*.png") != null;
        }
        catch (IOException) { }
        if (LevelUtils.verbose[LevelUtils.curlevel.id])
        {
            if (__result)
            {
                Plugin.logger.LogMessage("Fetching asset for " + id);
            }
            else
            {
                Plugin.logger.LogMessage("No asset avialable for " + id);
            }
        }
    }
}


[HarmonyPatch(typeof(ResourceLoader), nameof(ResourceLoader.GetAnimation))]
internal class ResourceLoader_GetAnimation
{
    // This function is called once every frame of the animation,
    // so we must maintain a cache to avoid reading the image file every time,
    // which is slow

    static readonly Dictionary<string, (string, DateTime, FrameSpec[])> cache = [];

    static bool Prefix(string id, ref Il2CppReferenceArray<FrameSpec> __result)
    {
        if (!ChapterUtils.IsInCustomLevel())
        {
            return true;
        }
        string file;
        byte[] data;
        DateTime time;
        try
        {
            file = ChapterUtils.GetFile(id + ".png") ?? ChapterUtils.GetFile(id + "_*.png");
            if (file == null)
            {
                return true;
            }
            time = File.GetLastWriteTimeUtc(file);
            if (cache.TryGetValue(id, out var cached))
            {
                (string oldFile, DateTime oldTime, __result) = cached;
                if (oldFile == file && oldTime == time)
                {
                    return false;
                }
            }
            data = File.ReadAllBytes(file);
        }
        catch (IOException)
        {
            return true;
        }
        file = Path.GetFileNameWithoutExtension(file);
        int frames;
        if (file.Length == id.Length)
        {
            frames = 1;
        }
        else if (!int.TryParse(file[(id.Length + 1)..], out frames))
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
                details = Array.Empty<FrameDetail>(),
                padding = 15,
                ppu = 614
            };
        }
        cache[id] = (file, time, __result);
        return false;
    }
}