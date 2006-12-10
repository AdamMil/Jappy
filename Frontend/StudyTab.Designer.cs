namespace Jappy
{
  partial class StudyTab
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
      System.Windows.Forms.ToolStripMenuItem goMenuItem;
      System.Windows.Forms.ToolStripMenuItem fileMenu;
      System.Windows.Forms.ToolStripSeparator menuSep2;
      System.Windows.Forms.ToolStripSeparator menuSep3;
      this.loadStudylistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveListAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.studyMenu = new System.Windows.Forms.ToolStripMenuItem();
      this.output = new Jappy.DocumentRenderer();
      settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      goMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fileMenu = new System.Windows.Forms.ToolStripMenuItem();
      menuSep2 = new System.Windows.Forms.ToolStripSeparator();
      menuSep3 = new System.Windows.Forms.ToolStripSeparator();
      this.menuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // settingsMenuItem
      // 
      settingsMenuItem.Name = "settingsMenuItem";
      settingsMenuItem.Size = new System.Drawing.Size(175, 22);
      settingsMenuItem.Text = "List &settings...";
      // 
      // goMenuItem
      // 
      goMenuItem.Name = "goMenuItem";
      goMenuItem.Size = new System.Drawing.Size(175, 22);
      goMenuItem.Text = "&Go!";
      // 
      // fileMenu
      // 
      fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadStudylistToolStripMenuItem,
            menuSep2,
            this.saveListMenuItem,
            this.saveListAsMenuItem,
            menuSep3});
      fileMenu.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
      fileMenu.Name = "fileMenu";
      fileMenu.Size = new System.Drawing.Size(35, 20);
      fileMenu.Text = "&File";
      // 
      // loadStudylistToolStripMenuItem
      // 
      this.loadStudylistToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
      this.loadStudylistToolStripMenuItem.MergeIndex = 1;
      this.loadStudylistToolStripMenuItem.Name = "loadStudylistToolStripMenuItem";
      this.loadStudylistToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.loadStudylistToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
      this.loadStudylistToolStripMenuItem.Text = "Load study &list...";
      this.loadStudylistToolStripMenuItem.Click += new System.EventHandler(this.loadStudylistToolStripMenuItem_Click);
      // 
      // menuSep2
      // 
      menuSep2.MergeAction = System.Windows.Forms.MergeAction.Insert;
      menuSep2.MergeIndex = 2;
      menuSep2.Name = "menuSep2";
      menuSep2.Size = new System.Drawing.Size(205, 6);
      // 
      // saveListMenuItem
      // 
      this.saveListMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
      this.saveListMenuItem.MergeIndex = 3;
      this.saveListMenuItem.Name = "saveListMenuItem";
      this.saveListMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      this.saveListMenuItem.Size = new System.Drawing.Size(208, 22);
      this.saveListMenuItem.Text = "&Save study list";
      this.saveListMenuItem.Click += new System.EventHandler(this.saveListMenuItem_Click);
      // 
      // saveListAsMenuItem
      // 
      this.saveListAsMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
      this.saveListAsMenuItem.MergeIndex = 4;
      this.saveListAsMenuItem.Name = "saveListAsMenuItem";
      this.saveListAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
      this.saveListAsMenuItem.Size = new System.Drawing.Size(208, 22);
      this.saveListAsMenuItem.Text = "Save list &as...";
      this.saveListAsMenuItem.Click += new System.EventHandler(this.saveListAsMenuItem_Click);
      // 
      // menuSep3
      // 
      menuSep3.MergeAction = System.Windows.Forms.MergeAction.Insert;
      menuSep3.MergeIndex = 5;
      menuSep3.Name = "menuSep3";
      menuSep3.Size = new System.Drawing.Size(205, 6);
      // 
      // menuStrip
      // 
      this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
      this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileMenu,
            this.studyMenu});
      this.menuStrip.Location = new System.Drawing.Point(0, 0);
      this.menuStrip.Name = "menuStrip";
      this.menuStrip.Size = new System.Drawing.Size(90, 24);
      this.menuStrip.TabIndex = 0;
      this.menuStrip.Visible = false;
      // 
      // studyMenu
      // 
      this.studyMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            settingsMenuItem,
            goMenuItem});
      this.studyMenu.Name = "studyMenu";
      this.studyMenu.Size = new System.Drawing.Size(47, 20);
      this.studyMenu.Text = "&Study";
      // 
      // output
      // 
      this.output.AllowSelection = true;
      this.output.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.output.Location = new System.Drawing.Point(0, 59);
      this.output.Name = "output";
      this.output.SelectionLength = 0;
      this.output.SelectionStart = 0;
      this.output.Size = new System.Drawing.Size(266, 91);
      this.output.TabIndex = 1;
      this.output.Text = "documentRenderer1";
      this.output.MouseLeave += new System.EventHandler(this.output_MouseLeave);
      this.output.MouseClick += new System.Windows.Forms.MouseEventHandler(this.output_MouseClick);
      this.output.MouseEnter += new System.EventHandler(this.output_MouseEnter);
      // 
      // StudyTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.output);
      this.Controls.Add(this.menuStrip);
      this.Name = "StudyTab";
      this.Size = new System.Drawing.Size(266, 150);
      this.menuStrip.ResumeLayout(false);
      this.menuStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStripMenuItem studyMenu;
    private System.Windows.Forms.ToolStripMenuItem loadStudylistToolStripMenuItem;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem saveListMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveListAsMenuItem;
    private DocumentRenderer output;
  }
}
