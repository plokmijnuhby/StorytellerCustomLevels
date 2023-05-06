namespace CustomLevels;

internal struct GoalQuery
{
    public ET eventType;
    public ActorId source;
    public ActorId target;

    public bool CheckEvent(LevelSpec spec, Story story)
    { 
        for (int i = 0; i < spec.frames; i++)
        {
            if (Solver.HasEvent(story, i, eventType, source, target))
            {
                return true;
            }
        }
        return false;
    }
}
