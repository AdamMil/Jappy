namespace Jappy
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
      if(disposing && (components != null))
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
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.MenuStrip menuStrip;
      System.Windows.Forms.ToolStripMenuItem fileMenu;
      System.Windows.Forms.ToolStripMenuItem exitMenuItem;
      System.Windows.Forms.StatusStrip statusBar;
      Jappy.DictionarySearchTab dictionarySearchControl;
      Jappy.TranslateTab translateControl;
      Jappy.ExampleSearchTab exampleSearchControl;
      Jappy.StudyTab studyTabControl;
      this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.dictionaryPage = new System.Windows.Forms.TabPage();
      this.translatePage = new System.Windows.Forms.TabPage();
      this.examplePage = new System.Windows.Forms.TabPage();
      this.studyPage = new System.Windows.Forms.TabPage();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      menuStrip = new System.Windows.Forms.MenuStrip();
      fileMenu = new System.Windows.Forms.ToolStripMenuItem();
      exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      statusBar = new System.Windows.Forms.StatusStrip();
      dictionarySearchControl = new Jappy.DictionarySearchTab();
      translateControl = new Jappy.TranslateTab();
      exampleSearchControl = new Jappy.ExampleSearchTab();
      studyTabControl = new Jappy.StudyTab();
      menuStrip.SuspendLayout();
      statusBar.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.dictionaryPage.SuspendLayout();
      this.translatePage.SuspendLayout();
      this.examplePage.SuspendLayout();
      this.studyPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip
      // 
      menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileMenu});
      menuStrip.Location = new System.Drawing.Point(0, 0);
      menuStrip.Name = "menuStrip";
      menuStrip.Size = new System.Drawing.Size(752, 24);
      menuStrip.TabIndex = 10;
      // 
      // fileMenu
      // 
      fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            exitMenuItem});
      fileMenu.Name = "fileMenu";
      fileMenu.Size = new System.Drawing.Size(35, 20);
      fileMenu.Text = "&File";
      // 
      // exitMenuItem
      // 
      exitMenuItem.MergeIndex = 100;
      exitMenuItem.Name = "exitMenuItem";
      exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      exitMenuItem.Size = new System.Drawing.Size(132, 22);
      exitMenuItem.Text = "E&xit";
      exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
      // 
      // statusBar
      // 
      statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
      statusBar.Location = new System.Drawing.Point(0, 511);
      statusBar.Name = "statusBar";
      statusBar.Size = new System.Drawing.Size(752, 22);
      statusBar.TabIndex = 3;
      // 
      // statusText
      // 
      this.statusText.AutoSize = false;
      this.statusText.Name = "statusText";
      this.statusText.Size = new System.Drawing.Size(737, 17);
      this.statusText.Spring = true;
      this.statusText.Text = "Welcome to Jappy!";
      this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dictionarySearchControl
      // 
      dictionarySearchControl.Dock = System.Windows.Forms.DockStyle.Fill;
      dictionarySearchControl.Font = new System.Drawing.Font("Verdana", 9F);
      dictionarySearchControl.Location = new System.Drawing.Point(0, 0);
      dictionarySearchControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      dictionarySearchControl.Name = "dictionarySearchControl";
      dictionarySearchControl.Size = new System.Drawing.Size(744, 458);
      dictionarySearchControl.TabIndex = 0;
      // 
      // translateControl
      // 
      translateControl.Dock = System.Windows.Forms.DockStyle.Fill;
      translateControl.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      translateControl.Location = new System.Drawing.Point(0, 0);
      translateControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      translateControl.Name = "translateControl";
      translateControl.Size = new System.Drawing.Size(566, 398);
      translateControl.TabIndex = 0;
      // 
      // exampleSearchControl
      // 
      exampleSearchControl.Dock = System.Windows.Forms.DockStyle.Fill;
      exampleSearchControl.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      exampleSearchControl.Location = new System.Drawing.Point(0, 0);
      exampleSearchControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      exampleSearchControl.Name = "exampleSearchControl";
      exampleSearchControl.Size = new System.Drawing.Size(566, 398);
      exampleSearchControl.TabIndex = 0;
      // 
      // studyTabControl
      // 
      studyTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      studyTabControl.Location = new System.Drawing.Point(3, 3);
      studyTabControl.Name = "studyTabControl";
      studyTabControl.Size = new System.Drawing.Size(560, 392);
      studyTabControl.TabIndex = 0;
      // 
      // tabControl
      // 
      this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tabControl.Controls.Add(this.dictionaryPage);
      this.tabControl.Controls.Add(this.translatePage);
      this.tabControl.Controls.Add(this.examplePage);
      this.tabControl.Controls.Add(this.studyPage);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.HotTrack = true;
      this.tabControl.Location = new System.Drawing.Point(0, 24);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.ShowToolTips = true;
      this.tabControl.Size = new System.Drawing.Size(752, 487);
      this.tabControl.TabIndex = 4;
      this.tabControl.TabStop = false;
      this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
      this.tabControl.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Deselecting);
      // 
      // dictionaryPage
      // 
      this.dictionaryPage.Controls.Add(dictionarySearchControl);
      this.dictionaryPage.Location = new System.Drawing.Point(4, 25);
      this.dictionaryPage.Name = "dictionaryPage";
      this.dictionaryPage.Size = new System.Drawing.Size(744, 458);
      this.dictionaryPage.TabIndex = 0;
      this.dictionaryPage.Text = "Dictionary";
      this.dictionaryPage.ToolTipText = "Look up words in the dictionary.";
      this.dictionaryPage.UseVisualStyleBackColor = true;
      // 
      // translatePage
      // 
      this.translatePage.Controls.Add(translateControl);
      this.translatePage.Location = new System.Drawing.Point(4, 25);
      this.translatePage.Name = "translatePage";
      this.translatePage.Size = new System.Drawing.Size(566, 398);
      this.translatePage.TabIndex = 2;
      this.translatePage.Text = "Translate";
      this.translatePage.ToolTipText = "Translate words in Japanese text.";
      this.translatePage.UseVisualStyleBackColor = true;
      // 
      // examplePage
      // 
      this.examplePage.Controls.Add(exampleSearchControl);
      this.examplePage.Location = new System.Drawing.Point(4, 25);
      this.examplePage.Name = "examplePage";
      this.examplePage.Size = new System.Drawing.Size(566, 398);
      this.examplePage.TabIndex = 3;
      this.examplePage.Text = "Examples";
      this.examplePage.ToolTipText = "Search example sentences.";
      this.examplePage.UseVisualStyleBackColor = true;
      // 
      // studyPage
      // 
      this.studyPage.Controls.Add(studyTabControl);
      this.studyPage.Location = new System.Drawing.Point(4, 25);
      this.studyPage.Name = "studyPage";
      this.studyPage.Padding = new System.Windows.Forms.Padding(3);
      this.studyPage.Size = new System.Drawing.Size(566, 398);
      this.studyPage.TabIndex = 1;
      this.studyPage.Text = "Study";
      this.studyPage.ToolTipText = "Study words and kanji.";
      this.studyPage.UseVisualStyleBackColor = true;
      // 
      // toolTip
      // 
      this.toolTip.IsBalloon = true;
      // 
      // notifyIcon
      // 
      this.notifyIcon.Icon = global::Jappy.Properties.Resources.NotifyIcon;
      this.notifyIcon.Text = "Jappy. Click to restore.";
      this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(752, 533);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(statusBar);
      this.Controls.Add(menuStrip);
      this.Icon = global::Jappy.Properties.Resources.JappyIcon;
      this.KeyPreview = true;
      this.MainMenuStrip = menuStrip;
      this.MinimumSize = new System.Drawing.Size(582, 254);
      this.Name = "MainForm";
      this.Text = "Jappy";
      menuStrip.ResumeLayout(false);
      menuStrip.PerformLayout();
      statusBar.ResumeLayout(false);
      statusBar.PerformLayout();
      this.tabControl.ResumeLayout(false);
      this.dictionaryPage.ResumeLayout(false);
      this.translatePage.ResumeLayout(false);
      this.examplePage.ResumeLayout(false);
      this.studyPage.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStripStatusLabel statusText;
    private System.Windows.Forms.TabPage dictionaryPage;
    private System.Windows.Forms.TabPage studyPage;
    private System.Windows.Forms.TabPage translatePage;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.TabPage examplePage;
  }
}