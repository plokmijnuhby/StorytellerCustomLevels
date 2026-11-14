using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomLevels;

internal class LevelUtils
{
    static bool logEvents = false;

    static int currentSubgoals = 0;
    public static List<(LevelID, bool)> musicSources = [default];

    static LevelSpec curlevel;
    public static List<Dictionary<string, Goal>> goalInfos = [null];
    public static readonly List<string> filePaths = [null];
    static T GetEnum<T>(string name) where T : struct
    {
        if (Enum.TryParse(name, true, out T result))
        {
            return result;
        }
        else
        {
            throw new InvalidOperationException($"Argument {name} was not a valid value");
        }
    }
    public static string FixMusic(string id)
    {
        if (id.StartsWith("music_"))
        {
            bool isLevel = Enum.TryParse(id["music_".Length..], true, out LevelID levelID);
            if (isLevel)
            {
                var (newLevelID, isDevil) = musicSources[(int)levelID];
                if (newLevelID == LevelID.Invalid) { return null; }
                string result = "music_" + newLevelID.ToString().ToLowerInvariant();
                if (isDevil)
                {
                    result += "_devil";
                }
                return result;
            }
        }
        return id;
    }
    static Goal ProcessEventGoal(string[] line, CustomGoalType type)
    {
        var source = ActorId.None;
        if (line.Length > 1)
        {
            source = GetEnum<ActorId>(line[1]);
        }
        var target = ActorId.None;
        if (line.Length > 2)
        {
            target = GetEnum<ActorId>(line[2]);
        }
        return new Goal(type)
        {
            eventType = GetEnum<ET>(line[0]),
            source = source,
            target = target
        };
    }
    static int ProcessGoal(string[] lines, int start, string oldIndent, Goal root)
    {
        if (lines.Length <= start) return start - 1;
        string indent = string.Concat(lines[start].TakeWhile(char.IsWhiteSpace));
        if (!indent.StartsWith(oldIndent) || indent == oldIndent)
        {
            return start - 1;
        }
        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.All(char.IsWhiteSpace))
            {
                continue;
            }
            if (!line.StartsWith(indent))
            {
                return i - 1;
            }
            string[] lineParts = line[indent.Length..].Split();

            CustomGoalType type = CustomGoalType.Invalid;
            Goal goal = null;
            try
            {
                type = GetEnum<CustomGoalType>(lineParts[0]);
            }
            catch (InvalidOperationException) { }

