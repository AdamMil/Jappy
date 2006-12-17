using System;
using System.Windows.Forms;

namespace Jappy
{

partial class StudyListDialog : Form
{
  public StudyListDialog()
  {
    InitializeComponent();
  }

  public string ListName
  {
    get { return txtName.Text.Trim(); }
    set { txtName.Text = value == null ? string.Empty : value.Trim(); }
  }
  
  public bool HintReadings
  {
    get { return chkReadings.Checked; }
    set { chkReadings.Checked = value; }
  }
  
  public bool HintExample
  {
    get { return chkExample.Checked; }
    set { chkExample.Checked = value; }
  }

  public void LoadList(StudyList list)
  {
    ListName     = list.Name;
    HintExample  = list.HintExample;
    HintReadings = list.HintReadings;
  }

  public void SaveList(StudyList list)
  {
    list.Name         = ListName;
    list.HintExample  = HintExample;
    list.HintReadings = HintReadings;
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