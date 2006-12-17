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