using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidBar
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("syrus.mechanoidbar");
             
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_OnGUI_Postfix)));
                 
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.MarkColonistsDirty)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_MarkDirty_Postfix)));
                 
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.CheckRecacheEntries)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_CheckRecacheEntries_Postfix)));
                
            Log.Message("[MechanoidBar] Harmony patches applied successfully.");
             
            var core = new MechanoidBarCore();
        }

        static void ColonistBar_OnGUI_Postfix()
        {
            MechanoidBarCore.Instance?.MechanoidBarOnGUI();
        }

        static void ColonistBar_MarkDirty_Postfix()
        {
            MechanoidBarCore.Instance?.MarkDirty();
        }

        static void ColonistBar_CheckRecacheEntries_Postfix()
        {
            MechanoidBarCore.Instance?.CheckRecacheEntries();
        }
    }
}