using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace PlattenTek {
    public static class DecompilerHelper {

        public const string MAP_DECOMPILED_FOLDER = "Maps_Decompiled";

        public static float Progress { get { return (fileProgress + progress) / fileCount; } }

        public static void ResetProgress() {
            progress = 0;
            fileCount = 1;
            fileProgress = 0;
        }

        private static float progress;
        private static int fileCount = 1, fileProgress;
        private static int nodeCount, nodeProgress;
        
        public static string CreateDirectory(string file, bool makeProjectFolder) {
            string ext = Path.GetExtension(file);

            file = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
            file = file.Replace('\\', '/');

            var binMatch = Regex.Match(file, @"/Mods/([A-Za-z0-9_\s])+/Maps/");
            var projMatch = Regex.Match(file, @$"/Mods/([A-Za-z0-9_\s])+/{MAP_DECOMPILED_FOLDER}/");

            if (!binMatch.Success && !projMatch.Success)
                return file;

            string toReplace;
            int index;

            if (binMatch.Success) {
                index = binMatch.Index + binMatch.Length - 5;
            }
            else {
                index = projMatch.Index + projMatch.Length - (MAP_DECOMPILED_FOLDER.Length + 1);
            }

            toReplace = file.Substring(index, file.IndexOf('/', index) - index);

            file = file.Replace(toReplace, makeProjectFolder ? MAP_DECOMPILED_FOLDER : "Maps");

            return file;
        }

        public static void DecompileLevel(string file) {
            DecompileLevel(file, true);

        }
        private static void DecompileLevel(string file, bool resetFileCount) {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return;

            string roomsDirectory = CreateDirectory(file, true);

            if (!Directory.Exists(roomsDirectory))
                Directory.CreateDirectory(roomsDirectory);

            if (resetFileCount) {
                fileProgress = 0;
                fileCount = 1;
            }
            progress = 0;

            var parser = new BinaryFileParser(file);

            nodeCount = parser.NodeCount;
            nodeProgress = 0;

            DecompileNode(parser.RootNode, roomsDirectory, false);

        }
        public static void DecompileMultiple(string[] files) {
            fileCount = files.Length;
            fileProgress = 0;

            for (int i = 0; i < files.Length; ++i) {
                fileProgress = i;
                DecompileLevel(files[i], false);
            }
        }
        public static void CompileLevel(string file) {

            if (string.IsNullOrWhiteSpace(file))
                return;

            progress = 0;
            fileProgress = 0;
            fileCount = 1;

            string roomsDirectory = CreateDirectory(file, true);
            file = Path.ChangeExtension(CreateDirectory(file, false), ".bin");

            nodeCount = Directory.GetFiles(roomsDirectory, "*.binnode", SearchOption.AllDirectories).Length;
            nodeProgress = 0;

            using (var writer = new BinaryFileWriter()) {

                BinaryFileParser parser = File.Exists(file) ? new BinaryFileParser(file) : null;

                writer.LevelName = parser != null ? parser.LevelName : "SampleName";

                RecompileNode(writer.RootNode, roomsDirectory);

                writer.RootNode.Name = "Maps";

                writer.Save(file, false);

                writer.Dispose();
            }

            progress = 1;
        }

        private static void DecompileNode(BinaryFileNode node, string path, bool compileChildren) {

            if (File.Exists(Path.Combine(path, BinaryFileNode.ATTRIBUTE_FILE_NAME))) {
                File.Delete(Path.Combine(path, BinaryFileNode.ATTRIBUTE_FILE_NAME));
            }

            if (!compileChildren) {

                int childCount = 0;

                foreach (var child in node.Children) {
                    string name = child.Name;

                    bool addChildren = false;

                    switch (node.Name) {
                        case "levels":
                            name = child.GetString("name");
                            addChildren = true;
                            break;
                        case "Style":
                            addChildren = true;
                            break;
                        case "Filler":
                            addChildren = true;
                            name = $"rect_{childCount++}";
                            break;
                    }

                    var folder = Path.Combine(path, name);

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    else if (child.Name == "level") {
                        foreach (var dir in Directory.GetDirectories(folder)) {
                            Directory.Delete(dir, true);
                        }
                    }

                    DecompileNode(child, folder, addChildren);
                }
            }

            node.SaveAttributes(path, compileChildren);

            if (compileChildren) {
                nodeProgress += node.NodeCount;
            }
            else {
                nodeProgress += 1;
            }
            progress = (float)nodeProgress / nodeCount;
        }
        private static void RecompileNode(BinaryFileNode node, string path) {

            node.ReadAttributes(Path.Combine(path, BinaryFileNode.ATTRIBUTE_FILE_NAME));

            nodeProgress++;

            progress = (float)nodeProgress / nodeCount;

            if (node.Name == "level") {
                node.Attributes["name"] = BinaryFileParser.GetFromSafeString(Path.GetFileName(path));
            }

            foreach (var folder in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)) {
                var child = new BinaryFileNode();
                RecompileNode(child, folder);
                node.Children.Add(child);

            }

        }
    }
}
