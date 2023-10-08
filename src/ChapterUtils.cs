using System;
using System.IO;
using System.Linq;

namespace CustomLevels;
internal class ChapterUtils
{
    public static Chapter customChapter;
    static int normalPages = -1;
    static string[] chapterPaths;
    static readonly LevelID[] allowedIDs = new LevelID[]
    {
        // Unused levels, can be overwritten relatively safely.
        // We can only have 7 or we'll run out of space to display them.
        LevelID.GenesisSandbox,
        LevelID.DwarfSandbox,
        LevelID.CrownSandbox,
        LevelID.VampireSandbox,
        LevelID.MansionSandbox,
        LevelID.GothicSandbox,
        LevelID.DogSandbox
    };

    public static void LoadChapter()
    {
        var pages = Storyteller.game.pages;
        if (normalPages == -1)
        {
            // First time running this function, therefore levels haven't been loaded in yet
            normalPages = pages.Count;
        }
        else
        {
            // Not the first time, so we must remove the pages we added last time
            pages.RemoveRange(normalPages - 4, pages.Count - normalPages);
        }
        
        customChapter.levels.Clear();
        LevelUtils.ClearLevelData();
        Campaign.curChapter = customChapter;
        Campaign.chapterLevelNumber = 1;

        if (!Directory.Exists("./custom_levels"))
        {
            Directory.CreateDirectory("./custom_levels");
            // Obviously there won't be any levels in the directory in this case
            return;
        }

        chapterPaths = Directory.GetDirectories("./custom_levels");
        string[] files;
        if (chapterPaths.Length == 0)
        {
            files = Directory.GetFiles("./custom_levels", "*.txt");
        }
        else
        {
            Array.Sort(chapterPaths);
            files = Directory.GetFiles(chapterPaths[0], "*.txt");
        }
        Array.Sort(files);
        foreach (var (file, id) in Enumerable.Zip(files, allowedIDs))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            name = name.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries).Last();
            Campaign.AddLevel(name.Replace('_', ' '), id);

            var page = new PageSpec()
            {
                id = "level_" + name,
                type = PageType.Level,
                levelId = id
            };
            pages.Insert(pages.Count - 4, page);

            // Load the level here so that save games work properly
            LevelUtils.filePaths[id] = file;
            LevelUtils.LoadLevel(id);
        }
        Storyteller.game.UpdateSavegameCache();
    }
}
