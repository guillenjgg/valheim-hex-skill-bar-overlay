using HexSkillBarOverlay.UI.Models;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexSkillBarOverlay.UI
{
    internal sealed class SkillBarPrefabController : MonoBehaviour
    {
        private const float DisplayDuration = 2f;
        private const float GainPulseScale = 1.5f;
        private const float GainPulseDuration = 0.25f;

        private readonly Dictionary<Skills.SkillType, SkillBarViewModel> _skillBars = new Dictionary<Skills.SkillType, SkillBarViewModel>();
        private readonly List<Skills.SkillType> _activeSkills = new List<Skills.SkillType>();
        private readonly Dictionary<Skills.SkillType, float> _gainPulseTimes = new Dictionary<Skills.SkillType, float>();

        private GameObject _canvasObject;
        private GameObject _skillBarsObject;
        private RectTransform _container;
        private GameObject _skillBarTemplate;
        private SkillBarDragHandler _dragHandler;
        private GameObject _dragArea;
        private TextMeshProUGUI _dragText;

        internal static SkillBarPrefabController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            Plugin.EnableDragging.SettingChanged += OnEnableDraggingChanged;
        }

        private void Update()
        {
            if (_activeSkills.Count == 0)
            {
                return;
            }

            for (int i = _activeSkills.Count - 1; i >= 0; i--)
            {
                Skills.SkillType skillType = _activeSkills[i];
                SkillBarViewModel skillBar = _skillBars[skillType];

                UpdateGainPulse(skillType, skillBar);

                if (Time.time < skillBar.HideTime)
                {
                    continue;
                }

                skillBar.GameObject.SetActive(false);
                skillBar.IsVisible = false;
                skillBar.GainText.rectTransform.localScale = Vector3.one;
                skillBar.GainText.fontStyle = FontStyles.Normal;

                _gainPulseTimes.Remove(skillType);
                _activeSkills.RemoveAt(i);
            }
        }

        internal void CreateUi()
        {
            if (_canvasObject != null)
            {
                return;
            }

            if (Plugin.SkillBarPrefab == null)
            {
                return;
            }

            _canvasObject = new GameObject("HexSkillBarPrefabCanvas");
            _canvasObject.transform.SetParent(transform, false);

            Canvas canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 400;

            CanvasScaler canvasScaler = _canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            _canvasObject.AddComponent<GraphicRaycaster>();

            _skillBarsObject = Instantiate(Plugin.SkillBarPrefab, _canvasObject.transform, false);
            _skillBarsObject.name = "HexSkillBarsRuntime";

            _container = _skillBarsObject.GetComponent<RectTransform>();

            if (_container != null)
            {
                _container.anchoredPosition = new Vector2(Plugin.OverlayPositionX.Value, Plugin.OverlayPositionY.Value);
            }

            _skillBarTemplate = _skillBarsObject.transform.Find("SkillBar")?.gameObject;
            _dragArea = _skillBarsObject.transform.Find("DragArea")?.gameObject;
            _dragText = _dragArea?.transform.Find("DragText")?.GetComponent<TextMeshProUGUI>();

            if (_container == null || _skillBarTemplate == null || _dragArea == null || _dragText == null)
            {
                return;
            }

            if (!ConfigureText(_dragText))
            {
                return;
            }

            _dragHandler = _skillBarsObject.AddComponent<SkillBarDragHandler>();
            _dragHandler.DragEnded = OnDragEnded;
            _dragHandler.enabled = Plugin.EnableDragging.Value;

            _dragArea.SetActive(Plugin.EnableDragging.Value);
            _skillBarTemplate.SetActive(false);
        }

        private void OnEnableDraggingChanged(object sender, System.EventArgs eventArgs)
        {
            bool enableDragging = Plugin.EnableDragging.Value;

            if (_dragHandler != null)
            {
                _dragHandler.enabled = enableDragging;
            }

            if (_dragArea != null)
            {
                _dragArea.SetActive(enableDragging);
            }
        }

        private void OnDestroy()
        {
            if (Plugin.EnableDragging != null)
            {
                Plugin.EnableDragging.SettingChanged -= OnEnableDraggingChanged;
            }

            if (_canvasObject != null)
            {
                Destroy(_canvasObject);
                _canvasObject = null;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        internal void ShowSkill(Skills.SkillType skillType, Sprite icon, float level, float progress, float gain)
        {
            SkillBarViewModel skillBar;

            if (!_skillBars.TryGetValue(skillType, out skillBar))
            {
                skillBar = CreateSkillBar(skillType);

                if (skillBar == null)
                {
                    return;
                }

                _skillBars.Add(skillType, skillBar);
            }

            skillBar.SkillIcon.sprite = icon;
            skillBar.SkillIcon.enabled = icon != null;
            skillBar.LevelText.text = ((int)level).ToString();
            skillBar.SkillNameText.text = skillType.ToString();
            skillBar.GainText.text = $"+{gain:0.##}";

            skillBar.GainText.fontStyle = FontStyles.Bold;
            skillBar.GainText.rectTransform.localScale = Vector3.one * GainPulseScale;

            float clampedProgress = Mathf.Clamp01(progress);
            skillBar.ProgressFillRect.anchorMax = new Vector2(clampedProgress, 1f);

            skillBar.HideTime = Time.time + DisplayDuration;
            _gainPulseTimes[skillType] = Time.time;

            if (skillBar.IsVisible)
            {
                return;
            }

            skillBar.GameObject.SetActive(true);
            skillBar.IsVisible = true;

            _activeSkills.Add(skillType);
        }

        private SkillBarViewModel CreateSkillBar(Skills.SkillType skillType)
        {
            if (_skillBarTemplate == null || _container == null)
            {
                return null;
            }

            GameObject skillBarObject = Instantiate(_skillBarTemplate, _container, false);
            skillBarObject.name = $"SkillBar_{skillType}";

            RectTransform skillBarRect = skillBarObject.GetComponent<RectTransform>();
            Image skillIcon = skillBarObject.transform.Find("SkillIcon")?.GetComponent<Image>();
            Image progressFill = skillBarObject.transform.Find("Background/Fill")?.GetComponent<Image>();
            TextMeshProUGUI levelText = skillBarObject.transform.Find("Background/Level")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI skillNameText = skillBarObject.transform.Find("Background/SkillName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI gainText = skillBarObject.transform.Find("Gain")?.GetComponent<TextMeshProUGUI>();

            if (skillBarRect == null || skillIcon == null || progressFill == null || levelText == null || skillNameText == null || gainText == null)
            {
                Plugin.Log.LogError($"SkillBar prefab hierarchy is invalid for {skillType}.");
                Destroy(skillBarObject);
                return null;
            }

            if (!ConfigureText(levelText) || !ConfigureText(skillNameText) || !ConfigureText(gainText))
            {
                Destroy(skillBarObject);
                return null;
            }

            skillBarObject.SetActive(false);

            return new SkillBarViewModel(
                skillBarObject,
                skillBarRect,
                skillIcon,
                progressFill,
                progressFill.rectTransform,
                levelText,
                skillNameText,
                gainText
            );
        }

        private void UpdateGainPulse(Skills.SkillType skillType, SkillBarViewModel skillBar)
        {
            float pulseStartTime;

            if (!_gainPulseTimes.TryGetValue(skillType, out pulseStartTime))
            {
                return;
            }

            float elapsed = Time.time - pulseStartTime;

            if (elapsed >= GainPulseDuration)
            {
                skillBar.GainText.rectTransform.localScale = Vector3.one;
                _gainPulseTimes.Remove(skillType);
                return;
            }

            float progress = elapsed / GainPulseDuration;
            float scale = Mathf.Lerp(GainPulseScale, 1f, progress);
            skillBar.GainText.rectTransform.localScale = Vector3.one * scale;
        }

        private static bool ConfigureText(TextMeshProUGUI text)
        {
            if (Hud.instance == null || Hud.instance.m_healthText == null)
            {
                return false;
            }

            text.font = Hud.instance.m_healthText.font;
            text.fontSharedMaterial = Hud.instance.m_healthText.fontSharedMaterial;
            text.raycastTarget = false;

            return true;
        }

        private static void OnDragEnded(Vector2 position)
        {
            Plugin.OverlayPositionX.Value = position.x;
            Plugin.OverlayPositionY.Value = position.y;
        }
    }
}