namespace Jappy
{
  partial class AdvancedDictionarySearchDialog
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
      System.Windows.Forms.Button btnSearch;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Label lblDictionary;
      System.Windows.Forms.Label lblFrequency;
      System.Windows.Forms.Label lblPOS;
      System.Windows.Forms.Label lblItemLimit;
      this.txtQuery = new System.Windows.Forms.TextBox();
      this.txtFrequency = new System.Windows.Forms.TextBox();
      this.pos = new System.Windows.Forms.CheckedListBox();
      this.txtLimit = new System.Windows.Forms.TextBox();
      this.cmbDictionary = new Jappy.DictionaryDropdown();
      btnSearch = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      lblDictionary = new System.Windows.Forms.Label();
      lblFrequency = new System.Windows.Forms.Label();
      lblPOS = new System.Windows.Forms.Label();
      lblItemLimit = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnSearch
      // 
      btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnSearch.DialogResult = System.Windows.Forms.DialogResult.OK;
      btnSearch.Location = new System.Drawing.Point(4, 242);
      btnSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnSearch.Name = "btnSearch";
      btnSearch.Size = new System.Drawing.Size(100, 25);
      btnSearch.TabIndex = 5;
      btnSearch.Text = "&Search";
      btnSearch.UseVisualStyleBackColor = true;
      btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(112, 242);
      btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(100, 25);
      btnCancel.TabIndex = 6;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // lblDictionary
      // 
      lblDictionary.Location = new System.Drawing.Point(77, 33);
      lblDictionary.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblDictionary.Name = "lblDictionary";
      lblDictionary.Size = new System.Drawing.Size(76, 19);
      lblDictionary.TabIndex = 1;
      lblDictionary.Text = "Dictionary:";
      lblDictionary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblFrequency
      // 
      lblFrequency.Location = new System.Drawing.Point(4, 59);
      lblFrequency.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblFrequency.Name = "lblFrequency";
      lblFrequency.Size = new System.Drawing.Size(149, 19);
      lblFrequency.TabIndex = 2;
      lblFrequency.Text = "Frequency Threshold:";
      lblFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblPOS
      // 
      lblPOS.AutoSize = true;
      lblPOS.Location = new System.Drawing.Point(4, 110);
      lblPOS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblPOS.Name = "lblPOS";
      lblPOS.Size = new System.Drawing.Size(106, 14);
      lblPOS.TabIndex = 4;
      lblPOS.Text = "Parts of Speech";
      lblPOS.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // lblItemLimit
      // 
      lblItemLimit.Location = new System.Drawing.Point(69, 87);
      lblItemLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblItemLimit.Name = "lblItemLimit";
      lblItemLimit.Size = new System.Drawing.Size(85, 19);
      lblItemLimit.TabIndex = 3;
      lblItemLimit.Text = "Item Limit:";
      lblItemLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // txtQuery
      // 
      this.txtQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtQuery.Location = new System.Drawing.Point(4, 5);
      this.txtQuery.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtQuery.Name = "txtQuery";
      this.txtQuery.Size = new System.Drawing.Size(312, 22);
      this.txtQuery.TabIndex = 0;
      // 
      // txtFrequency
      // 
      this.txtFrequency.Location = new System.Drawing.Point(156, 57);
      this.txtFrequency.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtFrequency.Name = "txtFrequency";
      this.txtFrequency.Size = new System.Drawing.Size(47, 22);
      this.txtFrequency.TabIndex = 2;
      this.txtFrequency.Text = "0";
      // 
      // pos
      // 
      this.pos.CheckOnClick = true;
      this.pos.Location = new System.Drawing.Point(4, 130);
      this.pos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.pos.Name = "pos";
      this.pos.Size = new System.Drawing.Size(309, 106);
      this.pos.TabIndex = 4;
      // 
      // txtLimit
      // 
      this.txtLimit.Location = new System.Drawing.Point(156, 85);
      this.txtLimit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtLimit.Name = "txtLimit";
      this.txtLimit.Size = new System.Drawing.Size(47, 22);
      this.txtLimit.TabIndex = 3;
      this.txtLimit.Text = "100";
      // 
      // cmbDictionary
      // 
      this.cmbDictionary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDictionary.Location = new System.Drawing.Point(156, 31);
      this.cmbDictionary.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.cmbDictionary.Name = "cmbDictionary";
      this.cmbDictionary.Size = new System.Drawing.Size(160, 22);
      this.cmbDictionary.TabIndex = 1;
      // 
      // AdvancedDictionarySearchDialog
      // 
      this.AcceptButton = btnSearch;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(319, 270);
      this.Controls.Add(this.txtLimit);
      this.Controls.Add(lblItemLimit);
      this.Controls.Add(lblPOS);
      this.Controls.Add(this.pos);
      this.Controls.Add(this.txtFrequency);
      this.Controls.Add(lblFrequency);
      this.Controls.Add(this.cmbDictionary);
      this.Controls.Add(lblDictionary);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnSearch);
      this.Controls.Add(this.txtQuery);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AdvancedDictionarySearchDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Advanced Dictionary Search";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtQuery;
    private DictionaryDropdown cmbDictionary;
    private System.Windows.Forms.TextBox txtFrequency;
    private System.Windows.Forms.CheckedListBox pos;
    private System.Windows.Forms.TextBox txtLimit;
  }
}