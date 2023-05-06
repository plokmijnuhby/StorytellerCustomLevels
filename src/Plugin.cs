using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace CustomLevels;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}
