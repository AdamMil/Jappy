namespace Jappy
{
  partial class ExampleSearchTab
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
      this.output = new Jappy.DocumentRenderer();
      this.input = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // output
      // 
      this.output.AllowSelection = true;
      this.output.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.output.Dock = System.Windows.Forms.DockStyle.Fill;
      this.output.Location = new System.Drawing.Point(0, 22);
      this.output.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.output.Name = "output";
      this.output.SelectionLength = 0;
      this.output.SelectionStart = 0;
      this.output.Size = new System.Drawing.Size(200, 140);
      this.output.TabIndex = 6;
      this.output.TabStop = false;
      this.output.MouseLeave += new System.EventHandler(this.common_MouseLeave);
      this.output.MouseClick += new System.Windows.Forms.MouseEventHandler(this.output_MouseClick);
      this.output.MouseEnter += new System.EventHandler(this.output_MouseEnter);
      // 
      // input
      // 
      this.input.BackColor = System.Drawing.SystemColors.Window;
      this.input.Dock = System.Windows.Forms.DockStyle.Top;
      this.input.Location = new System.Drawing.Point(0, 0);
      this.input.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.input.Name = "input";
      this.input.Size = new System.Drawing.Size(200, 22);
      this.input.TabIndex = 5;
      this.input.MouseLeave += new System.EventHandler(this.common_MouseLeave);
      this.input.MouseEnter += new System.EventHandler(this.input_MouseEnter);
      this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.input_KeyPress);
      // 
      // ExampleSearchTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.output);
      this.Controls.Add(this.input);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "ExampleSearchTab";
      this.Size = new System.Drawing.Size(200, 162);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DocumentRenderer output;
    private System.Windows.Forms.TextBox input;
  }
}
