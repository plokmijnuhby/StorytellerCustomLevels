using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomLevels;

internal class Utils
{
    static readonly LevelID[] allowedIDs = new LevelID[]
    {
        // Unused levels, can be overwritten relatively safely.
        // We can only have 7 or we'll run out of space to display them.
        LevelID.Trailer_Reunion,
        LevelID.Trailer_Reunion2,
        LevelID.Trailer_Suitors,
        LevelID.Trailer_Mystery,
        LevelID.Trailer_Monster,
        LevelID.Trailer_Beauty,
        LevelID.Trailer_Death
    };
    static int currentSubgoals = 0;
    static readonly Dictionary<LevelID, Dictionary<string, List<GoalQuery>>> goalInfos = new();
    static List<GoalQuery> lastGoal;
    static LevelSpec curlevel; 
    static int normalPages = -1;
    static readonly Dictionary<LevelID, string> filePaths = new();

    public static readonly Dictionary<LevelID, LevelID> musicSources = new();

    static void AddGoal(string goal, bool isSubgoal)
    {
        // The default version of Campaign.LoadCampaign uses functions that do what I'm about to do in a much simpler way,
        // but they don't work here because I'm overwriting a level, which is not supposed to happen.
        // I honestly don't know what the arrays in this function do, but leaving them empty seems fine.
        var name = $"custom_goal_{curlevel.id}_{curlevel.goals.Count}";

        if (isSubgoal)
        {
            if (currentSubgoals == 3)
            {
                throw new InvalidOperationException("Can't fit more than three subgoals on screen");
            }
            currentSubgoals++;
        }

        Text.AddText(name, goal, Array.Empty<string>());
        var description = new TextBlock()
        {
            key = name,
            values = Array.Empty<Il2CppSystem.Object>()
        };
        var goalSpec = new LevelGoalSpec()
        {
            id = name,
            description = description,
            isSubgoal = isSubgoal
        };
        curlevel.goals.Add(goalSpec);

        lastGoal = new();
        goalInfos[curlevel.id][name] = lastGoal;
    }
    static void SetFrames(string frameString)
    {
        var valid = new string[] { "3", "4", "6", "8" };
        if (!valid.Contains(frameString))
        {
            throw new InvalidOperationException("Frames must be 3, 4, 6, or 8, not " + frameString);
        }

        int frames = int.Parse(frameString);
        curlevel.frames = frames;

        // The settings that are in place at the start of the level.
        // Used for some tutorials, but here we can just leave them as the default Setting.Empty.
        curlevel.startingSettings = new Setting[frames];
    }
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
    static void SetWitchStartsHot()
    {
        // No, I don't know why this has to be set in the toolbox either.
        curlevel.toolbox.Add(new ToolSpec
        {
            witchStartsHot = true
        });
    }
    static void Process(string[] line)
    {
        try
        {
            switch (line[0])
            {
                case "Frames":
                    SetFrames(line[1]);
                    break;
                case "Goal":
                    AddGoal(string.Join(" ", line.Skip(1)), false);
                    break;
                case "Subgoal":
                    AddGoal(string.Join(" ", line.Skip(1)), true);
                    break;
                case "Actor":
                    Campaign.AddActor(GetEnum<ActorId>(line[1]));
                    break;
                case "Setting":
                    Campaign.AddSetting(GetEnum<Setting>(line[1]));
                    break;
                case "MarriageAloneIsSolitude":
                    curlevel.marriageAloneIsSolitude = true;
                    break;
                case "WitchStartsHot":
                    SetWitchStartsHot();
                    break;
                case "Music":
                    musicSources[curlevel.id] = GetEnum<LevelID>(line[1]);
                    break;
                default:
                    throw new InvalidOperationException("Unknown command: " + line[0]);
            }
        }
        catch (IndexOutOfRangeException) {
            throw new InvalidOperationException("Could not parse level file");
        }
    }
    static void ProcessGoal(string[] line)
    {
        if (line.Length == 0) return;
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
        var query = new GoalQuery
        {
            eventType = GetEnum<ET>(line[0]),
            source = source,
            target = target
        };
        lastGoal.Add(query);
    }
    static void ReportError(string message)
    {
        SetFrames("3");
        curlevel.toolbox.Clear();
        curlevel.goals.Clear();
        AddGoal("ERROR: " + message, false);
    }
    static void EvalGoal(LevelSpec spec, Story story, EvaluatorResult result)
    {
        foreach (var (goalId, queries) in goalInfos[spec.id])
        {
            bool succeeded = true;
            foreach (var query in queries)
            {
                if (!query.CheckEvent(spec, story))
                {
                    succeeded = false;
                    break;
                }
            }
            if (succeeded)
            {
                result.SetGoal(goalId);
            }
        }
    }
    public static void LoadLevel(LevelID id)
    {
        if (!allowedIDs.Contains(id)) return;

        Campaign.Begin(id, 3);
        curlevel = Campaign.curlevel;
        currentSubgoals = 0;
        goalInfos[id] = new();
        lastGoal = new();
        musicSources[id] = LevelID.Invalid;

        // The toolbox contains all the actors and settings.
        curlevel.toolbox.Clear();
        curlevel.marriageAloneIsSolitude = false;

        try
        {
            foreach (var line in File.ReadAllLines(filePaths[id]))
            {
                if (line != "" && (line[0] == ' ' || line[0] == '\t'))
                {
                    var trimmed_line = line.Trim();
                    ProcessGoal(trimmed_line.Split(' '));
                }
                else if (line != "")
                {
                    var splitLine = line.Split(' ');
                    Process(splitLine);
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            ReportError(ex.Message);
        }
        catch (IOException)
        {
            ReportError("Level file deleted, renamed, or otherwise inaccessible");
        }
        if (currentSubgoals != 0 && curlevel.frames == 8)
        {
            ReportError("Due to display bug, can't have subgoals when 8 frames are used");
        }

        Campaign.SetEval(DelegateSupport.ConvertDelegate<GoalsEvaluator>(EvalGoal));

        Campaign.End();
        Campaign.goalDescriptions.Clear();
    }


    public static void LoadChapter(Chapter chapter)
    {
        // Note: We want to insert all pages before the final page (the epilogue).

        var pages = Storyteller.game.pages;
        if (normalPages == -1)
        {
            // First time running this function, therefore levels haven't been loaded in yet
            normalPages = pages.Count;
        }
        else
        {
            // Not the first time, so we must remove the pages we added last time
            pages.RemoveRange(normalPages - 1, pages.Count - normalPages);
        }

        filePaths.Clear();
        chapter.levels.Clear();
        goalInfos.Clear();
        musicSources.Clear();
        Campaign.curChapter = chapter;
        Campaign.chapterLevelNumber = 1;

        if (!Directory.Exists("./custom_levels"))
        {
            Directory.CreateDirectory("./custom_levels");
            // Obviously there won't be any levels in the directory in this case
            return;
        }

        var files = Directory.GetFiles("./custom_levels", "*.txt");
        Array.Sort(files);
        foreach (var (file, id) in Enumerable.Zip(files, allowedIDs))
        {
            filePaths[id] = file;
            var name = Path.GetFileNameWithoutExtension(file);
            Campaign.AddLevel(name.Replace('_', ' '), id);

            var page = new PageSpec()
            {
                id = "level_" + name,
                type = PageType.Level,
                levelId = id
            };
            pages.Insert(pages.Count - 1, page);

            LoadLevel(id);
        }
    }
}
