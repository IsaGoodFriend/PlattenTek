
namespace PlattenTek {
	partial class Studio {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Studio));
			this.hotkeyToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.statusBarContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.decompileMultipleContextItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openLevelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeLevelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.decompileMultipleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.detectLevelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.autoCompileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.autoDecompileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dividerLabel = new System.Windows.Forms.Label();
			this.decompileButton = new System.Windows.Forms.Button();
			this.openToCompile = new System.Windows.Forms.OpenFileDialog();
			this.compileButton = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.deleteUnusedLevelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusBarContextMenuStrip.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// hotkeyToolTip
			// 
			this.hotkeyToolTip.AutomaticDelay = 200;
			this.hotkeyToolTip.AutoPopDelay = 5000;
			this.hotkeyToolTip.InitialDelay = 200;
			this.hotkeyToolTip.IsBalloon = true;
			this.hotkeyToolTip.ReshowDelay = 200;
			this.hotkeyToolTip.ShowAlways = true;
			this.hotkeyToolTip.ToolTipTitle = "Fact: Birds are hard to catch";
			// 
			// statusBarContextMenuStrip
			// 
			this.statusBarContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decompileMultipleContextItem});
			this.statusBarContextMenuStrip.Name = "statusBarMenuStrip";
			this.statusBarContextMenuStrip.Size = new System.Drawing.Size(188, 26);
			// 
			// decompileMultipleContextItem
			// 
			this.decompileMultipleContextItem.Name = "decompileMultipleContextItem";
			this.decompileMultipleContextItem.Size = new System.Drawing.Size(187, 22);
			this.decompileMultipleContextItem.Text = "Decompile Multiple...";
			this.decompileMultipleContextItem.Click += new System.EventHandler(this.compileMultipleMenuItem_Click);
			// 
			// menuStrip
			// 
			this.menuStrip.BackColor = System.Drawing.SystemColors.Control;
			this.menuStrip.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(284, 24);
			this.menuStrip.TabIndex = 3;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.Checked = true;
			this.fileToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLevelMenuItem,
            this.closeLevelMenuItem,
            this.toolStripSeparator1,
            this.decompileMultipleMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// openLevelMenuItem
			// 
			this.openLevelMenuItem.Name = "openLevelMenuItem";
			this.openLevelMenuItem.Size = new System.Drawing.Size(244, 22);
			this.openLevelMenuItem.Text = "&Open Level...";
			this.openLevelMenuItem.Click += new System.EventHandler(this.openLevelMenuItem_Click);
			// 
			// closeLevelMenuItem
			// 
			this.closeLevelMenuItem.Name = "closeLevelMenuItem";
			this.closeLevelMenuItem.Size = new System.Drawing.Size(244, 22);
			this.closeLevelMenuItem.Text = "&Close Level";
			this.closeLevelMenuItem.Click += new System.EventHandler(this.closeLevelMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(241, 6);
			// 
			// decompileMultipleMenuItem
			// 
			this.decompileMultipleMenuItem.Name = "decompileMultipleMenuItem";
			this.decompileMultipleMenuItem.Size = new System.Drawing.Size(244, 22);
			this.decompileMultipleMenuItem.Text = "&Decompile Multiple...";
			this.decompileMultipleMenuItem.Click += new System.EventHandler(this.compileMultipleMenuItem_Click);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detectLevelMenuItem,
            this.autoCompileMenuItem,
            this.autoDecompileMenuItem,
            this.deleteUnusedLevelsToolStripMenuItem});
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
			this.settingsToolStripMenuItem.Text = "&Settings";
			// 
			// detectLevelMenuItem
			// 
			this.detectLevelMenuItem.Checked = true;
			this.detectLevelMenuItem.CheckOnClick = true;
			this.detectLevelMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.detectLevelMenuItem.Name = "detectLevelMenuItem";
			this.detectLevelMenuItem.Size = new System.Drawing.Size(276, 22);
			this.detectLevelMenuItem.Text = "Detect &Level From Celeste";
			this.detectLevelMenuItem.CheckedChanged += new System.EventHandler(this.detectLevelMenuItem_CheckedChanged);
			// 
			// autoCompileMenuItem
			// 
			this.autoCompileMenuItem.CheckOnClick = true;
			this.autoCompileMenuItem.Name = "autoCompileMenuItem";
			this.autoCompileMenuItem.Size = new System.Drawing.Size(276, 22);
			this.autoCompileMenuItem.Text = "Auto &Compile Level";
			// 
			// autoDecompileMenuItem
			// 
			this.autoDecompileMenuItem.CheckOnClick = true;
			this.autoDecompileMenuItem.Name = "autoDecompileMenuItem";
			this.autoDecompileMenuItem.Size = new System.Drawing.Size(276, 22);
			this.autoDecompileMenuItem.Text = "Auto &Decompile Level";
			// 
			// dividerLabel
			// 
			this.dividerLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dividerLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.dividerLabel.Location = new System.Drawing.Point(0, 24);
			this.dividerLabel.Name = "dividerLabel";
			this.dividerLabel.Size = new System.Drawing.Size(284, 1);
			this.dividerLabel.TabIndex = 4;
			// 
			// decompileButton
			// 
			this.decompileButton.Enabled = false;
			this.decompileButton.Location = new System.Drawing.Point(12, 105);
			this.decompileButton.Name = "decompileButton";
			this.decompileButton.Size = new System.Drawing.Size(118, 44);
			this.decompileButton.TabIndex = 5;
			this.decompileButton.Text = "Decompile Level";
			this.decompileButton.UseVisualStyleBackColor = true;
			this.decompileButton.Click += new System.EventHandler(this.decompileButton_Click);
			// 
			// openToCompile
			// 
			this.openToCompile.FileName = "openBinForCompile";
			// 
			// compileButton
			// 
			this.compileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.compileButton.Enabled = false;
			this.compileButton.Location = new System.Drawing.Point(154, 105);
			this.compileButton.Name = "compileButton";
			this.compileButton.Size = new System.Drawing.Size(118, 44);
			this.compileButton.TabIndex = 6;
			this.compileButton.Text = "Compile Level";
			this.compileButton.UseVisualStyleBackColor = true;
			this.compileButton.Click += new System.EventHandler(this.compileButton_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(12, 43);
			this.progressBar.MarqueeAnimationSpeed = 1;
			this.progressBar.Maximum = 1000;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(259, 23);
			this.progressBar.Step = 1000;
			this.progressBar.TabIndex = 7;
			// 
			// deleteUnusedLevelsToolStripMenuItem
			// 
			this.deleteUnusedLevelsToolStripMenuItem.Name = "deleteUnusedLevelsToolStripMenuItem";
			this.deleteUnusedLevelsToolStripMenuItem.Size = new System.Drawing.Size(276, 22);
			this.deleteUnusedLevelsToolStripMenuItem.Text = "Delete &Unused Levels";
			// 
			// Studio
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(284, 161);
			this.ContextMenuStrip = this.statusBarContextMenuStrip;
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.compileButton);
			this.Controls.Add(this.decompileButton);
			this.Controls.Add(this.dividerLabel);
			this.Controls.Add(this.menuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip;
			this.MaximumSize = new System.Drawing.Size(300, 200);
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "Studio";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "PlattenTek";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TASStudio_FormClosed);
			this.Shown += new System.EventHandler(this.Studio_Shown);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Studio_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Studio_DragEnter);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Studio_KeyDown);
			this.statusBarContextMenuStrip.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip hotkeyToolTip;
        private System.Windows.Forms.ContextMenuStrip statusBarContextMenuStrip;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.Label dividerLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectLevelMenuItem;
        private System.Windows.Forms.Button decompileButton;
        private System.Windows.Forms.OpenFileDialog openToCompile;
        private System.Windows.Forms.ToolStripMenuItem openLevelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoCompileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoDecompileMenuItem;
        private System.Windows.Forms.Button compileButton;
        private System.Windows.Forms.ToolStripMenuItem closeLevelMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem decompileMultipleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decompileMultipleContextItem;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ToolStripMenuItem deleteUnusedLevelsToolStripMenuItem;
    }
}

