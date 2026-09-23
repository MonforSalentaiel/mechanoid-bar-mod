using System;
using UnityEngine;
using Verse;

namespace MechanoidBar
{
    public class MechanoidBarSettings : ModSettings
    {
        #region CONSTANTS
        public const float Default_OffsetX = 0f;
        public const float Default_OffsetY = -10f;
        public const float Default_BaseScale = 0.75f;
        public const int Default_MechsPerRow = 12;
        public const int Default_MaxNumberOfRows = 3;
        public const float Default_MarginX = 4f;
        public const float Default_MarginY = 4f;
        public const bool Default_HideBackground = false;
        public const bool Default_ShowHealthBar = true;
        #endregion

        #region PROPERTIES
        private float _offsetX = Default_OffsetX;
        public float OffsetX { get => _offsetX; set => Util.SetValue(ref _offsetX, value, v => ApplyChanges()); }

        private float _offsetY = Default_OffsetY;
        public float OffsetY { get => _offsetY; set => Util.SetValue(ref _offsetY, value, v => ApplyChanges()); }

        private float _baseScale = Default_BaseScale;
        public float BaseScale { get => _baseScale; set => Util.SetValue(ref _baseScale, value, v => ApplyChanges()); }

        private int _mechsPerRow = Default_MechsPerRow;
        public int MechsPerRow { get => _mechsPerRow; set => Util.SetValue(ref _mechsPerRow, value, v => ApplyChanges()); }

        private int _maxNumberOfRows = Default_MaxNumberOfRows;
        public int MaxNumberOfRows { get => _maxNumberOfRows; set => Util.SetValue(ref _maxNumberOfRows, value, v => ApplyChanges()); }

        private float _marginX = Default_MarginX;
        public float MarginX { get => _marginX; set => Util.SetValue(ref _marginX, value, v => ApplyChanges()); }

        private float _marginY = Default_MarginY;
        public float MarginY { get => _marginY; set => Util.SetValue(ref _marginY, value, v => ApplyChanges()); }

        private bool _hideBackground = Default_HideBackground;
        public bool HideBackground { get => _hideBackground; set => Util.SetValue(ref _hideBackground, value, v => ApplyChanges()); }

        private bool _showHealthBar = Default_ShowHealthBar;
        public bool ShowHealthBar { get => _showHealthBar; set => Util.SetValue(ref _showHealthBar, value, v => ApplyChanges()); }

        private bool _isBarHidden = false;
        public bool IsBarHidden { get => _isBarHidden; set => _isBarHidden = value; }
        #endregion

        #region PUBLIC METHODS
        public void DoSettingsWindowContents(Rect inRect)
        {
            var width = inRect.width;
            var offsetY = 0.0f;

            ControlsBuilder.Begin(inRect);
            try
            {
                OffsetX = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.OffsetX".Translate(), "MB.OffsetXDesc".Translate(), OffsetX, Default_OffsetX, nameof(OffsetX), float.MinValue);
                OffsetY = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.OffsetY".Translate(), "MB.OffsetYDesc".Translate(), OffsetY, Default_OffsetY, nameof(OffsetY), float.MinValue);
                BaseScale = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.BaseScale".Translate(), "MB.BaseScaleDesc".Translate(), BaseScale, Default_BaseScale, nameof(BaseScale), 0.2f, 3f);
                MechsPerRow = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.MechsPerRow".Translate(), "MB.MechsPerRowDesc".Translate(), MechsPerRow, Default_MechsPerRow, nameof(MechsPerRow), 1, 50);
                MaxNumberOfRows = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.MaxNumOfRows".Translate(), "MB.MaxNumOfRowsDesc".Translate(), MaxNumberOfRows, Default_MaxNumberOfRows, nameof(MaxNumberOfRows), 1, 10);
                MarginX = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.MarginX".Translate(), "MB.MarginXDesc".Translate(), MarginX, Default_MarginX, nameof(MarginX), 0f, 50f);
                MarginY = ControlsBuilder.CreateNumeric(ref offsetY, width, "MB.MarginY".Translate(), "MB.MarginYDesc".Translate(), MarginY, Default_MarginY, nameof(MarginY), 0f, 50f);
                HideBackground = ControlsBuilder.CreateCheckbox(ref offsetY, width, "MB.HideBackground".Translate(), "MB.HideBackgroundDesc".Translate(), HideBackground, Default_HideBackground);
                ShowHealthBar = ControlsBuilder.CreateCheckbox(ref offsetY, width, "MB.ShowHealthBar".Translate(), "MB.ShowHealthBarDesc".Translate(), ShowHealthBar, Default_ShowHealthBar);
            }
            finally
            {
                ControlsBuilder.End(offsetY);
            }
        }
        #endregion

        #region OVERRIDES
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _offsetX, nameof(OffsetX), Default_OffsetX);
            Scribe_Values.Look(ref _offsetY, nameof(OffsetY), Default_OffsetY);
            Scribe_Values.Look(ref _baseScale, nameof(BaseScale), Default_BaseScale);
            Scribe_Values.Look(ref _mechsPerRow, nameof(MechsPerRow), Default_MechsPerRow);
            Scribe_Values.Look(ref _maxNumberOfRows, nameof(MaxNumberOfRows), Default_MaxNumberOfRows);
            Scribe_Values.Look(ref _marginX, nameof(MarginX), Default_MarginX);
            Scribe_Values.Look(ref _marginY, nameof(MarginY), Default_MarginY);
            Scribe_Values.Look(ref _hideBackground, nameof(HideBackground), Default_HideBackground);
            Scribe_Values.Look(ref _showHealthBar, nameof(ShowHealthBar), Default_ShowHealthBar);
            Scribe_Values.Look(ref _isBarHidden, nameof(IsBarHidden), false);
            ApplyChanges();
        }
        #endregion

        #region PRIVATE METHODS
        private void ApplyChanges()
        {
            if (!GenScene.InPlayScene) return;
            MechanoidBarCore.Instance?.MarkDirty();
        }
        #endregion
    }
}