using System;
using System.Collections.Generic;

namespace CustomLevels;

internal enum GoalType
{
    Invalid,
    Event,
    Solver,
    All,
    Any,
    Sequence
}


internal class Goal
{
    public GoalType type;
    public ET eventType;
    public ActorId source;
    public ActorId target;
    public List<Goal> goals = new();
    public List<Goal> withouts = new();

    public Goal(GoalType type)
    {
        this.type = type;
    }

    private bool CheckWithouts(Story story, int end)
    {
        foreach (var goal in withouts)
        {
            for (int i = 0; i <= end; i++)
            {
                if (Solver.HasEvent(story, i, goal.eventType, goal.source, goal.target))
                {
                    return false;
                }
            }
        }
        return true;
    }


    public int CheckGoal(LevelSpec spec, Story story, int start)
    {
        int end;
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
            case GoalType.Any:
            {
                end = -1;
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
                break;
            }
            case GoalType.All:
            {
                end = start;
                foreach (var goal in goals)
                {
                    int found = goal.CheckGoal(spec, story, start);
                    if (found == -1)
                    {
                        return -1;
                    }
                    end = Math.Max(end, found);
                }
                break;
            }
            case GoalType.Sequence:
            {
                end = start;
                foreach (var goal in goals)
                {
                    end = goal.CheckGoal(spec, story, end);
                    if (end == -1)
                    {
                        return -1;
                    }
                }
                break;
            }
            default: return -1;
        }
        if (CheckWithouts(story, end))
        {
            return end;
        }
        return -1;
    }
}
