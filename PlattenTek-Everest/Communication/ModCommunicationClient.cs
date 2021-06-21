using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Input;
using Monocle;
using PlattekCommunication;
using PlattekMod.EverestInterop;
using PlattekMod.Utils;
using WinForms = System.Windows.Forms;

namespace PlattekMod.Communication {
    public sealed class ModCommunicationClient : StudioCommunicationBase {
        public static ModCommunicationClient Instance;

        private Thread thread;

        private ModCommunicationClient() { }
        private ModCommunicationClient(string target) : base(target) { }

        public static void Load() {
            Everest.Events.Level.OnEnter += Level_OnEnter;
            Everest.Events.Level.OnExit += Level_OnExit;
        }

        public static void Unload() {
            Everest.Events.Level.OnEnter -= Level_OnEnter;
            Everest.Events.Level.OnExit -= Level_OnExit;

        }

        private static void Level_OnEnter(Session session, bool fromSaveData) {
            if (Initialized) {
                Instance.SendLevelLoaded(session);
            }
        }

        private static void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            if (Initialized) {
                Instance.SendLevelUnloaded();
            }
        }

        public static bool Run() {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                return false;
            }

            Instance = new ModCommunicationClient();

#if DEBUG
            //SetupDebugVariables();
#endif

            RunThread.Start(Setup, "PlattenTekCom Client");

            void Setup() {
                Engine.Instance.Exiting -= Destroy;
                Engine.Instance.Exiting += Destroy;
                Instance.thread = Thread.CurrentThread;
                Instance.UpdateLoop();
            }

