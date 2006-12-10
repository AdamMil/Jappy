namespace Jappy
{
  partial class StudyListNameDialog
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
      System.Windows.Forms.Label lblHelpText;
      System.Windows.Forms.Button btnOK;
      System.Windows.Forms.Button btnCancel;
      this.txtName = new System.Windows.Forms.TextBox();
      lblHelpText = new System.Windows.Forms.Label();
      btnOK = new System.Windows.Forms.Button();
      btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblHelpText
      // 
      lblHelpText.AutoSize = true;
      lblHelpText.Location = new System.Drawing.Point(6, 3);
      lblHelpText.Name = "lblHelpText";
      lblHelpText.Size = new System.Drawing.Size(205, 14);
      lblHelpText.TabIndex = 0;
      lblHelpText.Text = "Enter a name for this study list.";
      // 
      // btnOK
      // 
      btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      btnOK.Location = new System.Drawing.Point(7, 50);
      btnOK.Name = "btnOK";
      btnOK.Size = new System.Drawing.Size(75, 23);
      btnOK.TabIndex = 2;
      btnOK.Text = "&OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      btnCancel.Location = new System.Drawing.Point(89, 50);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new System.Drawing.Size(75, 23);
      btnCancel.TabIndex = 3;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      // 
      // txtName
      // 
      this.txtName.Location = new System.Drawing.Point(7, 21);
      this.txtName.Name = "txtName";
      this.txtName.Size = new System.Drawing.Size(202, 22);
      this.txtName.TabIndex = 1;
      // 
      // StudyListNameDialog
      // 
      this.AcceptButton = btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = btnCancel;
      this.ClientSize = new System.Drawing.Size(217, 77);
      this.Controls.Add(btnCancel);
      this.Controls.Add(btnOK);
      this.Controls.Add(this.txtName);
      this.Controls.Add(lblHelpText);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StudyListNameDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Study List Name";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtName;

  }
}