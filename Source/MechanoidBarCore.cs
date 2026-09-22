using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MechanoidBar
{
    public class MechanoidBarCore
    {
        public static MechanoidBarCore Instance { get; private set; }
        public List<MechanoidBarEntry> Entries = new List<MechanoidBarEntry>();
        public bool entriesDirty = true;

        private const float BaseIconSize = 48f;

        public MechanoidBarCore()
        {
            Instance = this;
            Log.Message("[MechanoidBar] Core system started.");
        }

        public void MarkDirty() => entriesDirty = true;

        public void CheckRecacheEntries()
        {
            // 1. Безопасная проверка каждый кадр: обновлялся ли список пешек
            if (Find.CurrentMap == null) return;

            // mapPawns.SpawnedPawnsInFaction возвращает кэшированный список (без выделения памяти)
            List<Pawn> playerPawns = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);

            int currentMechCount = 0;
            for (int i = 0; i < playerPawns.Count; i++)
            {
                if (playerPawns[i].RaceProps.IsMechanoid)
                    currentMechCount++;
            }

            // Если количество механоидов не изменилось с прошлого кадра, сбрасываем флаг и выходим
            if (Entries.Count == currentMechCount)
            {
                entriesDirty = false;
                return;
            }

            // 2. Если количество изменилось, пересоздаем список (без LINQ, чтобы не мусорить в памяти)
            entriesDirty = false;
            Entries.Clear();

            for (int i = 0; i < playerPawns.Count; i++)
            {
                Pawn p = playerPawns[i];
                if (p.RaceProps.IsMechanoid)
                {
                    Entries.Add(new MechanoidBarEntry { pawn = p, group = 0 });
                }
            }

            // Debug лог только при реальном изменении
            // Log.Message($"[MechanoidBar] Recached entries. Mechanoids found: {Entries.Count}");
        }

        public void MechanoidBarOnGUI()
        {
            CheckRecacheEntries();

            if (Entries.Count == 0) return;

            DrawToggleButton();

            if (MechanoidBar.Settings.IsBarHidden) return;

            float scale = MechanoidBar.Settings.BaseScale;
            int maxPerRow = MechanoidBar.Settings.MechsPerRow;
            float iconSize = BaseIconSize * scale;
            float marginX = MechanoidBar.Settings.MarginX * scale;
            float marginY = MechanoidBar.Settings.MarginY * scale;

            int rows = Mathf.CeilToInt((float)Entries.Count / maxPerRow);
            if (rows > MechanoidBar.Settings.MaxNumberOfRows) rows = MechanoidBar.Settings.MaxNumberOfRows;

            float barWidth = Mathf.Min(Entries.Count, maxPerRow) * (iconSize + marginX) + marginX;
            float barHeight = rows * (iconSize + marginY) + marginY;

            float posX = (UI.screenWidth / 2f) - (barWidth / 2f) + MechanoidBar.Settings.OffsetX;
            float posY = UI.screenHeight - 70f - barHeight + MechanoidBar.Settings.OffsetY;

            Rect bgRect = new Rect(posX, posY, barWidth, barHeight);

            if (!MechanoidBar.Settings.HideBackground)
            {
                Widgets.DrawBoxSolid(bgRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
                Color prevColor = GUI.color;
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                Widgets.DrawBox(bgRect, 1);
                GUI.color = prevColor;
            }

            // Кэшируем выделение, чтобы не вызывать его дважды для каждой пешки
            List<object> selectedObjects = Find.Selector.SelectedObjects;

            for (int i = 0; i < Entries.Count; i++)
            {
                int row = i / maxPerRow;
                int col = i % maxPerRow;

                float x = posX + marginX + col * (iconSize + marginX);
                float y = posY + marginY + row * (iconSize + marginY);

                Rect iconRect = new Rect(x, y, iconSize, iconSize);

                DrawMechanoid(Entries[i], iconRect, selectedObjects);
                HandleInteraction(Entries[i], iconRect);
            }
        }

        private void DrawToggleButton()
        {
            if (Entries.Count == 0) return;

            float scale = MechanoidBar.Settings.BaseScale;
            int maxPerRow = MechanoidBar.Settings.MechsPerRow;
            float iconSize = BaseIconSize * scale;
            float marginX = MechanoidBar.Settings.MarginX * scale;
            float marginY = MechanoidBar.Settings.MarginY * scale;

            int rows = Mathf.CeilToInt((float)Entries.Count / maxPerRow);
            if (rows > MechanoidBar.Settings.MaxNumberOfRows) rows = MechanoidBar.Settings.MaxNumberOfRows;

            float barWidth = Mathf.Min(Entries.Count, maxPerRow) * (iconSize + marginX) + marginX;
            float barHeight = rows * (iconSize + marginY) + marginY;

            float posX = (UI.screenWidth / 2f) - (barWidth / 2f) + MechanoidBar.Settings.OffsetX;
            float posY = UI.screenHeight - 70f - barHeight + MechanoidBar.Settings.OffsetY;

            float buttonSize = 24f;
            Rect buttonRect = new Rect(posX - 20f, posY - buttonSize / 2f, buttonSize, buttonSize);

            TooltipHandler.TipRegion(buttonRect, MechanoidBar.Settings.IsBarHidden ? "Show Mechanoid Bar" : "Hide Mechanoid Bar");

            bool mouseOver = Mouse.IsOver(buttonRect);
            if (mouseOver)
            {
                Widgets.DrawHighlight(buttonRect);
            }

            Texture2D icon = TexButton.Collapse;
            Color prevColor = GUI.color;
            GUI.color = mouseOver ? GenUI.MouseoverColor : Color.white;

            if (MechanoidBar.Settings.IsBarHidden)
            {
                GUI.DrawTexture(new Rect(buttonRect.x, buttonRect.yMax, buttonRect.width, -buttonRect.height), icon);
            }
            else
            {
                GUI.DrawTexture(buttonRect, icon);
            }
            GUI.color = prevColor;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && mouseOver)
            {
                MechanoidBar.Settings.IsBarHidden = !MechanoidBar.Settings.IsBarHidden;
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                Event.current.Use();
            }
        }

        private void DrawMechanoid(MechanoidBarEntry entry, Rect rect, List<object> selectedObjects)
        {
            Pawn pawn = entry.pawn;

            Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 1f));

            RenderTexture portrait = PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height), Rot4.South);
            if (portrait != null)
            {
                GUI.DrawTexture(rect, portrait);
            }

            if (MechanoidBar.Settings.ShowHealthBar && pawn.health != null && pawn.health.summaryHealth != null)
            {
                float healthPct = pawn.health.summaryHealth.SummaryHealthPercent;
                Rect healthRect = new Rect(rect.x, rect.yMax - 5f, rect.width * healthPct, 5f);
                Color healthColor = healthPct < 0.3f ? Color.red : (healthPct < 0.7f ? Color.yellow : Color.green);
                Widgets.DrawBoxSolid(healthRect, healthColor);
            }

            bool isDrafted = pawn.drafter != null && pawn.Drafted;
            bool isFightMode = false;

            var controlGroup = pawn.GetMechControlGroup();
            if (controlGroup != null && pawn.Drafted)
            {
                isFightMode = true;
            }

            if (isDrafted || isFightMode)
            {
                Color prevColor = GUI.color;
                GUI.color = isDrafted ? new Color(0.5f, 1f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);
                Widgets.DrawBox(rect, 2);
                GUI.color = prevColor;
            }

            // Быстрая проверка выделения без вызова Find.Selector.IsSelected каждый кадр
            bool isSelected = selectedObjects.Contains(pawn);
            if (isSelected)
            {
                Color prevColor = GUI.color;
                GUI.color = Color.cyan;
                Widgets.DrawBox(rect, 2);
                GUI.color = prevColor;
            }
        }

        private void HandleInteraction(MechanoidBarEntry entry, Rect rect)
        {
            Pawn pawn = entry.pawn;

            if (Mouse.IsOver(rect))
            {
                Color prevColor = GUI.color;
                GUI.color = Color.yellow;
                Widgets.DrawBox(rect, 1);
                GUI.color = prevColor;

                TooltipHandler.TipRegion(rect, pawn.LabelCap);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                    if (additive)
                    {
                        if (Find.Selector.IsSelected(pawn))
                            Find.Selector.Deselect(pawn);
                        else
                            Find.Selector.Select(pawn, true, false);
                    }
                    else
                    {
                        Find.Selector.ClearSelection();
                        Find.Selector.Select(pawn, false, false);
                    }

                    if (Event.current.clickCount == 2)
                    {
                        Find.CameraDriver.JumpToCurrentMapLoc(pawn.Position);
                    }

                    Event.current.Use();
                }
            }
        }
    }
}