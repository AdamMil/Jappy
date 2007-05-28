/*
Jappy is a Japanese dictionary and study tool.

http://www.adammil.net/
Copyright (C) 2007 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

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

  public bool ShowReversedCards
  {
    get { return chkReversed.Checked; }
    set { chkReversed.Checked = value; }
  }

  public void LoadList(StudyList list)
  {
    ListName          = list.Name;
    HintExample       = list.HintExample;
    HintReadings      = list.HintReadings;
    ShowReversedCards = list.ShowReversedCards;
  }

  public void SaveList(StudyList list)
  {
    list.Name              = ListName;
    list.HintExample       = HintExample;
    list.HintReadings      = HintReadings;
    list.ShowReversedCards = ShowReversedCards;
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