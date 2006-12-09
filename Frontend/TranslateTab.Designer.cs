namespace Jappy
{
  partial class TranslateTab
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
      this.output.Dock = System.Windows.Forms.DockStyle.Fill;
      this.output.Location = new System.Drawing.Point(0, 71);
      this.output.Name = "output";
      this.output.Size = new System.Drawing.Size(200, 91);
      this.output.TabIndex = 3;
      this.output.TabStop = false;
      // 
      // input
      // 
      this.input.Dock = System.Windows.Forms.DockStyle.Top;
      this.input.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
      this.input.Location = new System.Drawing.Point(0, 0);
      this.input.Multiline = true;
      this.input.Name = "input";
      this.input.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.input.Size = new System.Drawing.Size(200, 71);
      this.input.TabIndex = 2;
      this.input.MouseLeave += new System.EventHandler(this.common_MouseLeave);
      this.input.MouseEnter += new System.EventHandler(this.input_MouseEnter);
      this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.transInput_KeyPress);
      // 
      // TranslateTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.output);
      this.Controls.Add(this.input);
      this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "TranslateTab";
      this.Size = new System.Drawing.Size(200, 162);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DocumentRenderer output;
    private System.Windows.Forms.TextBox input;
  }
}
