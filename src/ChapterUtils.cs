using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace CustomLevels;
internal class ChapterUtils
{
    public static Chapter customChapter;
    public static int insertionPoint;
    static int addedPages = 0;
    public static string currentChapterPath;
    public static readonly LevelID[] allowedIDs =
    [
        // Unused levels, can be overwritten relatively safely.
        // We can only have 7 or we'll run out of space to display them.
        LevelID.GenesisSandbox,
        LevelID.DwarfSandbox,
        LevelID.CrownSandbox,
        LevelID.VampireSandbox,
        LevelID.MansionSandbox,
        LevelID.GothicSandbox,
        LevelID.DogSandbox
    ];

    static string[] GetFolders()
    {
        if (!Directory.Exists("./custom_levels"))
        {
            Directory.CreateDirectory("./custom_levels");
            // Obviously there won't be any folders in this case
            return [];
        }
        else
        {
            return Directory.GetDirectories("./custom_levels")
                .Where(dir => Directory.GetFiles(dir, "*.txt").Length != 0).ToArray();
        }
    }

    static string[] SplitWhitespace(string x, bool extension = false)
    {
        string name;
        if (extension)
        {
            name = Path.GetFileNameWithoutExtension(x);
        }
        else
        {
            name = Path.GetFileName(x);
        }
        return name.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
    }

    static int Compare(string x, string y)
    {
        if (x == null)
        {
            return y == null ? 0 : -1;
        }
        else if (y == null)
        {
            return 1;
        }
        string xPrefix = SplitWhitespace(x)[0];
        string yPrefix = SplitWhitespace(y)[0];

        // "C10" > "C2"
        int res = xPrefix.Length.CompareTo(yPrefix.Length);
        if (res == 0)
        {
            // "C2" > "C1"
            res = string.Compare(xPrefix, yPrefix, false, CultureInfo.InvariantCulture);
            if (res == 0)
            {
                // "C 2" > "C 1" 
                res = string.Compare(x, y, false, CultureInfo.InvariantCulture);
            }
        }
        return res;
    }

    static readonly Comparer<string> comparer = Comparer<string>.Create(Compare);

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
            Text.AddText("chapter_title_custom_levels", "Custom levels", Array.Empty<string>());
        }
        else
        {
            files = Directory.GetFiles(currentChapterPath, "*.txt");
            string chapterName = SplitWhitespace(currentChapterPath).Last();
            Text.AddText("chapter_title_custom_levels", chapterName, Array.Empty<string>());
        }
        Array.Sort(files, Compare);
        foreach (var (file, id) in Enumerable.Zip(files, allowedIDs))
        {
            string name = SplitWhitespace(file, true).Last();
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
        var game = Storyteller.game;
        if (game.activePageIndex == insertionPoint)
        {
            return;
        }
        // If we got here, normally that means we flipped right from the index page,
        // although there are some other ways of getting here too.
        currentChapterPath = GetFolders().Where(folder => Compare(folder, currentChapterPath) > 0)
            .Min(comparer);
        if (currentChapterPath != null)
        {
            LoadChapter();
            game.activePageIndex = insertionPoint;
            game.book.pageManager.currentPageTargetIndex = -1;
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
            currentChapterPath = GetFolders().Where(folder => Compare(folder, currentChapterPath) < 0)
                .Max(comparer);
            if (currentChapterPath != null)
            {
                LoadChapter();
                pageIndex = insertionPoint;
                game.activePageIndex = insertionPoint + addedPages + 1;
            }
        }
        // Second, flipping left from the badges page.
        else if (game.activePageIndex == pageIndex + 1 && pageIndex == insertionPoint + addedPages + 1)
        {
            currentChapterPath = GetFolders().Max(comparer);
            LoadChapter();
            pageIndex = insertionPoint;
            game.activePageIndex = insertionPoint + addedPages + 1;
        }
        // Third, flipping right from the epilogue page.
        // We don't set the activePageIndex here, it's already correct.
        else if (game.activePageIndex == insertionPoint - 1 && pageIndex == insertionPoint)
        {
            currentChapterPath = GetFolders().Min(comparer);
            LoadChapter();
            pageIndex = insertionPoint;
        }
        // Fourth, anything else (do nothing).
        // Note that flipping right from the index page falls into this category;
        // this can't be dealt with here, because we have to wait until we have flipped past the levels pages
        // before we reload the levels. So this case is mainly handled by LoadIndexPage.
    }

    public static void InitGame()
    {
        insertionPoint = Storyteller.game.pages.Count - 4;
        Text.AddText("custom_subchapterTitle", "Custom chapter", Array.Empty<string>());

        var subchapterTitle = new TextBlock()
        {
            key = "custom_subchapterTitle",
            values = Array.Empty<Il2CppSystem.Object>()
        };

        Campaign.BeginChapter("custom_levels", "Custom levels", "illustration_custom");
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
        currentChapterPath = GetFolders().Min(comparer);
        LoadChapter();
    }
}
