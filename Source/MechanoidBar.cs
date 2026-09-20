using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidBar
{
    public class MechanoidBar : Mod
    {
        public static MechanoidBar Instance { get; private set; }
        public static MechanoidBarSettings Settings { get; private set; }

        public static float MarginX => Settings.MarginX;
        public static float MarginY => Settings.MarginY;
        public static float OffsetX => Settings.OffsetX;
        public static float OffsetY => Settings.OffsetY;
        public static float BaseScale => Settings.BaseScale;
        public static bool HideBackground => Settings.HideBackground;
        public static bool ShowHealthBar => Settings.ShowHealthBar;

        public MechanoidBar(ModContentPack content) : base(content)
        {
            Instance = this;
            LongEventHandler.ExecuteWhenFinished(Initialize);
        }

        private void Initialize()
        {
            Settings = GetSettings<MechanoidBarSettings>();
            Log.Message("[MechanoidBar] Mod initialized successfully.");
        }

        public override string SettingsCategory() => "Mechanoid Bar";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}