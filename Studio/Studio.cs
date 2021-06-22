using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using PlattenTek.Communication;
using PlattenTek.Entities;
using PlattenTek.Properties;
using PlattekCommunication;

namespace PlattenTek {
    public partial class Studio : Form {

        private const int DECOMPILE_WAIT_TIMEOUT = 20 * 1000;

        public static Studio Instance;

        private DateTime slowUpdateTimer = DateTime.MinValue;

        private Thread backgroundThread;

        private const NotifyFilters NOTIFICATION_FILTERS = NotifyFilters.Size | NotifyFilters.CreationTime;
        private FileSystemWatcher decompiledWatcher, compiledWatcher;
        private string watchPathDecompile, watchPathCompile;

        private string customLoadedLevel;
        private string projectFolder, binFolder;

        public string CurrentFileName {
            get {
                if (detectLevelMenuItem.Checked && StudioCommunicationBase.Initialized)
                    return CelesteLevel;

                return customLoadedLevel;
            }
            private set {
                customLoadedLevel = value;
            }
        }
        public string CelesteLevelName { get; set; }
        public string CelesteLevel {
            get { return celesteLevel; }
            set { celesteLevel = value;
                if (detectLevelMenuItem.Checked)
                    ChangeFile(value);
            }
        }
        private string celesteLevel;

        private List<string> toDecompile = new List<string>();

        private string TitleBarText {
            get {
                string title = "PlattenTek - ";

                if (detectLevelMenuItem.Checked && StudioCommunicationBase.Initialized) {
                    if (CelesteLevel == null)
                        title += "No Level Loaded";
                    else
                        title += CelesteLevelName;
                } else if (string.IsNullOrWhiteSpace(customLoadedLevel))
                    title += "No Level Loaded";
                else if (!FileExists())
                    title += "File is missing";
                else {
                    string fullname = "";
                    string separated = CurrentFileName;

                    for (int i = 0; i < 3; ++i) {
                        fullname = Path.Combine(Path.GetFileNameWithoutExtension(separated), fullname);
                        separated= Path.GetDirectoryName(separated);
                    }
                    fullname = fullname.Replace('\\', '/');
                    title += fullname;
                }


                return title;
            }
        }

        #region Init Studio

        public Studio() {

            backgroundThread = new Thread((ThreadStart)delegate { });

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            InitializeComponent();
            InitSettings();

            EnableStudio(false);

            Text = TitleBarText;

            DesktopLocation = Settings.Default.DesktopLocation;

            if (!IsTitleBarVisible()) {
                DesktopLocation = new Point(0, 0);
            }

            Instance = this;
        }

        private void InitSettings() {
            if (Settings.Default.UpgradeTime < File.GetLastWriteTime(Assembly.GetEntryAssembly().Location)) {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeTime = DateTime.Now;
            }
            autoCompileMenuItem.Checked = Settings.Default.AutoCompile;
            autoDecompileMenuItem.Checked = Settings.Default.AutoDecompile;
            detectLevelMenuItem.Checked = Settings.Default.DetectFromCeleste;
        }

        #endregion

        [STAThread]
        public static void Main() {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            RunSingleton(() => {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Studio());
            });
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception exception = e.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception");
            if (exception.GetType().FullName == "System.Configuration.ConfigurationErrorsException") {
                MessageBox.Show("Your configuration file is corrupted and will be deleted automatically, please try to launch celeste studio again.",
                    "Configuration Errors Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Celeste_Studio");
                if (Directory.Exists(configFolder)) {
                    Directory.Delete(configFolder, true);
                }

                return;
            }

            ErrorLog.Write(exception);
            ErrorLog.Open();
        }

