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
      System.Windows.Forms.StatusStrip statusBar;
      System.Windows.Forms.ToolStripMenuItem exitMenuItem;
      this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.dictionaryPage = new System.Windows.Forms.TabPage();
      this.transSplitter = new System.Windows.Forms.SplitContainer();
      this.dictResults = new Jappy.RicherTextBox();
      this.dictDetails = new Jappy.RicherTextBox();
      this.dictInput = new System.Windows.Forms.TextBox();
      this.translatePage = new System.Windows.Forms.TabPage();
      this.transOutput = new Jappy.RicherTextBox();
      this.transInput = new System.Windows.Forms.TextBox();
      this.studyPage = new System.Windows.Forms.TabPage();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      menuStrip = new System.Windows.Forms.MenuStrip();
      fileMenu = new System.Windows.Forms.ToolStripMenuItem();
      statusBar = new System.Windows.Forms.StatusStrip();
      exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      menuStrip.SuspendLayout();
      statusBar.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.dictionaryPage.SuspendLayout();
      this.transSplitter.Panel1.SuspendLayout();
      this.transSplitter.Panel2.SuspendLayout();
      this.transSplitter.SuspendLayout();
      this.translatePage.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip
      // 
      menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileMenu});
      menuStrip.Location = new System.Drawing.Point(0, 0);
      menuStrip.Name = "menuStrip";
      menuStrip.Size = new System.Drawing.Size(599, 24);
      menuStrip.TabIndex = 0;
      // 
      // fileMenu
      // 
      fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            exitMenuItem});
      fileMenu.Name = "fileMenu";
      fileMenu.Size = new System.Drawing.Size(35, 20);
      fileMenu.Text = "&File";
      // 
      // statusBar
      // 
      statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
      statusBar.Location = new System.Drawing.Point(0, 383);
      statusBar.Name = "statusBar";
      statusBar.Size = new System.Drawing.Size(599, 22);
      statusBar.TabIndex = 3;
      // 
      // statusText
      // 
      this.statusText.AutoSize = false;
      this.statusText.Name = "statusText";
      this.statusText.Size = new System.Drawing.Size(584, 17);
      this.statusText.Spring = true;
      this.statusText.Text = "Welcome to Jappy!";
      this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabControl
      // 
      this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
      this.tabControl.Controls.Add(this.dictionaryPage);
      this.tabControl.Controls.Add(this.translatePage);
      this.tabControl.Controls.Add(this.studyPage);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.HotTrack = true;
      this.tabControl.Location = new System.Drawing.Point(0, 24);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.ShowToolTips = true;
      this.tabControl.Size = new System.Drawing.Size(599, 359);
      this.tabControl.TabIndex = 4;
      this.tabControl.TabStop = false;
      // 
      // dictionaryPage
      // 
      this.dictionaryPage.Controls.Add(this.transSplitter);
      this.dictionaryPage.Controls.Add(this.dictInput);
      this.dictionaryPage.Font = new System.Drawing.Font("Verdana", 9F);
      this.dictionaryPage.Location = new System.Drawing.Point(4, 25);
      this.dictionaryPage.Name = "dictionaryPage";
      this.dictionaryPage.Size = new System.Drawing.Size(591, 330);
      this.dictionaryPage.TabIndex = 0;
      this.dictionaryPage.Text = "Dictionary";
      this.dictionaryPage.ToolTipText = "Look up words in the dictionary.";
      this.dictionaryPage.UseVisualStyleBackColor = true;
      // 
      // transSplitter
      // 
      this.transSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.transSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.transSplitter.Location = new System.Drawing.Point(0, 22);
      this.transSplitter.Name = "transSplitter";
      this.transSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // transSplitter.Panel1
      // 
      this.transSplitter.Panel1.Controls.Add(this.dictResults);
      // 
      // transSplitter.Panel2
      // 
      this.transSplitter.Panel2.Controls.Add(this.dictDetails);
      this.transSplitter.Size = new System.Drawing.Size(591, 308);
      this.transSplitter.SplitterDistance = 157;
      this.transSplitter.TabIndex = 6;
      this.transSplitter.TabStop = false;
      // 
      // dictResults
      // 
      this.dictResults.BackColor = System.Drawing.SystemColors.Window;
      this.dictResults.DetectUrls = false;
      this.dictResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dictResults.Location = new System.Drawing.Point(0, 0);
      this.dictResults.Name = "dictResults";
      this.dictResults.ReadOnly = true;
      this.dictResults.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.dictResults.Size = new System.Drawing.Size(591, 157);
      this.dictResults.TabIndex = 2;
      this.dictResults.TabStop = false;
      this.dictResults.Text = "";
      this.dictResults.MouseEnter += new System.EventHandler(this.dictResults_MouseEnter);
      this.dictResults.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_CommonKeyDown);
      this.dictResults.MouseLeave += new System.EventHandler(this.control_PopStatusText);
      // 
      // dictDetails
      // 
      this.dictDetails.BackColor = System.Drawing.SystemColors.Window;
      this.dictDetails.DetectUrls = false;
      this.dictDetails.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dictDetails.Location = new System.Drawing.Point(0, 0);
      this.dictDetails.Name = "dictDetails";
      this.dictDetails.ReadOnly = true;
      this.dictDetails.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.dictDetails.Size = new System.Drawing.Size(591, 147);
      this.dictDetails.TabIndex = 0;
      this.dictDetails.TabStop = false;
      this.dictDetails.Text = "";
      this.dictDetails.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_CommonKeyDown);
      // 
      // dictInput
      // 
      this.dictInput.BackColor = System.Drawing.SystemColors.Window;
      this.dictInput.Dock = System.Windows.Forms.DockStyle.Top;
      this.dictInput.Location = new System.Drawing.Point(0, 0);
      this.dictInput.Name = "dictInput";
      this.dictInput.Size = new System.Drawing.Size(591, 22);
      this.dictInput.TabIndex = 5;
      this.dictInput.MouseLeave += new System.EventHandler(this.control_PopStatusText);
      this.dictInput.MouseEnter += new System.EventHandler(this.dictInput_MouseEnter);
      this.dictInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dictInput_KeyPress);
      this.dictInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_CommonKeyDown);
      // 
      // translatePage
      // 
      this.translatePage.Controls.Add(this.transOutput);
      this.translatePage.Controls.Add(this.transInput);
      this.translatePage.Font = new System.Drawing.Font("Verdana", 9F);
      this.translatePage.Location = new System.Drawing.Point(4, 25);
      this.translatePage.Name = "translatePage";
      this.translatePage.Size = new System.Drawing.Size(591, 330);
      this.translatePage.TabIndex = 2;
      this.translatePage.Text = "Translate";
      this.translatePage.ToolTipText = "Translate words in Japanese text.";
      this.translatePage.UseVisualStyleBackColor = true;
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
      this.transOutput.Size = new System.Drawing.Size(591, 259);
      this.transOutput.TabIndex = 1;
      this.transOutput.TabStop = false;
      this.transOutput.Text = "";
      this.transOutput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_CommonKeyDown);
      // 
      // transInput
      // 
      this.transInput.Dock = System.Windows.Forms.DockStyle.Top;
      this.transInput.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
      this.transInput.Location = new System.Drawing.Point(0, 0);
      this.transInput.Multiline = true;
      this.transInput.Name = "transInput";
      this.transInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.transInput.Size = new System.Drawing.Size(591, 71);
      this.transInput.TabIndex = 0;
      this.transInput.MouseLeave += new System.EventHandler(this.control_PopStatusText);
      this.transInput.MouseEnter += new System.EventHandler(this.transInput_MouseEnter);
      this.transInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_CommonKeyDown);
      // 
      // studyPage
      // 
      this.studyPage.Font = new System.Drawing.Font("Verdana", 9F);
      this.studyPage.Location = new System.Drawing.Point(4, 25);
      this.studyPage.Name = "studyPage";
      this.studyPage.Padding = new System.Windows.Forms.Padding(3);
      this.studyPage.Size = new System.Drawing.Size(591, 330);
      this.studyPage.TabIndex = 1;
      this.studyPage.Text = "Study";
      this.studyPage.ToolTipText = "Study words and kanji.";
      this.studyPage.UseVisualStyleBackColor = true;
      // 
      // toolTip
      // 
      this.toolTip.IsBalloon = true;
      // 
      // exitMenuItem
      // 
      exitMenuItem.Name = "exitMenuItem";
      exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      exitMenuItem.Size = new System.Drawing.Size(152, 22);
      exitMenuItem.Text = "E&xit";
      exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(599, 405);
      this.Controls.Add(this.tabControl);
      this.Controls.Add(statusBar);
      this.Controls.Add(menuStrip);
      this.MainMenuStrip = menuStrip;
      this.Name = "MainForm";
      this.Text = "Jappy";
      menuStrip.ResumeLayout(false);
      menuStrip.PerformLayout();
      statusBar.ResumeLayout(false);
      statusBar.PerformLayout();
      this.tabControl.ResumeLayout(false);
      this.dictionaryPage.ResumeLayout(false);
      this.dictionaryPage.PerformLayout();
      this.transSplitter.Panel1.ResumeLayout(false);
      this.transSplitter.Panel2.ResumeLayout(false);
      this.transSplitter.ResumeLayout(false);
      this.translatePage.ResumeLayout(false);
      this.translatePage.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStripStatusLabel statusText;
    private System.Windows.Forms.TabPage dictionaryPage;
    private System.Windows.Forms.SplitContainer transSplitter;
    private RicherTextBox dictResults;
    private System.Windows.Forms.TextBox dictInput;
    private System.Windows.Forms.TabPage studyPage;
    private RicherTextBox dictDetails;
    private System.Windows.Forms.TabPage translatePage;
    private System.Windows.Forms.TextBox transInput;
    private RicherTextBox transOutput;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.ToolTip toolTip;
  }
}