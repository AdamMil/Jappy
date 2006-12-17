namespace Jappy
{
  partial class StudyListDialog
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
      System.Windows.Forms.Label lblName;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.txtName = new System.Windows.Forms.TextBox();
      this.chkReadings = new System.Windows.Forms.CheckBox();
      this.chkExample = new System.Windows.Forms.CheckBox();
      lblName = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblName
      // 
      lblName.Location = new System.Drawing.Point(2, 7);
      lblName.Name = "lblName";
      lblName.Size = new System.Drawing.Size(50, 22);
      lblName.TabIndex = 0;
      lblName.Text = "&Name:";
      lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // btnOK
      // 
      btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      btnOK.Location = new System.Drawing.Point(7, 86);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 2;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(89, 86);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // txtName
      // 
      this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtName.Location = new System.Drawing.Point(52, 7);
      this.txtName.Name = "txtName";
      this.txtName.Size = new System.Drawing.Size(230, 22);
      this.txtName.TabIndex = 1;
      // 
      // chkReadings
      // 
      this.chkReadings.AutoSize = true;
      this.chkReadings.Location = new System.Drawing.Point(7, 35);
      this.chkReadings.Name = "chkReadings";
      this.chkReadings.Size = new System.Drawing.Size(199, 18);
      this.chkReadings.TabIndex = 4;
      this.chkReadings.Text = "Show &readings with phrase";
      this.chkReadings.UseVisualStyleBackColor = true;
      // 
      // chkExample
      // 
      this.chkExample.AutoSize = true;
      this.chkExample.Location = new System.Drawing.Point(7, 59);
      this.chkExample.Name = "chkExample";
      this.chkExample.Size = new System.Drawing.Size(197, 18);
      this.chkExample.TabIndex = 5;
      this.chkExample.Text = "Show &example with phrase";
      this.chkExample.UseVisualStyleBackColor = true;
      // 
      // StudyListDialog
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(288, 113);
      this.Controls.Add(this.chkExample);
      this.Controls.Add(this.chkReadings);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.txtName);
      this.Controls.Add(lblName);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StudyListDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Study List Properties";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.CheckBox chkReadings;
    private System.Windows.Forms.CheckBox chkExample;

  }
}