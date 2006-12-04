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
      System.Windows.Forms.SplitContainer dictSplitter;
      LinkLabel lblAdvanced;
      System.Windows.Forms.Label lblDictionary;
      this.cmbDictionary = new Jappy.DictionaryDropdown();
      this.chkCommon = new System.Windows.Forms.CheckBox();
      this.input = new System.Windows.Forms.TextBox();
      this.resultList = new Jappy.RicherTextBox();
      this.details = new Jappy.RicherTextBox();
      dictSplitter = new System.Windows.Forms.SplitContainer();
      lblAdvanced = new LinkLabel();
      lblDictionary = new System.Windows.Forms.Label();
      dictSplitter.Panel1.SuspendLayout();
      dictSplitter.Panel2.SuspendLayout();
      dictSplitter.SuspendLayout();
      this.SuspendLayout();
      // 
      // dictSplitter
      // 
      dictSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
      dictSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      dictSplitter.Location = new System.Drawing.Point(0, 0);
      dictSplitter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      dictSplitter.Name = "dictSplitter";
      dictSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // dictSplitter.Panel1
      // 
      dictSplitter.Panel1.Controls.Add(lblAdvanced);
      dictSplitter.Panel1.Controls.Add(lblDictionary);
      dictSplitter.Panel1.Controls.Add(this.cmbDictionary);
      dictSplitter.Panel1.Controls.Add(this.chkCommon);
      dictSplitter.Panel1.Controls.Add(this.input);
      dictSplitter.Panel1.Controls.Add(this.resultList);
      dictSplitter.Panel1MinSize = 100;
      // 
      // dictSplitter.Panel2
      // 
      dictSplitter.Panel2.Controls.Add(this.details);
      dictSplitter.Panel2MinSize = 100;
      dictSplitter.Size = new System.Drawing.Size(564, 430);
      dictSplitter.SplitterDistance = 325;
      dictSplitter.TabIndex = 7;
      dictSplitter.TabStop = false;
      // 
      // lblAdvanced
      // 
      lblAdvanced.Location = new System.Drawing.Point(444, 30);
      lblAdvanced.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblAdvanced.Name = "lblAdvanced";
      lblAdvanced.Size = new System.Drawing.Size(121, 15);
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
      lblDictionary.Text = "&Dictionary:";
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
      this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.input_KeyPress);
      // 
      // resultList
      // 
      this.resultList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.resultList.BackColor = System.Drawing.SystemColors.Window;
      this.resultList.DetectUrls = false;
      this.resultList.Location = new System.Drawing.Point(0, 52);
      this.resultList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.resultList.Name = "resultList";
      this.resultList.ReadOnly = true;
      this.resultList.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.resultList.Size = new System.Drawing.Size(562, 274);
      this.resultList.TabIndex = 2;
      this.resultList.TabStop = false;
      this.resultList.Text = "";
      // 
      // details
      // 
      this.details.BackColor = System.Drawing.SystemColors.Window;
      this.details.DetectUrls = false;
      this.details.Dock = System.Windows.Forms.DockStyle.Fill;
      this.details.Location = new System.Drawing.Point(0, 0);
      this.details.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.details.Name = "details";
      this.details.ReadOnly = true;
      this.details.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.details.Size = new System.Drawing.Size(564, 101);
      this.details.TabIndex = 3;
      this.details.TabStop = false;
      this.details.Text = "";
      // 
      // DictionarySearchTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(dictSplitter);
      this.Font = new System.Drawing.Font("Verdana", 9F);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "DictionarySearchTab";
      this.Size = new System.Drawing.Size(564, 430);
      dictSplitter.Panel1.ResumeLayout(false);
      dictSplitter.Panel1.PerformLayout();
      dictSplitter.Panel2.ResumeLayout(false);
      dictSplitter.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private DictionaryDropdown cmbDictionary;
    private System.Windows.Forms.CheckBox chkCommon;
    private System.Windows.Forms.TextBox input;
    private RicherTextBox resultList;
    private RicherTextBox details;
  }
}
