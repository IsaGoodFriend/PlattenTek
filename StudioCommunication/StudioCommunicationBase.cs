using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace PlattekCommunication {
    public class StudioCommunicationBase {
        protected const int BufferSize = 0x100000;
        protected const int HeaderLength = 9;

        private static readonly List<StudioCommunicationBase> AttachedCom = new();
        private readonly Mutex mutex;

        private readonly MemoryMappedFile sharedMemory;

        protected bool Abort;
        public Func<byte[], bool> ExternalReadHandler;
        private int failedWrites = 0;
        private int lastSignature;

        public Action PendingWrite;
        protected int Timeout = 16;
        private int timeoutCount = 0;
        private bool waiting;

        protected StudioCommunicationBase() {
            sharedMemory = MemoryMappedFile.CreateOrOpen("PlattenTek", BufferSize);
            mutex = new Mutex(false, "PlattenTekCOM", out bool created);
            if (!created) {
                mutex = Mutex.OpenExisting("PlattenTekCOM");
            }

            Log($"Creating Base: {created}");

            AttachedCom.Add(this);
        }

        protected StudioCommunicationBase(string target) {
            sharedMemory = MemoryMappedFile.CreateOrOpen(target, BufferSize);
            mutex = new Mutex(false, target, out bool created);
            if (!created) {
                mutex = Mutex.OpenExisting(target);
            }

            AttachedCom.Add(this);
        }

        public static bool Initialized { get; protected set; }

        ~StudioCommunicationBase() {
            sharedMemory.Dispose();
            mutex.Dispose();
        }

        protected void UpdateLoop() {

            while (!Abort) {

                EstablishConnectionLoop();
                try {
                    while (!Abort) {
                        Message? message = ReadMessage();

                        if (message != null) {
                            ReadData((Message) message);
                            waiting = false;
                        }

                        Thread.Sleep(Timeout);

                        if (!NeedsToWait()) {
                            PendingWrite?.Invoke();
                            PendingWrite = null;
                        }
                    }
                }
                //For this to work all writes must occur in this thread
                catch (NeedsResetException e) {
                    ForceReset(e);
                }
            }
        }

        protected virtual bool NeedsToWait() => waiting;

        private bool IsHighPriority(MessageIDs id) =>
            Attribute.IsDefined(typeof(MessageIDs).GetField(Enum.GetName(typeof(MessageIDs), id)), typeof(HighPriorityAttribute));

        protected Message? ReadMessage() {
            MessageIDs id = default;
            int signature;
            int size;
            byte[] data;

            using (MemoryMappedViewStream stream = sharedMemory.CreateViewStream()) {
                mutex.WaitOne();
                //Log($"{this} acquired mutex for read");

                BinaryReader reader = new(stream);
                BinaryWriter writer = new(stream);

                id = (MessageIDs) reader.ReadByte();
                if (id == MessageIDs.Default) {
                    mutex.ReleaseMutex();
                    return null;
                }

                //Make sure the message came from the other side
                signature = reader.ReadInt32();
                if (signature == lastSignature) {
                    mutex.ReleaseMutex();
                    return null;
                }

                size = reader.ReadInt32();
                data = reader.ReadBytes(size);

                //Overwriting the first byte ensures that the data will only be read once
                stream.Position = 0;
                writer.Write((byte) 0);

                mutex.ReleaseMutex();
            }


            Message message = new(id, data);
            //if (message.Id != MessageIDs.SendState && message.Id != MessageIDs.SendHotkeyPressed) {
            //    Log($"{this} received {message.Id} with length {message.Length}");
            //}

            return message;
        }

        protected Message ReadMessageGuaranteed() {
            Log($"{this} forcing read");
            int failedReads = 0;
            for (;;) {
                Message? message = ReadMessage();
                if (message != null) {
                    return (Message) message;
                }

                if (Initialized && ++failedReads > 100) {
                    Log("Read timed out");
                    //throw new NeedsResetException("Read timed out");
                    Initialized = false;
                }

                Thread.Sleep(Timeout);
            }
        }

        protected bool WriteMessage(Message message, bool local = true) {
            if (!local) {
                foreach (var com in AttachedCom) {
                    if (com != this) {
                        com.PendingWrite = com.PendingWrite ?? (() => WriteMessage(message));
                    }
                }
            }

            using (MemoryMappedViewStream stream = sharedMemory.CreateViewStream()) {
                mutex.WaitOne();

                //Log($"{this} acquired mutex for write");
                BinaryReader reader = new(stream);
                BinaryWriter writer = new(stream);

                //Check that there isn't a message waiting to be read
                byte firstByte = reader.ReadByte();
                if (firstByte != 0 && (!IsHighPriority(message.Id) || IsHighPriority((MessageIDs) firstByte))) {
                    mutex.ReleaseMutex();
                    if (Initialized && ++failedWrites > 100) {
                        Log("Write timed out");
                        //throw new NeedsResetException("Write timed out");
                        Initialized = false;
                    }

                    return false;
                }

                //if (message.Id != MessageIDs.SendState && message.Id != MessageIDs.SendHotkeyPressed) {
                //    Log($"{this} writing {message.Id} with length {message.Length}");
                //}

                stream.Position = 0;
                writer.Write(message.GetBytes());

                mutex.ReleaseMutex();
            }

            lastSignature = Message.Signature;
            failedWrites = 0;
            return true;
        }

        protected void WriteMessageGuaranteed(Message message, bool local = true) {
            if (!local) {
                foreach (var com in AttachedCom) {
                    if (com != this) {
                        com.PendingWrite = () => WriteMessageGuaranteed(message);
                    }
                }
            }

            if (message.Id != MessageIDs.SendState) {
                Log($"{this} forcing write of {message.Id} with length {message.Length}");
            }

            for (;;) {
                if (WriteMessage(message)) {
                    break;
                }

                Thread.Sleep(Timeout);
            }
        }

        protected void ForceReset(NeedsResetException e) {
            Initialized = false;
            waiting = false;
            failedWrites = 0;
            PendingWrite = null;
            timeoutCount++;
            Log($"Exception thrown - {e.Message}");
            //Ensure the first byte of the mmf is reset
            using (MemoryMappedViewStream stream = sharedMemory.CreateViewStream()) {
                mutex.WaitOne();
                BinaryWriter writer = new(stream);
                writer.Write((byte) 0);
                mutex.ReleaseMutex();
            }

            WriteReset();
            Thread.Sleep(Timeout * 2);
        }

        //Only needs to be used on the Celeste end, as Celeste will detect disconnects much faster
        protected virtual void WriteReset() {
            using (MemoryMappedViewStream stream = sharedMemory.CreateViewStream()) {
                mutex.WaitOne();
                BinaryWriter writer = new(stream);
                Message reset = new(MessageIDs.Reset, new byte[0]);
                writer.Write(reset.GetBytes());
                mutex.ReleaseMutex();
            }
        }

        public void WriteWait() {
            PendingWrite = () => WriteMessageGuaranteed(new Message(MessageIDs.Wait, new byte[0]));
        }

        protected void ProcessWait() {
            waiting = true;
        }

        protected virtual void ReadData(Message message) { }

        private void EstablishConnectionLoop() {
            for (;;) {
                try {
                    EstablishConnection();
                    timeoutCount = 0;

                    break;
                } catch (NeedsResetException e) {
                    ForceReset(e);
                }
            }
        }

        protected virtual void EstablishConnection() { }

        //ty stackoverflow
        protected T FromByteArray<T>(byte[] data, int offset = 0, int length = 0) {
            if (data == null) {
                return default(T);
            }

            if (length == 0) {
                length = data.Length - offset;
            }

            BinaryFormatter bf = new();
            using (MemoryStream ms = new(data, offset, length)) {
                object obj = bf.Deserialize(ms);
                return (T) obj;
            }
        }

        protected byte[] ToByteArray<T>(T obj) {
            if (obj == null) {
                return new byte[0];
            }

            BinaryFormatter bf = new();
            using (MemoryStream ms = new()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public override string ToString() {
            string location = Assembly.GetExecutingAssembly().GetName().Name;
            return $"StudioCommunicationBase Location @ {location}";
        }

        internal void Log(string s) {
            if (timeoutCount <= 5) {
                Console.WriteLine(s);
            }
        }
        // This is literally the first thing I have ever written with threading
        // Apologies in advance to anyone else working on this

        public struct Message {
            public MessageIDs Id { get; private set; }
            public int Length { get; private set; }
            public byte[] Data { get; private set; }

            public static readonly int Signature = Thread.CurrentThread.GetHashCode();

            public Message(MessageIDs id, byte[] data) {
                Id = id;
                Data = data;
                Length = data.Length;
            }

            public byte[] GetBytes() {
                byte[] bytes = new byte[Length + HeaderLength];
                bytes[0] = (byte) Id;
                Buffer.BlockCopy(BitConverter.GetBytes(Signature), 0, bytes, 1, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, bytes, 5, 4);
                Buffer.BlockCopy(Data, 0, bytes, HeaderLength, Length);
                return bytes;
            }
        }

        protected class NeedsResetException : Exception {
            public NeedsResetException() { }
            public NeedsResetException(string message) : base(message) { }
        }
    }
}