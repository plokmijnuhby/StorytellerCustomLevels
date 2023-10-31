using System;
using System.IO;
using System.Linq;

namespace CustomLevels;
internal class ChapterUtils
{
    public static Chapter customChapter;
    public static int insertionPoint;
    static int addedPages = 0;
    static string currentChapterPath;
    static bool chapterAlreadyFixed = false;
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
            Text.AddText("chapter_title_custom_levels", "Custom levels", new string[0]);
        }
        else
        {
            files = Directory.GetFiles(currentChapterPath, "*.txt");
            string chapterName = Path.GetFileName(currentChapterPath)
                .Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries).Last();
            Text.AddText("chapter_title_custom_levels", chapterName, new string[0]);
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

    public static void LoadIndexPage()
    {
        if (chapterAlreadyFixed)
        {
            chapterAlreadyFixed = false;
            return;
        }
        // If we got here, normally that means we flipped right from the index page.
        // There are some other ways of getting here, but they are rare enough to not bother fixing properly -
        // switching to the badges page is a good enough response.
        currentChapterPath = GetFolders().Where(folder => string.Compare(folder, currentChapterPath) > 0).Min();
        var game = Storyteller.game;
        if (currentChapterPath != null)
        {
            LoadChapter();
            game.activePageIndex = insertionPoint;
        }
        else
        {
            game.GoToPageImmediate(insertionPoint + addedPages + 2, true, true);
        }
    }

    public static void GoToPage(ref int pageIndex)
    {
        var game = Storyteller.game;
        // There are four cases here.
        // First, flipping left from the index page.
        if (game.activePageIndex == insertionPoint && pageIndex == insertionPoint - 1)
        {
            currentChapterPath = GetFolders().Where(folder => string.Compare(folder, currentChapterPath) < 0).Max();
            if (currentChapterPath != null)
            {
                LoadChapter();
                pageIndex = insertionPoint;
                game.activePageIndex = insertionPoint + addedPages + 1;
                chapterAlreadyFixed = true;
            }
        }
        // Second, flipping left from the badges page.
        else if (game.activePageIndex == pageIndex + 1 && pageIndex == insertionPoint + addedPages + 1)
        {
            currentChapterPath = GetFolders().Max();
            LoadChapter();
            pageIndex = insertionPoint;
            game.activePageIndex = insertionPoint + addedPages + 1;
            chapterAlreadyFixed = true;
        }
        // Third, flipping right from the epilogue page.
        // We don't set the activePageIndex here, it's already correct.
        else if (game.activePageIndex == insertionPoint - 1 && pageIndex == insertionPoint)
        {
            currentChapterPath = GetFolders().Min();
            LoadChapter();
            pageIndex = insertionPoint;
            chapterAlreadyFixed = true;
        }
        // Fourth, anything else (do nothing).
        // Note that flipping right from the index page falls into this category;
        // this can't be dealt with here, because we have to wait until we have flipped past the levels pages
        // before we reload the levels. So this case is mainly handled by LoadIndexPage.
    }

    public static void InitGame()
    {
        insertionPoint = Storyteller.game.pages.Count - 4;
        Text.AddText("custom_subchapterTitle", "Custom levels", Array.Empty<string>());

        var subchapterTitle = new TextBlock()
        {
            key = "custom_subchapterTitle",
            values = Array.Empty<Il2CppSystem.Object>()
        };

        // illustration_marco is a blank illustration, seen on the left of the list of levels.
        Campaign.BeginChapter("custom_levels", "Custom levels", "illustration_marco");
        customChapter = Campaign.curChapter;
        customChapter.subchapterTitle = subchapterTitle;
        Campaign.EndChapter();

        var indexPage = new PageSpec()
        {
            id = "chapter_custom_levels",
            type = PageType.Index,
            chapterId = "custom_levels"
        };
        Storyteller.game.pages.Insert(insertionPoint, indexPage);
        Storyteller.game.pages.Insert(insertionPoint, indexPage);
        currentChapterPath = GetFolders().Min();
        LoadChapter();
    }
}
