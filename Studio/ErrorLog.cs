using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace PlattenTek {
    public static class ErrorLog {
        private const string Filename = "plattentek_errorlog.txt";
        private const string Marker = "==========================================";

        public static void Write(Exception e) {
            Write(e.ToString());
        }

        public static void Write(string str) {
            StringBuilder stringBuilder = new();
            string text = "";
            if (Path.IsPathRooted(Filename)) {
                string directoryName = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(directoryName)) {
                    Directory.CreateDirectory(directoryName);
                }
            }

            if (File.Exists(Filename)) {
                StreamReader streamReader = new(Filename);
                text = streamReader.ReadToEnd();
                streamReader.Close();
                if (!text.Contains(Marker)) {
                    text = "";
                }
            }

            stringBuilder.Append("PlattenTek Compiler");

            stringBuilder.AppendLine(" Error Log");
            stringBuilder.AppendLine(Marker);
            stringBuilder.AppendLine();
            stringBuilder.Append("Ver ");
            stringBuilder.AppendLine(Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

            stringBuilder.AppendLine(DateTime.Now.ToString());
            stringBuilder.AppendLine(str);
            if (text != "") {
                int startIndex = text.IndexOf(Marker) + Marker.Length;
                string value = text.Substring(startIndex);
                stringBuilder.AppendLine(value);
            }

            StreamWriter streamWriter = new(Filename, append: false);
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
        }

        public static void Open() {
            if (File.Exists(Filename)) {
                Process.Start(Filename);
            }
        }
    }
}