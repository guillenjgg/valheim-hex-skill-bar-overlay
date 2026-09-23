using HarmonyLib;
using HexSkillBarOverlay.UI;
using UnityEngine;

namespace HexSkillBarOverlay.Patches
{
    [HarmonyPatch(typeof(Skills), nameof(Skills.RaiseSkill))]
    internal static class SkillsRaiseSkillPatch
    {
        private struct SkillProgressState
        {
            internal float Level;
            internal float Progress;
        }

        private static void Prefix(Skills __instance, Skills.SkillType skillType, out SkillProgressState __state)
        {
            __state = default;

            Player player = Plugin.SkillsPlayerField(__instance);

            if (player != Player.m_localPlayer || skillType == Skills.SkillType.None)
            {
                return;
            }

            Skills.Skill skill = (Skills.Skill)Plugin.SkillsGetSkillMethod(__instance, skillType);

            if (skill == null)
            {
                return;
            }

            __state.Level = Plugin.SkillLevelField(skill);
            __state.Progress = (float)Plugin.SkillGetLevelPercentageMethod(skill);
        }

        private static void Postfix(Skills __instance, Skills.SkillType skillType, SkillProgressState __state)
        {
            Player player = Plugin.SkillsPlayerField(__instance);

            if (player != Player.m_localPlayer || skillType == Skills.SkillType.None)
            {
                return;
            }

            Skills.Skill skill = (Skills.Skill)Plugin.SkillsGetSkillMethod(__instance, skillType);

            if (skill == null)
            {
                return;
            }

            Skills.SkillDef skillInfo = Plugin.SkillInfoField(skill);

            if (skillInfo == null)
            {
                return;
            }

            float level = Plugin.SkillLevelField(skill);
            float progress = (float)Plugin.SkillGetLevelPercentageMethod(skill);

            float levelGain = Mathf.Floor(level) - Mathf.Floor(__state.Level);
            float progressGain = levelGain * 100f + (progress - __state.Progress) * 100f;

            Sprite icon = Plugin.SkillIconField(skillInfo);

            SkillBarPrefabController.Instance?.ShowSkill(skillType, icon, level, progress, progressGain);
        }
    }
}