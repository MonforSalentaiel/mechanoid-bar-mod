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
            
            // 1. Патч на отрисовку UI
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_OnGUI_Postfix)));
                
            // 2. Патч на пометку "грязным" (когда пешки меняются)
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.MarkColonistsDirty)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_MarkDirty_Postfix)));
                
            // 3. Патч на пересчёт списка пешек
            harmony.Patch(
                AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.CheckRecacheEntries)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistBar_CheckRecacheEntries_Postfix)));
                
            Log.Message("[MechanoidBar] Harmony patches applied successfully.");
            
            // Инициализация ядра
            var core = new MechanoidBarCore();
        }

        // --- POSTFIX МЕТОДЫ ---

        static void ColonistBar_OnGUI_Postfix()
        {
            // Вызываем отрисовку панели механоидов сразу после панели колонистов
            MechanoidBarCore.Instance?.MechanoidBarOnGUI();
        }

        static void ColonistBar_MarkDirty_Postfix()
        {
            // Если игра пометила панель колонистов на обновление, делаем то же для механоидов
            MechanoidBarCore.Instance?.MarkDirty();
        }

        static void ColonistBar_CheckRecacheEntries_Postfix()
        {
            // Если игра пересчитала колонистов, мы тоже пересчитываем механоидов
            MechanoidBarCore.Instance?.CheckRecacheEntries();
        }
    }
}