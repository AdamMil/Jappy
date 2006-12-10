namespace Jappy
{
  partial class DictionarySearchTab
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
      System.Windows.Forms.SplitContainer splitter;
      Jappy.LinkLabel lblAdvanced;
      System.Windows.Forms.Label lblDictionary;
      this.cmbDictionary = new Jappy.DictionaryDropdown();
      this.chkCommon = new System.Windows.Forms.CheckBox();
      this.input = new System.Windows.Forms.TextBox();
      this.resultList = new Jappy.DocumentRenderer();
      this.details = new Jappy.DocumentRenderer();
      splitter = new System.Windows.Forms.SplitContainer();
      lblAdvanced = new Jappy.LinkLabel();
      lblDictionary = new System.Windows.Forms.Label();
      splitter.Panel1.SuspendLayout();
      splitter.Panel2.SuspendLayout();
      splitter.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitter
      // 
      splitter.Dock = System.Windows.Forms.DockStyle.Fill;
      splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      splitter.Location = new System.Drawing.Point(0, 0);
      splitter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      splitter.Name = "splitter";
      splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitter.Panel1
      // 
      splitter.Panel1.Controls.Add(lblAdvanced);
      splitter.Panel1.Controls.Add(lblDictionary);
      splitter.Panel1.Controls.Add(this.cmbDictionary);
      splitter.Panel1.Controls.Add(this.chkCommon);
      splitter.Panel1.Controls.Add(this.input);
      splitter.Panel1.Controls.Add(this.resultList);
      splitter.Panel1MinSize = 100;
      // 
      // splitter.Panel2
      // 
      splitter.Panel2.Controls.Add(this.details);
      splitter.Panel2MinSize = 100;
      splitter.Size = new System.Drawing.Size(564, 430);
      splitter.SplitterDistance = 229;
      splitter.TabIndex = 7;
      splitter.TabStop = false;
      // 
      // lblAdvanced
      // 
      lblAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
      lblAdvanced.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Underline);
      lblAdvanced.ForeColor = System.Drawing.Color.Blue;
      lblAdvanced.Location = new System.Drawing.Point(444, 30);
      lblAdvanced.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblAdvanced.Name = "lblAdvanced";
      lblAdvanced.Size = new System.Drawing.Size(116, 15);
      lblAdvanced.TabIndex = 7;
      lblAdvanced.Text = "Advanced Search";
      lblAdvanced.Click += new System.EventHandler(this.lblAdvanced_Click);
      // 
      // lblDictionary
      // 
      lblDictionary.Location = new System.Drawing.Point(198, 28);
      lblDictionary.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblDictionary.Name = "lblDictionary";
      lblDictionary.Size = new System.Drawing.Size(76, 19);
      lblDictionary.TabIndex = 6;
      lblDictionary.Text = "Dictionary:";
      lblDictionary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // cmbDictionary
      // 
      this.cmbDictionary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDictionary.Location = new System.Drawing.Point(275, 27);
      this.cmbDictionary.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cmbDictionary.Name = "cmbDictionary";
      this.cmbDictionary.Size = new System.Drawing.Size(160, 22);
      this.cmbDictionary.TabIndex = 5;
      this.cmbDictionary.TabStop = false;
      // 
      // chkCommon
      // 
      this.chkCommon.AutoSize = true;
      this.chkCommon.Checked = true;
      this.chkCommon.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkCommon.Location = new System.Drawing.Point(2, 28);
      this.chkCommon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.chkCommon.Name = "chkCommon";
      this.chkCommon.Size = new System.Drawing.Size(189, 18);
      this.chkCommon.TabIndex = 4;
      this.chkCommon.TabStop = false;
      this.chkCommon.Text = "Restrict to &common words";
      this.chkCommon.UseVisualStyleBackColor = true;
      // 
      // input
      // 
      this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.input.BackColor = System.Drawing.SystemColors.Window;
      this.input.Location = new System.Drawing.Point(0, 0);
      this.input.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.input.Name = "input";
      this.input.Size = new System.Drawing.Size(562, 22);
      this.input.TabIndex = 3;
      this.input.MouseLeave += new System.EventHandler(this.common_MouseLeave);
      this.input.MouseEnter += new System.EventHandler(this.input_MouseEnter);
      this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.input_KeyPress);
      this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.input_KeyDown);
      // 
      // resultList
      // 
      this.resultList.AllowSelection = true;
      this.resultList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.resultList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.resultList.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.resultList.Location = new System.Drawing.Point(0, 52);
      this.resultList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.resultList.Name = "resultList";
      this.resultList.SelectionLength = 0;
      this.resultList.SelectionStart = 0;
      this.resultList.Size = new System.Drawing.Size(562, 178);
      this.resultList.TabIndex = 2;
      this.resultList.TabStop = false;
      this.resultList.MouseLeave += new System.EventHandler(this.common_MouseLeave);
      this.resultList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.output_MouseClick);
      this.resultList.MouseEnter += new System.EventHandler(this.resultList_MouseEnter);
      // 
      // details
      // 
      this.details.AllowSelection = true;
      this.details.BackColor = System.Drawing.SystemColors.Window;
      this.details.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.details.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.details.Dock = System.Windows.Forms.DockStyle.Fill;
      this.details.Location = new System.Drawing.Point(0, 0);
      this.details.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.details.Name = "details";
      this.details.SelectionLength = 0;
      this.details.SelectionStart = 0;
      this.details.Size = new System.Drawing.Size(564, 197);
      this.details.TabIndex = 3;
      this.details.TabStop = false;
      this.details.MouseClick += new System.Windows.Forms.MouseEventHandler(this.output_MouseClick);
      // 
      // DictionarySearchTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(splitter);
      this.Font = new System.Drawing.Font("Verdana", 9F);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "DictionarySearchTab";
      this.Size = new System.Drawing.Size(564, 430);
      splitter.Panel1.ResumeLayout(false);
      splitter.Panel1.PerformLayout();
      splitter.Panel2.ResumeLayout(false);
      splitter.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private DictionaryDropdown cmbDictionary;
    private System.Windows.Forms.CheckBox chkCommon;
    private System.Windows.Forms.TextBox input;
    private DocumentRenderer resultList;
    private DocumentRenderer details;
  }
}
