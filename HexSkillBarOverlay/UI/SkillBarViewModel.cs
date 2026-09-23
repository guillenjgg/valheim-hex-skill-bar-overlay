using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexSkillBarOverlay.UI.Models
{
    internal sealed class SkillBarViewModel
    {
        internal GameObject GameObject { get; }
        internal RectTransform RectTransform { get; }
        internal Image SkillIcon { get; }
        internal Image ProgressFill { get; }
        internal RectTransform ProgressFillRect { get; }
        internal TextMeshProUGUI LevelText { get; }
        internal TextMeshProUGUI SkillNameText { get; }
        internal TextMeshProUGUI GainText { get; }

        internal float HideTime { get; set; }
        internal bool IsVisible { get; set; }

        internal SkillBarViewModel(
            GameObject gameObject,
            RectTransform rectTransform,
            Image skillIcon,
            Image progressFill,
            RectTransform progressFillRect,
            TextMeshProUGUI levelText,
            TextMeshProUGUI skillNameText,
            TextMeshProUGUI gainText)
        {
            GameObject = gameObject;
            RectTransform = rectTransform;
            SkillIcon = skillIcon;
            ProgressFill = progressFill;
            ProgressFillRect = progressFillRect;
            LevelText = levelText;
            SkillNameText = skillNameText;
            GainText = gainText;
        }
    }
}