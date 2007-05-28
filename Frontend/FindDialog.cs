using System;
using System.Windows.Forms;

namespace Jappy
{

partial class FindDialog : Form
{
  public FindDialog()
  {
    InitializeComponent();
  }

  public DocumentRenderer Document
  {
    get { return document; }
    set
    {
      if(value != document)
      {
        if(document != null) document.Document.NodeChanged -= OnDocumentChanged;
        document = value;
        Enabled  = value != null;
        if(value != null) document.Document.NodeChanged += OnDocumentChanged;

        nextLocation = null;
      }
    }
  }

  public string SearchText
  {
    get { return txtSearchFor.Text; }
    set { txtSearchFor.Text = value == null ? string.Empty : value; }
  }

  public void FindNext()
  {
    if(string.IsNullOrEmpty(SearchText)) return;

    // if we don't have a start location, start from the beginning of the selection or document
    if(nextLocation == null) nextLocation = document.SelectionLength == 0 ? 0 : document.SelectionStart;
    else if(document.SelectionStart != 0) nextLocation = document.SelectionStart+1;

    // now search the document from the current position (the selection start)
    int index = document.Text.IndexOf(SearchText, nextLocation.Value);

    if(index == -1) // if we hit the end of the document, search from the beginning
    {
      index = document.Text.IndexOf(SearchText);
    }

    if(index == -1 || index == nextLocation-1) // show a message if there is no next occurence
    {
      MessageBox.Show("No more occurrences of "+SearchText+" could be found.", "Not found",
                      MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    else
    {
      document.ScrollTo(index);
      document.SelectionStart  = index;
      document.SelectionLength = SearchText.Length;
    }
  }

  DocumentRenderer document;
  int? nextLocation;

  protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    base.OnClosing(e);
    e.Cancel = true;
    Hide();
  }

  void OnDocumentChanged(Document doc, DocumentNode node)
  {
    nextLocation = null;
  }

  void btnFind_Click(object sender, EventArgs e)
  {
    FindNext();
  }

  void txtSearchFor_TextChanged(object sender, EventArgs e)
  {
    nextLocation = null;
  }

  void btnClose_Click(object sender, EventArgs e)
  {
    Hide();
  }
}

} // namespace Jappy