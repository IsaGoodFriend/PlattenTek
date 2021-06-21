using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Monocle;
using PlattekMod.Utils;

namespace PlattekMod.EverestInterop {
    internal static class Menu {
        private static readonly MethodInfo CreateKeyboardConfigUi = typeof(EverestModule).GetMethodInfo("CreateKeyboardConfigUI");
        private static List<TextMenu.Item> options;
        private static TextMenu.Item keyConfigButton;
        private static PlattenTekSettings Settings => PlattenTekModule.Settings;

        internal static string ToDialogText(this string input) => Dialog.Clean("TAS_" + input.Replace(" ", "_"));

        private static void CreateOptions(EverestModule everestModule, TextMenu menu, bool inGame) {
            options = new List<TextMenu.Item> {

                new TextMenuExt.SubMenu("Relaunch Required".ToDialogText(), false).Apply(subMenu => {
                    subMenu.Add(new TextMenu.OnOff("Launch Studio At Boot".ToDialogText(), Settings.LaunchStudioAtBoot).Change(value =>
                        Settings.LaunchStudioAtBoot = value));
                    subMenu.Add(new TextMenu.OnOff("Auto Extract New Studio".ToDialogText(), Settings.AutoExtractNewStudio).Change(value =>
                        Settings.AutoExtractNewStudio = value));

                }
                ).Apply(item => keyConfigButton = item)
            };
        }

        public static void CreateMenu(EverestModule everestModule, TextMenu menu, bool inGame) {
            
            CreateOptions(everestModule, menu, inGame);
            foreach (TextMenu.Item item in options) {
                menu.Add(item);
            }
        }

        private static IEnumerable<string> Split(string str, int n) {
            if (String.IsNullOrEmpty(str) || n < 1) {
                throw new ArgumentException();
            }

            for (int i = 0; i < str.Length; i += n) {
                yield return str.Substring(i, Math.Min(n, str.Length - i));
            }
        }

        public static IEnumerable<KeyValuePair<int?, string>> CreateSliderOptions(int start, int end, Func<int, string> formatter = null) {
            if (formatter == null) {
                formatter = i => i.ToString();
            }

            List<KeyValuePair<int?, string>> result = new();

            if (start <= end) {
                for (int current = start; current <= end; current++) {
                    result.Add(new KeyValuePair<int?, string>(current, formatter(current)));
                }

                result.Insert(0, new KeyValuePair<int?, string>(null, "Default".ToDialogText()));
            } else {
                for (int current = start; current >= end; current--) {
                    result.Add(new KeyValuePair<int?, string>(current, formatter(current)));
                }

                result.Insert(0, new KeyValuePair<int?, string>(null, "Default".ToDialogText()));
            }

            return result;
        }

        public static IEnumerable<KeyValuePair<bool, string>> CreateDefaultHideOptions() {
            return new List<KeyValuePair<bool, string>> {
                new(false, "Default".ToDialogText()),
                new(true, "Hide".ToDialogText()),
            };
        }

        public static IEnumerable<KeyValuePair<bool, string>> CreateSimplifyOptions() {
            return new List<KeyValuePair<bool, string>> {
                new(false, "Default".ToDialogText()),
                new(true, "Simplify".ToDialogText()),
            };
        }
    }
}