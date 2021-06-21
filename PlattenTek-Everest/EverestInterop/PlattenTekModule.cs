using System;
using System.IO;
using System.IO.Pipes;
using Celeste;
using Celeste.Mod;
using FMOD.Studio;
using PlattekMod.Communication;
using PlattekMod.Utils;

namespace PlattekMod.EverestInterop {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlattenTekModule : EverestModule {
        public static PlattenTekModule Instance;

        public PlattenTekModule() {
            Instance = this;
            AttributeUtils.CollectMethods<LoadAttribute>();
            AttributeUtils.CollectMethods<UnloadAttribute>();
        }

        public override Type SettingsType => typeof(PlattenTekSettings);
        public static PlattenTekSettings Settings => (PlattenTekSettings) Instance?._Settings;

        public override void Initialize() {
            StudioHelper.Initialize();
        }

        public override void Load() {

            ModCommunicationClient.Load();

            AttributeUtils.Invoke<LoadAttribute>();

            // Open memory mapped file for interfacing with Windows Celeste Studio
            if (ModCommunicationClient.Instance == null) {
                ModCommunicationClient.Run();
            }
        }

        public override void Unload() {
            ModCommunicationClient.Unload();

            ModCommunicationClient.Destroy();

            AttributeUtils.Invoke<UnloadAttribute>();
        }

        public override void LoadContent(bool firstLoad) {
            if (firstLoad) {
            }
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            CreateModMenuSectionHeader(menu, inGame, snapshot);
            Menu.CreateMenu(this, menu, inGame);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class LoadAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    internal class UnloadAttribute : Attribute { }
}