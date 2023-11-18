using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomLevels;
internal class DebugTools
{
    // Debug functions. A release version of this mod should never run these!

    public static void DumpEnums()
    {
        var enums = new Dictionary<string, Type> {
            { "actor_names", typeof(ActorId) },
            { "event_names", typeof(ET) },
            { "level_names", typeof(LevelID) },
            { "setting_names", typeof(Setting) }
        };
        foreach(var (file, type) in enums)
        {
            File.WriteAllLines($"enums/{file}.txt", Enum.GetNames(type));
        }
    }
}
