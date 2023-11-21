using HarmonyLib;
using UnityEngine;

namespace CustomLevels.hooks;

[HarmonyPatch(typeof(LevelPage), nameof(LevelPage.LoadLevel))]
internal class LevelPage_LoadLevel
{
    static void Prefix(LevelID id)
    {
        LevelUtils.LoadLevel(id);
    }
}

[HarmonyPatch(typeof(LevelPage), nameof(LevelPage.ComputeLayout))]
internal class LevelPage_ComputeLayout
{
    static bool Prefix(LevelPage __instance, LevelSpec spec, ref LevelLayout __result)
    {
        __result = new LevelLayout();
        int frames = spec.frames;
        string settingIdSuffix;
        int cols;
        int rows;
        switch (frames)
        {
            case 2:
                settingIdSuffix = "wide3";
                __result.backgroundId = "background_ingame_wide2";
                cols = 2;
                rows = 1;
                break;
            case 3:
                settingIdSuffix = "wide3";
                __result.backgroundId = "background_ingame_wide3";
                cols = 3;
                rows = 1;
                break;
            case 4:
                settingIdSuffix = "wide6";
                __result.backgroundId = "background_ingame_wide4";
                cols = 2;
                rows = 2;
                break;
            case 6:
                settingIdSuffix = "wide6";
                __result.backgroundId = "background_ingame_wide6";
                cols = 3;
                rows = 2;
                break;
            case 8:
                settingIdSuffix = "wide3";
                __result.backgroundId = "background_ingame_wide8";
                cols = 4;
                rows = 2;
                break;
            default:
                settingIdSuffix = "wide6";
                __result.backgroundId = "background_ingame_wide6";
                cols = frames;
                rows = 1;
                break;
        }
        __result.framesToolboxSeparatorY = frames < 4 ? -0.32f : -0.55f;
        __result.actorsScale = frames < 4 ? 1.1f : 1.06f;
        __result.framesContainerScale = frames < 7 ? 1.0f : 0.78f;
        __result.usesCompactFrames = frames < 7;
        
        if (frames == 8)
        {
            __result.borderSprites = new string[]
            {
                "set_frame_1_wide3",
                "set_frame_2_wide3",
                "set_frame_2_wide3",
                "set_frame_3_wide3",
                "set_frame_1_wide3",
                "set_frame_2_wide3",
                "set_frame_2_wide3",
                "set_frame_3_wide3"
            };
        }
        else
        {
            __result.borderSprites = new string[frames];
            for (int frame = 0; frame < frames; frame++)
            {
                __result.borderSprites[frame] = $"set_frame_{frame + 1}_{settingIdSuffix}";
            }
        }
        
        bool hasSubgoals = spec.HasSubgoals() || (spec.HasDevilGoals() && Storyteller.game.IsDevilUnlocked());
        switch (frames)
        {
            case < 4:
                __result.mainGoalAnchor = __instance.goalsAnchor3NoSubgoals;
                if (hasSubgoals)
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor3;
                    __result.subgoalAnchor = __instance.subgoalsAnchor3;
                }
                else
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor3NoSubgoals;
                }
                __result.framesAnchor = __instance.framesAnchor3;
                __result.toolboxAnchor = __instance.toolsAnchor3;
                break;
            case 4:
                __result.mainGoalAnchor = __instance.goalsAnchor4NoSubgoals;
                if (hasSubgoals)
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor4;
                    __result.subgoalAnchor = __instance.subgoalsAnchor4;
                }
                else
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor4NoSubgoals;
                }
                __result.framesAnchor = __instance.framesAnchor4;
                __result.toolboxAnchor = __instance.toolsAnchor4;
                break;
            case 6:
                __result.mainGoalAnchor = __instance.goalsAnchor6NoSubgoals;
                if (hasSubgoals)
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor6;
                    __result.subgoalAnchor = __instance.subgoalsAnchor6;
                }
                else
                {
                    __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor6NoSubgoals;
                }
                __result.framesAnchor = __instance.framesAnchor6;
                __result.toolboxAnchor = __instance.toolsAnchor6;
                break;
            case 8:
                __result.mainGoalAnchor = __instance.goalsAnchor8;
                __result.mainGoalAnchorPostSubgoalReveal = __instance.goalsAnchor8;
                __result.framesAnchor = __instance.framesAnchor8;
                __result.toolboxAnchor = __instance.toolsAnchor8;
                break;
        }

        __result.settingIdSuffix = settingIdSuffix;
        __result.supportedSettingSuffixes = new string[] { settingIdSuffix, "wide3", "wide6" };
        __result.settingPlaceholder = ResourceLoader.GetSprite("set_placeholder_" + settingIdSuffix);
        __result.shadowSprite = ResourceLoader.GetSprite("set_framecontainer_shadow_" + settingIdSuffix);
        __result.extrusionSprite = ResourceLoader.GetSprite("set_extrude_" + settingIdSuffix);
        __result.dragBorderSprite = ResourceLoader.GetSprite("set_frame_dragged_" + settingIdSuffix);
        __result.cols = cols;
        __result.rows = rows;
        __result.gutterH = 0.02f;
        __result.gutterW = 0.02f;
        __result.frameSelectorSprite = "ui_frame_selector_" + settingIdSuffix;

        var bounds = __result.settingPlaceholder.bounds;
        Vector3 extents = bounds.extents * __result.framesContainerScale;
        bounds.extents = extents;
        Vector3 framesAnchorPos = __result.framesAnchor.position;
        float yOffset = rows * (extents.y + 0.01f) - 0.01f + framesAnchorPos.y;
        for (int row = 0; row < rows; row++)
        {
            float xOffset = 0.01f - cols * (extents.x + 0.01f);
            for (int col = 0; col < cols; col++)
            {
                bounds.center = new Vector3(extents.x + xOffset, yOffset - extents.y);
                __result.frames.Add(bounds);
                xOffset += extents.x * 2 + 0.02f;
            }
            yOffset -= extents.y * 2 + 0.02f;
        }

        return false;
    }
}