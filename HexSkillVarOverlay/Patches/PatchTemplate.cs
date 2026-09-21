using HarmonyLib;
using System;

namespace HexSkillBarOverlay.Patches
{
    [HarmonyPatch(typeof(Object), nameof(Object.ToString))]
    internal static class PatchTemplate
    {
        private static void Prefix()
        {
            throw new NotImplementedException();
        }

        private static void Postfix()
        {
            throw new NotImplementedException();
        }
    }
}
