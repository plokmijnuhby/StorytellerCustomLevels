using HarmonyLib;
using System.Collections.Generic;

/*
 * This one is confusing.
 * We want to be able to patch the Solver.Solve function to add in some more events at the start of the story.
 * This is a very big function, so a complete reimplementation isn't feasible.
 * We need to somehow gain access to the Story object created near the start of the function,
 * which we can do with Solver.Add. However, in some stories an event is not added until it is too late,
 * so we need to force Solver to add an event earlier.
 * This can be done by forcing a monarch to appear in the toolbox chars, adding an event giving them a crown.
 */

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Solver), nameof(Solver.Solve))]
internal class Solver_Solve
{
    internal static bool addingQueen = false;
    static void Prefix()
    {
        addingQueen = true;
    }
}

[HarmonyPatch(typeof(Solver), nameof(Solver.ToolboxChars))]
internal class Solver_ToolboxChars
{
    static void Postfix(Il2CppSystem.Collections.Generic.HashSet<ActorId> __result)
    {
        if (__result.Contains(ActorId.Queen) || __result.Contains(ActorId.King))
        {
            Solver_Solve.addingQueen = false;
        }
        else if (Solver_Solve.addingQueen)
        {
            __result.Add(ActorId.Queen);
        }
    }
}

[HarmonyPatch(typeof(Solver), nameof(Solver.Add))]
internal class Solver_Add
{
    static bool Prefix(Story story, int frame, ET type, ActorId source, ActorId target, ref Event __result)
    {
        if (frame > -1)
        {
            return true;
        }
        Event[] events = story.events;
        if (events.Length == 0)
        {
            events = [.. LevelUtils.eventList];
        }
        if (Solver_Solve.addingQueen && source == ActorId.Queen)
        {
            story.events = events;
            Solver_Solve.addingQueen = false;
            return false;
        }
        __result = new Event
        {
            frame = -2,
            type = type,
            source = source,
            target = target
        };
        List<Event> eventList = [.. events];
        eventList.Insert(eventList.Count - LevelUtils.eventList.Count, __result);
        story.events = eventList.ToArray();
        return false;
    }
}