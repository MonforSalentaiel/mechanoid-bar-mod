using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

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
            if (!entriesDirty) return;
            entriesDirty = false;
            Entries.Clear();

            if (Find.CurrentMap == null) return;

            var mechs = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                                .Where(p => p.RaceProps.IsMechanoid);

            foreach (var mech in mechs)
            {
                Entries.Add(new MechanoidBarEntry { pawn = mech, group = 0 });
            }
        }

        public void MechanoidBarOnGUI()
        {
            CheckRecacheEntries();

            // Если механоидов нет, не рисуем ничего
            if (Entries.Count == 0) return;

            // Сначала рисуем кнопку скрытия/раскрытия
            DrawToggleButton();

            // Если панель скрыта в настройках (или кнопкой), не рисуем её
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

            for (int i = 0; i < Entries.Count; i++)
            {
                int row = i / maxPerRow;
                int col = i % maxPerRow;

                float x = posX + marginX + col * (iconSize + marginX);
                float y = posY + marginY + row * (iconSize + marginY);

                Rect iconRect = new Rect(x, y, iconSize, iconSize);

                DrawMechanoid(Entries[i], iconRect);
                HandleInteraction(Entries[i], iconRect);
            }
        }

        private void DrawToggleButton()
        {
            // Если механоидов нет, кнопку тоже не рисуем
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

            float buttonSize = 24f; // Стандартный размер нативных кнопок UI

            // Кнопка всегда располагается справа от панели, независимо от того, скрыта панель или нет
            Rect buttonRect = new Rect(posX + barWidth + 4f, posY + barHeight / 2f - buttonSize / 2f, buttonSize, buttonSize);

            // Используем нативную текстуру RimWorld (стрелочка вниз)
            Texture2D icon = TexButton.Collapse;
             
            bool clicked;
            if (MechanoidBar.Settings.IsBarHidden)
            {
                // Если панель скрыта, поворачиваем стрелочку на 180 градусов (чтобы она указывала вверх)
                Matrix4x4 matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(180f, buttonRect.center);
                clicked = Widgets.ButtonImage(buttonRect, icon, Color.white, GenUI.MouseoverColor);
                GUI.matrix = matrix; // Возвращаем матрицу в исходное состояние
            }
            else
            {
                // Если панель видима, рисуем стандартную стрелочку вниз
                clicked = Widgets.ButtonImage(buttonRect, icon, Color.white, GenUI.MouseoverColor);
            }

            if (clicked)
            {
                MechanoidBar.Settings.IsBarHidden = !MechanoidBar.Settings.IsBarHidden;
                Event.current.Use();
            }

            // Нативный тултип
            TooltipHandler.TipRegion(buttonRect, MechanoidBar.Settings.IsBarHidden ? "Show Mechanoid Bar" : "Hide Mechanoid Bar");
        }

        private void DrawMechanoid(MechanoidBarEntry entry, Rect rect)
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

            if (Find.Selector.IsSelected(pawn))
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
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    Find.CameraDriver.JumpToCurrentMapLoc(pawn.Position);
                    Event.current.Use();
                }
            }
        }
    }
}