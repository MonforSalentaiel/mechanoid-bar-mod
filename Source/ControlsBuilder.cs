using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MechanoidBar
{
    public static class ControlsBuilder
    {
        #region DELEGATE
        public delegate string ConvertDelegate<T>(T value);
        #endregion

        #region CONSTANTS
        public const float SettingsRowHeight = 32f;
        public const float SettingsRowMargin = SettingsRowHeight * 0.25f;
        private const float SettingsScrollbarWidth = 16f;
        #endregion

        #region FIELDS
        private static readonly Dictionary<string, object> ValueBuffers = new Dictionary<string, object>();
        private static readonly Color ModifiedColor = Color.cyan;
        private static GameFont OriTextFont;
        private static TextAnchor OriTextAnchor;
        private static Color OriColor;
        private static float SettingsViewHeight = 0f;
        private static Vector2 SettingsScrollPosition = new Vector2();
        #endregion

        #region PUBLIC METHODS
        public static float Begin(Rect inRect)
        {
            OriTextFont = Text.Font;
            OriTextAnchor = Text.Anchor;
            OriColor = GUI.color;
            Text.Anchor = TextAnchor.MiddleLeft;
            var viewWidth = inRect.width - SettingsScrollbarWidth;
            GUI.BeginGroup(inRect);
            Widgets.BeginScrollView(new Rect(0, 0, inRect.width, inRect.height), ref SettingsScrollPosition, new Rect(0, 0, viewWidth, SettingsViewHeight));
            return viewWidth;
        }

        public static void End(float offsetY)
        {
            Widgets.EndScrollView();
            GUI.EndGroup();
            Text.Font = OriTextFont;
            Text.Anchor = OriTextAnchor;
            GUI.color = OriColor;
            SettingsViewHeight = offsetY + SettingsRowHeight;
        }

        public static T CreateNumeric<T>(ref float offsetY, float viewWidth, string label, string tooltip, T value, T defaultValue, string valueBufferKey, float min = 0f, float max = 1e+9f, ConvertDelegate<T> additionalText = null, string unit = null) where T : struct, IComparable
        {
            var isModified = !value.Equals(defaultValue);
            var controlWidth = GetControlWidth(viewWidth);
            if (isModified) GUI.color = ModifiedColor;
            Widgets.Label(new Rect(0, offsetY, controlWidth - 8, SettingsRowHeight), label);
            GUI.color = OriColor;

            var textFieldRect = new Rect(controlWidth + 2, offsetY + 6, controlWidth - 4, SettingsRowHeight - 12);
            var valueBuffer = GetValueBuffer(valueBufferKey, value);
            Widgets.TextFieldNumeric(textFieldRect, ref value, ref valueBuffer.Buffer, min, max);
            if (!string.IsNullOrWhiteSpace(tooltip)) DrawTooltip(textFieldRect, tooltip);
            DrawTextFieldUnit(textFieldRect, unit);

            if (additionalText != null)
            {
                var additionalTextRect = textFieldRect;
                additionalTextRect.x += textFieldRect.width + 8;
                additionalTextRect.width -= 8;
                Widgets.Label(additionalTextRect, additionalText(value));
            }

            if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
            {
                value = defaultValue;
                ValueBuffers.Remove(valueBufferKey);
            }
            offsetY += SettingsRowHeight;
            return value;
        }

        public static bool CreateCheckbox(ref float offsetY, float viewWidth, string label, string tooltip, bool value, bool defaultValue, string text = null)
        {
            var isModified = value != defaultValue;
            var controlWidth = GetControlWidth(viewWidth);
            if (isModified) GUI.color = ModifiedColor;
            Widgets.Label(new Rect(0, offsetY, controlWidth, SettingsRowHeight), label);
            GUI.color = OriColor;

            var checkboxSize = SettingsRowHeight - 8;
            Widgets.Checkbox(controlWidth, offsetY + (SettingsRowHeight - checkboxSize) / 2, ref value, checkboxSize);
            DrawTooltip(new Rect(controlWidth, offsetY, checkboxSize, checkboxSize), tooltip);

            if (text != null) Widgets.Label(new Rect(controlWidth + checkboxSize + 4, offsetY + 4, controlWidth - checkboxSize - 6, SettingsRowHeight - 8), text ?? "");
            if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString())) value = defaultValue;
            offsetY += SettingsRowHeight;
            return value;
        }

        public static bool DrawResetButton(float offsetY, float viewWidth, string tooltip)
        {
            var buttonRect = new Rect(viewWidth / 3 * 2 + 4, offsetY + 2, SettingsRowHeight * 2 - 4, SettingsRowHeight - 4);
            DrawTooltip(buttonRect, "Reset to " + tooltip);
            return Widgets.ButtonText(buttonRect, "Reset");
        }

        public static void DrawTooltip(Rect rect, string tooltip)
        {
            if (Mouse.IsOver(rect))
            {
                ActiveTip activeTip = new ActiveTip(tooltip);
                activeTip.DrawTooltip(GenUI.GetMouseAttachedWindowPos(activeTip.TipRect.width, activeTip.TipRect.height) + (UI.MousePositionOnUIInverted - Event.current.mousePosition));
            }
        }

        public static void DrawTextFieldUnit(Rect rect, string text)
        {
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(new Rect(rect.x + 4, rect.y + 1, rect.width - 8, rect.height), text);
            Text.Anchor = TextAnchor.MiddleLeft;
        }

        public static float GetControlWidth(float viewWidth) => viewWidth / 3 - 4;
        #endregion

        #region PRIVATE METHODS
        private static ValueBuffer<T> GetValueBuffer<T>(string key, T value) where T : struct, IComparable
        {
            if (ValueBuffers.TryGetValue(key, out var obj))
            {
                if (obj is ValueBuffer<T> foundVB)
                {
                    if (foundVB.Value.Equals(value) != true) foundVB.Buffer = null;
                    foundVB.Value = value;
                    return foundVB;
                }
                Log.ErrorOnce($"'{key}' found but not of correct type: expected '{typeof(ValueBuffer<T>)}', found '{obj?.GetType()}'", key.GetHashCode());
            }
            var newVB = new ValueBuffer<T>(value);
            ValueBuffers[key] = newVB;
            return newVB;
        }
        #endregion

        #region CLASSES
        private class ValueBuffer<T> where T : struct, IComparable
        {
            public string Buffer = null;
            public T Value;
            public ValueBuffer(T value) { Value = value; }
        }
        #endregion
    }

    public static class Util
    {
        public static void SetValue<T>(ref T storage, T value, Action<T> action = null) where T : IComparable
        {
            if (storage == null && value == null || storage?.Equals(value) == true) return;
            storage = value;
            action?.Invoke(value);
        }
    }
}