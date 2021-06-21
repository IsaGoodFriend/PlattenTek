using System;

namespace PlattekCommunication {
    public class HighPriorityAttribute : Attribute { }

    public enum MessageIDs : byte {
        //Connection
        /// <summary>
        /// Unused
        /// </summary>
        Default = 0x00,

        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] StudioClosing = 0x01,

        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] ModClosing = 0x02,

        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] GetModInfo = 0x09,

        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] EstablishConnection = 0x0D,

        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] Wait = 0x0E,

        /// <summary>
        /// Structure:
        /// </summary>
        Reset = 0x0F,

        //Pure data transfer
        /// <summary>
        /// Structure: string[] = { state, gameData }
        /// </summary>
        SendState = 0x10,

        /// <summary>
        /// Structure: string
        /// </summary>
        SendGameData = 0x11,

        //Data transfer from Studio
        /// <summary>
        /// Structure: string
        /// </summary>
        [HighPriority] SendPath = 0x20,

        // Celeste loaded a level
        /// <summary>
        /// Structure: string
        /// the level path and the level name, separate by a new line char
        /// </summary>
        [HighPriority] LoadedLevel = 0x21,

        // Celeste unloaded a level
        /// <summary>
        /// Structure:
        /// </summary>
        [HighPriority] UnloadedLevel = 0x22,

        /// <summary>
        /// Structure: string
        /// </summary>
        [HighPriority] ReturnModInfo = 0x32,

        //External data transfer
        ExternLow1 = 0x40,
        ExternLow2 = 0x41,
        [HighPriority] ExternHigh1 = 0x42,
        [HighPriority] ExternHigh2 = 0x43,
    }
}