        private static void RunSingleton(Action action) {
            string appGuid =
                ((GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;

            string mutexId = $"Global\\{{{appGuid}}}";

            var allowEveryoneRule =
                new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                        , null)
                    , MutexRights.FullControl
                    , AccessControlType.Allow
                );
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            using (var mutex = new Mutex(false, mutexId, out _, securitySettings)) {
                var hasHandle = false;
                try {
                    try {
                        hasHandle = mutex.WaitOne(TimeSpan.Zero, false);
                        if (hasHandle == false) {
                            MessageBox.Show("Studio already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    } catch (AbandonedMutexException) {
                        hasHandle = true;
                    }

                    // Perform your work here.
                    action();
                } finally {
                    if (hasHandle) {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }


        private bool IsTitleBarVisible() {
            int titleBarHeight = RectangleToScreen(ClientRectangle).Top - Top;
            Rectangle titleBar = new(Left, Top, Width, titleBarHeight);
            foreach (Screen screen in Screen.AllScreens) {
                if (screen.Bounds.IntersectsWith(titleBar)) {
                    return true;
                }
            }

            return false;
        }

        private void SaveSettings() {
            Settings.Default.DesktopLocation = DesktopLocation;
            Settings.Default.AutoCompile = autoCompileMenuItem.Checked;
            Settings.Default.AutoDecompile = autoDecompileMenuItem.Checked;
            Settings.Default.DetectFromCeleste = detectLevelMenuItem.Checked;

            Settings.Default.Save();
        }
        private void EnableStudio(bool hooked) {
            if (hooked) {

            } else {
                StudioCommunicationServer.Run();
            }
        }

        private void OpenFile() {

            using OpenFileDialog fileBox = new();

            fileBox.Filter = "Celeste Level Files|*.bin" + '|' + 
                "Decompiled Level Files|*.binnode";
            fileBox.FilterIndex = 0;
            fileBox.InitialDirectory = Settings.Default.LastFolder;

            if (!Directory.Exists(Settings.Default.LastFolder)) {
                fileBox.InitialDirectory = Directory.GetCurrentDirectory();
                Settings.Default.LastFolder = fileBox.InitialDirectory;
                Settings.Default.Save();
            }

            switch (fileBox.ShowDialog()) {
                case DialogResult.OK:

                    string modsFolder = fileBox.FileName.Replace('/', '\\');
                    
                    do {
                        modsFolder = Path.GetDirectoryName(modsFolder);
                    } while (modsFolder.Contains("Celeste\\Mods") && !modsFolder.EndsWith("Celeste\\Mods"));

                    Settings.Default.LastFolder = modsFolder;
                    Settings.Default.Save();

                    ChangeFile(fileBox.FileName);
                    detectLevelMenuItem.Checked = false;

                    Invoke((Action)delegate {
                        detectLevelMenuItem.Checked = false;
                    });

                    if (!FileExists()) {
                        CompileLevel();
                    }

                    break;
            }

        }
        private void ChangeFile(string fileName) {

            if (string.IsNullOrEmpty(fileName))
                return;

            if (fileName.EndsWith(".binnode")) {

                bool foundBaseFolder;

                do {
                    var allFiles = Directory.GetFiles(Path.GetDirectoryName(fileName));

                    if (allFiles.Length == 0)
                        break;
                    foundBaseFolder = true;

                    foreach (var f in allFiles) {
                        if (f.EndsWith(".binnode")) {
                            foundBaseFolder = false;
                            break;
                        }
                    }

                    if (!foundBaseFolder)
                        fileName = Path.GetDirectoryName(fileName);

                } while (!foundBaseFolder);

                fileName = DecompilerHelper.CreateDirectory(fileName, false) + ".bin";
            }

            projectFolder = DecompilerHelper.CreateDirectory(fileName, true);
            binFolder = DecompilerHelper.CreateDirectory(fileName, false);

            if (!File.Exists(fileName) && !Directory.Exists(projectFolder))
                return;

            CurrentFileName = fileName;

            watchPathDecompile = projectFolder;
            watchPathCompile = Path.GetDirectoryName(fileName);

            DecompilerHelper.ResetProgress();
            DestroyWatchers();

            if (!Directory.Exists(projectFolder)) {
                Directory.CreateDirectory(projectFolder);
            }
        }
        private bool FileExists() {
            return ProjectExists() || BinExists();
        }
        private bool BinExists() {
            return File.Exists(CurrentFileName);
        }
        private bool ProjectExists() {
            string exPath = Path.Combine(
                projectFolder,
                BinaryFileNode.ATTRIBUTE_FILE_NAME
                );

            return File.Exists(exPath);
        }

        private void DestroyWatchers() {
            if (decompiledWatcher != null) {
                decompiledWatcher.Dispose();
                decompiledWatcher = null;
            }
            if (compiledWatcher != null) {
                compiledWatcher.Dispose();
                compiledWatcher = null;
            }
        }
        private void CreateWatchers() {
            if (string.IsNullOrEmpty(watchPathDecompile))
                return;

            decompiledWatcher = new FileSystemWatcher(watchPathDecompile);

            decompiledWatcher.NotifyFilter = NOTIFICATION_FILTERS;
            decompiledWatcher.Changed += OnDecompiledChanged;
            decompiledWatcher.Created += OnDecompiledChanged;
            decompiledWatcher.Deleted += OnDecompiledChanged;
            decompiledWatcher.Renamed += OnDecompiledRenamed;

            decompiledWatcher.IncludeSubdirectories = true;
            decompiledWatcher.EnableRaisingEvents = true;

            compiledWatcher = new FileSystemWatcher(watchPathCompile, Path.GetFileName(CurrentFileName));

            compiledWatcher.NotifyFilter = NOTIFICATION_FILTERS;
            compiledWatcher.Changed += OnCompiledChanged;
            compiledWatcher.Created += OnCompiledChanged;
            compiledWatcher.Deleted += OnCompiledChanged;

            compiledWatcher.EnableRaisingEvents = true;
        }

        private void UpdateLoop() {
            bool lastHooked = false;

            while (true) {
                try {
                    bool hooked = StudioCommunicationBase.Initialized;
                    if (lastHooked != hooked) {
                        lastHooked = hooked;

                        Invoke((Action)delegate { EnableStudio(hooked); });

                        if (detectLevelMenuItem.Checked && !hooked) {
                            CurrentFileName = null;
                        }
                    }
                    bool levelLoaded = CurrentFileName != null && FileExists() && (!backgroundThread.IsAlive);

                    if (levelLoaded) {
                        if (compileButton.Enabled != ProjectExists()) {
                            Invoke((Action)delegate {
                                compileButton.Enabled = ProjectExists();
                            });
                        }
                        if (decompileButton.Enabled != BinExists()) {
                            Invoke((Action)delegate {
                                decompileButton.Enabled = BinExists();
                            });
                        }
                    }
                    else {
                        if (compileButton.Enabled) {
                            Invoke((Action)delegate {
                                compileButton.Enabled = false;
                            });
                        }
                        if (decompileButton.Enabled) {
                            Invoke((Action)delegate {
                                decompileButton.Enabled = false;
                            });
                        }

                    }
                    
                    if (toDecompile.Count > 0) {
                        DecompileMultipleLevels();
                    }

                    if (slowUpdateTimer.AddSeconds(0.3f) < DateTime.Now) {
                        slowUpdateTimer = DateTime.Now;
                    }

                    closeLevelMenuItem.Enabled = levelLoaded;
                    
                    Invoke((Action)delegate {
                        Text = TitleBarText;
                        progressBar.Value = (int)(DecompilerHelper.Progress * progressBar.Maximum);
                    });


                    Thread.Sleep(14);
                } catch {
                    // ignore
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }

        #region Create New Thread

        private bool WaitForBGThead(bool wait) {

            if (backgroundThread.IsAlive) {
                if (!wait)
                    return false;
                const int threadPause = 100;

                int i;

                for (i = 0; i < DECOMPILE_WAIT_TIMEOUT && backgroundThread.IsAlive; i += threadPause) {
                    Thread.Sleep(threadPause);
                }

                return i < DECOMPILE_WAIT_TIMEOUT;
            }
            
            return true;
        }

        private void DecompileLevel(bool wait = false) {
            if (!WaitForBGThead(wait))
                return;

            backgroundThread = new Thread(DecompileThread);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }
        private void DecompileThread() {
            DestroyWatchers();

            DecompilerHelper.DecompileLevel(CurrentFileName);

            CreateWatchers();
        }

        private void DecompileMultipleLevels(bool wait = false) {
            if (!WaitForBGThead(wait))
                return;

            backgroundThread = new Thread(DecompileMultipleThread);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();

        }
        private void DecompileMultipleThread() {
            DestroyWatchers();

            string[] multifiles = toDecompile.ToArray();
            toDecompile.Clear();

            DecompilerHelper.DecompileMultiple(multifiles);

            CreateWatchers();
        }

        private void CompileLevel(bool wait = false) {
            if (!WaitForBGThead(wait))
                return;

            backgroundThread = new Thread(CompileThread);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();

        }
        private void CompileThread() {
            DestroyWatchers();

            DecompilerHelper.CompileLevel(CurrentFileName);

            CreateWatchers();
        }

        #endregion

        #region Events

        private void TASStudio_FormClosed(object sender, FormClosedEventArgs e) {
            SaveSettings();
            StudioCommunicationServer.instance?.OnStudioClosing();
            Thread.Sleep(100);
        }

        private void Studio_Shown(object sender, EventArgs e) {
            Thread updateThread = new(UpdateLoop);
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        private void Studio_KeyDown(object sender, KeyEventArgs e) {
            try {
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.O) {
                    OpenFile();
                } else if (e.Modifiers == (Keys.Shift | Keys.Alt) && e.KeyCode == Keys.C) {
                    CompileLevel();
                } else if (e.Modifiers == (Keys.Shift | Keys.Alt) && e.KeyCode == Keys.D) {
                    DecompileLevel();
                } else if (e.Modifiers == (Keys.Shift | Keys.Control) && e.KeyCode == Keys.D) {
                    StudioCommunicationServer.instance?.ExternalReset();
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.Write(ex);
            }
        }

        private void OnCompiledChanged(object sender, FileSystemEventArgs e) {
            if (autoDecompileMenuItem.Checked) {
                DecompileLevel();
            }
        }

        private void OnDecompiledChanged(object sender, FileSystemEventArgs e) {
            if (autoCompileMenuItem.Checked) {
                CompileLevel();
            }
        }

        private void OnDecompiledRenamed(object sender, RenamedEventArgs e) {
            OnDecompiledChanged(null, null);
        }

        private void openLevelMenuItem_Click(object sender, EventArgs e) {
            OpenFile();
        }

        private void decompileButton_Click(object sender, EventArgs e) {
            DecompileLevel();
        }

        private void closeLevelMenuItem_Click(object sender, EventArgs e) {
            if (StudioCommunicationBase.Initialized && CurrentFileName != null && FileExists() && detectLevelMenuItem.Checked)
                detectLevelMenuItem.Checked = false;
            customLoadedLevel = null;
        }

        private void detectLevelMenuItem_CheckedChanged(object sender, EventArgs e) {
            if (detectLevelMenuItem.Checked) {
                ChangeFile(celesteLevel);
            }
        }

        private void compileMultipleMenuItem_Click(object sender, EventArgs e) {

            using OpenFileDialog fileBox = new();

            fileBox.Filter = "Celeste Level Files|*.bin";
            fileBox.FilterIndex = 0;
            fileBox.InitialDirectory = Settings.Default.LastFolder;
            fileBox.Multiselect = true;

            if (!Directory.Exists(Settings.Default.LastFolder)) {
                fileBox.InitialDirectory = Directory.GetCurrentDirectory();
                Settings.Default.LastFolder = fileBox.InitialDirectory;
                Settings.Default.Save();
            }

            switch (fileBox.ShowDialog()) {
                case DialogResult.OK:

                    toDecompile.AddRange(fileBox.FileNames);

                    break;
            }

        }

        private void Studio_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.None;

            string[] fileList = (string[]) e.Data.GetData(DataFormats.FileDrop, false);

            if (fileList.Length == 1) {
                if (fileList[0].EndsWith(".bin") || fileList[0].EndsWith(".binnode")) {
                    e.Effect = DragDropEffects.Copy;

                } else if (!Path.HasExtension(fileList[0]) && Directory.Exists(fileList[0])) {
                    fileList = Directory.GetFiles(fileList[0]);
                    if (fileList.Length == 1 &&  fileList[0].EndsWith(".binnode")) {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
            else if (fileList.Length > 1) {

                foreach (var file in fileList) {
                    if (file.EndsWith(".bin")) {
                        e.Effect = DragDropEffects.Copy;
                        break;
                    }
                }
            }
        }

        private void infoLabel_TextChanged(object sender, EventArgs e) {

        }

        private void Studio_DragDrop(object sender, DragEventArgs e) {
            string[] fileList = (string[]) e.Data.GetData(DataFormats.FileDrop, false);

            if (fileList.Length == 1) {
                if (fileList[0].EndsWith(".bin") || fileList[0].EndsWith(".binnode")) {
                    detectLevelMenuItem.Checked = false;
                    ChangeFile(fileList[0]);
                }
                else if (!Path.HasExtension(fileList[0]) && Directory.Exists(fileList[0])) {

                    fileList = Directory.GetFiles(fileList[0]);
                    if (fileList.Length == 1 &&  fileList[0].EndsWith(".binnode")) {
                        detectLevelMenuItem.Checked = false;
                        ChangeFile(fileList[0]);
                    }
                }
            } else if (fileList.Length > 1) {
                List<string> foundFiles = new List<string>();
                foreach (var file in fileList) {
                    if (file.EndsWith(".bin")) {
                        foundFiles.Add(file);
                    }
                }

                if (foundFiles.Count > 0) {
                    var options = MessageBox.Show("Do you want to decompile all these levels?", "PlattenTek", MessageBoxButtons.YesNo);
                    if (options == DialogResult.Yes) {
                        toDecompile.AddRange(foundFiles);
                    }
                }
            }
        }

        private void compileButton_Click(object sender, EventArgs e) {
            CompileLevel();
        }

        #endregion
    }
}