            if (type == CustomGoalType.Without)
            {
                goal = ProcessEventGoal(lineParts[1..], type);
            }
            else if (type == CustomGoalType.Invalid)
            {
                goal = ProcessEventGoal(lineParts, CustomGoalType.Event);
            }
            else
            {
                goal = new Goal(type);
                i = ProcessGoal(lines, i + 1, indent, goal);
            }
            root.children.Add(goal);
        }
        return lines.Length - 1;
    }
    static Goal AddGoal(string name, GoalType goalType, bool isDevil)
    {
        if (goalType == GoalType.Subgoal)
        {
            if (currentSubgoals++ == 3)
            {
                throw new InvalidOperationException("Can't fit more than three subgoals on screen");
            }
        }

        string internalName = $"custom_goal_{curlevel.id}_{curlevel.goals.Count}";
        var goalSpec = Campaign.AddGoal(internalName, name, goalType);
        goalSpec.isDevil = isDevil;

        // I honestly don't know what the arrays in this function do, but leaving them empty seems fine.
        Text.AddText(internalName, name, []);
        goalSpec.description = new TextBlock
        {
            key = internalName,
            values = Array.Empty<Il2CppSystem.Object>()
        };

        var goal = new Goal(CustomGoalType.All);
        goalInfos[(int)curlevel.id][internalName] = goal;
        return goal;
    }
    static int AddGoal(string[] lines, int start, GoalType goalType, bool isDevil = false)
    {
        // Ignore the word "Goal" or "Subgoal" at the start of the line
        string name = string.Join(' ', lines[start].Split().Skip(1));
        return ProcessGoal(lines, start + 1, "", AddGoal(name, goalType, isDevil));
    }
    static void SetFrames(string frameString)
    {
        if (!int.TryParse(frameString, out int frames))
        {
            throw new InvalidOperationException("Frames must be an integer");
        }
        else if (frames <= 0)
        {
            throw new InvalidOperationException("Frames must be greater than 0");
        }
        curlevel.frames = frames;
    }
    static void SetWitchStartsHot()
    {
        // No, I don't know why this has to be set in the toolbox either.
        curlevel.toolbox.Add(new ToolSpec
        {
            witchStartsHot = true
        });
    }
    static int Process(string[] lines, int start)
    {
        string[] line = lines[start].Split();
        switch (line[0])
        {
            case "Frames":
                SetFrames(line[1]);
                return start;
            case "Goal":
                return AddGoal(lines, start, GoalType.Main);
            case "Subgoal":
                return AddGoal(lines, start, GoalType.Subgoal);
            case "DevilGoal":
                return AddGoal(lines, start, GoalType.Subgoal, true);
            case "Actor":
                Campaign.AddActor(GetEnum<ActorId>(line[1]));
                return start;
            case "Setting":
                Campaign.AddSetting(GetEnum<Setting>(line[1]));
                return start;
            case "WitchStartsHot":
                SetWitchStartsHot();
                return start;
            case "Music":
                musicSources[(int)curlevel.id] = (GetEnum<LevelID>(line[1]), line.Length > 2 && line[2] == "Devil");
                return start;
            default:
                throw new InvalidOperationException("Unknown command: " + line[0]);
        }
    }
    static void ReportError(string message)
    {
        SetFrames("3");
        curlevel.toolbox.Clear();
        curlevel.goals.Clear();
        AddGoal("ERROR: " + message, GoalType.Main, false);
    }
    static void EvalGoal(LevelSpec spec, Story story, EvaluatorResult result)
    {
        if (logEvents)
        {
            var logger = Plugin.logger;
            logger.LogMessage("Logging events:");
            foreach (var storyEvent in story.events)
            {
                logger.LogMessage($"{storyEvent.type} {storyEvent.source} {storyEvent.target} (frame {storyEvent.frame})");
            }
        }

        foreach (var (goalId, goal) in goalInfos[(int)spec.id])
        {
            if (goal.CheckGoal(spec, story, 0) != -1)
            {
                result.SetGoal(goalId);
            }
        }
    }
    public static void LoadLevel(LevelID id, string file)
    {
        logEvents = false;
        try
        {
            foreach(var line in File.ReadLines("./custom_levels/config.txt"))
            {
                switch (line.TrimEnd())
                {
                    case "LogEvents":
                        logEvents = true;
                        break;
                    default:
                        Plugin.logger.LogError($"Unknown config {line.TrimEnd()}");
                        break;
                }
            }
        }
        catch (IOException) { }
        int curId = (int)id;
        Campaign.Begin(id, LevelType.Regular, 3);
        curlevel = Campaign.curlevel;
        currentSubgoals = 0;
        goalInfos[curId].Clear();
        musicSources[curId] = default;

        try
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.All(char.IsWhiteSpace))
                {
                    i = Process(lines, i);
                }
            }
            /*string events = ChapterUtils.GetFile("events.txt");
            if (events != null)
            {
                foreach (string line in File.ReadLines(events))
                {
                    if (line == "")
                    {
                        continue;
                    }
                    string[] lineParts = line.Split();
                    var e = new Event()
                    {
                        type = GetEnum<ET>(lineParts[0]),
                        frame = -1
                    };
                    if (lineParts.Length > 1)
                    {
                        e.source = GetEnum<ActorId>(lineParts[1]);
                    }
                    if (lineParts.Length > 2)
                    {
                        e.target = GetEnum<ActorId>(lineParts[2]);
                    }
                    LevelUtils.events.Add(e);
                }
            }*/
        }
        catch (InvalidOperationException ex)
        {
            ReportError(ex.Message);
        }
        catch (IOException)
        {
            ReportError("File deleted, renamed, or otherwise inaccessible");
        }
        catch (IndexOutOfRangeException)
        {
            ReportError("Could not parse file");
        }

        Campaign.SetEval(DelegateSupport.ConvertDelegate<GoalsEvaluator>(EvalGoal));
        Campaign.End();

        // Verify whether old solutions still work, if not then erase them
        var solutions = Storyteller.game.savegame.solutions;
        for (var i = 0; i < solutions.Count; i++)
        {
            var goal = solutions[i];
            // "current" is the state of the puzzle when the player last saw it,
            // not really a solution as such
            if (goal.levelId == id && goal.goalId != "current")
            {
                var story = Solver.Solve(goal.layout, curlevel.toolbox);
                var evalResult = new EvaluatorResult();
                EvalGoal(curlevel, story, evalResult);
                if (!evalResult.goals.Contains(goal.goalId))
                {
                    solutions.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
