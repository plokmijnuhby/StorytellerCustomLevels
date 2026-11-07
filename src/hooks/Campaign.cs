using HarmonyLib;
using Il2CppInterop.Runtime;
using System;
namespace CustomLevels.hooks;

[HarmonyPatch(typeof(Campaign), nameof(Campaign.LoadCampaign))]
internal class Campaign_LoadCampaign
{
    static bool Prefix()
    {
        Campaign.levels = new();
        Campaign.levelSpecs.Clear();
        Campaign.chapters = new();
        Campaign.platformAchievements.Clear();
        Campaign.stamps.Clear();
        Campaign.goalDescriptions.Clear();

        Campaign.Begin(LevelID.Macbeth, LevelType.Regular, 3);
        var goal = Campaign.AddGoal("custom_goal_0_0", "Test goal");
        Text.AddText("custom_goal_0_0", "Test goal", []);
        goal.description = new TextBlock
        {
            key = "custom_goal_0_0",
            values = Array.Empty<Il2CppSystem.Object>()
        };
        Campaign.SetEval(DelegateSupport.ConvertDelegate<GoalsEvaluator>((LevelSpec spec, Story story, EvaluatorResult result) => { }));
        Campaign.End();

        Campaign.BeginChapter("test_chapter", "Test Chapter", "illustration_marco", "CHAPTER_LOCKED", "Test");
        Campaign.AddLevel("Test level", LevelID.Macbeth);
        Campaign.EndChapter();

        Plugin.logger.LogInfo(Campaign.levels[0].goals[0].description.values);

        // TODO: Add stamps here?
        // TODO: Meta/stamp/story achievements?

        return false;
    }
}
