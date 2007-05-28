namespace Jappy
{
  partial class StudyDialog
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
      System.Windows.Forms.Button btnNo;
      System.Windows.Forms.Button btnYes;
      System.Windows.Forms.Label lblDoYouKnow;
      System.Windows.Forms.Button btnShowAnswer;
      System.Windows.Forms.Button btnFinish;
      this.area = new Jappy.DocumentRenderer();
      btnNo = new System.Windows.Forms.Button();
      btnYes = new System.Windows.Forms.Button();
      lblDoYouKnow = new System.Windows.Forms.Label();
      btnShowAnswer = new System.Windows.Forms.Button();
      btnFinish = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnNo
      // 
      btnNo.Location = new System.Drawing.Point(435, 157);
      btnNo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnNo.Name = "btnNo";
      btnNo.Size = new System.Drawing.Size(59, 25);
      btnNo.TabIndex = 2;
      btnNo.Text = "&No";
      btnNo.UseVisualStyleBackColor = true;
      btnNo.Click += new System.EventHandler(this.btnNo_Click);
      // 
      // btnYes
      // 
      btnYes.Location = new System.Drawing.Point(369, 157);
      btnYes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnYes.Name = "btnYes";
      btnYes.Size = new System.Drawing.Size(59, 25);
      btnYes.TabIndex = 1;
      btnYes.Text = "&Yes";
      btnYes.UseVisualStyleBackColor = true;
      btnYes.Click += new System.EventHandler(this.btnYes_Click);
      // 
      // lblDoYouKnow
      // 
      lblDoYouKnow.AutoSize = true;
      lblDoYouKnow.Location = new System.Drawing.Point(0, 163);
      lblDoYouKnow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      lblDoYouKnow.Name = "lblDoYouKnow";
      lblDoYouKnow.Size = new System.Drawing.Size(365, 14);
      lblDoYouKnow.TabIndex = 0;
      lblDoYouKnow.Text = "Do you know the answer (press space to double check)?";
      lblDoYouKnow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnShowAnswer
      // 
      btnShowAnswer.Location = new System.Drawing.Point(500, 157);
      btnShowAnswer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnShowAnswer.Name = "btnShowAnswer";
      btnShowAnswer.Size = new System.Drawing.Size(111, 25);
      btnShowAnswer.TabIndex = 3;
      btnShowAnswer.Text = "Show &answer";
      btnShowAnswer.UseVisualStyleBackColor = true;
      btnShowAnswer.Click += new System.EventHandler(this.btnShowAnswer_Click);
      // 
      // btnFinish
      // 
      btnFinish.Location = new System.Drawing.Point(627, 157);
      btnFinish.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      btnFinish.Name = "btnFinish";
      btnFinish.Size = new System.Drawing.Size(111, 25);
      btnFinish.TabIndex = 4;
      btnFinish.Text = "&Stop studying";
      btnFinish.UseVisualStyleBackColor = true;
      btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
      // 
      // area
      // 
      this.area.AllowSelection = true;
      this.area.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.area.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.area.Location = new System.Drawing.Point(0, 0);
      this.area.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.area.Name = "area";
      this.area.SelectionLength = 0;
      this.area.SelectionStart = 0;
      this.area.Size = new System.Drawing.Size(741, 151);
      this.area.TabIndex = 0;
      this.area.TabStop = false;
      // 
      // StudyDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(741, 185);
      this.Controls.Add(btnShowAnswer);
      this.Controls.Add(btnNo);
      this.Controls.Add(btnYes);
      this.Controls.Add(lblDoYouKnow);
      this.Controls.Add(btnFinish);
      this.Controls.Add(this.area);
      this.Font = new System.Drawing.Font("Verdana", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StudyDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Study";
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.StudyDialog_KeyUp);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StudyDialog_KeyDown);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DocumentRenderer area;
  }
}