namespace Jappy
{
  partial class FindDialog
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
      System.Windows.Forms.Label lblSearchFor;
      System.Windows.Forms.Button btnFind;
      System.Windows.Forms.Button btnClose;
      this.txtSearchFor = new System.Windows.Forms.TextBox();
      this.chkMatchCase = new System.Windows.Forms.CheckBox();
      lblSearchFor = new System.Windows.Forms.Label();
      btnFind = new System.Windows.Forms.Button();
      btnClose = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblSearchFor
      // 
      lblSearchFor.AutoSize = true;
      lblSearchFor.Location = new System.Drawing.Point(4, 7);
      lblSearchFor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblSearchFor.Name = "lblSearchFor";
      lblSearchFor.Size = new System.Drawing.Size(76, 14);
      lblSearchFor.TabIndex = 0;
      lblSearchFor.Text = "Search for:";
      // 
      // btnFind
      // 
      btnFind.Location = new System.Drawing.Point(375, 2);
      btnFind.Name = "btnFind";
      btnFind.Size = new System.Drawing.Size(75, 23);
      btnFind.TabIndex = 2;
      btnFind.Text = "&Find";
      btnFind.UseVisualStyleBackColor = true;
      btnFind.Click += new System.EventHandler(this.btnFind_Click);
      // 
      // btnClose
      // 
      btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnClose.Location = new System.Drawing.Point(375, 31);
      btnClose.Name = "btnClose";
      btnClose.Size = new System.Drawing.Size(75, 23);
      btnClose.TabIndex = 3;
      btnClose.Text = "Close";
      btnClose.UseVisualStyleBackColor = true;
      btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // txtSearchFor
      // 
      this.txtSearchFor.Location = new System.Drawing.Point(83, 3);
      this.txtSearchFor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.txtSearchFor.Name = "txtSearchFor";
      this.txtSearchFor.Size = new System.Drawing.Size(285, 22);
      this.txtSearchFor.TabIndex = 1;
      this.txtSearchFor.TextChanged += new System.EventHandler(this.txtSearchFor_TextChanged);
      // 
      // chkMatchCase
      // 
      this.chkMatchCase.AutoSize = true;
      this.chkMatchCase.Location = new System.Drawing.Point(8, 31);
      this.chkMatchCase.Name = "chkMatchCase";
      this.chkMatchCase.Size = new System.Drawing.Size(96, 18);
      this.chkMatchCase.TabIndex = 4;
      this.chkMatchCase.Text = "Match &case";
      this.chkMatchCase.UseVisualStyleBackColor = true;
      // 
      // FindDialog
      // 
      this.AcceptButton = btnFind;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnClose;
      this.ClientSize = new System.Drawing.Size(455, 59);
      this.Controls.Add(btnClose);
      this.Controls.Add(btnFind);
      this.Controls.Add(this.chkMatchCase);
      this.Controls.Add(this.txtSearchFor);
      this.Controls.Add(lblSearchFor);
      this.Font = new System.Drawing.Font("Verdana", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FindDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Find";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtSearchFor;
    private System.Windows.Forms.CheckBox chkMatchCase;

  }
}