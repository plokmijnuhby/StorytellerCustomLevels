using System;
using System.Collections.Generic;

namespace CustomLevels;

internal enum GoalType
{
    Invalid,
    Event,
    Solver,
    Without,
    All,
    Any,
    Sequence
}


internal struct Goal
{
    public GoalType type;
    public ET eventType;
    public ActorId source;
    public ActorId target;
    public List<Goal> goals;

    public readonly int CheckGoal(LevelSpec spec, Story story, int start)
    {
        switch (type)
        {
            case GoalType.Event:
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
            case GoalType.Without:
            {
                for (int i = 0; i <= start; i++)
                {
                    if (Solver.HasEvent(story, i, eventType, source, target))
                    {
                        return -1;
                    }
                }
                return start;
            }
            case GoalType.Any:
            {
                int end = -1;
                foreach (var goal in goals)
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
            case GoalType.All:
            {
                int end = start;
                foreach (var goal in goals)
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
            case GoalType.Sequence:
            {
                int end = start;
                foreach (var goal in goals)
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
