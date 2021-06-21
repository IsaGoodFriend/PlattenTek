using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using PlattekMod.Utils;

namespace PlattekMod.EverestInterop {
    public class PlattenTekSettings : EverestModuleSettings {

        [SettingNeedsRelaunch] public bool LaunchStudioAtBoot { get; set; } = false;

        public bool Mod9DLighting { get; set; } = false;

        [SettingIgnore] public DateTime StudioLastModifiedTime { get; set; } = new();
        public bool AutoExtractNewStudio { get; set; } = true;

        public bool RestoreSettings { get; set; } = false;
        [SettingIgnore] public bool FirstLaunch { get; set; } = true;

        #region Info HUD

        public bool InfoHud { get; set; } = false;
        public bool InfoGame { get; set; } = true;
        public bool InfoTasInput { get; set; } = true;
        public bool InfoSubPixelIndicator { get; set; } = true;
        public bool InfoCustom { get; set; } = false;
        public bool InfoIgnoreTriggerWhenClickEntity { get; set; } = true;

        [SettingIgnore]
        public string InfoCustomTemplate { get; set; } =
            "Wind: {Level.Wind}\n" +
            "AutoJump: {Player.AutoJump} ({Player.AutoJumpTimer.toFrame()})\n" +
            "ForceMoveX: {Player.forceMoveX} ({Player.forceMoveXTimer.toFrame()})\n" +
            "Theo: {TheoCrystal.ExactPosition}\n" +
            "TheoCantGrab: {TheoCrystal.Hold.cannotHoldTimer.toFrame()}";

        [SettingIgnore] public Vector2 InfoPosition { get; set; } = Vector2.Zero;
        [SettingIgnore] public int InfoTextSize { get; set; } = 10;
        [SettingIgnore] public int InfoSubPixelIndicatorSize { get; set; } = 10;
        [SettingIgnore] public int InfoOpacity { get; set; } = 6;
        [SettingIgnore] public int InfoMaskedOpacity { get; set; } = 3;

        #endregion

    }
}