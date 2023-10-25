using System;
using System.IO;
using System.Linq;

namespace CustomLevels;
internal class ChapterUtils
{
    static int insertionPoint;
    static Chapter customChapter;
    static int addedPages = 0;
    static string currentChapterPath;
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

    static string[] GetFolders()
    {
        if (!Directory.Exists("./custom_levels"))
        {
            Directory.CreateDirectory("./custom_levels");
            // Obviously there won't be any folders in this case
            return new string[0];
        }
        else
        {
            return Directory.GetDirectories("./custom_levels");
        }
    }

    public static void LoadChapter()
    {
        var pages = Storyteller.game.pages;
        pages.RemoveRange(insertionPoint + 1, addedPages);
        addedPages = 0;
        
        customChapter.levels.Clear();
        LevelUtils.ClearLevelData();
        Campaign.curChapter = customChapter;
        Campaign.chapterLevelNumber = 1;

        string[] files;
        if (currentChapterPath == null)
        {
            files = Directory.GetFiles("./custom_levels", "*.txt");
        }
        else
        {
            files = Directory.GetFiles(currentChapterPath, "*.txt");
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

    public static void LoadNextChapter()
    {
        currentChapterPath = GetFolders().Where(folder => string.Compare(folder, currentChapterPath) > 0).Min();
        var game = Storyteller.game;
        if (currentChapterPath != null)
        {
            LoadChapter();
            game.activePageIndex = insertionPoint;
        }
        else
        {
            game.GoToPageImmediate(game.activePageIndex + 1, false, true);
        }
    }

    public static void GoToPage(ref int pageIndex)
    {
        var game = Storyteller.game;
        if (game.activePageIndex == insertionPoint)
        {
            if (pageIndex == insertionPoint - 1)
            {
                currentChapterPath = GetFolders().Where(folder => string.Compare(folder, currentChapterPath) < 0).Max();
                if (currentChapterPath != null)
                {
                    LoadChapter();
                    pageIndex = insertionPoint;
                    game.activePageIndex = insertionPoint + addedPages + 1;
                }
            }
        }
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
        currentChapterPath = GetFolders().Min();
        LoadChapter();
    }
}
