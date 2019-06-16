namespace GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ScreenPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundColourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pixelColourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.partyModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wrapYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveState5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadState6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayButton = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ScreenPanel
            // 
            this.ScreenPanel.BackColor = System.Drawing.SystemColors.Window;
            this.ScreenPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ScreenPanel.Location = new System.Drawing.Point(12, 27);
            this.ScreenPanel.Name = "ScreenPanel";
            this.ScreenPanel.Size = new System.Drawing.Size(512, 256);
            this.ScreenPanel.TabIndex = 0;
            this.ScreenPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ScreenPanel_Paint);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(538, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveState5ToolStripMenuItem,
            this.loadState6ToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundColourToolStripMenuItem,
            this.pixelColourToolStripMenuItem,
            this.partyModeToolStripMenuItem,
            this.wrapYToolStripMenuItem,
            this.debugLogToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // backgroundColourToolStripMenuItem
            // 
            this.backgroundColourToolStripMenuItem.Name = "backgroundColourToolStripMenuItem";
            this.backgroundColourToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.backgroundColourToolStripMenuItem.Text = "Background Colour";
            this.backgroundColourToolStripMenuItem.Click += new System.EventHandler(this.backgroundColourToolStripMenuItem_Click);
            // 
            // pixelColourToolStripMenuItem
            // 
            this.pixelColourToolStripMenuItem.Name = "pixelColourToolStripMenuItem";
            this.pixelColourToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.pixelColourToolStripMenuItem.Text = "Pixel Colour";
            this.pixelColourToolStripMenuItem.Click += new System.EventHandler(this.pixelColourToolStripMenuItem_Click);
            // 
            // partyModeToolStripMenuItem
            // 
            this.partyModeToolStripMenuItem.Name = "partyModeToolStripMenuItem";
            this.partyModeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.partyModeToolStripMenuItem.Text = "Party Mode";
            this.partyModeToolStripMenuItem.Click += new System.EventHandler(this.partyModeToolStripMenuItem_Click);
            // 
            // wrapYToolStripMenuItem
            // 
            this.wrapYToolStripMenuItem.Checked = true;
            this.wrapYToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wrapYToolStripMenuItem.Name = "wrapYToolStripMenuItem";
            this.wrapYToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wrapYToolStripMenuItem.Text = "Wrap Y";
            this.wrapYToolStripMenuItem.Click += new System.EventHandler(this.wrapYToolStripMenuItem_Click);
            // 
            // debugLogToolStripMenuItem
            // 
            this.debugLogToolStripMenuItem.Name = "debugLogToolStripMenuItem";
            this.debugLogToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.debugLogToolStripMenuItem.Text = "Debug Log";
            this.debugLogToolStripMenuItem.Click += new System.EventHandler(this.debugLogToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // saveState5ToolStripMenuItem
            // 
            this.saveState5ToolStripMenuItem.Name = "saveState5ToolStripMenuItem";
            this.saveState5ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveState5ToolStripMenuItem.Text = "Save state (5)";
            this.saveState5ToolStripMenuItem.Click += new System.EventHandler(this.saveState5ToolStripMenuItem_Click);
            // 
            // loadState6ToolStripMenuItem
            // 
            this.loadState6ToolStripMenuItem.Name = "loadState6ToolStripMenuItem";
            this.loadState6ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadState6ToolStripMenuItem.Text = "Load state (6)";
            this.loadState6ToolStripMenuItem.Click += new System.EventHandler(this.loadState6ToolStripMenuItem_Click);
            // 
            // PlayButton
            // 
            this.PlayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlayButton.Location = new System.Drawing.Point(324, 2);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(25, 23);
            this.PlayButton.TabIndex = 2;
            this.PlayButton.Text = ">";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // PauseButton
            // 
            this.PauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PauseButton.Location = new System.Drawing.Point(352, 2);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(25, 23);
            this.PauseButton.TabIndex = 3;
            this.PauseButton.Text = "| |";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 297);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.PlayButton);
            this.Controls.Add(this.ScreenPanel);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(554, 336);
            this.MinimumSize = new System.Drawing.Size(554, 336);
            this.Name = "MainForm";
            this.Text = "Chip8";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel ScreenPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundColourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pixelColourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem partyModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wrapYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveState5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadState6ToolStripMenuItem;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.Button PauseButton;
    }
}

