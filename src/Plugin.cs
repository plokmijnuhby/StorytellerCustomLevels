using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace CustomLevels;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static ManualLogSource logger;

    public override void Load()
    {
        logger = Log;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}
