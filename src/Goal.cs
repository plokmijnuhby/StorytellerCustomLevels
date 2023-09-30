using System;
using System.Collections.Generic;

namespace CustomLevels;

internal enum CustomGoalType
{
    Invalid,
    Event,
    Without,
    Solver,
    All,
    Any,
    Sequence
}


internal class Goal
{
    public CustomGoalType type;
    public ET eventType;
    public ActorId source;
    public ActorId target;
    public List<Goal> children = new();

    public Goal(CustomGoalType type)
    {
        this.type = type;
    }

    public int CheckGoal(LevelSpec spec, Story story, int start)
    {
        switch (type)
        {
            case CustomGoalType.Event:
            {
                for (int i = start; i < spec.frames; i++)
                {
                    if (Solver.HasEvent(story, i, eventType, source, target))
                    {
                        return i;
                    }
                }
                return -1;
            }
            case CustomGoalType.Without:
            {
                for (int i = 0; i < spec.frames; i++)
                {
                    if (Solver.HasEvent(story, i, eventType, source, target))
                    {
                        return -1;
                    }
                }
                return start;
            }
            case CustomGoalType.Any:
            {
                int end = -1;
                foreach (var goal in children)
                {
                    int found = goal.CheckGoal(spec, story, start);
                    if (end == -1)
                    {
                        end = found;
                    }
                    else if (found != -1)
                    {
                        end = Math.Min(end, found);
                    }
                }
                return end;
            }
            case CustomGoalType.All:
            {
                int end = start;
                foreach (var goal in children)
                {
                    int found = goal.CheckGoal(spec, story, start);
                    if (found == -1)
                    {
                        return -1;
                    }
                    end = Math.Max(end, found);
                }
                return end;
            }
            case CustomGoalType.Sequence:
            {
                int end = start;
                foreach (var goal in children)
                {
                    end = goal.CheckGoal(spec, story, end);
                    if (end == -1)
                    {
                        return -1;
                    }
                }
                return end;
            }
            default: return -1;
        }
    }
}
