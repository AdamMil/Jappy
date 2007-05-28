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

partial class StudyListEntryDialog : Form
{
  public StudyListEntryDialog()
  {
    InitializeComponent();
    SetRatio(0, 0);
  }
  
  public string Phrase
  {
    get { return txtPhrase.Text.Trim(); }
    set { txtPhrase.Text = value.Trim(); }
  }
  
  public string Readings
  {
    get
    {
      string text = txtReadings.Text.Trim();
      return string.IsNullOrEmpty(text) ? null : text;
    }
    set { txtReadings.Text = value == null ? string.Empty : value.Trim(); }
  }
  
  public string Meanings
  {
    get { return txtMeanings.Text.Trim(); }
    set { txtMeanings.Text = value.Trim(); }
  }
  
  public string EnExample
  {
    get
    {
      string text = txtEnExample.Text.Trim();
      return string.IsNullOrEmpty(text) ? null : text;
    }
    set { txtEnExample.Text = value == null ? string.Empty : value.Trim(); }
  }

  public string JpExample
  {
    get
    {
      string text = txtJpExample.Text.Trim();
      return string.IsNullOrEmpty(text) ? null : text;
    }
    set { txtJpExample.Text = value == null ? string.Empty : value.Trim(); }
  }

  public void LoadItem(StudyList.Item item)
  {
    Phrase    = item.Phrase;
    Readings  = item.Readings;
    Meanings  = item.Meanings;
    EnExample = item.ExampleDest;
    JpExample = item.ExampleSource;
    SetRatio(item.CorrectCount, item.ShownCount);
  }

  public void SaveItem(StudyList.Item item)
  {
    item.Phrase        = Phrase;
    item.Readings      = Readings;
    item.Meanings      = Meanings;
    item.ExampleDest   = EnExample;
    item.ExampleSource = JpExample;
    item.CorrectCount  = correctCount;
    item.ShownCount    = shownCount;
  }

  protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    base.OnClosing(e);

    if(saveClicked && (string.IsNullOrEmpty(Phrase) || string.IsNullOrEmpty(Meanings)))
    {
      MessageBox.Show("Please enter a value for the phrase and meanings.", "Values required");
      e.Cancel = true;
      saveClicked = false;
    }
  }

  void SetRatio(int correct, int shown)
  {
    correctCount = correct;
    shownCount   = shown;
    lblSuccess.Text = "Correct " + correct + " of " + shown;
  }

  void btnReset_Click(object sender, EventArgs e)
  {
    SetRatio(0, 0);
  }

  void btnSave_Click(object sender, EventArgs e)
  {
    saveClicked = true;
  }

  int correctCount, shownCount;
  bool saveClicked;
}

} // namespace Jappy