using System.Text;
using Celeste.Mod;

namespace PlattekMod.EverestInterop {
    public static class DebugRcPage {
        private static readonly RCEndPoint InfoEndPoint = new() {
            Path = "/tas/info",
            Name = "PlattenTek Info",
            InfoHTML = "List some tas info.",
            Handle = c => {
                StringBuilder builder = new();
                Everest.DebugRC.WriteHTMLStart(c, builder);

                Everest.DebugRC.WriteHTMLEnd(c, builder);
                Everest.DebugRC.Write(c, builder.ToString());
            }
        };

        [Load]
        private static void Load() {
            Everest.DebugRC.EndPoints.Add(InfoEndPoint);
        }

        [Unload]
        private static void Unload() {
            Everest.DebugRC.EndPoints.Remove(InfoEndPoint);
        }

        private static void WriteLine(StringBuilder builder, string text) {
            builder.Append($@"{text}<br />");
        }
    }
}