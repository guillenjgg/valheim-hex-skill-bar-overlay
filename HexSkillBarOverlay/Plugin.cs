using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HexSkillBarOverlay.UI;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HexSkillBarOverlay
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.skillbaroverlay";
        private const string PluginName = "HexSkillBarOverlay";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;
        private GameObject _controllerObject;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static readonly AccessTools.FieldRef<Skills, Player> SkillsPlayerField = AccessTools.FieldRefAccess<Skills, Player>("m_player");
        internal static readonly AccessTools.FieldRef<Skills.Skill, Skills.SkillDef> SkillInfoField = AccessTools.FieldRefAccess<Skills.Skill, Skills.SkillDef>("m_info");
        internal static readonly AccessTools.FieldRef<Skills.Skill, float> SkillLevelField = AccessTools.FieldRefAccess<Skills.Skill, float>("m_level");
        internal static readonly AccessTools.FieldRef<Skills.SkillDef, Sprite> SkillIconField = AccessTools.FieldRefAccess<Skills.SkillDef, Sprite>("m_icon");
        internal static readonly AccessTools.FieldRef<Skills.SkillDef, float> SkillIncreaseStepField = AccessTools.FieldRefAccess<Skills.SkillDef, float>("m_increseStep");

        internal static readonly FastInvokeHandler SkillsGetSkillMethod = MethodInvoker.GetHandler(AccessTools.Method(typeof(Skills), "GetSkill"));
        internal static readonly FastInvokeHandler SkillGetLevelPercentageMethod = MethodInvoker.GetHandler(AccessTools.Method(typeof(Skills.Skill), "GetLevelPercentage"));

        internal static AssetBundle Bundle;
        internal static GameObject SkillBarPrefab;

        internal static ConfigEntry<bool> EnableDragging { get; private set; }
        internal static ConfigEntry<float> OverlayPositionX { get; private set; }
        internal static ConfigEntry<float> OverlayPositionY { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            EnableDragging = Config.Bind(
                "General",
                "Enable Dragging",
                false,
                "Allows the skill bar overlay to be repositioned by dragging it. The game must be paused while moving the overlay."
            );

            OverlayPositionX = Config.Bind(
                "General",
                "Overlay Position X",
                40f,
                "Saved horizontal position of the skill bar overlay."
            );

            OverlayPositionY = Config.Bind(
                "General",
                "Overlay Position Y",
                0f,
                "Saved vertical position of the skill bar overlay."
            );

            Bundle = GetAssetBundleFromResources("skill-bar-overlay");

            if (Bundle == null)
            {
                Log.LogError("Failed to load skill bar AssetBundle.");
                return;
            }

            SkillBarPrefab = Bundle.LoadAsset<GameObject>("SkillBars");

            if (SkillBarPrefab == null)
            {
                Log.LogError("Failed to load SkillBars prefab from AssetBundle.");
                return;
            }

            _controllerObject = new GameObject("HexSkillBarOverlayController");
            DontDestroyOnLoad(_controllerObject);

            _controllerObject.AddComponent<SkillBarPrefabController>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);
        }

        private void OnDestroy()
        {
            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;

            if (_controllerObject != null)
            {
                Destroy(_controllerObject);
                _controllerObject = null;
            }

            Instance = null;
            Log = null;
        }

        private static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = execAssembly.GetManifestResourceNames();
            string matchingResourceName = resourceNames.SingleOrDefault(resourceName => resourceName.EndsWith(fileName));

            if (matchingResourceName == null)
            {
                Log.LogError($"Could not find embedded resource ending with '{fileName}'.");
                return null;
            }

            using (Stream stream = execAssembly.GetManifestResourceStream(matchingResourceName))
            {
                if (stream == null)
                {
                    Log.LogError($"Failed to open embedded resource stream: {matchingResourceName}");
                    return null;
                }

                return AssetBundle.LoadFromStream(stream);
            }
        }
    }
}