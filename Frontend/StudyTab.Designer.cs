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
      System.Windows.Forms.ToolStripMenuItem fileMenu;
      System.Windows.Forms.ToolStripMenuItem newListMenu;
      System.Windows.Forms.ToolStripMenuItem newEmptyListMenuItem;
      System.Windows.Forms.ToolStripSeparator menuSep4;
      System.Windows.Forms.ToolStripMenuItem fromLevel1KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel2KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel3KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel4KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel5KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel6KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem fromLevel7KanjiToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem loadStudylistMenuItem;
      System.Windows.Forms.ToolStripSeparator menuSep2;
      System.Windows.Forms.ToolStripSeparator menuSep3;
      System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
      System.Windows.Forms.ToolStripMenuItem goMenuItem;
      this.saveListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveListAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.output = new Jappy.DocumentRenderer();
      this.menuStrip = new System.Windows.Forms.MenuStrip();
      this.studyMenu = new System.Windows.Forms.ToolStripMenuItem();
      this.newEntryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fileMenu = new System.Windows.Forms.ToolStripMenuItem();
      newListMenu = new System.Windows.Forms.ToolStripMenuItem();
      newEmptyListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      menuSep4 = new System.Windows.Forms.ToolStripSeparator();
      fromLevel1KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel2KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel3KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel4KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel5KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel6KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      fromLevel7KanjiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      loadStudylistMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      menuSep2 = new System.Windows.Forms.ToolStripSeparator();
      menuSep3 = new System.Windows.Forms.ToolStripSeparator();
      settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      goMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // fileMenu
      // 
      fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            newListMenu,
            loadStudylistMenuItem,
            menuSep2,
            this.saveListMenuItem,
            this.saveListAsMenuItem,
            menuSep3});
      fileMenu.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
      fileMenu.Name = "fileMenu";
      fileMenu.Size = new System.Drawing.Size(35, 20);
      fileMenu.Text = "&File";
      // 
      // newListMenu
      // 
      newListMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            newEmptyListMenuItem,
            menuSep4,
            fromLevel1KanjiToolStripMenuItem,
            fromLevel2KanjiToolStripMenuItem,
            fromLevel3KanjiToolStripMenuItem,
            fromLevel4KanjiToolStripMenuItem,
            fromLevel5KanjiToolStripMenuItem,
            fromLevel6KanjiToolStripMenuItem,
            fromLevel7KanjiToolStripMenuItem});
      newListMenu.MergeAction = System.Windows.Forms.MergeAction.Insert;
      newListMenu.MergeIndex = 0;
      newListMenu.Name = "newListMenu";
      newListMenu.Size = new System.Drawing.Size(208, 22);
      newListMenu.Text = "&New study list";
      // 
      // newEmptyListMenuItem
      // 
      newEmptyListMenuItem.Name = "newEmptyListMenuItem";
      newEmptyListMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
      newEmptyListMenuItem.Size = new System.Drawing.Size(201, 22);
      newEmptyListMenuItem.Text = "&Empty list...";
      newEmptyListMenuItem.Click += new System.EventHandler(this.newEmptyListMenuItem_Click);
      // 
      // menuSep4
      // 
      menuSep4.Name = "menuSep4";
      menuSep4.Size = new System.Drawing.Size(198, 6);
      // 
      // fromLevel1KanjiToolStripMenuItem
      // 
      fromLevel1KanjiToolStripMenuItem.Name = "fromLevel1KanjiToolStripMenuItem";
      fromLevel1KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel1KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.First;
      fromLevel1KanjiToolStripMenuItem.Text = "From level 1 kanji";
      fromLevel1KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel2KanjiToolStripMenuItem
      // 
      fromLevel2KanjiToolStripMenuItem.Name = "fromLevel2KanjiToolStripMenuItem";
      fromLevel2KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel2KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.Second;
      fromLevel2KanjiToolStripMenuItem.Text = "From level 2 kanji";
      fromLevel2KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel3KanjiToolStripMenuItem
      // 
      fromLevel3KanjiToolStripMenuItem.Name = "fromLevel3KanjiToolStripMenuItem";
      fromLevel3KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel3KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.Third;
      fromLevel3KanjiToolStripMenuItem.Text = "From level 3 kanji";
      fromLevel3KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel4KanjiToolStripMenuItem
      // 
      fromLevel4KanjiToolStripMenuItem.Name = "fromLevel4KanjiToolStripMenuItem";
      fromLevel4KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel4KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.Fourth;
      fromLevel4KanjiToolStripMenuItem.Text = "From level 4 kanji";
      fromLevel4KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel5KanjiToolStripMenuItem
      // 
      fromLevel5KanjiToolStripMenuItem.Name = "fromLevel5KanjiToolStripMenuItem";
      fromLevel5KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel5KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.Fifth;
      fromLevel5KanjiToolStripMenuItem.Text = "From level 5 kanji";
      fromLevel5KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel6KanjiToolStripMenuItem
      // 
      fromLevel6KanjiToolStripMenuItem.Name = "fromLevel6KanjiToolStripMenuItem";
      fromLevel6KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel6KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.Sixth;
      fromLevel6KanjiToolStripMenuItem.Text = "From level 6 kanji";
      fromLevel6KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // fromLevel7KanjiToolStripMenuItem
      // 
      fromLevel7KanjiToolStripMenuItem.Name = "fromLevel7KanjiToolStripMenuItem";
      fromLevel7KanjiToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
      fromLevel7KanjiToolStripMenuItem.Tag = Jappy.Backend.Level.HighSchool;
      fromLevel7KanjiToolStripMenuItem.Text = "From high school kanji";
      fromLevel7KanjiToolStripMenuItem.Click += new System.EventHandler(this.newListFromKanji_Click);
      // 
      // loadStudylistMenuItem
      // 
      loadStudylistMenuItem.MergeAction = System.Windows.Forms.MergeAction.Insert;
      loadStudylistMenuItem.MergeIndex = 1;
      loadStudylistMenuItem.Name = "loadStudylistMenuItem";
      loadStudylistMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      loadStudylistMenuItem.Size = new System.Drawing.Size(208, 22);
      loadStudylistMenuItem.Text = "Load study &list...";
      loadStudylistMenuItem.Click += new System.EventHandler(this.loadStudylistMenuItem_Click);
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
      // settingsMenuItem
      // 
      settingsMenuItem.Name = "settingsMenuItem";
      settingsMenuItem.Size = new System.Drawing.Size(175, 22);
      settingsMenuItem.Text = "List &settings...";
      settingsMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
      // 
      // goMenuItem
      // 
      goMenuItem.Name = "goMenuItem";
      goMenuItem.Size = new System.Drawing.Size(175, 22);
      goMenuItem.Text = "&Go!";
      // 
      // output
      // 
      this.output.AllowSelection = true;
      this.output.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.output.Dock = System.Windows.Forms.DockStyle.Fill;
      this.output.Location = new System.Drawing.Point(0, 0);
      this.output.Name = "output";
      this.output.SelectionLength = 0;
      this.output.SelectionStart = 0;
      this.output.Size = new System.Drawing.Size(266, 150);
      this.output.TabIndex = 0;
      this.output.MouseLeave += new System.EventHandler(this.output_MouseLeave);
      this.output.MouseClick += new System.Windows.Forms.MouseEventHandler(this.output_MouseClick);
      this.output.MouseEnter += new System.EventHandler(this.output_MouseEnter);
      // 
      // menuStrip
      // 
      this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
      this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileMenu,
            this.studyMenu});
      this.menuStrip.Location = new System.Drawing.Point(0, 0);
      this.menuStrip.Name = "menuStrip";
      this.menuStrip.Size = new System.Drawing.Size(182, 24);
      this.menuStrip.TabIndex = 2;
      this.menuStrip.Visible = false;
      // 
      // studyMenu
      // 
      this.studyMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newEntryMenuItem,
            settingsMenuItem,
            goMenuItem});
      this.studyMenu.Name = "studyMenu";
      this.studyMenu.Size = new System.Drawing.Size(47, 20);
      this.studyMenu.Text = "&Study";
      // 
      // newEntryMenuItem
      // 
      this.newEntryMenuItem.Name = "newEntryMenuItem";
      this.newEntryMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
      this.newEntryMenuItem.Size = new System.Drawing.Size(175, 22);
      this.newEntryMenuItem.Text = "&New entry...";
      this.newEntryMenuItem.Click += new System.EventHandler(this.newEntryMenuItem_Click);
      // 
      // StudyTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.menuStrip);
      this.Controls.Add(this.output);
      this.Name = "StudyTab";
      this.Size = new System.Drawing.Size(266, 150);
      this.menuStrip.ResumeLayout(false);
      this.menuStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DocumentRenderer output;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem saveListMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveListAsMenuItem;
    private System.Windows.Forms.ToolStripMenuItem studyMenu;
    private System.Windows.Forms.ToolStripMenuItem newEntryMenuItem;

  }
}