            return true;
        }

        public static void Destroy(object sender = null, EventArgs e = null) {
            if (Instance != null) {
                Instance.Abort = true;
                Instance.thread.Abort();
                Instance = null;
            }
        }

        /// <summary>
        /// Do not use outside of multiplayer mods. Allows more than 2 processes to communicate.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ModCommunicationClient RunExternal(string target) {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                return null;
            }

            var client = new ModCommunicationClient(target);

            RunThread.Start(Instance.UpdateLoop, "StudioCom Client_" + target);

            return client;
        }

        #region Read

        protected override void ReadData(Message message) {
            switch (message.Id) {
                case MessageIDs.EstablishConnection:
                    throw new NeedsResetException("Initialization data recieved in main loop");
                case MessageIDs.Wait:
                    ProcessWait();
                    break;
                case MessageIDs.GetModInfo:
                    ProcessGetModInfo();
                    break;
                case MessageIDs.SendPath:
                    ProcessSendPath(message.Data);
                    break;
                default:
                    if (ExternalReadHandler?.Invoke(message.Data) != true) {
                        throw new InvalidOperationException($"{message.Id}");
                    }

                    break;
            }
        }

        private void ProcessGetModInfo() {
            string command = "";

            Logger.Log("PlattenTek", "Sending Mod Info");

            if (Engine.Scene is Level level) {
                string MetaToString(EverestModuleMetadata metadata, int indentation = 0, bool comment = true) {
                    return (comment ? "# " : string.Empty) + string.Empty.PadLeft(indentation) + $"{metadata.Name} {metadata.VersionString}\n";
                }

                List<EverestModuleMetadata> metas = Everest.Modules
                    .Where(module => module.Metadata.Name != "UpdateChecker" && module.Metadata.Name != "DialogCutscene")
                    .Select(module => module.Metadata).ToList();
                metas.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

                AreaData areaData = AreaData.Get(level);
                string moduleName = string.Empty;
                EverestModuleMetadata mapMeta = null;
                if (Everest.Content.TryGet<AssetTypeMap>("Maps/" + areaData.SID, out ModAsset mapModAsset) && mapModAsset.Source != null) {
                    moduleName = mapModAsset.Source.Name;
                    mapMeta = metas.FirstOrDefault(meta => meta.Name == moduleName);
                }


                EverestModuleMetadata celesteMeta = metas.First(metadata => metadata.Name == "Celeste");
                EverestModuleMetadata everestMeta = metas.First(metadata => metadata.Name == "Everest");
                EverestModuleMetadata tasMeta = metas.First(metadata => metadata.Name == "PlattenTek");
                command += MetaToString(celesteMeta);
                command += MetaToString(everestMeta);
                command += MetaToString(tasMeta);
                metas.Remove(celesteMeta);
                metas.Remove(everestMeta);
                metas.Remove(tasMeta);

                EverestModuleMetadata speedrunToolMeta = metas.FirstOrDefault(metadata => metadata.Name == "SpeedrunTool");
                if (speedrunToolMeta != null) {
                    command += MetaToString(speedrunToolMeta);
                    metas.Remove(speedrunToolMeta);
                }

                command += "\n# Map:\n";
                if (mapMeta != null) {
                    command += MetaToString(mapMeta, 2);
                }

                string mode = level.Session.Area.Mode == AreaMode.Normal ? "ASide" : level.Session.Area.Mode.ToString();
                command += $"#   {areaData.SID} {mode}\n";

                if (!string.IsNullOrEmpty(moduleName) && mapMeta != null) {
                    List<EverestModuleMetadata> dependencies = mapMeta.Dependencies.Where(metadata =>
                        metadata.Name != "Celeste" && metadata.Name != "Everest" && metadata.Name != "UpdateChecker" &&
                        metadata.Name != "DialogCutscene" && metadata.Name != "PlattenTek").ToList();
                    dependencies.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                    if (dependencies.Count > 0) {
                        command += "\n# Dependencies:\n";
                        command += string.Join(string.Empty,
                            dependencies.Select(meta => metas.First(metadata => metadata.Name == meta.Name)).Select(meta => MetaToString(meta, 2)));
                    }

                    command += "\n# Other Installed Mods:\n";
                    command += string.Join(string.Empty,
                        metas.Where(meta => meta.Name != moduleName && dependencies.All(metadata => metadata.Name != meta.Name))
                            .Select(meta => MetaToString(meta, 2)));
                } else if (metas.IsNotEmpty()) {
                    command += "\n# Other Installed Mods:\n";
                    command += string.Join(string.Empty, metas.Select(meta => MetaToString(meta, 2)));
                }


            } else {
                command = "No Level";
            }

            byte[] commandBytes = Encoding.Default.GetBytes(command);
            WriteMessageGuaranteed(new Message(MessageIDs.ReturnModInfo, commandBytes));
        }

        private void ProcessSendPath(byte[] data) {
            string path = Encoding.Default.GetString(data);
            Log("ProcessSendPath: " + path);
        }

        private void ProcessToggleGameSetting(byte[] data) {
            string settingName = Encoding.Default.GetString(data);
            Log("Toggle game setting: " + settingName);
            if (settingName.IsNullOrEmpty()) {
                return;
            }

            PlattenTekSettings settings = PlattenTekModule.Settings;

            switch (settingName) {
                case "Copy Custom Info Template to Clipboard":
                    TextInput.SetClipboardText(settings.InfoCustomTemplate);
                    return;
                case "Set Custom Info Template From Clipboard":
                    settings.InfoCustomTemplate = TextInput.GetClipboardText();
                    PlattenTekModule.Instance.SaveSettings();
                    return;
            }

            if (typeof(PlattenTekSettings).GetProperty(settingName) is { } property) {
                if (property.GetSetMethod(true) == null) {
                    return;
                }

                object value = property.GetValue(settings);
                if (value is bool boolValue) {
                    property.SetValue(settings, !boolValue);
                } else if (value is Enum) {
                    property.SetValue(settings, ((int) value + 1) % Enum.GetValues(property.PropertyType).Length);
                }
            }
        }

        #endregion

        #region Write

        protected override void EstablishConnection() {
            var studio = this;
            var celeste = this;
            studio = null;


            Message? lastMessage;

            Log("Sending handshake");
            studio?.WriteMessageGuaranteed(new Message(MessageIDs.EstablishConnection, new byte[0]));

            lastMessage = celeste?.ReadMessageGuaranteed();
            if (lastMessage?.Id != MessageIDs.EstablishConnection) {
                if (lastMessage != null) {
                    throw new NeedsResetException($"Invalid data recieved while establishing connection (first): {lastMessage?.Id}");
                }
                else
                    throw new NeedsResetException($"Invalid data recieved while establishing connection (first)");
            }
            Log("Got handshake.  Sending Directory");

            celeste?.SendPath(Directory.GetCurrentDirectory());
            lastMessage = studio?.ReadMessageGuaranteed();
            studio?.ProcessSendPath(lastMessage?.Data);

            studio?.SendPath(null);

            Log("Getting data back");
            lastMessage = celeste?.ReadMessageGuaranteed();
            if (lastMessage?.Id != MessageIDs.SendPath) {
                throw new NeedsResetException("Invalid data recieved while establishing connection (second)");
            }

            celeste?.ProcessSendPath(lastMessage?.Data);

            if (Engine.Scene is Level)
                SendLevelLoaded();

            Initialized = true;
        }

        private void SendPath(string path) {
            byte[] pathBytes = Encoding.Default.GetBytes(path);
            WriteMessageGuaranteed(new Message(MessageIDs.SendPath, pathBytes));
        }

        private void SendLevelLoaded(Session session = null) {

            if (session  == null) {
                var level = Engine.Scene as Level;
                if (level == null)
                    level = Engine.NextScene as Level;

                if (level == null)
                    return;

                session = level.Session;
            }

            if (session.Area.LevelSet == "Celeste")
                return;

            string command = "";

            var content = Everest.Content.Get($"Maps/{session.MapData.ModeData.Path}");
            var asset = content as FileSystemModAsset;

            if (asset == null)
                return;

            command += asset.Path + '\n';
            string chapterSide = "A";
            switch (session.Area.Mode) {
                case AreaMode.BSide:
                    chapterSide = "B";
                    break;
                case AreaMode.CSide:
                    chapterSide = "C";
                    break;
            }
                
            command += $"{Dialog.Clean(session.Area.GetLevelSet())} {session.Area.ChapterIndex}-{chapterSide}";

            byte[] commandBytes = Encoding.Default.GetBytes(command);
            WriteMessageGuaranteed(new Message(MessageIDs.LoadedLevel, commandBytes));
        }
        private void SendLevelUnloaded() {
            WriteMessageGuaranteed(new Message(MessageIDs.UnloadedLevel, new byte[0]));
        }

        private void SendStateAndGameDataNow(string state, string gameData, bool canFail) {
            if (Initialized) {
                string[] data = new string[] {state, gameData};
                byte[] dataBytes = ToByteArray(data);
                Message message = new(MessageIDs.SendState, dataBytes);
                if (canFail) {
                    WriteMessage(message);
                } else {
                    WriteMessageGuaranteed(message);
                }
            }
        }

        public void SendStateAndGameData(string state, string gameData, bool canFail) {
            PendingWrite = () => SendStateAndGameDataNow(state, gameData, canFail);
        }

        #endregion
    }
}