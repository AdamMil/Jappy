namespace Jappy
{
  partial class StudyListEntryDialog
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
      System.Windows.Forms.Label lblPhrase;
      System.Windows.Forms.Label lblReadings;
      System.Windows.Forms.Label lblMeanings;
      System.Windows.Forms.Label lblJpExample;
      System.Windows.Forms.Label lblEnExample;
      System.Windows.Forms.Button btnSave;
      System.Windows.Forms.Button btnCancel;
      System.Windows.Forms.Button btnReset;
      this.txtPhrase = new System.Windows.Forms.TextBox();
      this.txtReadings = new System.Windows.Forms.TextBox();
      this.txtMeanings = new System.Windows.Forms.TextBox();
      this.txtJpExample = new System.Windows.Forms.TextBox();
      this.txtEnExample = new System.Windows.Forms.TextBox();
      this.lblSuccess = new System.Windows.Forms.Label();
      lblPhrase = new System.Windows.Forms.Label();
      lblReadings = new System.Windows.Forms.Label();
      lblMeanings = new System.Windows.Forms.Label();
      lblJpExample = new System.Windows.Forms.Label();
      lblEnExample = new System.Windows.Forms.Label();
      btnSave = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      btnReset = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblPhrase
      // 
      lblPhrase.Location = new System.Drawing.Point(2, 10);
      lblPhrase.Name = "lblPhrase";
      lblPhrase.Size = new System.Drawing.Size(72, 13);
      lblPhrase.TabIndex = 0;
      lblPhrase.Text = "Phrase:";
      lblPhrase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblReadings
      // 
      lblReadings.Location = new System.Drawing.Point(2, 37);
      lblReadings.Name = "lblReadings";
      lblReadings.Size = new System.Drawing.Size(72, 14);
      lblReadings.TabIndex = 2;
      lblReadings.Text = "Readings:";
      lblReadings.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblMeanings
      // 
      lblMeanings.Location = new System.Drawing.Point(2, 65);
      lblMeanings.Name = "lblMeanings";
      lblMeanings.Size = new System.Drawing.Size(72, 14);
      lblMeanings.TabIndex = 4;
      lblMeanings.Text = "Meanings:";
      lblMeanings.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblJpExample
      // 
      lblJpExample.Location = new System.Drawing.Point(2, 97);
      lblJpExample.Name = "lblJpExample";
      lblJpExample.Size = new System.Drawing.Size(129, 14);
      lblJpExample.TabIndex = 6;
      lblJpExample.Text = "Japanese example:";
      lblJpExample.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // lblEnExample
      // 
      lblEnExample.Location = new System.Drawing.Point(2, 151);
      lblEnExample.Name = "lblEnExample";
      lblEnExample.Size = new System.Drawing.Size(129, 14);
      lblEnExample.TabIndex = 8;
      lblEnExample.Text = "English example:";
      lblEnExample.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // btnSave
      // 
      btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      btnSave.Location = new System.Drawing.Point(5, 209);
      btnSave.Name = "btnSave";
      btnSave.Size = new System.Drawing.Size(75, 23);
      btnSave.TabIndex = 11;
      btnSave.Text = "&Save";
      btnSave.UseVisualStyleBackColor = true;
      btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(86, 208);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 12;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnReset
      // 
      btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnReset.Location = new System.Drawing.Point(306, 208);
      btnReset.Name = "btnReset";
      btnReset.Size = new System.Drawing.Size(75, 23);
      btnReset.TabIndex = 14;
      btnReset.Text = "Reset";
      btnReset.UseVisualStyleBackColor = true;
      btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // txtPhrase
      // 
      this.txtPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPhrase.Location = new System.Drawing.Point(73, 5);
      this.txtPhrase.Name = "txtPhrase";
      this.txtPhrase.Size = new System.Drawing.Size(308, 22);
      this.txtPhrase.TabIndex = 1;
      // 
      // txtReadings
      // 
      this.txtReadings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtReadings.Location = new System.Drawing.Point(73, 33);
      this.txtReadings.Name = "txtReadings";
      this.txtReadings.Size = new System.Drawing.Size(308, 22);
      this.txtReadings.TabIndex = 3;
      // 
      // txtMeanings
      // 
      this.txtMeanings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtMeanings.Location = new System.Drawing.Point(73, 61);
      this.txtMeanings.Name = "txtMeanings";
      this.txtMeanings.Size = new System.Drawing.Size(308, 22);
      this.txtMeanings.TabIndex = 5;
      // 
      // txtJpExample
      // 
      this.txtJpExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtJpExample.Location = new System.Drawing.Point(5, 114);
      this.txtJpExample.Multiline = true;
      this.txtJpExample.Name = "txtJpExample";
      this.txtJpExample.Size = new System.Drawing.Size(376, 34);
      this.txtJpExample.TabIndex = 7;
      // 
      // txtEnExample
      // 
      this.txtEnExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtEnExample.Location = new System.Drawing.Point(5, 168);
      this.txtEnExample.Multiline = true;
      this.txtEnExample.Name = "txtEnExample";
      this.txtEnExample.Size = new System.Drawing.Size(376, 34);
      this.txtEnExample.TabIndex = 10;
      // 
      // lblSuccess
      // 
      this.lblSuccess.Location = new System.Drawing.Point(169, 213);
      this.lblSuccess.Name = "lblSuccess";
      this.lblSuccess.Size = new System.Drawing.Size(131, 13);
      this.lblSuccess.TabIndex = 13;
      this.lblSuccess.Text = "Success 5 of 10";
      // 
      // StudyListEntryDialog
      // 
      this.AcceptButton = btnSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(385, 238);
      this.Controls.Add(btnReset);
      this.Controls.Add(this.lblSuccess);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnSave);
      this.Controls.Add(this.txtEnExample);
      this.Controls.Add(this.txtJpExample);
      this.Controls.Add(this.txtMeanings);
      this.Controls.Add(this.txtReadings);
      this.Controls.Add(this.txtPhrase);
      this.Controls.Add(lblEnExample);
      this.Controls.Add(lblJpExample);
      this.Controls.Add(lblMeanings);
      this.Controls.Add(lblReadings);
      this.Controls.Add(lblPhrase);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StudyListEntryDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Edit study list entry";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtPhrase;
    private System.Windows.Forms.TextBox txtReadings;
    private System.Windows.Forms.TextBox txtMeanings;
    private System.Windows.Forms.TextBox txtJpExample;
    private System.Windows.Forms.TextBox txtEnExample;
    private System.Windows.Forms.Label lblSuccess;
  }
}