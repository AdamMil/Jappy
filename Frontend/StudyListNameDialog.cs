using System;
using System.Windows.Forms;

namespace Jappy
{

public partial class StudyListNameDialog : Form
{
  public StudyListNameDialog()
  {
    InitializeComponent();
  }

  public string ListName
  {
    get { return txtName.Text.Trim(); }
  }
  
  protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    base.OnClosing(e);

    if(okClicked && ListName.Length == 0)
    {
      MessageBox.Show("Please enter a name for the study list.", "Enter a name",
                      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      e.Cancel  = true;
      okClicked = false;
    }
  }

  void btnOK_Click(object sender, EventArgs e)
  {
    okClicked = true;
  }
  
  bool okClicked;
}

} // namespace Jappy