using System;
using System.IO;
using System.Linq;

namespace CustomLevels;
internal class ChapterUtils
{
    static int insertionPoint;
    static Chapter customChapter;
    static int addedPages = 0;
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
        pages.RemoveRange(insertionPoint + 1, addedPages);
        addedPages = 0;
        
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
            addedPages++;

            var page = new PageSpec()
            {
                id = "level_" + name,
                type = PageType.Level,
                levelId = id
            };
            pages.Insert(insertionPoint + addedPages, page);

            // Load the level here so that save games work properly
            LevelUtils.filePaths[id] = file;
            LevelUtils.LoadLevel(id);
        }
        Storyteller.game.UpdateSavegameCache();
    }

    public static Chapter CreateChapter(string id, string name)
    {
        var subchapterTitle = new TextBlock()
        {
            key = "custom_subchapterTitle",
            values = Array.Empty<Il2CppSystem.Object>()
        };

        // illustration_marco is a blank illustration, seen on the left of the list of levels.
        Campaign.BeginChapter(id, name, "illustration_marco");
        var chapter = Campaign.curChapter;
        chapter.subchapterTitle = subchapterTitle;
        Campaign.EndChapter();

        var indexPage = new PageSpec()
        {
            id = "chapter_" + id,
            type = PageType.Index,
            chapterId = id
        };
        Storyteller.game.pages.Insert(insertionPoint, indexPage);
        return chapter;
    }

    public static void InitGame()
    {
        insertionPoint = Storyteller.game.pages.Count - 4;
        Text.AddText("custom_subchapterTitle", "Custom chapter", Array.Empty<string>());

        CreateChapter("custom_levels_helper", "You shouldn't be seeing this,\nplease ignore it");
        customChapter = CreateChapter("custom_levels", "Custom levels");
        LoadChapter();
    }
}
