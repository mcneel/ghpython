namespace GhPython.Forms
{
    partial class PythonScriptForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PythonScriptForm));
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.applyAndCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.ghPythonGrasshopperHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.rhinoscriptsyntaxHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.rhinoscriptsyntaxBasicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.rhinoCommonBasicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.rhinoPythonWebsiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.grasshopperForumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.pythonDocumentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.mainStatusText = new System.Windows.Forms.ToolStripStatusLabel();
      this.versionLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.testButton = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      this.menuStrip.SuspendLayout();
      this.statusStrip.SuspendLayout();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip
      // 
      this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
      this.menuStrip.Location = new System.Drawing.Point(0, 0);
      this.menuStrip.Name = "menuStrip";
      this.menuStrip.Size = new System.Drawing.Size(542, 24);
      this.menuStrip.TabIndex = 0;
      this.menuStrip.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.testToolStripMenuItem,
            this.applyAndCloseToolStripMenuItem,
            this.toolStripSeparator1,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "&File";
      // 
      // closeToolStripMenuItem
      // 
      this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
      this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      this.closeToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
      this.closeToolStripMenuItem.Text = "&Close";
      this.closeToolStripMenuItem.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // testToolStripMenuItem
      // 
      this.testToolStripMenuItem.Name = "testToolStripMenuItem";
      this.testToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
      this.testToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
      this.testToolStripMenuItem.Text = "&Test";
      this.testToolStripMenuItem.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // applyAndCloseToolStripMenuItem
      // 
      this.applyAndCloseToolStripMenuItem.Name = "applyAndCloseToolStripMenuItem";
      this.applyAndCloseToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F5)));
      this.applyAndCloseToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
      this.applyAndCloseToolStripMenuItem.Text = "&OK (Test and Close)";
      this.applyAndCloseToolStripMenuItem.Click += new System.EventHandler(this.okButton_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
      // 
      // openToolStripMenuItem
      // 
      this.openToolStripMenuItem.Name = "openToolStripMenuItem";
      this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
      this.openToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
      this.openToolStripMenuItem.Text = "&Import From...";
      this.openToolStripMenuItem.Click += new System.EventHandler(this.importFrom_Click);
      // 
      // saveToolStripMenuItem
      // 
      this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
      this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
      this.saveToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
      this.saveToolStripMenuItem.Text = "&Export As....";
      this.saveToolStripMenuItem.Click += new System.EventHandler(this.exportAs_Click);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ghPythonGrasshopperHelpToolStripMenuItem,
            this.rhinoscriptsyntaxHelpToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripSeparator3,
            this.rhinoPythonWebsiteToolStripMenuItem,
            this.grasshopperForumToolStripMenuItem,
            this.toolStripSeparator2,
            this.pythonDocumentationToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
      this.helpToolStripMenuItem.Text = "&Help";
      // 
      // ghPythonGrasshopperHelpToolStripMenuItem
      // 
      this.ghPythonGrasshopperHelpToolStripMenuItem.Name = "ghPythonGrasshopperHelpToolStripMenuItem";
      this.ghPythonGrasshopperHelpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
      this.ghPythonGrasshopperHelpToolStripMenuItem.Text = "GhPython Component Help";
      this.ghPythonGrasshopperHelpToolStripMenuItem.Click += new System.EventHandler(this.ghPythonGrasshopperHelpToolStripMenuItem_Click);
      // 
      // rhinoscriptsyntaxHelpToolStripMenuItem
      // 
      this.rhinoscriptsyntaxHelpToolStripMenuItem.Name = "rhinoscriptsyntaxHelpToolStripMenuItem";
      this.rhinoscriptsyntaxHelpToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
      this.rhinoscriptsyntaxHelpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
      this.rhinoscriptsyntaxHelpToolStripMenuItem.Text = "&Help for rhinoscriptsyntax";
      this.rhinoscriptsyntaxHelpToolStripMenuItem.Click += new System.EventHandler(this.rhinoscriptsyntaxHelp);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rhinoscriptsyntaxBasicsToolStripMenuItem,
            this.rhinoCommonBasicsToolStripMenuItem});
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(229, 22);
      this.toolStripMenuItem1.Text = "&Samples";
      // 
      // rhinoscriptsyntaxBasicsToolStripMenuItem
      // 
      this.rhinoscriptsyntaxBasicsToolStripMenuItem.Name = "rhinoscriptsyntaxBasicsToolStripMenuItem";
      this.rhinoscriptsyntaxBasicsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
      this.rhinoscriptsyntaxBasicsToolStripMenuItem.Text = "rhinoscriptsyntax basics";
      this.rhinoscriptsyntaxBasicsToolStripMenuItem.Click += new System.EventHandler(this.rhinoscriptsyntaxBasicsToolStripMenuItem_Click);
      // 
      // rhinoCommonBasicsToolStripMenuItem
      // 
      this.rhinoCommonBasicsToolStripMenuItem.Name = "rhinoCommonBasicsToolStripMenuItem";
      this.rhinoCommonBasicsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
      this.rhinoCommonBasicsToolStripMenuItem.Text = "RhinoCommon basics";
      this.rhinoCommonBasicsToolStripMenuItem.Click += new System.EventHandler(this.rhinoCommonBasicsToolStripMenuItem_Click);
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(226, 6);
      // 
      // rhinoPythonWebsiteToolStripMenuItem
      // 
      this.rhinoPythonWebsiteToolStripMenuItem.Name = "rhinoPythonWebsiteToolStripMenuItem";
      this.rhinoPythonWebsiteToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
      this.rhinoPythonWebsiteToolStripMenuItem.Text = "&Rhino.Python Forum";
      this.rhinoPythonWebsiteToolStripMenuItem.Click += new System.EventHandler(this.rhinoPythonWebsiteToolStripMenuItem_Click);
      // 
      // grasshopperForumToolStripMenuItem
      // 
      this.grasshopperForumToolStripMenuItem.Name = "grasshopperForumToolStripMenuItem";
      this.grasshopperForumToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
      this.grasshopperForumToolStripMenuItem.Text = "&Grasshopper Forum";
      this.grasshopperForumToolStripMenuItem.Click += new System.EventHandler(this.grasshopperForumToolStripMenuItem_Click);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
      // 
      // pythonDocumentationToolStripMenuItem
      // 
      this.pythonDocumentationToolStripMenuItem.Name = "pythonDocumentationToolStripMenuItem";
      this.pythonDocumentationToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
      this.pythonDocumentationToolStripMenuItem.Text = "&Python.org Documentation";
      this.pythonDocumentationToolStripMenuItem.Click += new System.EventHandler(this.pythonDocumentationToolStripMenuItem_Click);
      // 
      // statusStrip
      // 
      this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainStatusText,
            this.versionLabel});
      this.statusStrip.Location = new System.Drawing.Point(0, 524);
      this.statusStrip.Name = "statusStrip";
      this.statusStrip.Size = new System.Drawing.Size(542, 22);
      this.statusStrip.TabIndex = 1;
      this.statusStrip.Text = "statusStrip1";
      // 
      // mainStatusText
      // 
      this.mainStatusText.Name = "mainStatusText";
      this.mainStatusText.Size = new System.Drawing.Size(450, 17);
      this.mainStatusText.Spring = true;
      this.mainStatusText.Text = "...";
      this.mainStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // versionLabel
      // 
      this.versionLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.versionLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.versionLabel.Enabled = false;
      this.versionLabel.Name = "versionLabel";
      this.versionLabel.Size = new System.Drawing.Size(46, 17);
      this.versionLabel.Text = "Version";
      this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(374, 6);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.Location = new System.Drawing.Point(455, 6);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Close";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // testButton
      // 
      this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.testButton.Location = new System.Drawing.Point(3, 6);
      this.testButton.Name = "testButton";
      this.testButton.Size = new System.Drawing.Size(75, 23);
      this.testButton.TabIndex = 2;
      this.testButton.Text = "Test";
      this.testButton.UseVisualStyleBackColor = true;
      this.testButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.testButton);
      this.panel1.Controls.Add(this.cancelButton);
      this.panel1.Controls.Add(this.okButton);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 488);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(542, 36);
      this.panel1.TabIndex = 2;
      // 
      // richTextBox1
      // 
      this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBox1.Location = new System.Drawing.Point(0, 0);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.ReadOnly = true;
      this.richTextBox1.Size = new System.Drawing.Size(538, 126);
      this.richTextBox1.TabIndex = 1;
      this.richTextBox1.Text = "";
      // 
      // splitContainer
      // 
      this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer.Location = new System.Drawing.Point(0, 24);
      this.splitContainer.Name = "splitContainer";
      this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      this.splitContainer.Panel1MinSize = 200;
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.richTextBox1);
      this.splitContainer.Size = new System.Drawing.Size(542, 464);
      this.splitContainer.SplitterDistance = 330;
      this.splitContainer.TabIndex = 3;
      // 
      // PythonScriptForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(542, 546);
      this.Controls.Add(this.splitContainer);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.statusStrip);
      this.Controls.Add(this.menuStrip);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip;
      this.MinimumSize = new System.Drawing.Size(300, 400);
      this.Name = "PythonScriptForm";
      this.Text = "Grasshopper Python Script Editor";
      this.Load += new System.EventHandler(this.PythonScriptForm_Load);
      this.menuStrip.ResumeLayout(false);
      this.menuStrip.PerformLayout();
      this.statusStrip.ResumeLayout(false);
      this.statusStrip.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rhinoPythonWebsiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem grasshopperForumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pythonDocumentationToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel versionLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ToolStripStatusLabel mainStatusText;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rhinoscriptsyntaxHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem rhinoscriptsyntaxBasicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rhinoCommonBasicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyAndCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ghPythonGrasshopperHelpToolStripMenuItem;
    }
}