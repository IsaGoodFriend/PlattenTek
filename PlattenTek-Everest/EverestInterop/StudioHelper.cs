using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Celeste.Mod;
using Ionic.Zip;

namespace PlattekMod.EverestInterop {
    public static class StudioHelper {
        private const string StudioName = "PlattenTek";
        private static EverestModuleMetadata Metadata => PlattenTekModule.Instance.Metadata;
        private static string StudioNameWithExe => StudioName + ".exe";
        private static string StudioNameInZip => $"net452/{StudioNameWithExe}";
        private static string CopiedStudioExePath => Path.Combine(Everest.PathGame, StudioNameWithExe);

        public static void Initialize() {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {

                ExtractStudio(out bool studioProcessWasKilled);
                LaunchStudioAtBoot(studioProcessWasKilled);
            }
        }

        private static void ExtractStudio(out bool studioProcessWasKilled) {

            studioProcessWasKilled = false;
            if (!File.Exists(CopiedStudioExePath) || CheckNewerStudio()) {
                try {
                    Process studioProcess = Process.GetProcesses().FirstOrDefault(process =>
                        process.ProcessName.Contains("PlattenTek"));


                    if (studioProcess != null) {
                        
                        studioProcess.Kill();
                        studioProcess.WaitForExit(50000);
                    }

                    if (studioProcess?.HasExited == false) {
                        return;
                    }

                    if (studioProcess?.HasExited == true) {
                        studioProcessWasKilled = true;
                    }

                    if (!string.IsNullOrEmpty(Metadata.PathArchive)) {
                        using (ZipFile zip = ZipFile.Read(Metadata.PathArchive)) {

                            if (zip.EntryFileNames.Contains(StudioNameWithExe)) {
                                foreach (ZipEntry entry in zip.Entries) {
                                    if (entry.FileName.StartsWith(StudioName) && !entry.FileName.Contains(".dll")) {
                                        entry.Extract(Everest.PathGame, ExtractExistingFileAction.OverwriteSilently);
                                    }
                                }
                            }
                        }
                    } else if (!string.IsNullOrEmpty(Metadata.PathDirectory)) {
                        string[] files = Directory.GetFiles(Metadata.PathDirectory, "*", SearchOption.AllDirectories);

                        if (files.Any(filePath => filePath.EndsWith(StudioNameWithExe))) {
                            foreach (string sourceFile in files) {
                                string fileName = Path.GetFileName(sourceFile);
                                if (fileName.StartsWith(StudioName) && !fileName.Contains(".dll")) {
                                    string destFile = Path.Combine(Everest.PathGame, fileName);
                                    File.Copy(sourceFile, destFile, true);
                                }
                            }
                        }
                    }

                    PlattenTekModule.Settings.StudioLastModifiedTime = File.GetLastWriteTime(CopiedStudioExePath);
                    PlattenTekModule.Instance.SaveSettings();
                } catch (UnauthorizedAccessException e) {
                    Logger.Log("PlattenTekModule", "Failed to extract studio.  UnauthorizedAccessException");
                    Logger.LogDetailed(e);
                } catch (Exception e) {
                    Logger.Log("PlattenTekModule", "Failed to extract studio.  Unknown Exception");
                    Logger.LogDetailed(e);
                }
            } else {
                foreach (string file in Directory.GetFiles(Everest.PathGame, "*.PendingOverwrite")) {
                    File.Delete(file);
                }
            }
        }

        private static bool CheckNewerStudio() {
            if (!PlattenTekModule.Settings.AutoExtractNewStudio) {
                return false;
            }

            DateTime modifiedTime = new();

            if (!string.IsNullOrEmpty(Metadata.PathArchive)) {
                using (ZipFile zip = ZipFile.Read(Metadata.PathArchive)) {
                    if (zip.Entries.FirstOrDefault(zipEntry => zipEntry.FileName == StudioNameWithExe) is { } studioZipEntry) {
                        modifiedTime = studioZipEntry.LastModified;
                    }
                }
            } else if (!string.IsNullOrEmpty(Metadata.PathDirectory)) {
                string[] files = Directory.GetFiles(Metadata.PathDirectory);

                if (files.FirstOrDefault(filePath => filePath.EndsWith(StudioNameWithExe)) is { } studioFilePath) {
                    modifiedTime = File.GetLastWriteTime(studioFilePath);
                }
            }

            return modifiedTime.CompareTo(PlattenTekModule.Settings.StudioLastModifiedTime) > 0;
        }

        private static void LaunchStudioAtBoot(bool studioProcessWasKilled) {
            if (PlattenTekModule.Settings.LaunchStudioAtBoot || studioProcessWasKilled) {
                try {

                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes) {
                        if (process.ProcessName.Contains(StudioName)) {
                            return;
                        }
                    }

                    if (File.Exists(CopiedStudioExePath)) {
                        Process.Start(CopiedStudioExePath);
                    }

                } catch (Exception e) {
                    Logger.Log("PlattenTekModule", "Failed to launch studio at boot.");
                    Logger.LogDetailed(e);
                }
            }
        }
    }
}