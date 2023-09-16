﻿using Il2CppInterop.Runtime;
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
    static readonly Dictionary<LevelID, Dictionary<string, Goal>> goalInfos = new();
    static LevelSpec curlevel; 
    static int normalPages = -1;
    static readonly Dictionary<LevelID, string> filePaths = new();
    static Goal lastRootGoal = new(GoalType.All);

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

        lastRootGoal = new(GoalType.All);
        goalInfos[curlevel.id][name] = lastRootGoal;
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
    static int ProcessGoal(string[] lines, int start, string oldIndent)
    {
        string indent = string.Concat(lines[start].TakeWhile(char.IsWhiteSpace));
        if (!indent.StartsWith(oldIndent))
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

            var source = ActorId.None;
            if (lineParts.Length > 1)
            {
                source = GetEnum<ActorId>(lineParts[1]);
            }
            var target = ActorId.None;
            if (lineParts.Length > 2)
            {
                target = GetEnum<ActorId>(lineParts[2]);
            }
            var query = new Goal(GoalType.Event)
            {
                eventType = GetEnum<ET>(lineParts[0]),
                source = source,
                target = target
            };
            lastRootGoal.goals.Add(query);
        }
        return lines.Length - 1;
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
                AddGoal(string.Join(" ", line.Skip(1)), false);
                return ProcessGoal(lines, start + 1, "");
            case "Subgoal":
                AddGoal(string.Join(" ", line.Skip(1)), true);
                return ProcessGoal(lines, start + 1, "");
            case "Actor":
                Campaign.AddActor(GetEnum<ActorId>(line[1]));
                return start;
            case "Setting":
                Campaign.AddSetting(GetEnum<Setting>(line[1]));
                return start;
            case "MarriageAloneIsSolitude":
                curlevel.marriageAloneIsSolitude = true;
                return start;
            case "WitchStartsHot":
                SetWitchStartsHot();
                return start;
            case "Music":
                musicSources[curlevel.id] = GetEnum<LevelID>(line[1]);
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
        AddGoal("ERROR: " + message, false);
    }
    static void EvalGoal(LevelSpec spec, Story story, EvaluatorResult result)
    {
        foreach (var (goalId, goal) in goalInfos[spec.id])
        {
            if (goal.CheckGoal(spec, story, 0) != -1)
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
        lastRootGoal = new(GoalType.All);
        musicSources[id] = LevelID.Invalid;

        // The toolbox contains all the actors and settings.
        curlevel.toolbox.Clear();
        curlevel.marriageAloneIsSolitude = false;

        try
        {
            var lines = File.ReadAllLines(filePaths[id]);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.All(char.IsWhiteSpace))
                {
                    i = Process(lines, i);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            ReportError(ex.Message);
        }
        catch (IOException)
        {
            ReportError("Level file deleted, renamed, or otherwise inaccessible");
        }
        catch (IndexOutOfRangeException)
        {
            ReportError("Could not parse level file");
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
