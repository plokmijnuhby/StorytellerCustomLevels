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
        Transform mainAnchorSubgoals;
        Transform subgoalAnchor;
        switch (frames)
        {
            case < 4:
                settingIdSuffix = "wide3";
                __result.backgroundId = "background_ingame_wide3";
                __result.mainGoalAnchor = __instance.goalsAnchor3NoSubgoals;
                mainAnchorSubgoals = __instance.goalsAnchor3;
                subgoalAnchor = __instance.subgoalsAnchor3;
                __result.framesAnchor = __instance.framesAnchor3;
                __result.toolboxAnchor = __instance.toolsAnchor3;
                break;
            case 4:
            case 9:
                settingIdSuffix = "wide6";
                __result.backgroundId = "background_ingame_wide4";
                __result.mainGoalAnchor = __instance.goalsAnchor4NoSubgoals;
                mainAnchorSubgoals = __instance.goalsAnchor4;
                subgoalAnchor = __instance.subgoalsAnchor4;
                __result.framesAnchor = __instance.framesAnchor4;
                __result.toolboxAnchor = __instance.toolsAnchor4;
                break;
            case 5:
            case 6:
            case > 9:
                settingIdSuffix = "wide6";
                __result.backgroundId = "background_ingame_wide6";
                __result.mainGoalAnchor = __instance.goalsAnchor6NoSubgoals;
                mainAnchorSubgoals = __instance.goalsAnchor6;
                subgoalAnchor = __instance.subgoalsAnchor6;
                __result.framesAnchor = __instance.framesAnchor6;
                __result.toolboxAnchor = __instance.toolsAnchor6;
                break;
            case 7:
            case 8:
                settingIdSuffix = "wide3";
                __result.backgroundId = "background_ingame_wide8";
                __result.mainGoalAnchor = __instance.goalsAnchor8;
                mainAnchorSubgoals = __instance.goalsAnchor6;
                subgoalAnchor = __instance.subgoalsAnchor6;
                __result.framesAnchor = __instance.framesAnchor8;
                __result.toolboxAnchor = __instance.toolsAnchor8;
                break;
        }
        if (spec.HasSubgoals() || (spec.HasDevilGoals() && Storyteller.game.IsDevilUnlocked()))
        {
            __result.mainGoalAnchorPostSubgoalReveal = mainAnchorSubgoals;
            __result.subgoalAnchor = subgoalAnchor;
        }
        else
        {
            __result.mainGoalAnchorPostSubgoalReveal = __result.mainGoalAnchor;
        }

        int rows;
        switch (frames)
        {
            case < 4: rows = 1; break;
            case < 9: rows = 2; break;
            default: rows = 3; break;
        }
        int cols = (frames - 1) / rows + 1;

        __result.framesToolboxSeparatorY = frames < 4 ? -0.32f : -0.55f;
        __result.actorsScale = frames < 4 ? 1.1f : 1.06f;

        switch (frames)
        {
            case < 7:
                __result.framesContainerScale = 1.0f;
                __result.usesCompactFrames = false;
                break;
            case 7:
            case 8:
                __result.framesContainerScale = 0.78f;
                __result.usesCompactFrames = true;
                break;
            default:
                __result.framesContainerScale = 0.66f;
                __result.usesCompactFrames = true;
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

        __result.borderSprites = new string[frames];
        var bounds = __result.settingPlaceholder.bounds;
        Vector3 extents = bounds.extents * __result.framesContainerScale;
        bounds.extents = extents;
        Vector3 framesAnchorPos = __result.framesAnchor.position;
        float yOffset = rows * (extents.y + 0.01f) - 0.01f + framesAnchorPos.y;
        int currentFrame = 0;
        for (int row = 0; row < rows; row++)
        {
            int colsThisRow = cols;
            if (row != 0 && frames % rows >= row)
            {
                colsThisRow--;
            }
            float xOffset = 0.01f - colsThisRow * (extents.x + 0.01f);
            for (int col = 0; col < colsThisRow; col++)
            {
                int spriteNumber;
                if (frames < 7)
                {
                    spriteNumber = currentFrame + 1;
                }
                else
                {
                    // Just trust me
                    int availableBorders = settingIdSuffix == "wide3" ? 3 : 6;
                    spriteNumber = availableBorders * (col + 1) / (colsThisRow + 1) + 1;
                }
                __result.borderSprites[currentFrame++] = $"set_frame_{spriteNumber}_{settingIdSuffix}";

                bounds.center = new Vector3(extents.x + xOffset, yOffset - extents.y);
                __result.frames.Add(bounds);
                xOffset += extents.x * 2 + 0.02f;
            }
            yOffset -= extents.y * 2 + 0.02f;
        }

        return false;
    }
}