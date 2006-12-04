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
      this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.dictionaryPage = new System.Windows.Forms.TabPage();
      this.translatePage = new System.Windows.Forms.TabPage();
      this.transInput = new System.Windows.Forms.TextBox();
      this.examplePage = new System.Windows.Forms.TabPage();
      this.exampleInput = new System.Windows.Forms.TextBox();
      this.studyPage = new System.Windows.Forms.TabPage();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.transOutput = new Jappy.RicherTextBox();
      this.exampleResults = new Jappy.RicherTextBox();
      menuStrip = new System.Windows.Forms.MenuStrip();
      fileMenu = new System.Windows.Forms.ToolStripMenuItem();
      exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      statusBar = new System.Windows.Forms.StatusStrip();
      dictionarySearchControl = new Jappy.DictionarySearchTab();
      menuStrip.SuspendLayout();
      statusBar.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.dictionaryPage.SuspendLayout();
      this.translatePage.SuspendLayout();
      this.examplePage.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip
      // 
      menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileMenu});
      menuStrip.Location = new System.Drawing.Point(0, 0);
      menuStrip.Name = "menuStrip";
      menuStrip.Size = new System.Drawing.Size(574, 24);
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
      statusBar.Location = new System.Drawing.Point(0, 451);
      statusBar.Name = "statusBar";
      statusBar.Size = new System.Drawing.Size(574, 22);
      statusBar.TabIndex = 3;
      // 
      // statusText
      // 
      this.statusText.AutoSize = false;
      this.statusText.Name = "statusText";
      this.statusText.Size = new System.Drawing.Size(559, 17);
      this.statusText.Spring = true;
      this.statusText.Text = "Welcome to Jappy!";
      this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.tabControl.Size = new System.Drawing.Size(574, 427);
      this.tabControl.TabIndex = 4;
      this.tabControl.TabStop = false;
      // 
      // dictionaryPage
      // 
      this.dictionaryPage.Controls.Add(dictionarySearchControl);
      this.dictionaryPage.Location = new System.Drawing.Point(4, 25);
      this.dictionaryPage.Name = "dictionaryPage";
      this.dictionaryPage.Size = new System.Drawing.Size(566, 398);
      this.dictionaryPage.TabIndex = 0;
      this.dictionaryPage.Text = "Dictionary";
      this.dictionaryPage.ToolTipText = "Look up words in the dictionary.";
      this.dictionaryPage.UseVisualStyleBackColor = true;
      // 
      // translatePage
      // 
      this.translatePage.Controls.Add(this.transOutput);
      this.translatePage.Controls.Add(this.transInput);
      this.translatePage.Location = new System.Drawing.Point(4, 25);
      this.translatePage.Name = "translatePage";
      this.translatePage.Size = new System.Drawing.Size(533, 398);
      this.translatePage.TabIndex = 2;
      this.translatePage.Text = "Translate";
      this.translatePage.ToolTipText = "Translate words in Japanese text.";
      this.translatePage.UseVisualStyleBackColor = true;
      // 
      // transInput
      // 
      this.transInput.Dock = System.Windows.Forms.DockStyle.Top;
      this.transInput.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
      this.transInput.Location = new System.Drawing.Point(0, 0);
      this.transInput.Multiline = true;
      this.transInput.Name = "transInput";
      this.transInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.transInput.Size = new System.Drawing.Size(533, 71);
      this.transInput.TabIndex = 0;
      this.transInput.MouseLeave += new System.EventHandler(this.control_PopStatusText);
      this.transInput.MouseEnter += new System.EventHandler(this.transInput_MouseEnter);
      this.transInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.transInput_KeyPress);
      // 
      // examplePage
      // 
      this.examplePage.Controls.Add(this.exampleResults);
      this.examplePage.Controls.Add(this.exampleInput);
      this.examplePage.Location = new System.Drawing.Point(4, 25);
      this.examplePage.Name = "examplePage";
      this.examplePage.Size = new System.Drawing.Size(533, 398);
      this.examplePage.TabIndex = 3;
      this.examplePage.Text = "Examples";
      this.examplePage.ToolTipText = "Search example sentences.";
      this.examplePage.UseVisualStyleBackColor = true;
      // 
      // exampleInput
      // 
      this.exampleInput.BackColor = System.Drawing.SystemColors.Window;
      this.exampleInput.Dock = System.Windows.Forms.DockStyle.Top;
      this.exampleInput.Location = new System.Drawing.Point(0, 0);
      this.exampleInput.Name = "exampleInput";
      this.exampleInput.Size = new System.Drawing.Size(533, 22);
      this.exampleInput.TabIndex = 3;
      this.exampleInput.MouseLeave += new System.EventHandler(this.control_PopStatusText);
      this.exampleInput.MouseEnter += new System.EventHandler(this.exampleInput_MouseEnter);
      this.exampleInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.exampleInput_KeyPress);
      // 
      // studyPage
      // 
      this.studyPage.Location = new System.Drawing.Point(4, 25);
      this.studyPage.Name = "studyPage";
      this.studyPage.Padding = new System.Windows.Forms.Padding(3);
      this.studyPage.Size = new System.Drawing.Size(533, 398);
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
      // dictionarySearchControl
      // 
      dictionarySearchControl.Dock = System.Windows.Forms.DockStyle.Fill;
      dictionarySearchControl.Location = new System.Drawing.Point(0, 0);
      dictionarySearchControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      dictionarySearchControl.Name = "dictionarySearchControl";
      dictionarySearchControl.Size = new System.Drawing.Size(566, 398);
      dictionarySearchControl.TabIndex = 0;
      // 
      // transOutput
      // 
      this.transOutput.BackColor = System.Drawing.SystemColors.Window;
      this.transOutput.DetectUrls = false;
      this.transOutput.Dock = System.Windows.Forms.DockStyle.Fill;
      this.transOutput.Location = new System.Drawing.Point(0, 71);
      this.transOutput.Name = "transOutput";
      this.transOutput.ReadOnly = true;
      this.transOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.transOutput.Size = new System.Drawing.Size(533, 327);
      this.transOutput.TabIndex = 1;
      this.transOutput.TabStop = false;
      this.transOutput.Text = "";
      // 
      // exampleResults
      // 
      this.exampleResults.BackColor = System.Drawing.SystemColors.Window;
      this.exampleResults.DetectUrls = false;
      this.exampleResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.exampleResults.Location = new System.Drawing.Point(0, 22);
      this.exampleResults.Name = "exampleResults";
      this.exampleResults.ReadOnly = true;
      this.exampleResults.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.exampleResults.Size = new System.Drawing.Size(533, 376);
      this.exampleResults.TabIndex = 4;
      this.exampleResults.TabStop = false;
      this.exampleResults.Text = "";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(574, 473);
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
      this.translatePage.PerformLayout();
      this.examplePage.ResumeLayout(false);
      this.examplePage.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStripStatusLabel statusText;
    private System.Windows.Forms.TabPage dictionaryPage;
    private System.Windows.Forms.TabPage studyPage;
    private System.Windows.Forms.TabPage translatePage;
    private System.Windows.Forms.TextBox transInput;
    private RicherTextBox transOutput;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.TabPage examplePage;
    private RicherTextBox exampleResults;
    private System.Windows.Forms.TextBox exampleInput;
  }
}