using System;
using System.IO;
using UnityEngine;

namespace CustomLevels;
internal class DebugTools
{
    // Debug functions. A release version of this mod should never run these!
    public static void DumpAllTextures()
    {
        RenderTexture prevRender = RenderTexture.active;
        foreach (var animation in ResourceLoader.animationCache.Keys)
        {
            var texture = ResourceLoader.GetSprite(animation).texture;

            // You'd think we'd be done here, but actually the old texture isn't readable;
            // the CPU component has been deleted, and only the GPU component remains.
            // To read it, we need to render it, then effectively take a screenshot,
            // creating a texture that retains its CPU component.
            var render = RenderTexture.GetTemporary(
                texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear
                );
            Graphics.Blit(texture, render);
            RenderTexture.active = render;
            var newTexture = new Texture2D(texture.width, texture.height);
            newTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            newTexture.Apply();
            RenderTexture.ReleaseTemporary(render);

            // Now we just have to write it.
            Console.WriteLine($"Dumping {animation}");
            File.WriteAllBytes($"output/{animation}.png", ImageConversion.EncodeToPNG(newTexture));
        }
        RenderTexture.active = prevRender;
    }

    public static void DumpAllEnums()
    {
        foreach(var type in new Type[] { typeof(ActorId), typeof(ET), typeof(LevelID), typeof(Setting) })
        {
            File.WriteAllLines("enums/" + type.Name, Enum.GetNames(type));
        }
    }
}
