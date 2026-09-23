using HarmonyLib;

namespace HexSkillBarOverlay.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    internal static class PlayerSetLocalPlayerPatch
    {
        private static void Postfix()
        {
            UI.SkillBarPrefabController.Instance?.CreateUi();
        }
    }
}