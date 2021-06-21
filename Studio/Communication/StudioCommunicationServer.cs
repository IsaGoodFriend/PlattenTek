using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PlattekCommunication;

//using Microsoft.Xna.Framework.Input;

namespace PlattenTek.Communication {
    public sealed class StudioCommunicationServer : StudioCommunicationBase {
        public static StudioCommunicationServer instance;

        private StudioCommunicationServer() { }

        public static void Run() {
            //this should be modified to check if there's another studio open as well
            if (instance != null) {
                return;
            }

            instance = new StudioCommunicationServer();

            ThreadStart mainLoop = new(instance.UpdateLoop);
            Thread updateThread = new(mainLoop);
            updateThread.CurrentCulture = CultureInfo.InvariantCulture;
            updateThread.Name = "PlattenStudioCom Server";
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        protected override void WriteReset() {
            // ignored
        }

        public void ExternalReset() => PendingWrite = () => throw new NeedsResetException();


        #region Read

        protected override void ReadData(Message message) {
            switch (message.Id) {
                case MessageIDs.EstablishConnection:
                    throw new NeedsResetException("Recieved initialization message (EstablishConnection) from main loop");
                case MessageIDs.Reset:
                    throw new NeedsResetException("Recieved reset message from main loop");
                case MessageIDs.Wait:
                    ProcessWait();
                    break;
                case MessageIDs.SendPath:
                    throw new NeedsResetException("Recieved initialization message (SendPath) from main loop");
                case MessageIDs.LoadedLevel:
                    ProcessLoadedLevel(message.Data);
                    break;
                case MessageIDs.UnloadedLevel:
                    ProcessUnloadedLevel();
                    break;
                default:
                    throw new InvalidOperationException($"{message.Id}");
            }
        }

        private void ProcessSendPath(byte[] data) {
            string path = Encoding.Default.GetString(data);
            Log(path);
        }

        private void ProcessLoadedLevel(byte[] data) {
            string[] values = Encoding.Default.GetString(data).Split('\n');

            Studio.Instance.CelesteLevel = values[0];
            Studio.Instance.CelesteLevelName = values[1];
        }
        private void ProcessUnloadedLevel() {
            Studio.Instance.CelesteLevel = null;

        }

        #endregion

        #region Write

        protected override void EstablishConnection() {
            var studio = this;
            var celeste = this;
            celeste = null;
            Message? lastMessage;

            studio?.ReadMessage();
            studio?.WriteMessageGuaranteed(new Message(MessageIDs.EstablishConnection, new byte[0]));
            celeste?.ReadMessageGuaranteed();

            celeste?.SendPath(null);
            lastMessage = studio?.ReadMessageGuaranteed();
            if (lastMessage?.Id != MessageIDs.SendPath) {
                throw new NeedsResetException("Invalid data recieved while establishing connection");
            }

            studio?.ProcessSendPath(lastMessage?.Data);

            lastMessage = celeste?.ReadMessageGuaranteed();
            celeste?.ProcessSendPath(lastMessage?.Data);

            studio?.SendPath("Empty");

            Initialized = true;
        }

        public void SendPath(string path) => PendingWrite = () => SendPathNow(path, false);
        public void GetModInfo() => PendingWrite = GetModInfoNow;
        public void OnStudioClosing() => PendingWrite = () => SendClosingNow(false);


        private void SendPathNow(string path, bool canFail) {
            if (Initialized || !canFail) {
                byte[] pathBytes = path != null ? Encoding.Default.GetBytes(path) : new byte[0];

                WriteMessageGuaranteed(new Message(MessageIDs.SendPath, pathBytes));
            }
        }
        private void GetModInfoNow() {
            if (!Initialized) {
                return;
            }

            WriteMessageGuaranteed(new Message(MessageIDs.GetModInfo, new byte[0]));
        }
        private void SendClosingNow(bool canFail) {
            if (Initialized || !canFail) {

                WriteMessageGuaranteed(new Message(MessageIDs.StudioClosing, new byte[0]));
            }

        }


        #endregion
    }
}